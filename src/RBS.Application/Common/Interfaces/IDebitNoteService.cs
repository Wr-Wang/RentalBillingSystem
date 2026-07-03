using RBS.Core.Entities.Billing;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 欠款通知单（账单）服务
/// </summary>
public interface IDebitNoteService
{
    /// <summary>查询指定合同的账单</summary>
    Task<List<DebitNote>> GetByContractAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>根据 ID 获取账单（含明细）</summary>
    Task<DebitNote?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>从应收计划生成账单快照</summary>
    Task<DebitNote> GenerateAsync(Guid contractId, string period, CancellationToken ct = default);

    /// <summary>导出账单 PDF</summary>
    Task<byte[]> ExportPdfAsync(Guid id, CancellationToken ct = default);
}
