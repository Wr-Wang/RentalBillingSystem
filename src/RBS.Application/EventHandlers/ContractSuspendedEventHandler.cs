using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 合同暂停事件处理器 — 冻结未来应收计划
/// </summary>
public class ContractSuspendedEventHandler : IEventHandler<ContractSuspendedEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly ISqlLoader _sql;
    private readonly INotificationService _notificationService;

    public ContractSuspendedEventHandler(IUnitOfWork uow, ISqlLoader sql, INotificationService notificationService)
    {
        _uow = uow;
        _sql = sql;
        _notificationService = notificationService;
    }

    public async Task HandleAsync(ContractSuspendedEvent @event, CancellationToken ct)
    {
        // 冻结未来应收：当前月份及之后的 Pending 应收改为 Frozen
        var currentPeriod = DateTime.UtcNow.ToString("yyyy-MM");
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Update.ReceivablePlan.FreezeByContract"),
            new { ContractId = @event.ContractId, Period = currentPeriod }, ct);

        await _uow.CommitAsync(ct);
    }
}
