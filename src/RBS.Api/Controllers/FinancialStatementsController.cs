using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Common;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

/// <summary>
/// 财务报表 — 资产负债表、利润表
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialStatementsController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly IUnitOfWork _uow;

    public FinancialStatementsController(IDbConnectionFactory db, IUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    /// <summary>
    /// 资产负债表 — 按科目类别汇总期末余额
    /// 科目编码规则：1=资产 2=负债 4=权益
    /// </summary>
    [HttpGet("balancesheet")]
    public async Task<IActionResult> BalanceSheet([FromQuery] DateOnly? endDate, CancellationToken ct)
    {
        var end = endDate ?? DateOnly.FromDateTime(ChinaTime.Now);
        using var conn = _db.CreateConnection(); conn.Open();

        // 取所有叶子科目
        var subjects = await _uow.AccountingSubjects.GetAllAsync(ct);
        var bsSubjects = subjects.Where(s => s.IsActive && s.IsLeaf).ToList();

        // 汇总分录
        var entries = await conn.QueryAsync<dynamic>(@"
            SELECT j.AccountingSubjectId,
                   SUM(CASE WHEN j.Direction='Debit' THEN j.Amount ELSE 0 END) AS TotalDebit,
                   SUM(CASE WHEN j.Direction='Credit' THEN j.Amount ELSE 0 END) AS TotalCredit
            FROM JournalEntries j
            INNER JOIN Vouchers v ON v.Id = j.VoucherId
            WHERE v.VoucherDate <= @EndDate
            GROUP BY j.AccountingSubjectId",
            new { EndDate = end });

        var entryDict = entries.ToDictionary<dynamic, Guid, (decimal Debit, decimal Credit)>(
            e => (Guid)e.AccountingSubjectId,
            e => (Debit: (decimal)e.TotalDebit, Credit: (decimal)e.TotalCredit));

        // 按编码前缀归类：1=资产 2=负债 4=权益
        var categoryMap = new Dictionary<string, string>
        {
            ["1"] = "资产",
            ["2"] = "负债",
            ["4"] = "所有者权益"
        };

        // 子类划分
        var subCategoryMap = new Dictionary<Func<string, bool>, string>
        {
            [code => code.StartsWith("10") || code.StartsWith("11") || code.StartsWith("12") || code.StartsWith("14")] = "流动资产",
            [code => code.StartsWith("15") || code.StartsWith("16") || code.StartsWith("17") || code.StartsWith("18") || code.StartsWith("19")] = "非流动资产",
            [code => code.StartsWith("20") || code.StartsWith("21") || code.StartsWith("22")] = "流动负债",
            [code => code.StartsWith("23") || code.StartsWith("24") || code.StartsWith("25") || code.StartsWith("26") || code.StartsWith("27") || code.StartsWith("28") || code.StartsWith("29")] = "非流动负债",
            [code => code.StartsWith("4")] = "所有者权益",
        };

        var items = new List<object>();
        foreach (var s in bsSubjects)
        {
            entryDict.TryGetValue(s.Id, out var totals);
            var balance = s.Direction == "Debit"
                ? totals.Debit - totals.Credit
                : totals.Credit - totals.Debit;

            var prefix = s.Code.Length > 0 ? s.Code[..1] : "";
            if (!categoryMap.ContainsKey(prefix)) continue; // 非资产负债表科目

            var subCat = subCategoryMap.FirstOrDefault(kv => kv.Key(s.Code)).Value ?? "其他";
            items.Add(new
            {
                s.Code, s.Name, Category = categoryMap[prefix], SubCategory = subCat,
                DebitAmount = totals.Debit, CreditAmount = totals.Credit,
                Balance = balance, s.Direction
            });
        }

        var grouped = items
            .GroupBy(i => ((dynamic)i).Category)
            .ToDictionary(g => g.Key, g => g
                .GroupBy(i => ((dynamic)i).SubCategory)
                .ToDictionary(sg => sg.Key, sg => sg.ToList()));

        return Ok(new { endDate = end, categories = grouped });
    }

    /// <summary>
    /// 利润表 — 按损益科目汇总发生额
    /// 科目编码规则：6=损益
    /// </summary>
    [HttpGet("incomestatement")]
    public async Task<IActionResult> IncomeStatement(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken ct)
    {
        var end = endDate ?? DateOnly.FromDateTime(ChinaTime.Now);
        var start = startDate ?? new DateOnly(end.Year, end.Month, 1); // 默认当月

        using var conn = _db.CreateConnection(); conn.Open();

        // 取 6xxx 损益类叶子科目
        var subjects = await _uow.AccountingSubjects.GetAllAsync(ct);
        var plSubjects = subjects.Where(s => s.IsActive && s.IsLeaf && s.Code.StartsWith('6')).ToList();

        var entries = await conn.QueryAsync<dynamic>(@"
            SELECT j.AccountingSubjectId,
                   SUM(CASE WHEN j.Direction='Debit' THEN j.Amount ELSE 0 END) AS TotalDebit,
                   SUM(CASE WHEN j.Direction='Credit' THEN j.Amount ELSE 0 END) AS TotalCredit
            FROM JournalEntries j
            INNER JOIN Vouchers v ON v.Id = j.VoucherId
            WHERE v.VoucherDate >= @StartDate AND v.VoucherDate <= @EndDate
            GROUP BY j.AccountingSubjectId",
            new { StartDate = start, EndDate = end });

        var entryDict = entries.ToDictionary<dynamic, Guid, (decimal Debit, decimal Credit)>(
            e => (Guid)e.AccountingSubjectId,
            e => (Debit: (decimal)e.TotalDebit, Credit: (decimal)e.TotalCredit));

        // 损益类：收入(6xxx) 贷方-借方=发生额；费用(6xxx) 借方-贷方=发生额
        // 收入: 6001, 6051, 6061... (Credit balance normally)
        // 费用: 6401, 6405, 6601, 6602, 6603, 6711... (Debit balance normally)
        var revenueCodes = new[] { "6001", "6041", "6051", "6061", "6071", "6081", "6091", "6101", "6111", "6301" };
        var items = plSubjects.Select(s =>
        {
            entryDict.TryGetValue(s.Id, out var totals);
            var isRevenue = revenueCodes.Any(p => s.Code.StartsWith(p)) || s.Direction == "Credit";
            var amount = isRevenue ? totals.Credit - totals.Debit : totals.Debit - totals.Credit;
            return new
            {
                s.Code, s.Name,
                DebitAmount = totals.Debit,
                CreditAmount = totals.Credit,
                Amount = Math.Max(amount, 0),
                IsRevenue = isRevenue
            };
        }).Where(i => i.Amount != 0).OrderBy(i => i.Code).ToList();

        var revenue = items.Where(i => i.IsRevenue).ToList();
        var expenses = items.Where(i => !i.IsRevenue).ToList();

        return Ok(new
        {
            startDate = start,
            endDate = end,
            revenue = revenue,
            expenses = expenses,
            totalRevenue = revenue.Sum(r => r.Amount),
            totalExpenses = expenses.Sum(e => e.Amount),
            netIncome = revenue.Sum(r => r.Amount) - expenses.Sum(e => e.Amount)
        });
    }
}
