namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class CollectionRecord : AuditableEntity
{
    public Guid ContractId { get; private set; }
    public Guid CollectionStageId { get; private set; }
    public string? ContactResult { get; private set; }
    public string? Remark { get; private set; }
    private CollectionRecord() { }
    public CollectionRecord(Guid contractId, Guid collectionStageId)
    {
        if (contractId == Guid.Empty) throw new ArgumentException("合同ID不能为空", nameof(contractId));
        if (collectionStageId == Guid.Empty) throw new ArgumentException("催缴阶段ID不能为空", nameof(collectionStageId));
        ContractId = contractId; CollectionStageId = collectionStageId;
    }
}
