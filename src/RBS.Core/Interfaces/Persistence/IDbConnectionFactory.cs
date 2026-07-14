using System.Data;

namespace RBS.Core.Interfaces.Persistence;

/// <summary>
/// 数据库连接工厂 — 替代 EF Core DbContext，用于 Dapper 查询。
/// 提供数据库连接的创建能力，不与具体 ORM 绑定，
/// 支持读写分离场景下根据查询类型返回不同的连接实例。
/// Core 层通过此接口获取 IDbConnection，实现与具体数据库技术的解耦。
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// 创建并返回一个数据库连接实例。
    /// 连接的数据库类型和连接字符串由实现层（如 Infrastructure）配置，
    /// 支持 SQL Server、PostgreSQL 等多种数据库。
    /// </summary>
    /// <returns>数据库连接实例，调用方需自行管理连接的打开和释放</returns>
    IDbConnection CreateConnection();
}
