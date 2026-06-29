using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

public class LateFeeConfigService : ILateFeeConfigService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public LateFeeConfigService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }
    private Guid CompanyId => _tenant.DefaultCompanyId;

    public async Task<List<LateFeeConfigDto>> GetListAsync(CancellationToken ct = default)
        => (await _uow.LateFeeConfigs.GetAllAsync(ct))
            .OrderByDescending(x => x.EffectiveDate)
            .Select(Map).ToList();

    public async Task<LateFeeConfigDto> GetActiveAsync(CancellationToken ct = default)
    {
        var list = await _uow.LateFeeConfigs.GetAllAsync(ct);
        var active = list.FirstOrDefault(x => x.IsActive && x.CompanyId == CompanyId);
        if (active == null && list.Count > 0)
            active = list.OrderByDescending(x => x.EffectiveDate).First(x => x.CompanyId == CompanyId);
        return active != null ? Map(active) : new LateFeeConfigDto
        {
            DailyRate = 0.0005m, GraceDays = 3, MaxRate = 100, MinAmount = 1,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today), IsActive = true
        };
    }

    public async Task<LateFeeConfigDto> SaveAsync(SaveLateFeeConfigRequest request, CancellationToken ct = default)
    {
        var list = await _uow.LateFeeConfigs.GetAllAsync(ct);
        var existing = list.FirstOrDefault(x => x.IsActive && x.CompanyId == CompanyId);

        if (existing != null)
        {
            existing.Update(request.DailyRate, request.GraceDays, request.MaxRate, request.MinAmount, request.EffectiveDate);
            await _uow.CommitAsync(ct);
            return Map(existing);
        }

        var entity = new LateFeeConfig(request.DailyRate, request.GraceDays, CompanyId, request.EffectiveDate);
        entity.Update(request.DailyRate, request.GraceDays, request.MaxRate, request.MinAmount, request.EffectiveDate);
        await _uow.LateFeeConfigs.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.LateFeeConfigs.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("滞纳金配置不存在");
        await _uow.LateFeeConfigs.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    private static LateFeeConfigDto Map(LateFeeConfig c) => new()
    {
        Id = c.Id, DailyRate = c.DailyRate, GraceDays = c.GraceDays,
        MaxRate = c.MaxRate, MinAmount = c.MinAmount,
        EffectiveDate = c.EffectiveDate, IsActive = c.IsActive
    };
}
