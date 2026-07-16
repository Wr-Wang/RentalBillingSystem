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

                // 3. 写 GL 分录
                var receiptFull = await conn.QuerySingleAsync<dynamic>(
                    _sql.Get("Receipt.Select.Receipt.WithContractInfo"),
                    new { Id = id }, tx);
                if (receiptFull != null)
                {
                    var companyId = (Guid)receiptFull.CompanyId;
                    var period = ((string)receiptFull.ReceivedDate)[..7];
                    var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
                        _sql.Get("Accounting.Select.Subject.ByCodes"), tx)).ToDictionary(r => r.Code, r => r.Id);

                    if (subjects.ContainsKey("1002"))
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = companyId, CId = cId,
                                CNo = (string)receiptFull.ContractNo ?? "", Period = period,
                                SId = subjects["1002"], SCode = "1002", Dir = "Debit",
                                Amt = amt, SrcType = "Receipt", SrcId = id,
                                Desc = (string)receiptFull.ReceiptNo, CBy = Guid.Empty }, tx);
                    if (offset > 0 && subjects.ContainsKey("1122"))
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = companyId, CId = cId,
                                CNo = (string)receiptFull.ContractNo ?? "", Period = period,
                                SId = subjects["1122"], SCode = "1122", Dir = "Credit",
                                Amt = offset, SrcType = "Receipt", SrcId = id,
                                Desc = (string)receiptFull.ReceiptNo, CBy = Guid.Empty }, tx);
                    if (overflow > 0 && subjects.ContainsKey("2203"))
                        await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                            new { Id = Guid.NewGuid(), CoId = companyId, CId = cId,
                                CNo = (string)receiptFull.ContractNo ?? "", Period = period,
                                SId = subjects["2203"], SCode = "2203", Dir = "Credit",
                                Amt = overflow, SrcType = "Receipt", SrcId = id,
                                Desc = (string)receiptFull.ReceiptNo, CBy = Guid.Empty }, tx);
                }
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
        using var tx = conn.BeginTransaction();

        var receiptFull = await conn.QuerySingleAsync<dynamic>(
            _sql.Get("Receipt.Select.Receipt.WithContractInfo"),
            new { Id = id }, tx);
        if (receiptFull == null) throw new KeyNotFoundException("收款信息不存在");

        var companyId = (Guid)receiptFull.CompanyId;
        var period = ((string)receiptFull.ReceivedDate)[..7];
        var cId = (Guid)receiptFull.ContractId;
        var amt = (decimal)receiptFull.Amount;

        // 从 GL 分录反查 offset/overflow
        var entries = await conn.QueryAsync<dynamic>(
            "SELECT Direction, Amount, SubjectCode FROM GeneralLedgerEntries WHERE SourceType='Receipt' AND SourceId=@Id",
            new { Id = id }, tx);
        decimal offset = 0, overflow = 0;
        foreach (var e in entries)
        {
            if ((string)e.SubjectCode == "1122" && (string)e.Direction == "Credit")
                offset = (decimal)e.Amount;
            if ((string)e.SubjectCode == "2203" && (string)e.Direction == "Credit")
                overflow = (decimal)e.Amount;
        }

        var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes"), tx)).ToDictionary(r => r.Code, r => r.Id);

        // 反向 GL 分录
        if (subjects.ContainsKey("1002"))
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                new { Id = Guid.NewGuid(), CoId = companyId, CId = cId,
                    CNo = (string)receiptFull.ContractNo ?? "", Period = period,
                    SId = subjects["1002"], SCode = "1002", Dir = "Credit",
                    Amt = amt, SrcType = "Reverse", SrcId = id,
                    Desc = (string)receiptFull.ReceiptNo, CBy = Guid.Empty }, tx);
        if (offset > 0 && subjects.ContainsKey("1122"))
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                new { Id = Guid.NewGuid(), CoId = companyId, CId = cId,
                    CNo = (string)receiptFull.ContractNo ?? "", Period = period,
                    SId = subjects["1122"], SCode = "1122", Dir = "Debit",
                    Amt = offset, SrcType = "Reverse", SrcId = id,
                    Desc = (string)receiptFull.ReceiptNo, CBy = Guid.Empty }, tx);
        if (overflow > 0 && subjects.ContainsKey("2203"))
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.GL.Entry"),
                new { Id = Guid.NewGuid(), CoId = companyId, CId = cId,
                    CNo = (string)receiptFull.ContractNo ?? "", Period = period,
                    SId = subjects["2203"], SCode = "2203", Dir = "Debit",
                    Amt = overflow, SrcType = "Reverse", SrcId = id,
                    Desc = (string)receiptFull.ReceiptNo, CBy = Guid.Empty }, tx);

        // 恢复合同余额
        if (offset > 0)
            await conn.ExecuteAsync(_sql.Get("Contract.Update.Contract.OutstandingBalanceIncrement"),
                new { Id = cId, Amt = offset }, tx);
        if (overflow > 0)
            await conn.ExecuteAsync(_sql.Get("Accounting.Update.Contract.PrepaidBalanceDecrement"),
                new { Id = cId, Amt = overflow }, tx);

        entity.Cancel();
        await _uow.Receipts.UpdateAsync(entity, ct);
        tx.Commit();

        return new { message = "冲销成功", receiptId = id };
    }
}
