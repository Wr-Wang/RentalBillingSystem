namespace RBS.Core.Entities.Base;

/// <summary>
/// 领域事件标记接口
/// 所有领域事件记录（record）必须实现此接口，以被聚合根的领域事件收集机制识别。
/// 基础设施层通过扫描实现此接口的事件并将其发布到消息总线或调度器。
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// 事件发生的时间戳（中国标准时间 UTC+8）
    /// 在事件构造时由 ChinaTime.Now 自动赋值，记录业务操作发生的真实时刻。
    /// </summary>
    DateTime OccurredAt { get; }
}

/// <summary>
/// 聚合根基类（Aggregate Root Base）
///
/// DDD 角色：聚合根（Aggregate Root）是领域驱动设计中一致性边界（Consistency Boundary）的入口。
/// 聚合根负责保证其内部所有实体和值对象的不变量（Invariants），外部只能通过聚合根的方法来修改内部状态。
///
/// 本类扩展了 <see cref="AuditableEntity"/>，增加了领域事件（Domain Event）的收集与发布能力。
/// 领域事件用于记录聚合根内部发生的业务行为，供同一聚合内的其他组件或外部订阅者响应。
///
/// 使用方式：
/// - 具体的聚合根实体（如 Contract、Receipt）继承此类
/// - 在业务方法中使用 AddDomainEvent 记录事件
/// - 基础设施层（如 EF Core SaveChanges 拦截器或 UnitOfWork）在持久化后调用 ClearDomainEvents
/// </summary>
public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// 获取当前聚合根中待发布的领域事件集合（只读视图）
    /// 基础设施层在提交工作单元（Unit of Work）后，应遍历此集合并逐条发布事件到消息总线，
    /// 然后调用 <see cref="ClearDomainEvents"/> 清空列表，避免重复发布。
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 添加一条领域事件到待发布队列
    /// 由聚合根内部的业务方法在关键业务操作完成后调用（如合同签订、收款确认、应收逾期等）。
    /// 事件不会立即发布，而是等到工作单元提交时由基础设施层统一发布，保证事件发布的原子性。
    /// </summary>
    /// <param name="event">要发布的领域事件实例，必须实现 <see cref="IDomainEvent"/> 接口</param>
    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    /// <summary>
    /// 清除所有已收集的领域事件
    /// 由基础设施层在事件成功发布到消息总线后调用，防止同一事件被重复推送。
    /// 通常在 EF Core 的 SaveChanges 后置拦截器、UnitOfWork.Commit 或 MediatR 发布管道中执行。
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// 保护构造方法，供子类通过链式构造调用
    /// </summary>
    protected AggregateRoot() { }
}
