using Dapper;
using System.Data;

namespace RBS.Infrastructure.Data;

public class DapperDateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        if (value is DateTime dt) return DateOnly.FromDateTime(dt);
        if (value is DateOnly d) return d;
        return DateOnly.FromDateTime(Convert.ToDateTime(value));
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}
