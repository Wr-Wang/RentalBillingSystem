namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 利息配置 — 每家公司的利息计算规则（AuditableEntity）
/// 定义了逾期付款时利息的计算参数，包括日利率、宽限期、上限及下限。
/// 支持多版本配置，通过生效日期（EffectiveDate）和启用状态（IsActive）管理
/// </summary>
public class InterestConfig : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 利息日利率，如 0.0005 表示万分之五（即每天 0.05%）
    /// </summary>
    public decimal DailyRate { get; private set; }

    /// <summary>
    /// 宽限天数，逾期超过此天数后才开始计算利息。
    /// 例如 GraceDays=3 表示逾期 3 天内不计利息
    /// </summary>
    public int GraceDays { get; private set; }

    /// <summary>
    /// 利息上限百分比（可选），如 100 表示利息不超过本金的 100%。
    /// 为 null 表示不设上限
    /// </summary>
    public decimal? MaxRate { get; private set; }

    /// <summary>
    /// 最低利息金额（可选），低于此值的利息不收取。
    /// 为 null 表示不设最低限制
    /// </summary>
    public decimal? MinAmount { get; private set; }

    /// <summary>
    /// 生效日期，该配置从该日期起生效。
    /// 系统按生效日期查找最新的活跃配置进行计算
    /// </summary>
    public DateOnly EffectiveDate { get; private set; }

    /// <summary>
    /// 是否启用，true=启用（使用该配置），false=停用（不生效）
    /// 默认值为 true
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 所属公司标识，每家公司可以设置独立的利息规则
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private InterestConfig() { }

    /// <summary>
    /// 创建利息配置实例。提供默认的日利率和宽限天数，其他参数可选
    /// </summary>
    /// <param name="dailyRate">日利率，如 0.0005 表示万分之五</param>
    /// <param name="graceDays">宽限天数</param>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="effectiveDate">生效日期</param>
    public InterestConfig(decimal dailyRate, int graceDays, Guid companyId, DateOnly effectiveDate)
    {
        DailyRate = dailyRate;
        GraceDays = graceDays;
        CompanyId = companyId;
        EffectiveDate = effectiveDate;
    }

    /// <summary>
    /// 更新利息配置的全部参数
    /// </summary>
    /// <param name="dailyRate">日利率</param>
    /// <param name="graceDays">宽限天数</param>
    /// <param name="maxRate">利息上限百分比（可选）</param>
    /// <param name="minAmount">最低利息（可选）</param>
    /// <param name="effectiveDate">生效日期</param>
    public void Update(decimal dailyRate, int graceDays, decimal? maxRate, decimal? minAmount, DateOnly effectiveDate)
    {
        DailyRate = dailyRate;
        GraceDays = graceDays;
        MaxRate = maxRate;
        MinAmount = minAmount;
        EffectiveDate = effectiveDate;
    }

    /// <summary>停用该利息配置，使其不再生效</summary>
    public void Deactivate() => IsActive = false;
}
