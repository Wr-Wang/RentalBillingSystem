using Microsoft.Data.SqlClient;
using RBS.Core.Interfaces.Persistence;
using System.Data;
using Dapper;

namespace RBS.Infrastructure.Data;

/// <summary>
/// 数据库连接工厂 — 基于连接字符串创建 SqlConnection
/// </summary>
/// <remarks>
/// 在静态构造函数中注册全局 Dapper 类型处理器：
/// <list type="bullet">
///   <item><description>DapperDateOnlyHandler — DateOnly ↔ DateTime 转换</description></item>
///   <item><description>DapperNullableDateOnlyHandler — Nullable DateOnly 处理</description></item>
///   <item><description>DapperRoomStatusHandler — RoomStatus 值对象 ↔ 字符串转换</description></item>
/// </list>
/// 设计为单例生命周期，所有仓储复用同一工厂。
/// </remarks>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// 静态构造函数 — 注册全局 Dapper 类型处理器（进程生命周期内仅执行一次）
    /// </summary>
    static DbConnectionFactory()
    {
        SqlMapper.AddTypeHandler(new DapperDateOnlyHandler());
        SqlMapper.AddTypeHandler(new DapperNullableDateOnlyHandler());
        SqlMapper.AddTypeHandler(new DapperRoomStatusHandler());
    }

    /// <summary>
    /// 初始化连接工厂
    /// </summary>
    /// <param name="connectionString">SQL Server 连接字符串</param>
    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// 创建一个新的数据库连接
    /// </summary>
    /// <remarks>每次调用返回新连接，调用方负责释放</remarks>
    /// <returns>SqlConnection 实例</returns>
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
