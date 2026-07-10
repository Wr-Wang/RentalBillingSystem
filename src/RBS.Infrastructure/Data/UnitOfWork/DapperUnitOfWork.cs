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
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Infrastructure.Data.Repositories;

namespace RBS.Infrastructure.Data.UnitOfWork;

public class DapperUnitOfWork : IUnitOfWork, IChangeTracker
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly IAuditLogWriter _auditWriter;
    private readonly ITenantService? _tenant;
    private IDbConnection? _sharedConnection;
    private IDbTransaction? _sharedTransaction;

    // ===== 变更追踪 =====
    private readonly Dictionary<string, Dictionary<Guid, TrackedEntry>> _tracked = new();

    public DapperUnitOfWork(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, ITenantService? tenant = null) { _db = db; _sql = sql; _auditWriter = auditWriter;
        _tenant = tenant; }

    public IUserRepository Users => _users ??= new DapperUserRepository(_db, _sql, _auditWriter, this);
    public IRoleRepository Roles => _roles ??= new DapperRoleRepository(_db, _sql, _auditWriter, this);
    public IMenuRepository Menus => _menus ??= new DapperMenuRepository(_db, _sql, _auditWriter, this);
    public ICompanyRepository Companies => _companies ??= new DapperCompanyRepository(_db, _sql, _auditWriter, this);
    public IApprovalRequestRepository ApprovalRequests => _approvalRequests ??= new DapperApprovalRequestRepository(_db, _sql, _auditWriter, this);
    public IFeeCodeRepository FeeCodes => _feeCodes ??= new DapperFeeCodeRepository(_db, _sql, _auditWriter, this, _tenant);
    public IRepository<FeeCodeTemplate> FeeCodeTemplates => _feeCodeTemplates ??= new DapperRepository<FeeCodeTemplate>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IPaymentChannelRepository PaymentChannels => _paymentChannels ??= new DapperPaymentChannelRepository(_db, _sql, _auditWriter, this, _tenant);
    public IHolidayCalendarRepository HolidayCalendars => _holidayCalendars ??= new DapperHolidayCalendarRepository(_db, _sql, _auditWriter, this, _tenant);
    public IRepository<HousingUnit> HousingUnits => _housingUnits ??= new DapperRepository<HousingUnit>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<RoomType> RoomTypes => _roomTypes ??= new DapperRepository<RoomType>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ApprovalType> ApprovalTypes => _approvalTypes ??= new DapperRepository<ApprovalType>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ApprovalLevelConfig> ApprovalLevelConfigs => _approvalLevelConfigs ??= new DapperRepository<ApprovalLevelConfig>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<FloorLevelBand> FloorLevelBands => _floorLevelBands ??= new DapperRepository<FloorLevelBand>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<TaxRateConfig> TaxRateConfigs => _taxRateConfigs ??= new DapperRepository<TaxRateConfig>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<LateFeeConfig> LateFeeConfigs => _lateFeeConfigs ??= new DapperRepository<LateFeeConfig>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<AutoRenewConfig> AutoRenewConfigs => _autoRenewConfigs ??= new DapperRepository<AutoRenewConfig>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<AccountingSubject> AccountingSubjects => _accountingSubjects ??= new DapperRepository<AccountingSubject>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<Voucher> Vouchers => _vouchers ??= new DapperRepository<Voucher>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<JobSchedule> JobSchedules => _jobSchedules ??= new DapperRepository<JobSchedule>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<JobTemplate> JobTemplates => _jobTemplates ??= new DapperRepository<JobTemplate>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<JobScheduleExecution> JobScheduleExecutions => _jobScheduleExecutions ??= new DapperRepository<JobScheduleExecution>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ImportBatch> ImportBatches => _importBatches ??= new DapperRepository<ImportBatch>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ImportBatchItem> ImportBatchItems => _importBatchItems ??= new DapperRepository<ImportBatchItem>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<RoomPricingStandard> RoomPricingStandards => _roomPricingStandards ??= new DapperRepository<RoomPricingStandard>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public ITenantRepository Tenants => _tenants ??= new DapperTenantRepository(_db, _sql, _auditWriter, this, _tenant);
    public IReceivablePlanRepository ReceivablePlans => _receivablePlans ??= new DapperReceivablePlanRepository(_db, _sql, _auditWriter, this, _tenant);
    public IReceiptRepository Receipts => _receipts ??= new DapperReceiptRepository(_db, _sql, _auditWriter, this, _tenant);
    public IRepository<BankStatement> BankStatements => _bankStatements ??= new DapperRepository<BankStatement>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<BankReconciliation> BankReconciliations => _bankReconciliations ??= new DapperRepository<BankReconciliation>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<BankMatch> BankMatches => _bankMatches ??= new DapperRepository<BankMatch>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<DepositLog> DepositLogs => _depositLogs ??= new DapperRepository<DepositLog>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<CollectionStage> CollectionStages => _collectionStages ??= new DapperRepository<CollectionStage>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<CollectionRecord> CollectionRecords => _collectionRecords ??= new DapperRepository<CollectionRecord>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<DebitNote> DebitNotes => _debitNotes ??= new DapperRepository<DebitNote>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IMeterReadingRepository MeterReadings => _meterReadings ??= new DapperMeterReadingRepository(_db, _sql, _auditWriter, this, _tenant);
    public IContractRepository Contracts => _contracts ??= new DapperContractRepository(_db, _sql, _auditWriter, this, _tenant);
    public IRenewalRequestRepository RenewalRequests => _renewalRequests ??= new DapperRenewalRequestRepository(_db, _sql, _auditWriter, this, _tenant);
    public IApprovalBizDataRepository ApprovalBizData => _approvalBizData ??= new DapperApprovalBizDataRepository(_db, _sql, _auditWriter, this, _tenant);
    public IApprovalFeeItemRepository ApprovalFeeItems => _approvalFeeItems ??= new DapperApprovalFeeItemRepository(_db, _sql, _auditWriter, this, _tenant);

    // ===== 审批闭环暂存表 =====
    public IRepository<ContractCreateRequest> ContractCreateRequests => _contractCreateRequests ??= new DapperRepository<ContractCreateRequest>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ContractCreateRequestTenant> ContractCreateRequestTenants => _contractCreateRequestTenants ??= new DapperRepository<ContractCreateRequestTenant>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ContractCreateRequestFee> ContractCreateRequestFees => _contractCreateRequestFees ??= new DapperRepository<ContractCreateRequestFee>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ContractModifyRequest> ContractModifyRequests => _contractModifyRequests ??= new DapperRepository<ContractModifyRequest>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<TenantCreateRequest> TenantCreateRequests => _tenantCreateRequests ??= new DapperRepository<TenantCreateRequest>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<SupplementaryFeeRequest> SupplementaryFeeRequests => _supplementaryFeeRequests ??= new DapperRepository<SupplementaryFeeRequest>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<SupplementaryFeeRequestItem> SupplementaryFeeRequestItems => _supplementaryFeeRequestItems ??= new DapperRepository<SupplementaryFeeRequestItem>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ReceivableGenerateRequest> ReceivableGenerateRequests => _receivableGenerateRequests ??= new DapperRepository<ReceivableGenerateRequest>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ReceivableGenerateRequestItem> ReceivableGenerateRequestItems => _receivableGenerateRequestItems ??= new DapperRepository<ReceivableGenerateRequestItem>(_db, _auditWriter, tracker: this, tenant: _tenant);

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
    private IRepository<AutoRenewConfig>? _autoRenewConfigs;
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
    private IApprovalBizDataRepository? _approvalBizData;
    private IApprovalFeeItemRepository? _approvalFeeItems;

    // ===== 审批闭环暂存表 fields =====
    private IRepository<ContractCreateRequest>? _contractCreateRequests;
    private IRepository<ContractCreateRequestTenant>? _contractCreateRequestTenants;
    private IRepository<ContractCreateRequestFee>? _contractCreateRequestFees;
    private IRepository<ContractModifyRequest>? _contractModifyRequests;
    private IRepository<TenantCreateRequest>? _tenantCreateRequests;
    private IRepository<SupplementaryFeeRequest>? _supplementaryFeeRequests;
    private IRepository<SupplementaryFeeRequestItem>? _supplementaryFeeRequestItems;
    private IRepository<ReceivableGenerateRequest>? _receivableGenerateRequests;
    private IRepository<ReceivableGenerateRequestItem>? _receivableGenerateRequestItems;

    // ==================================================================
    // IChangeTracker 实现
    // ==================================================================

    void IChangeTracker.Track<T>(T entity, string tableName)
    {
        var id = typeof(T).GetProperty("Id")?.GetValue(entity) is Guid g ? g : Guid.Empty;
        if (id == Guid.Empty) return;
        if (!_tracked.ContainsKey(tableName))
            _tracked[tableName] = new Dictionary<Guid, TrackedEntry>();
        _tracked[tableName][id] = new TrackedEntry(entity, EntityToDict(entity));
    }

    IReadOnlyDictionary<string, Dictionary<Guid, TrackedEntry>> IChangeTracker.DirtyEntries
        => _tracked;

    void IChangeTracker.Clear() => _tracked.Clear();

    // ==================================================================
    // CommitAsync — 真正的变更持久化入口
    // ==================================================================

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        if (_tracked.Count == 0) return 0;

        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            int affected = 0;
            var auditBatch = new List<(string tableName, string entityId, string action, Dictionary<string, object?> changes, Guid userId)>();

            foreach (var (tableName, entities) in _tracked)
            {
                foreach (var (id, entry) in entities)
                {
                    var now = EntityToDict(entry.Entity);
                    var changes = DiffDict(entry.Snapshot, now);
                    if (changes.Count == 0) continue;

                    // 生成 UPDATE — 只 SET 变化的字段
                    var sets = string.Join(",", changes.Keys.Select(k => $"[{k}]=@{k}"));
                    var sql = $"UPDATE [{tableName}] SET {sets} WHERE Id=@Id";
                    var parms = new DynamicParameters();
                    parms.Add("@Id", id);
                    foreach (var k in changes.Keys)
                    {
                        var prop = entry.Entity.GetType().GetProperty(k);
                        parms.Add($"@{k}", prop?.GetValue(entry.Entity));
                    }

                    affected += await conn.ExecuteAsync(sql, parms, tx);

                    // 收集审计（延迟写入避免与事务表竞争）
                    var updatedBy = entry.Entity.GetType().GetProperty("UpdatedBy")?.GetValue(entry.Entity) as Guid? ?? Guid.Empty;
                    auditBatch.Add((tableName, id.ToString(), "Update", changes, updatedBy));
                }
            }

            tx.Commit();

            // 审计日志在事务外写入（审计表不应影响业务事务）
            foreach (var (tn, eid, action, chg, uid) in auditBatch)
            {
                await _auditWriter.LogChangesAsync(tn, eid, action, chg, uid, ct);
            }

            _tracked.Clear();
            return affected;
        }
        catch
        {
            tx.Rollback();
            _tracked.Clear();
            throw;
        }
    }

    public Task ReloadAsync<T>(T entity, CancellationToken ct = default) where T : class => Task.CompletedTask;

    // ==================================================================
    // 其他方法（原样保留）
    // ==================================================================

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
            _sql.Get("Billing.Select.DebitNoteItem.ByDebitNoteId"),
            new { Id = debitNoteId })).ToList();
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        _sharedConnection = _db.CreateConnection();
        _sharedConnection.Open();
        _sharedTransaction = _sharedConnection.BeginTransaction();
        return new DapperTransaction(_sharedTransaction, () => {
            _sharedTransaction = null;
            _sharedConnection?.Dispose();
            _sharedConnection = null;
        });
    }

    public Task<int> CommitWithRetryAsync(int maxRetries = 3, CancellationToken ct = default) => Task.FromResult(0);

    public async Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken ct = default)
    {
        var args = parameters.ToArray();
        var dp = new DynamicParameters();
        for (int i = 0; i < args.Length; i++)
            dp.Add($"p{i}", args[i]);

        if (_sharedConnection != null && _sharedConnection.State == ConnectionState.Open && _sharedTransaction != null)
        {
            return await _sharedConnection.ExecuteAsync(sql, dp, _sharedTransaction);
        }

        using var conn = _db.CreateConnection();
        conn.Open();
        return await conn.ExecuteAsync(sql, dp);
    }

    public async Task<int> ExecuteSqlRawAsync(string sql, object parameters, CancellationToken ct = default)
    {
        var dp = new DynamicParameters(parameters);

        if (_sharedConnection != null && _sharedConnection.State == ConnectionState.Open && _sharedTransaction != null)
        {
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

    // ==================================================================
    // 快照/比对工具（与 DapperRepository 共享逻辑）
    // ==================================================================

    internal static Dictionary<string, object?> EntityToDict(object entity)
    {
        var dict = new Dictionary<string, object?>();
        var props = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var p in props)
        {
            if (p.Name is "DomainEvents" or "RowVersion") continue;
            if (!p.CanWrite) continue; // 排除计算属性（如 IsVacant/IsRented）
            if (IsNavProp(p)) continue;
            dict[p.Name] = p.GetValue(entity);
        }
        return dict;
    }

    internal static Dictionary<string, object?> DiffDict(Dictionary<string, object?> old, Dictionary<string, object?> now)
    {
        var diff = new Dictionary<string, object?>();
        var exclude = new HashSet<string> { "RowVersion", "UpdatedAt", "UpdatedBy", "UpdatedIp", "UpdatedHostname" };
        foreach (var kv in now)
        {
            if (exclude.Contains(kv.Key)) continue;
            if (!old.ContainsKey(kv.Key)) { diff[kv.Key] = kv.Value; continue; }
            var oldVal = old[kv.Key];
            var newVal = kv.Value;
            if (!Equals(oldVal, newVal))
                diff[kv.Key] = newVal;
        }
        return diff;
    }

    private static bool IsNavProp(System.Reflection.PropertyInfo p)
    {
        var t = p.PropertyType;
        // 可空值类型 Nullable<T> 不是导航属性，应参与 INSERT/UPDATE
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            return false;
        return t == typeof(System.Collections.IList) || t.IsGenericType ||
               p.Name is "DomainEvents" or "RowVersion" or "Records" or "Roles";
    }
}
