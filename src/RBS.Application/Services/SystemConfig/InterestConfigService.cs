using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Application.DTOs.SystemConfig;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

public class InterestConfigService : IInterestConfigService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public InterestConfigService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }
    private Guid CompanyId => _tenant.DefaultCompanyId;

    public async Task<List<InterestConfigDto>> GetListAsync(CancellationToken ct = default)
    {
        var items = await _uow.InterestConfigs.GetAllAsync(ct);
        var cid = _tenant.EffectiveCompanyId;
        return items.Where(x => !cid.HasValue || x.CompanyId == cid.Value)
            .OrderByDescending(x => x.EffectiveDate)
            .Select(Map).ToList();
    }

    public async Task<InterestConfigDto> GetActiveAsync(CancellationToken ct = default)
    {
        var list = await _uow.InterestConfigs.GetAllAsync(ct);
        var active = list.FirstOrDefault(x => x.IsActive && x.CompanyId == CompanyId);
        if (active == null && list.Count > 0)
            active = list.OrderByDescending(x => x.EffectiveDate).FirstOrDefault(x => x.CompanyId == CompanyId);
        return active != null ? Map(active) : new InterestConfigDto
        {
            DailyRate = 0.0005m, GraceDays = 3, MaxRate = 100, MinAmount = 1,
            EffectiveDate = ChinaTime.Now, IsActive = true
        };
    }

    public async Task<InterestConfigDto> SaveAsync(SaveInterestConfigRequest request, CancellationToken ct = default)
    {
        var list = await _uow.InterestConfigs.GetAllAsync(ct);
        var existing = list.FirstOrDefault(x => x.IsActive && x.CompanyId == CompanyId);

        if (existing != null)
        {
            existing.Update(request.DailyRate, request.GraceDays, request.MaxRate, request.MinAmount, request.EffectiveDate);
            await _uow.CommitAsync(ct);
            return Map(existing);
        }

        var entity = new InterestConfig(request.DailyRate, request.GraceDays, CompanyId, request.EffectiveDate);
        entity.Update(request.DailyRate, request.GraceDays, request.MaxRate, request.MinAmount, request.EffectiveDate);
        await _uow.InterestConfigs.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.InterestConfigs.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("利息配置不存在");
        await _uow.InterestConfigs.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    private static InterestConfigDto Map(InterestConfig c) => new()
    {
        Id = c.Id, DailyRate = c.DailyRate, GraceDays = c.GraceDays,
        MaxRate = c.MaxRate, MinAmount = c.MinAmount,
        EffectiveDate = c.EffectiveDate, IsActive = c.IsActive
    };
}
