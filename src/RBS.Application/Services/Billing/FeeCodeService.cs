using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Billing;

public class FeeCodeService : IFeeCodeService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public FeeCodeService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }

    private Guid CompanyId => _tenant.EffectiveCompanyId ?? _tenant.HomeCompanyId ?? Guid.Empty;

    public async Task<List<FeeCodeDto>> GetListAsync(CancellationToken ct = default)
    {
        var items = await _uow.FeeCodes.GetAllAsync(ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task<FeeCodeDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _uow.FeeCodes.GetByIdAsync(id, ct);
        return item == null ? null : MapToDto(item);
    }

    public async Task<FeeCodeDto> CreateAsync(CreateFeeCodeRequest request, CancellationToken ct = default)
    {
        var entity = new FeeCode(request.Code, request.Name, CompanyId);
        entity.SetBillingMode(request.BillingMode);
        entity.SetUnit(request.Unit);
        entity.SetSortOrder(request.SortOrder);
        entity.SetCategory(request.Category);
        entity.SetRequired(request.IsRequired);
        await _uow.FeeCodes.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return MapToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateFeeCodeRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.FeeCodes.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("收费项目不存在");
        if (request.Name != null) entity.Rename(request.Name);
        if (request.Code != null) entity.SetCode(request.Code);
        if (request.BillingMode != null) entity.SetBillingMode(request.BillingMode);
        if (request.Unit != null) entity.SetUnit(request.Unit);
        if (request.SortOrder.HasValue) entity.SetSortOrder(request.SortOrder.Value);
        if (request.Category != null) entity.SetCategory(request.Category);
        if (request.IsRequired.HasValue) entity.SetRequired(request.IsRequired.Value);
        if (request.IsActive.HasValue) { if (request.IsActive.Value) entity.Activate(); else entity.Deactivate(); }
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.FeeCodes.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("收费项目不存在");
        await _uow.FeeCodes.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    private static FeeCodeDto MapToDto(FeeCode f) => new()
    {
        Id = f.Id, Code = f.Code, Name = f.Name, BillingMode = f.BillingMode,
        Unit = f.Unit, SortOrder = f.SortOrder, IsActive = f.IsActive,
        Category = f.Category, IsRequired = f.IsRequired, CompanyId = f.CompanyId
    };
}
