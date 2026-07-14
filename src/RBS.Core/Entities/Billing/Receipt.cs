namespace RBS.Core.Entities.Billing;
using RBS.Core.Common;

using RBS.Core.Entities.Base;

/// <summary>
/// 收款聚合根 — 记录一笔来自租客的付款
/// 包含收款确认、驳回、分配等业务行为
/// </summary>
public class Receipt : AggregateRoot, IHasCompany
{
    /// <summary>收款单号，业务唯一标识</summary>
    public string ReceiptNo { get; private set; }
    /// <summary>关联合同标识，通过 LinkToContract 绑定</summary>
    public Guid? ContractId { get; private set; }
    /// <summary>收款总金额，创建时确定</summary>
    public decimal Amount { get; private set; }
    /// <summary>收款日期（到账日期）</summary>
    public DateOnly ReceivedDate { get; private set; }
    /// <summary>支付渠道标识，记录付款方式（银行转账、微信、支付宝等）</summary>
    public Guid? PaymentChannelId { get; private set; }
    /// <summary>外部流水号/参考号，来自支付渠道的回执</summary>
    public string? ReferenceNo { get; private set; }
    /// <summary>
    /// 收款状态：Pending（待确认）| Confirmed（已确认）| Rejected（已驳回）| Cancelled（已取消）
    /// </summary>
    public string Status { get; private set; }
    /// <summary>公司标识，实现 IHasCompany 接口</summary>
    public Guid CompanyId { get; private set; }
    /// <summary>乐观并发控制版本戳，用于 EF Core 并发冲突检测</summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // ===== 驳回信息 =====
    /// <summary>驳回原因，驳回时填写</summary>
    public string? RejectReason { get; private set; }
    /// <summary>确认时间，确认收款到账时记录</summary>
    public DateTime? ConfirmedAt { get; private set; }
    /// <summary>确认人用户标识</summary>
    public Guid? ConfirmedBy { get; private set; }

    // ===== 分配明细 =====
    private readonly List<ReceiptAllocation> _allocations = new();
    /// <summary>收款分配明细集合，将收款分配到各个应收计划</summary>
    public IReadOnlyCollection<ReceiptAllocation> Allocations => _allocations.AsReadOnly();

    /// <summary>已分配金额，所有分配明细金额之和，始终 ≤ Amount</summary>
    public decimal AllocatedAmount => _allocations.Sum(a => a.Amount);

    /// <summary>未分配金额</summary>
    public decimal UnallocatedAmount => Amount - AllocatedAmount;

    private Receipt() : base()
    {
        ReceiptNo = string.Empty;
        Status = "Pending";
    }

    /// <summary>领域构造函数</summary>
    /// <param name="receiptNo">收款单号，业务唯一标识</param>
    /// <param name="amount">收款总金额，必须大于 0</param>
    /// <param name="receivedDate">收款日期（到账日期）</param>
    /// <param name="companyId">公司标识</param>
    /// <exception cref="ArgumentException">收款单号为空或金额小于等于 0 时抛出</exception>
    public Receipt(string receiptNo, decimal amount, DateOnly receivedDate, Guid companyId) : base()
    {
        if (string.IsNullOrWhiteSpace(receiptNo))
            throw new ArgumentException("收款单号不能为空", nameof(receiptNo));
        if (amount <= 0) throw new ArgumentException("收款金额必须大于0", nameof(amount));

        ReceiptNo = receiptNo;
        Amount = amount;
        ReceivedDate = receivedDate;
        CompanyId = companyId;
        Status = "Pending";
    }

    /// <summary>
    /// 静态工厂方法 — 创建一笔新收款，自动生成收款单号
    /// </summary>
    /// <param name="amount">收款总金额，必须大于 0</param>
    /// <param name="receivedDate">收款日期（到账日期）</param>
    /// <param name="companyId">公司标识</param>
    /// <param name="paymentChannelId">支付渠道标识（可选）</param>
    /// <returns>新创建的 Pending 状态收款</returns>
    public static Receipt CreateNew(decimal amount, DateOnly receivedDate, Guid companyId, Guid? paymentChannelId = null)
    {
        var receipt = new Receipt(GenerateReceiptNo(), amount, receivedDate, companyId);
        if (paymentChannelId.HasValue)
        {
            receipt.SetPaymentChannel(paymentChannelId.Value);
        }
        return receipt;
    }

    /// <summary>生成收款单号：RCP + yyyyMMdd + 4位随机数</summary>
    private static string GenerateReceiptNo()
    {
        return $"RCP{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
    }

    // ===== 领域行为 =====

    /// <summary>确认收款到账，将状态从 Pending 变更为 Confirmed，记录确认时间和确认人</summary>
    /// <param name="userId">确认人用户标识</param>
    /// <exception cref="InvalidOperationException">状态不是 Pending 时无法确认</exception>
    /// <remarks>确认后触发 <see cref="PaymentConfirmedEvent"/> 领域事件</remarks>
    public void Confirm(Guid userId)
    {
        if (Status != "Pending")
            throw new InvalidOperationException($"状态为 {Status} 的收款不能确认");

        Status = "Confirmed";
        ConfirmedAt = ChinaTime.Now;
        ConfirmedBy = userId;

        AddDomainEvent(new PaymentConfirmedEvent(
            Id, ContractId ?? Guid.Empty, Amount));
    }

    /// <summary>驳回收款，将状态从 Pending 变更为 Rejected，记录驳回原因</summary>
    /// <param name="reason">驳回原因，不能为空</param>
    /// <exception cref="InvalidOperationException">状态不是 Pending 时无法驳回</exception>
    /// <exception cref="ArgumentException">驳回原因为空时抛出</exception>
    /// <remarks>驳回后触发 <see cref="PaymentRejectedEvent"/> 领域事件</remarks>
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

    /// <summary>设置支付渠道</summary>
    /// <param name="paymentChannelId">支付渠道标识</param>
    /// <exception cref="InvalidOperationException">只有待确认（Pending）的收款可以设置支付渠道</exception>
    public void SetPaymentChannel(Guid paymentChannelId)
    {
        if (Status != "Pending")
            throw new InvalidOperationException("只有待确认的收款可以设置支付渠道");
        PaymentChannelId = paymentChannelId;
    }

    /// <summary>关联合同，将收款归属到指定合同</summary>
    /// <param name="contractId">合同标识</param>
    /// <exception cref="InvalidOperationException">只有待确认（Pending）的收款可以关联合同</exception>
    public void LinkToContract(Guid contractId)
    {
        if (Status != "Pending")
            throw new InvalidOperationException("只有待确认的收款可以关联合同");
        ContractId = contractId;
    }

    /// <summary>分配收款到指定应收计划，创建 ReceiptAllocation 关联记录</summary>
    /// <param name="receivablePlanId">应收计划标识</param>
    /// <param name="amount">分配金额，必须大于 0 且不超过未分配余额</param>
    /// <exception cref="InvalidOperationException">只有已确认（Confirmed）的收款才能分配</exception>
    /// <exception cref="ArgumentException">分配金额小于等于 0 时抛出</exception>
    public void AllocateTo(Guid receivablePlanId, decimal amount)
    {
        if (Status != "Confirmed")
            throw new InvalidOperationException("只有已确认的收款才能分配");
        if (amount <= 0) throw new ArgumentException("分配金额必须大于0");
        if (AllocatedAmount + amount > Amount)
            throw new InvalidOperationException($"可分配余额不足（剩余 {UnallocatedAmount}）");

        _allocations.Add(new ReceiptAllocation(Id, receivablePlanId, amount));
    }

    /// <summary>取消分配，移除指定分配记录</summary>
    /// <param name="allocationId">分配记录标识</param>
    /// <exception cref="InvalidOperationException">未找到该分配记录时抛出</exception>
    public void RemoveAllocation(Guid allocationId)
    {
        var allocation = _allocations.FirstOrDefault(a => a.Id == allocationId)
            ?? throw new InvalidOperationException("未找到该分配记录");
        _allocations.Remove(allocation);
    }

    /// <summary>取消整笔收款，将状态置为 Cancelled</summary>
    /// <exception cref="InvalidOperationException">已确认且有分配记录的收款不能取消，需先取消分配</exception>
    public void Cancel()
    {
        if (Status == "Cancelled") return;
        if (Status == "Confirmed" && _allocations.Count > 0)
            throw new InvalidOperationException("已分配的收款不能取消，请先取消分配");
        Status = "Cancelled";
    }

    /// <summary>反向冲销 — 反转所有分配，取消收款</summary>
    /// <param name="allocations">外部传入的分配列表（Dapper 无延迟加载）</param>
    /// <param name="planReverseFn">为每个分配反转应收计划已收金额的回调委托</param>
    /// <exception cref="InvalidOperationException">只能冲销已确认（Confirmed）的收款</exception>
    public void Reverse(IReadOnlyList<ReceiptAllocation> allocations,
        Action<ReceiptAllocation> planReverseFn)
    {
        if (Status != "Confirmed")
            throw new InvalidOperationException("只能冲销已确认的收款");

        foreach (var alloc in allocations)
        {
            planReverseFn(alloc);
            _allocations.Remove(alloc);
        }

        Status = "Cancelled";
    }
}
