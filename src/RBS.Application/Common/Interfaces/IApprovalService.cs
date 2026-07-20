using RBS.Application.DTOs.Approval;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 审批应用服务接口 — 管理审批流程的创建、审批、查询
/// </summary>
public interface IApprovalService
{
    /// <summary>提交审批请求</summary>
    Task<ApprovalRequestDto> SubmitAsync(SubmitApprovalRequest request, CancellationToken ct = default);

    /// <summary>审批通过</summary>
    Task<ApprovalRequestDto> ApproveAsync(Guid id, string? comment, CancellationToken ct = default);

    /// <summary>审批驳回</summary>
    Task<ApprovalRequestDto> RejectAsync(Guid id, string comment, CancellationToken ct = default);

    /// <summary>获取待审批列表（当前用户有权限审批的）</summary>
    Task<List<ApprovalRequestDto>> GetPendingAsync(CancellationToken ct = default);

    /// <summary>获取我提交的请求</summary>
    Task<List<ApprovalRequestDto>> GetMyRequestsAsync(CancellationToken ct = default);

    /// <summary>获取审批详情</summary>
    Task<ApprovalRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>撤回审批（仅提交人可操作）</summary>
    Task<ApprovalRequestDto> CancelAsync(Guid id, string? reason = null, CancellationToken ct = default);

    /// <summary>重新提交已撤回的审批</summary>
    Task<ApprovalRequestDto> ResubmitAsync(Guid id, CancellationToken ct = default);

    /// <summary>获取审批历史（分页）</summary>
    Task<PagedResult<ApprovalRequestDto>> GetHistoryAsync(ApprovalHistoryQuery query, CancellationToken ct = default);

    /// <summary>获取最近一次被驳回的审批数据</summary>
    Task<LastRejectedApprovalDto?> GetLastRejectedAsync(Guid targetEntityId, string targetEntityType, CancellationToken ct = default);

    /// <summary>获取审批业务详情（对比数据）</summary>
    Task<ApprovalBizDetailDto?> GetBizDetailAsync(Guid id, CancellationToken ct = default);
}
