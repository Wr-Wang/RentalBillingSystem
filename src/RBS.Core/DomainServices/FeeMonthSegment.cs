namespace RBS.Core.DomainServices;

/// <summary>
/// 周期收费月份拆分分段。
/// 纯数据对象，不含行为，由 BillingDomainService.CalculateMonthlySplit 返回。
/// 表示从生效日到当前月份按自然月拆分后的一个配置分段。
/// </summary>
public record FeeMonthSegment
{
    /// <summary>生效日期（yyyy-MM-dd）</summary>
    public string EffectiveDate { get; init; } = string.Empty;

    /// <summary>到期日期（yyyy-MM-dd），NULL 表示长期有效</summary>
    public string? ExpiryDate { get; init; }

    /// <summary>是否启用</summary>
    public bool IsActive { get; init; }
}
