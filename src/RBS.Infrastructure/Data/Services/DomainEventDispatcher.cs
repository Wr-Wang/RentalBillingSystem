using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 领域事件调度器实现 — 在聚合根持久化后分发挂起的领域事件
/// 遵循"发布后清除"模式：事件分发成功后自动调用 ClearDomainEvents()
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 分发多个聚合根上的所有待处理领域事件
    /// </summary>
    public async Task DispatchAsync(IEnumerable<AggregateRoot> aggregateRoots, CancellationToken ct = default)
    {
        foreach (var aggregate in aggregateRoots)
        {
            await DispatchAsync(aggregate, ct);
        }
    }

    /// <summary>
    /// 分发单个聚合根上的所有待处理领域事件
    /// </summary>
    /// <remarks>
    /// 对每个领域事件，通过 DI 解析所有已注册的 IEventHandler&lt;T&gt; 实现并逐一调用。
    /// 事件处理器的异常会记录日志但不会重新抛出，避免影响主事务。
    /// 所有事件处理完成后自动调用 ClearDomainEvents() 清空事件列表。
    /// </remarks>
    public async Task DispatchAsync(AggregateRoot aggregateRoot, CancellationToken ct = default)
    {
        var events = aggregateRoot.DomainEvents.ToArray();
        if (events.Length == 0) return;

        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                if (handler == null) continue;
                try
                {
                    var method = handlerType.GetMethod("HandleAsync");
                    if (method != null)
                    {
                        var task = (Task)method.Invoke(handler, new object[] { domainEvent, ct })!;
                        await task;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "领域事件 {EventType} 分发失败", domainEvent.GetType().Name);
                }
            }
        }

        aggregateRoot.ClearDomainEvents();
    }
}
