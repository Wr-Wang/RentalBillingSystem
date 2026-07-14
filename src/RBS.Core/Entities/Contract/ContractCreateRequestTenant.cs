namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同创建请求暂存租客关联 — ContractCreateRequest 的子实体
/// 暂存新建合同申请中选定的已有租客，审批通过后创建 ContractTenant 正式记录
/// </summary>
public class ContractCreateRequestTenant : AuditableEntity
{
    /// <summary>所属合同创建请求标识</summary>
    public Guid RequestId { get; private set; }
    /// <summary>租客标识，指向已存在的 Tenant 实体</summary>
    public Guid TenantId { get; private set; }
    /// <summary>是否为主承租人</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private ContractCreateRequestTenant() { }

    /// <summary>
    /// 创建合同创建请求的租客关联
    /// </summary>
    /// <param name="requestId">所属合同创建请求标识</param>
    /// <param name="tenantId">租客标识</param>
    /// <param name="isPrimary">是否为主承租人，默认 false</param>
    public ContractCreateRequestTenant(Guid requestId, Guid tenantId, bool isPrimary = false)
    {
        RequestId = requestId;
        TenantId = tenantId;
        IsPrimary = isPrimary;
    }
}
