namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同-租客关联实体 — Contract 聚合下的关联子实体（非聚合根）
/// 表示合同与租客之间的多对多关联关系，支持标识主承租人
/// </summary>
public class ContractTenant : AssociationEntity
{
    /// <summary>所属合同标识</summary>
    public Guid ContractId { get; private set; }
    /// <summary>租客标识，指向 Tenant 实体</summary>
    public Guid TenantId { get; private set; }
    /// <summary>是否为主承租人，一份合同有且仅有一个主承租人（用于催缴通知等场景）</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private ContractTenant() { }

    /// <summary>
    /// 创建合同-租客关联
    /// </summary>
    /// <param name="contractId">合同标识</param>
    /// <param name="tenantId">租客标识</param>
    /// <param name="isPrimary">是否为主承租人</param>
    public ContractTenant(Guid contractId, Guid tenantId, bool isPrimary)
    { ContractId = contractId; TenantId = tenantId; IsPrimary = isPrimary; }
}
