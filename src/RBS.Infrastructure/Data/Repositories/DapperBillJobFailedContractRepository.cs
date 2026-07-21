using Dapper;
using RBS.Core.Entities.Scheduling;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

/// <summary>
/// Dapper BillJob 失败合同仓储实现
/// 记录出账任务中各合同的失败明细，支持重试标记
/// </summary>
public class DapperBillJobFailedContractRepository : IBillJobFailedContractRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public DapperBillJobFailedContractRepository(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    public async Task CreateBatchAsync(IEnumerable<BillJobFailedContract> contracts, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        foreach (var c in contracts)
        {
            await conn.ExecuteAsync(
                _sql.Get("Scheduling.Insert.BillJobFailedContract.Batch"), c);
        }
    }

    public async Task<List<BillJobFailedContract>> GetByTaskLogIdAsync(Guid taskLogId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<BillJobFailedContract>(
            _sql.Get("Scheduling.Select.BillJobFailedContract.ByTaskLogId"),
            new { TaskLogId = taskLogId });
        return rows.ToList();
    }

    public async Task MarkRetriedAsync(long id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(
            _sql.Get("Scheduling.Update.BillJobFailedContract.MarkRetried"),
            new { Id = id });
    }

    public async Task MarkRetriedBatchAsync(Guid taskLogId, IEnumerable<Guid> contractIds, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync(
            _sql.Get("Scheduling.Update.BillJobFailedContract.MarkRetriedBatch"),
            new { TaskLogId = taskLogId, ContractIds = contractIds });
    }
}
