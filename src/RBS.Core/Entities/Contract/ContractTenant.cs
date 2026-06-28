namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

public class ContractTenant : AssociationEntity
{
    public Guid ContractId { get; private set; }
    public Guid TenantId { get; private set; }
    public bool IsPrimary { get; private set; }
    private ContractTenant() { }
    public ContractTenant(Guid contractId, Guid tenantId, bool isPrimary)
    { ContractId = contractId; TenantId = tenantId; IsPrimary = isPrimary; }
}
