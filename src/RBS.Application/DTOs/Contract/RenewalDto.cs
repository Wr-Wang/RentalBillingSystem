namespace RBS.Application.DTOs.Contract;

/// <summary>
/// 续签预览响应
/// </summary>
public class RenewalPreviewDto
{
    public RenewalOldContractDto OldContract { get; set; } = new();
    public List<RenewalInheritedTenantDto> Tenants { get; set; } = new();
    public List<RenewalInheritedFeeDto> FeeConfigs { get; set; } = new();
    public RenewalChecksDto Checks { get; set; } = new();
    public RenewalDefaultsDto DefaultRenewalInfo { get; set; } = new();
}

public class RenewalOldContractDto
{
    public Guid Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public decimal RentAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string PaymentCycle { get; set; } = "Monthly";
    public string Status { get; set; } = string.Empty;
    public string? RoomFullCode { get; set; }
    public int RenewalCount { get; set; }
}

public class RenewalInheritedTenantDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class RenewalInheritedFeeDto
{
    public Guid FeeCodeId { get; set; }
    public string FeeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BillingMode { get; set; } = "FixedAmount";
    public string? Unit { get; set; }
    public decimal? UnitPrice { get; set; }
}

public class RenewalChecksDto
{
    public PaymentStatusDto PaymentStatus { get; set; } = new();
    public ConcurrentApprovalsDto ConcurrentApprovals { get; set; } = new();
    public MarketPriceInfoDto? MarketPrice { get; set; }
}

public class PaymentStatusDto
{
    public bool Passed { get; set; }
    public decimal OutstandingAmount { get; set; }
}

public class ConcurrentApprovalsDto
{
    public bool HasPending { get; set; }
    public string? PendingType { get; set; }
    public string? PendingSubmitter { get; set; }
    public string? PendingTime { get; set; }
    public string? BlockedMessage { get; set; }

    /// <summary>该合同是否已被续签（存在另一份合同 PreviousContractId 指向它）</summary>
    public bool AlreadyRenewed { get; set; }
    /// <summary>续签产生的新合同号</summary>
    public string? RenewedContractNo { get; set; }
    /// <summary>续签产生的新合同ID</summary>
    public Guid? RenewedContractId { get; set; }
}

public class MarketPriceInfoDto
{
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public string SourceDescription { get; set; } = string.Empty;
}

public class RenewalDefaultsDto
{
    public string SuggestedStartDate { get; set; } = string.Empty;
    public string SuggestedEndDate { get; set; } = string.Empty;
    public decimal CurrentRentAmount { get; set; }
}

/// <summary>
/// 提交续签请求
/// </summary>
public class SubmitRenewalRequest
{
    public Guid ContractId { get; set; }
    public decimal NewRentAmount { get; set; }
    public DateOnly NewEndDate { get; set; }
    public string DepositHandling { get; set; } = "TRANSFER";  // TRANSFER / NEW
    public decimal? NewDepositAmount { get; set; }
    public string? Remark { get; set; }
    public decimal? MarketReferencePrice { get; set; }
}

/// <summary>
/// 续签提交响应
/// </summary>
public class RenewalSubmitResultDto
{
    public Guid RenewalRequestId { get; set; }
    public Guid ApprovalRequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 续签历史记录
/// </summary>
public class RenewalHistoryDto
{
    public Guid Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public decimal PreviousRent { get; set; }
    public decimal NewRent { get; set; }
    public string NewEndDate { get; set; } = string.Empty;
    public string DepositHandling { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Remark { get; set; }
    public Guid? NewContractId { get; set; }
}

/// <summary>
/// 续签链节点
/// </summary>
public class RenewalChainNodeDto
{
    public Guid ContractId { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal RentAmount { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public int RenewalCount { get; set; }
    public bool IsCurrent { get; set; }
}

/// <summary>
/// 合同当前允许的操作
/// </summary>
public class ContractOperationsDto
{
    public bool CanModifyRent { get; set; } = true;
    public bool CanTerminate { get; set; } = true;
    public bool CanRenew { get; set; } = true;
    public bool CanSuspend { get; set; } = true;
    public bool CanResume { get; set; } = true;
    public bool CanAdjustFee { get; set; } = true;
    public string? PendingApprovalType { get; set; }
}
