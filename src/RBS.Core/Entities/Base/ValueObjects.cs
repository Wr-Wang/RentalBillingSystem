namespace RBS.Core.Entities.Base;

/// <summary>
/// 金额值对象（Money Value Object）
///
/// DDD 角色：值对象（Value Object），封装金额与货币单位的业务语义。
/// 确保金额精度始终保留两位小数，避免浮点运算导致的精度损失。
/// 默认使用人民币（CNY），可通过构造参数指定其他币种。
///
/// 业务约束：
/// - 金额不能为负数（系统中不涉及退款场景，负数金额无业务含义）
/// - 不同币种的金额不能直接进行加减运算
/// - 减法结果不能为负（余额不足时抛出异常，不再向下传递负数）
///
/// 隐式转换：
/// - Money → decimal：方便直接参与算术运算和 ORM 映射
/// - decimal → Money：允许自然地将数值字面量提升为带货币语义的金额对象
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>
    /// 金额数值（精确到两位小数，四舍五入）
    /// 通过 Math.Round(amount, 2) 保证精度，避免数据库存储与计算产生误差。
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// 货币代码（ISO 4217 标准三位字母码）
    /// 默认值为 "CNY"（人民币），系统中目前仅支持人民币。
    /// 扩展至多币种场景时，通过此属性区分币种。
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// 私有无参构造，仅供序列化/ORM 反序列化使用
    /// 默认金额为 0，货币为 CNY。不应在业务代码中直接调用。
    /// </summary>
    private Money() { Amount = 0; Currency = "CNY"; }

    /// <summary>
    /// 以人民币创建金额值对象
    /// </summary>
    /// <param name="amount">金额数值，不能为负数，会四舍五入到两位小数</param>
    /// <exception cref="ArgumentException">amount 小于 0 时抛出</exception>
    public Money(decimal amount) : this(amount, "CNY") { }

    /// <summary>
    /// 以指定币种创建金额值对象
    /// </summary>
    /// <param name="amount">金额数值，不能为负数，会四舍五入到两位小数</param>
    /// <param name="currency">ISO 4217 货币代码（如 "CNY"、"USD"），为 null 时默认 "CNY"</param>
    /// <exception cref="ArgumentException">amount 小于 0 时抛出</exception>
    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("金额不能为负数", nameof(amount));
        Amount = Math.Round(amount, 2);
        Currency = currency ?? "CNY";
    }

    /// <summary>
    /// 获取零金额（CNY 0.00）
    /// 用于初始化空值或作为默认值，避免反复 new Money(0)。
    /// </summary>
    public static Money Zero => new(0);

    /// <summary>
    /// 判断当前金额是否为零
    /// 用于业务逻辑中的金额是否为空的判断（如应收余额已结清）。
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// 隐式转换为 decimal，方便参与算术运算和 ORM 映射
    /// </summary>
    public static implicit operator decimal(Money m) => m.Amount;

    /// <summary>
    /// 隐式从 decimal 创建 Money（默认使用人民币）
    /// 允许直接使用数值字面量（如 100.50m）作为金额传入业务方法。
    /// </summary>
    public static implicit operator Money(decimal amount) => new(amount);

    /// <summary>
    /// 金额加法运算
    /// 要求两个金额的币种必须一致。
    /// </summary>
    /// <param name="other">加数，必须与当前金额币种相同</param>
    /// <returns>相加后的新金额对象</returns>
    /// <exception cref="InvalidOperationException">币种不一致时抛出</exception>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// 金额减法运算
    /// 要求两个金额的币种必须一致，且结果不能为负数。
    /// </summary>
    /// <param name="other">减数，必须与当前金额币种相同</param>
    /// <returns>相减后的新金额对象</returns>
    /// <exception cref="InvalidOperationException">币种不一致或余额不足时抛出</exception>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0) throw new InvalidOperationException("余额不足");
        return new Money(result, Currency);
    }

    /// <summary>
    /// 金额乘法运算（乘以系数）
    /// 用于计费计算场景（如单价 × 使用量 = 费用金额）。
    /// </summary>
    /// <param name="factor">乘数系数</param>
    /// <returns>相乘后的新金额对象</returns>
    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    /// <summary>运算符重载：金额相加</summary>
    public static Money operator +(Money a, Money b) => a.Add(b);
    /// <summary>运算符重载：金额相减</summary>
    public static Money operator -(Money a, Money b) => a.Subtract(b);
    /// <summary>运算符重载：大于比较</summary>
    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    /// <summary>运算符重载：小于比较</summary>
    public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
    /// <summary>运算符重载：大于等于比较</summary>
    public static bool operator >=(Money a, Money b) => a.Amount >= b.Amount;
    /// <summary>运算符重载：小于等于比较</summary>
    public static bool operator <=(Money a, Money b) => a.Amount <= b.Amount;

    /// <summary>
    /// 校验两个金额的币种是否一致
    /// </summary>
    /// <param name="other">待校验的另一个金额对象</param>
    /// <exception cref="InvalidOperationException">币种不一致时抛出，包含两个币种的详细信息</exception>
    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"货币单位不匹配: {Currency} vs {other.Currency}");
    }

    /// <summary>
    /// 获取值对象相等性比较的分量
    /// 金额的相等性由金额数值和货币代码共同决定。
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

/// <summary>
/// 账期值对象（Period Value Object）
///
/// DDD 角色：值对象（Value Object），表示会计/业务期间，格式为 "yyyy-MM"（如 "2026-06"）。
/// 用于标识费用、收款、应收等业务数据所属的月份期间，是租赁计费系统的核心时间维度。
///
/// 业务含义：
/// - 定义费用归属的会计月份，如 2026 年 6 月的租金属于账期 "2026-06"
/// - 支持账期的前后滚动（Next / Previous），用于生成连续的计费计划
/// - 提供当月的起止日期（StartDate / EndDate），方便生成账单及逾期计算
///
/// 隐式转换：
/// - string ↔ Period：允许直接使用 "2026-06" 字符串与 Period 对象互转
/// - 隐式转换使 Dapper ORM 能自动将 varchar 字段映射为 Period 类型
///
/// 校验规则：
/// - 年份范围：2000~2100（合理业务范围）
/// - 月份范围：1~12
/// </summary>
public sealed class Period : ValueObject
{
    /// <summary>
    /// 年份（四位数字，2000~2100）
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// 月份（1~12）
    /// </summary>
    public int Month { get; }

    /// <summary>
    /// 私有无参构造，仅供序列化/ORM 反序列化使用
    /// </summary>
    private Period() { }

    /// <summary>
    /// 创建指定年月的账期值对象
    /// </summary>
    /// <param name="year">年份（2000~2100）</param>
    /// <param name="month">月份（1~12）</param>
    /// <exception cref="ArgumentOutOfRangeException">year 或 month 超出有效范围时抛出</exception>
    public Period(int year, int month)
    {
        if (year < 2000 || year > 2100) throw new ArgumentOutOfRangeException(nameof(year));
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));
        Year = year;
        Month = month;
    }

    /// <summary>
    /// 从 "yyyy-MM" 格式的字符串解析为 Period 值对象
    /// 支持在前端传参、API 路由、导入文件等场景中将字符串转换为强类型账期。
    /// </summary>
    /// <param name="value">格式为 "yyyy-MM" 的字符串，如 "2026-06"</param>
    /// <returns>解析后的 Period 对象</returns>
    /// <exception cref="FormatException">字符串格式不正确（不含分隔符或分段数不为 2）时抛出</exception>
    public static Period Parse(string value)
    {
        var parts = value.Split('-');
        if (parts.Length != 2) throw new FormatException($"无效账期格式: {value}");
        return new Period(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    /// <summary>
    /// 格式化为 "yyyy-MM" 格式字符串
    /// 如 Period(2026, 6).ToString() 返回 "2026-06"。
    /// 月份始终占两位，不足两位时补零（如 "2026-01"）。
    /// </summary>
    public override string ToString() => $"{Year:D4}-{Month:D2}";

    public static implicit operator string(Period p) => p.ToString();
    public static implicit operator Period(string s) => Parse(s);

    /// <summary>
    /// 获取下一个月份账期
    /// 用于顺序生成后续计费月计划，如 2026-12 的下一个月是 2027-01。
    /// </summary>
    /// <returns>下一个月对应的 Period 值对象</returns>
    public Period Next() => Month == 12 ? new Period(Year + 1, 1) : new Period(Year, Month + 1);

    /// <summary>
    /// 获取上一个月份账期
    /// 用于回溯到前一个计费月，如 2026-01 的上一个月是 2025-12。
    /// </summary>
    /// <returns>上一个月对应的 Period 值对象</returns>
    public Period Previous() => Month == 1 ? new Period(Year - 1, 12) : new Period(Year, Month - 1);

    /// <summary>
    /// 获取当月的第一天日期
    /// 用于生成应收账单的开始日期、逾期计算的起始基准。
    /// </summary>
    public DateTime StartDate => new(Year, Month, 1);

    /// <summary>
    /// 获取当月的最后一天日期
    /// 自动处理闰年和大/小月差异（如 2 月 28/29 天、4 月 30 天等）。
    /// 用于计算应收账单的结束日期、逾期天数计算的截止日。
    /// </summary>
    public DateTime EndDate => new(Year, Month, DateTime.DaysInMonth(Year, Month));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Year;
        yield return Month;
    }
}

/// <summary>
/// 合同状态枚举值对象（Contract Status Value Object）
///
/// DDD 角色：值对象（Value Object），替代传统枚举来承载合同状态及状态机迁移规则。
/// 将状态、显示名、合法迁移规则封装在一起，避免状态逻辑散落在各处。
///
/// 状态流（State Machine）：
///   Draft（草稿） → PendingApproval（待审批） → Active（生效中） → Expired（已到期）
///                                                           → Terminated（已终止）
///                                                           → Suspended（已暂停） → Active / Expired / Terminated
///                                                           → Renewed（已续签）
///                                          ← Draft（退回重填）
///   Expired（已到期） → Renewed（已续签）
///
/// 设计要点：
/// - 使用隐式转换 string ↔ ContractStatus，使 Dapper ORM 能自动映射字段
/// - CanTransitionTo 方法集中管理状态迁移规则，防止非法状态跃迁
/// - 与数据库存储的字段对应的是 Code 属性（字符串），而非枚举序号
/// </summary>
public sealed class ContractStatus : ValueObject
{
    /// <summary>草稿 — 合同初始状态，尚未提交审批</summary>
    public static readonly ContractStatus Draft = new("Draft", "草稿");
    /// <summary>待审批 — 合同已提交，等待审批流程处理</summary>
    public static readonly ContractStatus PendingApproval = new("PendingApproval", "待审批");
    /// <summary>生效中 — 合同审批通过，开始计费并产生应收</summary>
    public static readonly ContractStatus Active = new("Active", "生效中");
    /// <summary>已暂停 — 合同因故暂停（如房屋维修、租户暂停），暂停期间不计费</summary>
    public static readonly ContractStatus Suspended = new("Suspended", "已暂停");
    /// <summary>已到期 — 合同按约定期限自然到期，不再产生新的应收</summary>
    public static readonly ContractStatus Expired = new("Expired", "已到期");
    /// <summary>已终止 — 合同在到期前被提前解除</summary>
    public static readonly ContractStatus Terminated = new("Terminated", "已终止");
    /// <summary>已续签 — 合同在原到期后续签新合同</summary>
    public static readonly ContractStatus Renewed = new("Renewed", "已续签");

    /// <summary>
    /// 状态代码（英文标识，用于数据库持久化和 API 传参）
    /// 取值：Draft / PendingApproval / Active / Suspended / Expired / Terminated / Renewed
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 状态中文显示名称（用于前端 UI 展示）
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 私有无参构造，仅供序列化/ORM 反序列化使用
    /// 默认状态为 Draft（草稿）。
    /// </summary>
    private ContractStatus() { Code = "Draft"; DisplayName = "草稿"; }

    /// <summary>
    /// 创建合同状态值对象
    /// </summary>
    /// <param name="code">状态代码（英文标识）</param>
    /// <param name="displayName">状态中文名称</param>
    private ContractStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>
    /// 隐式转换为字符串（返回 Code），方便 Dapper ORM 映射和与字符串字面量比较
    /// 如 if (contract.Status == "Active") 可直接使用。
    /// </summary>
    public static implicit operator string(ContractStatus s) => s.Code;
    /// <summary>
    /// 隐式从字符串创建 ContractStatus（自动匹配 Code）
    /// 如 ContractStatus status = "Active" 等价于 ContractStatus.FromCode("Active")
    /// </summary>
    public static implicit operator ContractStatus(string code) => FromCode(code);
    /// <summary>
    /// 返回状态代码字符串
    /// </summary>
    public override string ToString() => Code;

    /// <summary>
    /// 判断当前状态是否可以迁移到目标状态
    /// 根据预定义的状态机迁移规则表 _validTransitions 进行校验。
    /// 非法迁移会返回 false，由调用方决定如何响应（抛出异常或返回错误信息）。
    /// </summary>
    /// <param name="target">目标状态</param>
    /// <returns>如果可以迁移则返回 true，否则 false</returns>
    public bool CanTransitionTo(ContractStatus target) => (_validTransitions[this] ?? Array.Empty<ContractStatus>()).Contains(target);

    /// <summary>
    /// 状态迁移规则表
    /// 定义每个状态允许转移到哪些目标状态。
    /// 未在此表中定义的状态（如 Renewed）不可再转移到其他状态。
    /// </summary>
    private static readonly Dictionary<ContractStatus, ContractStatus[]> _validTransitions = new()
    {
        [Draft] = new[] { PendingApproval },
        [PendingApproval] = new[] { Active, Draft },
        [Active] = new[] { Suspended, Expired, Terminated, Renewed },
        [Suspended] = new[] { Active, Expired, Terminated },
        [Expired] = new[] { Renewed },
    };

    /// <summary>
    /// 根据状态代码字符串查找对应的 ContractStatus 实例
    /// 如果未找到匹配的状态，则返回 Draft 作为默认值（容错处理）。
    /// </summary>
    /// <param name="code">状态代码（不区分大小写敏感，实际使用时应统一大小写）</param>
    /// <returns>匹配的 ContractStatus 实例，未匹配时返回 Draft</returns>
    public static ContractStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Draft;

    /// <summary>
    /// 所有合同状态的静态集合，用于 FromCode 查找
    /// </summary>
    private static readonly ContractStatus[] _all = { Draft, PendingApproval, Active, Suspended, Expired, Terminated, Renewed };

    /// <summary>
    /// 值对象相等性比较分量：仅使用 Code 作为比较依据
    /// 两个 ContractStatus 如果 Code 相同则视为相等。
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}

/// <summary>
/// 收款状态枚举值对象（Receipt Status Value Object）
///
/// DDD 角色：值对象（Value Object），定义收款单的生命周期状态。
///
/// 状态流转：
///   Pending（待确认） → Confirmed（已确认） → 收款完成，更新应收
///                     → Rejected（已驳回） → 退回，需修改后重新提交
///                     → Cancelled（已取消） → 该笔收款作废
///
/// 业务含义：
/// - 收款单创建后默认为 Pending（待确认），等待财务审核确认
/// - Confirmed 表示款项已到账，系统将自动更新对应应收计划的已收金额
/// - Rejected 表示财务审核不通过，需退回修改
/// - Cancelled 表示该笔收款被取消（删除），不影响应收数据
/// </summary>
public sealed class ReceiptStatus : ValueObject
{
    /// <summary>待确认 — 收款单已创建，等待财务确认到账</summary>
    public static readonly ReceiptStatus Pending = new("Pending", "待确认");
    /// <summary>已确认 — 财务已确认收款到账，自动更新应收已收金额</summary>
    public static readonly ReceiptStatus Confirmed = new("Confirmed", "已确认");
    /// <summary>已驳回 — 财务审核不通过，退回修改</summary>
    public static readonly ReceiptStatus Rejected = new("Rejected", "已驳回");
    /// <summary>已取消 — 该笔收款作废/删除</summary>
    public static readonly ReceiptStatus Cancelled = new("Cancelled", "已取消");

    /// <summary>
    /// 状态代码（英文标识，用于数据库持久化和 API 传参）
    /// 取值：Pending / Confirmed / Rejected / Cancelled
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 状态中文显示名称（用于前端 UI 展示）
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 私有无参构造，仅供序列化/ORM 反序列化使用
    /// 默认状态为 Pending（待确认）。
    /// </summary>
    private ReceiptStatus() { Code = "Pending"; DisplayName = "待确认"; }

    /// <summary>
    /// 创建收款状态值对象
    /// </summary>
    /// <param name="code">状态代码</param>
    /// <param name="displayName">状态中文名称</param>
    private ReceiptStatus(string code, string displayName) { Code = code; DisplayName = displayName; }

    /// <summary>
    /// 根据状态代码查找对应的 ReceiptStatus 实例，未匹配时默认返回 Pending
    /// </summary>
    public static ReceiptStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Pending;
    /// <summary>隐式转换为字符串（返回 Code）</summary>
    public static implicit operator string(ReceiptStatus s) => s.Code;
    /// <summary>隐式从字符串创建 ReceiptStatus</summary>
    public static implicit operator ReceiptStatus(string code) => FromCode(code);
    /// <summary>返回状态代码字符串</summary>
    public override string ToString() => Code;
    /// <summary>所有收款状态的静态集合</summary>
    private static readonly ReceiptStatus[] _all = { Pending, Confirmed, Rejected, Cancelled };

    /// <summary>值对象相等性比较分量：仅使用 Code</summary>
    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}

/// <summary>
/// 应收状态枚举值对象（Receivable Status Value Object）
///
/// DDD 角色：值对象（Value Object），定义应收计划单的生命周期状态。
/// 应收计划按月生成，代表该月应收取的费用，其状态反映费用回收进度。
///
/// 状态流转：
///   Pending（待收款） → Partial（部分收款） → Paid（已结清）
///                    → Overdue（已逾期）     → Partial/Paid（收到款项后）
///                    → Cancelled（已取消）   → 该笔应收作废
///
/// 业务含义：
/// - Pending：应收计划已生成，尚未收到任何款项
/// - Partial：已收到部分款项，但未达到应收总额
/// - Paid：应收金额已全部收齐，该月费用已结清
/// - Overdue：超过约定付款日仍未结清，触发逾期处理流程
/// - Cancelled：该笔应收因合同终止或费用调整等原因被取消
/// </summary>
public sealed class ReceivableStatus : ValueObject
{
    /// <summary>待收款 — 应收计划已生成，尚未收到款项</summary>
    public static readonly ReceivableStatus Pending = new("Pending", "待收款");
    /// <summary>部分收款 — 已收到部分款项，剩余未付</summary>
    public static readonly ReceivableStatus Partial = new("Partial", "部分收款");
    /// <summary>已结清 — 应收金额已全部收齐</summary>
    public static readonly ReceivableStatus Paid = new("Paid", "已结清");
    /// <summary>已逾期 — 超过付款日仍未结清，需进行催收或利息计算</summary>
    public static readonly ReceivableStatus Overdue = new("Overdue", "已逾期");
    /// <summary>已取消 — 该笔应收计划已作废（合同终止或费用调整）</summary>
    public static readonly ReceivableStatus Cancelled = new("Cancelled", "已取消");

    /// <summary>状态代码（英文标识，用于数据库持久化和 API 传参）</summary>
    public string Code { get; }
    /// <summary>状态中文显示名称（用于前端 UI 展示）</summary>
    public string DisplayName { get; }

    /// <summary>私有无参构造，仅供序列化/ORM 反序列化使用，默认 Pending</summary>
    private ReceivableStatus() { Code = "Pending"; DisplayName = "待收款"; }
    /// <summary>创建应收状态值对象</summary>
    private ReceivableStatus(string code, string displayName) { Code = code; DisplayName = displayName; }

    /// <summary>根据状态代码查找对应的 ReceivableStatus 实例，未匹配时默认返回 Pending</summary>
    public static ReceivableStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Pending;
    /// <summary>隐式转换为字符串（返回 Code）</summary>
    public static implicit operator string(ReceivableStatus s) => s.Code;
    /// <summary>隐式从字符串创建 ReceivableStatus</summary>
    public static implicit operator ReceivableStatus(string code) => FromCode(code);
    /// <summary>返回状态代码字符串</summary>
    public override string ToString() => Code;
    /// <summary>所有应收状态的静态集合</summary>
    private static readonly ReceivableStatus[] _all = { Pending, Partial, Paid, Overdue, Cancelled };

    /// <summary>值对象相等性比较分量：仅使用 Code</summary>
    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}

/// <summary>
/// 房屋状态枚举值对象（Room Status Value Object）
///
/// DDD 角色：值对象（Value Object），定义物理房源的当前使用状态。
///
/// 状态含义：
/// - Vacant（空置）：房源可出租，等待签订合同
/// - Rented（已租）：房源已出租，处于合同期内
/// - Maintenance（维修中）：房源暂停出租，正在进行维修或装修
///
/// 业务影响：
/// - 只有 Vacant 状态的房源可以签订新合同
/// - 合同生效后，房源自动变为 Rented
/// - 合同到期/终止后，房源应恢复为 Vacant
/// - Maintenance 状态可在任意非 Rented 状态下手动设置
/// </summary>
public sealed class RoomStatus : ValueObject
{
    /// <summary>空置 — 房源可出租，等待签订新合同</summary>
    public static readonly RoomStatus Vacant = new("Vacant", "空置");
    /// <summary>已租 — 房源正在出租中，处于合同有效期内</summary>
    public static readonly RoomStatus Rented = new("Rented", "已租");
    /// <summary>维修中 — 房源暂停出租，正在进行维修或装修</summary>
    public static readonly RoomStatus Maintenance = new("Maintenance", "维修中");

    /// <summary>状态代码（英文标识，用于数据库持久化和 API 传参）</summary>
    public string Code { get; }
    /// <summary>状态中文显示名称（用于前端 UI 展示）</summary>
    public string DisplayName { get; }

    /// <summary>私有无参构造，仅供序列化/ORM 反序列化使用，默认 Vacant</summary>
    private RoomStatus() { Code = "Vacant"; DisplayName = "空置"; }
    /// <summary>创建房屋状态值对象</summary>
    private RoomStatus(string code, string displayName) { Code = code; DisplayName = displayName; }

    /// <summary>根据状态代码查找对应的 RoomStatus 实例，未匹配时默认返回 Vacant</summary>
    public static RoomStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Vacant;
    /// <summary>所有房屋状态的静态集合</summary>
    private static readonly RoomStatus[] _all = { Vacant, Rented, Maintenance };

    /// <summary>值对象相等性比较分量：仅使用 Code</summary>
    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}

/// <summary>
/// 审批状态枚举值对象（Approval Status Value Object）
///
/// DDD 角色：值对象（Value Object），定义审批请求的生命周期状态。
/// 适用于合同审批、费用变更审批等各种审批流程。
///
/// 状态流转：
///   Pending（待审批） → Approved（已通过） → 完成审批，触发后续业务动作
///                    → Rejected（已驳回） → 退回申请，申请人可重新提交
///                    → Cancelled（已撤销） → 申请人主动取消申请
///
/// 多级审批场景：
/// - 在审批流程有多级时，每一次审核人完成审核后可能触发 ApprovalLevelAdvancedEvent
/// - 直到最后一级审核通过，才标记为 Approved
/// - 任意一级审核不通过即标记为 Rejected
/// </summary>
public sealed class ApprovalStatus : ValueObject
{
    /// <summary>待审批 — 审批请求已提交，等待审批人处理</summary>
    public static readonly ApprovalStatus Pending = new("Pending", "待审批");
    /// <summary>已通过 — 全部审批级别已完成，审批通过</summary>
    public static readonly ApprovalStatus Approved = new("Approved", "已通过");
    /// <summary>已驳回 — 审批不通过，退回给申请人</summary>
    public static readonly ApprovalStatus Rejected = new("Rejected", "已驳回");
    /// <summary>已撤销 — 申请人主动取消该审批请求</summary>
    public static readonly ApprovalStatus Cancelled = new("Cancelled", "已撤销");

    /// <summary>状态代码（英文标识，用于数据库持久化和 API 传参）</summary>
    public string Code { get; }
    /// <summary>状态中文显示名称（用于前端 UI 展示）</summary>
    public string DisplayName { get; }

    /// <summary>私有无参构造，仅供序列化/ORM 反序列化使用，默认 Pending</summary>
    private ApprovalStatus() { Code = "Pending"; DisplayName = "待审批"; }
    /// <summary>创建审批状态值对象</summary>
    private ApprovalStatus(string code, string displayName) { Code = code; DisplayName = displayName; }

    /// <summary>根据状态代码查找对应的 ApprovalStatus 实例，未匹配时默认返回 Pending</summary>
    public static ApprovalStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Pending;
    /// <summary>所有审批状态的静态集合</summary>
    private static readonly ApprovalStatus[] _all = { Pending, Approved, Rejected, Cancelled };

    /// <summary>值对象相等性比较分量：仅使用 Code</summary>
    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}

/// <summary>
/// 计费模式枚举值对象（Billing Mode Value Object）
///
/// DDD 角色：值对象（Value Object），定义合同中某项费用的计费方式。
///
/// 模式说明：
/// - FixedAmount（固定金额）：每月按固定金额计费，如租金固定每月 5000 元
/// - MeterBased（抄表计量）：根据抄表读数按实际用量计费，如水费、电费
///
/// 业务影响：
/// - FixedAmount：计费时直接取合同约定的固定金额
/// - MeterBased：计费时需要读取对应月份的抄表记录，用量 × 单价 = 应计金额
///   且 MeterBased 模式需要配置对应的单价和抄表计划
/// </summary>
public sealed class BillingMode : ValueObject
{
    /// <summary>固定金额 — 每月按合同约定的固定金额计费（如租金、管理费）</summary>
    public static readonly BillingMode FixedAmount = new("FixedAmount", "固定金额");
    /// <summary>抄表计量 — 根据实际抄表用量 × 单价计费（如水费、电费）</summary>
    public static readonly BillingMode MeterBased = new("MeterBased", "抄表计量");

    /// <summary>模式代码（英文标识，用于数据库持久化和 API 传参）</summary>
    public string Code { get; }
    /// <summary>模式中文显示名称（用于前端 UI 展示）</summary>
    public string DisplayName { get; }

    /// <summary>私有无参构造，仅供序列化/ORM 反序列化使用，默认 FixedAmount</summary>
    private BillingMode() { Code = "FixedAmount"; DisplayName = "固定金额"; }
    /// <summary>创建计费模式值对象</summary>
    private BillingMode(string code, string displayName) { Code = code; DisplayName = displayName; }

    /// <summary>根据模式代码查找对应的 BillingMode 实例，未匹配时默认返回 FixedAmount</summary>
    public static BillingMode FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? FixedAmount;
    /// <summary>隐式转换为字符串（返回 Code）</summary>
    public static implicit operator string(BillingMode b) => b.Code;
    /// <summary>隐式从字符串创建 BillingMode</summary>
    public static implicit operator BillingMode(string code) => FromCode(code);
    /// <summary>返回模式代码字符串</summary>
    public override string ToString() => Code;
    /// <summary>所有计费模式的静态集合</summary>
    private static readonly BillingMode[] _all = { FixedAmount, MeterBased };

    /// <summary>值对象相等性比较分量：仅使用 Code</summary>
    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}
