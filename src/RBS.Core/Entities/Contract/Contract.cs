namespace RBS.Core.Entities.Contract;

using RBS.Core.Entities.Base;

/// <summary>
/// 合同聚合根 — 租赁合同，管理租约全生命周期
/// </summary>
public class Contract : AggregateRoot, IHasLandlord
{
    // ===== 基本属性 =====
    public string ContractNo { get; private set; }
    public Guid RoomId { get; private set; }
    public decimal RentAmount { get; private set; }
    public decimal DepositAmount { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string PaymentCycle { get; private set; }
    public string StatusCode { get; private set; }
    public Guid LandlordId { get; private set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>兼容旧代码</summary>
    [Obsolete("Use StatusCode instead")]
    public string Status => StatusCode;

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
        StatusCode = "Draft";
    }

    // ===== 领域构造函数 =====
    public Contract(string contractNo, Guid roomId, Guid landlordId) : base()
    {
        if (string.IsNullOrWhiteSpace(contractNo))
            throw new ArgumentException("合同编号不能为空", nameof(contractNo));
        ContractNo = contractNo;
        RoomId = roomId;
        LandlordId = landlordId;
        PaymentCycle = "Monthly";
        StatusCode = "Draft";
    }

    // ===== 设置器（草稿状态可修改）=====

    public void SetRentAmount(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("租金不能为负数");
        AssertIsDraft();
        RentAmount = amount;
    }

    public void SetDepositAmount(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("押金不能为负数");
        AssertIsDraft();
        DepositAmount = amount;
    }

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
    public void SetStatus(string status) => StatusCode = status;

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
        string billingMode = "FixedAmount", string? unit = null, decimal? unitPrice = null)
    {
        if (_feeConfigs.Any(fc => fc.FeeCodeId == feeCodeId && fc.IsActive))
            throw new InvalidOperationException("该费用项目已配置");

        var config = new ContractFeeConfig(Id, feeCodeId, amount);
        config.SetBillingMode(BillingMode.FromCode(billingMode));
        if (unit != null) config.SetUnit(unit);
        if (unitPrice.HasValue) config.SetUnitPrice(unitPrice.Value);
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
        StatusCode = "PendingApproval";
    }

    public void Activate()
    {
        AssertValidTransition("Active");
        StatusCode = "Active";
        AddDomainEvent(new ContractActivatedEvent(Id, RoomId, LandlordId));
    }

    public void Suspend()
    {
        AssertValidTransition("Suspended");
        StatusCode = "Suspended";
        SuspendedAt = DateTime.Now;
        AddDomainEvent(new ContractSuspendedEvent(Id));
    }

    public void Resume()
    {
        if (StatusCode != "Suspended")
            throw new InvalidOperationException("只有已暂停的合同可以恢复");
        StatusCode = "Active";
        ResumedAt = DateTime.Now;
    }

    public void Terminate(string reason)
    {
        AssertValidTransition("Terminated");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("终止原因不能为空");
        StatusCode = "Terminated";
        TerminatedAt = DateTime.Now;
        TerminationReason = reason;
        AddDomainEvent(new ContractTerminatedEvent(Id, RoomId, reason));
    }

    public void Expire()
    {
        AssertValidTransition("Expired");
        StatusCode = "Expired";
    }

    public void MarkAsRenewed()
    {
        AssertValidTransition("Renewed");
        StatusCode = "Renewed";
    }

    // ===== 查询方法 =====

    /// <summary>判断合同在指定日期是否有效</summary>
    public bool IsEffectiveOn(DateOnly date)
        => StatusCode == "Active" && date >= StartDate && date <= EndDate;

    /// <summary>判断指定账期是否需要生成应收</summary>
    public bool ShouldGenerateReceivableFor(string periodStr)
    {
        if (StatusCode != "Active") return false;
        var period = Period.Parse(periodStr);
        return period.StartDate <= EndDate && period.EndDate >= StartDate;
    }

    /// <summary>获取已过天数</summary>
    public int ElapsedDaysSinceStart()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return today.DayNumber - StartDate.DayNumber;
    }

    /// <summary>获取剩余天数</summary>
    public int RemainingDays()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return EndDate.DayNumber - today.DayNumber;
    }

    // ===== 私有校验 =====

    private void AssertIsDraft()
    {
        if (StatusCode != "Draft")
            throw new InvalidOperationException("只有草稿状态的合同可以修改");
    }

    private void ValidateForSubmission()
    {
        if (RentAmount <= 0) throw new InvalidOperationException("租金金额未设置");
        if (_contractTenants.Count == 0) throw new InvalidOperationException("合同必须至少有一个租客");
        if (StartDate == default) throw new InvalidOperationException("合同起租日期未设置");
        if (EndDate == default) throw new InvalidOperationException("合同结束日期未设置");
    }

    private void AssertValidTransition(string targetStatus)
    {
        var current = ContractStatus.FromCode(StatusCode);
        var target = ContractStatus.FromCode(targetStatus);
        if (!current.CanTransitionTo(target))
            throw new InvalidOperationException($"不允许从 {StatusCode} 变更为 {targetStatus}");
    }
}
