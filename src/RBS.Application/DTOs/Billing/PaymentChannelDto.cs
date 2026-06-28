namespace RBS.Application.DTOs.Billing;

public class PaymentChannelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreatePaymentChannelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class UpdatePaymentChannelRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
}
