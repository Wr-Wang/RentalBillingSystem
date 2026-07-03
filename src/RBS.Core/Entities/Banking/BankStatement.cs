using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Banking;

/// <summary>
/// 银行流水行 — 从银行导入的每笔交易
/// </summary>
public class BankStatement : AuditableEntity, IHasCompany
{
    public Guid CompanyId { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    /// <summary>正=收入，负=支出</summary>
    public decimal Amount { get; private set; }
    public decimal Balance { get; private set; }
    public string? Description { get; private set; }
    /// <summary>银行流水号</summary>
    public string? ReferenceNo { get; private set; }
    /// <summary>对方账户</summary>
    public string? Counterparty { get; private set; }
    /// <summary>Unmatched / Matched / Reconciled</summary>
    public string Status { get; private set; } = "Unmatched";
    public Guid? ImportBatchId { get; private set; }

    private BankStatement() { }

    public BankStatement(Guid companyId, DateOnly transactionDate, decimal amount, decimal balance)
    {
        CompanyId = companyId;
        TransactionDate = transactionDate;
        Amount = amount;
        Balance = balance;
    }

    public void SetReference(string? refNo, string? description, string? counterparty)
    {
        ReferenceNo = refNo;
        Description = description;
        Counterparty = counterparty;
    }

    public void MarkMatched() => Status = "Matched";
    public void MarkReconciled() => Status = "Reconciled";
}
