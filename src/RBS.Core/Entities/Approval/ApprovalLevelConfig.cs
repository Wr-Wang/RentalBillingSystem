namespace RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

public class ApprovalLevelConfig : AuditableEntity, IHasCompany
{
    public Guid ApprovalTypeId { get; private set; }
    public int LevelNo { get; private set; }
    public Guid ApproverRoleId { get; private set; }
    public decimal? MinAmount { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public Guid CompanyId { get; private set; }
    private ApprovalLevelConfig() { }
    public ApprovalLevelConfig(Guid approvalTypeId, int levelNo, Guid approverRoleId, Guid companyId)
    { ApprovalTypeId = approvalTypeId; LevelNo = levelNo; ApproverRoleId = approverRoleId; CompanyId = companyId; }
    public void SetLevelNo(int levelNo) => LevelNo = levelNo;
    public void SetApproverRole(Guid approverRoleId) => ApproverRoleId = approverRoleId;
    public void SetAmountRange(decimal? min, decimal? max) { MinAmount = min; MaxAmount = max; }
}
