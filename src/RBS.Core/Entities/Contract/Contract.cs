namespace RBS.Core.Entities.Contract;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同聚合根 — 租赁合同，管理租约全生命周期
/// </summary>
public class Contract : AggregateRoot, IHasCompany
{
    // ===== 基本属性 =====
    public string ContractNo { get; private set; }
    public Guid RoomId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string PaymentCycle { get; private set; }
    public ContractStatus Status { get; private set; } = ContractStatus.Draft;
    public Guid CompanyId { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // ===== 续签链字段 =====
    public Guid? PreviousContractId { get; private set; }
    public int RenewalCount { get; private set; }
    public Guid? OriginalContractId { get; private set; }
    public decimal? MarketPriceAtRenewal { get; private set; }

    // ===== 自动续签 =====
    public bool AutoRenew { get; private set; } = true;

    // ===== 终止信息 =====
    public DateTime? TerminatedAt { get; private set; }
    public string? TerminationReason { get; private set; }
    public DateTime? SuspendedAt { get; private set; }
    public DateTime? ResumedAt { get; private set; }

    // ===== 内部集合 =====
    private readonly List<ContractTenant> _contractTenants = new();
    private readonly List<ContractFeeConfig> _feeConfigs = new();
    public IReadOnlyCollection<ContractTenant> ContractTenants => _contractTenants.AsReadOnly();
    public IReadOnlyCollection<ContractFeeConfig> FeeConfigs => _feeConfigs.AsReadOnly();

    // ===== EF Core =====
    private Contract() : base()
    {
        ContractNo = string.Empty;
        PaymentCycle = "Monthly";
        Status = ContractStatus.Draft;
    }

    // ===== 领域构造函数 =====
    public Contract(string contractNo, Guid roomId, Guid companyId) : base()
    {
        if (string.IsNullOrWhiteSpace(contractNo))
            throw new ArgumentException("合同编号不能为空", nameof(contractNo));
        ContractNo = contractNo;
        RoomId = roomId;
        CompanyId = companyId;
        PaymentCycle = "Monthly";
        Status = "Draft";
    }

    // ===== 设置器（草稿状态可修改）=====

    public void SetPeriod(DateOnly start, DateOnly end)
    {
        if (start >= end) throw new ArgumentException("结束日期必须大于开始日期");
        AssertIsDraft();
        StartDate = start;
        EndDate = end;
    }

    public void SetPaymentCycle(string cycle)
    {
        var valid = new[] { "Monthly", "Quarterly", "Yearly", "OneTime" };
        if (!valid.Contains(cycle))
            throw new ArgumentException($"无效付款周期: {cycle}");
        AssertIsDraft();
        PaymentCycle = cycle;
    }

    /// <summary>供内部/ORM 使用</summary>
    public void SetStatus(string status) => Status = ContractStatus.FromCode(status);
    public void SetRenewalCount(int count) => RenewalCount = count;
    public void SetAutoRenew(bool autoRenew) => AutoRenew = autoRenew;

    // ===== 租客管理 =====

    public void AddTenant(Guid tenantId, bool isPrimary = false)
    {
        if (_contractTenants.Any(ct => ct.TenantId == tenantId))
            throw new InvalidOperationException("该租客已关联到此合同");
        _contractTenants.Add(new ContractTenant(Id, tenantId, isPrimary));
    }

    public void RemoveTenant(Guid tenantId)
    {
        var ct = _contractTenants.FirstOrDefault(x => x.TenantId == tenantId)
            ?? throw new InvalidOperationException("该租客未关联到此合同");
        if (_contractTenants.Count <= 1)
            throw new InvalidOperationException("合同必须至少有一个租客");
        _contractTenants.Remove(ct);
    }

    // ===== 费用管理 =====

    public void AddFeeConfig(Guid feeCodeId, decimal amount,
        string billingMode = "FixedAmount", string? unit = null, decimal? unitPrice = null,
        string? effectiveDate = null)
    {
        if (_feeConfigs.Any(fc => fc.FeeCodeId == feeCodeId && fc.IsActive))
            throw new InvalidOperationException("该费用项目已配置");

        var config = new ContractFeeConfig(Id, feeCodeId, amount);
        config.SetBillingMode(BillingMode.FromCode(billingMode));
        if (unit != null) config.SetUnit(unit);
        if (unitPrice.HasValue) config.SetUnitPrice(unitPrice.Value);
        if (effectiveDate != null)
            config.SetEffectivePeriod(effectiveDate, null);
        _feeConfigs.Add(config);
    }

    public void AdjustFeeConfig(Guid feeCodeId, decimal newAmount, string effectiveDate)
    {
        var active = _feeConfigs.FirstOrDefault(f => f.FeeCodeId == feeCodeId && f.IsActive);
        if (active != null)
        {
            var expiryDate = DateOnly.Parse(effectiveDate).AddDays(-1).ToString("yyyy-MM-dd");
            active.ExpireOn(expiryDate);
        }

        var config = new ContractFeeConfig(Id, feeCodeId, newAmount);
        config.SetBillingMode(BillingMode.FixedAmount);
        config.SetEffectivePeriod(effectiveDate, null);
        _feeConfigs.Add(config);
    }

    public void RemoveFeeConfig(Guid feeCodeId)
    {
        var fc = _feeConfigs.FirstOrDefault(f => f.FeeCodeId == feeCodeId)
            ?? throw new InvalidOperationException("未找到该费用配置");
        fc.Deactivate();
    }

    // ===== 状态机 =====

    public void SubmitForApproval()
    {
        AssertValidTransition("PendingApproval");
        ValidateForSubmission();
        Status = "PendingApproval";
    }

    public void Activate()
    {
        AssertValidTransition("Active");
        Status = "Active";
        AddDomainEvent(new ContractActivatedEvent(Id, RoomId, CompanyId));
    }

    public void Suspend()
    {
        AssertValidTransition("Suspended");
        Status = "Suspended";
        SuspendedAt = ChinaTime.Now;
        AddDomainEvent(new ContractSuspendedEvent(Id));
    }

    public void Resume()
    {
        if (Status != "Suspended")
            throw new InvalidOperationException("只有已暂停的合同可以恢复");
        Status = "Active";
        ResumedAt = ChinaTime.Now;
        AddDomainEvent(new ContractResumedEvent(Id, ResumedAt.Value));
    }

    public void Terminate(string reason)
    {
        AssertValidTransition("Terminated");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("终止原因不能为空");
        Status = "Terminated";
        TerminatedAt = ChinaTime.Now;
        TerminationReason = reason;
        AddDomainEvent(new ContractTerminatedEvent(Id, RoomId, reason));
    }

    public void Expire()
    {
        AssertValidTransition("Expired");
        Status = "Expired";
    }

    public void MarkAsRenewed()
    {
        AssertValidTransition("Renewed");
        Status = "Renewed";
    }

    /// <summary>设置续签链信息（创建新合同时调用）</summary>
    public void SetRenewalChain(Guid previousContractId, int renewalCount, Guid? originalContractId, decimal? marketPrice)
    {
        PreviousContractId = previousContractId;
        RenewalCount = renewalCount;
        OriginalContractId = originalContractId ?? previousContractId;
        MarketPriceAtRenewal = marketPrice;
    }

    // ===== 查询方法 =====

    /// <summary>判断合同在指定日期是否有效</summary>
    public bool IsEffectiveOn(DateOnly date)
        => Status == "Active" && date >= StartDate && date <= EndDate;

    /// <summary>判断指定账期是否需要生成应收</summary>
    public bool ShouldGenerateReceivableFor(string periodStr)
    {
        if (Status != "Active") return false;
        var period = Period.Parse(periodStr);
        return period.StartDate <= EndDate && period.EndDate >= StartDate;
    }

    /// <summary>获取活跃的 FeeConfigs 集合</summary>
    /// <remarks>ChargeType 过滤在应用层/SQL 层处理（Entity 无 FeeCode 导航属性）</remarks>
    public IEnumerable<ContractFeeConfig> GetActiveFeeConfigs()
        => _feeConfigs.Where(f => f.IsActive);

    /// <summary>获取已过天数</summary>
    public int ElapsedDaysSinceStart()
    {
        var today = DateOnly.FromDateTime(ChinaTime.Now);
        return today.DayNumber - StartDate.DayNumber;
    }

    /// <summary>获取剩余天数</summary>
    public int RemainingDays()
    {
        var today = DateOnly.FromDateTime(ChinaTime.Now);
        return EndDate.DayNumber - today.DayNumber;
    }

    // ===== 私有校验 =====

    private void AssertIsDraft()
    {
        if (Status != "Draft")
            throw new InvalidOperationException("只有草稿状态的合同可以修改");
    }

    private void ValidateForSubmission()
    {
        if (!_feeConfigs.Any(f => f.IsActive)) throw new InvalidOperationException("合同必须至少有一个费用配置");
        if (_contractTenants.Count == 0) throw new InvalidOperationException("合同必须至少有一个租客");
        if (StartDate == default) throw new InvalidOperationException("合同起租日期未设置");
        if (EndDate == default) throw new InvalidOperationException("合同结束日期未设置");
    }

    private void AssertValidTransition(string targetStatus)
    {
        var current = ContractStatus.FromCode(Status);
        var target = ContractStatus.FromCode(targetStatus);
        if (!current.CanTransitionTo(target))
            throw new InvalidOperationException($"不允许从 {Status} 变更为 {targetStatus}");
    }
}
