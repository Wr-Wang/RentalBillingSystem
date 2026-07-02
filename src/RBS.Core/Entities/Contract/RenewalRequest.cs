namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 续签请求实体 — 存储待审批的续签数据
/// 审批通过前不触碰 Contracts 主表
/// </summary>
public class RenewalRequest : AuditableEntity
{
    public Guid OldContractId { get; private set; }
    public Guid? NewContractId { get; private set; }
    public string ContractNo { get; private set; } = string.Empty;
    public string RenewalType { get; private set; } = "Standard";
    public decimal PreviousRent { get; private set; }
    public decimal NewRent { get; private set; }
    public DateOnly NewEndDate { get; private set; }
    public string DepositHandling { get; private set; } = "TRANSFER";
    public decimal OldDepositAmount { get; private set; }
    public decimal? NewDepositAmount { get; private set; }
    public decimal? MarketReferencePrice { get; private set; }
    public bool PaymentStatusCheck { get; private set; }
    public string Status { get; private set; } = "Draft";
    public string? Remark { get; private set; }

    private RenewalRequest() { }

    public RenewalRequest(Guid oldContractId, string contractNo, decimal previousRent, decimal newRent, DateOnly newEndDate)
    {
        OldContractId = oldContractId;
        ContractNo = contractNo;
        PreviousRent = previousRent;
        NewRent = newRent;
        NewEndDate = newEndDate;
        Status = "Draft";
    }

    public void SetDepositInfo(string handling, decimal oldAmount, decimal? newAmount)
    {
        DepositHandling = handling;
        OldDepositAmount = oldAmount;
        NewDepositAmount = newAmount;
    }

    public void SetMarketPrice(decimal? price) => MarketReferencePrice = price;
    public void SetPaymentStatusCheck(bool passed) => PaymentStatusCheck = passed;
    public void SetRemark(string? remark) => Remark = remark;

    public void SubmitForApproval()
    {
        if (Status != "Draft")
            throw new InvalidOperationException($"状态为 {Status} 的续签请求不能提交");
        Status = "PendingApproval";
    }

    public void Complete(Guid newContractId)
    {
        if (Status != "PendingApproval" && Status != "Approved")
            throw new InvalidOperationException($"状态为 {Status} 的续签请求不能完成");
        NewContractId = newContractId;
        Status = "Completed";
    }

    public void Reject()
    {
        if (Status != "PendingApproval")
            throw new InvalidOperationException($"状态为 {Status} 的续签请求不能驳回");
        Status = "Rejected";
    }
}
