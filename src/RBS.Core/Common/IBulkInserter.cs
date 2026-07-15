using System.Data;

namespace RBS.Core.Common;

/// <summary>
/// 批量插入器接口 — 用于大量数据的快速写入（SqlBulkCopy 的抽象）
/// 将基础设施层的批量写入能力通过接口暴露给应用层
/// </summary>
public interface IBulkInserter
{
    /// <summary>
    /// 批量写入 DataTable 到指定表
    /// </summary>
    /// <param name="tableName">目标表名</param>
    /// <param name="dataTable">已填充数据的 DataTable（列名需匹配目标表）</param>
    /// <param name="ct">取消令牌</param>
    Task BulkInsertAsync(string tableName, DataTable dataTable, CancellationToken ct = default);
}
