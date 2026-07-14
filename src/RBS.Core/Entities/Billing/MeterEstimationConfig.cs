namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 抄表估算配置实体 — 当实际抄表读数不可用时，存储各费用项目的默认估算用量。
/// 继承自 <see cref="AuditableEntity"/> 并实现 <see cref="IHasCompany"/>，属于公司级数据。
/// </summary>
public class MeterEstimationConfig : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 费用项目ID — 标识此估算配置所属的费用项目（如电费、水费等）。
    /// </summary>
    public Guid FeeCodeId { get; private set; }

    /// <summary>
    /// 估算用量 — 当实际抄表数据缺失时使用的默认估算值，单位根据费用项目定义。
    /// </summary>
    public decimal EstimatedUsage { get; private set; }

    /// <summary>
    /// 备注 — 可选的补充说明，用于记录估算依据或特殊情形。
    /// </summary>
    public string? Remark { get; private set; }

    /// <summary>
    /// 所属公司ID — 实现 <see cref="IHasCompany"/>，标识此配置归属的租户/公司。
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 私有无参构造函数 — 供 EF Core 等 ORM 框架使用，禁止外部直接调用。
    /// </summary>
    private MeterEstimationConfig() { }

    /// <summary>
    /// 初始化抄表估算配置实体。
    /// </summary>
    /// <param name="feeCodeId">费用项目ID，关联费用项目定义。</param>
    /// <param name="estimatedUsage">估算用量，当抄表读数缺失时使用此默认值。</param>
    /// <param name="companyId">所属公司ID，标识租户/公司归属。</param>
    public MeterEstimationConfig(Guid feeCodeId, decimal estimatedUsage, Guid companyId)
    { FeeCodeId = feeCodeId; EstimatedUsage = estimatedUsage; CompanyId = companyId; }
}
