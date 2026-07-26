using Dapper;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.Common;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Core.DomainServices;

namespace RBS.Application.Services.Approval;

/// <summary>
/// 审批应用服务实现
/// 写操作（Approve/Reject/Cancel）使用原始 SQL + 显式事务，
/// 读操作使用 Dapper 仓储。
/// </summary>
public class ApprovalService : IApprovalService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlLoader _sql;
    private readonly IApprovalDomainService _approvalDomain;
    private readonly IApprovalNumberGenerator _approvalNoGenerator;
    private readonly IApprovalBizDetailBuilder _bizDetailBuilder;
    private readonly IAuditLogWriter _auditWriter;
    private readonly IClientInfoService _clientInfo;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="uow">工作单元</param>
    /// <param name="tenantService">租户服务，获取默认公司 ID</param>
    /// <param name="currentUserService">当前用户服务，获取操作用户 ID</param>
    /// <param name="connectionFactory">数据库连接工厂</param>
    /// <param name="serviceProvider">服务提供者（延迟获取事件处理器）</param>
    /// <param name="sql">SQL 加载器</param>
    /// <param name="approvalDomain">审批领域服务</param>
    public ApprovalService(
        IUnitOfWork uow,
        ITenantService tenantService,
        ICurrentUserService currentUserService,
        IDbConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ISqlLoader sql,
        IApprovalDomainService approvalDomain,
        IApprovalNumberGenerator approvalNoGenerator,
        IApprovalBizDetailBuilder bizDetailBuilder,
        IAuditLogWriter auditWriter,
        IClientInfoService clientInfo)
    {
        _uow = uow;
        _tenantService = tenantService;
        _connectionFactory = connectionFactory;
        _currentUserService = currentUserService;
        _serviceProvider = serviceProvider;
        _sql = sql;
        _approvalDomain = approvalDomain;
        _approvalNoGenerator = approvalNoGenerator;
        _bizDetailBuilder = bizDetailBuilder;
        _auditWriter = auditWriter;
        _clientInfo = clientInfo;
    }

    // =====================================================================
    // 写操作：SubmitAsync
    // 包含并发守卫（同一业务实体不能有两个待审批） + 事件分发
    // =====================================================================

    /// <summary>
    /// 提交审批请求 — 含并发守卫，自动提交后通知第一级审批人
    /// </summary>
    public async Task<ApprovalRequestDto> SubmitAsync(SubmitApprovalRequest request, CancellationToken ct = default)
    {
        // ===== 并发守卫：同一业务实体+同类型不能有两个待审批 =====
        using (var guardConn = _connectionFactory.CreateConnection())
        {
            guardConn.Open();
            var pending = await guardConn.QuerySingleAsync<int>(
                _sql.Get("Approval.Select.Request.PendingCount"),
                new { Id = request.TargetEntityId, Type = request.TargetEntityType });
            if (pending > 0)
                throw new InvalidOperationException("该业务已有待审批的申请，请处理完成后再提交");
        }

        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var typeLevels = levels.Where(l => l.ApprovalTypeId == request.ApprovalTypeId).ToList();
        var maxLevel = typeLevels.Count > 0 ? typeLevels.Max(l => l.LevelNo) : 0;

        var entity = new ApprovalRequest(
            request.ApprovalTypeId,
            request.Title,
            request.TargetEntityId,
            request.TargetEntityType,
            _tenantService.DefaultCompanyId,
            maxLevel);

        entity.SetCreated(_currentUserService.UserId, ChinaTime.Now, null, null);
        entity.SetRequestNo(await _approvalNoGenerator.GenerateRequestNo());
        entity.AddRecord(_currentUserService.UserId, "Submitted", request.Description);
        await _uow.ApprovalRequests.AddAsync(entity, ct);

        // [领域] 通过领域服务执行提交状态变迁（Draft → Pending，若0级则自动 Approved）
        _approvalDomain.SubmitRequest(entity);

        var record = entity.Records.First();
        using (var updateConn = _connectionFactory.CreateConnection())
        {
            updateConn.Open();
            // 插入审批记录（Submitted），与状态更新共用同一连接
            await Dapper.SqlMapper.ExecuteAsync(updateConn,
                _sql.Get("Approval.Insert.Record.Default"),
                new { record.Id, ApprovalRequestId = entity.Id, Level = record.LevelNo, record.ApproverId, record.Action, Comment = record.Comment ?? "", record.CreatedBy, record.CreatedAt });

            await Dapper.SqlMapper.ExecuteAsync(updateConn,
                _sql.Get("Approval.Update.Request.SetStatus"),
                new { entity.Status, Id = entity.Id });
        }

        // [事件] 提交后通知第一级审批人
        if (entity.Status == "Pending")
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalSubmittedEvent>>();
            await handler.HandleAsync(
                new ApprovalSubmittedEvent(entity.Id, entity.ApprovalTypeId,
                    entity.TargetEntityId, entity.TargetEntityType, entity.Title),
                ct);
        }

        return await MapToDtoAsync(entity, ct);
    }

    /// <summary>
    /// 重新提交已撤回的审批 — 将状态从 Cancelled 改为 Pending，重置级别并添加提交记录
    /// </summary>
    public async Task<ApprovalRequestDto> ResubmitAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdWithRecordsAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Cancelled")
            throw new InvalidOperationException("仅已撤回的审批可以重新提交");

        var submitRecord = entity.Records.FirstOrDefault(r => r.Action == "Submitted");
        if (submitRecord == null || submitRecord.ApproverId != _currentUserService.UserId)
            throw new InvalidOperationException("仅提交人可以重新提交");

        var now = ChinaTime.Now;
        var userId = _currentUserService.UserId;

        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Update.Request.Resubmit"),
                new object[] { userId, now, id }, ct);

            var recordId = Guid.NewGuid();
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Insert.Record.Raw"),
                new object[] { recordId, id, 0, userId, "Submitted", "重新提交", userId, now }, ct);

            await tx.CommitAsync(ct);

            // ★ 审计：记录重提交变更
            var ip = _clientInfo.GetClientIp();
            var hostname = _clientInfo.GetClientHostname();
            var entityDict = AuditDict(entity, new Dictionary<string, object?>
            {
                ["Status"] = "Pending",
                ["CurrentLevel"] = 1,
                ["UpdatedBy"] = userId,
                ["UpdatedAt"] = now,
                ["UpdatedIp"] = ip,
                ["UpdatedHostname"] = hostname
            });
            await _auditWriter.LogChangesAsync("ApprovalRequests", id.ToString(), "Update", entityDict, userId, ct);
            await _auditWriter.LogChangesAsync("ApprovalRecords", recordId.ToString(), "Create",
                new Dictionary<string, object?>
                {
                    ["Id"] = recordId, ["RequestId"] = id, ["LevelNo"] = 0,
                    ["ApproverId"] = userId, ["Action"] = "Submitted",
                    ["Comment"] = "重新提交", ["CreatedBy"] = userId, ["CreatedAt"] = now
                }, userId, ct);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        // 触发提交事件，通知下一级审批人
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider
            .GetRequiredService<IEventHandler<ApprovalSubmittedEvent>>();
        await handler.HandleAsync(
            new ApprovalSubmittedEvent(entity.Id, entity.ApprovalTypeId,
                entity.TargetEntityId, entity.TargetEntityType, entity.Title),
            ct);

        return await MapToDtoAsync(entity, ct);
    }

    // =====================================================================
    // 写操作：ApproveAsync / RejectAsync / CancelAsync 使用原始 SQL
    // =====================================================================

    /// <summary>
    /// 审批通过 — 终审时触发业务回调事件，非终审则推进到下一级
    /// </summary>
    public async Task<ApprovalRequestDto> ApproveAsync(Guid id, string? comment, CancellationToken ct = default)
    {
        // [读] 加载实体，验证状态
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("该审批已处理，请刷新后重试");

        var userId = _currentUserService.UserId;
        var now = ChinaTime.Now;

        // [领域] 通过领域服务执行审批流转
        var result = await _approvalDomain.ApproveAsync(entity, userId, comment, ct);

        // [写] 原始 SQL + 显式事务
        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            // 状态变迁：终审设 Status，非终审进 CurrentLevel+1
            string updateSql;
            if (result.IsCompleted)
            {
                updateSql = _sql.Get("Approval.Update.Request.ToApproved");
            }
            else
            {
                updateSql = _sql.Get("Approval.Update.Request.AdvanceLevel");
            }

            var ip = _clientInfo.GetClientIp();
            var hostname = _clientInfo.GetClientHostname();
            var rows = await _uow.ExecuteSqlRawAsync(updateSql,
                new object[] { userId, now, id, ip, hostname }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            // 插入审批记录（读取领域服务在聚合根上添加的记录）
            var newRecord = entity.LatestRecord!;
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Insert.Record.Raw"),
                new object[] { newRecord.Id, id, newRecord.LevelNo, userId, result.Action,
                    comment ?? "", userId, now }, ct);

            await tx.CommitAsync(ct);

            // ★ 审计：记录审批变更
            var newStatus = result.IsCompleted ? "Approved" : "Pending";
            var newLevel = result.IsCompleted ? entity.MaxLevel : entity.CurrentLevel + 1;
            var entityDict = AuditDict(entity, new Dictionary<string, object?>
            {
                ["Status"] = newStatus,
                ["CurrentLevel"] = newLevel,
                ["UpdatedBy"] = userId,
                ["UpdatedAt"] = now,
                ["UpdatedIp"] = ip,
                ["UpdatedHostname"] = hostname
            });
            await _auditWriter.LogChangesAsync("ApprovalRequests", id.ToString(), "Update", entityDict, userId, ct);
            await _auditWriter.LogChangesAsync("ApprovalRecords", newRecord.Id.ToString(), "Create",
                new Dictionary<string, object?>
                {
                    ["Id"] = newRecord.Id, ["RequestId"] = id, ["LevelNo"] = newRecord.LevelNo,
                    ["ApproverId"] = userId, ["Action"] = result.Action,
                    ["Comment"] = comment ?? "", ["CreatedBy"] = userId, ["CreatedAt"] = now
                }, userId, ct);
        }
        catch (InvalidOperationException) { throw; }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        // [事件] 终审时手动分发领域事件
        if (result.IsCompleted)
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalCompletedEvent>>();
            await handler.HandleAsync(
                new ApprovalCompletedEvent(id, entity.TargetEntityId, entity.TargetEntityType, result.Action),
                ct);
        }
        else
        {
            // 非终审：通知下一级审批人
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider
                .GetRequiredService<IEventHandler<ApprovalLevelAdvancedEvent>>();
            await handler.HandleAsync(
                new ApprovalLevelAdvancedEvent(id, result.NextLevel!.Value),
                ct);
        }

        // [读] 重新加载实体
        await _uow.ReloadAsync(entity, ct);
        return await MapToDtoAsync(entity, ct);
    }

    /// <summary>
    /// 审批驳回 — 驳回即为终审，触发审批完成事件
    /// </summary>
    public async Task<ApprovalRequestDto> RejectAsync(Guid id, string comment, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("该审批已处理，请刷新后重试");

        var userId = _currentUserService.UserId;
        var now = ChinaTime.Now;

        // [领域] 通过领域服务执行审批驳回
        var result = await _approvalDomain.RejectAsync(entity, userId, comment, ct);

        // [写] 原始 SQL + 显式事务
        using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var ip = _clientInfo.GetClientIp();
            var hostname = _clientInfo.GetClientHostname();
            var rows = await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Update.Request.ToRejected"),
                new object[] { userId, now, id, ip, hostname }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            // 插入审批记录（读取领域服务在聚合根上添加的记录）
            var newRecord = entity.LatestRecord!;
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Insert.Record.Raw"),
                new object[] { newRecord.Id, id, newRecord.LevelNo, userId, result.Action,
                    comment, userId, now }, ct);

            await tx.CommitAsync(ct);

            // ★ 审计：记录驳回
            var entityDict = AuditDict(entity, new Dictionary<string, object?>
            {
                ["Status"] = "Rejected",
                ["UpdatedBy"] = userId,
                ["UpdatedAt"] = now,
                ["UpdatedIp"] = ip,
                ["UpdatedHostname"] = hostname
            });
            await _auditWriter.LogChangesAsync("ApprovalRequests", id.ToString(), "Update", entityDict, userId, ct);
            await _auditWriter.LogChangesAsync("ApprovalRecords", newRecord.Id.ToString(), "Create",
                new Dictionary<string, object?>
                {
                    ["Id"] = newRecord.Id, ["RequestId"] = id, ["LevelNo"] = newRecord.LevelNo,
                    ["ApproverId"] = userId, ["Action"] = result.Action,
                    ["Comment"] = comment, ["CreatedBy"] = userId, ["CreatedAt"] = now
                }, userId, ct);
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
            new ApprovalCompletedEvent(id, entity.TargetEntityId, entity.TargetEntityType, result.Action),
            ct);

        // [读] 重新加载实体
        await _uow.ReloadAsync(entity, ct);
        return await MapToDtoAsync(entity, ct);
    }

    /// <summary>
    /// 撤回审批请求 — 仅提交人可操作，关联导入批次时同步更新批次状态
    /// </summary>
    public async Task<ApprovalRequestDto> CancelAsync(Guid id, string? reason = null, CancellationToken ct = default)
    {
        // [读] 加载实体（含 Records，需校验提交人）
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
            var ip = _clientInfo.GetClientIp();
            var hostname = _clientInfo.GetClientHostname();
            var rows = await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Update.Request.ToCancelled"),
                new object[] { userId, now, id, ip, hostname }, ct);
            if (rows == 0)
                throw new InvalidOperationException("该审批已被其他人处理，请刷新后查看");

            // 插入"撤回"记录
            var recordId = Guid.NewGuid();
            await _uow.ExecuteSqlRawAsync(
                _sql.Get("Approval.Insert.Record.Raw"),
                new object[] { recordId, id, entity.CurrentLevel, userId, "Cancelled",
                    reason ?? "提交人撤回", userId, now }, ct);

            // 如果关联的是导入批次，同时更新批次状态
            if (entity.TargetEntityType == "Import")
            {
                await _uow.ExecuteSqlRawAsync(
                    _sql.Get("Approval.Update.ImportBatch.Cancelled"),
                    new object[] { userId, now, entity.TargetEntityId }, ct);
            }

            await tx.CommitAsync(ct);

            // ★ 审计：记录撤回
            var entityDict = AuditDict(entity, new Dictionary<string, object?>
            {
                ["Status"] = "Cancelled",
                ["UpdatedBy"] = userId,
                ["UpdatedAt"] = now,
                ["UpdatedIp"] = ip,
                ["UpdatedHostname"] = hostname
            });
            await _auditWriter.LogChangesAsync("ApprovalRequests", id.ToString(), "Update", entityDict, userId, ct);
            await _auditWriter.LogChangesAsync("ApprovalRecords", recordId.ToString(), "Create",
                new Dictionary<string, object?>
                {
                    ["Id"] = recordId, ["RequestId"] = id, ["LevelNo"] = entity.CurrentLevel,
                    ["ApproverId"] = userId, ["Action"] = "Cancelled",
                    ["Comment"] = reason ?? "提交人撤回", ["CreatedBy"] = userId, ["CreatedAt"] = now
                }, userId, ct);
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
    // 读操作
    // =====================================================================

    /// <summary>
    /// 获取当前用户可审批的待审批列表
    /// </summary>
    public async Task<List<ApprovalRequestDto>> GetPendingAsync(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        var list = await _uow.ApprovalRequests.GetPendingByApproverAsync(userId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var item in list)
            dtos.Add(await MapToDtoAsync(item, ct));
        return dtos;
    }

    /// <summary>
    /// 获取当前用户提交的审批请求
    /// </summary>
    public async Task<List<ApprovalRequestDto>> GetMyRequestsAsync(CancellationToken ct = default)
    {
        var list = await _uow.ApprovalRequests.GetByApproverAsync(_currentUserService.UserId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var item in list)
            dtos.Add(await MapToDtoAsync(item, ct));
        return dtos;
    }

    /// <summary>
    /// 根据 ID 获取审批请求详情（含审批记录）
    /// </summary>
    public async Task<ApprovalRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdWithRecordsAsync(id, ct);
        if (entity == null) return null;
        return await MapToDtoAsync(entity, ct);
    }

    /// <summary>
    /// 分页获取当前用户的审批历史记录
    /// </summary>
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

    /// <summary>
    /// 获取指定业务实体的最近一次被驳回的审批数据（用于重新提交预填）
    /// </summary>
    public async Task<LastRejectedApprovalDto?> GetLastRejectedAsync(Guid targetEntityId, string targetEntityType, CancellationToken ct = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        return await conn.QuerySingleOrDefaultAsync<LastRejectedApprovalDto>(
            _sql.Get("Approval.Select.Request.LastRejected"),
            new { Id = targetEntityId, Type = targetEntityType });
    }

    /// <summary>
    /// 获取审批业务详情（新旧对比数据）
    /// 优先从 ApprovalBizData 结构化数据构建，无结构化数据时回退 Description 正则解析
    /// 按业务类型分发：RENT_ADJUST / FEE_ADJUST / TERMINATE / ContractRenewal / ContractActivation
    /// </summary>
    public async Task<ApprovalBizDetailDto?> GetBizDetailAsync(Guid id, CancellationToken ct = default)
    {
        var approval = await _uow.ApprovalRequests.GetByIdAsync(id, ct);
        if (approval == null) return null;
        return await _bizDetailBuilder.GetBizDetailAsync(approval);
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
        if (entity.CreatedBy != Guid.Empty && !approverIds.Contains(entity.CreatedBy))
            approverIds.Add(entity.CreatedBy);
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
            l.ApprovalTypeId == entity.ApprovalTypeId && l.LevelNo == entity.CurrentLevel);
        if (currentLevelConfig != null)
        {
            var role = await _uow.Roles.GetByIdAsync(currentLevelConfig.ApproverRoleId, ct);
            if (role != null) currentLevelName = $"{role.Name}审批";
        }

        var lastRecord = entity.Records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

        var levelChain = new List<ApprovalLevelStatusDto>();
        var submitRecord = entity.Records.FirstOrDefault(r => r.Action == "Submitted");
        var submitterId = submitRecord?.ApproverId;
        if (submitterId == null || submitterId.Value == Guid.Empty)
            submitterId = entity.CreatedBy != Guid.Empty ? entity.CreatedBy : null;

        if (submitterId.HasValue)
        {
            var submitterInfo = userDict.GetValueOrDefault(submitterId.Value);
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
            .OrderBy(l => l.LevelNo).ToList();

        foreach (var lc in allLevelConfigs)
        {
            var role = await _uow.Roles.GetByIdAsync(lc.ApproverRoleId, ct);
            var roleName = role?.Name ?? "未知角色";
            var approvedRecord = entity.Records.FirstOrDefault(r => r.LevelNo == lc.LevelNo && r.Action != "Submitted");

            string status;
            if (approvedRecord != null)
                status = approvedRecord.Action switch
                {
                    "Rejected" => "rejected",
                    "Cancelled" => "cancelled",
                    _ => "completed"
                };
            else if (lc.LevelNo == entity.CurrentLevel && entity.Status == "Pending")
                status = "current";
            else if (entity.Status is "Approved" or "Rejected" || lc.LevelNo < entity.CurrentLevel)
                status = "skipped";
            else
                status = "pending";

            string? expectedNames = null;
            if (approvedRecord == null && roleUserMap.TryGetValue(lc.ApproverRoleId, out var usersWithRole))
            {
                expectedNames = string.Join("、", usersWithRole.Select(u => $"{u.Name}({u.Account})"));
            }

            levelChain.Add(new ApprovalLevelStatusDto
            {
                Level = lc.LevelNo,
                RoleName = roleName,
                Status = status,
                ApproverName = approvedRecord != null
                    ? userDict.GetValueOrDefault(approvedRecord.ApproverId).Name : expectedNames,
                ApproverAccount = approvedRecord != null
                    ? userDict.GetValueOrDefault(approvedRecord.ApproverId).Account : null
            });
        }

        return new ApprovalRequestDto
        {
            Id = entity.Id,
            RequestNo = entity.RequestNo,
            ApprovalTypeId = entity.ApprovalTypeId,
            Title = entity.Title,
            Description = entity.Description,
            TargetEntityId = entity.TargetEntityId,
            TargetEntityType = entity.TargetEntityType,
            Status = entity.Status,
            CurrentLevel = entity.CurrentLevel,
            MaxLevel = entity.MaxLevel,
            ApprovalTypeName = typeName,
            SubmitterName = entity.CreatedBy != Guid.Empty
                ? userDict.GetValueOrDefault(entity.CreatedBy).Name : null,
            CurrentLevelName = currentLevelName,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt,
            LevelChain = levelChain,
            Records = entity.Records.OrderBy(r => r.CreatedAt).Select(r =>
            {
                var info = userDict.GetValueOrDefault(r.ApproverId);
                return new ApprovalRecordDto
                {
                    Id = r.Id,
                    Level = r.LevelNo,
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

    /// <summary>
    /// 构造审计字典 — 取实体的当前属性快照，并用 overrides 覆盖 SQL 变更后的值
    /// </summary>
    private static Dictionary<string, object?> AuditDict(object entity, Dictionary<string, object?> overrides)
    {
        var dict = new Dictionary<string, object?>();
        var props = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var p in props)
        {
            if (p.Name is "DomainEvents" or "RowVersion") continue;
            if (!p.CanWrite) continue;
            if (p.PropertyType == typeof(System.Collections.IList) || p.PropertyType.IsGenericType) continue;
            dict[p.Name] = p.GetValue(entity);
        }
        // 用 SQL 变更后的值覆盖
        foreach (var kv in overrides)
        {
            dict[kv.Key] = kv.Value;
        }
        return dict;
    }
}
