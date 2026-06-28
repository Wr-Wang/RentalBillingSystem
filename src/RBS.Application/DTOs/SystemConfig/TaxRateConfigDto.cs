namespace RBS.Application.DTOs.SystemConfig;

public class TaxRateConfigDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTaxRateConfigRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateOnly EffectiveDate { get; set; }
}

public class UpdateTaxRateConfigRequest
{
    public string? Name { get; set; }
    public decimal? Rate { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public bool? IsActive { get; set; }
}
