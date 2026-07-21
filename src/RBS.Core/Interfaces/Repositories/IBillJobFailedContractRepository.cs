namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Scheduling;

/// <summary>
/// BillJob 失败合同仓储接口
/// 记录和查询出账任务中各合同的失败明细，支持重试标记
/// </summary>
public interface IBillJobFailedContractRepository
{
    /// <summary>批量插入失败合同记录</summary>
    Task CreateBatchAsync(IEnumerable<BillJobFailedContract> contracts, CancellationToken ct = default);

    /// <summary>根据 TaskLogId 获取该次任务的所有失败合同</summary>
    Task<List<BillJobFailedContract>> GetByTaskLogIdAsync(Guid taskLogId, CancellationToken ct = default);

    /// <summary>标记指定失败记录为重试成功</summary>
    Task MarkRetriedAsync(long id, CancellationToken ct = default);

    /// <summary>批量标记重试成功（按 TaskLogId + ContractId）</summary>
    Task MarkRetriedBatchAsync(Guid taskLogId, IEnumerable<Guid> contractIds, CancellationToken ct = default);
}
