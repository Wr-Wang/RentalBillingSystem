namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 费用科目模板 — 关联 FeeCode 与会计科目，驱动自动凭证生成
/// </summary>
public class FeeCodeTemplate : AuditableEntity, IHasCompany
{
    public Guid FeeCodeId { get; private set; }
    public string? Description { get; private set; }
    public decimal DefaultAmount { get; private set; }
    public decimal? DefaultUnitPrice { get; private set; }
    public Guid CompanyId { get; private set; }

    // ===== 会计科目映射（驱动自动凭证） =====
    /// <summary>借方会计科目 ID（如 银行存款）</summary>
    public Guid? DebitSubjectId { get; private set; }
    /// <summary>贷方会计科目 ID（如 应收账款）</summary>
    public Guid? CreditSubjectId { get; private set; }

    private FeeCodeTemplate() { }

    public FeeCodeTemplate(Guid feeCodeId, decimal defaultAmount, Guid companyId)
    {
        FeeCodeId = feeCodeId;
        DefaultAmount = defaultAmount;
        CompanyId = companyId;
    }

    public void SetAccountingSubjects(Guid? debitSubjectId, Guid? creditSubjectId)
    {
        DebitSubjectId = debitSubjectId;
        CreditSubjectId = creditSubjectId;
    }
}
