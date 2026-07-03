using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrialBalanceController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly IUnitOfWork _uow;

    public TrialBalanceController(IDbConnectionFactory db, IUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly? endDate, CancellationToken ct)
    {
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        using var conn = _db.CreateConnection(); conn.Open();

        // 取所有科目
        var subjects = await _uow.AccountingSubjects.GetAllAsync(ct);
        var activeSubjects = subjects.Where(s => s.IsActive).ToList();

        // 取截止日期前的所有分录汇总
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

        var result = activeSubjects.Select(s =>
        {
            entryDict.TryGetValue(s.Id, out var totals);
            var balance = s.Direction == "Debit"
                ? totals.Debit - totals.Credit
                : totals.Credit - totals.Debit;
            return new
            {
                s.Code, s.Name, s.Direction, s.Level,
                DebitAmount = totals.Debit,
                CreditAmount = totals.Credit,
                Balance = balance
            };
        }).OrderBy(r => r.Code).ToList();

        return Ok(new
        {
            endDate = end,
            subjects = result,
            totalDebit = result.Sum(r => r.DebitAmount),
            totalCredit = result.Sum(r => r.CreditAmount)
        });
    }
}
