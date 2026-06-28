using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

public class TaxRateConfigService : ITaxRateConfigService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public TaxRateConfigService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }
    private Guid CompanyId => _tenant.EffectiveCompanyId ?? _tenant.HomeCompanyId ?? Guid.Empty;

    public async Task<List<TaxRateConfigDto>> GetListAsync(CancellationToken ct = default)
        => (await _uow.TaxRateConfigs.GetAllAsync(ct)).Select(MapToDto).ToList();

    public async Task<TaxRateConfigDto> CreateAsync(CreateTaxRateConfigRequest request, CancellationToken ct = default)
    {
        var entity = new TaxRateConfig(request.Name, request.Rate, request.EffectiveDate, CompanyId);
        await _uow.TaxRateConfigs.AddAsync(entity, ct); await _uow.CommitAsync(ct);
        return MapToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateTaxRateConfigRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.TaxRateConfigs.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("税率不存在");
        if (request.Name != null) entity.Rename(request.Name);
        if (request.Rate.HasValue) entity.SetRate(request.Rate.Value);
        if (request.EffectiveDate.HasValue) entity.SetEffectiveDate(request.EffectiveDate.Value);
        if (request.IsActive.HasValue) { if (request.IsActive.Value) entity.Activate(); else entity.Deactivate(); }
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.TaxRateConfigs.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("税率不存在");
        await _uow.TaxRateConfigs.DeleteAsync(entity, ct); await _uow.CommitAsync(ct);
    }

    private static TaxRateConfigDto MapToDto(TaxRateConfig t) => new()
    { Id = t.Id, Name = t.Name, Rate = t.Rate, EffectiveDate = t.EffectiveDate, IsActive = t.IsActive };
}
