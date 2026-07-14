namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 费用科目模板实体（领域实体，继承 AuditableEntity 并实现 IHasCompany）
/// —— 关联费用科目（FeeCode）与会计科目（Subject），为自动凭证生成提供映射配置。
/// 每个费用科目可对应一个模板，模板中记录默认金额/单价以及借贷方会计科目。
/// 生命周期：创建 -> 设置会计科目 -> 可在使用中更新，无停用状态（通过 FeeCode 的 IsActive 控制）。
/// </summary>
public class FeeCodeTemplate : AuditableEntity, IHasCompany
{
    /// <summary>关联的费用科目 ID（FeeCode 的主键）</summary>
    public Guid FeeCodeId { get; private set; }

    /// <summary>模板描述，说明该模板的用途或适用场景，例如 "标准租金模板"</summary>
    public string? Description { get; private set; }

    /// <summary>默认金额（FixedAmount 模式下使用），单位：元，例如 5000.00 表示月租金 5000 元</summary>
    public decimal DefaultAmount { get; private set; }

    /// <summary>默认单价（Metered 模式下使用），例如水费 3.50 元/吨；FixedAmount 模式下为 null</summary>
    public decimal? DefaultUnitPrice { get; private set; }

    /// <summary>所属公司 ID，实现多租户隔离（IHasCompany）</summary>
    public Guid CompanyId { get; private set; }

    // ===== 会计科目映射（驱动自动凭证） =====
    /// <summary>借方会计科目 ID（如 银行存款/应收账款），自动凭证生成时作为借方分录</summary>
    public Guid? DebitSubjectId { get; private set; }
    /// <summary>贷方会计科目 ID（如 主营业务收入），自动凭证生成时作为贷方分录</summary>
    public Guid? CreditSubjectId { get; private set; }

    /// <summary>私有无参构造函数，供 EF Core 延迟加载使用</summary>
    private FeeCodeTemplate() { }

    /// <summary>
    /// 创建费用科目模板实例
    /// </summary>
    /// <param name="feeCodeId">关联的费用科目 ID</param>
    /// <param name="defaultAmount">默认金额，单位：元（FixedAmount 模式下的月金额）</param>
    /// <param name="companyId">所属公司 ID</param>
    public FeeCodeTemplate(Guid feeCodeId, decimal defaultAmount, Guid companyId)
    {
        FeeCodeId = feeCodeId;
        DefaultAmount = defaultAmount;
        CompanyId = companyId;
    }

    /// <summary>
    /// 设置借贷方会计科目映射，用于驱动自动凭证生成。
    /// 设置后，系统在生成账单时可根据此模板自动创建会计凭证的借贷分录。
    /// </summary>
    /// <param name="debitSubjectId">借方会计科目 ID（可 null，未设置时暂不生成借方分录）</param>
    /// <param name="creditSubjectId">贷方会计科目 ID（可 null，未设置时暂不生成贷方分录）</param>
    public void SetAccountingSubjects(Guid? debitSubjectId, Guid? creditSubjectId)
    {
        DebitSubjectId = debitSubjectId;
        CreditSubjectId = creditSubjectId;
    }
}
