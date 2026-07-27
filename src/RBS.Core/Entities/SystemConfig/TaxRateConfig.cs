namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 税率配置 — 各税率种类及其计算比例（AuditableEntity）
/// 定义了系统中使用的各类税率，如增值税、附加税等。
/// 每家公司可维护独立的税率表，支持多版本生效日期管理
/// </summary>
public class TaxRateConfig : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 税率名称，如 "增值税（6%）"、"附加税（12%）"，用于界面展示
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 税率比例，如 0.06 表示 6%、0.12 表示 12%
    /// </summary>
    public decimal Rate { get; private set; }

    /// <summary>
    /// 生效日期，该税率从该日期起适用。
    /// 系统按日期查找最新的活跃税率配置进行计算
    /// </summary>
    public DateTime EffectiveDate { get; private set; }

    /// <summary>
    /// 是否启用，true=启用（使用该税率），false=停用（不生效）
    /// 默认值为 true
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 所属公司标识，每家公司可设置独立的税率规则
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private TaxRateConfig() { }

    /// <summary>
    /// 创建税率配置实例
    /// </summary>
    /// <param name="name">税率名称</param>
    /// <param name="rate">税率比例（如 0.06 表示 6%）</param>
    /// <param name="effectiveDate">生效日期</param>
    /// <param name="companyId">所属公司标识</param>
    public TaxRateConfig(string name, decimal rate, DateTime effectiveDate, Guid companyId)
    { Name = name; Rate = rate; EffectiveDate = effectiveDate; CompanyId = companyId; }

    /// <summary>重命名税率名称</summary>
    /// <param name="name">新的税率名称</param>
    public void Rename(string name) => Name = name;

    /// <summary>设置税率比例</summary>
    /// <param name="rate">税率比例（如 0.06 表示 6%）</param>
    public void SetRate(decimal rate) => Rate = rate;

    /// <summary>设置生效日期</summary>
    /// <param name="date">生效日期</param>
    public void SetEffectiveDate(DateTime date) => EffectiveDate = date;

    /// <summary>启用该税率配置</summary>
    public void Activate() => IsActive = true;

    /// <summary>停用该税率配置</summary>
    public void Deactivate() => IsActive = false;
}
