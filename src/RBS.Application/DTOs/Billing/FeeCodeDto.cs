namespace RBS.Application.DTOs.Billing;

public class FeeCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BillingMode { get; set; } = "FixedAmount";
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string Category { get; set; } = "Other";
    public bool IsRequired { get; set; }
    public Guid CompanyId { get; set; }
}

public class CreateFeeCodeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BillingMode { get; set; } = "FixedAmount";
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
    public string Category { get; set; } = "Other";
    public bool IsRequired { get; set; }
}

public class UpdateFeeCodeRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? BillingMode { get; set; }
    public string? Unit { get; set; }
    public int? SortOrder { get; set; }
    public string? Category { get; set; }
    public bool? IsRequired { get; set; }
    public bool? IsActive { get; set; }
}
