namespace RBS.Core.Entities.Billing;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

/// <summary>
/// 催缴记录 — 每次催缴动作的记录
/// </summary>
public class CollectionRecord : AuditableEntity, IHasCompany
{
    public Guid ContractId { get; private set; }
    public int StageNo { get; private set; }
    public string Channel { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public Guid? OperatedBy { get; private set; }
    public Guid CompanyId { get; private set; }

    private CollectionRecord() { }

    public CollectionRecord(Guid contractId, int stageNo, string channel, string content, Guid companyId)
    {
        if (contractId == Guid.Empty) throw new ArgumentException("合同ID不能为空", nameof(contractId));
        ContractId = contractId;
        StageNo = stageNo;
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Status = "Pending";
        SentAt = ChinaTime.Now;
        CompanyId = companyId;
    }
}
