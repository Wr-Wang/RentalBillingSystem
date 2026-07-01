using System.Data;

namespace RBS.Core.Interfaces.Persistence;

/// <summary>
/// 数据库连接工厂 — 替代 EF Core DbContext，用于 Dapper 查询
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
