using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
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
                    "Adjustment", DateTime.UtcNow, null, journal.Id, "收款冲销");
                await _uow.Journals.AddAsync(reverseEntry, ct);
            }
        }

        await conn.ExecuteAsync(_sql.Get("Lease.Delete.ReceiptAllocation.ByReceiptId"), new { Id = id });
        entity.Cancel();
        await _uow.CommitAsync(ct);

        return new { message = "冲销成功", receiptId = id };
    }
}
