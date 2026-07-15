using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Accounting;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.Services.Accounting;

/// <summary>
/// 自动凭证服务 — 收款确认时自动创建会计凭证
/// 全部操作在同一 Dapper 事务中完成，保障 Voucher + JE + PrepaidBalance 一致性
/// </summary>
public class AutoVoucherService : IAutoVoucherService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public AutoVoucherService(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    /// <summary>
    /// 收款确认后自动生成凭证（独立连接/事务）
    /// </summary>
    public async Task<Voucher?> GenerateFromReceiptAsync(Guid receiptId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var result = await GenerateFromReceiptCoreAsync(conn, tx, receiptId, ct);
            tx.Commit();
            return result;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 收款确认后自动生成凭证（共享连接/事务，与调用方同一事务）
    /// 调用方负责 Commit/Rollback
    /// </summary>
    public async Task<Voucher?> GenerateFromReceiptAsync(IDbConnection conn, IDbTransaction tx, Guid receiptId, CancellationToken ct)
    {
        return await GenerateFromReceiptCoreAsync(conn, tx, receiptId, ct);
    }

    /// <summary>
    /// 核心逻辑：查收款单 → 查科目 → 查应收余额 → 拆分 → 写入 Voucher + JE + PrepaidBalance
    /// </summary>
    private async Task<Voucher?> GenerateFromReceiptCoreAsync(IDbConnection conn, IDbTransaction tx, Guid receiptId, CancellationToken ct)
    {
        // 1. 查收款单
        var receipt = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Collection.Select.Receipt.ConfirmedById"),
            new { Id = receiptId }, tx);
        if (receipt == null) return null;

        var amount = (decimal)receipt.Amount;
        Guid? contractId = (Guid?)receipt.ContractId;

        // 2. 查会计科目
        var subjects = await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ReceiptCodes"), transaction: tx);
        var subjectDict = subjects.ToDictionary(r => r.Code, r => r.Id);

        if (!subjectDict.TryGetValue("1001", out var subject1001) ||
            !subjectDict.TryGetValue("1122", out var subject1122))
            return null;

        subjectDict.TryGetValue("2203", out var subject2203);

        // 3. 查询该合同的应收账款余额（按合同维度）
        decimal arBalance = 0;
        if (contractId.HasValue)
        {
            arBalance = await conn.QuerySingleAsync<decimal>(
                _sql.Get("Billing.Select.JournalEntry.BalanceByContract"),
                new { Code = "1122", ContractId = contractId.Value }, tx);
        }

        // 4. 拆分：offset 冲应收，overflow 进预收
        var offset = Math.Min(amount, Math.Max(0, arBalance));
        var overflow = amount - offset;

        // 5. 创建凭证
        var voucherId = Guid.NewGuid();
        var voucherNo = $"PZ-{DateTime.UtcNow:yyyyMMdd}-{receiptId:N}".Substring(0, 32);
        var now = DateTime.UtcNow;
        var period = DateOnly.FromDateTime(now).ToString("yyyy-MM");
        var companyId = (Guid)receipt.CompanyId;

        await conn.ExecuteAsync(
            _sql.Get("Accounting.Insert.Voucher.WithCompanyId"),
            new
            {
                Id = voucherId, No = voucherNo,
                Date = DateOnly.FromDateTime(now),
                Type = "Receipt", SrcId = receiptId,
                CId = contractId ?? (object)DBNull.Value,
                CoId = companyId, Period = period,
                CBy = Guid.Empty
            }, tx);

        // 6. 插入分录：借 1001 银行存款（全额）
        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
            new { Id = Guid.NewGuid(), VId = voucherId, SId = subject1001,
                Dir = "Debit", Amt = amount, Sum = $"收款 {(string)receipt.ReceiptNo}", CBy = Guid.Empty }, tx);

        // 贷 1122 应收账款（≤ 余额冲应收）
        if (offset > 0)
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                new { Id = Guid.NewGuid(), VId = voucherId, SId = subject1122,
                    Dir = "Credit", Amt = offset, Sum = "冲应收", CBy = Guid.Empty }, tx);

        // 贷 2203 预收账款（溢出部分）
        if (overflow > 0 && subject2203 != Guid.Empty)
        {
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                new { Id = Guid.NewGuid(), VId = voucherId, SId = subject2203,
                    Dir = "Credit", Amt = overflow, Sum = "溢出进预收", CBy = Guid.Empty }, tx);

            // 7. 更新合同预存金额（供 SettleJob 预收抵应收使用）
            if (contractId.HasValue)
            {
                await conn.ExecuteAsync(
                    _sql.Get("Accounting.Update.Contract.PrepaidBalanceIncrement"),
                    new { Amt = overflow, Id = contractId.Value }, tx);
            }
        }

        return new Voucher(voucherNo, DateOnly.FromDateTime(now), $"收款确认：{(string)receipt.ReceiptNo}");
    }
}
