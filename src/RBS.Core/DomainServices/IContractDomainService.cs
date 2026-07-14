namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;

/// <summary>
/// 合同领域服务接口 — 跨聚合的合同业务操作。
/// 定义合同生命周期中各阶段（生效、暂停、恢复、终止、续签）的业务契约，
/// 以及合同终止时跨聚合（应收计划、费用配置、押金）的编排操作。
/// </summary>
public interface IContractDomainService
{
    /// <summary>
    /// 生效合同：校验房间状态 → 变更状态 → 记录事件。
    /// 在合同生效前检查目标房屋单元是否已有其他生效合同，防止重复签约。
    /// </summary>
    /// <param name="contract">待生效的合同聚合根</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="InvalidOperationException">房屋单元已有生效合同时抛出</exception>
    Task ActivateContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>
    /// 终止合同：计算违约金 → 变更状态 → 释放房间。
    /// 将合同标记为"Terminated"状态，并记录终止原因。
    /// </summary>
    /// <param name="contract">待终止的合同聚合根</param>
    /// <param name="reason">终止原因说明</param>
    /// <param name="ct">取消令牌</param>
    Task TerminateContractAsync(Contract contract, string reason, CancellationToken ct = default);

    /// <summary>
    /// 暂停合同：暂停期间不生成应收计划。
    /// 将合同置为"Suspend"状态，暂停计费周期。
    /// </summary>
    /// <param name="contract">待暂停的合同聚合根</param>
    /// <param name="ct">取消令牌</param>
    Task SuspendContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>
    /// 恢复已暂停的合同，重新开始计费。
    /// 将合同从"Suspend"状态恢复为"Active"状态。
    /// </summary>
    /// <param name="contract">待恢复的合同聚合根</param>
    /// <param name="ct">取消令牌</param>
    Task ResumeContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>
    /// 续签合同：创建新合同，关联旧合同。
    /// 旧合同标记为已续签，新合同继承旧合同的房间、公司、付款周期等信息，
    /// 起始日期为旧合同到期日次日，结束日期由参数指定。
    /// </summary>
    /// <param name="oldContract">旧合同聚合根，必须是生效或已到期状态</param>
    /// <param name="newEndDate">新合同的到期日期</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的合同聚合根</returns>
    /// <exception cref="InvalidOperationException">合同不是生效或到期状态，或无固定到期日时抛出</exception>
    Task<Contract> RenewContractAsync(Contract oldContract, DateOnly newEndDate, CancellationToken ct = default);

    /// <summary>
    /// 按天分摊计算应收金额。
    /// 根据月度金额和实际占用天数，计算指定账期内应收取的按天分摊金额。
    /// 处理首月/末月非完整周期的费用计算场景。
    /// </summary>
    /// <param name="monthlyAmount">月度标准金额</param>
    /// <param name="startDate">合同或费用生效日期</param>
    /// <param name="endDate">合同或费用到期日期</param>
    /// <param name="periodStart">账期起始日</param>
    /// <param name="periodEnd">账期结束日</param>
    /// <returns>按天分摊后的应收金额，保留两位小数</returns>
    decimal CalculateProratedAmount(decimal monthlyAmount, DateOnly startDate, DateOnly endDate, DateOnly periodStart, DateOnly periodEnd);

    /// <summary>
    /// 执行合同终止（审批通过后回调）— 取消应收 + 费用到期 + 押金处理。
    /// 一次性完成所有终止操作：终止合同状态、取消未结清应收计划、将费用配置置为到期。
    /// 押金处理由 ApprovalCompletedEventHandler 通过 TerminateJob 生成会计凭证。
    /// </summary>
    /// <param name="contractId">待终止的合同 ID</param>
    /// <param name="actualEndDate">实际终止日期，为空时使用合同到期日</param>
    /// <param name="depositReturn">押金处理方式说明</param>
    /// <param name="reason">终止原因</param>
    /// <param name="userId">操作人用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="KeyNotFoundException">合同不存在时抛出</exception>
    /// <exception cref="InvalidOperationException">合同已终止时抛出</exception>
    Task ExecuteContractTerminationAsync(Guid contractId, DateOnly? actualEndDate, string depositReturn, string reason, Guid userId, CancellationToken ct = default);
}
