namespace RBS.Application.DTOs.Billing;

/// <summary>
/// 收款记录数据传输对象
/// </summary>
public class ReceiptDto
{
    /// <summary>收款记录 ID</summary>
    public Guid Id { get; set; }
    /// <summary>收款单号</summary>
    public string ReceiptNo { get; set; } = string.Empty;
    /// <summary>合同 ID（可为空）</summary>
    public Guid? ContractId { get; set; }
    /// <summary>收款金额</summary>
    public decimal Amount { get; set; }
    /// <summary>收款日期</summary>
    public DateOnly ReceivedDate { get; set; }
    /// <summary>状态：Pending / Confirmed / Rejected</summary>
    public string Status { get; set; } = "Pending";
    /// <summary>支付渠道名称</summary>
    public string? PaymentChannelName { get; set; }
}

/// <summary>
/// 创建收款记录请求
/// </summary>
public class CreateReceiptRequest
{
    /// <summary>收款单号</summary>
    public string ReceiptNo { get; set; } = string.Empty;
    /// <summary>合同 ID（可为空）</summary>
    public Guid? ContractId { get; set; }
    /// <summary>收款金额</summary>
    public decimal Amount { get; set; }
    /// <summary>收款日期</summary>
    public DateOnly ReceivedDate { get; set; }
    /// <summary>支付渠道 ID</summary>
    public Guid? PaymentChannelId { get; set; }
    /// <summary>参考号（银行流水号等）</summary>
    public string? ReferenceNo { get; set; }
    /// <summary>所属公司 ID</summary>
    public Guid CompanyId { get; set; }
}
