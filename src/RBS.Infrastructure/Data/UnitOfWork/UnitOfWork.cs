using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Property;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Property;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Infrastructure.Data.Repositories;

namespace RBS.Infrastructure.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IMenuRepository? _menus;
    private ICompanyRepository? _companies;
    private IBuildingRepository? _buildings;
    private IRoomRepository? _rooms;
    private IContractRepository? _contracts;
    private ITenantRepository? _tenants;
    private IFeeCodeRepository? _feeCodes;
    private IReceivablePlanRepository? _receivablePlans;
    private IReceiptRepository? _receipts;
    private IPaymentChannelRepository? _paymentChannels;
    private IMeterReadingRepository? _meterReadings;
    private IApprovalRequestRepository? _approvalRequests;
    private IHolidayCalendarRepository? _holidayCalendars;
    private IRepository<RoomType>? _roomTypes;
    private IRepository<ApprovalType>? _approvalTypes;
    private IRepository<ApprovalLevelConfig>? _approvalLevelConfigs;
    private IRepository<FloorLevelBand>? _floorLevelBands;
    private IRepository<RoomPricingStandard>? _roomPricingStandards;
    private IRepository<TaxRateConfig>? _taxRateConfigs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public IMenuRepository Menus => _menus ??= new MenuRepository(_context);
    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);
    public IBuildingRepository Buildings => _buildings ??= new BuildingRepository(_context);
    public IRoomRepository Rooms => _rooms ??= new RoomRepository(_context);
    public IContractRepository Contracts => _contracts ??= new ContractRepository(_context);
    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);
    public IFeeCodeRepository FeeCodes => _feeCodes ??= new FeeCodeRepository(_context);
    public IReceivablePlanRepository ReceivablePlans => _receivablePlans ??= new ReceivablePlanRepository(_context);
    public IReceiptRepository Receipts => _receipts ??= new ReceiptRepository(_context);
    public IPaymentChannelRepository PaymentChannels => _paymentChannels ??= new PaymentChannelRepository(_context);
    public IMeterReadingRepository MeterReadings => _meterReadings ??= new MeterReadingRepository(_context);
    public IApprovalRequestRepository ApprovalRequests => _approvalRequests ??= new ApprovalRequestRepository(_context);
    public IHolidayCalendarRepository HolidayCalendars => _holidayCalendars ??= new HolidayCalendarRepository(_context);
    public IRepository<RoomType> RoomTypes => _roomTypes ??= new BaseRepository<RoomType>(_context);
    public IRepository<ApprovalType> ApprovalTypes => _approvalTypes ??= new BaseRepository<ApprovalType>(_context);
    public IRepository<ApprovalLevelConfig> ApprovalLevelConfigs => _approvalLevelConfigs ??= new BaseRepository<ApprovalLevelConfig>(_context);
    public IRepository<FloorLevelBand> FloorLevelBands => _floorLevelBands ??= new BaseRepository<FloorLevelBand>(_context);
    public IRepository<RoomPricingStandard> RoomPricingStandards => _roomPricingStandards ??= new BaseRepository<RoomPricingStandard>(_context);
    public IRepository<TaxRateConfig> TaxRateConfigs => _taxRateConfigs ??= new BaseRepository<TaxRateConfig>(_context);

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var efTransaction = await _context.Database.BeginTransactionAsync(ct);
        return new EfTransaction(efTransaction);
    }

    public async Task<int> CommitWithRetryAsync(int maxRetries = 3, CancellationToken ct = default)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (i == maxRetries - 1) throw;
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync(ct);
                }
            }
        }
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
