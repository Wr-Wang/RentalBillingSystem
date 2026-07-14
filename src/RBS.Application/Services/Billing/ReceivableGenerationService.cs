using Dapper;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using ContractEntity = RBS.Core.Entities.Contract.Contract;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 应收生成编排服务实现 — 批量按合同生成应收计划，去重后写入
/// 合同激活时同步生成应收计划及对应的会计凭证（Voucher + JournalEntry）
/// 依赖 IBillingDomainService 领域服务计算按月分摊的应收计划
/// </summary>
public class ReceivableGenerationService : IReceivableGenerationService
{
    private readonly IUnitOfWork _uow;
    private readonly IBillingDomainService _billingDomain;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="uow">工作单元</param>
    /// <param name="billingDomain">计费领域服务，用于计算应收计划</param>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="sql">SQL 加载器</param>
    /// <param name="serviceProvider">服务提供者（延迟获取 IJournalGenerationService）</param>
    public ReceivableGenerationService(
        IUnitOfWork uow,
        IBillingDomainService billingDomain,
        IDbConnectionFactory db,
        ISqlLoader sql,
        IServiceProvider serviceProvider)
    {
        _uow = uow;
        _billingDomain = billingDomain;
        _db = db;
        _sql = sql;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 为指定合同在指定账期范围内生成应收计划（去重：同一合同+账期+费用类型只生成一次）
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="periodFrom">起始账期 (yyyy-MM)，null 表示合同起租月</param>
    /// <param name="periodTo">截止账期 (yyyy-MM)，null 表示合同结束月</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>本次新生成的应收计划数量</returns>
    /// <exception cref="InvalidOperationException">合同不存在或非生效中时抛出</exception>
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

    /// <summary>
    /// 合同激活时初始化生成所有应收（含财务日记账）
    /// 从合同起租月补全到当前月的应收计划 + 凭证，一次性费用另行生成 JE
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>初始化结果，包含生成的应收数量和处理的账期列表</returns>
    public async Task<ActivationInitResult> GenerateForActivationAsync(Guid contractId, CancellationToken ct)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct)
            ?? throw new InvalidOperationException($"合同 {contractId} 不存在");

        if (contract.Status != "Active")
            throw new InvalidOperationException($"合同状态为 {contract.Status}，非生效中无法初始化应收");

        var result = new ActivationInitResult();
        var today = ChinaTime.Now;
        var currentPeriod = $"{today.Year:D4}-{today.Month:D2}";
        var startPeriod = contract.StartDate.ToString("yyyy-MM");

        if (string.Compare(startPeriod, currentPeriod, StringComparison.Ordinal) > 0)
            return result; // 未来合同，不生成

        // 加载会计科目
        using var conn = _db.CreateConnection();
        conn.Open();
        var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes")))
            .ToDictionary(r => r.Code, r => r.Id);
        var receivableId = subjects.GetValueOrDefault("1122", Guid.Empty);
        var revenueId = subjects.GetValueOrDefault("6001", subjects.GetValueOrDefault("6051", Guid.Empty));

        // 处理 OneTime 费用（押金等）
        var feeConfigs = await conn.QueryAsync<dynamic>(
            _sql.Get("Lease.Select.ContractFeeConfig.WithFeeCodeByContract"),
            new { ContractId = contractId });
        foreach (var fc in feeConfigs)
        {
            string chargeType = fc.ChargeType ?? "Recurring";
            if (chargeType == "OneTime")
            {
                try
                {
                    var journalGen = _serviceProvider.GetRequiredService<IJournalGenerationService>();
                    await journalGen.GenerateOneTimeAsync(contractId, fc.Id, ct);
                    result.OneTimeFeeGenerated = true;
                }
                catch
                {
                    // 单个 JE 生成失败不影响激活主流程，可后续手动重试
                    // GenerateOneTimeAsync 已具备幂等性，重试调用不会产生重复 JE
                }
            }
        }

        // 从起租月到当前月逐月生成
        var current = startPeriod;
        while (string.Compare(current, currentPeriod, StringComparison.Ordinal) <= 0)
        {
            // 去重：该月是否已有应收
            var exists = await conn.QuerySingleAsync<int>(
                _sql.Get("Billing.Select.ReceivablePlan.ExistsByContractAndPeriod"),
                new { ContractId = contractId, Period = current });
            if (exists > 0) { current = NextPeriod(current); continue; }

            var lastDay = DateTime.DaysInMonth(
                int.Parse(current[..4]), int.Parse(current[5..7]));
            var dueDay = contract.EndDate != null ? Math.Min(contract.EndDate.Value.Day, lastDay) : lastDay;
            var dueDate = new DateOnly(
                int.Parse(current[..4]), int.Parse(current[5..7]), dueDay);
            var periodStart = $"{current}-01";
            var periodEnd = $"{current}-{lastDay:D2}";

            var activeFees = (await conn.QueryAsync<(Guid FeeCodeId, decimal Amount, string? EffDate, string? ExpDate, string FeeName)>(
                _sql.Get("Lease.Select.FeeConfig.AllForPeriod"),
                new { ContractId = contractId, PeriodStart = periodStart, PeriodEnd = periodEnd })).ToList();

            if (activeFees.Count == 0) { current = NextPeriod(current); continue; }

            var plans = _billingDomain.GenerateProratedReceivablePlans(
                activeFees.Select(f => (f.FeeCodeId, f.Amount, f.EffDate, f.ExpDate, f.FeeName)).ToList(),
                contractId, current, dueDate);

            foreach (var plan in plans)
            {
                var planId = Guid.NewGuid();
                await conn.ExecuteAsync(
                    _sql.Get("Billing.Insert.ReceivablePlan.Default"),
                    new { Id = planId, CId = contractId, FId = plan.FeeCodeId,
                        P = current, Amt = plan.Amount, Due = dueDate, CBy = Guid.Empty });

                // Voucher + JE (Type = "ContractActivation")
                var voucherId = Guid.NewGuid();
                await conn.ExecuteAsync(
                    _sql.Get("Accounting.Insert.Voucher.BillJob"),
                    new { Id = voucherId, No = $"ACT-{current}-{Guid.NewGuid():N}"[..32],
                        Date = DateOnly.FromDateTime(today), Desc = $"合同激活初始化应收 {current}",
                        SrcId = contractId, Type = "ContractActivation", CId = contractId, Period = current, CBy = Guid.Empty });
                await conn.ExecuteAsync(
                    _sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = voucherId, SId = receivableId,
                        Dir = "Debit", Amt = plan.Amount, Sum = $"合同激活 {current} 应收", CBy = Guid.Empty });
                await conn.ExecuteAsync(
                    _sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = voucherId, SId = revenueId,
                        Dir = "Credit", Amt = plan.Amount, Sum = $"合同激活 {current} 收入", CBy = Guid.Empty });

                result.ReceivablePlansCreated++;
                result.JournalEntriesCreated += 2;
            }
            result.PeriodsProcessed.Add(current);
            current = NextPeriod(current);
        }

        result.Message = $"已生成 {result.PeriodsProcessed.Count} 个月应收";
        return result;
    }

    /// <summary>
    /// 切换到下一个月份
    /// </summary>
    private static string NextPeriod(string period)
    {
        var parts = period.Split('-');
        var year = int.Parse(parts[0]);
        var month = int.Parse(parts[1]);
        if (month == 12) return $"{year + 1}-01";
        return $"{year}-{month + 1:D2}";
    }

    /// <summary>
    /// 按付款周期拆分合同有效期内的所有应收月份
    /// </summary>
    public List<string> SplitPeriods(ContractEntity contract)
    {
        var start = contract.StartDate;
        var periods = new List<string>();

        var current = new Period(start.Year, start.Month);
        if (contract.EndDate == null) { periods.Add(current.ToString()); return periods; }
        var endPeriod = new Period(contract.EndDate.Value.Year, contract.EndDate.Value.Month);

        while (current.Year < endPeriod.Year || (current.Year == endPeriod.Year && current.Month <= endPeriod.Month))
        {
            periods.Add(current.ToString());
            current = current.Next();
        }

        return periods;
    }

    /// <summary>
    /// 计算指定账期的到期日 — 取合同到期日与当月最后一天的较小值
    /// </summary>
    public DateOnly CalculateDueDate(string periodStr, ContractEntity contract)
    {
        var period = Period.Parse(periodStr);
        var lastDay = DateTime.DaysInMonth(period.Year, period.Month);
        var dueDay = contract.EndDate != null ? Math.Min(contract.EndDate.Value.Day, lastDay) : lastDay;
        return new DateOnly(period.Year, period.Month, dueDay);
    }
}
