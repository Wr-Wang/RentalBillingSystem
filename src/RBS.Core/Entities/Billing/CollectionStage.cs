namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 催缴阶段配置 — 按逾期天数范围定义催缴动作
/// </summary>
public class CollectionStage : AuditableEntity, IHasCompany
{
    public int StageNo { get; private set; }
    public string StageName { get; private set; } = string.Empty;
    public int OverdueDaysFrom { get; private set; }
    public int OverdueDaysTo { get; private set; }
    public string ActionType { get; private set; } = string.Empty;
    public bool IsAuto { get; private set; } = true;
    public Guid CompanyId { get; private set; }

    private CollectionStage() { }

    public CollectionStage(int stageNo, string stageName, int overdueDaysFrom, int overdueDaysTo, string actionType, Guid companyId)
    {
        StageNo = stageNo;
        StageName = stageName ?? throw new ArgumentNullException(nameof(stageName));
        OverdueDaysFrom = overdueDaysFrom;
        OverdueDaysTo = overdueDaysTo;
        ActionType = actionType ?? throw new ArgumentNullException(nameof(actionType));
        CompanyId = companyId;
    }
}
