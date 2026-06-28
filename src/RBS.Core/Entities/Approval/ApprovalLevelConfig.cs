namespace RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

public class ApprovalLevelConfig : AuditableEntity, IHasCompany
{
    public Guid ApprovalTypeId { get; private set; }
    public int Level { get; private set; }
    public Guid RoleId { get; private set; }
    public decimal? MinAmount { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public Guid CompanyId { get; private set; }
    private ApprovalLevelConfig() { }
    public ApprovalLevelConfig(Guid approvalTypeId, int level, Guid roleId, Guid companyId)
    { ApprovalTypeId = approvalTypeId; Level = level; RoleId = roleId; CompanyId = companyId; }
    public void SetLevel(int level) => Level = level;
    public void SetRole(Guid roleId) => RoleId = roleId;
    public void SetAmountRange(decimal? min, decimal? max) { MinAmount = min; MaxAmount = max; }
}
