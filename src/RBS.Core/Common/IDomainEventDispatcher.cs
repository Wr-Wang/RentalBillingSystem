using RBS.Core.Entities.Base;

namespace RBS.Core.Common;

/// <summary>
/// 领域事件调度器接口 — 负责在聚合根持久化后分发挂起的领域事件
/// 实现遵循"发布后清除"模式：事件分发成功后自动调用 ClearDomainEvents()
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// 分发聚合根上的所有待处理领域事件
    /// </summary>
    /// <param name="aggregateRoots">已持久化的聚合根集合</param>
    /// <param name="ct">取消令牌</param>
    Task DispatchAsync(IEnumerable<AggregateRoot> aggregateRoots, CancellationToken ct = default);

    /// <summary>
    /// 分发单个聚合根上的所有待处理领域事件
    /// </summary>
    Task DispatchAsync(AggregateRoot aggregateRoot, CancellationToken ct = default);
}
