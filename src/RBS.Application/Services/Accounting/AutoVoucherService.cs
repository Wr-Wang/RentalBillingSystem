using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Accounting;

public class AutoVoucherService : IAutoVoucherService
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public AutoVoucherService(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
    }

    public async Task<Voucher?> GenerateFromReceiptAsync(Guid receiptId, CancellationToken ct)
    {
        var receipt = await _uow.Receipts.GetByIdAsync(receiptId, ct);
        if (receipt == null || receipt.Status != "Confirmed")
            return null;

        var allSubjects = await _uow.AccountingSubjects.GetAllAsync(ct);
        var subject1001 = allSubjects.FirstOrDefault(s => s.Code == "1001")?.Id;
        var subject1122 = allSubjects.FirstOrDefault(s => s.Code == "1122")?.Id;
        var subject2203 = allSubjects.FirstOrDefault(s => s.Code == "2203")?.Id;

        if (subject1001 == null || subject1122 == null)
            return null;

        // 查询该合同的应收余额（通过 Dapper 直查）
        decimal arBalance = 0;
        if (receipt.ContractId.HasValue)
        {
            using var conn = _db.CreateConnection(); conn.Open();
            arBalance = await conn.QuerySingleAsync<decimal>(
                _sql.Get("Billing.Select.JournalEntry.BalanceBySubject"),
                new { Code = "1122", SrcId = receipt.ContractId.Value });
        }

        // 拆分：offset 冲应收，overflow 进预收
        var offset = Math.Min(receipt.Amount, Math.Max(0, arBalance));
        var overflow = receipt.Amount - offset;

        var voucherNo = $"PZ-{DateTime.UtcNow:yyyyMMdd}-{receiptId:N}".Substring(0, 32);
        var voucher = new Voucher(voucherNo, DateOnly.FromDateTime(DateTime.UtcNow),
            $"收款确认：{receipt.ReceiptNo}");
        voucher.SetSource(receiptId, "Receipt");

        voucher.AddEntry(subject1001.Value, "Debit", receipt.Amount, $"收款 {receipt.ReceiptNo}");
        if (offset > 0)
            voucher.AddEntry(subject1122.Value, "Credit", offset, "冲应收");
        if (overflow > 0 && subject2203 != null)
            voucher.AddEntry(subject2203.Value, "Credit", overflow, "溢出进预收");

        voucher.Post();
        await _uow.Vouchers.AddAsync(voucher, ct);

        using var conn2 = _db.CreateConnection(); conn2.Open();
        foreach (var entry in voucher.Entries)
        {
            await conn2.ExecuteAsync(
                _sql.Get("Accounting.Insert.JournalEntry.Default"),
                new
                {
                    Id = Guid.NewGuid(), VId = voucher.Id, SId = entry.AccountingSubjectId,
                    Dir = entry.Direction, Amt = entry.Amount, Sum = entry.Summary ?? "",
                    CBy = Guid.Empty, Now = DateTime.UtcNow
                });
        }

        await _uow.CommitAsync(ct);
        return voucher;
    }
}
