using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Banking;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 银行对账服务 — 流水导入/自动匹配/对账确认
/// </summary>
public class BankingService : IBankingService
{
    private readonly IUnitOfWork _uow;
    public BankingService(IUnitOfWork uow) => _uow = uow;

    public async Task<int> ImportStatementsAsync(Guid companyId, List<BankStatement> statements, CancellationToken ct)
    {
        int count = 0;
        foreach (var stmt in statements)
        {
            // 去重：同公司+同日+同金额+同余额视为重复
            var existing = await _uow.BankStatements.GetAllAsync(ct);
            var dup = existing.Any(s =>
                s.CompanyId == companyId
                && s.TransactionDate == stmt.TransactionDate
                && s.Amount == stmt.Amount
                && s.Balance == stmt.Balance);
            if (dup) continue;

            await _uow.BankStatements.AddAsync(stmt, ct);
            count++;
        }
        await _uow.CommitAsync(ct);
        return count;
    }

    public async Task<List<BankMatch>> AutoMatchAsync(Guid reconciliationId, CancellationToken ct)
    {
        var recon = await _uow.BankReconciliations.GetByIdAsync(reconciliationId, ct)
            ?? throw new InvalidOperationException("对账会话不存在");

        var statements = await _uow.BankStatements.GetAllAsync(ct);
        var unmatched = statements
            .Where(s => s.CompanyId == recon.CompanyId
                     && s.Status == "Unmatched"
                     && s.TransactionDate >= recon.StartDate
                     && s.TransactionDate <= recon.EndDate)
            .ToList();

        var receipts = await _uow.Receipts.GetAllAsync(ct);
        var confirmedReceipts = receipts
            .Where(r => r.Status == "Confirmed"
                     && r.ReceivedDate >= recon.StartDate
                     && r.ReceivedDate <= recon.EndDate)
            .ToList();

        var matches = new List<BankMatch>();
        foreach (var stmt in unmatched)
        {
            // 按金额匹配（绝对值相等）
            var match = confirmedReceipts.FirstOrDefault(r =>
                Math.Abs(r.Amount) == Math.Abs(stmt.Amount)
                && Math.Abs(r.ReceivedDate.DayNumber - stmt.TransactionDate.DayNumber) <= 3);

            if (match == null) continue;

            var bankMatch = new BankMatch(stmt.Id, match.Id, "Receipt",
                Math.Min(stmt.Amount, match.Amount), "Auto");
            await _uow.BankMatches.AddAsync(bankMatch, ct);
            stmt.MarkMatched();
            matches.Add(bankMatch);
        }

        await _uow.CommitAsync(ct);
        return matches;
    }

    public async Task<BankMatch> ManualMatchAsync(Guid statementId, Guid receiptId, decimal amount, CancellationToken ct)
    {
        var stmt = await _uow.BankStatements.GetByIdAsync(statementId, ct)
            ?? throw new InvalidOperationException("银行流水不存在");

        var match = new BankMatch(statementId, receiptId, "Receipt", amount, "Manual");
        await _uow.BankMatches.AddAsync(match, ct);
        stmt.MarkMatched();
        await _uow.CommitAsync(ct);
        return match;
    }

    public async Task CompleteReconciliationAsync(Guid reconciliationId, CancellationToken ct)
    {
        var recon = await _uow.BankReconciliations.GetByIdAsync(reconciliationId, ct)
            ?? throw new InvalidOperationException("对账会话不存在");

        // 统计银行流水总额
        var statements = await _uow.BankStatements.GetAllAsync(ct);
        var periodStmts = statements
            .Where(s => s.CompanyId == recon.CompanyId
                     && s.TransactionDate >= recon.StartDate
                     && s.TransactionDate <= recon.EndDate)
            .ToList();
        var stmtTotal = periodStmts.Sum(s => s.Amount);

        // 统计系统收款总额
        var receipts = await _uow.Receipts.GetAllAsync(ct);
        var periodReceipts = receipts
            .Where(r => r.CompanyId == recon.CompanyId
                     && r.ReceivedDate >= recon.StartDate
                     && r.ReceivedDate <= recon.EndDate
                     && r.Status == "Confirmed")
            .ToList();
        var sysTotal = periodReceipts.Sum(r => r.Amount);

        recon.SetTotals(stmtTotal, sysTotal);
        recon.Complete();

        // 标记已匹配的流水为已对账
        var matches = await _uow.BankMatches.GetAllAsync(ct);
        var matchedStmtIds = matches.Select(m => m.BankStatementId).ToHashSet();
        foreach (var stmt in periodStmts.Where(s => matchedStmtIds.Contains(s.Id)))
        {
            stmt.MarkReconciled();
        }

        await _uow.CommitAsync(ct);
    }
}
