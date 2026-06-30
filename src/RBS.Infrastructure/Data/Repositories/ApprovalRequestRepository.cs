using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Approval;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class ApprovalRequestRepository : BaseRepository<ApprovalRequest>, IApprovalRequestRepository
{
    public ApprovalRequestRepository(AppDbContext context) : base(context) { }

    public async Task<List<ApprovalRequest>> GetPendingByApproverAsync(Guid userId, CancellationToken ct = default)
    {
        // 查询用户角色
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);

        if (roleIds.Count == 0)
            return new List<ApprovalRequest>();

        // 查询用户角色可以审批的等级配置
        var levelConfigs = await _context.Set<ApprovalLevelConfig>()
            .Where(lc => roleIds.Contains(lc.RoleId))
            .Select(lc => new { lc.ApprovalTypeId, lc.Level })
            .ToListAsync(ct);

        if (levelConfigs.Count == 0)
            return new List<ApprovalRequest>();

        // 查询当前等级匹配的待审批请求（含 Records 导航属性）
        var results = new List<ApprovalRequest>();
        foreach (var config in levelConfigs)
        {
            var items = await _dbSet
                .Include(a => a.Records)
                .Where(a => a.Status == "Pending"
                    && a.ApprovalTypeId == config.ApprovalTypeId
                    && a.CurrentLevel == config.Level)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
            results.AddRange(items);
        }

        return results.Distinct().OrderByDescending(a => a.CreatedAt).ToList();
    }

    /// <summary>获取审批历史（分页）—— 我作为审批人操作过的记录</summary>
    public async Task<PagedResult<ApprovalRequest>> GetHistoryAsync(
        Guid userId, string? keyword, string? status,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(a => a.Records)
            .Where(a => a.Records.Any(r => r.ApproverId == userId
                && (r.Action == "Approved" || r.Action == "Rejected")));

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(a => a.Title.Contains(keyword));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ApprovalRequest>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    /// <summary>查询用户参与过的所有审批请求</summary>
    public async Task<List<ApprovalRequest>> GetByApproverAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(a => a.Records)
            .Where(a => a.Records.Any(r => r.ApproverId == userId && r.Action == "Submitted"))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    /// <summary>获取审批请求详情（含 Records）</summary>
    public async Task<ApprovalRequest?> GetByIdWithRecordsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(a => a.Records)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<ApprovalRequest>> GetByTargetAsync(
        Guid targetEntityId, string targetEntityType, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(a => a.TargetEntityId == targetEntityId && a.TargetEntityType == targetEntityType)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}
