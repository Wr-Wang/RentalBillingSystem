using RBS.Core.Entities.Base;

namespace RBS.Core.Common;

/// <summary>
/// 领域事件处理器泛型接口（Domain Event Handler）
///
/// 职责：定义领域事件的处理器契约，所有订阅领域事件的组件都应实现此接口。
///
/// 设计说明：
/// - 通过泛型参数 T 约束事件类型，确保处理器只处理其声明的事件类型
/// - IEventHandler&lt;ContractActivatedEvent&gt; 实现类只负责处理合同生效事件
/// - HandleAsync 为异步方法，支持事件处理中的 IO 操作（如数据库写入、消息推送）
/// - 基础设施层（如 MediatR 或自定义 Dispatcher）在领域事件发布后，
///   查找所有已注册的 IEventHandler&lt;T&gt; 实现并逐一调用 HandleAsync
///
/// 使用方式：
/// 1. 为每个领域事件创建对应的处理器类，实现 IEventHandler&lt;T&gt;
/// 2. 在 DI 容器中注册（通常用 Scoped 生命周期）
/// 3. 基础设施层自动完成事件路由和调用
///
/// 示例：
/// <code>
/// public class ContractActivatedHandler : IEventHandler&lt;ContractActivatedEvent&gt;
/// {
///     public async Task HandleAsync(ContractActivatedEvent @event, CancellationToken ct)
///     {
///         // 更新房源状态为已租、生成首期应收等
///     }
/// }
/// </code>
/// </summary>
/// <typeparam name="T">要处理的领域事件类型，必须实现 <see cref="IDomainEvent"/> 接口</typeparam>
public interface IEventHandler<T> where T : IDomainEvent
{
    /// <summary>
    /// 异步处理领域事件
    /// </summary>
    /// <param name="event">领域事件实例，包含事件发生时捕获的业务数据</param>
    /// <param name="ct">取消令牌，用于取消长时间运行的事件处理操作</param>
    /// <returns>表示异步操作的任务</returns>
    Task HandleAsync(T @event, CancellationToken ct);
}
