using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Banking;

/// <summary>
/// 银行对账会话 — 一个对账周期
/// </summary>
public class BankReconciliation : AuditableEntity, IHasCompany
{
    public Guid CompanyId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    /// <summary>InProgress / Completed / Cancelled</summary>
    public string Status { get; private set; } = "InProgress";
    public decimal OpeningBalance { get; private set; }
    public decimal ClosingBalance { get; private set; }
    public decimal StatementTotal { get; private set; }
    public decimal SystemTotal { get; private set; }
    public decimal Difference => StatementTotal - SystemTotal;
    public DateTime? CompletedAt { get; private set; }

    private BankReconciliation() { }

    public BankReconciliation(Guid companyId, DateOnly startDate, DateOnly endDate,
        decimal openingBalance, decimal closingBalance)
    {
        CompanyId = companyId;
        StartDate = startDate;
        EndDate = endDate;
        OpeningBalance = openingBalance;
        ClosingBalance = closingBalance;
    }

    public void SetTotals(decimal statementTotal, decimal systemTotal)
    {
        StatementTotal = statementTotal;
        SystemTotal = systemTotal;
    }

    public void Complete()
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = "Cancelled";
    }
}
