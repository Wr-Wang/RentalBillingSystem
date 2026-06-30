using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

public class ContractRepository : BaseRepository<Contract>, IContractRepository
{
    public ContractRepository(AppDbContext context) : base(context) { }

    public async Task<Contract?> GetByContractNoAsync(string contractNo, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.ContractTenants)
            .Include(c => c.FeeConfigs)
            .FirstOrDefaultAsync(c => c.ContractNo == contractNo, ct);
    }

    public async Task<List<Contract>> GetActiveContractsAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.CompanyId == companyId && c.StatusCode == "Active")
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
    }

    public async Task<List<Contract>> GetContractsExpiringAsync(DateOnly date, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.StatusCode == "Active" && c.EndDate <= date)
            .ToListAsync(ct);
    }

    public async Task<bool> HasActiveForHousingUnitAsync(Guid housingUnitId, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(c => c.RoomId == housingUnitId && c.StatusCode == "Active", ct);
    }
}
