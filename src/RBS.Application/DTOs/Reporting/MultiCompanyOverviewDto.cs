namespace RBS.Application.DTOs.Reporting;

/// <summary>多公司总览 — 聚合响应 DTO</summary>
public class MultiCompanyOverviewDto
{
    /// <summary>统计时间（查询月份）</summary>
    public string Period { get; set; } = string.Empty;

    // ========== 汇总指标 ==========
    public int TotalCompanies { get; set; }
    public int ActiveCompanies { get; set; }
    public int TotalBuildings { get; set; }
    public int TotalRooms { get; set; }
    public int TotalRented { get; set; }
    public decimal AvgOccupancyRate { get; set; }
    public decimal TotalMonthlyReceivable { get; set; }
    public decimal TotalMonthlyReceived { get; set; }
    public decimal AvgCollectionRate { get; set; }
    public decimal TotalOverdueAmount { get; set; }
    public int TotalOverdueCount { get; set; }
    public int TotalActiveContracts { get; set; }

    /// <summary>同比上期综合收租率变化（百分点）</summary>
    public decimal? CollectionRateChange { get; set; }
    /// <summary>环比上月综合收租率变化（百分点）</summary>
    public decimal? CollectionRateMomChange { get; set; }
    /// <summary>同比上期综合出租率变化（百分点）</summary>
    public decimal? OccupancyRateChange { get; set; }

    /// <summary>各公司明细</summary>
    public List<CompanyOverviewItem> Companies { get; set; } = new();
}

/// <summary>单个公司在总览中的指标</summary>
public class CompanyOverviewItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // 资产概况
    public int BuildingCount { get; set; }
    public int RoomCount { get; set; }
    public int RentedCount { get; set; }
    public decimal OccupancyRate { get; set; }

    // 财务概况
    public decimal MonthlyReceivable { get; set; }
    public decimal MonthlyReceived { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal OverdueAmount { get; set; }
    public int OverdueCount { get; set; }

    // 合同概况
    public int ActiveContractCount { get; set; }
    public int TotalContractCount { get; set; }

    // 上月收租率（用于趋势比较）
    public decimal? PrevMonthCollectionRate { get; set; }

    /// <summary>健康分 0-100（加权：出租率30% + 收租率40% + 逾期率30%）</summary>
    public decimal HealthScore
    {
        get
        {
            var occupancyScore = OccupancyRate * 0.3m;
            var collectionScore = CollectionRate * 0.4m;
            var overdueScore = Math.Max(0, 100 - (OverdueAmount > 0 ? Math.Min(OverdueAmount / 1000, 100) : 0)) * 0.3m;
            return Math.Round(occupancyScore + collectionScore + overdueScore, 1);
        }
    }
}
