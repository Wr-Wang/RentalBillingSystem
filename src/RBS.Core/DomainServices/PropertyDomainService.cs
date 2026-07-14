namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.UnitOfWork;

/// <summary>
/// 房源领域服务 — 跨聚合的业务逻辑编排。
/// 实现 IPropertyDomainService 接口，协调房源状态校验、可租判断、
/// 定价计算等跨聚合操作，通过 IUnitOfWork 保证跨仓储事务一致性。
/// </summary>
public class PropertyDomainService : IPropertyDomainService
{
    private readonly IUnitOfWork _uow;

    /// <summary>
    /// 初始化房源领域服务
    /// </summary>
    /// <param name="uow">工作单元，用于协调多个仓储的写入</param>
    public PropertyDomainService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <summary>
    /// 判断房源是否可以出租（无其他生效合同）。
    /// 通过工作单元检查目标房屋单元是否已有生效合同。
    /// </summary>
    /// <param name="housingUnitId">房源ID</param>
    /// <param name="excludeContractId">排除的合同ID（续签时使用，暂保留参数）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>该房源目前可出租则为 true</returns>
    public async Task<bool> CanOccupyRoomAsync(Guid housingUnitId, Guid? excludeContractId = null, CancellationToken ct = default)
    {
        // 检查该房源是否有生效合同
        return !await _uow.Contracts.HasActiveForHousingUnitAsync(housingUnitId, ct);
    }

    /// <summary>
    /// 计算房源预估租金（基础租金 × 楼层调整 × 房型溢价）。
    /// 若未传入定价标准且房源有基础租金，直接返回基础租金；
    /// 否则以定价标准为基底乘以楼层调整系数。
    /// </summary>
    /// <param name="housingUnit">房源</param>
    /// <param name="pricingStandard">定价标准</param>
    /// <param name="floorLevelBand">楼层段配置</param>
    /// <returns>计算后的预估月租金，保留两位小数</returns>
    public decimal CalculateEstimatedRent(HousingUnit housingUnit, RoomPricingStandard? pricingStandard, FloorLevelBand? floorLevelBand)
    {
        // 无定价标准时回退到房源基础租金
        if (pricingStandard == null && housingUnit.BaseRentAmount.HasValue)
            return housingUnit.BaseRentAmount.Value;

        // 有定价标准则取其标准租金，否则使用房源基础租金
        var basePrice = pricingStandard?.RentAmount ?? housingUnit.BaseRentAmount ?? 0;
        // TODO: 楼层调整系数目前未在 FloorLevelBand 中定义，后续补充后取消硬编码
        var adjustment = 1.0m;
        return Math.Round(basePrice * adjustment, 2);
    }
}
