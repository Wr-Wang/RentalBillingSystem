using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 合同暂停事件处理器 — 合同暂停时将当前月及之后所有待收款（Pending）
/// 应收计划冻结为 Frozen 状态，防止暂停期间继续计费
/// </summary>
public class ContractSuspendedEventHandler : IEventHandler<ContractSuspendedEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly ISqlLoader _sql;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="uow">工作单元</param>
    /// <param name="sql">SQL 加载器</param>
    /// <param name="notificationService">通知服务</param>
    public ContractSuspendedEventHandler(IUnitOfWork uow, ISqlLoader sql, INotificationService notificationService)
    {
        _uow = uow;
        _sql = sql;
        _notificationService = notificationService;
    }

    /// <summary>
    /// 处理合同暂停事件 — 冻结当前月份及之后的 Pending 应收计划为 Frozen
    /// </summary>
    public async Task HandleAsync(ContractSuspendedEvent @event, CancellationToken ct)
    {
        // 冻结未来应收：当前月份及之后的 Pending 应收改为 Frozen
        var currentPeriod = DateTime.UtcNow.ToString("yyyy-MM");
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Update.Journal.FreezeByContract"),
            new { ContractId = @event.ContractId, Period = currentPeriod }, ct);

        await _uow.CommitAsync(ct);
    }
}
