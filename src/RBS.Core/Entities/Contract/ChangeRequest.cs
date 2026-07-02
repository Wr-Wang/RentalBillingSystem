namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同变更请求实体 — 存储待审批的通用变更数据
/// 支持单个修改和批量修改（通过 BatchId 分组）
/// </summary>
public class ChangeRequest : AuditableEntity
{
    public Guid ContractId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string ChangeType { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Draft";
    public DateOnly? EffectiveDate { get; private set; }
    public string? Reason { get; private set; }
    public Guid? BatchId { get; private set; }
    public Guid? ApprovalRequestId { get; private set; }

    private readonly List<ChangeRequestItem> _items = new();
    public IReadOnlyCollection<ChangeRequestItem> Items => _items.AsReadOnly();

    private ChangeRequest() { }

    public ChangeRequest(Guid contractId, Guid companyId, string changeType, string? reason)
    {
        ContractId = contractId;
        CompanyId = companyId;
        ChangeType = changeType;
        Reason = reason;
        Status = "Draft";
    }

    public void SetEffectiveDate(DateOnly? date) => EffectiveDate = date;
    public void SetApprovalRequestId(Guid id) => ApprovalRequestId = id;
    public void SetBatchId(Guid? batchId) => BatchId = batchId;

    public void AddItem(string targetType, Guid? targetId, string fieldName,
        string? oldValue, string newValue, decimal? oldValueDec, decimal? newValueDec)
    {
        _items.Add(new ChangeRequestItem(Id, targetType, targetId, fieldName,
            oldValue, newValue, oldValueDec, newValueDec));
    }

    public void SubmitForApproval()
    {
        if (Status != "Draft")
            throw new InvalidOperationException($"状态为 {Status} 的变更请求不能提交");
        if (_items.Count == 0)
            throw new InvalidOperationException("变更请求至少需要一个变更项");
        Status = "PendingApproval";
    }

    public void Approve()
    {
        if (Status != "PendingApproval")
            throw new InvalidOperationException($"状态为 {Status} 的变更请求不能审批通过");
        Status = "Approved";
    }

    public void Reject()
    {
        if (Status != "PendingApproval")
            throw new InvalidOperationException($"状态为 {Status} 的变更请求不能驳回");
        Status = "Rejected";
    }
}
