using RBS.Core.Entities.Banking;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 银行对账服务接口 — 提供银行流水导入、自动/手动匹配、对账确认等能力
/// </summary>
public interface IBankingService
{
    /// <summary>导入银行流水</summary>
    Task<int> ImportStatementsAsync(Guid companyId, List<BankStatement> statements, CancellationToken ct = default);

    /// <summary>自动匹配 — 银行流水与已确认收款按金额+时间段匹配</summary>
    Task<List<BankMatch>> AutoMatchAsync(Guid reconciliationId, CancellationToken ct = default);

    /// <summary>手动匹配</summary>
    Task<BankMatch> ManualMatchAsync(Guid statementId, Guid receiptId, decimal amount, CancellationToken ct = default);

    /// <summary>确认对账</summary>
    Task CompleteReconciliationAsync(Guid reconciliationId, CancellationToken ct = default);
}
