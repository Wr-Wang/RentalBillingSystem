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
/// 生成一张 Voucher 包含所有终止分录（扣款/抵扣欠费/退还）
/// </summary>
public class TerminateJob : ITerminateJob
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
            // Step01: 查询合同信息
            var step01 = await _stepLogger.StartStepAsync(taskLogId, "TermStep01", "查询合同信息", null, null, ct);
            var contract = await conn.QuerySingleOrDefaultAsync<dynamic>(
                _sql.Get("Terminate.Select.Contract.Detail"), new { Id = contractId }, tx);
            if (contract == null) throw new InvalidOperationException("合同不存在");

            var companyId = (Guid)contract.CompanyId;
            var now = DateTime.UtcNow;
            var period = $"{now.Year:D4}-{now.Month:D2}";
            await _stepLogger.CompleteStepAsync(step01, 1, null, ct);

            // Step02: 查询押金 + 应收余额
            var step02 = await _stepLogger.StartStepAsync(taskLogId, "TermStep02", "查询押金与应收余额", null, null, ct);
            var depositAmount = await conn.QuerySingleOrDefaultAsync<decimal>(
                _sql.Get("Contract.Select.DepositConfig.AmountByContract"),
                new { Cid = contractId }, tx);

            var receivableBal = await conn.QuerySingleAsync<decimal>(
                _sql.Get("Billing.Select.JournalEntry.BalanceByContract"),
                new { Code = "1122", ContractId = contractId }, tx);
            await _stepLogger.CompleteStepAsync(step02, 1, null, ct);

            if (depositAmount <= 0)
            {
                await _stepLogger.SkipStepAsync(
                    await _stepLogger.StartStepAsync(taskLogId, "TermStep03", "押金结算", null, null, ct),
                    "无押金，跳过结算", null, ct);
                tx.Commit();
                await _taskLogRepo.CompleteAsync(taskLogId, 1, 1, 0, 0, "无押金，终止结算完成", ct);
                return;
            }

            // Step03: 创建终止结算 Voucher
            var step03 = await _stepLogger.StartStepAsync(taskLogId, "TermStep03", "生成终止结算凭证", null, null, ct);

            var voucherId = Guid.NewGuid();
            var voucherNo = $"TERM-{now:yyyyMMdd}-{contractId:N}".Substring(0, 32);
            var deduction = 0m; // TODO: 从 TerminationBizData 获取扣款明细
            var offsetAmount = Math.Min(receivableBal, depositAmount);
            var refundAmount = depositAmount - offsetAmount - deduction;

            // 插入 Voucher
            await conn.ExecuteAsync(
                _sql.Get("Accounting.Insert.Voucher.WithCompanyId"),
                new
                {
                    Id = voucherId, No = voucherNo,
                    Date = DateOnly.FromDateTime(now),
                    Type = "ContractTermination", SrcId = contractId,
                    CId = contractId, CoId = companyId, Period = period,
                    CBy = Guid.Empty
                }, tx);

            // 借方：2241 押金（全额冲减）
            await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                new { Id = Guid.NewGuid(), VId = voucherId, SId = subjects["2241"],
                    Dir = "Debit", Amt = depositAmount, Sum = "合同终止押金结算", CBy = Guid.Empty }, tx);

            // 贷方：6051 扣款（如有）
            if (deduction > 0)
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = voucherId, SId = subjects["6051"],
                        Dir = "Credit", Amt = deduction, Sum = "终止扣款", CBy = Guid.Empty }, tx);

            // 贷方：1122 抵扣欠费
            if (offsetAmount > 0)
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = voucherId, SId = subjects["1122"],
                        Dir = "Credit", Amt = offsetAmount, Sum = "押金抵扣欠费", CBy = Guid.Empty }, tx);

            // 贷方：1001 退还押金
            if (refundAmount > 0)
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new { Id = Guid.NewGuid(), VId = voucherId, SId = subjects["1001"],
                        Dir = "Credit", Amt = refundAmount, Sum = "退还押金", CBy = Guid.Empty }, tx);

            await _stepLogger.CompleteStepAsync(step03, 1, null, ct);

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
        var codes = new[] { "1001", "1122", "2241", "6051" };
        var rows = await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodeList"), new { Codes = codes });
        return rows.ToDictionary(r => r.Code, r => r.Id);
    }
}
