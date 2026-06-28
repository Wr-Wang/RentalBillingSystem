namespace RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Base;

public class AccountingSubject : AuditableEntity, IHasCompany
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ParentCode { get; private set; }
    public int Level { get; private set; }
    public string Direction { get; private set; } = "Debit";
    public bool IsLeaf { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CompanyId { get; private set; }
    private AccountingSubject() { }
    public AccountingSubject(string code, string name, Guid companyId) { Code = code; Name = name; CompanyId = companyId; }
}
