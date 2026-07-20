using System.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 审批完成事件处理器 — 审批通过/驳回后执行业务回调 + 通知相关人员
/// ★ v3 重构：幂等守卫（ApprovalBizData.IsProcessed） + 按 TargetEntityType 分发
/// 支持的业务类型包括：Import（导入）、ContractFeeAdjust（费用调价）、
/// ContractFeeAdd（费用添加）、ContractTerminate（终止）、ContractRenewal（续签）、
/// ContractActivation（合同创建）、ReceivableGeneration（应收生成）、
/// ContractModify（修改）、ContractTenantChange（换租）、
/// SupplementaryFee（补充收费）等
/// </summary>
public class ApprovalCompletedEventHandler : IEventHandler<ApprovalCompletedEvent>
{
    private readonly IImportService _importService;
    private readonly IContractService _contractService;
    private readonly IRenewalService _renewalService;
    private readonly IContractDomainService _contractDomainService;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;
    private readonly ISqlLoader _sql;
    private readonly IDbConnectionFactory _db;
    private readonly ITerminateJob _terminateJob;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBillingDomainService _billingDomain;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="importService">导入服务（处理 Import 类型审批回调）</param>
    /// <param name="contractService">合同服务</param>
    /// <param name="renewalService">续签服务</param>
    /// <param name="contractDomainService">合同领域服务（终止逻辑）</param>
    /// <param name="uow">工作单元</param>
    /// <param name="notificationService">通知服务</param>
    /// <param name="sql">SQL 加载器</param>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="billingDomain">计费领域服务（月份拆分）</param>
    /// <param name="terminateJob">终止结算服务</param>
    /// <param name="serviceProvider">服务提供者</param>
    public ApprovalCompletedEventHandler(
        IImportService importService,
        IContractService contractService,
        IRenewalService renewalService,
        IContractDomainService contractDomainService,
        IUnitOfWork uow,
        INotificationService notificationService,
        ISqlLoader sql,
        IDbConnectionFactory db,
        IBillingDomainService billingDomain,
        ITerminateJob terminateJob,
        IServiceProvider serviceProvider)
    {
        _importService = importService;
        _contractService = contractService;
        _renewalService = renewalService;
        _contractDomainService = contractDomainService;
        _uow = uow;
        _notificationService = notificationService;
        _sql = sql;
        _db = db;
        _billingDomain = billingDomain;
        _terminateJob = terminateJob;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 处理审批完成事件 — 幂等守卫 + 业务回调 + 通知相关人员
    /// </summary>
    public async Task HandleAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        // ★ 幂等守卫：通过 ApprovalBizData.IsProcessed（直接查 DB 避免缓存的脏数据）
        var bizData = await _uow.ApprovalBizData.GetByApprovalRequestIdAsync(@event.ApprovalRequestId, ct);
        if (bizData != null && bizData.IsProcessed)
        {
            return; // 已处理过，跳过
        }

        try
        {
            // 1. 业务回调
            await ExecuteBusinessCallbacksAsync(@event, bizData, ct);

            // 2. ★ 幂等标记：直接用 SQL 更新（回调内部可能已调 CommitAsync 清空了 ChangeTracker）
            if (bizData != null)
            {
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Approval.Update.ApprovalBizData.MarkProcessed"),
                    new { Id = bizData.Id });  // 用匿名类型匹配 @Id
            }

            // 3. 通知相关人员
            await SendNotificationsAsync(@event, ct);
        }
        catch
        {
            // 业务处理失败 → 审批退回待审批状态 + 删除本次流转记录，允许用户修复后重新审批
            try
            {
                using var rollbackConn = _db.CreateConnection();
                rollbackConn.Open();
                await rollbackConn.ExecuteAsync(
                    _sql.Get("Approval.Update.Request.RollbackToPending"),
                    new { Id = @event.ApprovalRequestId, UpdatedBy = Guid.Empty });
                await rollbackConn.ExecuteAsync(
                    _sql.Get("Approval.Delete.Record.LastByRequest"),
                    new { RequestId = @event.ApprovalRequestId });
            }
            catch { /* 回滚失败不影响原始异常 */ }
            throw;
        }
    }

    private async Task ExecuteBusinessCallbacksAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
    {
        switch (@event.TargetEntityType)
        {
            case "Import":
                await HandleImportAsync(@event, ct);
                break;

            case "ContractFeeAdjust":
            case "ContractFeeChange":
                await HandleContractFeeAdjustAsync(@event, bizData, ct);
                break;

            case "ContractFeeAdd":
                await HandleContractFeeAddAsync(@event, ct);
                break;

            case "ContractTerminate":
                await HandleContractTerminateAsync(@event, bizData, ct);
                break;

            case "ContractRenewal":
                await HandleContractRenewalAsync(@event, ct);
                break;

            // ★ 向后兼容：旧审批 TargetEntityType="Contract"（调租或终止）
            case "Contract":
                await HandleLegacyContractAsync(@event, bizData, ct);
                break;

            // ★ 新增审批闭环分支
            case "ContractActivation":
                await HandleContractActivationAsync(@event, ct);
                break;
            case "ReceivableGeneration":
                await HandleReceivableGenerationAsync(@event, ct);
                break;
            case "ContractModify":
                await HandleContractModifyAsync(@event, ct);
                break;
            case "ContractTenantChange":
                await HandleContractTenantChangeAsync(@event, bizData, ct);
                break;
            case "SupplementaryFee":
                await HandleSupplementaryFeeAsync(@event, ct);
                break;
        }
    }

    /// <summary>
    /// 处理旧版合同审批回调（向后兼容） — 根据标题前缀区分终止/调租
    /// </summary>
    private async Task HandleLegacyContractAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
    {
        var request = await _uow.ApprovalRequests.GetByIdAsync(@event.ApprovalRequestId, ct);
        if (request?.Title == null) return;

        if (@event.Action == "Approved")
        {
            if (request.Title.StartsWith("[合同终止]"))
            {
                // 旧终止审批：直接 Terminate
                if (bizData != null)
                {
                    var contract = await _uow.Contracts.GetByIdAsync(bizData.ContractId, ct);
                    var journals = new List<Journal>();

                    var result = _contractDomainService.ExecuteContractTermination(
                        contract!, journals, null, request.Description ?? "合同终止");

                    if (contract != null) await _uow.Contracts.UpdateAsync(contract, ct);
                    await _uow.ExecuteSqlRawAsync(
                        _sql.Get("Contract.Update.ContractFeeConfig.ExpireByContract"),
                        new { ExpiryDate = result.EffectiveEndDate, ContractId = bizData.ContractId }, ct);
                    await _uow.CommitAsync(ct);

            try { using (var conn2 = _db.CreateConnection()) { conn2.Open();
                    await conn2.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
                        new { Id = Guid.NewGuid(), ContractId = bizData.ContractId, ChangeType = "TERMINATE",
                            Title = "合同终止", Detail = bizData.Reason ?? "",
                            OldValue = (decimal?)null, NewValue = (decimal?)null,
                            EffectiveDate = bizData.ActualEndDate?.ToString("yyyy-MM-dd"),
                            OperatorId = Guid.Empty, OperatorName = "" }); } } catch { }
                }
                else
                {
                    // 极旧数据无 bizData → 直接调 Terminate
                    var contract = await _uow.Contracts.GetByIdAsync(@event.TargetEntityId, ct);
                    if (contract != null && contract.Status != "Terminated")
                    {
                        contract.Terminate(request.Description ?? "合同终止");
                        await _uow.CommitAsync(ct);
                    }
                }
            }
            // else 旧租金调整分支已移除（v3 改为 FeeConfig 模式）
            // 旧审批记录（TargetEntityType='Contract'）中的租金调整数据不再处理
        }
    }

    /// <summary>
    /// 处理导入审批回调 — 通过则执行导入，驳回则标记批次为已驳回
    /// </summary>
    private async Task HandleImportAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        if (@event.Action == "Approved")
        {
            await _importService.ExecuteApprovedImportAsync(@event.TargetEntityId, ct);
        }
        else if (@event.Action == "Rejected")
        {
            var batch = await _uow.ImportBatches.GetByIdAsync(@event.TargetEntityId, ct);
            if (batch != null && batch.Status == "PendingApproval")
            {
                batch.Reject();
                await _uow.CommitAsync(ct);
            }
        }
    }

    /// <summary>
    /// 处理费用调价审批回调 — 到期旧配置 → 插入新配置 → 生成补差 JE
    /// 按 FeeConfig 逐项处理，抄表计量调 UnitPrice，固定金额调 Amount
    /// 补差 JE 在 FeeConfig 落库后独立生成，失败不影响 FeeConfig 变更
    /// </summary>
    private async Task HandleContractFeeAdjustAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
    {
        if (@event.Action != "Approved" || bizData == null) return;

        var feeItems = await _uow.ApprovalFeeItems.GetByApprovalRequestIdAsync(@event.ApprovalRequestId, ct);
        if (feeItems.Count == 0) return;

        var approvalReq = await _uow.ApprovalRequests.GetByIdAsync(@event.ApprovalRequestId, ct);
        var userId = approvalReq?.CreatedBy ?? Guid.Empty;

        // 校验所有调价项的生效日期在合同起止日期范围内
        var contract = await _uow.Contracts.GetByIdAsync(bizData.ContractId, ct);
        if (contract != null)
        {
            foreach (var item in feeItems)
            {
                var effDate = item.EffectiveDate ?? bizData.EffectiveDate?.ToString("yyyy-MM-dd") ?? "";
                if (!string.IsNullOrEmpty(effDate))
                    Contract.ValidateFeeEffectiveDate(DateOnly.Parse(effDate), contract.StartDate, contract.EndDate, item.FeeName);
            }
        }

        foreach (var item in feeItems)
        {
            var effectiveDate = item.EffectiveDate ?? bizData.EffectiveDate?.ToString("yyyy-MM-dd") ?? "";
            if (string.IsNullOrEmpty(effectiveDate)) continue;
            var expiryDate = DateOnly.Parse(effectiveDate).AddDays(-1).ToString("yyyy-MM-dd");

            // 校验新生效日必须大于原生效日，否则到期日 < 生效日
            using var conn = _db.CreateConnection(); conn.Open();
            var current = await conn.QuerySingleOrDefaultAsync(
                _sql.Get("Lease.Select.ContractFeeConfig.CurrentByContractAndFee"),
                new { ContractId = item.ContractId, FeeCodeId = item.FeeCodeId });
            if (current != null)
            {
                var curEff = (DateTime)((dynamic)current).EffectiveDate;
                var newEff = DateOnly.Parse(effectiveDate);
                if (newEff <= DateOnly.FromDateTime(curEff))
                    throw new InvalidOperationException(
                        $"费用项 {item.FeeName} 的生效日期 {effectiveDate} 必须晚于当前配置的生效日期 {curEff:yyyy-MM-dd}");
            }

            // 先到期旧配置，再校验区间不交叉（防止异常数据）
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Contract.Update.ContractFeeConfig.ExpireByCodeId"),
                new { ExpiryDate = expiryDate, ContractId = item.ContractId, FeeCodeId = item.FeeCodeId });
            await EnsureNoOverlappingFeeConfigAsync(item.ContractId, item.FeeCodeId,
                effectiveDate, null, null, ct);

            if (item.BillingMode == "MeterBased")
            {
                // 抄表计量：Amount 不变，调 UnitPrice
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Contract.Insert.ContractFeeConfig.MeterBased"),
                    new { Id = Guid.NewGuid(), ContractId = item.ContractId, FeeCodeId = item.FeeCodeId,
                        Amount = item.OldAmount, Unit = item.Unit, UnitPrice = item.NewAmount,
                        EffectiveDate = effectiveDate, CreatedBy = userId });
            }
            else
            {
                using (var conn2 = _db.CreateConnection()) { conn2.Open();
                    await InsertChangeHistoryAsync(conn2, null, item.ContractId, "FEE_ADJUST",
                        "费用调价", item.FeeName + ": " + item.OldAmount.ToString("F2") + " -> " + item.NewAmount.ToString("F2"),
                        item.OldAmount, item.NewAmount, effectiveDate, userId); }

                // 固定金额：调 Amount
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
                    new { Id = Guid.NewGuid(), ContractId = item.ContractId, FeeCodeId = item.FeeCodeId,
                        BillingMode = "FixedAmount", Amount = item.NewAmount,
                        EffectiveDate = effectiveDate, CreatedBy = userId, Now = ChinaTime.Now }, ct);
            }
        }

        await _uow.CommitAsync(ct);

        // ★ Commit 之后再生成补差 Supplementary JE（FeeConfig 已落库，
        //    补差 JE 生成失败不影响 FeeConfig 变更，可手动重试）
        var currentMonth = DateOnly.FromDateTime(ChinaTime.Now).ToString("yyyy-MM");
        var companyId = bizData.CompanyId;

        foreach (var item in feeItems)
        {
            var effDate = item.EffectiveDate ?? bizData.EffectiveDate?.ToString("yyyy-MM-dd") ?? "";
            if (string.IsNullOrEmpty(effDate)) continue;
            var effMonth = effDate.Substring(0, 7);

            // 生效月 M ≤ 当前月+1 → 已出账单范围 → 生成补差
            if (string.Compare(effMonth, currentMonth, StringComparison.Ordinal) <= 0 ||
                effMonth == DateOnly.FromDateTime(ChinaTime.Now).AddMonths(1).ToString("yyyy-MM"))
            {
                var diff = item.NewAmount - item.OldAmount;
                if (diff == 0) continue;

                // 生效月按天分摊差价
                var effDateObj = DateOnly.Parse(effDate);
                var daysInMonth = DateTime.DaysInMonth(effDateObj.Year, effDateObj.Month);
                var occupiedDays = daysInMonth - effDateObj.Day + 1;
                var proratedDiff = Math.Round(diff / daysInMonth * occupiedDays, 2);
                if (proratedDiff == 0) continue;

                try
                {
                    using var jeConn = _db.CreateConnection();
                    jeConn.Open();
                    await InsertJournalAsync(jeConn, null,
                        companyId, item.ContractId, item.FeeCodeId, null,
                        effMonth, proratedDiff, DateOnly.FromDateTime(ChinaTime.Now),
                        "Supplementary", $"调价补差 {item.FeeName} {effMonth}");
                }
                catch { /* 补差 JE 失败不影响 FeeConfig 变更，可手动重试 */ }
            }
        }
    }

    /// <summary>
    /// 处理费用添加审批回调 — 插入新 FeeConfig + 一次性费用应收计划 + JE
    /// 一次性费用在事务 Commit 后独立生成 OneTime JE（避免事务锁阻塞）
    /// </summary>
    private async Task HandleContractFeeAddAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        if (@event.Action != "Approved") return;

        var feeItems = await _uow.ApprovalFeeItems.GetByApprovalRequestIdAsync(@event.ApprovalRequestId, ct);
        if (feeItems.Count == 0) return;

        var bizData = await _uow.ApprovalBizData.GetByApprovalRequestIdAsync(@event.ApprovalRequestId, ct);
        if (bizData != null && bizData.IsProcessed) return;

        // 校验生效日期在合同起止日期范围内，并缓存合同日期供后续使用
        var contractCache = new Dictionary<Guid, (DateOnly StartDate, DateOnly? EndDate)>();
        var contractIds = feeItems.Select(f => f.ContractId).Distinct().ToList();
        foreach (var cid in contractIds)
        {
            var c = await _uow.Contracts.GetByIdAsync(cid, ct);
            if (c == null) continue;
            contractCache[cid] = (c.StartDate, c.EndDate);
            foreach (var item in feeItems.Where(f => f.ContractId == cid))
            {
                var effDate = item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd");
                Contract.ValidateFeeEffectiveDate(DateOnly.Parse(effDate), c.StartDate, c.EndDate, item.FeeName);
            }
        }

        // 事务外收集：审批后需要生成 JE 的一次性费用项（等 FeeConfig 落库后执行）
        var oneTimeJobs = new List<(Guid ContractId, Guid ConfigId)>();

        using var conn = _db.CreateConnection();
        conn.Open();

        // 获取公司 ID（优先从 bizData，无则查合同）
        var companyId = bizData?.CompanyId;
        if (companyId == null || companyId == Guid.Empty)
        {
            var firstItem = feeItems.FirstOrDefault();
            if (firstItem != null)
            {
                var contract = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    "SELECT CompanyId FROM Contracts WHERE Id = @Id", new { Id = firstItem.ContractId });
                companyId = contract?.CompanyId is Guid cid ? cid : Guid.Empty;
            }
        }

        using var tx = conn.BeginTransaction();
        try
        {
            foreach (var item in feeItems)
            {
                // 查 FeeCode 的 ChargeType
                var feeCode = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    _sql.Get("FeeCode.Select.FeeCode.ChargeTypeById"),
                    new { Id = item.FeeCodeId }, tx);
                var chargeType = (string)(feeCode?.ChargeType ?? "Recurring");

                List<Guid> configIds = new();
                if (chargeType == "Recurring")
                {
                    var (cStart, cEnd) = contractCache.TryGetValue(item.ContractId, out var cd)
                        ? cd : (DateOnly.FromDateTime(ChinaTime.Now), (DateOnly?)null);
                    var segments = _billingDomain.CalculateMonthlySplit(
                        item.NewAmount,
                        item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                        ChinaTime.Now,
                        cStart, cEnd);
                    configIds = await RecurringFeeSplitHelper.InsertMonthlySplitFeeConfigs(
                        conn, tx, _sql, _billingDomain,
                        item.ContractId, item.FeeCodeId,
                        item.NewAmount, item.BillingMode, item.Unit, (decimal?)null,
                        item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                        Guid.Empty,
                        cStart, cEnd);
                        // 生成所有历史月份的日记账（首月及后续各月），不包含未来段
                        for (int i = 0; i < segments.Count; i++)
                        {
                            var seg = segments[i];
                            if (seg.ExpiryDate == null) continue;

                            var period = DateOnly.Parse(seg.EffectiveDate).ToString("yyyy-MM");
                            var exists = await conn.QuerySingleAsync<int>(
                                _sql.Get("Billing.Select.Journal.ExistsByKey"),
                                new { C = item.ContractId, F = item.FeeCodeId, P = period }, tx);
                            if (exists > 0) continue;

                            await InsertJournalAsync(conn, tx,
                                companyId ?? Guid.Empty, item.ContractId, item.FeeCodeId, configIds[i],
                                period, seg.Amount,
                                DateOnly.FromDateTime(ChinaTime.Now),
                                "Normal", $"应收 {item.FeeName} {period}");
                        }
}
if (chargeType == "OneTime")
                {
                    var oneTimeConfigId = Guid.NewGuid();
                    await conn.ExecuteAsync(
                        _sql.Get("Lease.Insert.ContractFeeConfig.Default"),
                        new
                        {
                            Id = oneTimeConfigId,
                            ContractId = item.ContractId,
                            FeeCodeId = item.FeeCodeId,
                            BillingMode = item.BillingMode ?? "FixedAmount",
                            Amount = item.NewAmount,
                            Unit = (string?)null,
                            UnitPrice = (decimal?)null,
                            EffectiveDate = item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                            CreatedBy = Guid.Empty,
                            Now = ChinaTime.Now
                        }, tx);
                    // 暂记，等 Commit 后再生成 JE（避免独立连接被事务锁阻塞 → 超时）
                    oneTimeJobs.Add((item.ContractId, oneTimeConfigId));

                    // 插入 ReceivablePlan（一次性费用应收计划，关联到 FeeConfig 实例以支持同费用多次添加）
                    // 使用 Unposted SQL，GLPosted=0，待收款确认后再过账
                    var feeContract = await conn.QuerySingleOrDefaultAsync("SELECT StartDate FROM Contracts WHERE Id = @Id", new { Id = item.ContractId }, tx);
                    var contractStart = feeContract != null ? DateOnly.FromDateTime((DateTime)feeContract.StartDate) : DateOnly.FromDateTime(ChinaTime.Now);
                    var period = (item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd")).Substring(0, 7);
                    await InsertJournalAsync(conn, tx,
                        companyId ?? Guid.Empty, item.ContractId, item.FeeCodeId, oneTimeConfigId,
                        period, item.NewAmount, contractStart.AddDays(30),
                        "Normal", $"一次性 {item.FeeName}");
                }

                // 写入变更历史（审计追踪）
                var effDate = item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd");
                await InsertChangeHistoryAsync(conn, tx, item.ContractId, "FEE_ADD",
                    "添加费用",
                    $"添加 {item.FeeName} ¥{item.NewAmount:F2}，生效 {effDate}",
                    null, item.NewAmount, effDate, Guid.Empty);
            }

            // 幂等标记
            if (bizData != null)
            {
                await conn.ExecuteAsync(
                    _sql.Get("Approval.Update.ApprovalBizData.MarkProcessed"),
                    new { Id = bizData.Id }, tx);
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        // ★ Commit 后再生成 OneTime JE（FeeConfig 已落库，独立连接可正常读取）
        //    TODO: 原 _journalGen.GenerateOneTimeAsync 已移除
    }

    /// <summary>
    /// 处理合同终止审批回调 — 执行终止 + 写入变更历史 + 生成押金结算凭证
    /// 押金 JE 生成失败不阻断终止主流程，可后续手动重试
    /// </summary>
    private async Task HandleContractTerminateAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
    {
        if (@event.Action != "Approved" || bizData == null) return;

        // 应用层加载数据 → 领域层状态变更 → 应用层持久化
        var contract = await _uow.Contracts.GetByIdAsync(bizData.ContractId, ct);
        var journals = new List<Journal>();

        var result = _contractDomainService.ExecuteContractTermination(
            contract!, journals, bizData.ActualEndDate, bizData.Reason ?? "合同终止");

        if (contract != null) await _uow.Contracts.UpdateAsync(contract, ct);
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Update.ContractFeeConfig.ExpireByContract"),
            new { ExpiryDate = result.EffectiveEndDate, ContractId = bizData.ContractId }, ct);
        await _uow.CommitAsync(ct);

        try { using var conn = _db.CreateConnection(); conn.Open();
            await conn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
                new { Id = Guid.NewGuid(), ContractId = bizData.ContractId, ChangeType = "TERMINATE",
                    Title = "合同终止", Detail = bizData.Reason ?? "",
                    OldValue = (decimal?)null, NewValue = (decimal?)null,
                    EffectiveDate = bizData.ActualEndDate?.ToString("yyyy-MM-dd"),
                    OperatorId = Guid.Empty, OperatorName = "" }); } catch { }

        // 生成押金结算凭证（独立事务，失败不阻断终止主流程）
        try
        {
            await _terminateJob.ExecuteAsync(
                bizData.ContractId,
                bizData.ActualEndDate?.ToString("yyyy-MM-dd"),
                bizData.DepositReturn ?? "FULL",
                bizData.Reason ?? "合同终止", ct);
        }
        catch { /* 押金 JE 生成失败可后续手动重试 */ }
    }

    /// <summary>
    /// 处理续签审批回调 — 通过则执行续签，驳回则标记续签请求为已驳回
    /// 续签执行失败时回滚审批状态为 Pending，允许用户修复后重新审批
    /// </summary>
    private async Task HandleContractRenewalAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        if (@event.Action == "Approved")
        {
            try
            {
                await _renewalService.ExecuteRenewalAsync(@event.TargetEntityId, ct);
            }
            catch
            {
                // 续签执行失败时回滚审批状态为 Pending，允许用户修复后重新审批
                try
                {
                    using var rollbackConn = _db.CreateConnection();
                    rollbackConn.Open();
                    await rollbackConn.ExecuteAsync(
                        _sql.Get("Approval.Update.Request.RollbackToPending"),
                        new { Id = @event.ApprovalRequestId, UpdatedBy = Guid.Empty });
                }
                catch { /* 回滚失败不影响原始异常 */ }
                throw;
            }
        }
        else if (@event.Action == "Rejected")
        {
            var renewal = await _uow.RenewalRequests.GetByIdAsync(@event.TargetEntityId, ct);
            if (renewal != null)
            {
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Approval.Update.RenewalRequest.ToRejected"),
                    new { Id = renewal.Id }, ct);
            }
        }
    }

	    		    // ===== 合同创建审批通过 =====
    /// <summary>
    /// 处理合同创建审批回调 — 乐观锁 + 房间竞争检查 + 写入合同/租客/费用 + 初始化应收
    /// 流程：乐观锁锁定 → 检查房间是否有生效合同 → 写入合同主表 → 写入租客关联 → 写入费用配置 → 事务提交 → 初始化应收（独立于事务执行，失败不阻断审批完成）
    /// </summary>
		    private async Task HandleContractActivationAsync(ApprovalCompletedEvent @event, CancellationToken ct)
		    {
		        if (@event.Action != "Approved") return;

		        var request = await _uow.ContractCreateRequests.GetByIdAsync(@event.TargetEntityId, ct);
		        if (request == null || request.Status != "PendingApproval") return;

		        // 加载暂存数据（只读，独立连接）
		        List<dynamic> tenants, fees;
		        using (var readConn = _db.CreateConnection()) { readConn.Open();
		            tenants = (await readConn.QueryAsync<dynamic>(
		                _sql.Get("ContractCreate.Select.Tenants.ByRequestId"), new { RequestId = request.Id })).ToList();
		            fees = (await readConn.QueryAsync<dynamic>(
		                _sql.Get("ContractCreate.Select.Fees.ByRequestId"), new { RequestId = request.Id })).ToList();
		        }

		        var now = ChinaTime.Now;
		        var contractId = Guid.NewGuid();

		        // 统一事务：乐观锁 + 竞争检查 + 所有写入
		        using var conn = _db.CreateConnection(); conn.Open();
		        using var tx = conn.BeginTransaction();
		        try
		        {
		            // 1. 乐观锁
		            var locked = await conn.ExecuteAsync(
		                _sql.Get("ContractCreate.Update.Request.LockExecuting"),
		                new { Id = request.Id, Now = now }, tx);
		            if (locked == 0) return;

		            // 2. 房间竞争检查（同一事务内，防止并发）
		            var hasActive = await conn.QuerySingleAsync<int>(
		                _sql.Get("Lease.Select.Contract.HasActiveByRoom"),
		                new { RoomId = request.RoomId }, tx);
		            if (hasActive > 0) throw new InvalidOperationException("该房源已有生效合同");

		            // 3. 写入合同主表
		            await conn.ExecuteAsync(
		                _sql.Get("Lease.Insert.Contract.Default"),
		                new { Id = contractId, ContractNo = request.ContractNo,
		                    RoomId = request.RoomId, StartDate = request.StartDate,
		                    EndDate = request.EndDate, PaymentCycle = request.PaymentCycle,
		                    Status = "Active", CompanyId = request.CompanyId,
		                    CreatedBy = request.CreatedBy, CreatedAt = now }, tx);

		            // 4. 写入租客关联
		            foreach (var t in tenants)
		            {
		                await conn.ExecuteAsync(
		                    _sql.Get("Lease.Insert.ContractTenant.Default"),
		                    new { ContractId = contractId,
		                        TenantId = t.TenantId, IsPrimary = t.IsPrimary,
		                        CreatedBy = request.CreatedBy, CreatedAt = now }, tx);
		            }

		            // 5. 写入费用配置
		            foreach (var f in fees)
		            {
		                var feeChargeType = (string)(f.ChargeType ?? "Recurring");
		                if (feeChargeType == "Recurring")
		                {
		                    var effDate = f.EffectiveDate ?? request.StartDate.ToString("yyyy-MM-dd");
		                    await RecurringFeeSplitHelper.InsertMonthlySplitFeeConfigs(
		                        conn, tx, _sql, _billingDomain,
		                        contractId, (Guid)f.FeeCodeId,
		                        (decimal)f.Amount, (string)f.BillingMode,
		                        (string?)f.Unit, (decimal?)f.UnitPrice,
		                        effDate, request.CreatedBy,
		                        request.StartDate, request.EndDate);
		                }

		                else
		                {
		                    await conn.ExecuteAsync(
		                _sql.Get("Lease.Insert.ContractFeeConfig.Default"),
		                        new { Id = Guid.NewGuid(), ContractId = contractId,
		                            FeeCodeId = f.FeeCodeId, BillingMode = f.BillingMode,
		                            Amount = f.Amount, Unit = f.Unit, UnitPrice = f.UnitPrice,
		                            EffectiveDate = f.EffectiveDate ?? request.StartDate.ToString("yyyy-MM-dd"),
		                            CreatedBy = request.CreatedBy, Now = now }, tx);
		                }
		            }

		            // 6. 标记暂存表完成
		            await conn.ExecuteAsync(
		                _sql.Get("ContractCreate.Update.Request.Complete"),
		                new { Id = request.Id, NewContractId = contractId }, tx);

		            tx.Commit();
		        }
		        catch
		        {
		            tx.Rollback();
		            throw;
		        }

		        // 初始化应收（事务外独立执行，失败不阻断审批完成）
		        try
		        {
		            var receivableGen = _serviceProvider.GetRequiredService<IReceivableGenerationService>();
		            await receivableGen.GenerateForActivationAsync(contractId, ct);
		        }
		        catch { /* 不阻断审批完成 */ }
		    }// ===== 生成应收审批通过 =====
	    private async Task HandleReceivableGenerationAsync(ApprovalCompletedEvent @event, CancellationToken ct)
	    {
	        if (@event.Action == "Rejected")
	        {
	            using var rejectConn = _db.CreateConnection();
	            rejectConn.Open();
	            await rejectConn.ExecuteAsync(
	                _sql.Get("ReceivableGenerate.Update.Request.Reject"),
	                new { Id = @event.TargetEntityId });
	            return;
	        }

	        if (@event.Action != "Approved") return;

	        var request = await _uow.ReceivableGenerateRequests.GetByIdAsync(@event.TargetEntityId, ct);
	        if (request == null || request.Status != "PendingApproval") return;

	        using var lockConn = _db.CreateConnection(); lockConn.Open();
	        var locked = await lockConn.ExecuteAsync(
	            _sql.Get("ReceivableGenerate.Update.Request.LockExecuting"),
	            new { Id = request.Id, Now = ChinaTime.Now });
	        if (locked == 0) return;

        List<dynamic> items;
        using (var conn2 = _db.CreateConnection())
        {
            conn2.Open();
            items = (await conn2.QueryAsync<dynamic>(
                _sql.Get("ReceivableGenerate.Select.Items.ByRequestId"), new { RequestId = request.Id })).ToList();
        }

        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            // 补充 FeeConfig：按现有 Recurring 费用的生效日和全额月租展开月度分段
            var contractRow = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT StartDate, EndDate FROM Contracts WHERE Id=@Id",
                new { Id = request.ContractId }, tx);
            if (contractRow != null)
            {
                var cStart = DateOnly.FromDateTime((DateTime)contractRow.StartDate);
                var cEnd = contractRow.EndDate != null
                    ? DateOnly.FromDateTime((DateTime)contractRow.EndDate) : (DateOnly?)null;

                var allConfigs = (await conn.QueryAsync<dynamic>(
                    _sql.Get("Lease.Select.ContractFeeConfig.WithFeeCodeByContract"),
                    new { ContractId = request.ContractId }, tx)).ToList();

                foreach (var feeGroup in allConfigs.GroupBy(x => (Guid)x.FeeCodeId))
                {
                    var feeCodeId = feeGroup.Key;
                    var feeCodeInfo = await conn.QuerySingleOrDefaultAsync<dynamic>(
                        _sql.Get("FeeCode.Select.FeeCode.ChargeTypeById"), new { Id = feeCodeId }, tx);
                    var chargeType = (string)(feeCodeInfo?.ChargeType ?? "Recurring");
                    if (chargeType != "Recurring") continue;

                    // 1. 删除旧的懒汉式未来段（IsActive=1, ExpiryDate=null）
                    foreach (var cfg in feeGroup)
                    {
                        if (!(bool)cfg.IsActive) continue;
                        if (cfg.ExpiryDate != null) continue;
                        await conn.ExecuteAsync(
                            _sql.Get("Lease.Delete.ContractFeeConfig.ById"),
                            new { Id = (Guid)cfg.Id }, tx);
                        break;
                    }

                    // 2. 取原始生效日和全额月租
                    var cfgList = feeGroup.ToList();
                    var origEff = cfgList.Min(x => (DateTime)x.EffectiveDate).ToString("yyyy-MM-dd");
                    var fullAmt = cfgList.Max(x => (decimal)x.Amount);

                    // 3. 算出当前应有的完整月度分段
                    var expected = _billingDomain.CalculateMonthlySplit(
                        fullAmt, origEff, ChinaTime.Now, cStart, cEnd);
                    if (expected.Count == 0) continue;

                    // 4. 已有分段集合 (Eff, Exp)
                    var existing = new HashSet<(string Eff, string Exp)>();
                    foreach (var cfg in feeGroup)
                    {
                        var eff = ((DateTime)cfg.EffectiveDate).ToString("yyyy-MM-dd");
                        var exp = cfg.ExpiryDate != null
                            ? ((DateTime)cfg.ExpiryDate).ToString("yyyy-MM-dd") : "null";
                        existing.Add((eff, exp));
                    }

                    // 5. 创建缺失的分段
                    foreach (var seg in expected)
                    {
                        var key = (seg.EffectiveDate, seg.ExpiryDate ?? "null");
                        if (existing.Contains(key)) continue;

                        await conn.ExecuteAsync(
                            _sql.Get("Lease.Insert.ContractFeeConfig.WithExpiry"),
                            new
                            {
                                Id = Guid.NewGuid(),
                                ContractId = request.ContractId,
                                FeeCodeId = feeCodeId,
                                BillingMode = "FixedAmount",
                                Amount = seg.Amount,
                                Unit = (string?)null,
                                UnitPrice = (decimal?)null,
                                IsActive = seg.IsActive,
                                EffectiveDate = seg.EffectiveDate,
                                ExpiryDate = seg.ExpiryDate,
                                CreatedBy = Guid.Empty,
                                Now = ChinaTime.Now
                            }, tx);
                    }
                }
            }

            // 读取会计科目
            var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
                _sql.Get("Accounting.Select.Subject.ByCodes"), null, tx)).ToDictionary(r => r.Code, r => r.Id);
            var receivableId = subjects.GetValueOrDefault("1122", Guid.Empty);
            var revenueId = subjects.GetValueOrDefault("6001", subjects.GetValueOrDefault("6051", Guid.Empty));
            var now = ChinaTime.Now;

            // 生成日记账
            foreach (var item in items)
            {
                var exists = await conn.QuerySingleAsync<int>(
                    _sql.Get("Billing.Select.Journal.ExistsByKey"),
                    new { C = request.ContractId, F = (Guid)item.FeeCodeId, P = (string)item.Period }, tx);
                if (exists > 0) continue;

                var jId = Guid.NewGuid();
                await InsertJournalAsync(conn, tx,
                    request.CompanyId, request.ContractId, (Guid)item.FeeCodeId, null,
                    (string)item.Period, (decimal)item.Amount,
                    DateOnly.FromDateTime((DateTime)item.DueDate),
                    "Normal", $"生成应收 {item.Period}");

                await conn.ExecuteAsync(
                    _sql.Get("ReceivableGenerate.Update.Item.SetPlanIds"),
                    new { Id = (Guid)item.Id, ReceivablePlanId = jId, VoucherId = (Guid?)null }, tx);
            }

            await conn.ExecuteAsync(
                _sql.Get("ReceivableGenerate.Update.Request.Complete"),
                new { Id = request.Id }, tx);
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
	    }

	    // ===== 暂停审批通过 =====
	    private async Task HandleContractModifyAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        if (@event.Action != "Approved") return;

        var request = await _uow.ContractModifyRequests.GetByIdAsync(@event.TargetEntityId, ct);
        if (request == null || request.Status != "PendingApproval") return;

        var locked = await _uow.ExecuteSqlRawAsync(
            _sql.Get("ContractModify.Update.Request.LockExecuting"),
            new { Id = request.Id, Now = ChinaTime.Now }, ct);
        if (locked == 0) return;

        await _uow.ExecuteSqlRawAsync(
            _sql.Get("ContractModify.Update.Contract.ApplyChanges"), request, ct);
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("ContractModify.Update.Request.Complete"),
            new { Id = request.Id }, ct);

        // 写入变更历史（独立连接，失败不阻断主流程）
        try
        {
            using var histConn = _db.CreateConnection();
            histConn.Open();
            var detail = BuildModifyChangeDetail(request);
            await histConn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
                new
                {
                    Id = Guid.NewGuid(),
                    ContractId = request.ContractId,
                    ChangeType = "CONTRACT_MODIFY",
                    Title = "修改合同信息",
                    Detail = detail,
                    OldValue = (decimal?)null,
                    NewValue = (decimal?)null,
                    EffectiveDate = (string?)null,
                    OperatorId = Guid.Empty,
                    OperatorName = ""
                });
        }
        catch { /* 变更历史写入失败不影响主流程 */ }

        await _uow.CommitAsync(ct);
    }

    /// <summary>构建合同修改变更详情文本</summary>
    private static string BuildModifyChangeDetail(ContractModifyRequest req)
    {
        var parts = new List<string>();
        if (req.StartDate.HasValue) parts.Add($"起租日: {req.StartDate:yyyy-MM-dd}");
        if (req.EndDate.HasValue) parts.Add($"到期日: {req.EndDate:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(req.PaymentCycle)) parts.Add($"付款周期: {req.PaymentCycle}");
        if (req.PaymentDueDay.HasValue) parts.Add($"付款到期日: {req.PaymentDueDay}日");
        if (req.AllowDepositAsLastRent.HasValue) parts.Add($"押金抵租金: {(req.AllowDepositAsLastRent.Value ? "是" : "否")}");
        if (!string.IsNullOrEmpty(req.TenantPhone)) parts.Add($"电话: {req.TenantPhone}");
        if (!string.IsNullOrEmpty(req.Remark)) parts.Add($"备注: {req.Remark}");
        return string.Join("; ", parts);
    }

    
private async Task HandleContractTenantChangeAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
	    {
	        if (@event.Action != "Approved" || bizData == null) return;
	        if (bizData.IsProcessed) return;

	        var tenantId = Guid.Parse(bizData.Reason ?? "");

	        using var conn = _db.CreateConnection(); conn.Open();
	        using var tx = conn.BeginTransaction();
	        try
	        {
	            // 验证合同存在
	            var contractExists = await conn.QuerySingleAsync<int>(
	                "SELECT COUNT(1) FROM Contracts WHERE Id=@Id", new { Id = bizData.ContractId }, tx);
	            if (contractExists == 0) return;

	            if (bizData.ChangeType == "TENANT_ADD")
	            {
	                // 验证租客未关联
	                var exists = await conn.QuerySingleAsync<int>(
	                    _sql.Get("Lease.Select.ContractTenant.ExistsByContractAndTenant"),
	                    new { ContractId = bizData.ContractId, TenantId = tenantId }, tx);
	                if (exists > 0)
	                    throw new InvalidOperationException("该租客已关联到此合同");

	                await conn.ExecuteAsync(
	                    _sql.Get("Lease.Insert.ContractTenant.Default"),
	                    new { ContractId = bizData.ContractId, TenantId = tenantId,
	                        IsPrimary = false, CreatedBy = Guid.Empty,
	                        CreatedAt = ChinaTime.Now }, tx);
	            }
	            else if (bizData.ChangeType == "TENANT_REMOVE")
	            {
	                // 验证租客已关联
	                var exists = await conn.QuerySingleAsync<int>(
	                    _sql.Get("Lease.Select.ContractTenant.ExistsByContractAndTenant"),
	                    new { ContractId = bizData.ContractId, TenantId = tenantId }, tx);
	                if (exists == 0)
	                    throw new InvalidOperationException("该租客未关联到此合同");

	                // 验证至少还有一个租客
	                var total = await conn.QuerySingleAsync<int>(
	                    _sql.Get("Lease.Select.ContractTenant.CountByContract"),
	                    new { ContractId = bizData.ContractId }, tx);
	                if (total <= 1)
	                    throw new InvalidOperationException("合同必须至少有一个租客");

	                await conn.ExecuteAsync(
	                    _sql.Get("Lease.Delete.ContractTenant.ByContractAndTenant"),
	                    new { ContractId = bizData.ContractId, TenantId = tenantId }, tx);
	            }

	            await conn.ExecuteAsync(
	                _sql.Get("Approval.Update.ApprovalBizData.MarkProcessed"),
	                new { Id = bizData.Id }, tx);
	            tx.Commit();
	        }
	        catch
	        {
	            tx.Rollback();
	            throw;
	        }
	    }

	    private async Task HandleSupplementaryFeeAsync(ApprovalCompletedEvent @event, CancellationToken ct)
	    {
	        if (@event.Action != "Approved") return;

	        var request = await _uow.SupplementaryFeeRequests.GetByIdAsync(@event.TargetEntityId, ct);
	        if (request == null || request.Status != "PendingApproval") return;

	        var locked = await _uow.ExecuteSqlRawAsync(
	            _sql.Get("SupplementaryFee.Update.Request.LockExecuting"),
	            new { Id = request.Id, Now = ChinaTime.Now }, ct);
	        if (locked == 0) return;

	        // 校验生效日期在合同起止日期范围内
	        var contract = await _uow.Contracts.GetByIdAsync(request.ContractId, ct);
	        if (contract != null && !string.IsNullOrEmpty(request.EffectiveDate))
	            Contract.ValidateFeeEffectiveDate(DateOnly.Parse(request.EffectiveDate), contract.StartDate, contract.EndDate);

	        List<dynamic> items;
        using (var conn3 = _db.CreateConnection()) { conn3.Open();
            items = (await conn3.QueryAsync<dynamic>(
                _sql.Get("SupplementaryFee.Select.Items.ByRequestId"), new { RequestId = request.Id })).ToList();
        }

	        using var conn = _db.CreateConnection(); conn.Open();
	        using var tx = conn.BeginTransaction();
	        try
	        {
	            // ★ 校验新生效日不与其他 FeeConfig 区间交叉
	            var overlap = await conn.QuerySingleAsync<int>(
	                _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
	                new { ContractId = request.ContractId, FeeCodeId = request.FeeCodeId,
	                    EffectiveDate = request.EffectiveDate, ExpiryDate = (string?)null,
	                    ExcludeId = (Guid?)null }, tx);
	            if (overlap > 0)
	                throw new InvalidOperationException("费用配置生效日期与已有记录存在交叉，请调整生效日期");

	            // 查 FeeCode 的 ChargeType
	            var feeCodeInfo = await conn.QuerySingleOrDefaultAsync<dynamic>(
	                            _sql.Get("FeeCode.Select.FeeCode.ChargeTypeById"),
	                            new { Id = request.FeeCodeId }, tx);
	            var feeChargeType = (string)(feeCodeInfo?.ChargeType ?? "Recurring");

	            Guid configId;
	            if (feeChargeType == "Recurring")
	            {
	                var segments = _billingDomain.CalculateMonthlySplit(
	                    request.Amount, request.EffectiveDate, ChinaTime.Now,
	                    contract.StartDate, contract.EndDate);
	                var segIds = await RecurringFeeSplitHelper.InsertMonthlySplitFeeConfigs(
	                    conn, tx, _sql, _billingDomain,
	                    request.ContractId, request.FeeCodeId,
	                    request.Amount, request.BillingMode, (string?)null, (decimal?)null,
	                    request.EffectiveDate, request.CreatedBy,
	                    contract.StartDate, contract.EndDate);
	                configId = segIds.Last();
	            }
	            else
	            {
	                configId = Guid.NewGuid();
	                await conn.ExecuteAsync(
	                            _sql.Get("Lease.Insert.ContractFeeConfig.Default"),
	                    new { Id = configId, ContractId = request.ContractId,
	                        FeeCodeId = request.FeeCodeId, BillingMode = request.BillingMode,
	                        Amount = request.Amount, EffectiveDate = request.EffectiveDate,
	                        CreatedBy = request.CreatedBy, CreatedAt = ChinaTime.Now }, tx);
	            }

	            var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
	                _sql.Get("Accounting.Select.Subject.ByCodes"), null, tx)).ToDictionary(r => r.Code, r => r.Id);
	            var receivableId = subjects.GetValueOrDefault("1122", Guid.Empty);
	            var revenueId = subjects.GetValueOrDefault("6001", subjects.GetValueOrDefault("6051", Guid.Empty));

	            foreach (var item in items)
	            {
	                var exists = await conn.QuerySingleAsync<int>(
	                    _sql.Get("Billing.Select.Journal.ExistsByKey"),
	                    new { C = request.ContractId, F = request.FeeCodeId, P = (string)item.Period }, tx);
	                if (exists > 0) continue;

		                var jId = Guid.NewGuid();
		                await InsertJournalAsync(conn, tx,
		                    request.CompanyId, request.ContractId, request.FeeCodeId, configId,
		                    (string)item.Period, (decimal)item.ProratedAmount,
		                    DateOnly.FromDateTime(ChinaTime.Now),
		                    "Normal", $"补充收费 {item.Period}");

	               
	                await conn.ExecuteAsync(
	                    _sql.Get("SupplementaryFee.Update.Item.SetPlanIds"),
	                    new { Id = (Guid)item.Id, ReceivablePlanId = jId, VoucherId = (Guid?)null }, tx);
	            }

	            await conn.ExecuteAsync(
	                _sql.Get("SupplementaryFee.Update.Request.Complete"),
	                new { Id = request.Id, FeeConfigId = configId }, tx);
	            tx.Commit();
	        }
	        catch { tx.Rollback(); throw; }
	    }

        private async Task SendNotificationsAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        var request = await _uow.ApprovalRequests.GetByIdAsync(@event.ApprovalRequestId, ct);
        if (request == null) return;

        if (@event.Action == "Approved")
        {
            await _notificationService.NotifySubmitterAsync(
                @event.ApprovalRequestId, $"{request.Title} 已通过", null, ct);
            await _notificationService.NotifyAllParticipantsAsync(
                @event.ApprovalRequestId, $"{request.Title} 已通过", null, ct);
        }
        else if (@event.Action == "Rejected")
        {
            var records = (await _uow.ApprovalRequests.GetByIdWithRecordsAsync(@event.ApprovalRequestId, ct))?.Records;
            var rejectRecord = records?.FirstOrDefault(r => r.Action == "Rejected");
            var reason = rejectRecord?.Comment;
            var content = reason != null ? $"原因：{reason}" : null;
            await _notificationService.NotifySubmitterAsync(
                @event.ApprovalRequestId, $"{request.Title} 已驳回", content, ct);
        }
    }

    /// <summary>校验费用配置区间不交叉（同合同+同费用项目，生效/到期日期不可重叠）</summary>
    private async Task EnsureNoOverlappingFeeConfigAsync(
        Guid contractId, Guid feeCodeId, string effectiveDate, string? expiryDate, Guid? excludeId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var overlap = await Dapper.SqlMapper.QuerySingleAsync<int>(conn,
            _sql.Get("Lease.Select.ContractFeeConfig.CheckOverlap"),
            new { ContractId = contractId, FeeCodeId = feeCodeId,
                EffectiveDate = effectiveDate, ExpiryDate = expiryDate, ExcludeId = excludeId });
        if (overlap > 0)
            throw new InvalidOperationException("费用配置生效日期区间存在交叉，请调整生效日期");
    }

    private async Task InsertChangeHistoryAsync(IDbConnection conn, IDbTransaction? tx,
        Guid contractId, string changeType, string title, string detail,
        decimal? oldValue, decimal? newValue, string? effectiveDate, Guid? operatorId, string? operatorName = null)
    {
        if (string.IsNullOrEmpty(operatorName) && operatorId.HasValue)
        {
            try { operatorName = await conn.QuerySingleOrDefaultAsync<string>(
                _sql.Get("Identity.Select.User.DisplayName"), new { Id = operatorId }, tx); } catch { }
        }
        await conn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
            new { Id = Guid.NewGuid(), ContractId = contractId, ChangeType = changeType,
                Title = title, Detail = detail, OldValue = oldValue, NewValue = newValue,
                EffectiveDate = effectiveDate, OperatorId = operatorId, OperatorName = operatorName ?? "" }, tx);
    }

    /// <summary>
    /// 插入未过账 Journal（GLPosted=0），统一入口，消除重复
    /// </summary>
    private async Task InsertJournalAsync(IDbConnection conn, IDbTransaction? tx,
        Guid companyId, Guid contractId, Guid feeCodeId, Guid? feeConfigId,
        string period, decimal amount, DateOnly dueDate, string entryType, string summary)
    {
        await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Unposted"),
            new
            {
                Id = Guid.NewGuid(),
                CoId = companyId,
                CId = contractId,
                FId = feeCodeId,
                FConfigId = feeConfigId,
                SubjId = Guid.Empty,
                Period = period,
                Amt = amount,
                Due = dueDate,
                EntryType = entryType,
                BilledAt = ChinaTime.Now,
                DNId = (Guid?)null,
                ParentId = (Guid?)null,
                Summary = summary,
                CBy = Guid.Empty
            }, tx);
    }

}
