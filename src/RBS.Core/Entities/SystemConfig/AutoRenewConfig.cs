namespace RBS.Core.Entities.SystemConfig;

/// <summary>
/// 自动续签策略配置
/// </summary>
public class AutoRenewConfig
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public int AdvanceDays { get; private set; } = 7;
    public string RentRule { get; private set; } = "Same";       // Same / Percentage / MarketPrice
    public decimal? RentIncreasePercent { get; private set; }
    public string TermRule { get; private set; } = "Same";       // Same / FixedMonths
    public int? TermMonths { get; private set; }
    public string OverdueAction { get; private set; } = "Block"; // Block / WarnAndContinue / Skip
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private AutoRenewConfig() { }

    public AutoRenewConfig(Guid companyId)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string rentRule, decimal? rentIncreasePercent, string termRule, int? termMonths,
        int advanceDays, string overdueAction)
    {
        RentRule = rentRule;
        RentIncreasePercent = rentIncreasePercent;
        TermRule = termRule;
        TermMonths = termMonths;
        AdvanceDays = advanceDays;
        OverdueAction = overdueAction;
        UpdatedAt = DateTime.UtcNow;
    }
}
