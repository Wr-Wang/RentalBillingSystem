namespace RBS.Application.DTOs.Billing;

/// <summary>
/// 收款记录数据传输对象
/// </summary>
public class ReceiptDto
{
    public Guid Id { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public Guid? ContractId { get; set; }
    public string? ContractNo { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public Guid? PaymentChannelId { get; set; }
    public string? ReferenceNo { get; set; }
    public string Status { get; set; } = "Pending";
    public Guid CompanyId { get; set; }
    public string? RejectReason { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public Guid? ConfirmedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
