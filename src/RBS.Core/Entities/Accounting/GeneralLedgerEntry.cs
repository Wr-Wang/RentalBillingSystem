using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Accounting;

/// <summary>
/// 总账分录 — 记录每笔业务事件的科目级借贷变动
///
/// DDD 角色：领域实体（Entity），继承 AuditableEntity，实现 IHasCompany。
/// 由 JournalAppService.PostAsync（日记账过账）和 ReceiptService.ConfirmReceiptAsync（收款确认）写入，
/// 只读不修改，作为总账查询的原子数据单元。
/// </summary>
public class GeneralLedgerEntry : AuditableEntity, IHasCompany
{
    public Guid CompanyId { get; private set; }
    public Guid? ContractId { get; private set; }
    public string? ContractNo { get; private set; }
    public string Period { get; private set; } = string.Empty;
    public Guid SubjectId { get; private set; }
    public string SubjectCode { get; private set; } = string.Empty;
    public string Direction { get; private set; } = "Debit";
    public decimal Amount { get; private set; }
    public string SourceType { get; private set; } = string.Empty;
    public Guid? SourceId { get; private set; }
    public string? Description { get; private set; }
}
