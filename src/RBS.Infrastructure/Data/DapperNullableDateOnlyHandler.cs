using Dapper;
using System.Data;

namespace RBS.Infrastructure.Data;

/// <summary>
/// Dapper Nullable DateOnly 类型处理器 — 处理 SQL DATE → DateOnly? 的映射
/// </summary>
/// <remarks>
/// 与 DapperDateOnlyHandler 功能相同，额外处理 DBNull 和 null 值。
/// 用于实体中可空的 DateOnly 属性（如合同的终止日期）。
/// </remarks>
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
            parameter.DbType = DbType.DateTime;
            parameter.Value = value.Value.ToDateTime(TimeOnly.MinValue);
        }
        else
        {
            parameter.Value = DBNull.Value;
        }
    }
}
