using System.Data;
using RBS.Core.Entities.Accounting;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 自动凭证服务 — 收款确认时自动创建会计凭证
/// </summary>
public interface IAutoVoucherService
{
    /// <summary>根据已确认的收款自动生成凭证（独立连接/事务）</summary>
    Task<Voucher?> GenerateFromReceiptAsync(Guid receiptId, CancellationToken ct = default);

    /// <summary>根据已确认的收款自动生成凭证（共享连接/事务，与调用方同一事务）</summary>
    Task<Voucher?> GenerateFromReceiptAsync(IDbConnection conn, IDbTransaction tx, Guid receiptId, CancellationToken ct = default);
}
