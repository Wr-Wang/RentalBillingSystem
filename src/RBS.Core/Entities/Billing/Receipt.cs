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

    /// <summary>生成收款单号：RCP + yyyyMMddHHmmss + 3位随机数（精确到秒+随机防重）</summary>
    private static string GenerateReceiptNo()
    {
        var now = ChinaTime.Now;
        return $"RCP{now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
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

    /// <summary>取消整笔收款，将状态置为 Cancelled</summary>
    public void Cancel()
    {
        if (Status == "Cancelled") return;
        Status = "Cancelled";
    }
}
