using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.Common;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Approval;

/// <summary>
/// 审批应用服务实现
/// 写操作（Approve/Reject/Cancel）使用原始 SQL + 显式事务，完全绕过 EF Core SaveChanges 管道，
/// 避免 MirrorAuditInterceptor / DomainEventDispatcher / SqlServerRetryExecutionStrategy 的交互问题。
/// 读操作保持 EF Core 不变。
/// </summary>
public class ApprovalService : IApprovalService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceProvider _serviceProvider;

    public ApprovalService(
        IUnitOfWork uow,
        ITenantService tenantService,
        ICurrentUserService currentUserService,
        IServiceProvider serviceProvider)
    {
        _uow = uow;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
        _serviceProvider = serviceProvider;
    }

    // =====================================================================
    // 写操作：SubmitAsync 保持使用 EF Core（无并发冲突风险）
    // =====================================================================

    public async Task<ApprovalRequestDto> SubmitAsync(SubmitApprovalRequest request, CancellationToken ct = default)
    {
        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var typeLevels = levels.Where(l => l.ApprovalTypeId == request.ApprovalTypeId).ToList();
        var maxLevel = typeLevels.Count > 0 ? typeLevels.Max(l => l.Level) : 0;

        var entity = new ApprovalRequest(
            request.ApprovalTypeId,
            request.Title,
            request.TargetEntityId,
            request.TargetEntityType,
            _tenantService.DefaultCompanyId,
            maxLevel);

        entity.AddRecord(_currentUserService.UserId, "Submitted", request.Description);
        await _uow.ApprovalRequests.AddAsync(entity, ct);

        // 提交（Draft → Pending，若0级则自动 Approved）
        entity.Submit();

        await _uow.CommitAsync(ct);
        return await MapToDtoAsync(entity, ct);
    }

    // =====================================================================
    // 写操作：ApproveAsync / RejectAsync / CancelAsync 使用原始 SQL
    // =====================================================================

    public async Task<ApprovalRequestDto> ApproveAsync(Guid id, string? comment, CancellationToken ct = default)
    {
        // [读] 用 EF Core 加载实体，验证状态
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("该审批已处理，请刷新后重试");

        var now = ChinaTime.Now;
        var userId = _currentUserService.UserId;
        var isFinalLevel = entity.CurrentLevel >= entity.MaxLevel;

        // [写] 原始 SQL + 显式事务
        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            // 状态变迁：终审设 Status，非终审进 CurrentLevel+1
            string updateSql;
            if (isFinalLevel)
            {
                updateSql = @"UPDATE ApprovalRequests SET Status = 'Approved',
                              UpdatedBy = @p0, UpdatedAt = @p1
                              WHERE Id = @p2 AND Status = 'Pending'";
            }
            else
            {
                updateSql = @"UPDATE ApprovalRequests SET CurrentLevel = CurrentLevel + 1,
                              UpdatedBy = @p0, UpdatedAt = @p1
                              WHERE Id = @p2 AND Status = 'Pending'";
            }

            var rows = await _uow.ExecuteSqlRawAsync(updateSql,
                new object[] { userId, now, id }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            // 插入审批记录
            var recordId = Guid.NewGuid();
            await _uow.ExecuteSqlRawAsync(
                @"INSERT INTO ApprovalRecords (Id, ApprovalRequestId, Level, ApproverId, Action, Comment, CreatedBy, CreatedAt)
                  VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)",
                new object[] { recordId, id, entity.CurrentLevel, userId, "Approved",
                    comment ?? "", userId, now }, ct);

            await tx.CommitAsync(ct);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        // [事件] 终审时手动分发领域事件
        if (isFinalLevel)
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalCompletedEvent>>();
            await handler.HandleAsync(
                new ApprovalCompletedEvent(id, entity.TargetEntityId, entity.TargetEntityType, "Approved"),
                ct);
        }

        // [读] 重新加载实体
        await _uow.ReloadAsync(entity, ct);
        return await MapToDtoAsync(entity, ct);
    }

    public async Task<ApprovalRequestDto> RejectAsync(Guid id, string comment, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("该审批已处理，请刷新后重试");

        var now = ChinaTime.Now;
        var userId = _currentUserService.UserId;

        // [写] 原始 SQL + 显式事务
        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var rows = await _uow.ExecuteSqlRawAsync(
                @"UPDATE ApprovalRequests SET Status = 'Rejected',
                  UpdatedBy = @p0, UpdatedAt = @p1
                  WHERE Id = @p2 AND Status = 'Pending'",
                new object[] { userId, now, id }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            var recordId = Guid.NewGuid();
            await _uow.ExecuteSqlRawAsync(
                @"INSERT INTO ApprovalRecords (Id, ApprovalRequestId, Level, ApproverId, Action, Comment, CreatedBy, CreatedAt)
                  VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)",
                new object[] { recordId, id, entity.CurrentLevel, userId, "Rejected",
                    comment, userId, now }, ct);

            await tx.CommitAsync(ct);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        // [事件] 驳回即为终审
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider
            .GetRequiredService<IEventHandler<ApprovalCompletedEvent>>();
        await handler.HandleAsync(
            new ApprovalCompletedEvent(id, entity.TargetEntityId, entity.TargetEntityType, "Rejected"),
            ct);

        // [读] 重新加载实体
        await _uow.ReloadAsync(entity, ct);
        return await MapToDtoAsync(entity, ct);
    }

    public async Task<ApprovalRequestDto> CancelAsync(Guid id, string? reason = null, CancellationToken ct = default)
    {
        // [读] 用 EF Core 加载实体（含 Records，需校验提交人）
        var entity = await _uow.ApprovalRequests.GetByIdWithRecordsAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("只能撤回待审批的请求");

        var submitRecord = entity.Records.FirstOrDefault(r => r.Action == "Submitted");
        if (submitRecord == null || submitRecord.ApproverId != _currentUserService.UserId)
            throw new InvalidOperationException("仅提交人可以撤回");

        var now = ChinaTime.Now;
        var userId = _currentUserService.UserId;

        // [写] 原始 SQL + 显式事务（同时更新审批状态和关联的导入批次）
        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            // 更新审批状态
            var rows = await _uow.ExecuteSqlRawAsync(
                @"UPDATE ApprovalRequests SET Status = 'Cancelled',
                  UpdatedBy = @p0, UpdatedAt = @p1
                  WHERE Id = @p2 AND Status = 'Pending'",
                new object[] { userId, now, id }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            // 插入"撤回"记录
            var recordId = Guid.NewGuid();
            await _uow.ExecuteSqlRawAsync(
                @"INSERT INTO ApprovalRecords (Id, ApprovalRequestId, Level, ApproverId, Action, Comment, CreatedBy, CreatedAt)
                  VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)",
                new object[] { recordId, id, entity.CurrentLevel, userId, "Cancelled",
                    reason ?? "提交人撤回", userId, now }, ct);

            // 如果关联的是导入批次，同时更新批次状态
            if (entity.TargetEntityType == "Import")
            {
                await _uow.ExecuteSqlRawAsync(
                    @"UPDATE ImportBatches SET Status = 'Cancelled',
                      UpdatedBy = @p0, UpdatedAt = @p1
                      WHERE Id = @p2 AND Status = 'PendingApproval'",
                    new object[] { userId, now, entity.TargetEntityId }, ct);
            }

            await tx.CommitAsync(ct);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        // [读] 重新加载实体
        await _uow.ReloadAsync(entity, ct);
        return await MapToDtoAsync(entity, ct);
    }

    // =====================================================================
    // 读操作：保持 EF Core
    // =====================================================================

    public async Task<List<ApprovalRequestDto>> GetPendingAsync(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        var list = await _uow.ApprovalRequests.GetPendingByApproverAsync(userId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var item in list)
            dtos.Add(await MapToDtoAsync(item, ct));
        return dtos;
    }

    public async Task<List<ApprovalRequestDto>> GetMyRequestsAsync(CancellationToken ct = default)
    {
        var list = await _uow.ApprovalRequests.GetByApproverAsync(_currentUserService.UserId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var item in list)
            dtos.Add(await MapToDtoAsync(item, ct));
        return dtos;
    }

    public async Task<ApprovalRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdWithRecordsAsync(id, ct);
        if (entity == null) return null;
        return await MapToDtoAsync(entity, ct);
    }

    public async Task<PagedResult<ApprovalRequestDto>> GetHistoryAsync(
        ApprovalHistoryQuery query, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        var result = await _uow.ApprovalRequests.GetHistoryAsync(
            userId, query.Keyword, query.Status, query.Page, query.PageSize, ct);

        var items = new List<ApprovalRequestDto>();
        foreach (var entity in result.Items)
            items.Add(await MapToDtoAsync(entity, ct));

        return new PagedResult<ApprovalRequestDto>
        {
            Items = items,
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }

    private async Task<ApprovalRequestDto> MapToDtoAsync(ApprovalRequest entity, CancellationToken ct)
    {
        string? typeName = null;
        if (entity.ApprovalTypeId != Guid.Empty)
        {
            var type = await _uow.ApprovalTypes.GetByIdAsync(entity.ApprovalTypeId, ct);
            typeName = type?.Name;
        }

        var approverIds = entity.Records.Select(r => r.ApproverId).Distinct().ToList();
        var userDict = new Dictionary<Guid, (string Name, string Account)>();
        if (approverIds.Count > 0)
        {
            foreach (var uid in approverIds)
            {
                var user = await _uow.Users.GetByIdAsync(uid, ct);
                if (user != null) userDict[uid] = (user.DisplayName, user.Username);
            }
        }

        var allUsersWithRoles = await _uow.Users.GetAllWithRolesAsync(ct);
        var roleUserMap = new Dictionary<Guid, List<(string Name, string Account)>>();
        foreach (var u in allUsersWithRoles)
        {
            foreach (var ur in u.Roles)
            {
                if (!roleUserMap.ContainsKey(ur.RoleId))
                    roleUserMap[ur.RoleId] = new();
                roleUserMap[ur.RoleId].Add((u.DisplayName, u.Username));
            }
        }

        string? currentLevelName = null;
        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var currentLevelConfig = levels.FirstOrDefault(l =>
            l.ApprovalTypeId == entity.ApprovalTypeId && l.Level == entity.CurrentLevel);
        if (currentLevelConfig != null)
        {
            var role = await _uow.Roles.GetByIdAsync(currentLevelConfig.RoleId, ct);
            if (role != null) currentLevelName = $"{role.Name}审批";
        }

        var lastRecord = entity.Records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

        var levelChain = new List<ApprovalLevelStatusDto>();
        var submitRecord = entity.Records.FirstOrDefault(r => r.Action == "Submitted");
        if (submitRecord != null)
        {
            var submitterInfo = userDict.GetValueOrDefault(submitRecord.ApproverId);
            levelChain.Add(new ApprovalLevelStatusDto
            {
                Level = 0,
                RoleName = "提交审批",
                Status = "submitted",
                ApproverName = submitterInfo.Name,
                ApproverAccount = submitterInfo.Account
            });
        }

        var allLevelConfigs = levels
            .Where(l => l.ApprovalTypeId == entity.ApprovalTypeId)
            .OrderBy(l => l.Level).ToList();

        foreach (var lc in allLevelConfigs)
        {
            var role = await _uow.Roles.GetByIdAsync(lc.RoleId, ct);
            var roleName = role?.Name ?? "未知角色";
            var approvedRecord = entity.Records.FirstOrDefault(r => r.Level == lc.Level && r.Action != "Submitted");

            string status;
            if (approvedRecord != null)
                status = "completed";
            else if (lc.Level == entity.CurrentLevel && entity.Status == "Pending")
                status = "current";
            else if (entity.Status is "Approved" or "Rejected" || lc.Level < entity.CurrentLevel)
                status = "skipped";
            else
                status = "pending";

            string? expectedNames = null;
            string? expectedAccounts = null;
            if (approvedRecord == null && roleUserMap.TryGetValue(lc.RoleId, out var usersWithRole))
            {
                expectedNames = string.Join("、", usersWithRole.Select(u => u.Name));
                expectedAccounts = string.Join("、", usersWithRole.Select(u => u.Account));
            }

            levelChain.Add(new ApprovalLevelStatusDto
            {
                Level = lc.Level,
                RoleName = roleName,
                Status = status,
                ApproverName = approvedRecord != null
                    ? userDict.GetValueOrDefault(approvedRecord.ApproverId).Name : expectedNames,
                ApproverAccount = approvedRecord != null
                    ? userDict.GetValueOrDefault(approvedRecord.ApproverId).Account : expectedAccounts
            });
        }

        return new ApprovalRequestDto
        {
            Id = entity.Id,
            ApprovalTypeId = entity.ApprovalTypeId,
            Title = entity.Title,
            Description = entity.Description,
            TargetEntityId = entity.TargetEntityId,
            TargetEntityType = entity.TargetEntityType,
            Status = entity.Status,
            CurrentLevel = entity.CurrentLevel,
            MaxLevel = entity.MaxLevel,
            ApprovalTypeName = typeName,
            SubmitterName = entity.Records.FirstOrDefault()?.ApproverId is Guid submitterId
                ? userDict.GetValueOrDefault(submitterId).Name : null,
            CurrentLevelName = currentLevelName,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.Status is "Approved" or "Rejected" ? lastRecord?.CreatedAt : null,
            LevelChain = levelChain,
            Records = entity.Records.OrderBy(r => r.CreatedAt).Select(r =>
            {
                var info = userDict.GetValueOrDefault(r.ApproverId);
                return new ApprovalRecordDto
                {
                    Id = r.Id,
                    Level = r.Level,
                    ApproverId = r.ApproverId,
                    ApproverName = info.Name ?? r.ApproverId.ToString(),
                    ApproverAccount = info.Account ?? "",
                    Action = r.Action,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                };
            }).ToList()
        };
    }
}
