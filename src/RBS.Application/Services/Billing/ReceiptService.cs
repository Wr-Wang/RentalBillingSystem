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
            "SELECT ReceivablePlanId, Amount FROM ReceiptAllocations WHERE ReceiptId=@Id",
            new { Id = id });

        foreach (var row in allocRows)
        {
            var plan = await _uow.ReceivablePlans.GetByIdAsync((Guid)row.ReceivablePlanId, ct);
            if (plan != null)
            {
                plan.ReversePayment((decimal)row.Amount);
            }
        }

        await conn.ExecuteAsync(_sql.Get("Lease.Delete.ReceiptAllocation.ByReceiptId"), new { Id = id });
        entity.Cancel();
        await _uow.CommitAsync(ct);

        return new { message = "冲销成功", receiptId = id };
    }
}
