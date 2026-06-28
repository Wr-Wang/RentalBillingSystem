namespace RBS.Application.DTOs.Billing;

public class ReceivablePlanDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Guid FeeCodeId { get; set; }
    public string? FeeCodeName { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Received { get; set; }
    public decimal Balance { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = "Pending";
}

public class ReceiptDto
{
    public Guid Id { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public Guid? ContractId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? PaymentChannelName { get; set; }
}

public class CreateReceiptRequest
{
    public decimal Amount { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public Guid? PaymentChannelId { get; set; }
    public string? ReferenceNo { get; set; }
    public Guid LandlordId { get; set; }
}
