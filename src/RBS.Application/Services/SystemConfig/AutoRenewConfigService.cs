using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.SystemConfig;

/// <summary>
/// 自动续签策略配置服务
/// </summary>
public class AutoRenewConfigService : IAutoRenewConfigService
{
    private readonly IUnitOfWork _uow;

    public AutoRenewConfigService(IUnitOfWork uow) => _uow = uow;

    public async Task<AutoRenewConfig?> GetByCompanyAsync(Guid companyId, CancellationToken ct = default)
    {
        var all = await _uow.AutoRenewConfigs.GetAllAsync(ct);
        return all.FirstOrDefault(c => c.CompanyId == companyId);
    }

    public async Task<AutoRenewConfig> SaveAsync(AutoRenewConfig config, CancellationToken ct = default)
    {
        var existing = await GetByCompanyAsync(config.CompanyId, ct);
        if (existing != null)
        {
            existing.Update(config.RentRule, config.RentIncreasePercent,
                config.TermRule, config.TermMonths,
                config.AdvanceDays, config.OverdueAction);
            await _uow.AutoRenewConfigs.UpdateAsync(existing, ct);
            await _uow.CommitAsync(ct);
            return existing;
        }
        else
        {
            var entity = new AutoRenewConfig(config.CompanyId);
            entity.Update(config.RentRule, config.RentIncreasePercent,
                config.TermRule, config.TermMonths,
                config.AdvanceDays, config.OverdueAction);
            await _uow.AutoRenewConfigs.AddAsync(entity, ct);
            await _uow.CommitAsync(ct);
            return entity;
        }
    }
}
