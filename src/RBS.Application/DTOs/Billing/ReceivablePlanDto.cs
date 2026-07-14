namespace RBS.Application.DTOs.Billing;

/// <summary>
/// 应收计划数据传输对象 — 按合同+账期拆分的应收明细
/// </summary>
public class ReceivablePlanDto
{
    /// <summary>应收计划 ID</summary>
    public Guid Id { get; set; }
    /// <summary>合同 ID</summary>
    public Guid ContractId { get; set; }
    /// <summary>费用代码 ID</summary>
    public Guid FeeCodeId { get; set; }
    /// <summary>费用代码名称</summary>
    public string? FeeCodeName { get; set; }
    /// <summary>账期 (yyyy-MM)</summary>
    public string Period { get; set; } = string.Empty;
    /// <summary>应收金额</summary>
    public decimal Amount { get; set; }
    /// <summary>已收金额</summary>
    public decimal Received { get; set; }
    /// <summary>余额（应收 - 已收）</summary>
    public decimal Balance { get; set; }
    /// <summary>到期日</summary>
    public DateOnly DueDate { get; set; }
    /// <summary>状态：Pending / Frozen / Paid / Overdue</summary>
    public string Status { get; set; } = "Pending";
}

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
