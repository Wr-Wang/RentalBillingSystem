namespace RBS.Core.Entities.Base;

/// <summary>
/// 可审计实体基类 — 所有聚合根继承此类
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; protected set; }

    // ===== 审计基线字段 =====
    public Guid CreatedBy { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public string? CreatedIp { get; protected set; }
    public string? CreatedHostname { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string? UpdatedIp { get; protected set; }
    public string? UpdatedHostname { get; protected set; }

    protected AuditableEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = RBS.Core.Common.ChinaTime.Now;
    }

    /// <summary>
    /// 由领域工厂方法在创建实体时调用（基础设施层不再自动调用）
    /// </summary>
    public void SetCreated(Guid userId, DateTime utcNow, string? ip, string? hostname)
    {
        CreatedBy = userId;
        CreatedAt = utcNow;
        CreatedIp = ip;
        CreatedHostname = hostname;
    }

    /// <summary>
    /// 由 DapperRepository.UpdateAsync / 上层服务在更新实体时调用
    /// </summary>
    public void SetUpdated(Guid userId, DateTime utcNow, string? ip, string? hostname)
    {
        UpdatedBy = userId;
        UpdatedAt = utcNow;
        UpdatedIp = ip;
        UpdatedHostname = hostname;
    }
}
