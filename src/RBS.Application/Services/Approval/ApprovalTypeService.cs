using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Core.Entities.Approval;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Approval;

public class ApprovalTypeService : IApprovalTypeService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public ApprovalTypeService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }

    private Guid CompanyId => _tenant.EffectiveCompanyId ?? _tenant.HomeCompanyId ?? Guid.Empty;

    public async Task<List<ApprovalTypeDto>> GetListAsync(CancellationToken ct = default)
    {
        var items = await _uow.ApprovalTypes.GetAllAsync(ct);
        return items.Select(a => new ApprovalTypeDto
        {
            Id = a.Id, Name = a.Name, Code = a.Code,
            Description = a.Description, IsActive = a.IsActive, CompanyId = a.CompanyId
        }).ToList();
    }

    public async Task<ApprovalTypeDto> CreateAsync(CreateApprovalTypeRequest request, CancellationToken ct = default)
    {
        var entity = new ApprovalType(request.Name, request.Code, CompanyId);
        entity.SetDescription(request.Description);
        await _uow.ApprovalTypes.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return new ApprovalTypeDto { Id = entity.Id, Name = entity.Name, Code = entity.Code,
            Description = entity.Description, IsActive = entity.IsActive, CompanyId = entity.CompanyId };
    }

    public async Task UpdateAsync(Guid id, UpdateApprovalTypeRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalTypes.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批类型不存在");
        if (request.Name != null) entity.Rename(request.Name);
        if (request.Description != null) entity.SetDescription(request.Description);
        if (request.IsActive.HasValue)
        { if (request.IsActive.Value) entity.Activate(); else entity.Deactivate(); }
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalTypes.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批类型不存在");
        await _uow.ApprovalTypes.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    // === 审批级别 ===
    public async Task<List<ApprovalLevelConfigDto>> GetLevelsAsync(Guid typeId, CancellationToken ct = default)
    {
        var all = await _uow.ApprovalLevelConfigs.GetAllAsync(ct);
        var levels = all.Where(l => l.ApprovalTypeId == typeId).OrderBy(l => l.Level).ToList();
        var result = new List<ApprovalLevelConfigDto>();
        foreach (var l in levels)
        {
            var dto = new ApprovalLevelConfigDto
            {
                Id = l.Id, ApprovalTypeId = l.ApprovalTypeId, Level = l.Level,
                RoleId = l.RoleId, MinAmount = l.MinAmount, MaxAmount = l.MaxAmount
            };
            var role = await _uow.Roles.GetByIdAsync(l.RoleId, ct);
            dto.RoleName = role?.Name;
            result.Add(dto);
        }
        return result;
    }

    public async Task<ApprovalLevelConfigDto> CreateLevelAsync(CreateApprovalLevelRequest request, CancellationToken ct = default)
    {
        var entity = new ApprovalLevelConfig(request.ApprovalTypeId, request.Level, request.RoleId, CompanyId);
        entity.SetAmountRange(request.MinAmount, request.MaxAmount);
        await _uow.ApprovalLevelConfigs.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return new ApprovalLevelConfigDto
        {
            Id = entity.Id, ApprovalTypeId = entity.ApprovalTypeId, Level = entity.Level,
            RoleId = entity.RoleId, MinAmount = entity.MinAmount, MaxAmount = entity.MaxAmount
        };
    }

    public async Task UpdateLevelAsync(Guid id, UpdateApprovalLevelRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalLevelConfigs.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批级别不存在");
        if (request.Level.HasValue) entity.SetLevel(request.Level.Value);
        if (request.RoleId.HasValue) entity.SetRole(request.RoleId.Value);
        if (request.MinAmount.HasValue || request.MaxAmount.HasValue)
            entity.SetAmountRange(request.MinAmount, request.MaxAmount);
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteLevelAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.ApprovalLevelConfigs.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("审批级别不存在");
        await _uow.ApprovalLevelConfigs.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }
}
