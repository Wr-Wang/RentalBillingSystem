using Dapper;
using System.Data;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Banking;
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

namespace RBS.Infrastructure.Data.UnitOfWork;

public class DapperUnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private IDbConnection? _sharedConnection;
    private IDbTransaction? _sharedTransaction;

    public DapperUnitOfWork(IDbConnectionFactory db, ISqlLoader sql) { _db = db; _sql = sql; }

    public IUserRepository Users => _users ??= new DapperUserRepository(_db, _sql);
    public IRoleRepository Roles => _roles ??= new DapperRoleRepository(_db, _sql);
    public IMenuRepository Menus => _menus ??= new DapperMenuRepository(_db, _sql);
    public ICompanyRepository Companies => _companies ??= new DapperCompanyRepository(_db, _sql);
    public IApprovalRequestRepository ApprovalRequests => _approvalRequests ??= new DapperApprovalRequestRepository(_db, _sql);
    public IFeeCodeRepository FeeCodes => _feeCodes ??= new DapperFeeCodeRepository(_db, _sql);
    public IRepository<FeeCodeTemplate> FeeCodeTemplates => _feeCodeTemplates ??= new DapperRepository<FeeCodeTemplate>(_db);
    public IPaymentChannelRepository PaymentChannels => _paymentChannels ??= new DapperPaymentChannelRepository(_db, _sql);
    public IHolidayCalendarRepository HolidayCalendars => _holidayCalendars ??= new DapperHolidayCalendarRepository(_db, _sql);
    public IRepository<HousingUnit> HousingUnits => _housingUnits ??= new DapperRepository<HousingUnit>(_db);
    public IRepository<RoomType> RoomTypes => _roomTypes ??= new DapperRepository<RoomType>(_db);
    public IRepository<ApprovalType> ApprovalTypes => _approvalTypes ??= new DapperRepository<ApprovalType>(_db);
    public IRepository<ApprovalLevelConfig> ApprovalLevelConfigs => _approvalLevelConfigs ??= new DapperRepository<ApprovalLevelConfig>(_db);
    public IRepository<FloorLevelBand> FloorLevelBands => _floorLevelBands ??= new DapperRepository<FloorLevelBand>(_db);
    public IRepository<TaxRateConfig> TaxRateConfigs => _taxRateConfigs ??= new DapperRepository<TaxRateConfig>(_db);
    public IRepository<LateFeeConfig> LateFeeConfigs => _lateFeeConfigs ??= new DapperRepository<LateFeeConfig>(_db);
    public IRepository<AccountingSubject> AccountingSubjects => _accountingSubjects ??= new DapperRepository<AccountingSubject>(_db);
    public IRepository<Voucher> Vouchers => _vouchers ??= new DapperRepository<Voucher>(_db);
    public IRepository<JobSchedule> JobSchedules => _jobSchedules ??= new DapperRepository<JobSchedule>(_db);
    public IRepository<JobTemplate> JobTemplates => _jobTemplates ??= new DapperRepository<JobTemplate>(_db);
    public IRepository<JobScheduleExecution> JobScheduleExecutions => _jobScheduleExecutions ??= new DapperRepository<JobScheduleExecution>(_db);
    public IRepository<ImportBatch> ImportBatches => _importBatches ??= new DapperRepository<ImportBatch>(_db);
    public IRepository<ImportBatchItem> ImportBatchItems => _importBatchItems ??= new DapperRepository<ImportBatchItem>(_db);
    public IRepository<RoomPricingStandard> RoomPricingStandards => _roomPricingStandards ??= new DapperRepository<RoomPricingStandard>(_db);
    public ITenantRepository Tenants => _tenants ??= new DapperTenantRepository(_db, _sql);
    public IReceivablePlanRepository ReceivablePlans => _receivablePlans ??= new DapperReceivablePlanRepository(_db, _sql);
    public IReceiptRepository Receipts => _receipts ??= new DapperReceiptRepository(_db, _sql);
    public IRepository<BankStatement> BankStatements => _bankStatements ??= new DapperRepository<BankStatement>(_db);
    public IRepository<BankReconciliation> BankReconciliations => _bankReconciliations ??= new DapperRepository<BankReconciliation>(_db);
    public IRepository<BankMatch> BankMatches => _bankMatches ??= new DapperRepository<BankMatch>(_db);
    public IRepository<DepositLog> DepositLogs => _depositLogs ??= new DapperRepository<DepositLog>(_db);
    public IRepository<CollectionStage> CollectionStages => _collectionStages ??= new DapperRepository<CollectionStage>(_db);
    public IRepository<CollectionRecord> CollectionRecords => _collectionRecords ??= new DapperRepository<CollectionRecord>(_db);
    public IRepository<DebitNote> DebitNotes => _debitNotes ??= new DapperRepository<DebitNote>(_db);
    public IMeterReadingRepository MeterReadings => _meterReadings ??= new DapperMeterReadingRepository(_db, _sql);
    public IContractRepository Contracts => _contracts ??= new DapperContractRepository(_db, _sql);
    public IRenewalRequestRepository RenewalRequests => _renewalRequests ??= new DapperRenewalRequestRepository(_db, _sql);
    public IRepository<ChangeRequest> ChangeRequests => _changeRequests ??= new DapperRepository<ChangeRequest>(_db);

    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IMenuRepository? _menus;
    private ICompanyRepository? _companies;
    private IApprovalRequestRepository? _approvalRequests;
    private IFeeCodeRepository? _feeCodes;
    private IRepository<FeeCodeTemplate>? _feeCodeTemplates;
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
    private IRepository<Voucher>? _vouchers;
    private IRepository<JobSchedule>? _jobSchedules;
    private IRepository<JobTemplate>? _jobTemplates;
    private IRepository<JobScheduleExecution>? _jobScheduleExecutions;
    private IRepository<ImportBatch>? _importBatches;
    private IRepository<ImportBatchItem>? _importBatchItems;
    private IRepository<RoomPricingStandard>? _roomPricingStandards;
    private ITenantRepository? _tenants;
    private IReceivablePlanRepository? _receivablePlans;
    private IReceiptRepository? _receipts;
    private IRepository<BankStatement>? _bankStatements;
    private IRepository<BankReconciliation>? _bankReconciliations;
    private IRepository<BankMatch>? _bankMatches;
    private IRepository<DepositLog>? _depositLogs;
    private IRepository<CollectionStage>? _collectionStages;
    private IRepository<CollectionRecord>? _collectionRecords;
    private IRepository<DebitNote>? _debitNotes;
    private IMeterReadingRepository? _meterReadings;
    private IContractRepository? _contracts;
    private IRenewalRequestRepository? _renewalRequests;
    private IRepository<ChangeRequest>? _changeRequests;

    public async Task<ApprovalType?> FindApprovalTypeByCodeAsync(string code, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<ApprovalType>(_sql.Get("Common.Select.ApprovalType.ByCode"), new { Code = code });
    }
    public async Task<ImportBatch?> GetImportBatchWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("Common.Select.ImportBatch.ByIdWithItems"),
            new { Id = id });
        var batch = await multi.ReadSingleOrDefaultAsync<ImportBatch>();
        if (batch != null)
        {
            batch.LoadItems((await multi.ReadAsync<ImportBatchItem>()).ToList());
        }
        return batch;
    }
    public Task<int> CommitAsync(CancellationToken ct = default) => Task.FromResult(0);
    public Task ReloadAsync<T>(T entity, CancellationToken ct = default) where T : class => Task.CompletedTask;

    public async Task<List<DebitNote>> GetDebitNotesByCompanyAsync(Guid companyId, string? period = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var sqlKey = string.IsNullOrEmpty(period)
            ? "Billing.Select.DebitNote.ByCompany"
            : "Billing.Select.DebitNote.ByPeriodCompany";
        var param = new { CompanyId = companyId, Period = period ?? "" };
        return (await conn.QueryAsync<DebitNote>(_sql.Get(sqlKey), param)).ToList();
    }

    public async Task<List<DebitNote>> GetDebitNotesByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<DebitNote>(_sql.Get("Billing.Select.DebitNote.ByTenantId"), new { TenantId = tenantId })).ToList();
    }

    public async Task<List<DebitNoteItem>> GetDebitNoteItemsAsync(Guid debitNoteId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync<DebitNoteItem>(
            "SELECT * FROM DebitNoteItems WHERE DebitNoteId=@Id ORDER BY CreatedAt",
            new { Id = debitNoteId })).ToList();
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        _sharedConnection = _db.CreateConnection();
        _sharedConnection.Open();
        _sharedTransaction = _sharedConnection.BeginTransaction();
        return new DapperTransaction(_sharedTransaction);
    }

    public Task<int> CommitWithRetryAsync(int maxRetries = 3, CancellationToken ct = default) => Task.FromResult(0);

    public async Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken ct = default)
    {
        var args = parameters.ToArray();
        var dp = new DynamicParameters();
        for (int i = 0; i < args.Length; i++)
            dp.Add($"p{i}", args[i]);

        if (_sharedConnection != null && _sharedConnection.State == ConnectionState.Open)
        {
            // 有活跃事务时复用连接和事务
            return await _sharedConnection.ExecuteAsync(sql, dp, _sharedTransaction);
        }

        using var conn = _db.CreateConnection();
        conn.Open();
        return await conn.ExecuteAsync(sql, dp);
    }

    public void Dispose()
    {
        _sharedTransaction?.Dispose();
        _sharedConnection?.Dispose();
        _sharedTransaction = null;
        _sharedConnection = null;
    }
}
