using RBS.Core.Common;
using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Approval;

/// <summary>
/// 审批业务数据 — 合同操作闭环的结构化业务参数
/// 1:1 关联 ApprovalRequest，存储调租/调价/终止等操作的业务数据
/// </summary>
public class ApprovalBizData : AuditableEntity
{
    public Guid? ApprovalRequestId { get; private set; }
    public Guid ContractId { get; private set; }
    public string? ContractNo { get; private set; }
    public Guid CompanyId { get; private set; }

    /// <summary>变更类型：RENT_ADJUST / FEE_ADJUST / TERMINATE / SUSPEND</summary>
    public string ChangeType { get; private set; }

    /// <summary>生效日期（调租/调价使用）</summary>
    public DateOnly? EffectiveDate { get; private set; }

    /// <summary>旧金额</summary>
    public decimal? OldAmount { get; private set; }

    /// <summary>新金额</summary>
    public decimal? NewAmount { get; private set; }

    /// <summary>原因说明</summary>
    public string? Reason { get; private set; }

    // === 终止专用字段 ===

    /// <summary>终止类型：EARLY / EXPIRED</summary>
    public string? TerminateType { get; private set; }

    /// <summary>实际搬离日</summary>
    public DateOnly? ActualEndDate { get; private set; }

    /// <summary>押金处理：FULL / DEDUCT / LAST_RENT</summary>
    public string? DepositReturn { get; private set; }

    // === 幂等标记 ===

    /// <summary>审批回调是否已执行</summary>
    public bool IsProcessed { get; private set; }

    /// <summary>回调执行时间</summary>
    public DateTime? ProcessedAt { get; private set; }

    private ApprovalBizData() : base()
    {
        ChangeType = string.Empty;
    }

    public ApprovalBizData(Guid contractId, string? contractNo, Guid companyId, string changeType) : base()
    {
        ContractId = contractId;
        ContractNo = contractNo;
        CompanyId = companyId;
        ChangeType = changeType;
        IsProcessed = false;
    }

    // ===== 领域行为 =====

    public void SetApprovalRequestId(Guid approvalRequestId) => ApprovalRequestId = approvalRequestId;

    public void SetRentAdjustData(decimal oldAmount, decimal newAmount, DateOnly? effectiveDate, string? reason)
    {
        OldAmount = oldAmount;
        NewAmount = newAmount;
        EffectiveDate = effectiveDate;
        Reason = reason;
    }

    public void SetFeeAdjustData(DateOnly? effectiveDate, string? reason)
    {
        EffectiveDate = effectiveDate;
        Reason = reason;
    }

    public void SetTerminateData(string terminateType, DateOnly? actualEndDate, string? depositReturn, string? reason)
    {
        TerminateType = terminateType;
        ActualEndDate = actualEndDate;
        DepositReturn = depositReturn;
        Reason = reason;
    }

    /// <summary>标记回调已执行（幂等保护）</summary>
    public void MarkAsProcessed()
    {
        if (IsProcessed)
            throw new InvalidOperationException("业务数据已被处理，不可重复标记");
        IsProcessed = true;
        ProcessedAt = ChinaTime.Now;
    }
}
