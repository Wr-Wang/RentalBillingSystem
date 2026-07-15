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
    private readonly IDbConnectionFactory _db;

    public BulkInserter(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task BulkInsertAsync(string tableName, DataTable dataTable, CancellationToken ct = default)
    {
        if (dataTable.Rows.Count == 0) return;

        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();

        using var bulk = new SqlBulkCopy(conn)
        {
            DestinationTableName = tableName,
            BatchSize = Math.Min(dataTable.Rows.Count, 5000),
            EnableStreaming = false
        };

        foreach (DataColumn col in dataTable.Columns)
            bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

        await bulk.WriteToServerAsync(dataTable, ct);
    }
}
