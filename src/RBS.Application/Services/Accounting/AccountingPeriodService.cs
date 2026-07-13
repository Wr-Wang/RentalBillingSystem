using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Accounting;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Accounting;

public class AccountingPeriodService : IAccountingPeriodService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;

    public AccountingPeriodService(IUnitOfWork uow, ITenantService tenant)
    {
        _uow = uow;
        _tenant = tenant;
    }

    private Guid CompanyId => _tenant.EffectiveCompanyId ?? _tenant.CompanyId ?? Guid.Empty;

    public async Task<List<AccountingPeriod>> GetAllAsync(CancellationToken ct = default)
    {
        var all = await _uow.AccountingPeriods.GetAllAsync(ct);
        return all.Where(p => p.CompanyId == CompanyId).OrderByDescending(p => p.Period).ToList();
    }

    public async Task<AccountingPeriod> OpenPeriodAsync(string period, CancellationToken ct = default)
    {
        var existing = await _uow.AccountingPeriods.GetAllAsync(ct);
        if (existing.Any(p => p.CompanyId == CompanyId && p.Period == period))
            throw new InvalidOperationException($"会计期间 {period} 已存在");

        var entity = new AccountingPeriod(CompanyId, period, _tenant.CompanyId ?? Guid.Empty);
        await _uow.AccountingPeriods.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return entity;
    }

    public async Task ClosePeriodAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.AccountingPeriods.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("会计期间不存在");
        entity.Close(_tenant.CompanyId ?? Guid.Empty);
        await _uow.CommitAsync(ct);
    }

    public async Task ReopenPeriodAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.AccountingPeriods.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("会计期间不存在");
        entity.Reopen();
        await _uow.CommitAsync(ct);
    }

    public async Task LockPeriodAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.AccountingPeriods.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("会计期间不存在");
        entity.Lock();
        await _uow.CommitAsync(ct);
    }

    public async Task<bool> IsPeriodOpenAsync(string period, CancellationToken ct = default)
    {
        var all = await _uow.AccountingPeriods.GetAllAsync(ct);
        var match = all.FirstOrDefault(p => p.CompanyId == CompanyId && p.Period == period);
        return match != null && match.Status == "Open";
    }
}
