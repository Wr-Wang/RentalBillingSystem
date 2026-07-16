using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Common;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Billing;

public class ReceiptService : IReceiptService
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public ReceiptService(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
    }

    public async Task<object> BatchConfirmAsync(List<Guid> ids, CancellationToken ct)
    {
        int count = 0;
        foreach (var id in ids)
        {
            try
            {
                var entity = await _uow.Receipts.GetByIdAsync(id, ct);
                if (entity != null && entity.Status == "Pending")
                {
                    entity.Confirm(Guid.Empty);
                    count++;
                }
            }
            catch { /* 单个失败不影响其余 */ }
        }
        await _uow.CommitAsync(ct);
        return new { confirmed = count };
    }

    public async Task<object> ConfirmReceiptAsync(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            // 1. 乐观锁更新收款单状态（仅 Pending 可确认）
            var updated = await conn.ExecuteAsync(
                _sql.Get("Collection.Update.Receipt.Confirm"),
                new { Id = id }, tx);

            if (updated == 0)
            {
                var receipt = await conn.QuerySingleOrDefaultAsync<dynamic>(
                    _sql.Get("Collection.Select.Receipt.StatusById"), new { Id = id }, tx);
                if (receipt == null) throw new KeyNotFoundException("收款单不存在");
                throw new InvalidOperationException($"收款单状态为「{(string)receipt.Status}」，仅待确认状态可确认");
            }

            // 2. 更新合同欠款/预存余额
            var receiptInfo = await conn.QuerySingleAsync<dynamic>(
                _sql.Get("Receipt.Select.Receipt.WithContractBalance"),
                new { Id = id }, tx);
            if (receiptInfo != null)
            {
                var rContractId = (Guid?)receiptInfo.ContractId;
                if (rContractId == null) { tx.Commit(); return new { id }; }
                var cId = rContractId.Value;
                var amt = (decimal)receiptInfo.Amount;
                var outstanding = (decimal?)receiptInfo.OutstandingBalance ?? 0m;
                var offset = Math.Min(amt, outstanding); // 先冲欠款
                var overflow = amt - offset;              // 超出进预存
                if (offset > 0)
                    await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                        new { Id = cId, Amt = -offset }, tx);
                if (overflow > 0)
                    await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.PrepaidBalanceIncrement"),
                        new { Id = cId, Amt = overflow }, tx);
            }

            tx.Commit();
            return new { id };
        }
        catch (Exception) when (tx is not null)
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<object> ReverseAsync(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Receipts.GetByIdAsync(id, ct);
        if (entity == null) throw new KeyNotFoundException("收款不存在");

        using var conn = _db.CreateConnection(); conn.Open();
        var allocRows = await conn.QueryAsync(
            _sql.Get("Billing.Select.ReceiptAllocation.ByReceiptId"),
            new { Id = id });

        foreach (var row in allocRows)
        {
            var journal = await _uow.Journals.GetByIdAsync((Guid)row.JournalId, ct);
            if (journal != null)
            {
                // Journal 为不可变记录，冲销通过创建负数金额的 Journal 实现
                var reverseEntry = new RBS.Core.Entities.Billing.Journal(
                    journal.CompanyId, journal.ContractId, journal.FeeCodeId,
                    journal.FeeConfigId, journal.AccountingSubjectId,
                    journal.Period, -(decimal)row.Amount, journal.DueDate,
                    "Adjustment", ChinaTime.Now, null, journal.Id, "收款冲销");
                await _uow.Journals.AddAsync(reverseEntry, ct);
            }
        }

        await conn.ExecuteAsync(_sql.Get("Lease.Delete.ReceiptAllocation.ByReceiptId"), new { Id = id });
        entity.Cancel();
        await _uow.CommitAsync(ct);

        return new { message = "冲销成功", receiptId = id };
    }
}
