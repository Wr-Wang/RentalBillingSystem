namespace RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

public class ApprovalLevelConfig : AuditableEntity, IHasLandlord
{
    public Guid ApprovalTypeId { get; private set; }
    public int Level { get; private set; }
    public Guid RoleId { get; private set; }
    public decimal? MinAmount { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public Guid LandlordId { get; private set; }
    private ApprovalLevelConfig() { }
    public ApprovalLevelConfig(Guid approvalTypeId, int level, Guid roleId, Guid landlordId)
    { ApprovalTypeId = approvalTypeId; Level = level; RoleId = roleId; LandlordId = landlordId; }
}
