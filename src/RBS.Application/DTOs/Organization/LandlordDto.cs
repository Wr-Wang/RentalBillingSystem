namespace RBS.Application.DTOs.Organization;

public class LandlordDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? IdType { get; set; }
    public string? IdNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? BankAccountName { get; set; }
    public string? SettlementCycle { get; set; }
    public int? SettlementDay { get; set; }
    public decimal? CommissionRate { get; set; }
    public string? Remark { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // 资产概况（由 Stats 接口补充）
    public int BuildingCount { get; set; }
    public int RoomCount { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal CollectionRate { get; set; }
}

public class CreateLandlordRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? IdType { get; set; }
    public string? IdNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? BankAccountName { get; set; }
    public string? SettlementCycle { get; set; }
    public int? SettlementDay { get; set; }
    public decimal? CommissionRate { get; set; }
    public string? Remark { get; set; }
    /// <summary>更新时传入以切换启用/停用状态；创建时忽略</summary>
    public bool? IsActive { get; set; }
}

public class LandlordStatsDto
{
    public int BuildingCount { get; set; }
    public int RoomCount { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal CollectionRate { get; set; }
}

public class LandlordQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
