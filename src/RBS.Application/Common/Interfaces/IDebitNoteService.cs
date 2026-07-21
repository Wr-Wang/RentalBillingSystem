using RBS.Core.Entities.Billing;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 欠款通知单（账单）服务
/// </summary>
public interface IDebitNoteService
{
    /// <summary>查询指定公司的账单列表</summary>
    Task<List<object>> GetByCompanyAsync(Guid companyId, string? period = null, Guid? contractId = null, string? keyword = null, string? status = null, CancellationToken ct = default);

    /// <summary>查询指定合同的账单</summary>
    Task<List<object>> GetByContractAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>根据 ID 获取账单（含明细）</summary>
    Task<dynamic?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>从应收计划生成账单快照</summary>
    Task<DebitNote> GenerateAsync(Guid contractId, string period, CancellationToken ct = default);

    /// <summary>导出账单 PDF</summary>
    Task<byte[]> ExportPdfAsync(Guid id, CancellationToken ct = default);

    /// <summary>作废账单</summary>
    Task CancelAsync(Guid id, string reason, Guid cancelledBy, CancellationToken ct = default);

    /// <summary>删除账单（硬删，用于重新生成）</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
