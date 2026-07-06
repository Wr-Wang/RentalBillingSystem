using System.Text.RegularExpressions;
using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 审批完成事件处理器 — 审批通过/驳回后执行业务回调 + 通知相关人员
/// ★ v3 重构：幂等守卫 + 按 TargetEntityType 分发
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

    public ApprovalCompletedEventHandler(
        IImportService importService,
        IContractService contractService,
        IRenewalService renewalService,
        IContractDomainService contractDomainService,
        IUnitOfWork uow,
        INotificationService notificationService,
        ISqlLoader sql,
        IDbConnectionFactory db)
    {
        _importService = importService;
        _contractService = contractService;
        _renewalService = renewalService;
        _contractDomainService = contractDomainService;
        _uow = uow;
        _notificationService = notificationService;
        _sql = sql;
        _db = db;
    }

    public async Task HandleAsync(ApprovalCompletedEvent @event, CancellationToken ct)
    {
        // ★ 幂等守卫：通过 ApprovalBizData.IsProcessed（直接查 DB 避免缓存的脏数据）
        var bizData = await _uow.ApprovalBizData.GetByApprovalRequestIdAsync(@event.ApprovalRequestId, ct);
        if (bizData != null && bizData.IsProcessed)
        {
            return; // 已处理过，跳过
        }

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

    private async Task ExecuteBusinessCallbacksAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
    {
        switch (@event.TargetEntityType)
        {
            case "Import":
                await HandleImportAsync(@event, ct);
                break;

            case "ContractRent":
                await HandleContractRentAsync(@event, bizData, ct);
                break;

            case "ContractFeeAdjust":
                await HandleContractFeeAdjustAsync(@event, bizData, ct);
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
        }
    }

    // ★ 向后兼容：旧审批 TargetEntityType="Contract"
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
                    await _contractDomainService.ExecuteContractTerminationAsync(
                        bizData.ContractId, null, "FULL", request.Description ?? "合同终止", Guid.Empty, ct);
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
            else
            {
                // 旧租金调整：从 Description 正则解析
                var match = System.Text.RegularExpressions.Regex.Match(request.Description ?? "", @"→\s*¥([\d,]+)");
                if (match.Success)
                {
                    var newAmount = decimal.Parse(match.Groups[1].Value.Replace(",", ""));
                    await _contractService.AdjustRentAsync(@event.TargetEntityId, newAmount, ct);
                }
            }
        }
    }

    // ===== Import =====
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

    // ===== 租金调整 =====
    private async Task HandleContractRentAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
    {
        if (@event.Action != "Approved" || bizData == null) return;
        var contractId = bizData.ContractId;
        var newAmount = bizData.NewAmount ?? 0;
        var effectiveDate = bizData.EffectiveDate;

        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct);
        if (contract == null) return;

        // 调整租金（走领域模型）
        contract.AdjustRent(newAmount, effectiveDate);

        // 同步更新房租费 FeeConfig
        var rentFeeConfig = contract.FeeConfigs
            .FirstOrDefault(f => f.IsActive);
        if (rentFeeConfig != null)
        {
            var effDateStr = effectiveDate?.ToString("yyyy-MM-dd") ?? "";
            if (!string.IsNullOrEmpty(effDateStr))
            {
                // 区间不交叉校验（排除当前配置自身）
                await EnsureNoOverlappingFeeConfigAsync(contract.Id, rentFeeConfig.FeeCodeId,
                    effDateStr, null, rentFeeConfig.Id, ct);

                var expiryDate = DateOnly.Parse(effDateStr).AddDays(-1).ToString("yyyy-MM-dd");
                rentFeeConfig.ExpireOn(expiryDate);
                using (var conn2 = _db.CreateConnection()) { conn2.Open();
                    await InsertChangeHistoryAsync(conn2, null, contract.Id, "RENT_ADJUST",
                        "租金调整", $"月租金 {contract.RentAmount.Amount:F2} -> {newAmount:F2}",
                        contract.RentAmount.Amount, newAmount, effDateStr, Guid.Empty); }

                // 旧配置到期 + 新配置
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Lease.Update.ContractFeeConfig.ExpiryDate"),
                    new { ExpiryDate = expiryDate, Id = rentFeeConfig.Id }, ct);
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Lease.Insert.ContractFeeConfig.AfterAdjust"),
                    new { Id = Guid.NewGuid(), ContractId = contract.Id, FeeCodeId = rentFeeConfig.FeeCodeId,
                        BillingMode = "FixedAmount", Amount = newAmount,
                        EffectiveDate = effDateStr, CreatedBy = contract.CreatedBy, Now = ChinaTime.Now }, ct);
            }
        }

        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Lease.Update.Contract.RentAmount"),
            new { Id = contractId, NewAmount = newAmount }, ct);
        await _uow.CommitAsync(ct);
    }

    // ===== 费用调价 =====
    private async Task HandleContractFeeAdjustAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
    {
        if (@event.Action != "Approved" || bizData == null) return;

        var feeItems = await _uow.ApprovalFeeItems.GetByApprovalRequestIdAsync(@event.ApprovalRequestId, ct);
        if (feeItems.Count == 0) return;

        var approvalReq = await _uow.ApprovalRequests.GetByIdAsync(@event.ApprovalRequestId, ct);
        var userId = approvalReq?.CreatedBy ?? Guid.Empty;

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
    }

    // ===== 合同终止 =====
    private async Task HandleContractTerminateAsync(ApprovalCompletedEvent @event, ApprovalBizData? bizData, CancellationToken ct)
    {
        if (@event.Action != "Approved" || bizData == null) return;

        await _contractDomainService.ExecuteContractTerminationAsync(
            bizData.ContractId,
            bizData.ActualEndDate,
            bizData.DepositReturn ?? "FULL",
            bizData.Reason ?? "合同终止",
            Guid.Empty, ct);
        try { using var conn = _db.CreateConnection(); conn.Open();
            await conn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
                new { Id = Guid.NewGuid(), ContractId = bizData.ContractId, ChangeType = "TERMINATE",
                    Title = "合同终止", Detail = bizData.Reason ?? "",
                    OldValue = (decimal?)null, NewValue = (decimal?)null,
                    EffectiveDate = bizData.ActualEndDate?.ToString("yyyy-MM-dd"),
                    OperatorId = Guid.Empty, OperatorName = "" }); } catch { }
    }

    // ===== 续签 =====
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
                "SELECT DisplayName FROM Users WHERE Id=@Id", new { Id = operatorId }, tx); } catch { }
        }
        await conn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
            new { Id = Guid.NewGuid(), ContractId = contractId, ChangeType = changeType,
                Title = title, Detail = detail, OldValue = oldValue, NewValue = newValue,
                EffectiveDate = effectiveDate, OperatorId = operatorId, OperatorName = operatorName ?? "" }, tx);
    }

}
