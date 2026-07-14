namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Property;

/// <summary>
/// 房源领域服务 — 房源状态管理、可租校验、定价计算等跨聚合操作
/// </summary>
public interface IPropertyDomainService
{
    /// <summary>
    /// 判断房源是否可以出租（无其他生效合同）
    /// </summary>
    /// <param name="housingUnitId">房源ID</param>
    /// <param name="excludeContractId">排除的合同ID（续签时使用）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>是否可以出租</returns>
    Task<bool> CanOccupyRoomAsync(Guid housingUnitId, Guid? excludeContractId = null, CancellationToken ct = default);

    /// <summary>
    /// 计算房源预估租金（基础租金 × 楼层调整 × 房型溢价）
    /// </summary>
    /// <param name="housingUnit">房源</param>
    /// <param name="pricingStandard">定价标准</param>
    /// <param name="floorLevelBand">楼层段配置</param>
    /// <returns>计算后的预估月租金</returns>
    decimal CalculateEstimatedRent(HousingUnit housingUnit, RoomPricingStandard? pricingStandard, FloorLevelBand? floorLevelBand);
}
