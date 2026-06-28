namespace RBS.Core.Entities.Base;

/// <summary>
/// 金额值对象（确保精度和货币语义）
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { Amount = 0; Currency = "CNY"; }

    public Money(decimal amount) : this(amount, "CNY") { }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("金额不能为负数", nameof(amount));
        Amount = Math.Round(amount, 2);
        Currency = currency ?? "CNY";
    }

    public static Money Zero => new(0);
    public bool IsZero => Amount == 0;

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0) throw new InvalidOperationException("余额不足");
        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public static Money operator +(Money a, Money b) => a.Add(b);
    public static Money operator -(Money a, Money b) => a.Subtract(b);
    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
    public static bool operator >=(Money a, Money b) => a.Amount >= b.Amount;
    public static bool operator <=(Money a, Money b) => a.Amount <= b.Amount;

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"货币单位不匹配: {Currency} vs {other.Currency}");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

/// <summary>
/// 账期值对象（如 "2026-06"）
/// </summary>
public sealed class Period : ValueObject
{
    public int Year { get; }
    public int Month { get; }

    private Period() { }

    public Period(int year, int month)
    {
        if (year < 2000 || year > 2100) throw new ArgumentOutOfRangeException(nameof(year));
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));
        Year = year;
        Month = month;
    }

    /// <summary>从 "2026-06" 格式解析</summary>
    public static Period Parse(string value)
    {
        var parts = value.Split('-');
        if (parts.Length != 2) throw new FormatException($"无效账期格式: {value}");
        return new Period(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    /// <summary>格式化为 "2026-06"</summary>
    public override string ToString() => $"{Year:D4}-{Month:D2}";

    /// <summary>获取下一个月账期</summary>
    public Period Next() => Month == 12 ? new Period(Year + 1, 1) : new Period(Year, Month + 1);

    /// <summary>获取上一个月账期</summary>
    public Period Previous() => Month == 1 ? new Period(Year - 1, 12) : new Period(Year, Month - 1);

    /// <summary>获取当月的第一天</summary>
    public DateOnly StartDate => new(Year, Month, 1);

    /// <summary>获取当月的最后一天</summary>
    public DateOnly EndDate => new(Year, Month, DateTime.DaysInMonth(Year, Month));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Year;
        yield return Month;
    }
}

/// <summary>
/// 合同状态枚举值对象
/// </summary>
public sealed class ContractStatus : ValueObject
{
    public static readonly ContractStatus Draft = new("Draft", "草稿");
    public static readonly ContractStatus PendingApproval = new("PendingApproval", "待审批");
    public static readonly ContractStatus Active = new("Active", "生效中");
    public static readonly ContractStatus Suspended = new("Suspended", "已暂停");
    public static readonly ContractStatus Expired = new("Expired", "已到期");
    public static readonly ContractStatus Terminated = new("Terminated", "已终止");
    public static readonly ContractStatus Renewed = new("Renewed", "已续签");

    public string Code { get; }
    public string DisplayName { get; }

    private ContractStatus() { Code = "Draft"; DisplayName = "草稿"; }

    private ContractStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    public bool CanTransitionTo(ContractStatus target) => (_validTransitions[this] ?? Array.Empty<ContractStatus>()).Contains(target);

    private static readonly Dictionary<ContractStatus, ContractStatus[]> _validTransitions = new()
    {
        [Draft] = new[] { PendingApproval },
        [PendingApproval] = new[] { Active, Draft },
        [Active] = new[] { Suspended, Expired, Terminated, Renewed },
        [Suspended] = new[] { Active, Expired, Terminated },
        [Expired] = new[] { Renewed },
    };

    public static ContractStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Draft;

    private static readonly ContractStatus[] _all = { Draft, PendingApproval, Active, Suspended, Expired, Terminated, Renewed };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}

/// <summary>
/// 收款状态枚举值对象
/// </summary>
public sealed class ReceiptStatus : ValueObject
{
    public static readonly ReceiptStatus Pending = new("Pending", "待确认");
    public static readonly ReceiptStatus Confirmed = new("Confirmed", "已确认");
    public static readonly ReceiptStatus Rejected = new("Rejected", "已驳回");
    public static readonly ReceiptStatus Cancelled = new("Cancelled", "已取消");

    public string Code { get; }
    public string DisplayName { get; }

    private ReceiptStatus() { Code = "Pending"; DisplayName = "待确认"; }
    private ReceiptStatus(string code, string displayName) { Code = code; DisplayName = displayName; }

    public static ReceiptStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Pending;
    private static readonly ReceiptStatus[] _all = { Pending, Confirmed, Rejected, Cancelled };

    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}

/// <summary>
/// 应收状态枚举值对象
/// </summary>
public sealed class ReceivableStatus : ValueObject
{
    public static readonly ReceivableStatus Pending = new("Pending", "待收款");
    public static readonly ReceivableStatus Partial = new("Partial", "部分收款");
    public static readonly ReceivableStatus Paid = new("Paid", "已结清");
    public static readonly ReceivableStatus Overdue = new("Overdue", "已逾期");
    public static readonly ReceivableStatus Cancelled = new("Cancelled", "已取消");

    public string Code { get; }
    public string DisplayName { get; }

    private ReceivableStatus() { Code = "Pending"; DisplayName = "待收款"; }
    private ReceivableStatus(string code, string displayName) { Code = code; DisplayName = displayName; }

    public static ReceivableStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Pending;
    private static readonly ReceivableStatus[] _all = { Pending, Partial, Paid, Overdue, Cancelled };

    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}

/// <summary>
/// 房屋状态枚举值对象
/// </summary>
public sealed class RoomStatus : ValueObject
{
    public static readonly RoomStatus Vacant = new("Vacant", "空置");
    public static readonly RoomStatus Rented = new("Rented", "已租");
    public static readonly RoomStatus Maintenance = new("Maintenance", "维修中");

    public string Code { get; }
    public string DisplayName { get; }

    private RoomStatus() { Code = "Vacant"; DisplayName = "空置"; }
    private RoomStatus(string code, string displayName) { Code = code; DisplayName = displayName; }

    public static RoomStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Vacant;
    private static readonly RoomStatus[] _all = { Vacant, Rented, Maintenance };

    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}

/// <summary>
/// 审批状态枚举值对象
/// </summary>
public sealed class ApprovalStatus : ValueObject
{
    public static readonly ApprovalStatus Pending = new("Pending", "待审批");
    public static readonly ApprovalStatus Approved = new("Approved", "已通过");
    public static readonly ApprovalStatus Rejected = new("Rejected", "已驳回");
    public static readonly ApprovalStatus Cancelled = new("Cancelled", "已撤销");

    public string Code { get; }
    public string DisplayName { get; }

    private ApprovalStatus() { Code = "Pending"; DisplayName = "待审批"; }
    private ApprovalStatus(string code, string displayName) { Code = code; DisplayName = displayName; }

    public static ApprovalStatus FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? Pending;
    private static readonly ApprovalStatus[] _all = { Pending, Approved, Rejected, Cancelled };

    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}

/// <summary>
/// 计费模式值对象
/// </summary>
public sealed class BillingMode : ValueObject
{
    public static readonly BillingMode FixedAmount = new("FixedAmount", "固定金额");
    public static readonly BillingMode MeterBased = new("MeterBased", "抄表计量");

    public string Code { get; }
    public string DisplayName { get; }

    private BillingMode() { Code = "FixedAmount"; DisplayName = "固定金额"; }
    private BillingMode(string code, string displayName) { Code = code; DisplayName = displayName; }

    public static BillingMode FromCode(string code) => _all.FirstOrDefault(s => s.Code == code) ?? FixedAmount;
    private static readonly BillingMode[] _all = { FixedAmount, MeterBased };

    protected override IEnumerable<object> GetEqualityComponents() { yield return Code; }
}
