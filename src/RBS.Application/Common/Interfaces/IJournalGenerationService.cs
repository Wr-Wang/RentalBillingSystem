using System.Data;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 日记账生成服务 — 负责按 FeeConfig 预生成 Voucher + JournalEntry
/// </summary>
public interface IJournalGenerationService
{
    /// <summary>生成 OneTime 费用的 JE（押金等，合同签署时调用）</summary>
    Task GenerateOneTimeAsync(Guid contractId, Guid feeConfigId, CancellationToken ct);

    /// <summary>BillJob：生成下一个月的 JE</summary>
    Task GenerateNextMonthAsync(Guid companyId, string targetMonth, CancellationToken ct);

    /// <summary>费用调价后：生成补差 Supplementary JE（账单已出时调用，独立连接）</summary>
    Task GenerateSupplementaryAsync(Guid contractId, Guid feeCodeId, decimal newAmount, decimal oldAmount, string effectiveDate, string period, CancellationToken ct);

    /// <summary>费用调价后：生成补差 Supplementary JE（事务内重载，与调用方共享连接）</summary>
    Task GenerateSupplementaryAsync(IDbConnection conn, IDbTransaction tx, Guid contractId, Guid feeCodeId, decimal newAmount, decimal oldAmount, string effectiveDate, string period, CancellationToken ct);
}
