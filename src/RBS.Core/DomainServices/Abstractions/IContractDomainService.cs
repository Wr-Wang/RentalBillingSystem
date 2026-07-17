namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.Billing;

/// <summary>合同终止结果 — 告知调用方需要过期费用配置的日期</summary>
public record TerminationResult(string EffectiveEndDate);

/// <summary>
/// 合同领域服务接口 — 跨聚合的合同业务操作。
/// 定义合同生命周期中各阶段（生效、暂停、恢复、终止、续签）的业务契约。
/// 注意：领域服务只做校验和状态变更，不负责数据加载和持久化。
/// </summary>
public interface IContractDomainService
{
    /// <summary>
    /// 生效合同：校验房间状态 → 变更状态 → 记录事件。
    /// </summary>
    Task ActivateContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>
    /// 终止合同：变更状态为"Terminated"并记录终止原因。
    /// </summary>
    Task TerminateContractAsync(Contract contract, string reason, CancellationToken ct = default);

    /// <summary>
    /// 暂停合同：暂停期间不生成应收计划。
    /// </summary>
    Task SuspendContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>
    /// 恢复已暂停的合同。
    /// </summary>
    Task ResumeContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>
    /// 续签合同：创建新合同，关联旧合同。
    /// 注意：合同编号应通过 IContractNumberGenerator 生成后传入，
    /// 此方法已废弃，请使用 RenewalService 的续签流程。
    /// </summary>
    [Obsolete("Use RenewalService with IContractNumberGenerator instead")]
    Task<Contract> RenewContractAsync(Contract oldContract, string contractNo, DateOnly newEndDate, CancellationToken ct = default);

    /// <summary>
    /// 按天分摊计算应收金额。
    /// </summary>
    decimal CalculateProratedAmount(decimal monthlyAmount, DateOnly startDate, DateOnly endDate, DateOnly periodStart, DateOnly periodEnd);

    /// <summary>
    /// 执行合同终止 — 校验并变更合同状态。
    /// </summary>
    /// <param name="contract">已加载的合同聚合根</param>
    /// <param name="journals">该合同下的全部 Journal（已加载）</param>
    /// <param name="actualEndDate">实际终止日期</param>
    /// <param name="reason">终止原因</param>
    /// <returns>终止结果，含费用配置到期日期</returns>
    TerminationResult ExecuteContractTermination(Contract contract, IReadOnlyList<Journal> journals, DateOnly? actualEndDate, string reason);
}
