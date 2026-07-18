namespace RBS.Application.DTOs.Billing;

/// <summary>
/// 创建收款请求
/// </summary>
public class CreateReceiptRequest
{
    public string? ReceiptNo { get; set; }
    public Guid? ContractId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public Guid? PaymentChannelId { get; set; }
    public string? ReferenceNo { get; set; }
    public Guid CompanyId { get; set; }
}
