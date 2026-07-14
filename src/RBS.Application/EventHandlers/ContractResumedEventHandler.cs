using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 合同恢复事件处理器 — 合同从暂停恢复为 Active 时，
/// 将之前冻结（Frozen）的应收计划批量解冻为待收款（Pending）状态
/// </summary>
public class ContractResumedEventHandler : IEventHandler<ContractResumedEvent>
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
    public ContractResumedEventHandler(IUnitOfWork uow, ISqlLoader sql, INotificationService notificationService)
    {
        _uow = uow;
        _sql = sql;
        _notificationService = notificationService;
    }

    /// <summary>
    /// 处理合同恢复事件 — 解冻该合同所有 Frozen 应收计划为 Pending
    /// </summary>
    public async Task HandleAsync(ContractResumedEvent @event, CancellationToken ct)
    {
        // 解冻应收：Frozen → Pending
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Update.ReceivablePlan.UnfreezeByContract"),
            new { ContractId = @event.ContractId }, ct);

        await _uow.CommitAsync(ct);
    }
}
