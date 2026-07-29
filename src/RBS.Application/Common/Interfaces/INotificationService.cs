using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 通知服务接口 — 读（分页查询/未读计数）+ 写（标记已读/创建通知）
/// </summary>
public interface INotificationService
{
    /// <summary>分页查询当前用户的通知（未读优先，时间倒序）</summary>
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(NotificationQueryDto query, CancellationToken ct = default);

    /// <summary>各分类未读计数</summary>
    Task<UnreadCountsDto> GetUnreadCountsAsync(CancellationToken ct = default);

    /// <summary>单条标记已读</summary>
    Task MarkReadAsync(Guid id, CancellationToken ct = default);

    /// <summary>当前用户全部标记已读</summary>
    Task MarkAllReadAsync(CancellationToken ct = default);

    /// <summary>直接创建一条通知</summary>
    Task CreateAsync(Core.Entities.SystemConfig.Notification notification, CancellationToken ct = default);

    /// <summary>去重创建（同用户+同分类+同天存在则跳过）</summary>
    Task CreateWithDedupAsync(Guid userId, string category, string title, string? content,
        string? referenceType = null, Guid? referenceId = null, Guid? companyId = null,
        CancellationToken ct = default);

    /// <summary>通知指定级别的审批人</summary>
    Task NotifyApproversAsync(Guid approvalRequestId, int level, string title, string? content,
        CancellationToken ct = default);

    /// <summary>通知审批提交人</summary>
    Task NotifySubmitterAsync(Guid approvalRequestId, string title, string? content,
        CancellationToken ct = default);

    /// <summary>通知所有审批参与人（提交人 + 各级审批人）</summary>
    Task NotifyAllParticipantsAsync(Guid approvalRequestId, string title, string? content,
        CancellationToken ct = default);

    /// <summary>通知指定角色的所有用户（使用默认分类 "Approval"）</summary>
    Task NotifyRoleAsync(string roleCode, string title, string? content,
        string? referenceType = null, Guid? referenceId = null, CancellationToken ct = default);

    /// <summary>通知指定角色的所有用户（指定通知分类）</summary>
    Task NotifyRoleAsync(string roleCode, string category, string title, string? content,
        string? referenceType = null, Guid? referenceId = null, Guid? companyId = null,
        CancellationToken ct = default);
}
