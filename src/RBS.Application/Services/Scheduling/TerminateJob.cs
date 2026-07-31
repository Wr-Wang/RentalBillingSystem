using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Common;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Scheduling;

/// <summary>
/// 合同终止结算 — 审批通过后执行全套结算
/// 事件驱动（由 ApprovalCompletedEventHandler 触发）
/// 生成 Journal + GL 更新完成终止结算（扣款/抵扣欠费/退还）
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
        using var conn = _db.CreateConnection(); conn.Open();

        // Step00: 预查合同信息（先拿到 companyId 再创建任务日志）
        var contract = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Terminate.Select.Contract.Detail"), new { Id = contractId });
        if (contract == null) throw new InvalidOperationException("合同不存在");

        var companyId = (Guid)contract.CompanyId;
        var taskLogEntry = new RBS.Core.Entities.Scheduling.TaskLog("TerminateJob", companyId, "", "Event", "Execute");
        var taskLogId = await _taskLogRepo.CreateAsync(taskLogEntry, ct);
        var subjects = await LoadSubjectsAsync(ct);

        using var tx = conn.BeginTransaction();

        try
        {
            var now = ChinaTime.Now;
            var period = $"{now.Year:D4}-{now.Month:D2}";

            // Step01: 查询押金 + 应收余额
            var step01 = await _stepLogger.StartStepAsync(taskLogId, "TermStep01", "查询押金与应收余额", null, null, ct);
            var depositAmt = await conn.QuerySingleOrDefaultAsync<decimal>(
                _sql.Get("Contract.Select.DepositConfig.AmountByContract"),
                new { Cid = contractId }, tx);
            var receivableBal = await conn.QuerySingleAsync<decimal>(
                _sql.Get("Billing.Select.Journal.BalanceByContract"),
                new { Code = "1122", CId = contractId }, tx);
            await _stepLogger.CompleteStepAsync(step01, 1, null, ct);

            if (depositAmt <= 0)
            {
                await _stepLogger.SkipStepAsync(
                    await _stepLogger.StartStepAsync(taskLogId, "TermStep02", "押金结算", null, null, ct),
                    "无押金，跳过结算", null, ct);
                tx.Commit();
                await _taskLogRepo.CompleteAsync(taskLogId, 1, 1, 0, 0, "无押金，终止结算完成", ct);
                return;
            }

            // Step02: 创建终止结算 Journal
            var step02 = await _stepLogger.StartStepAsync(taskLogId, "TermStep02", "生成终止结算日记账", null, null, ct);

            // 扣款：目前从 TerminationBizData 暂未传递扣款明细，默认为 0
            // TODO: 后续从 ApprovalBizData.Reason 或新增扣款字段获取
            var deduction = 0m;
            var offsetAmt = Math.Min(receivableBal, depositAmt);
            var refundAmt = depositAmt - offsetAmt - deduction;

            var billedAt = ChinaTime.Now;
            var dueDate = now.Date;

            // 借方：2241 押金（全额冲减）
            await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                new
                {
                    Id = Guid.NewGuid(), CoId = companyId, CId = contractId,
                    FId = Guid.Empty, FConfigId = (Guid?)null,
                    SubjId = subjects["2241"],
                    Period = period, Amt = depositAmt, Due = dueDate,
                    EntryType = "Adjustment", BilledAt = billedAt,
                    DNId = (Guid?)null, ParentId = (Guid?)null,
                    Summary = "合同终止押金结算", CBy = SystemUsers.Scheduler
                }, tx);

            // 贷方：6051 扣款（如有）
            if (deduction > 0)
                await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                    new
                    {
                        Id = Guid.NewGuid(), CoId = companyId, CId = contractId,
                        FId = Guid.Empty, FConfigId = (Guid?)null,
                        SubjId = subjects["6051"],
                        Period = period, Amt = deduction, Due = dueDate,
                        EntryType = "Adjustment", BilledAt = billedAt,
                        DNId = (Guid?)null, ParentId = (Guid?)null,
                        Summary = "终止扣款", CBy = SystemUsers.Scheduler
                    }, tx);

            // 贷方：1122 抵扣欠费
            if (offsetAmt > 0)
                await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                    new
                    {
                        Id = Guid.NewGuid(), CoId = companyId, CId = contractId,
                        FId = Guid.Empty, FConfigId = (Guid?)null,
                        SubjId = subjects["1122"],
                        Period = period, Amt = offsetAmt, Due = dueDate,
                        EntryType = "Adjustment", BilledAt = billedAt,
                        DNId = (Guid?)null, ParentId = (Guid?)null,
                        Summary = "押金抵扣欠费", CBy = SystemUsers.Scheduler
                    }, tx);

            // 贷方：1001 退还押金
            if (refundAmt > 0)
                await conn.ExecuteAsync(_sql.Get("Billing.Insert.Journal.Default"),
                    new
                    {
                        Id = Guid.NewGuid(), CoId = companyId, CId = contractId,
                        FId = Guid.Empty, FConfigId = (Guid?)null,
                        SubjId = subjects["1001"],
                        Period = period, Amt = refundAmt, Due = dueDate,
                        EntryType = "Adjustment", BilledAt = billedAt,
                        DNId = (Guid?)null, ParentId = (Guid?)null,
                        Summary = "退还押金", CBy = SystemUsers.Scheduler
                    }, tx);

            await _stepLogger.CompleteStepAsync(step02, 1, null, ct);

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

    private async Task<Dictionary<string, Guid>> LoadSubjectsAsync(CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var subjects = (await conn.QueryAsync<(string Code, Guid Id)>(
            _sql.Get("Accounting.Select.Subject.ByCodes")))
            .ToDictionary(r => r.Code, r => r.Id);
        return subjects;
    }
}
