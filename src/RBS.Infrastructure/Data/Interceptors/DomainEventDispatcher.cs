using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

namespace RBS.Infrastructure.Data.Interceptors;

/// <summary>
/// 领域事件分发器 — SaveChanges 后自动发布聚合根中的领域事件
/// SaveChangesAsync 成功后，发布所有 AggregateRoot 中累积的领域事件
/// </summary>
public class DomainEventDispatcher : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private List<IDomainEvent>? _capturedEvents;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context != null)
        {
            _capturedEvents = CollectDomainEvents(context);
        }
        return ValueTask.FromResult(result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_capturedEvents != null && _capturedEvents.Count > 0)
        {
            foreach (var @event in _capturedEvents)
            {
                await DispatchEventAsync(@event, cancellationToken);
            }

            // 清除已发布的事件
            var context = eventData.Context;
            if (context != null)
            {
                ClearDomainEvents(context);
            }
            _capturedEvents = null;
        }

        return result;
    }

    private static List<IDomainEvent> CollectDomainEvents(DbContext context)
    {
        var events = new List<IDomainEvent>();
        foreach (var entry in context.ChangeTracker.Entries<AggregateRoot>())
        {
            events.AddRange(entry.Entity.DomainEvents);
        }
        return events;
    }

    private static void ClearDomainEvents(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<AggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }

    private async Task DispatchEventAsync(IDomainEvent @event, CancellationToken ct)
    {
        var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);
        foreach (var handler in handlers)
        {
            if (handler == null) continue;
            var method = handlerType.GetMethod("HandleAsync");
            if (method != null)
            {
                await (Task)method.Invoke(handler, new object[] { @event, ct })!;
            }
        }
    }
}
