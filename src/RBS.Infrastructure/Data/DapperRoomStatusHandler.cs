using Dapper;
using RBS.Core.Entities.Base;
using System.Data;

namespace RBS.Infrastructure.Data;

public class DapperRoomStatusHandler : SqlMapper.TypeHandler<RoomStatus>
{
    public override RoomStatus Parse(object value)
    {
        if (value is string s) return RoomStatus.FromCode(s);
        if (value is RoomStatus rs) return rs;
        return RoomStatus.FromCode(value?.ToString() ?? "Vacant");
    }

    public override void SetValue(IDbDataParameter parameter, RoomStatus value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.Code;
    }
}
