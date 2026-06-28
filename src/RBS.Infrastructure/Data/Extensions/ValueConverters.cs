using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Accounting;

namespace RBS.Infrastructure.Data.Extensions;

/// <summary>
/// 值对象 <-> string 的 EF Core 值转换器
/// </summary>
public static class ValueConverters
{
    public static readonly ValueConverter<ContractStatus, string> ContractStatusConverter =
        new(v => v.Code, s => ContractStatus.FromCode(s));

    public static readonly ValueConverter<ReceiptStatus, string> ReceiptStatusConverter =
        new(v => v.Code, s => ReceiptStatus.FromCode(s));

    public static readonly ValueConverter<ReceivableStatus, string> ReceivableStatusConverter =
        new(v => v.Code, s => ReceivableStatus.FromCode(s));

    public static readonly ValueConverter<RoomStatus, string> RoomStatusConverter =
        new(v => v.Code, s => RoomStatus.FromCode(s));

    public static readonly ValueConverter<ApprovalStatus, string> ApprovalStatusConverter =
        new(v => v.Code, s => ApprovalStatus.FromCode(s));

    public static readonly ValueConverter<BillingMode, string> BillingModeConverter =
        new(v => v.Code, s => BillingMode.FromCode(s));

    public static readonly ValueConverter<VoucherStatus, string> VoucherStatusConverter =
        new(v => v.Code, s => VoucherStatus.FromCode(s));
}
