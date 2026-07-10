using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Common;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

/// <summary>
/// 通知服务实现 — 使用 Dapper 直写 Notifications 表
/// 读：按当前用户 userId 隔离
/// 写：去重控制 + 按角色/级别确定接收人
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IDbConnectionFactory _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly ISqlLoader _sql;

    public NotificationService(IDbConnectionFactory db, ICurrentUserService currentUser,
        IUnitOfWork uow, ISqlLoader sql)
    {
        _db = db;
        _currentUser = currentUser;
        _uow = uow;
        _sql = sql;
    }

    // =====================================================================
    // 读
    // =====================================================================

    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(
        NotificationQueryDto query, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        var sql = new List<string> { "UserId = @UserId" };
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);
        parameters.Add("PageSize", query.PageSize);

        if (!string.IsNullOrEmpty(query.Category))
        {
            sql.Add("Category = @Category");
            parameters.Add("Category", query.Category);
        }
        if (query.IsRead.HasValue)
        {
            sql.Add("IsRead = @IsRead");
            parameters.Add("IsRead", query.IsRead.Value);
        }
        if (!string.IsNullOrEmpty(query.Keyword))
        {
            sql.Add("(Title LIKE @Keyword OR Content LIKE @Keyword)");
            parameters.Add("Keyword", $"%{query.Keyword}%");
        }
        if (query.DateFrom.HasValue)
        {
            sql.Add("CreatedAt >= @DateFrom");
            parameters.Add("DateFrom", query.DateFrom.Value);
        }
        if (query.DateTo.HasValue)
        {
            sql.Add("CreatedAt <= @DateTo");
            parameters.Add("DateTo", query.DateTo.Value);
        }

        var whereClause = string.Join(" AND ", sql);

        using var conn = _db.CreateConnection();

        var countSql = $"SELECT COUNT(*) FROM [Notifications] WHERE {whereClause}";
        var total = await conn.ExecuteScalarAsync<int>(countSql, parameters);

        var dataSql = $@"
            SELECT [Id], [UserId], [Category], [Title], [Content],
                   [ReferenceType], [ReferenceId], [IsRead], [CreatedAt]
            FROM [Notifications]
            WHERE {whereClause}
            ORDER BY [IsRead] ASC, [CreatedAt] DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await conn.QueryAsync<NotificationDto>(dataSql, parameters)).AsList();

        return new PagedResult<NotificationDto>
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        };
    }

    public async Task<UnreadCountsDto> GetUnreadCountsAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        using var conn = _db.CreateConnection();

        var rows = await conn.QueryAsync<(string Category, int Count)>(
            _sql.Get("Notification.Select.Notification.UnreadCounts"),
            new { UserId = userId });

        var result = new UnreadCountsDto();
        foreach (var row in rows)
        {
            switch (row.Category)
            {
                case "Approval":   result.Approval   = row.Count; break;
                case "Renewal":    result.Renewal    = row.Count; break;
                case "Collection": result.Collection = row.Count; break;
                case "System":     result.System     = row.Count; break;
            }
        }
        result.Total = result.Approval + result.Renewal + result.Collection + result.System;
        return result;
    }

    // =====================================================================
    // 写 — 用户操作
    // =====================================================================

    public async Task MarkReadAsync(Guid id, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(_sql.Get("Notification.Update.Notification.MarkRead"),
            new { Id = id, UserId = userId });
    }

    public async Task MarkAllReadAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(_sql.Get("Notification.Update.Notification.MarkAllRead"),
            new { UserId = userId });
    }

    // =====================================================================
    // 写 — 系统创建
    // =====================================================================

    public async Task CreateAsync(Notification notification, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(_sql.Get("Notification.Insert.Notification.Default"),
            new
            {
                notification.Id,
                notification.UserId,
                notification.CompanyId,
                notification.Category,
                notification.Title,
                notification.Content,
                notification.ReferenceType,
                notification.ReferenceId,
                notification.IsRead,
                notification.CreatedAt
            });
    }

    public async Task CreateWithDedupAsync(Guid userId, string category, string title,
        string? content, string? referenceType = null, Guid? referenceId = null,
        Guid? companyId = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();

        // 去重：同用户 + 同分类 + 同天
        var exists = await conn.ExecuteScalarAsync<int>(
            _sql.Get("Notification.Select.Notification.DedupCheck"),
            new { UserId = userId, Category = category });

        if (exists > 0) return;

        var notification = new Notification(userId, category, title, content,
            referenceType, referenceId, companyId);
        await CreateAsync(notification, ct);
    }

    // =====================================================================
    // 通知场景 — 审批人
    // =====================================================================

    public async Task NotifyApproversAsync(Guid approvalRequestId, int level, string title,
        string? content, CancellationToken ct = default)
    {
        // 查该审批类型在当前级别的 RoleId
        var request = await _uow.ApprovalRequests.GetByIdAsync(approvalRequestId, ct);
        if (request == null) return;

        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var config = levels.FirstOrDefault(l =>
            l.ApprovalTypeId == request.ApprovalTypeId && l.LevelNo == level);
        if (config == null) return;

        await NotifyRoleByIdAsync(config.ApproverRoleId, title, content, "ApprovalRequest",
            approvalRequestId, request.CompanyId, ct);
    }

    public async Task NotifySubmitterAsync(Guid approvalRequestId, string title,
        string? content, CancellationToken ct = default)
    {
        var request = await _uow.ApprovalRequests.GetByIdWithRecordsAsync(approvalRequestId, ct);
        if (request == null) return;

        var submitRecord = request.Records.FirstOrDefault(r => r.Action == "Submitted");
        if (submitRecord == null) return;

        var notification = new Notification(submitRecord.ApproverId, "Approval",
            title, content, "ApprovalRequest", approvalRequestId, request.CompanyId);
        await CreateAsync(notification, ct);
    }

    public async Task NotifyAllParticipantsAsync(Guid approvalRequestId, string title,
        string? content, CancellationToken ct = default)
    {
        var request = await _uow.ApprovalRequests.GetByIdWithRecordsAsync(approvalRequestId, ct);
        if (request == null) return;

        var submittedBy = request.Records
            .FirstOrDefault(r => r.Action == "Submitted")?.ApproverId;

        var approverIds = request.Records
            .Where(r => r.Action == "Approved" || r.Action == "Rejected")
            .Select(r => r.ApproverId)
            .Distinct()
            // 排除提交人，避免与 NotifySubmitterAsync 重复
            .Where(id => id != submittedBy)
            .ToList();

        foreach (var approverId in approverIds)
        {
            var notification = new Notification(approverId, "Approval",
                title, content, "ApprovalRequest", approvalRequestId, request.CompanyId);
            await CreateAsync(notification, ct);
        }
    }

    public async Task NotifyRoleAsync(string roleCode, string title, string? content,
        string? referenceType = null, Guid? referenceId = null, CancellationToken ct = default)
    {
        var role = await _uow.Roles.GetByCodeAsync(roleCode, ct);
        if (role == null) return;

        await NotifyRoleByIdAsync(role.Id, title, content, referenceType, referenceId, null, ct);
    }

    // =====================================================================
    // 内部辅助方法
    // =====================================================================

    private async Task NotifyRoleByIdAsync(Guid roleId, string title, string? content,
        string? referenceType, Guid? referenceId, Guid? companyId, CancellationToken ct)
    {
        var allUsers = await _uow.Users.GetAllWithRolesAsync(ct);
        var userIds = allUsers
            .Where(u => u.Roles.Any(ur => ur.RoleId == roleId) && u.IsActive)
            .Select(u => u.Id)
            .ToList();

        foreach (var userId in userIds)
        {
            var notification = new Notification(userId, "Approval",
                title, content, referenceType, referenceId, companyId);
            await CreateAsync(notification, ct);
        }
    }
}
