using Dapper;
using System.Data;

namespace RBS.Infrastructure.Data;

/// <summary>
/// Dapper DateOnly 类型处理器 — 实现 DateOnly 与 DateTime 的双向转换
/// </summary>
/// <remarks>
/// SQL Server 不支持原生 DateOnly，DB 层使用 DateTime/Date 类型存储。
/// 该处理器使 C# 实体可以直接使用 DateOnly 属性，Dapper 自动完成转换。
/// 写入时将 DateOnly 转为 DateTime（TimeOnly.MinValue），读取时将 DateTime 转为 DateOnly。
/// </remarks>
public class DapperDateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
{
    /// <summary>将数据库值（DateTime）转换为 DateOnly</summary>
    public override DateOnly Parse(object value)
    {
        if (value is DateTime dt) return DateOnly.FromDateTime(dt);
        if (value is DateOnly d) return d;
        return DateOnly.FromDateTime(Convert.ToDateTime(value));
    }

    /// <summary>将 DateOnly 写入数据库参数（转为 DateTime，DbType=DateTime）</summary>
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.DateTime;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}
