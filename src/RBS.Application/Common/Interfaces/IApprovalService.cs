using RBS.Application.DTOs.Approval;

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
}
