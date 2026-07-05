using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Scheduling;

/// <summary>
/// 合同终止结算 — 审批通过后执行全套结算
/// 事件驱动（由 ApprovalCompletedEventHandler 触发）
/// </summary>
public class TerminateJob
{
    private readonly ITaskLogRepository _taskLogRepo;
    private readonly ITaskStepLogger _stepLogger;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IUnitOfWork _uow;

    public TerminateJob(
        ITaskLogRepository taskLogRepo, ITaskStepLogger stepLogger,
        IDbConnectionFactory db, ISqlLoader sql, IUnitOfWork uow)
    {
        _taskLogRepo = taskLogRepo; _stepLogger = stepLogger;
        _db = db; _sql = sql; _uow = uow;
    }

    public async Task ExecuteAsync(Guid contractId, string? actualEndDate, string depositReturn, string reason, CancellationToken ct)
    {
        var taskLogId = await BeginTaskLogAsync("TerminateJob", contractId, ct);
        var subjects = await LoadSubjectsAsync(ct);

        using var conn = _db.CreateConnection(); conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            // Step01: 查询合同和当前账期
            var step01 = await _stepLogger.StartStepAsync(taskLogId, "TermStep01", "查询合同信息", null, null, ct);
            var contract = await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT Id, ContractNo, StartDate, EndDate, DepositAmount, CompanyId FROM Contracts WHERE Id=@Id",
                new { Id = contractId }, tx);
            if (contract == null) throw new InvalidOperationException("合同不存在");
            var curPeriod = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month:D2}";
            await _stepLogger.CompleteStepAsync(step01, 1, null, ct);

            // Step02: 补生未出账月份应收
            var step02 = await _stepLogger.StartStepAsync(taskLogId, "TermStep02", "补生未出账应收", null, null, ct);
            // TODO: 按合同起止日期补生 missing 月份
            await _stepLogger.CompleteStepAsync(step02, 0, null, ct);

            // Step03: 扣款处理
            var step03 = await _stepLogger.StartStepAsync(taskLogId, "TermStep03", "押金扣款处理", null, null, ct);
            var depositAmount = (decimal)contract.DepositAmount;
            if (depositAmount > 0)
            {
                var deduction = 0m;
                // 扣款分录：借押金/贷其他业务收入
                // TODO: 从 TerminationBizData 获取扣款明细
                if (deduction > 0)
                {
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = Guid.NewGuid(), SId = subjects["2241"],
                            Dir = "Debit", Amt = deduction, Sum = "终止扣款", CBy = Guid.Empty }, tx);
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = Guid.NewGuid(), SId = subjects["6051"],
                            Dir = "Credit", Amt = deduction, Sum = "扣款收入", CBy = Guid.Empty }, tx);
                }
            }
            await _stepLogger.CompleteStepAsync(step03, 0, null, ct);

            // Step04: 押金抵扣欠费 + 退还
            var step04 = await _stepLogger.StartStepAsync(taskLogId, "TermStep04", "押金结算", null, null, ct);
            if (depositAmount > 0)
            {
                var receivableBal = await conn.QuerySingleAsync<decimal>(
                    _sql.Get("Billing.Select.JournalEntry.BalanceBySubject"),
                    new { Code = "1122", SrcId = contractId }, tx);
                var offsetAmount = Math.Min(receivableBal, depositAmount);
                if (offsetAmount > 0)
                {
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = Guid.NewGuid(), SId = subjects["2241"],
                            Dir = "Debit", Amt = offsetAmount, Sum = "押金抵扣欠费", CBy = Guid.Empty }, tx);
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = Guid.NewGuid(), SId = subjects["1122"],
                            Dir = "Credit", Amt = offsetAmount, Sum = "欠费已抵扣", CBy = Guid.Empty }, tx);
                }
                var refundAmount = depositAmount - offsetAmount;
                if (refundAmount > 0)
                {
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = Guid.NewGuid(), SId = subjects["2241"],
                            Dir = "Debit", Amt = refundAmount, Sum = "押金退还", CBy = Guid.Empty }, tx);
                    await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                        new { Id = Guid.NewGuid(), VId = Guid.NewGuid(), SId = subjects["1001"],
                            Dir = "Credit", Amt = refundAmount, Sum = "退还押金", CBy = Guid.Empty }, tx);
                }
            }
            await _stepLogger.CompleteStepAsync(step04, 1, null, ct);

            tx.Commit();
            await _taskLogRepo.CompleteAsync(taskLogId, 1, 1, 0, 0, "终止结算完成", ct);
        }
        catch
        {
            tx.Rollback();
            await _taskLogRepo.FailAsync(taskLogId, "终止结算失败", ct);
            throw;
        }
    }

    private async Task<Guid> BeginTaskLogAsync(string taskName, Guid contractId, CancellationToken ct)
    {
        var taskLog = new RBS.Core.Entities.Scheduling.TaskLog(taskName, Guid.Empty, "", "Event", "Execute");
        var f = typeof(RBS.Core.Entities.Scheduling.TaskLog).GetField("<ContractId>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        f?.SetValue(taskLog, contractId);
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
            "SELECT Code, Id FROM AccountingSubjects WHERE Code IN ('1001','1122','2241','6051')");
        return rows.ToDictionary(r => r.Code, r => r.Id);
    }
}
