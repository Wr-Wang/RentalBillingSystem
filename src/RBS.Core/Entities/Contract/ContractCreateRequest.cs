namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同创建请求暂存实体 — 新建合同审批通过前暂存数据
/// 审批通过前不触碰 Contracts 主表
/// </summary>
public class ContractCreateRequest : AuditableEntity, IHasCompany
{
    public string ContractNo { get; private set; } = string.Empty;
    public Guid RoomId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string PaymentCycle { get; private set; } = "Monthly";
    public Guid CompanyId { get; private set; }
    public string Status { get; private set; } = "Draft"; // Draft/PendingApproval/Executing/Completed/Rejected
    public string? Remark { get; private set; }
    public Guid? ApprovalRequestId { get; private set; }
    public Guid? NewContractId { get; private set; }

    private ContractCreateRequest() { }

    public ContractCreateRequest(string contractNo, Guid roomId, DateOnly startDate, DateOnly endDate, string paymentCycle, Guid companyId)
    {
        ContractNo = contractNo;
        RoomId = roomId;
        StartDate = startDate;
        EndDate = endDate;
        PaymentCycle = paymentCycle;
        CompanyId = companyId;
        Status = "Draft";
    }

    public void Submit() => Status = "PendingApproval";
    public void Complete(Guid newContractId) { NewContractId = newContractId; Status = "Completed"; }
    public void Reject() => Status = "Rejected";
}
