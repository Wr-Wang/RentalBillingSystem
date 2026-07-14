namespace RBS.Core.DomainServices;

using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Core.Entities.Billing;

/// <summary>
/// 合同领域服务 — 跨聚合的业务逻辑编排。
/// 实现 IContractDomainService 接口，协调合同聚合与应收计划、费用配置等
/// 关联聚合之间的操作，通过 IUnitOfWork 保证跨仓储事务一致性。
/// </summary>
public class ContractDomainService : IContractDomainService
{
    private readonly IUnitOfWork _uow;

    /// <summary>
    /// 初始化合同领域服务
    /// </summary>
    /// <param name="uow">工作单元，用于协调多个仓储的写入</param>
    public ContractDomainService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <summary>
    /// 生效合同：校验房间状态 → 变更状态 → 记录事件。
    /// 通过工作单元检查目标房屋单元是否已有生效合同。
    /// </summary>
    /// <param name="contract">待生效的合同聚合根</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="InvalidOperationException">房屋单元已有生效合同时抛出</exception>
    public async Task ActivateContractAsync(Contract contract, CancellationToken ct = default)
    {
        var hasActive = await _uow.Contracts.HasActiveForHousingUnitAsync(contract.RoomId, ct);
        if (hasActive)
            throw new InvalidOperationException("该房屋单元已有生效合同");
        contract.Activate();
    }

    /// <summary>
    /// 终止合同：变更状态为"Terminated"并记录终止原因。
    /// </summary>
    /// <param name="contract">待终止的合同聚合根</param>
    /// <param name="reason">终止原因说明</param>
    /// <param name="ct">取消令牌</param>
    public async Task TerminateContractAsync(Contract contract, string reason, CancellationToken ct = default)
    {
        contract.Terminate(reason);
    }

    /// <summary>
    /// 暂停合同：将合同置为暂停状态，暂停期间不生成应收计划。
    /// </summary>
    /// <param name="contract">待暂停的合同聚合根</param>
    /// <param name="ct">取消令牌</param>
    public async Task SuspendContractAsync(Contract contract, CancellationToken ct = default)
    {
        contract.Suspend();
    }

    /// <summary>
    /// 恢复已暂停的合同，重新开始计费。
    /// </summary>
    /// <param name="contract">待恢复的合同聚合根</param>
    /// <param name="ct">取消令牌</param>
    public async Task ResumeContractAsync(Contract contract, CancellationToken ct = default)
    {
        contract.Resume();
    }

    /// <summary>
    /// 续签合同：创建新合同，关联旧合同。
    /// 校验旧合同状态必须是"Active"或"Expired"且有固定到期日，
    /// 旧合同标记为已续签，新合同继承房间、公司、付款周期等属性。
    /// </summary>
    /// <param name="oldContract">待续签的旧合同</param>
    /// <param name="newEndDate">新合同到期日期</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的合同聚合根</returns>
    /// <exception cref="InvalidOperationException">合同不是生效/到期状态或无固定到期日时抛出</exception>
    public async Task<Contract> RenewContractAsync(Contract oldContract, DateOnly newEndDate, CancellationToken ct = default)
    {
        if (oldContract.Status != "Active" && oldContract.Status != "Expired")
            throw new InvalidOperationException("只有生效中或已到期的合同可以续签");
        if (oldContract.EndDate == null) throw new InvalidOperationException("无固定到期日的合同不可续签");
        oldContract.MarkAsRenewed();
        var newContract = new Contract(
            $"{oldContract.ContractNo}-R{new Random().Next(1, 99)}",
            oldContract.RoomId,
            oldContract.CompanyId);
        newContract.SetPeriod(oldContract.EndDate.Value.AddDays(1), newEndDate);
        newContract.SetPaymentCycle(oldContract.PaymentCycle);
        return newContract;
    }

    /// <summary>
    /// 执行合同终止 — 审批通过后一次性完成所有终止操作。
    /// 编排以下跨聚合操作：1) 终止合同状态；2) 取消未结清应收计划；
    /// 3) 将费用配置置为到期（通过原始 SQL 批量更新）。
    /// 押金生成会计凭证由外部事件处理器完成。
    /// 所有操作在同一工作单元事务中提交。
    /// </summary>
    /// <param name="contractId">待终止的合同 ID</param>
    /// <param name="actualEndDate">实际终止日期，为空时使用合同到期日</param>
    /// <param name="depositReturn">押金处理方式说明</param>
    /// <param name="reason">终止原因</param>
    /// <param name="userId">操作人用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="KeyNotFoundException">合同不存在时抛出</exception>
    /// <exception cref="InvalidOperationException">合同已终止时抛出</exception>
    public async Task ExecuteContractTerminationAsync(Guid contractId, DateOnly? actualEndDate, string depositReturn, string reason, Guid userId, CancellationToken ct = default)
    {
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct);
        if (contract == null) throw new KeyNotFoundException("合同不存在");
        if (contract.Status == "Terminated")
            throw new InvalidOperationException("合同已终止，无需重复操作");

        // 1. 终止合同（唯一一次调用）
        contract.Terminate(reason);
        await _uow.Contracts.UpdateAsync(contract, ct);

        // 2. 取消未结清应收计划
        var plans = await _uow.ReceivablePlans.GetByContractIdAsync(contractId, ct);
        foreach (var plan in plans.Where(p => p.Status is "Pending" or "Partial" or "Overdue"))
        {
            plan.Cancel("合同终止");
            await _uow.ReceivablePlans.UpdateAsync(plan, ct);
        }

        // 3. 费用配置到期
        // 使用原始 SQL 直接批量更新，因为 Contract 聚合的 _feeConfigs 子集合
        // 在默认仓储加载中不会被填充，无法通过实体遍历实现。
        // TODO: 当聚合支持子集合懒加载后，改为加载 ContractFeeConfig 实体并通过 ExpireOn() 方法操作。
        var effectiveEnd = actualEndDate?.ToString("yyyy-MM-dd") ?? contract.EndDate?.ToString("yyyy-MM-dd") ?? DateOnly.FromDateTime(ChinaTime.Now).ToString("yyyy-MM-dd");
        await _uow.ExecuteSqlRawAsync(
            "UPDATE ContractFeeConfigs SET ExpiryDate = @ExpiryDate, IsActive = 0 WHERE ContractId = @ContractId AND IsActive = 1",
            new { ExpiryDate = effectiveEnd, ContractId = contractId }, ct);

        // 4. 押金处理（由 ApprovalCompletedEventHandler 调用 TerminateJob 生成 JE）

        await _uow.CommitAsync(ct);
    }

    /// <summary>
    /// 按天分摊计算应收金额。
    /// 根据月度金额和实际占用天数（有效起始日与有效结束日之间的天数），
    /// 计算指定账期内应收取的按天分摊金额，以处理首月/末月非完整周期计费。
    /// </summary>
    /// <param name="monthlyAmount">月度标准金额</param>
    /// <param name="startDate">费用生效起始日期</param>
    /// <param name="endDate">费用到期日期</param>
    /// <param name="periodStart">账期起始日期</param>
    /// <param name="periodEnd">账期结束日期</param>
    /// <returns>按天分摊后的应收金额，保留两位小数</returns>
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
