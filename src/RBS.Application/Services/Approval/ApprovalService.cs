using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Approval;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Approval;

/// <summary>
/// 审批应用服务实现
/// </summary>
public class ApprovalService : IApprovalService
{
    private readonly IUnitOfWork _uow;
    private readonly IApprovalDomainService _domainService;
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ApprovalService(
        IUnitOfWork uow,
        IApprovalDomainService domainService,
        ITenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _uow = uow;
        _domainService = domainService;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<ApprovalRequestDto> SubmitAsync(SubmitApprovalRequest request, CancellationToken ct = default)
    {
        // 获取审批类型对应的级别配置
        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var typeLevels = levels.Where(l => l.ApprovalTypeId == request.ApprovalTypeId).ToList();
        var maxLevel = typeLevels.Count > 0 ? typeLevels.Max(l => l.Level) : 0;

        // 构造审批请求
        var entity = new ApprovalRequest(
            request.ApprovalTypeId,
            request.Title,
            request.TargetEntityId,
            request.TargetEntityType,
            _tenantService.DefaultCompanyId,
            maxLevel);

        // 记录提交者为第一条"提交"记录
        entity.AddRecord(_currentUserService.UserId, "Submitted", request.Description);

        await _uow.ApprovalRequests.AddAsync(entity, ct);

        // 提交（Draft → Pending，若0级则自动 Approved）
        _domainService.SubmitRequest(entity);

        await _uow.CommitAsync(ct);
        return await MapToDtoAsync(entity, ct);
    }

    public async Task<ApprovalRequestDto> ApproveAsync(Guid id, string? comment, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("该审批已处理，请刷新后重试");

        var result = await _domainService.ApproveAsync(entity, _currentUserService.UserId, comment, ct);
        await _uow.CommitAsync(ct);
        return await MapToDtoAsync(entity, ct);
    }

    public async Task<ApprovalRequestDto> RejectAsync(Guid id, string comment, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalRequests.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批请求不存在");

        if (entity.Status != "Pending")
            throw new InvalidOperationException("该审批已处理，请刷新后重试");

        var result = await _domainService.RejectAsync(entity, _currentUserService.UserId, comment, ct);
        await _uow.CommitAsync(ct);
        return await MapToDtoAsync(entity, ct);
    }

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

    private async Task<ApprovalRequestDto> MapToDtoAsync(ApprovalRequest entity, CancellationToken ct)
    {
        // 加载审批类型名称
        string? typeName = null;
        if (entity.ApprovalTypeId != Guid.Empty)
        {
            var type = await _uow.ApprovalTypes.GetByIdAsync(entity.ApprovalTypeId, ct);
            typeName = type?.Name;
        }

        // 加载审批人名称
        var approverIds = entity.Records.Select(r => r.ApproverId).Distinct().ToList();
        var userDict = new Dictionary<Guid, string>();
        if (approverIds.Count > 0)
        {
            foreach (var uid in approverIds)
            {
                var user = await _uow.Users.GetByIdAsync(uid, ct);
                if (user != null) userDict[uid] = user.DisplayName;
            }
        }

        // 获取当前等级名称
        string? currentLevelName = null;
        var levels = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var currentLevelConfig = levels.FirstOrDefault(l =>
            l.ApprovalTypeId == entity.ApprovalTypeId && l.Level == entity.CurrentLevel);
        if (currentLevelConfig != null)
        {
            var role = await _uow.Roles.GetByIdAsync(currentLevelConfig.RoleId, ct);
            if (role != null) currentLevelName = $"{role.Name}审批";
        }

        // 完成时间（取最后一条记录的时间）
        var lastRecord = entity.Records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

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
                ? userDict.GetValueOrDefault(submitterId) : null,
            CurrentLevelName = currentLevelName,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.Status is "Approved" or "Rejected" ? lastRecord?.CreatedAt : null,
            Records = entity.Records.OrderBy(r => r.CreatedAt).Select(r => new ApprovalRecordDto
            {
                Id = r.Id,
                Level = r.Level,
                ApproverId = r.ApproverId,
                ApproverName = userDict.GetValueOrDefault(r.ApproverId, r.ApproverId.ToString()),
                Action = r.Action,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }
}
