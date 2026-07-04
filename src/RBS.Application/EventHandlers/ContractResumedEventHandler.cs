using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 合同恢复事件处理器 — 解冻应收计划
/// </summary>
public class ContractResumedEventHandler : IEventHandler<ContractResumedEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly ISqlLoader _sql;
    private readonly INotificationService _notificationService;

    public ContractResumedEventHandler(IUnitOfWork uow, ISqlLoader sql, INotificationService notificationService)
    {
        _uow = uow;
        _sql = sql;
        _notificationService = notificationService;
    }

    public async Task HandleAsync(ContractResumedEvent @event, CancellationToken ct)
    {
        // 解冻应收：Frozen → Pending
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Update.ReceivablePlan.UnfreezeByContract"),
            new { ContractId = @event.ContractId }, ct);

        await _uow.CommitAsync(ct);
    }
}
