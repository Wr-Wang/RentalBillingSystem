namespace RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

public class ApprovalType : AuditableEntity, IHasCompany
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CompanyId { get; private set; }
    private ApprovalType() { }
    public ApprovalType(string name, string code, Guid companyId) { Name = name; Code = code; CompanyId = companyId; }
    public void Rename(string name) => Name = name;
    public void SetDescription(string? description) => Description = description;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
