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

        // 查找费用模板，确定会计科目
        var templates = await _uow.FeeCodeTemplates.GetAllAsync(ct);
        var receiptContractId = receipt.ContractId;
        Guid? debitSubjectId = null;
        Guid? creditSubjectId = null;

        // 尝试从 FeeCodeTemplate 获取科目
        if (receiptContractId.HasValue)
        {
            var plans = await _uow.ReceivablePlans.GetByContractIdAsync(receiptContractId.Value, ct);
            var feeCodeIds = plans.Select(p => p.FeeCodeId).Distinct().ToList();
            foreach (var fid in feeCodeIds)
            {
                var tpl = templates.FirstOrDefault(t => t.FeeCodeId == fid);
                if (tpl?.DebitSubjectId != null) debitSubjectId = tpl.DebitSubjectId;
                if (tpl?.CreditSubjectId != null) creditSubjectId = tpl.CreditSubjectId;
                if (debitSubjectId != null && creditSubjectId != null) break;
            }
        }

        // 若未配置模板科目，使用默认科目编码查找
        if (debitSubjectId == null || creditSubjectId == null)
        {
            var allSubjects = await _uow.AccountingSubjects.GetAllAsync(ct);
            if (debitSubjectId == null)
                debitSubjectId = allSubjects.FirstOrDefault(s => s.Code == "1001")?.Id; // 银行存款
            if (creditSubjectId == null)
                creditSubjectId = allSubjects.FirstOrDefault(s => s.Code == "1122")?.Id; // 应收账款
        }

        if (debitSubjectId == null || creditSubjectId == null)
            return null; // 无法确定科目，不生成凭证

        // 创建凭证
        var voucherNo = $"PZ-{DateTime.UtcNow:yyyyMMdd}-{receiptId:N}".Substring(0, 32);
        var voucher = new Voucher(voucherNo, DateOnly.FromDateTime(DateTime.UtcNow),
            $"收款确认：{receipt.ReceiptNo}");
        voucher.SetSource(receiptId, "Receipt");

        voucher.AddEntry(debitSubjectId.Value, "Debit", receipt.Amount, $"收款 {receipt.ReceiptNo}");
        voucher.AddEntry(creditSubjectId.Value, "Credit", receipt.Amount, $"收款 {receipt.ReceiptNo}");

        // 自动过账
        voucher.Post();

        await _uow.Vouchers.AddAsync(voucher, ct);

        // 手动持久化分录（Dapper 不自动保存 Voucher 的子实体 JournalEntry）
        using var conn = _db.CreateConnection(); conn.Open();
        foreach (var entry in voucher.Entries)
        {
            await conn.ExecuteAsync(
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
