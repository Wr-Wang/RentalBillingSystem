using Dapper;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Base;

namespace RBS.Infrastructure.Data.TypeHandlers;

#nullable disable  // Dapper 基类 SqlMapper.TypeHandler.Parse(object) 不支持 nullable，无法更改
/// <summary>
/// Dapper 类型处理器 — 实现值对象与原始类型的自动转换
/// 避免实体中同时维护两套属性（DB原始类型 + 领域类型）
/// </summary>
/// <remarks>
/// 注册的值对象处理器：
/// <list type="bullet">
///   <item><description>MoneyHandler — Money ↔ decimal</description></item>
///   <item><description>PeriodHandler — Period ↔ string (yyyy-MM)</description></item>
///   <item><description>ContractStatusHandler — ContractStatus ↔ string</description></item>
///   <item><description>ReceiptStatusHandler — ReceiptStatus ↔ string</description></item>
///   <item><description>ReceivableStatusHandler — ReceivableStatus ↔ string</description></item>
///   <item><description>VoucherStatusHandler — VoucherStatus ↔ string</description></item>
/// </list>
/// 每个处理器继承 SqlMapper.TypeHandler&lt;T&gt;，Dapper 自动在查询/写入时调用。
/// 该机制使得领域实体可以直接使用值对象属性，无需在仓储中手动转换。
/// </remarks>
public static class ValueObjectHandlers
{
    /// <summary>
    /// 注册所有值对象类型处理器 — 在应用程序启动时调用
    /// </summary>
    public static void Register()
    {
        SqlMapper.AddTypeHandler(new MoneyHandler());
        SqlMapper.AddTypeHandler(new PeriodHandler());
        SqlMapper.AddTypeHandler(new ContractStatusHandler());
        SqlMapper.AddTypeHandler(new ReceiptStatusHandler());
        SqlMapper.AddTypeHandler(new ReceivableStatusHandler());
        SqlMapper.AddTypeHandler(new VoucherStatusHandler());
    }
}

/// <summary>凭证状态处理器 — VoucherStatus ↔ string 转换</summary>
public class VoucherStatusHandler : SqlMapper.TypeHandler<VoucherStatus>
{
    public override VoucherStatus Parse(object value) =>
        VoucherStatus.FromCode(value?.ToString() ?? "Draft");

    public override void SetValue(System.Data.IDbDataParameter parameter, VoucherStatus value)
    {
        parameter.Value = value.Code;
        parameter.DbType = System.Data.DbType.String;
    }
}

/// <summary>金额值对象处理器 — Money ↔ decimal 转换</summary>
public class MoneyHandler : SqlMapper.TypeHandler<Money>
{
    public override Money Parse(object value) =>
        new Money(Convert.ToDecimal(value));

    public override void SetValue(System.Data.IDbDataParameter parameter, Money value)
    {
        parameter.Value = value.Amount;
        parameter.DbType = System.Data.DbType.Decimal;
    }
}

/// <summary>账期值对象处理器 — Period ↔ string (yyyy-MM) 转换</summary>
public class PeriodHandler : SqlMapper.TypeHandler<Period>
{
    public override Period Parse(object value) =>
        Period.Parse((value?.ToString()) ?? "");

    public override void SetValue(System.Data.IDbDataParameter parameter, Period value)
    {
        parameter.Value = value.ToString();
        parameter.DbType = System.Data.DbType.String;
    }
}

/// <summary>合同状态值对象处理器 — ContractStatus ↔ string 转换</summary>
public class ContractStatusHandler : SqlMapper.TypeHandler<ContractStatus>
{
    public override ContractStatus Parse(object value) =>
        ContractStatus.FromCode(value?.ToString() ?? "Draft");

    public override void SetValue(System.Data.IDbDataParameter parameter, ContractStatus value)
    {
        parameter.Value = value.Code;
        parameter.DbType = System.Data.DbType.String;
    }
}

/// <summary>收款单状态值对象处理器 — ReceiptStatus ↔ string 转换</summary>
public class ReceiptStatusHandler : SqlMapper.TypeHandler<ReceiptStatus>
{
    public override ReceiptStatus Parse(object value) =>
        ReceiptStatus.FromCode(value?.ToString() ?? "Pending");

    public override void SetValue(System.Data.IDbDataParameter parameter, ReceiptStatus value)
    {
        parameter.Value = value.Code;
        parameter.DbType = System.Data.DbType.String;
    }
}

/// <summary>应收状态值对象处理器 — ReceivableStatus ↔ string 转换</summary>
public class ReceivableStatusHandler : SqlMapper.TypeHandler<ReceivableStatus>
{
    public override ReceivableStatus Parse(object value) =>
        ReceivableStatus.FromCode(value?.ToString() ?? "");

    public override void SetValue(System.Data.IDbDataParameter parameter, ReceivableStatus value)
    {
        parameter.Value = value.Code;
        parameter.DbType = System.Data.DbType.String;
    }
}
