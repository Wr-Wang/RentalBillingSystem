namespace RBS.Core.Entities.Billing;

/// <summary>
/// 账单收款快照实体
/// —— 出账时定格写入当期收款记录，PDF 导出时直接读取，无需再查询 Receipts 表。
/// 后续收款变更不影响已生成的账单快照。
/// </summary>
public class DebitNoteReceipt
{
    /// <summary>主键</summary>
    public Guid Id { get; private set; }

    /// <summary>所属账单 ID，指向 <see cref="DebitNote"/></summary>
    public Guid DebitNoteId { get; private set; }

    /// <summary>收款金额，单位：元</summary>
    public decimal Amount { get; private set; }

    /// <summary>收款日期</summary>
    public DateTime? ReceivedDate { get; private set; }

    /// <summary>支付渠道名称（快照字段），如 "银行转账"、"微信支付"</summary>
    public string? PaymentChannel { get; private set; }

    /// <summary>排序号，与账单明细中的显示顺序一致</summary>
    public int SortOrder { get; private set; }

    /// <summary>私有无参构造函数，供 Dapper / EF Core 使用</summary>
    private DebitNoteReceipt() { }

    /// <summary>
    /// 创建收款快照
    /// </summary>
    /// <param name="debitNoteId">所属账单 ID</param>
    /// <param name="amount">收款金额</param>
    /// <param name="receivedDate">收款日期</param>
    /// <param name="paymentChannel">支付渠道名称</param>
    /// <param name="sortOrder">排序号</param>
    public DebitNoteReceipt(Guid debitNoteId, decimal amount, DateTime? receivedDate,
        string? paymentChannel = null, int sortOrder = 0)
    {
        Id = Guid.NewGuid();
        DebitNoteId = debitNoteId;
        Amount = amount;
        ReceivedDate = receivedDate;
        PaymentChannel = paymentChannel;
        SortOrder = sortOrder;
    }
}
