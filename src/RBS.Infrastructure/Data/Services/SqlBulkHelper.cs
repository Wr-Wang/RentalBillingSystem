using System.Data;
using Microsoft.Data.SqlClient;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// SqlBulkCopy 批量写入工具 — 将实体列表批量写入 SQL Server 表
/// 适用场景：批量导入、批量生成，单次写入 ≥ 100 条时优势明显
/// </summary>
public static class SqlBulkHelper
{
    /// <summary>
    /// 批量写入实体列表到指定表（使用现有连接）
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="connection">已打开的 SqlConnection</param>
    /// <param name="tableName">目标表名</param>
    /// <param name="entities">实体列表</param>
    /// <param name="propertyMapping">自定义属性映射（可选）</param>
    /// <param name="transaction">外部事务（可选）</param>
    /// <param name="ct">取消令牌</param>
    public static async Task BulkInsertAsync<T>(
        SqlConnection connection,
        string tableName,
        IReadOnlyList<T> entities,
        Dictionary<string, string>? propertyMapping = null,
        SqlTransaction? transaction = null,
        CancellationToken ct = default)
    {
        if (entities.Count == 0) return;

        var dt = BuildDataTable(entities, propertyMapping);
        await BulkInsertAsync(connection, tableName, dt, transaction, ct);
    }

    /// <summary>
    /// 批量写入 DataTable 到指定表（适用自定义列映射场景）
    /// </summary>
    /// <param name="connection">已打开的 SqlConnection</param>
    /// <param name="tableName">目标表名</param>
    /// <param name="dataTable">已填充数据的 DataTable（列名需匹配目标表）</param>
    /// <param name="transaction">外部事务（可选）</param>
    /// <param name="ct">取消令牌</param>
    public static async Task BulkInsertAsync(
        SqlConnection connection,
        string tableName,
        DataTable dataTable,
        SqlTransaction? transaction = null,
        CancellationToken ct = default)
    {
        if (dataTable.Rows.Count == 0) return;

        using var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
        {
            DestinationTableName = tableName,
            BatchSize = Math.Min(dataTable.Rows.Count, 5000),
            EnableStreaming = false
        };

        foreach (DataColumn col in dataTable.Columns)
            bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

        await bulk.WriteToServerAsync(dataTable, ct);
    }

    /// <summary>
    /// 将实体列表转换为 DataTable
    /// </summary>
    private static DataTable BuildDataTable<T>(IReadOnlyList<T> entities, Dictionary<string, string>? propertyMapping)
    {
        var props = typeof(T).GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        var dt = new DataTable();

        // 列定义（排除导航属性和只读计算属性）
        foreach (var prop in props)
        {
            var colName = propertyMapping?.ContainsKey(prop.Name) == true
                ? propertyMapping[prop.Name]
                : prop.Name;

            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (targetType.IsEnum) targetType = typeof(string);
            dt.Columns.Add(colName, targetType);
        }

        // 填充数据行
        foreach (var entity in entities)
        {
            var row = dt.NewRow();
            foreach (var prop in props)
            {
                var colName = propertyMapping?.ContainsKey(prop.Name) == true
                    ? propertyMapping[prop.Name]
                    : prop.Name;

                var value = prop.GetValue(entity);
                row[colName] = value ?? DBNull.Value;
            }
            dt.Rows.Add(row);
        }

        return dt;
    }
}
