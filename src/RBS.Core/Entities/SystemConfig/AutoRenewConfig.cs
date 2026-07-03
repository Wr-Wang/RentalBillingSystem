using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.SystemConfig;

/// <summary>
/// 自动续签策略配置
/// </summary>
public class AutoRenewConfig : AuditableEntity
{
    public Guid CompanyId { get; private set; }
    public int AdvanceDays { get; private set; } = 7;
    public string RentRule { get; private set; } = "Same";       // Same / Percentage / MarketPrice
    public decimal? RentIncreasePercent { get; private set; }
    public string TermRule { get; private set; } = "Same";       // Same / FixedMonths
    public int? TermMonths { get; private set; }
    public string OverdueAction { get; private set; } = "Block"; // Block / WarnAndContinue / Skip

    private AutoRenewConfig() { }

    public AutoRenewConfig(Guid companyId)
    {
        CompanyId = companyId;
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
    }
}
