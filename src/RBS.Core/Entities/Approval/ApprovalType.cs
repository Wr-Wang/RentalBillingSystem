namespace RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

public class ApprovalType : AuditableEntity, IHasLandlord
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid LandlordId { get; private set; }
    private ApprovalType() { }
    public ApprovalType(string name, string code, Guid landlordId) { Name = name; Code = code; LandlordId = landlordId; }
}
