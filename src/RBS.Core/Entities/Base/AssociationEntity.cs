namespace RBS.Core.Entities.Base;

/// <summary>
/// 关联表基类 — 仅记录创建信息，不做修改追踪
/// </summary>
public abstract class AssociationEntity
{
    public Guid Id { get; protected set; }
    public Guid CreatedBy { get; protected set; }
    public DateTime CreatedAt { get; protected set; }

    protected AssociationEntity()
    {
        Id = Guid.NewGuid();
    }

    public void SetCreated(Guid userId, DateTime utcNow)
    {
        CreatedBy = userId;
        CreatedAt = utcNow;
    }
}
