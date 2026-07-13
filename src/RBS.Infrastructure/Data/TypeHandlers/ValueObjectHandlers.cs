using Dapper;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Base;

namespace RBS.Infrastructure.Data.TypeHandlers;

#nullable disable  // Dapper 基类 SqlMapper.TypeHandler.Parse(object) 不支持 nullable，无法更改
/// <summary>
/// Dapper 类型处理器 — 实现值对象与原始类型的自动转换
/// 避免实体中同时维护两套属性（DB原始类型 + 领域类型）
/// </summary>
public static class ValueObjectHandlers
{
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
