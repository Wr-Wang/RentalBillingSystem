namespace RBS.Core.Entities.Contract;

using RBS.Core.Entities.Base;

/// <summary>
/// 合同费用配置 — Contract 聚合下的子实体（非聚合根）
/// 定义合同绑定的收费项目及其金额、计费方式、计量单位等信息
/// </summary>
public class ContractFeeConfig : AuditableEntity
{
    /// <summary>
    /// 所属合同标识
    /// </summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// 费用项目标识，指向 FeeCode 字典表
    /// </summary>
    public Guid FeeCodeId { get; private set; }

    /// <summary>
    /// 计费模式（固定金额 / 抄表计量）
    /// </summary>
    public BillingMode BillingMode { get; private set; } = BillingMode.FixedAmount;

    /// <summary>
    /// 金额（固定金额模式下必填）
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// 计量单位（抄表计量模式下必填，如 "吨"、"度"、"立方米"）
    /// </summary>
    public string? Unit { get; private set; }

    /// <summary>
    /// 单价（抄表计量模式下必填）
    /// </summary>
    public decimal? UnitPrice { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ContractFeeConfig()
    {
        BillingMode = BillingMode.FixedAmount;
    }

    /// <summary>
    /// 创建合同费用配置实例
    /// </summary>
    /// <param name="contractId">所属合同标识</param>
    /// <param name="feeCodeId">费用项目标识</param>
    /// <param name="amount">金额（固定金额模式）</param>
    /// <exception cref="ArgumentException">当金额为负数时抛出</exception>
    public ContractFeeConfig(Guid contractId, Guid feeCodeId, decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("金额不能为负数", nameof(amount));

        ContractId = contractId;
        FeeCodeId = feeCodeId;
        Amount = amount;
        BillingMode = BillingMode.FixedAmount;
        IsActive = true;
    }

    /// <summary>
    /// 设置计费模式
    /// </summary>
    /// <param name="mode">目标计费模式</param>
    /// <exception cref="InvalidOperationException">当配置处于启用状态时，不允许切换计费模式</exception>
    public void SetBillingMode(BillingMode mode)
    {
        if (BillingMode == mode)
            return;

        if (IsActive)
            throw new InvalidOperationException(
                $"费用配置当前为启用状态，无法将计费模式从「{BillingMode.DisplayName}」切换为「{mode.DisplayName}」，请先停用后再切换");

        BillingMode = mode;
    }

    /// <summary>
    /// 设置计量单位（抄表计量模式使用）
    /// </summary>
    /// <param name="unit">计量单位，如 "吨"、"度"、"立方米"</param>
    /// <exception cref="ArgumentException">当单位为空白时抛出</exception>
    public void SetUnit(string? unit)
    {
        if (unit is not null && string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("计量单位不能为空字符串", nameof(unit));

        Unit = unit?.Trim();
    }

    /// <summary>
    /// 设置单价（抄表计量模式使用）
    /// </summary>
    /// <param name="price">单价金额</param>
    /// <exception cref="ArgumentException">当单价为负数时抛出</exception>
    public void SetUnitPrice(decimal? price)
    {
        if (price.HasValue && price.Value < 0)
            throw new ArgumentException("单价不能为负数", nameof(price));

        UnitPrice = price;
    }

    /// <summary>
    /// 调整固定金额
    /// </summary>
    /// <param name="newAmount">新的金额值</param>
    /// <exception cref="ArgumentException">当金额为负数时抛出</exception>
    /// <exception cref="InvalidOperationException">当计费模式不为固定金额时抛出</exception>
    public void AdjustAmount(decimal newAmount)
    {
        if (newAmount < 0)
            throw new ArgumentException("金额不能为负数", nameof(newAmount));

        if (BillingMode != BillingMode.FixedAmount)
            throw new InvalidOperationException($"当前计费模式为「{BillingMode.DisplayName}」，金额仅在固定金额模式下可调整");

        Amount = newAmount;
    }

    /// <summary>
    /// 停用此费用配置
    /// </summary>
    /// <exception cref="InvalidOperationException">当配置已停用时抛出</exception>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("费用配置已是停用状态");

        IsActive = false;
    }

    /// <summary>
    /// 重新启用此费用配置
    /// </summary>
    /// <exception cref="InvalidOperationException">当配置已启用时抛出</exception>
    public void Reactivate()
    {
        if (IsActive)
            throw new InvalidOperationException("费用配置已是启用状态");

        IsActive = true;
    }

    /// <summary>
    /// 判断是否为固定金额计费模式
    /// </summary>
    public bool IsFixedAmount => BillingMode == BillingMode.FixedAmount;

    /// <summary>
    /// 判断是否为抄表计量计费模式
    /// </summary>
    public bool IsMeterBased => BillingMode == BillingMode.MeterBased;
}
