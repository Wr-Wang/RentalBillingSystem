using RBS.Core.Entities.Accounting;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 自动凭证服务 — 收款确认时自动创建会计凭证
/// </summary>
public interface IAutoVoucherService
{
    /// <summary>根据已确认的收款自动生成凭证</summary>
    Task<Voucher?> GenerateFromReceiptAsync(Guid receiptId, CancellationToken ct = default);
}
