namespace RBS.Application.DTOs.Contract;

public class ContractDto
{
    public Guid Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public string? RoomFullCode { get; set; }
    public decimal RentAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string PaymentCycle { get; set; } = "Monthly";
    public string Status { get; set; } = "Draft";
    public Guid CompanyId { get; set; }
    public Guid? PreviousContractId { get; set; }
    public int RenewalCount { get; set; }
    public Guid? OriginalContractId { get; set; }
    public decimal? MarketPriceAtRenewal { get; set; }
    public bool HasRenewalContract { get; set; }  // 是否存在其他合同 PreviousContractId 指向本合同
    public bool AutoRenew { get; set; } = true;
    public List<ContractTenantDto> Tenants { get; set; } = new();
    public List<ContractFeeConfigDto> FeeConfigs { get; set; } = new();
}

public class ContractTenantDto
{
    public Guid ContractId { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? TenantPhone { get; set; }
    public bool IsPrimary { get; set; }
}

public class ContractFeeConfigDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Guid FeeCodeId { get; set; }
    public string? FeeCodeName { get; set; }
    public decimal Amount { get; set; }
    public string BillingMode { get; set; } = "FixedAmount";
    public string? Unit { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool IsActive { get; set; }
}

public class CreateContractRequest
{
    public string? ContractNo { get; set; }
    public Guid RoomId { get; set; }
    public decimal RentAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string PaymentCycle { get; set; } = "Monthly";
    public Guid CompanyId { get; set; }
    public List<Guid> TenantIds { get; set; } = new();
    public List<ContractFeeDto> Fees { get; set; } = new();
}

public class ContractFeeDto
{
    public Guid FeeCodeId { get; set; }
    public decimal Amount { get; set; }
    public string BillingMode { get; set; } = "FixedAmount";
}
