using Dapper;
using System.Data;

namespace RBS.Infrastructure.Data;

/// <summary>
/// Dapper DateOnly? 类型处理器 — 处理 SQL DATE → DateOnly? 的映射
/// </summary>
public class DapperNullableDateOnlyHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value)
    {
        if (value == null || value is DBNull) return null;
        if (value is DateTime dt) return DateOnly.FromDateTime(dt);
        if (value is DateOnly d) return d;
        return DateOnly.FromDateTime(Convert.ToDateTime(value));
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        if (value.HasValue)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.Value.ToDateTime(TimeOnly.MinValue);
        }
        else
        {
            parameter.Value = DBNull.Value;
        }
    }
}
