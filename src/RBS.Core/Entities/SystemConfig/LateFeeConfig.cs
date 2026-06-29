namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 滞纳金配置 — 每家公司的滞纳金计算规则
/// </summary>
public class LateFeeConfig : AuditableEntity, IHasCompany
{
    public decimal DailyRate { get; private set; }         // 日利率（如 0.0005 = 万分之五）
    public int GraceDays { get; private set; }             // 宽限天数
    public decimal? MaxRate { get; private set; }          // 滞纳金上限（百分比，如 100 表示不超过本金）
    public decimal? MinAmount { get; private set; }        // 最低滞纳金（低于此值不计）
    public DateOnly EffectiveDate { get; private set; }    // 生效日期
    public bool IsActive { get; private set; } = true;
    public Guid CompanyId { get; private set; }

    private LateFeeConfig() { }

    public LateFeeConfig(decimal dailyRate, int graceDays, Guid companyId, DateOnly effectiveDate)
    {
        DailyRate = dailyRate;
        GraceDays = graceDays;
        CompanyId = companyId;
        EffectiveDate = effectiveDate;
    }

    public void Update(decimal dailyRate, int graceDays, decimal? maxRate, decimal? minAmount, DateOnly effectiveDate)
    {
        DailyRate = dailyRate;
        GraceDays = graceDays;
        MaxRate = maxRate;
        MinAmount = minAmount;
        EffectiveDate = effectiveDate;
    }

    public void Deactivate() => IsActive = false;
}
