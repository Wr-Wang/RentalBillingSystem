using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Scheduling;

/// <summary>
/// 📣 已废弃 — 请使用 AutoVoucherService 替代
///
/// 本类的逻辑（AR 余额查询 + 溢出拆到 2203 + PrepaidBalance 更新）
/// 已合并到 AutoVoucherService.GenerateFromReceiptAsync()。
/// AutoVoucherService 由 ReceiptsController.Confirm 触发，且使用统一的 Dapper 事务。
///
/// 保留此文件仅作参考，不再注入任何 Controller。
/// 移除时间：确认 AutoVoucherService 线上运行稳定后即可删除。
/// </summary>
[Obsolete("已由 AutoVoucherService 替代", false)]
public class ReceiptJob
{
    private readonly ITaskLogRepository _taskLogRepo;
    private readonly ITaskStepLogger _stepLogger;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IUnitOfWork _uow;

    public ReceiptJob(
        ITaskLogRepository taskLogRepo,
        ITaskStepLogger stepLogger,
        IDbConnectionFactory db,
        ISqlLoader sql,
        IUnitOfWork uow)
    {
        _taskLogRepo = taskLogRepo;
        _stepLogger = stepLogger;
        _db = db;
        _sql = sql;
        _uow = uow;
    }

    public async Task ProcessAsync(Guid receiptId, Guid companyId, CancellationToken ct)
    {
        var taskLogId = await BeginTaskLogAsync("ReceiptJob", companyId, "", ct);
        var subjects = await LoadSubjectsAsync(ct);

        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var step01 = await _stepLogger.StartStepAsync(taskLogId, "ReceiptStep01", "查询收款单", null, null, ct);
            var receipt = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT Id, ContractId, Amount, ReceiptNo FROM Receipts WHERE Id=@Id AND Status='Confirmed'",
                new { Id = receiptId }, tx);
            if (receipt == null) { await _stepLogger.FailStepAsync(step01, "收款单不存在或未确认", null, ct); return; }
            await _stepLogger.CompleteStepAsync(step01, 1, null, ct);

            var step02 = await _stepLogger.StartStepAsync(taskLogId, "ReceiptStep02", "生成分录", null, null, ct);
            var amount = (decimal)receipt.Amount;
            Guid? contractId = (Guid?)receipt.ContractId;

            // 查询应收账款余额
            decimal receivableBalance = 0;
            if (contractId.HasValue)
            {
                receivableBalance = await conn.QuerySingleAsync<decimal>(
                    _sql.Get("Billing.Select.JournalEntry.BalanceBySubject"),
                    new { Code = "1122", SrcId = contractId.Value }, tx);
            }

            var vid = Guid.NewGuid();
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.Voucher.BillJob"),
                new { Id = vid, No = $"RC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 32),
                    Date = DateOnly.FromDateTime(DateTime.UtcNow), Desc = $"收款确认 {receipt.ReceiptNo}",
                    SrcId = receiptId, Type = "Receipt", Period = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM"), CBy = Guid.Empty }, tx);

            var offset = Math.Min(amount, receivableBalance > 0 ? receivableBalance : amount);
            var overflow = amount - offset;

            // 借：银行存款
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                new { Id = Guid.NewGuid(), VId = vid, SId = subjects["1001"],
                    Dir = "Debit", Amt = amount, Sum = $"收款 {receipt.ReceiptNo}", CBy = Guid.Empty }, tx);
            // 贷：应收账款（≤余额）
            if (offset > 0)
            {
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = vid, SId = subjects["1122"],
                        Dir = "Credit", Amt = offset, Sum = "冲应收", CBy = Guid.Empty }, tx);
            }
            // 溢出部分 → 预收账款（同时增加合同预存金额，供 SettleJob 抵扣使用）
            if (overflow > 0)
            {
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = vid, SId = subjects["2203"],
                        Dir = "Credit", Amt = overflow, Sum = "溢出进预收", CBy = Guid.Empty }, tx);
                if (contractId.HasValue)
                {
                    await conn.ExecuteAsync(
                        "UPDATE Contracts SET PrepaidBalance = PrepaidBalance + @Amt WHERE Id = @Id",
                        new { Amt = overflow, Id = contractId.Value }, tx);
                }
            }
            await _stepLogger.CompleteStepAsync(step02, 1, null, ct);

            tx.Commit();
            await _taskLogRepo.CompleteAsync(taskLogId, 1, 1, 0, 0, "收款分录已完成", ct);
        }
        catch
        {
            tx.Rollback();
            await _taskLogRepo.FailAsync(taskLogId, "收款分录生成失败", ct);
            throw;
        }
    }

    private async Task<Guid> BeginTaskLogAsync(string taskName, Guid companyId, string targetMonth, CancellationToken ct)
    {
        var taskLog = new RBS.Core.Entities.Scheduling.TaskLog(taskName, companyId, targetMonth, "Event", "Execute");
        await _taskLogRepo.CreateAsync(taskLog, ct);
        _ = HeartbeatLoop(taskLog.Id, ct);
        return taskLog.Id;
    }

    private async Task HeartbeatLoop(Guid taskLogId, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await _taskLogRepo.UpdateHeartbeatAsync(taskLogId, ct); await Task.Delay(30000, ct); }
            catch { break; }
        }
    }

    private async Task<Dictionary<string, Guid>> LoadSubjectsAsync(CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<(string Code, Guid Id)>(
            "SELECT Code, Id FROM AccountingSubjects WHERE Code IN ('1001','1122','2203')");
        return rows.ToDictionary(r => r.Code, r => r.Id);
    }
}
