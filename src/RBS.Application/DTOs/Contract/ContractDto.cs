namespace RBS.Application.DTOs.Contract;

/// <summary>
/// 合同数据传输对象 — 合同列表及详情展示
/// </summary>
public class ContractDto
{
    /// <summary>合同 ID</summary>
    public Guid Id { get; set; }
    /// <summary>合同编号</summary>
    public string ContractNo { get; set; } = string.Empty;
    /// <summary>房间 ID</summary>
    public Guid RoomId { get; set; }
    /// <summary>房源完整编码</summary>
    public string? RoomFullCode { get; set; }
    /// <summary>起租日期</summary>
    public DateTime StartDate { get; set; }
    /// <summary>到期日期（null 表示不限）</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>付款周期：Monthly / Quarterly / HalfYearly / Yearly</summary>
    public string PaymentCycle { get; set; } = "Monthly";
    /// <summary>合同状态：Draft / Active / Suspended / Terminated</summary>
    public string Status { get; set; } = "Draft";
    /// <summary>所属公司 ID</summary>
    public Guid CompanyId { get; set; }
    /// <summary>前身合同 ID（续签链）</summary>
    public Guid? PreviousContractId { get; set; }
    /// <summary>续签次数</summary>
    public int RenewalCount { get; set; }
    /// <summary>原始合同 ID（续签链起点）</summary>
    public Guid? OriginalContractId { get; set; }
    /// <summary>续签时的市场价</summary>
    public decimal? MarketPriceAtRenewal { get; set; }
    /// <summary>是否存在其他合同 PreviousContractId 指向本合同（有续签）</summary>
    public bool HasRenewalContract { get; set; }
    /// <summary>是否有待审批的续签申请</summary>
    public bool HasPendingRenewal { get; set; }
    /// <summary>是否有被驳回的续签申请</summary>
    public bool HasRejectedRenewal { get; set; }
    /// <summary>是否自动续签</summary>
    public bool AutoRenew { get; set; } = true;
    /// <summary>月租金（冗余展示）</summary>
    public decimal? RentAmount { get; set; }
    /// <summary>欠款余额（应收未收）</summary>
    public decimal OutstandingBalance { get; set; }
    /// <summary>预存金额（溢收未抵）</summary>
    public decimal PrepaidBalance { get; set; }
    /// <summary>租客列表</summary>
    public List<ContractTenantDto> Tenants { get; set; } = new();
    /// <summary>费用配置列表</summary>
    public List<ContractFeeConfigDto> FeeConfigs { get; set; } = new();
}

/// <summary>
/// 合同租客数据传输对象
/// </summary>
public class ContractTenantDto
{
    /// <summary>合同 ID</summary>
    public Guid ContractId { get; set; }
    /// <summary>租客 ID</summary>
    public Guid TenantId { get; set; }
    /// <summary>租客姓名</summary>
    public string? TenantName { get; set; }
    /// <summary>租客手机号</summary>
    public string? TenantPhone { get; set; }
    /// <summary>是否主租客</summary>
    public bool IsPrimary { get; set; }
}

/// <summary>
/// 合同费用配置数据传输对象
/// </summary>
public class ContractFeeConfigDto
{
    /// <summary>费用配置 ID</summary>
    public Guid Id { get; set; }
    /// <summary>合同 ID</summary>
    public Guid ContractId { get; set; }
    /// <summary>费用代码 ID</summary>
    public Guid FeeCodeId { get; set; }
    /// <summary>费用代码名称</summary>
    public string? FeeCodeName { get; set; }
    /// <summary>费用代码标识：RENT / DEPOSIT / ...</summary>
    public string? FeeCode { get; set; }
    /// <summary>收费类型：Recurring / OneTime</summary>
    public string? ChargeType { get; set; }
    /// <summary>金额</summary>
    public decimal Amount { get; set; }
    /// <summary>计费方式：FixedAmount / MeterBased</summary>
    public string BillingMode { get; set; } = "FixedAmount";
    /// <summary>计量单位</summary>
    public string? Unit { get; set; }
    /// <summary>单价（抄表计量时使用）</summary>
    public decimal? UnitPrice { get; set; }
    /// <summary>是否当前有效</summary>
    public bool IsActive { get; set; }
    /// <summary>生效日期</summary>
    public string? EffectiveDate { get; set; }
    /// <summary>到期日期</summary>
    public string? ExpiryDate { get; set; }
}

/// <summary>
/// 创建合同请求
/// </summary>
public class CreateContractRequest
{
    /// <summary>合同编号</summary>
    public string? ContractNo { get; set; }
    /// <summary>房间 ID</summary>
    public Guid RoomId { get; set; }
    /// <summary>起租日期</summary>
    public DateTime StartDate { get; set; }
    /// <summary>到期日期</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>付款周期</summary>
    public string PaymentCycle { get; set; } = "Monthly";
    /// <summary>所属公司 ID</summary>
    public Guid CompanyId { get; set; }
    /// <summary>租客 ID 列表</summary>
    public List<Guid> TenantIds { get; set; } = new();
    /// <summary>费用配置列表</summary>
    public List<ContractFeeDto> Fees { get; set; } = new();
}

/// <summary>
/// 合同费用项请求
/// </summary>
public class ContractFeeDto
{
    /// <summary>费用代码 ID</summary>
    public Guid FeeCodeId { get; set; }
    /// <summary>金额</summary>
    public decimal Amount { get; set; }
    /// <summary>计费方式</summary>
    public string BillingMode { get; set; } = "FixedAmount";
}

/// <summary>
/// 费用调价请求 — 批量调整合同费用项的金额/单价
/// </summary>
public class FeeAdjustRequest
{
    /// <summary>待调价的费用项列表</summary>
    public List<FeeAdjustItem> Items { get; set; } = new();
}

/// <summary>
/// 费用调价单项
/// </summary>
public class FeeAdjustItem
{
    /// <summary>费用代码 ID</summary>
    public Guid FeeCodeId { get; set; }
    /// <summary>费用名称（展示用）</summary>
    public string FeeName { get; set; } = string.Empty;
    /// <summary>原金额</summary>
    public decimal OldAmount { get; set; }
    /// <summary>新金额</summary>
    public decimal NewAmount { get; set; }
    /// <summary>生效日期（yyyy-MM-dd）</summary>
    public string? EffectiveDate { get; set; }
    /// <summary>计费方式：FixedAmount / MeterBased</summary>
    public string? BillingMode { get; set; }
}

/// <summary>
/// 合同修改信息请求体 — 修改合同信息时提交的字段
/// 可空字段表示该属性不变更，仅非空字段参与更新
/// </summary>
public class ContractModifySubmitRequest
{
    /// <summary>新的起租日期，null 表示不变更</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>新的到期日期，null 表示不变更</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>新的付款周期，null 表示不变更</summary>
    public string? PaymentCycle { get; set; }
    /// <summary>新的自动续签标志，null 表示不变更</summary>
    public bool? AutoRenew { get; set; }
    /// <summary>新的押金抵最后租金标志，null 表示不变更</summary>
    public bool? AllowDepositAsLastRent { get; set; }
    /// <summary>新的付款到期日，null 表示不变更</summary>
    public int? PaymentDueDay { get; set; }
    /// <summary>新的租客联系电话，null 表示不变更</summary>
    public string? TenantPhone { get; set; }
    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
