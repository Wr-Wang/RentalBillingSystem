namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.UnitOfWork;

/// <summary>
/// 合同领域服务 — 跨聚合的业务逻辑编排
/// 领域服务只做跨聚合校验和编排，由聚合根自身触发领域事件
/// </summary>
public class ContractDomainService : IContractDomainService
{
    private readonly IUnitOfWork _uow;

    public ContractDomainService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task ActivateContractAsync(Contract contract, CancellationToken ct = default)
    {
        // 校验房屋单元是否已有生效合同
        var hasActive = await _uow.Contracts.HasActiveForHousingUnitAsync(contract.RoomId, ct);
        if (hasActive)
            throw new InvalidOperationException("该房屋单元已有生效合同");

        // 聚合根自身执行状态变更 + 触发领域事件
        contract.Activate();
    }

    public async Task TerminateContractAsync(Contract contract, string reason, CancellationToken ct = default)
    {
        // 聚合根自身校验 + 触发事件
        contract.Terminate(reason);
    }

    public async Task SuspendContractAsync(Contract contract, CancellationToken ct = default)
    {
        contract.Suspend();
    }

    public async Task ResumeContractAsync(Contract contract, CancellationToken ct = default)
    {
        contract.Resume();
    }

    public async Task<Contract> RenewContractAsync(Contract oldContract, DateOnly newEndDate, decimal? newRentAmount, CancellationToken ct = default)
    {
        if (oldContract.StatusCode != "Active" && oldContract.StatusCode != "Expired")
            throw new InvalidOperationException("只有生效中或已到期的合同可以续签");

        // 标记旧合同为已续签
        oldContract.MarkAsRenewed();

        // 创建新合同
        var newContract = new Contract(
            $"{oldContract.ContractNo}-R{new Random().Next(1, 99)}",
            oldContract.RoomId,
            oldContract.CompanyId);

        newContract.SetRentAmount(newRentAmount ?? oldContract.RentAmount);
        newContract.SetDepositAmount(oldContract.DepositAmount);
        newContract.SetPeriod(oldContract.EndDate.AddDays(1), newEndDate);
        newContract.SetPaymentCycle(oldContract.PaymentCycle);

        return newContract;
    }

    public decimal CalculateProratedAmount(decimal monthlyAmount, DateOnly startDate, DateOnly endDate,
        DateOnly periodStart, DateOnly periodEnd)
    {
        var daysInMonth = DateTime.DaysInMonth(periodStart.Year, periodStart.Month);
        var dailyRate = monthlyAmount / daysInMonth;

        var effectiveStart = startDate > periodStart ? startDate : periodStart;
        var effectiveEnd = endDate < periodEnd ? endDate : periodEnd;
        var days = effectiveStart.DayNumber <= effectiveEnd.DayNumber
            ? effectiveEnd.DayNumber - effectiveStart.DayNumber + 1
            : 0;

        return Math.Round(dailyRate * days, 2);
    }
}
