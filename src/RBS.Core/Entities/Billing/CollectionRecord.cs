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
    { ContractId = contractId; CollectionStageId = collectionStageId; }
}
