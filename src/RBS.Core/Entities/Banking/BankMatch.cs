using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Banking;

/// <summary>
/// 匹配记录 — 银行流水与内部收款/付款单据的关联
/// 将银行流水（BankStatement）与企业内部收付款单据进行配对，
/// 确认每笔银行交易对应的业务单据，是银行对账的核心关联实体。
/// </summary>
public class BankMatch : AuditableEntity
{
    /// <summary>
    /// 银行流水标识
    /// 关联 BankStatement，指向被匹配的银行交易记录
    /// </summary>
    public Guid BankStatementId { get; private set; }

    /// <summary>
    /// 内部单据标识
    /// 关联内部单据（Receipt 收款单 或 Payment 付款单）的 Id
    /// </summary>
    public Guid InternalDocumentId { get; private set; }

    /// <summary>
    /// 内部单据类型
    /// Receipt（收款单）— 表示银行流水对应一笔收款；
    /// Payment（付款单）— 表示银行流水对应一笔付款
    /// </summary>
    public string DocumentType { get; private set; } = "Receipt";

    /// <summary>
    /// 匹配金额
    /// 银行流水与内部单据实际匹配的金额，可能等于或小于单据金额（部分匹配）
    /// </summary>
    public decimal MatchedAmount { get; private set; }

    /// <summary>
    /// 匹配方式
    /// Auto（自动匹配）— 系统根据规则自动完成匹配；
    /// Manual（手动匹配）— 由人工操作完成匹配
    /// </summary>
    public string MatchMethod { get; private set; } = "Manual";

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private BankMatch() { }

    /// <summary>
    /// 创建银行匹配记录实例
    /// </summary>
    /// <param name="bankStatementId">银行流水标识</param>
    /// <param name="internalDocumentId">内部单据标识</param>
    /// <param name="documentType">内部单据类型（Receipt / Payment）</param>
    /// <param name="matchedAmount">匹配金额</param>
    /// <param name="matchMethod">匹配方式（Auto / Manual）</param>
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
