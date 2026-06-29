namespace RBS.Application.DTOs.SystemConfig;

public class LateFeeConfigDto
{
    public Guid Id { get; set; }
    public decimal DailyRate { get; set; }
    public int GraceDays { get; set; }
    public decimal? MaxRate { get; set; }
    public decimal? MinAmount { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public bool IsActive { get; set; }
}

public class SaveLateFeeConfigRequest
{
    public decimal DailyRate { get; set; } = 0.0005m;
    public int GraceDays { get; set; } = 0;
    public decimal? MaxRate { get; set; }
    public decimal? MinAmount { get; set; }
    public DateOnly EffectiveDate { get; set; }
}
