using Dapper;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.Import;
using RBS.Core.Entities.Organization;
using RBS.Core.Entities.Property;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Infrastructure.Data.Repositories;
using RBS.Infrastructure.Data.Repositories;

namespace RBS.Infrastructure.Data.UnitOfWork;

public class DapperUnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _db;
    public DapperUnitOfWork(IDbConnectionFactory db) => _db = db;

    public IUserRepository Users => _users ??= new DapperUserRepository(_db);
    public IRoleRepository Roles => _roles ??= new DapperRoleRepository(_db);
    public IMenuRepository Menus => _menus ??= new DapperMenuRepository(_db);
    public ICompanyRepository Companies => _companies ??= new DapperCompanyRepository(_db);
    public IApprovalRequestRepository ApprovalRequests => _approvalRequests ??= new DapperApprovalRequestRepository(_db);
    public IFeeCodeRepository FeeCodes => _feeCodes ??= new DapperFeeCodeRepository(_db);
    public IPaymentChannelRepository PaymentChannels => _paymentChannels ??= new DapperPaymentChannelRepository(_db);
    public IHolidayCalendarRepository HolidayCalendars => _holidayCalendars ??= new DapperHolidayCalendarRepository(_db);
    public IRepository<HousingUnit> HousingUnits => _housingUnits ??= new DapperRepository<HousingUnit>(_db);
    public IRepository<RoomType> RoomTypes => _roomTypes ??= new DapperRepository<RoomType>(_db);
    public IRepository<ApprovalType> ApprovalTypes => _approvalTypes ??= new DapperRepository<ApprovalType>(_db);
    public IRepository<ApprovalLevelConfig> ApprovalLevelConfigs => _approvalLevelConfigs ??= new DapperRepository<ApprovalLevelConfig>(_db);
    public IRepository<FloorLevelBand> FloorLevelBands => _floorLevelBands ??= new DapperRepository<FloorLevelBand>(_db);
    public IRepository<TaxRateConfig> TaxRateConfigs => _taxRateConfigs ??= new DapperRepository<TaxRateConfig>(_db);
    public IRepository<LateFeeConfig> LateFeeConfigs => _lateFeeConfigs ??= new DapperRepository<LateFeeConfig>(_db);
    public IRepository<AccountingSubject> AccountingSubjects => _accountingSubjects ??= new DapperRepository<AccountingSubject>(_db);
    public IRepository<JobSchedule> JobSchedules => _jobSchedules ??= new DapperRepository<JobSchedule>(_db);
    public IRepository<JobTemplate> JobTemplates => _jobTemplates ??= new DapperRepository<JobTemplate>(_db);
    public IRepository<JobScheduleExecution> JobScheduleExecutions => _jobScheduleExecutions ??= new DapperRepository<JobScheduleExecution>(_db);
    public IRepository<ImportBatch> ImportBatches => _importBatches ??= new DapperRepository<ImportBatch>(_db);
    public IRepository<ImportBatchItem> ImportBatchItems => _importBatchItems ??= new DapperRepository<ImportBatchItem>(_db);
    public IRepository<RoomPricingStandard> RoomPricingStandards => _roomPricingStandards ??= new DapperRepository<RoomPricingStandard>(_db);
    public ITenantRepository Tenants => throw new NotImplementedException();
    public IReceivablePlanRepository ReceivablePlans => throw new NotImplementedException();
    public IReceiptRepository Receipts => throw new NotImplementedException();
    public IMeterReadingRepository MeterReadings => throw new NotImplementedException();
    public IContractRepository Contracts => throw new NotImplementedException();

    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IMenuRepository? _menus;
    private ICompanyRepository? _companies;
    private IApprovalRequestRepository? _approvalRequests;
    private IFeeCodeRepository? _feeCodes;
    private IPaymentChannelRepository? _paymentChannels;
    private IHolidayCalendarRepository? _holidayCalendars;
    private IRepository<HousingUnit>? _housingUnits;
    private IRepository<RoomType>? _roomTypes;
    private IRepository<ApprovalType>? _approvalTypes;
    private IRepository<ApprovalLevelConfig>? _approvalLevelConfigs;
    private IRepository<FloorLevelBand>? _floorLevelBands;
    private IRepository<TaxRateConfig>? _taxRateConfigs;
    private IRepository<LateFeeConfig>? _lateFeeConfigs;
    private IRepository<AccountingSubject>? _accountingSubjects;
    private IRepository<JobSchedule>? _jobSchedules;
    private IRepository<JobTemplate>? _jobTemplates;
    private IRepository<JobScheduleExecution>? _jobScheduleExecutions;
    private IRepository<ImportBatch>? _importBatches;
    private IRepository<ImportBatchItem>? _importBatchItems;
    private IRepository<RoomPricingStandard>? _roomPricingStandards;

    public async Task<ApprovalType?> FindApprovalTypeByCodeAsync(string code, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<ApprovalType>("SELECT * FROM ApprovalTypes WHERE Code=@Code", new { Code = code });
    }
    public async Task<ImportBatch?> GetImportBatchWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            "SELECT * FROM ImportBatches WHERE Id=@Id; SELECT * FROM ImportBatchItems WHERE ImportBatchId=@Id ORDER BY RowIndex",
            new { Id = id });
        var batch = await multi.ReadSingleOrDefaultAsync<ImportBatch>();
        if (batch != null)
        {
            batch.Items = (await multi.ReadAsync<ImportBatchItem>()).ToList();
        }
        return batch;
    }
    public Task<int> CommitAsync(CancellationToken ct = default) => Task.FromResult(0);
    public Task ReloadAsync<T>(T entity, CancellationToken ct = default) where T : class => Task.CompletedTask;
    public Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CommitWithRetryAsync(int maxRetries = 3, CancellationToken ct = default) => Task.FromResult(0);
    public Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return conn.ExecuteAsync(sql, parameters.ToArray());
    }
    public void Dispose() { }
}
