using Dapper;
using RBS.Core.Entities.Base;
using System.Data;

namespace RBS.Infrastructure.Data;

/// <summary>
/// Dapper 房间状态类型处理器 — 实现 RoomStatus 值对象与数据库字符串的转换
/// </summary>
/// <remarks>
/// RoomStatus 使用 FromCode(string) 工厂方法创建，Code 作为持久化字段。
/// 默认值 "Vacant"（空闲）在 DB 值为 null 时使用。
/// </remarks>
public class DapperRoomStatusHandler : SqlMapper.TypeHandler<RoomStatus>
{
    public override RoomStatus Parse(object value)
    {
        if (value is string s) return RoomStatus.FromCode(s);
        if (value is RoomStatus rs) return rs;
        return RoomStatus.FromCode(value?.ToString() ?? "Vacant");
    }

    public override void SetValue(IDbDataParameter parameter, RoomStatus? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value?.Code ?? "Vacant";
    }
}
