namespace RBS.Core.Entities.Billing;

using RBS.Core.Entities.Base;

/// <summary>
/// 收款聚合根 — 记录一笔来自租客的付款
/// 包含收款确认、驳回、分配等业务行为
/// </summary>
public class Receipt : AggregateRoot, IHasLandlord
{
    public string ReceiptNo { get; private set; }
    public Guid? ContractId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly ReceivedDate { get; private set; }
    public Guid? PaymentChannelId { get; private set; }
    public string? ReferenceNo { get; private set; }
    public string Status { get; private set; }
    public Guid LandlordId { get; private set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // ===== 驳回信息 =====
    public string? RejectReason { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public Guid? ConfirmedBy { get; private set; }

    // ===== 分配明细 =====
    private readonly List<ReceiptAllocation> _allocations = new();
    public IReadOnlyCollection<ReceiptAllocation> Allocations => _allocations.AsReadOnly();

    /// <summary>已分配金额</summary>
    public decimal AllocatedAmount => _allocations.Sum(a => a.Amount);

    /// <summary>未分配金额</summary>
    public decimal UnallocatedAmount => Amount - AllocatedAmount;

    private Receipt() : base()
    {
        ReceiptNo = string.Empty;
        Status = "Pending";
    }

    /// <summary>领域构造函数</summary>
    public Receipt(string receiptNo, decimal amount, DateOnly receivedDate, Guid landlordId) : base()
    {
        if (string.IsNullOrWhiteSpace(receiptNo))
            throw new ArgumentException("收款单号不能为空", nameof(receiptNo));
        if (amount <= 0) throw new ArgumentException("收款金额必须大于0", nameof(amount));

        ReceiptNo = receiptNo;
        Amount = amount;
        ReceivedDate = receivedDate;
        LandlordId = landlordId;
        Status = "Pending";
    }

    // ===== 领域行为 =====

    /// <summary>确认收款到账</summary>
    public void Confirm(Guid userId)
    {
        if (Status != "Pending")
            throw new InvalidOperationException($"状态为 {Status} 的收款不能确认");

        Status = "Confirmed";
        ConfirmedAt = DateTime.Now;
        ConfirmedBy = userId;

        AddDomainEvent(new PaymentConfirmedEvent(
            Id, ContractId ?? Guid.Empty, Amount));
    }

    /// <summary>驳回收款</summary>
    public void Reject(string reason)
    {
        if (Status != "Pending")
            throw new InvalidOperationException($"状态为 {Status} 的收款不能驳回");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("驳回原因不能为空");

        Status = "Rejected";
        RejectReason = reason;

        AddDomainEvent(new PaymentRejectedEvent(Id, reason));
    }

    /// <summary>关联合同</summary>
    public void LinkToContract(Guid contractId)
    {
        if (Status != "Pending")
            throw new InvalidOperationException("只有待确认的收款可以关联合同");
        ContractId = contractId;
    }

    /// <summary>分配收款到指定应收计划</summary>
    public void AllocateTo(Guid receivablePlanId, decimal amount)
    {
        if (Status != "Confirmed")
            throw new InvalidOperationException("只有已确认的收款才能分配");
        if (amount <= 0) throw new ArgumentException("分配金额必须大于0");
        if (AllocatedAmount + amount > Amount)
            throw new InvalidOperationException($"可分配余额不足（剩余 {UnallocatedAmount}）");

        _allocations.Add(new ReceiptAllocation(Id, receivablePlanId, amount));
    }

    /// <summary>取消分配</summary>
    public void RemoveAllocation(Guid allocationId)
    {
        var allocation = _allocations.FirstOrDefault(a => a.Id == allocationId)
            ?? throw new InvalidOperationException("未找到该分配记录");
        _allocations.Remove(allocation);
    }

    /// <summary>取消整笔收款</summary>
    public void Cancel()
    {
        if (Status == "Cancelled") return;
        if (Status == "Confirmed" && _allocations.Count > 0)
            throw new InvalidOperationException("已分配的收款不能取消，请先取消分配");
        Status = "Cancelled";
    }
}
