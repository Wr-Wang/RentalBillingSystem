using System.Data;
using Microsoft.Data.SqlClient;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 批量插入器实现 — 基于 SqlBulkCopy 的高速写入
/// </summary>
public class BulkInserter : IBulkInserter
{
    /// <summary>
    /// 单批行数。SqlBulkCopy 按批发送：未使用外部事务时，每批是一个独立的内部事务，
    /// 因此该值同时决定“网络往返次数 / 服务端内存 / 失败回滚粒度”三者。
    /// 5000 是常用折中：批次过大则单批耗时长、失败回滚多；过小则往返与日志开销变高。
    /// </summary>
    private const int DefaultBatchSize = 5000;

    private readonly IDbConnectionFactory _db;

    public BulkInserter(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task BulkInsertAsync(string tableName, DataTable dataTable, CancellationToken ct = default, Action<long>? onRowsCopied = null, int bulkCopyTimeoutSeconds = 600)
    {
        if (dataTable.Rows.Count == 0) return;

        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();

        using var bulk = new SqlBulkCopy(conn)
        {
            DestinationTableName = tableName,
            BatchSize = Math.Min(dataTable.Rows.Count, DefaultBatchSize),
            EnableStreaming = false,
            // SqlBulkCopy 默认仅 30s，几十万行必然超时（Error -2）；0 表示不限
            BulkCopyTimeout = bulkCopyTimeoutSeconds
        };

        foreach (DataColumn col in dataTable.Columns)
            bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

        if (onRowsCopied != null)
        {
            // 回调粒度不粗于一个批次（最多 5000 行一次），保证进度条随批次推进；
            // 小表按约 1% 回调，但不少于 500 行，避免过于频繁。
            bulk.NotifyAfter = Math.Clamp(dataTable.Rows.Count / 100, 500, DefaultBatchSize);
            bulk.SqlRowsCopied += (_, e) => onRowsCopied(e.RowsCopied);
        }

        await bulk.WriteToServerAsync(dataTable, ct);
    }
}
