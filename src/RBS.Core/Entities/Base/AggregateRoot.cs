namespace RBS.Core.Entities.Base;

/// <summary>
/// 领域事件接口
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

/// <summary>
/// 聚合根基类 — 扩展 AuditableEntity，增加领域事件支持
/// </summary>
public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// 待发布的领域事件集合（只读）
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 添加领域事件
    /// </summary>
    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    /// <summary>
    /// 清除已发布的领域事件（由基础设施在发布后调用）
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected AggregateRoot() { }
}
