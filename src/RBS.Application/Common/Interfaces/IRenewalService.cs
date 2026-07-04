using RBS.Application.DTOs.Contract;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 续签应用服务接口
/// </summary>
public interface IRenewalService
{
    /// <summary>续签预览：检查欠费、并发、市场价、展示继承项</summary>
    Task<RenewalPreviewDto> PreviewAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>提交续签审批</summary>
    Task<RenewalSubmitResultDto> SubmitAsync(SubmitRenewalRequest request, Guid userId, CancellationToken ct = default);

    /// <summary>审批通过后执行续签</summary>
    Task ExecuteRenewalAsync(Guid renewalRequestId, CancellationToken ct = default);

    /// <summary>获取续签历史</summary>
    Task<List<RenewalHistoryDto>> GetHistoryAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>获取续签链</summary>
    Task<List<RenewalChainNodeDto>> GetRenewalChainAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>获取合同当前允许的操作</summary>
    Task<ContractOperationsDto> GetAllowedOperationsAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>统一并发检查</summary>
    Task EnsureNoPendingApprovalAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>获取最近一次被驳回的续签数据（用于重新提交预填）</summary>
    Task<RejectedRenewalDto?> GetLastRejectedAsync(Guid contractId, CancellationToken ct = default);
}
