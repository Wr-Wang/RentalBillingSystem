namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;

/// <summary>
/// 合同领域服务接口 — 跨聚合的合同业务操作
/// </summary>
public interface IContractDomainService
{
    /// <summary>生效合同：校验房间状态 → 变更状态 → 记录事件</summary>
    Task ActivateContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>终止合同：计算违约金 → 变更状态 → 释放房间</summary>
    Task TerminateContractAsync(Contract contract, string reason, CancellationToken ct = default);

    /// <summary>暂停合同：暂停期间不生成应收</summary>
    Task SuspendContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>恢复已暂停的合同</summary>
    Task ResumeContractAsync(Contract contract, CancellationToken ct = default);

    /// <summary>续签合同：创建新合同，关联旧合同</summary>
    Task<Contract> RenewContractAsync(Contract oldContract, DateOnly newEndDate, decimal? newRentAmount, CancellationToken ct = default);

    /// <summary>按天分摊计算应收金额</summary>
    decimal CalculateProratedAmount(decimal monthlyAmount, DateOnly startDate, DateOnly endDate, DateOnly periodStart, DateOnly periodEnd);
}
