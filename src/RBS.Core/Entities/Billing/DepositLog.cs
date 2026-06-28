namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class DepositLog : AuditableEntity
{
    public Guid ContractId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Balance { get; private set; }
    public string Action { get; private set; } = "Create";
    public string? Remark { get; private set; }
    private DepositLog() { }
    public DepositLog(Guid contractId, decimal amount)
    { ContractId = contractId; Amount = amount; Balance = amount; Action = "Create"; }
}
