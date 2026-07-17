namespace RBS.Core.DomainServices;

using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;

/// <summary>
/// 合同领域服务 — 跨聚合的合同业务操作实现。
/// 负责合同生命周期中的校验和状态变更，不负责数据加载和持久化。
/// 所有数据由应用层传入，领域服务只做业务判断。
/// </summary>
public class ContractDomainService : IContractDomainService
{
    /// <summary>
    /// 生效合同：校验房间状态 → 变更状态 → 记录事件。
    /// 检查目标房屋单元是否已有生效合同，防止重复签约。
    /// </summary>
    public async Task ActivateContractAsync(Contract contract, CancellationToken ct = default)
    {
        // 校验由应用层完成（房间冲突检查），领域层只做状态变更
        contract.Activate();
        await Task.CompletedTask;
    }

    /// <summary>
    /// 终止合同：变更状态为"Terminated"并记录终止原因。
    /// </summary>
    public async Task TerminateContractAsync(Contract contract, string reason, CancellationToken ct = default)
    {
        contract.Terminate(reason);
        await Task.CompletedTask;
    }

    /// <summary>
    /// 暂停合同。
    /// </summary>
    public async Task SuspendContractAsync(Contract contract, CancellationToken ct = default)
    {
        contract.Suspend();
        await Task.CompletedTask;
    }

    /// <summary>
    /// 恢复已暂停的合同。
    /// </summary>
    public async Task ResumeContractAsync(Contract contract, CancellationToken ct = default)
    {
        contract.Resume();
        await Task.CompletedTask;
    }

    /// <summary>
    /// 续签合同：校验旧合同状态、标记已续签、创建新合同。
    /// 注意：合同编号应通过 IContractNumberGenerator 生成后传入。
    /// 此方法已废弃，请使用 RenewalService 的续签流程。
    /// </summary>
    [Obsolete("Use RenewalService with IContractNumberGenerator instead")]
    public async Task<Contract> RenewContractAsync(Contract oldContract, string contractNo, DateOnly newEndDate, CancellationToken ct = default)
    {
        if (oldContract.Status != "Active" && oldContract.Status != "Expired")
            throw new InvalidOperationException("只有生效中或已到期的合同可以续签");
        if (oldContract.EndDate == null) throw new InvalidOperationException("无固定到期日的合同不可续签");

        oldContract.MarkAsRenewed();
        var newContract = new Contract(
            contractNo,
            oldContract.RoomId,
            oldContract.CompanyId);
        newContract.SetPeriod(oldContract.EndDate.Value.AddDays(1), newEndDate);
        newContract.SetPaymentCycle(oldContract.PaymentCycle);

        await Task.CompletedTask;
        return newContract;
    }

    /// <summary>
    /// 执行合同终止 — 仅做状态变更，不包含持久化。
    /// Journal 为不可变记录，终止时通过创建冲销 Journal 来处理应收余额。
    /// 由应用层在调用前加载好数据，调用后统一提交事务。
    /// </summary>
    public TerminationResult ExecuteContractTermination(
        Contract contract, IReadOnlyList<Journal> journals,
        DateOnly? actualEndDate, string reason)
    {
        // 1. 终止合同
        contract.Terminate(reason);

        // 2. Journal 为不可变记录，无需取消操作
        //    冲销通过 TerminateJob 生成负金额 Journal + GL 更新完成

        // 3. 计算费用配置到期日期（由应用层执行 SQL 更新）
        var effectiveEnd = actualEndDate?.ToString("yyyy-MM-dd")
            ?? contract.EndDate?.ToString("yyyy-MM-dd")
            ?? DateOnly.FromDateTime(ChinaTime.Now).ToString("yyyy-MM-dd");

        return new TerminationResult(effectiveEnd);
    }

    /// <summary>
    /// 按天分摊计算应收金额。
    /// </summary>
    public decimal CalculateProratedAmount(decimal monthlyAmount, DateOnly startDate, DateOnly endDate,
        DateOnly periodStart, DateOnly periodEnd)
    {
        var daysInMonth = DateTime.DaysInMonth(periodStart.Year, periodStart.Month);
        var dailyRate = monthlyAmount / daysInMonth;
        var effectiveStart = startDate > periodStart ? startDate : periodStart;
        var effectiveEnd = endDate < periodEnd ? endDate : periodEnd;
        var days = effectiveStart.DayNumber <= effectiveEnd.DayNumber
            ? effectiveEnd.DayNumber - effectiveStart.DayNumber + 1
            : 0;
        return Math.Round(dailyRate * days, 2);
    }
}
