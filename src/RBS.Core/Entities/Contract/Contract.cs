namespace RBS.Core.Entities.Contract;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同聚合根 — 租赁合同，管理租约全生命周期
/// </summary>
public class Contract : AggregateRoot, IHasCompany
{
    // ===== 基本属性 =====
    /// <summary>合同编号，业务唯一标识，生成后不可变更</summary>
    public string ContractNo { get; private set; }
    /// <summary>房源标识，指向 HousingUnit 聚合根</summary>
    public Guid RoomId { get; private set; }
    /// <summary>合同起租日期，用于生成应收的时间起点</summary>
    public DateOnly StartDate { get; private set; }
    /// <summary>合同到期日期，null 表示不限制（无固定到期日，即长期合同）</summary>
    public DateOnly? EndDate { get; private set; }
    /// <summary>付款周期，可选值：Monthly（月付）、Quarterly（季付）、Yearly（年付）、OneTime（一次性）</summary>
    public string PaymentCycle { get; private set; }
    /// <summary>合同状态，基于 ContractStatus 枚举的状态机管理</summary>
    public ContractStatus Status { get; private set; } = ContractStatus.Draft;
    /// <summary>所属公司标识，用于多租户数据隔离</summary>
    public Guid CompanyId { get; private set; }
    /// <summary>乐观并发控制版本戳，由数据库自动维护</summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // ===== 续签链字段 =====
    /// <summary>上一份合同的标识，用于追溯续签历史链</summary>
    public Guid? PreviousContractId { get; private set; }
    /// <summary>续签次数，从 0 开始计数，每次续签递增</summary>
    public int RenewalCount { get; private set; }
    /// <summary>预存金额（独立于日记账），用于 SettleJob 预收抵应收的判断和扣减逻辑</summary>
    public decimal PrepaidBalance { get; private set; }
    /// <summary>原始合同标识，整个续签链指向最初的首份合同</summary>
    public Guid? OriginalContractId { get; private set; }
    /// <summary>续签时的市场参考价格，用于审计和定价决策追溯</summary>
    public decimal? MarketPriceAtRenewal { get; private set; }

    // ===== 自动续签 =====
    /// <summary>是否启用自动续签，默认启用；到期前系统根据此标志判断是否需要触发续签流程</summary>
    public bool AutoRenew { get; private set; } = true;

    // ===== 终止信息 =====
    /// <summary>合同终止时间，仅当 Status 为 Terminated 时有值</summary>
    public DateTime? TerminatedAt { get; private set; }
    /// <summary>合同终止原因，如违约退租、协商解除等</summary>
    public string? TerminationReason { get; private set; }
    /// <summary>合同暂停时间，仅当 Status 为 Suspended 时有值</summary>
    public DateTime? SuspendedAt { get; private set; }
    /// <summary>合同恢复时间，从暂停状态恢复为 Active 时记录</summary>
    public DateTime? ResumedAt { get; private set; }

    // ===== 内部集合 =====
    private readonly List<ContractTenant> _contractTenants = new();
    private readonly List<ContractFeeConfig> _feeConfigs = new();
    /// <summary>合同关联租客集合，只读暴露，通过 AddTenant/RemoveTenant 操作</summary>
    public IReadOnlyCollection<ContractTenant> ContractTenants => _contractTenants.AsReadOnly();
    /// <summary>合同费用配置集合，只读暴露，通过 AddFeeConfig/AdjustFeeConfig/RemoveFeeConfig 操作</summary>
    public IReadOnlyCollection<ContractFeeConfig> FeeConfigs => _feeConfigs.AsReadOnly();

    // ===== EF Core =====
    /// <summary>
    /// 仅供 EF Core 反序列化使用，禁止业务代码直接调用
    /// </summary>
    private Contract() : base()
    {
        ContractNo = string.Empty;
        PaymentCycle = "Monthly";
        Status = ContractStatus.Draft;
    }

    // ===== 领域构造函数 =====
    /// <summary>
    /// 创建新的合同聚合根实例，初始状态为草稿
    /// </summary>
    /// <param name="contractNo">合同编号，业务唯一标识，不能为空</param>
    /// <param name="roomId">关联房源标识</param>
    /// <param name="companyId">所属公司标识</param>
    /// <exception cref="ArgumentException">当合同编号为空或空白时抛出</exception>
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

    /// <summary>
    /// 设置合同起止日期
    /// </summary>
    /// <param name="start">起租日期</param>
    /// <param name="end">到期日期，null 表示不限制</param>
    /// <exception cref="ArgumentException">当结束日期不为 null 且小于等于开始日期时抛出</exception>
    /// <exception cref="InvalidOperationException">当合同不是草稿状态时抛出</exception>
    public void SetPeriod(DateOnly start, DateOnly? end)
    {
        if (end.HasValue && start >= end.Value) throw new ArgumentException("结束日期必须大于开始日期");
        AssertIsDraft();
        StartDate = start;
        EndDate = end;
    }

    /// <summary>
    /// 设置付款周期
    /// </summary>
    /// <param name="cycle">付款周期编码，可选值：Monthly、Quarterly、Yearly、OneTime</param>
    /// <exception cref="ArgumentException">当传入无效的付款周期时抛出</exception>
    /// <exception cref="InvalidOperationException">当合同不是草稿状态时抛出</exception>
    public void SetPaymentCycle(string cycle)
    {
        var valid = new[] { "Monthly", "Quarterly", "Yearly", "OneTime" };
        if (!valid.Contains(cycle))
            throw new ArgumentException($"无效付款周期: {cycle}");
        AssertIsDraft();
        PaymentCycle = cycle;
    }

    /// <summary>
    /// 供内部/ORM 使用，直接设置状态编码
    /// </summary>
    /// <remarks>此方法绕过状态机校验，仅限领域内部或 ORM 反序列化场景使用，禁止外部业务调用</remarks>
    /// <param name="status">合同状态编码</param>
    internal void SetStatus(string status) => Status = ContractStatus.FromCode(status);
    /// <summary>
    /// 设置续签次数（仅供系统内部使用）
    /// </summary>
    /// <remarks>此方法仅限领域内部调用（如续签链设置），禁止外部业务直接修改续签次数</remarks>
    /// <param name="count">续签次数，必须大于等于 0</param>
    /// <exception cref="ArgumentOutOfRangeException">当 count 小于 0 时抛出</exception>
    internal void SetRenewalCount(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "续签次数不能为负");
        RenewalCount = count;
    }
    /// <summary>
    /// 设置是否启用自动续签（仅供系统内部使用）
    /// </summary>
    /// <remarks>此方法仅限领域内部或续签流程调用，禁止外部业务直接修改自动续签标志</remarks>
    /// <param name="autoRenew">true 为启用自动续签</param>
    internal void SetAutoRenew(bool autoRenew) => AutoRenew = autoRenew;

    /// <summary>
    /// 配置合同自动续签 — 对外公开的业务方法，含状态校验
    /// </summary>
    /// <param name="autoRenew">true 为启用自动续签</param>
    /// <exception cref="InvalidOperationException">合同已终止或已到期时无法修改</exception>
    public void ConfigureAutoRenew(bool autoRenew)
    {
        if (Status == "Terminated")
            throw new InvalidOperationException("合同已终止，无法修改自动续签设置");
        if (Status == "Expired")
            throw new InvalidOperationException("合同已到期，无法修改自动续签设置");
        AutoRenew = autoRenew;
    }

    // ===== 租客管理 =====

    /// <summary>
    /// 添加租客到合同
    /// </summary>
    /// <param name="tenantId">租客标识</param>
    /// <param name="isPrimary">是否为主承租人，默认 false</param>
    /// <exception cref="InvalidOperationException">当该租客已关联到此合同时抛出</exception>
    public void AddTenant(Guid tenantId, bool isPrimary = false)
    {
        if (_contractTenants.Any(ct => ct.TenantId == tenantId))
            throw new InvalidOperationException("该租客已关联到此合同");
        _contractTenants.Add(new ContractTenant(Id, tenantId, isPrimary));
    }

    /// <summary>
    /// 从合同中移除租客
    /// </summary>
    /// <param name="tenantId">租客标识</param>
    /// <exception cref="InvalidOperationException">当租客未关联到此合同，或移除此租客后合同将无任何租客时抛出</exception>
    public void RemoveTenant(Guid tenantId)
    {
        var ct = _contractTenants.FirstOrDefault(x => x.TenantId == tenantId)
            ?? throw new InvalidOperationException("该租客未关联到此合同");
        if (_contractTenants.Count <= 1)
            throw new InvalidOperationException("合同必须至少有一个租客");
        _contractTenants.Remove(ct);
    }

    // ===== 费用管理 =====

    /// <summary>
    /// 添加费用配置到合同
    /// </summary>
    /// <param name="feeCodeId">费用项目标识</param>
    /// <param name="amount">金额</param>
    /// <param name="billingMode">计费模式，默认 FixedAmount（固定金额）</param>
    /// <param name="unit">计量单位，抄表计量模式下必填，如"吨"、"度"</param>
    /// <param name="unitPrice">单价，抄表计量模式下必填</param>
    /// <param name="effectiveDate">生效日期（yyyy-MM-dd），null 表示立即生效</param>
    /// <exception cref="InvalidOperationException">当该费用项目已处于活跃配置状态时抛出</exception>
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

    /// <summary>
    /// 调整费用金额，将旧配置到期停用并创建新配置
    /// </summary>
    /// <param name="feeCodeId">费用项目标识</param>
    /// <param name="newAmount">新的金额值</param>
    /// <param name="effectiveDate">新金额生效日期（yyyy-MM-dd），旧配置在此日前一天到期</param>
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

    /// <summary>
    /// 停用费用配置（软删除）
    /// </summary>
    /// <param name="feeCodeId">费用项目标识</param>
    /// <exception cref="InvalidOperationException">当未找到该费用配置时抛出</exception>
    public void RemoveFeeConfig(Guid feeCodeId)
    {
        var fc = _feeConfigs.FirstOrDefault(f => f.FeeCodeId == feeCodeId)
            ?? throw new InvalidOperationException("未找到该费用配置");
        fc.Deactivate();
    }

    // ===== 状态机 =====

    /// <summary>
    /// 提交审批，将合同状态从草稿变更为待审批
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不允许此变更，或校验不通过时抛出</exception>
    public void SubmitForApproval()
    {
        AssertValidTransition("PendingApproval");
        ValidateForSubmission();
        Status = "PendingApproval";
    }

    /// <summary>
    /// 激活合同，将状态变更为生效中，触发合同激活领域事件
    /// </summary>
    public void Activate()
    {
        AssertValidTransition("Active");
        Status = "Active";
        AddDomainEvent(new ContractActivatedEvent(Id, RoomId, CompanyId));
    }

    /// <summary>
    /// 暂停合同，记录暂停时间并触发暂停领域事件
    /// </summary>
    public void Suspend()
    {
        AssertValidTransition("Suspended");
        Status = "Suspended";
        SuspendedAt = ChinaTime.Now;
        AddDomainEvent(new ContractSuspendedEvent(Id));
    }

    /// <summary>
    /// 恢复已暂停的合同至生效状态，记录恢复时间并触发恢复领域事件
    /// </summary>
    /// <exception cref="InvalidOperationException">当合同不是暂停状态时抛出</exception>
    public void Resume()
    {
        if (Status != "Suspended")
            throw new InvalidOperationException("只有已暂停的合同可以恢复");
        Status = "Active";
        ResumedAt = ChinaTime.Now;
        AddDomainEvent(new ContractResumedEvent(Id, ResumedAt.Value));
    }

    /// <summary>
    /// 终止合同，记录终止时间和原因，触发终止领域事件
    /// </summary>
    /// <param name="reason">终止原因，不能为空</param>
    /// <exception cref="ArgumentException">当终止原因为空或空白时抛出</exception>
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

    /// <summary>
    /// 合同到期，将状态变更为已到期
    /// </summary>
    public void Expire()
    {
        AssertValidTransition("Expired");
        Status = "Expired";
    }

    /// <summary>
    /// 标记合同为已续签状态（原合同在续签完成后调用）
    /// </summary>
    public void MarkAsRenewed()
    {
        AssertValidTransition("Renewed");
        Status = "Renewed";
    }

    /// <summary>
    /// 设置续签链信息（创建新续签合同时调用，关联上一份合同并继承原始合同标识）
    /// </summary>
    /// <param name="previousContractId">上一份合同标识</param>
    /// <param name="renewalCount">续签次数（从上一份合同的续签次数 +1）</param>
    /// <param name="originalContractId">原始合同标识，为 null 时使用 previousContractId</param>
    /// <param name="marketPrice">续签时的市场参考价格，可为 null</param>
    public void SetRenewalChain(Guid previousContractId, int renewalCount, Guid? originalContractId, decimal? marketPrice)
    {
        PreviousContractId = previousContractId;
        RenewalCount = renewalCount;
        OriginalContractId = originalContractId ?? previousContractId;
        MarketPriceAtRenewal = marketPrice;
    }

    // ===== 查询方法 =====

    /// <summary>
    /// 判断合同在指定日期是否有效
    /// </summary>
    /// <param name="date">目标日期</param>
    /// <returns>合同状态为生效中且日期在合同期内返回 true；EndDate 为 null 时仅判断不早于 StartDate</returns>
    public bool IsEffectiveOn(DateOnly date)
        => Status == "Active" && date >= StartDate && (EndDate == null || date <= EndDate.Value);

    /// <summary>
    /// 判断指定账期是否需要生成应收
    /// </summary>
    /// <param name="periodStr">账期字符串，格式如 "2026-07"</param>
    /// <returns>合同为生效中且账期与合同期有交集时返回 true；EndDate 为 null 时仅判断账期结束不早于 StartDate</returns>
    public bool ShouldGenerateReceivableFor(string periodStr)
    {
        if (Status != "Active") return false;
        var period = Period.Parse(periodStr);
        if (EndDate == null) return period.EndDate >= StartDate;
        return period.StartDate <= EndDate.Value && period.EndDate >= StartDate;
    }

    /// <summary>
    /// 获取当前活跃（启用中）的费用配置集合
    /// </summary>
    /// <remarks>ChargeType 过滤在应用层或 SQL 层处理，Entity 本身无 FeeCode 导航属性，无法按 ChargeType 过滤</remarks>
    /// <returns>IsActive 为 true 的 ContractFeeConfig 集合</returns>
    public IEnumerable<ContractFeeConfig> GetActiveFeeConfigs()
        => _feeConfigs.Where(f => f.IsActive);

    /// <summary>
    /// 获取自起租至今已过的天数
    /// </summary>
    /// <returns>从 StartDate 到今天的自然天数（含起租日）</returns>
    public int ElapsedDaysSinceStart()
    {
        var today = DateOnly.FromDateTime(ChinaTime.Now);
        return today.DayNumber - StartDate.DayNumber;
    }

    /// <summary>
    /// 获取合同剩余天数
    /// </summary>
    /// <returns>从今天到 EndDate 的天数；EndDate 为 null 时返回 int.MaxValue 表示无限制</returns>
    public int RemainingDays()
    {
        if (EndDate == null) return int.MaxValue;
        var today = DateOnly.FromDateTime(ChinaTime.Now);
        return EndDate.Value.DayNumber - today.DayNumber;
    }

    // ===== 私有校验 =====

    /// <summary>
    /// 断言合同为草稿状态
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不是 Draft 时抛出</exception>
    private void AssertIsDraft()
    {
        if (Status != "Draft")
            throw new InvalidOperationException("只有草稿状态的合同可以修改");
    }

    /// <summary>
    /// 校验提交审批前的业务规则：
    /// - 至少有一个活跃的费用配置
    /// - 至少有一个租客
    /// - 起租日期已设置（不为 default）
    /// </summary>
    /// <exception cref="InvalidOperationException">当任意校验不通过时抛出</exception>
    private void ValidateForSubmission()
    {
        if (!_feeConfigs.Any(f => f.IsActive)) throw new InvalidOperationException("合同必须至少有一个费用配置");
        if (_contractTenants.Count == 0) throw new InvalidOperationException("合同必须至少有一个租客");
        if (StartDate == default) throw new InvalidOperationException("合同起租日期未设置");
    }

    /// <summary>
    /// 断言允许从当前状态变更为目标状态（基于 ContractStatus 状态机规则）
    /// </summary>
    /// <param name="targetStatus">目标状态编码</param>
    /// <exception cref="InvalidOperationException">当状态机不允许此变更时抛出</exception>
    private void AssertValidTransition(string targetStatus)
    {
        var current = ContractStatus.FromCode(Status);
        var target = ContractStatus.FromCode(targetStatus);
        if (!current.CanTransitionTo(target))
            throw new InvalidOperationException($"不允许从 {Status} 变更为 {targetStatus}");
    }
}
