namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

/// <summary>
/// 房型楼层定价标准 — 按房型 + 楼层段定义的标准租金
/// 用于快速生成房源的参考租金，支持按定价标准批量定价
/// </summary>
public class RoomPricingStandard : AuditableEntity, IHasCompany
{
    /// <summary>房型标识，指向 RoomType 字典表</summary>
    public Guid RoomTypeId { get; private set; }
    /// <summary>楼层段标识，指向 FloorLevelBand</summary>
    public Guid FloorLevelBandId { get; private set; }
    /// <summary>此房型在此楼层段的标准租金金额</summary>
    public decimal RentAmount { get; private set; }
    /// <summary>所属公司标识</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private RoomPricingStandard() { }

    /// <summary>
    /// 创建定价标准
    /// </summary>
    /// <param name="roomTypeId">房型标识</param>
    /// <param name="floorLevelBandId">楼层段标识</param>
    /// <param name="rentAmount">标准租金金额</param>
    /// <param name="companyId">所属公司标识</param>
    public RoomPricingStandard(Guid roomTypeId, Guid floorLevelBandId, decimal rentAmount, Guid companyId)
    { RoomTypeId = roomTypeId; FloorLevelBandId = floorLevelBandId; RentAmount = rentAmount; CompanyId = companyId; }

    /// <summary>设置房型</summary>
    public void SetRoomType(Guid roomTypeId) => RoomTypeId = roomTypeId;
    /// <summary>设置楼层段</summary>
    public void SetFloorLevelBand(Guid bandId) => FloorLevelBandId = bandId;
    /// <summary>设置标准租金金额</summary>
    public void SetRentAmount(decimal amount) => RentAmount = amount;
}
