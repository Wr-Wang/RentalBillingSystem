using Microsoft.Data.SqlClient;
using RBS.Core.Interfaces.Persistence;
using System.Data;
using Dapper;

namespace RBS.Infrastructure.Data;

/// <summary>
/// 数据库连接工厂 — 基于连接字符串创建 SqlConnection
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
        static DbConnectionFactory() { SqlMapper.AddTypeHandler(new DapperDateOnlyHandler()); }

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
