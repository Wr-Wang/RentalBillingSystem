using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

namespace RBS.Application.EventHandlers;

/// <summary>
/// 合同租金调整事件处理器 — 从生效月份起重新生成应收
/// </summary>
public class ContractRentAdjustedEventHandler : IEventHandler<ContractRentAdjustedEvent>
{
    private readonly IReceivableGenerationService _receivableService;

    public ContractRentAdjustedEventHandler(IReceivableGenerationService receivableService)
    {
        _receivableService = receivableService;
    }

    public async Task HandleAsync(ContractRentAdjustedEvent @event, CancellationToken ct)
    {
        var fromPeriod = @event.EffectiveDate?.ToString("yyyy-MM");
        if (fromPeriod != null)
        {
            await _receivableService.GenerateAsync(@event.ContractId, fromPeriod, null, ct);
        }
    }
}
