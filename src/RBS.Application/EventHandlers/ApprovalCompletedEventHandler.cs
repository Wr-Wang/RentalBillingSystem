using System.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common;
using RBS.Application.Common.Interfaces;
using RBS.Application.Services.Contract;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
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
    private readonly IContractTimelineService _timelineService;
    private readonly IAuditLogWriter _auditWriter;
    private readonly ICurrentUserService _currentUser;
    private readonly IJobScheduleExecutionService _executionService;
    private Guid CurrentUserId => _currentUser?.UserId ?? Guid.Empty;

    /// <summary>查询 BillJob 最新成功排期的 Month，计算 refDate（用于 CalculateMonthlySplit）</summary>
    private async Task<DateTime> GetRefDateAsync(Guid companyId)
    {
        var month = await _executionService.GetLatestSuccessMonthAsync(companyId);
        if (string.IsNullOrEmpty(month)) return ChinaTime.Now;
        var parts = month.Split('-');
        return new DateTime(int.Parse(parts[0]), int.Parse(parts[1]),
            DateTime.DaysInMonth(int.Parse(parts[0]), int.Parse(parts[1])));
    }

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
        IServiceProvider serviceProvider,
        IContractTimelineService timelineService,
        IAuditLogWriter auditWriter,
        ICurrentUserService currentUser,
        IJobScheduleExecutionService executionService)
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
        _timelineService = timelineService;
        _auditWriter = auditWriter;
        _currentUser = currentUser;
        _executionService = executionService;
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
                    new { Id = @event.ApprovalRequestId, UpdatedBy = CurrentUserId });
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
                            OperatorId = CurrentUserId, OperatorName = "" }); } } catch { }
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
        var userId = approvalReq?.CreatedBy ?? CurrentUserId;

        // 校验所有调价项的生效日期在合同起止日期范围内
        var contract = await _uow.Contracts.GetByIdAsync(bizData.ContractId, ct);
        if (contract != null)
        {
            foreach (var item in feeItems)
            {
                var effDate = item.EffectiveDate ?? bizData.EffectiveDate?.ToString("yyyy-MM-dd") ?? "";
                if (!string.IsNullOrEmpty(effDate))
                    Contract.ValidateFeeEffectiveDate(DateTime.Parse(effDate), contract.StartDate, contract.EndDate, item.FeeName);
            }
        }

        foreach (var item in feeItems)
        {
            var effectiveDate = item.EffectiveDate ?? bizData.EffectiveDate?.ToString("yyyy-MM-dd") ?? "";
            if (string.IsNullOrEmpty(effectiveDate)) continue;
            var expiryDate = DateTime.Parse(effectiveDate).AddDays(-1).ToString("yyyy-MM-dd");

            // 校验新生效日必须大于原生效日，否则到期日 < 生效日
            using var conn = _db.CreateConnection(); conn.Open();
            var current = await conn.QuerySingleOrDefaultAsync(
                _sql.Get("Lease.Select.ContractFeeConfig.CurrentByContractAndFee"),
                new { ContractId = item.ContractId, FeeCodeId = item.FeeCodeId });
            if (current != null)
            {
                var curEff = (DateTime)((dynamic)current).EffectiveDate;
                var newEff = DateTime.Parse(effectiveDate);
                if (newEff <= curEff)
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
                var meterConfigId = Guid.NewGuid();
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Contract.Insert.ContractFeeConfig.MeterBased"),
                    new { Id = meterConfigId, ContractId = item.ContractId, FeeCodeId = item.FeeCodeId,
                        Amount = item.OldAmount, Unit = item.Unit, UnitPrice = item.NewAmount,
                        EffectiveDate = effectiveDate, CreatedBy = userId });
                // ★ 审计：新建 MeterBased FeeConfig
                await _auditWriter.LogChangesAsync("ContractFeeConfigs", meterConfigId.ToString(), "Create",
                    new Dictionary<string, object?>
                    {
                        ["Id"] = meterConfigId, ["ContractId"] = item.ContractId,
                        ["FeeCodeId"] = item.FeeCodeId, ["BillingMode"] = "MeterBased",
                        ["Amount"] = item.OldAmount, ["Unit"] = item.Unit,
                        ["UnitPrice"] = item.NewAmount, ["EffectiveDate"] = effectiveDate,
                        ["IsActive"] = true, ["CreatedBy"] = userId
                    }, userId, ct);
            }
            else
            {
                using (var conn2 = _db.CreateConnection()) { conn2.Open();
                    await InsertChangeHistoryAsync(conn2, null, item.ContractId, "FEE_ADJUST",
                        "费用调价", item.FeeName + ": " + item.OldAmount.ToString("F2") + " -> " + item.NewAmount.ToString("F2"),
                        item.OldAmount, item.NewAmount, effectiveDate, userId); }

                // 固定金额：调 Amount
                var fixedConfigId = Guid.NewGuid();
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
                    new { Id = fixedConfigId, ContractId = item.ContractId, FeeCodeId = item.FeeCodeId,
                        BillingMode = "FixedAmount", Amount = item.NewAmount,
                        EffectiveDate = effectiveDate, CreatedBy = userId, Now = ChinaTime.Now }, ct);
                // ★ 审计：新建 FixedAmount FeeConfig
                await _auditWriter.LogChangesAsync("ContractFeeConfigs", fixedConfigId.ToString(), "Create",
                    new Dictionary<string, object?>
                    {
                        ["Id"] = fixedConfigId, ["ContractId"] = item.ContractId,
                        ["FeeCodeId"] = item.FeeCodeId, ["BillingMode"] = "FixedAmount",
                        ["Amount"] = item.NewAmount, ["EffectiveDate"] = effectiveDate,
                        ["IsActive"] = true, ["CreatedBy"] = userId
                    }, userId, ct);
            }
        }

                // 校验：生效月不能 <= BillJob 最新已执行月份（已出账月份不允许调价）
        var month = await _executionService.GetLatestSuccessMonthAsync(bizData.CompanyId);
        if (!string.IsNullOrEmpty(month))
        {
            foreach (var item in feeItems)
            {
                var effDate = item.EffectiveDate ?? bizData.EffectiveDate?.ToString("yyyy-MM-dd") ?? "";
                if (string.IsNullOrEmpty(effDate)) continue;
                var effMonth = effDate.Substring(0, 7);

                if (string.Compare(effMonth, month, StringComparison.Ordinal) <= 0)
                {
                    var effParts = effMonth.Split('-');
                    var effYear = int.Parse(effParts[0]); var effMon = int.Parse(effParts[1]);
                    var refParts = month.Split('-');
                    var refYear = int.Parse(refParts[0]); var refMon = int.Parse(refParts[1]);
                    var affected = new List<string>();
                    int y = effYear, m = effMon;
                    while (y < refYear || (y == refYear && m <= refMon))
                    {
                        affected.Add(y + "年" + m.ToString("D2") + "月");
                        m++; if (m > 12) { y++; m = 1; }
                    }
                    var nextDate = DateTime.Parse(month + "-01").AddMonths(1);
                    var errMsg = "费用 \"" + item.FeeName + "\" 的生效日期 " + effDate
                        + " 影响以下月份已生成的账单："
                        + string.Join("、", affected) + "的账单已生成，调价无法追溯已出账月份。"
                        + "请将生效日期调整为 " + nextDate.ToString("yyyy-MM-dd") + " 或之后。";
                    throw new InvalidOperationException(errMsg);
                }
            }
        }

        await _uow.CommitAsync(ct);
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
        var contractCache = new Dictionary<Guid, (DateTime StartDate, DateTime? EndDate)>();
        var contractIds = feeItems.Select(f => f.ContractId).Distinct().ToList();
        foreach (var cid in contractIds)
        {
            var c = await _uow.Contracts.GetByIdAsync(cid, ct);
            if (c == null) continue;
            contractCache[cid] = (c.StartDate, c.EndDate);
            foreach (var item in feeItems.Where(f => f.ContractId == cid))
            {
                var effDate = item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd");
                Contract.ValidateFeeEffectiveDate(DateTime.Parse(effDate), c.StartDate, c.EndDate, item.FeeName);
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
            var refDate = await GetRefDateAsync(companyId ?? Guid.Empty);

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
                        ? cd : (ChinaTime.Now, (DateTime?)null);
                    var segments = _billingDomain.CalculateMonthlySplit(
                        item.NewAmount,
                        item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                        refDate,
                        cStart, cEnd);
                    configIds = await RecurringFeeSplitHelper.InsertMonthlySplitFeeConfigs(
                        conn, tx, _sql, _billingDomain,
                        item.ContractId, item.FeeCodeId,
                        item.NewAmount, item.BillingMode!, item.Unit, (decimal?)null,
                        item.EffectiveDate ?? ChinaTime.Now.ToString("yyyy-MM-dd"),
                        CurrentUserId,
                        cStart, cEnd, refDate);
                        // 生成所有历史月份的日记账（首月及后续各月），不包含未来段
                        for (int i = 0; i < segments.Count; i++)
                        {
                            var seg = segments[i];
                            if (seg.ExpiryDate == null) continue;

                            var period = DateTime.Parse(seg.EffectiveDate).ToString("yyyy-MM");
                            var exists = await conn.QuerySingleAsync<int>(
                                _sql.Get("Billing.Select.Journal.ExistsByKey"),
                                new { C = item.ContractId, F = item.FeeCodeId, P = period }, tx);
                            if (exists > 0) continue;

                            await InsertJournalAsync(conn, tx,
                                companyId ?? Guid.Empty, item.ContractId, item.FeeCodeId, configIds[i],
                                period, seg.Amount,
                                ChinaTime.Now,
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
                            CreatedBy = CurrentUserId,
                            Now = ChinaTime.Now
                        }, tx);
                    // 暂记，等 Commit 后再生成 JE（避免独立连接被事务锁阻塞 → 超时）
                    oneTimeJobs.Add((item.ContractId, oneTimeConfigId));

                    // 插入 ReceivablePlan（一次性费用应收计划，关联到 FeeConfig 实例以支持同费用多次添加）
                    // 使用 Unposted SQL，GLPosted=0，待收款确认后再过账
                    var feeContract = await conn.QuerySingleOrDefaultAsync("SELECT StartDate FROM Contracts WHERE Id = @Id", new { Id = item.ContractId }, tx);
                    var contractStart = feeContract != null ? (DateTime)feeContract.StartDate : ChinaTime.Now;
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
                    null, item.NewAmount, effDate, CurrentUserId);
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

        // ★ 审计：记录到期前活跃费用配置
        List<dynamic> activeCfgs;
        using (var auditConn = _db.CreateConnection()) { auditConn.Open();
            activeCfgs = (await auditConn.QueryAsync(
                _sql.Get("Lease.Select.ContractFeeConfig.ActiveByContract"),
                new { Id = bizData.ContractId })).ToList(); }

        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Update.ContractFeeConfig.ExpireByContract"),
            new { ExpiryDate = result.EffectiveEndDate, ContractId = bizData.ContractId }, ct);
        foreach (var cfg in activeCfgs)
        {
            await _auditWriter.LogChangesAsync("ContractFeeConfigs",
                ((IDictionary<string, object>)cfg)["Id"]?.ToString() ?? Guid.Empty.ToString(), "Update",
                new Dictionary<string, object?>
                {
                    ["ContractId"] = bizData.ContractId,
                    ["ExpiryDate"] = result.EffectiveEndDate,
                    ["IsActive"] = false
                }, CurrentUserId, ct);
        }

        await _uow.CommitAsync(ct);

        try { using var conn = _db.CreateConnection(); conn.Open();
            await conn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
                new { Id = Guid.NewGuid(), ContractId = bizData.ContractId, ChangeType = "TERMINATE",
                    Title = "合同终止", Detail = bizData.Reason ?? "",
                    OldValue = (decimal?)null, NewValue = (decimal?)null,
                    EffectiveDate = bizData.ActualEndDate?.ToString("yyyy-MM-dd"),
                    OperatorId = CurrentUserId, OperatorName = "" }); } catch { }

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
                        new { Id = @event.ApprovalRequestId, UpdatedBy = CurrentUserId });
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
                var oldStatus = renewal.Status;
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Approval.Update.RenewalRequest.ToRejected"),
                    new { Id = renewal.Id }, ct);
                // ★ 审计：续签驳回
                await _auditWriter.LogChangesAsync("RenewalRequests", renewal.Id.ToString(), "Update",
                    new Dictionary<string, object?>
                    {
                        ["Id"] = renewal.Id, ["Status"] = "Rejected",
                        ["UpdatedAt"] = ChinaTime.Now
                    }, CurrentUserId, ct);
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
		                    var refDate = await GetRefDateAsync(request.CompanyId);
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

		        // 写入变更历史（独立连接，失败不阻断主流程）
        try { await _timelineService.InsertChangeHistoryAsync(contractId, "CONTRACT_ACTIVATE",
                "合同创建审批通过",
                $"合同 {request.ContractNo} 已激活，起租 {request.StartDate:yyyy-MM-dd}",
                null, null, request.StartDate.ToString("yyyy-MM-dd"), request.CreatedBy); } catch { }

			        // 写入 Contracts_Audit 全量快照（事务外查询，失败不阻断）
			        try { await WriteAuditSnapshotAsync("Contracts", contractId, "Create", request.CreatedBy, ct); } catch { }

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
                var cStart = (DateTime)contractRow.StartDate;
                var cEnd = contractRow.EndDate != null
                    ? (DateTime)contractRow.EndDate : (DateTime?)null;

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
                                CreatedBy = CurrentUserId,
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
                    (DateTime)item.DueDate,
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
                    OperatorId = CurrentUserId,
                    OperatorName = ""
                });
        }
        catch { /* 变更历史写入失败不影响主流程 */ }

        await _uow.CommitAsync(ct);

        // 写入 Contracts_Audit 全量快照（修改后的合同数据，失败不阻断）
        try { await WriteAuditSnapshotAsync("Contracts", request.ContractId, "Update", CurrentUserId, ct); } catch { }
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
	                        IsPrimary = false, CreatedBy = CurrentUserId,
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

	            // 写入变更历史（领域服务）
	            var tenantName = await conn.QuerySingleOrDefaultAsync<string>(
	                _sql.Get("Contract.Select.Tenant.NameById"), new { Id = tenantId }, tx);
	            var tenantDisplay = tenantName ?? "未知租客";
	            var changeTypeLabel = bizData.ChangeType == "TENANT_ADD" ? "添加租客" : "移除租客";
	            try { await _timelineService.InsertChangeHistoryAsync(bizData.ContractId, "TENANT_CHANGE",
	                changeTypeLabel, $"租客 {tenantDisplay}（{bizData.ChangeType}）",
	                null, null, null, CurrentUserId); } catch { }

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
	            Contract.ValidateFeeEffectiveDate(DateTime.Parse(request.EffectiveDate), contract!.StartDate, contract!.EndDate);

	        List<dynamic> items;
        using (var conn3 = _db.CreateConnection()) { conn3.Open();
            items = (await conn3.QueryAsync<dynamic>(
                _sql.Get("SupplementaryFee.Select.Items.ByRequestId"), new { RequestId = request.Id })).ToList();
        }

	        using var conn = _db.CreateConnection(); conn.Open();
	        using var tx = conn.BeginTransaction();

	        Guid configId; // 提升到 try 外，供后续审计写入使用
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

            var refDate = await GetRefDateAsync(contract!.CompanyId);
	            if (feeChargeType == "Recurring")
	            {
	                var segments = _billingDomain.CalculateMonthlySplit(
	                    request.Amount, request.EffectiveDate, refDate,
	                    contract!.StartDate, contract!.EndDate);
	                var segIds = await RecurringFeeSplitHelper.InsertMonthlySplitFeeConfigs(
	                    conn, tx, _sql, _billingDomain,
	                    request.ContractId, request.FeeCodeId,
	                    request.Amount, request.BillingMode, (string?)null, (decimal?)null,
	                    request.EffectiveDate, request.CreatedBy,
	                    contract!.StartDate, contract!.EndDate);
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
		                    ChinaTime.Now,
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


	        // 写入变更历史（独立连接，失败不阻断主流程）
	        try { await _timelineService.InsertChangeHistoryAsync(request.ContractId, "SUPPLEMENTARY_FEE",
                "补充收费",
                $"补充收费 {request.FeeCodeId} ¥{request.Amount:F2}，生效 {request.EffectiveDate}",
                null, request.Amount, request.EffectiveDate, request.CreatedBy); } catch { }

	        // 写入 ContractFeeConfigs_Audit 全量快照（事务外查询，失败不阻断）
	        try { await WriteAuditSnapshotAsync("ContractFeeConfigs", configId, "Create", request.CreatedBy, ct); } catch { }

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
        string period, decimal amount, DateTime dueDate, string entryType, string summary)
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
                CBy = CurrentUserId
            }, tx);
    }

    /// <summary>
    /// 写入 _Audit 全量快照 — 从业务表查询当前数据并写入审计镜像表
    /// </summary>
    private async Task WriteAuditSnapshotAsync(string tableName, Guid entityId, string action,
        Guid? changedBy = null, CancellationToken ct = default)
    {
        try
        {
            using var qConn = _db.CreateConnection();
            qConn.Open();
            var entity = await qConn.QuerySingleOrDefaultAsync<dynamic>(
                $"SELECT * FROM [{tableName}] WHERE Id=@Id", new { Id = entityId });
            if (entity == null) return;
            var dict = new Dictionary<string, object?>();
            foreach (var prop in ((IDictionary<string, object>)entity))
                dict[prop.Key] = prop.Value;
            await _auditWriter.LogChangesAsync(tableName, entityId.ToString(), action, dict,
                changedBy ?? CurrentUserId, ct);
        }
        catch { /* 审计写入失败不影响主流程 */ }
    }

}
