using RBS.Core.Entities.Base;

namespace RBS.Core.Common;

/// <summary>
/// 领域事件处理器接口
/// </summary>
public interface IEventHandler<T> where T : IDomainEvent
{
    Task HandleAsync(T @event, CancellationToken ct);
}
