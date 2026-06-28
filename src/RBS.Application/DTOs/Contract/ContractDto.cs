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
    public List<ContractTenantDto> Tenants { get; set; } = new();
    public List<ContractFeeConfigDto> FeeConfigs { get; set; } = new();
}

public class ContractTenantDto
{
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public bool IsPrimary { get; set; }
}

public class ContractFeeConfigDto
{
    public Guid FeeCodeId { get; set; }
    public string? FeeCodeName { get; set; }
    public decimal Amount { get; set; }
    public string BillingMode { get; set; } = "FixedAmount";
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
