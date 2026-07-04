using RBS.Application.Common.Interfaces;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.UnitOfWork;
using ContractEntity = RBS.Core.Entities.Contract.Contract;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 应收生成编排服务 — 批量按合同生成应收计划，去重后写入
/// </summary>
public class ReceivableGenerationService : IReceivableGenerationService
{
    private readonly IUnitOfWork _uow;
    private readonly IBillingDomainService _billingDomain;

    public ReceivableGenerationService(
        IUnitOfWork uow,
        IBillingDomainService billingDomain)
    {
        _uow = uow;
        _billingDomain = billingDomain;
    }

    public async Task<int> GenerateAsync(Guid contractId, string? periodFrom, string? periodTo, CancellationToken ct)
    {
        // 1. 加载合同（含费用配置）
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct)
            ?? throw new InvalidOperationException($"合同 {contractId} 不存在");

        if (contract.Status != "Active")
            throw new InvalidOperationException($"合同状态为 {(string)contract.Status}，非生效中无法生成应收");

        // 2. 确定账期范围
        var allPeriods = SplitPeriods(contract);
        var from = periodFrom ?? allPeriods.First();
        var to = periodTo ?? allPeriods.Last();

        var matched = allPeriods
            .Where(p => string.Compare(p, from, StringComparison.Ordinal) >= 0
                     && string.Compare(p, to, StringComparison.Ordinal) <= 0)
            .ToList();

        if (matched.Count == 0)
            throw new InvalidOperationException($"账期范围 {from}~{to} 不在合同有效期内");

        // 3. 逐月生成（去重：同一合同+账期+费用类型只生成一次）
        int totalCreated = 0;

        foreach (var period in matched)
        {
            var dueDate = CalculateDueDate(period, contract);

            var plans = _billingDomain.GenerateReceivablePlans(contract, period, dueDate);

            foreach (var plan in plans)
            {
                // 去重检查
                var existing = await _uow.ReceivablePlans
                    .GetByContractPeriodFeeAsync(contractId, period, plan.FeeCodeId, ct);

                if (existing != null)
                    continue; // 已存在则跳过

                await _uow.ReceivablePlans.AddAsync(plan, ct);
                totalCreated++;
            }
        }

        await _uow.CommitAsync(ct);
        return totalCreated;
    }

    public List<string> SplitPeriods(ContractEntity contract)
    {
        var start = contract.StartDate;
        var end = contract.EndDate;
        var periods = new List<string>();

        var current = new Period(start.Year, start.Month);
        var endPeriod = new Period(end.Year, end.Month);

        while (current.Year < endPeriod.Year || (current.Year == endPeriod.Year && current.Month <= endPeriod.Month))
        {
            periods.Add(current.ToString());
            current = current.Next();
        }

        return periods;
    }

    public DateOnly CalculateDueDate(string periodStr, ContractEntity contract)
    {
        var period = Period.Parse(periodStr);
        var lastDay = DateTime.DaysInMonth(period.Year, period.Month);
        var dueDay = Math.Min(contract.EndDate.Day, lastDay);
        return new DateOnly(period.Year, period.Month, dueDay);
    }
}
