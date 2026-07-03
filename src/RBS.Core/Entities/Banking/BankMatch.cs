using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Banking;

/// <summary>
/// 匹配记录 — 银行流水与内部收款/付款的关联
/// </summary>
public class BankMatch : AuditableEntity
{
    public Guid BankStatementId { get; private set; }
    /// <summary>内部单据 ID（Receipt / Payment 的 Id）</summary>
    public Guid InternalDocumentId { get; private set; }
    /// <summary>单据类型 Receipt / Payment</summary>
    public string DocumentType { get; private set; } = "Receipt";
    public decimal MatchedAmount { get; private set; }
    /// <summary>Auto / Manual</summary>
    public string MatchMethod { get; private set; } = "Manual";

    private BankMatch() { }

    public BankMatch(Guid bankStatementId, Guid internalDocumentId, string documentType,
        decimal matchedAmount, string matchMethod)
    {
        BankStatementId = bankStatementId;
        InternalDocumentId = internalDocumentId;
        DocumentType = documentType;
        MatchedAmount = matchedAmount;
        MatchMethod = matchMethod;
    }
}
