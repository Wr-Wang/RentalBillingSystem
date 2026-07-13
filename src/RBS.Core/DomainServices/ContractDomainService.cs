namespace RBS.Core.DomainServices;

using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Core.Entities.Billing;

/// <summary>
/// 合同领域服务 — 跨聚合的业务逻辑编排
/// </summary>
public class ContractDomainService : IContractDomainService
{
    private readonly IUnitOfWork _uow;
    private readonly ISqlLoader _sql;

    public ContractDomainService(IUnitOfWork uow, ISqlLoader sql)
    {
        _uow = uow;
        _sql = sql;
    }

    public async Task ActivateContractAsync(Contract contract, CancellationToken ct = default)
    {
        var hasActive = await _uow.Contracts.HasActiveForHousingUnitAsync(contract.RoomId, ct);
        if (hasActive)
            throw new InvalidOperationException("该房屋单元已有生效合同");
        contract.Activate();
    }

    public async Task TerminateContractAsync(Contract contract, string reason, CancellationToken ct = default)
    {
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

    public async Task<Contract> RenewContractAsync(Contract oldContract, DateOnly newEndDate, CancellationToken ct = default)
    {
        if (oldContract.Status != "Active" && oldContract.Status != "Expired")
            throw new InvalidOperationException("只有生效中或已到期的合同可以续签");
        if (oldContract.EndDate == null) throw new InvalidOperationException("无固定到期日的合同不可续签");
        oldContract.MarkAsRenewed();
        var newContract = new Contract(
            $"{oldContract.ContractNo}-R{new Random().Next(1, 99)}",
            oldContract.RoomId,
            oldContract.CompanyId);
        newContract.SetPeriod(oldContract.EndDate.Value.AddDays(1), newEndDate);
        newContract.SetPaymentCycle(oldContract.PaymentCycle);
        return newContract;
    }

    /// <summary>
    /// 执行合同终止 — 审批通过后一次性完成所有终止操作
    /// </summary>
    public async Task ExecuteContractTerminationAsync(Guid contractId, DateOnly? actualEndDate, string depositReturn, string reason, Guid userId, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct);
        if (contract == null) throw new KeyNotFoundException("合同不存在");
        if (contract.Status == "Terminated")
            throw new InvalidOperationException("合同已终止，无需重复操作");

        // 1. 终止合同（唯一一次调用）
        contract.Terminate(reason);
        await _uow.Contracts.UpdateAsync(contract, ct);

        // 2. 取消未结清应收计划
        var plans = await _uow.ReceivablePlans.GetByContractIdAsync(contractId, ct);
        foreach (var plan in plans.Where(p => p.Status is "Pending" or "Partial" or "Overdue"))
        {
            plan.Cancel("合同终止");
            await _uow.ReceivablePlans.UpdateAsync(plan, ct);
        }

        // 3. 费用配置到期
        var effectiveEnd = actualEndDate?.ToString("yyyy-MM-dd") ?? contract.EndDate?.ToString("yyyy-MM-dd") ?? DateOnly.FromDateTime(ChinaTime.Now).ToString("yyyy-MM-dd");
        await _uow.ExecuteSqlRawAsync(
            _sql.Get("Contract.Update.ContractFeeConfig.ExpireByContract"),
            new { ExpiryDate = effectiveEnd, ContractId = contractId }, ct);

        // 4. 押金处理（由 ApprovalCompletedEventHandler 调用 TerminateJob 生成 JE）

        await _uow.CommitAsync(ct);
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
