using System.Data;
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
/// 应收生成编排服务实现 — 批量按合同生成 Journal，去重后写入
/// 合同激活时同步生成 Journal 及 GL 更新
/// 依赖 IBillingDomainService 领域服务计算按月分摊的应收
/// </summary>
public class ReceivableGenerationService : IReceivableGenerationService
{
    private readonly IUnitOfWork _uow;
    private readonly IBillingDomainService _billingDomain;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBulkInserter _bulk;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ReceivableGenerationService(
        IUnitOfWork uow,
        IBillingDomainService billingDomain,
        IDbConnectionFactory db,
        ISqlLoader sql,
        IServiceProvider serviceProvider,
        IBulkInserter bulk)
    {
        _uow = uow;
        _billingDomain = billingDomain;
        _db = db;
        _sql = sql;
        _serviceProvider = serviceProvider;
        _bulk = bulk;
    }

    /// <summary>
    /// 为指定合同在指定账期范围内生成 Journal（去重：同一合同+账期+费用类型只生成一次）
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="periodFrom">起始账期 (yyyy-MM)，null 表示合同起租月</param>
    /// <param name="periodTo">截止账期 (yyyy-MM)，null 表示合同结束月</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>本次新生成的 Journal 数量</returns>
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

        using var conn = _db.CreateConnection();
        conn.Open();

        // 读取费用配置（Dapper 泛型 GetByIdAsync 不含 FeeConfigs 导航属性）
        var feeConfigRows = (await conn.QueryAsync(
            _sql.Get("Lease.Select.ContractFeeConfig.WithFeeCodeByContract"),
            new { ContractId = contractId })).ToList();

        var feeConfigs = feeConfigRows.Select(f => (
            FeeCodeId: (Guid)f.FeeCodeId,
            Amount: (decimal)f.Amount,
            EffectiveDate: (string?)f.EffectiveDate,
            ExpiryDate: (string?)f.ExpiryDate
        )).ToList();

        foreach (var period in matched)
        {
            var dueDate = CalculateDueDate(period, contract);

            // 使用按天分摊生成 Journal，兼容拆分后的多条月度配置
            var feeConfigsWithName = feeConfigs.Select(f => (
                f.FeeCodeId, f.Amount, f.EffectiveDate, f.ExpiryDate,
                FeeName: (string?)null ?? "")).ToList();
            var journals = _billingDomain.GenerateProratedJournals(feeConfigsWithName,
                contract.Id, period, dueDate, contract.CompanyId, Guid.Empty, ChinaTime.Now);

            foreach (var journal in journals)
            {
                // 去重检查
                var exists = await conn.QuerySingleAsync<int>(
                    _sql.Get("Billing.Select.Journal.ExistsByKey"),
                    new { C = contractId, F = journal.FeeCodeId, P = period });

                if (exists > 0)
                    continue; // 已存在则跳过

                await _uow.Journals.AddAsync(journal, ct);
                totalCreated++;
            }
        }

        await _uow.CommitAsync(ct);
        return totalCreated;
    }

    /// <summary>
    /// 合同激活时初始化生成所有应收（Journal + GL）
    /// 从合同起租月补全到当前月的 Journal，一次性费用另行生成
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>初始化结果，包含生成的 Journal 数量和处理的账期列表</returns>
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
                // 一次性费用：INSERT Journal(EntryType='Deposit') + Update GL
                var jId = Guid.NewGuid();
                var dueDate = contract.StartDate.AddDays(30);
                var depositSubjectId = subjects.GetValueOrDefault("1122", receivableId);
                await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                    new { Id = jId, CoId = contract.CompanyId, CId = contractId, FId = (Guid)fc.FeeCodeId,
                        FConfigId = (Guid)fc.Id, SubjId = depositSubjectId, Period = currentPeriod,
                        Amt = (decimal)fc.Amount, Due = dueDate, EntryType = "Deposit",
                        BilledAt = today, DNId = (Guid?)null, ParentId = (Guid?)null,
                        Summary = $"一次性 {(string)fc.Name}", CBy = Guid.Empty });
                result.OneTimeFeeGenerated = true;
            }
        }
        return result;
    }

    /// <summary>
    /// 创建 Journal DataTable（列匹配 Billing.Insert.Journal.Default）
    /// </summary>
    private static DataTable BuildJournalDataTable()
    {
        var dt = new DataTable("Journals");
        dt.Columns.Add("Id", typeof(Guid));
        dt.Columns.Add("CompanyId", typeof(Guid));
        dt.Columns.Add("ContractId", typeof(Guid));
        dt.Columns.Add("FeeCodeId", typeof(Guid));
        dt.Columns.Add("FeeConfigId", typeof(Guid));
        dt.Columns.Add("AccountingSubjectId", typeof(Guid));
        dt.Columns.Add("Period", typeof(string));
        dt.Columns.Add("Amount", typeof(decimal));
        dt.Columns.Add("DueDate", typeof(DateOnly));
        dt.Columns.Add("EntryType", typeof(string));
        dt.Columns.Add("BilledAt", typeof(DateTime));
        dt.Columns.Add("DebitNoteId", typeof(Guid));
        dt.Columns.Add("ParentJournalId", typeof(Guid));
        dt.Columns.Add("Summary", typeof(string));
        dt.Columns.Add("CreatedBy", typeof(Guid));
        return dt;
    }

    /// <summary>静态版 SplitPeriods，供控制器预览用</summary>
    public static List<string> SplitPeriodsStatic(ContractEntity contract)
    {
        var start = contract.StartDate;
        var periods = new List<string>();

        // 结束月份：有 EndDate 用 EndDate，null 表示长期合同，用当前月兜底
        DateOnly endDate;
        if (contract.EndDate != null)
            endDate = contract.EndDate.Value;
        else
            endDate = DateOnly.FromDateTime(ChinaTime.Now);

        var curYear = start.Year; var curMonth = start.Month;
        var endYear = endDate.Year; var endMonth = endDate.Month;
        while (curYear < endYear || (curYear == endYear && curMonth <= endMonth))
        {
            periods.Add($"{curYear}-{curMonth:D2}");
            curMonth++; if (curMonth > 12) { curYear++; curMonth = 1; }
        }
        return periods;
    }

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

        // 结束月份：有 EndDate 用 EndDate，null 表示长期合同，用当前月兜底
        Period endPeriod;
        if (contract.EndDate != null)
            endPeriod = new Period(contract.EndDate.Value.Year, contract.EndDate.Value.Month);
        else
        {
            var now = ChinaTime.Now;
            endPeriod = new Period(now.Year, now.Month);
        }

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

    /// <summary>获取前一账期的期末余额作为当前账期的期初余额</summary>
}
