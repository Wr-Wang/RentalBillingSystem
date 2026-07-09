namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同创建请求暂存租客
/// </summary>
public class ContractCreateRequestTenant : AuditableEntity
{
    public Guid RequestId { get; private set; }
    public Guid TenantId { get; private set; }
    public bool IsPrimary { get; private set; }

    private ContractCreateRequestTenant() { }

    public ContractCreateRequestTenant(Guid requestId, Guid tenantId, bool isPrimary = false)
    {
        RequestId = requestId;
        TenantId = tenantId;
        IsPrimary = isPrimary;
    }
}
