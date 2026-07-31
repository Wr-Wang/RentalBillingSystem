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
using Microsoft.Extensions.DependencyInjection;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Infrastructure.Data.Repositories;
using RBS.Infrastructure.Data.Services;

namespace RBS.Infrastructure.Data.UnitOfWork;

/// <summary>
/// Dapper 工作单元实现 — 同时实现 IUnitOfWork（公开仓储访问）和 IChangeTracker（内部变更追踪）
/// </summary>
/// <remarks>
/// 架构说明：
/// <list type="bullet">
///   <item><description>聚合所有 Dapper 仓储实例，通过延迟初始化按需创建</description></item>
///   <item><description>实现 IChangeTracker 接口，支持快照追踪→差异计算→批量 UPDATE 的变更持久化模式</description></item>
///   <item><description>CommitAsync 遍历所有被追踪的实体，只写入有变化的字段（差异更新）</description></item>
///   <item><description>审计日志在事务提交后、事务外写入，避免审计表影响业务事务</description></item>
///   <item><description>BeginTransactionAsync 创建共享连接+事务，供 ExecuteSqlRawAsync 复用</description></item>
/// </list>
/// 设计模式：Unit of Work + Change Tracker（类似 EF Core 的 ChangeTracker 机制但更轻量）。
/// </remarks>
public class DapperUnitOfWork : IUnitOfWork, IChangeTracker
{
    /// <summary>数据库连接工厂</summary>
    private readonly IDbConnectionFactory _db;
    /// <summary>SQL 映射加载器</summary>
    private readonly ISqlLoader _sql;
    /// <summary>审计日志写入器</summary>
    private readonly IAuditLogWriter _auditWriter;
    /// <summary>多租户服务（可选）</summary>
    private readonly ITenantService? _tenant;
    /// <summary>服务提供者（可选，用于延迟解析领域事件调度器以避免循环依赖）</summary>
    private readonly IServiceProvider? _serviceProvider;
    /// <summary>审计装饰器（统一审计逻辑）</summary>
    private readonly RepositoryAuditService _auditService;
    /// <summary>共享连接（用于事务）</summary>
    private IDbConnection? _sharedConnection;
    /// <summary>共享事务（由 BeginTransactionAsync 创建）</summary>
    private IDbTransaction? _sharedTransaction;

    // ===== 变更追踪 =====
    /// <summary>变更追踪字典：表名 → (实体 ID → 快照条目)</summary>
    private readonly Dictionary<string, Dictionary<Guid, TrackedEntry>> _tracked = new();

    /// <summary>
    /// 初始化工作单元
    /// </summary>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="sql">SQL 映射加载器</param>
    /// <param name="auditWriter">审计日志写入器</param>
    /// <param name="tenant">多租户服务（可选）</param>
    public DapperUnitOfWork(IDbConnectionFactory db, ISqlLoader sql, IAuditLogWriter auditWriter, ITenantService? tenant = null, IServiceProvider? serviceProvider = null, RepositoryAuditService? auditService = null) { _db = db; _sql = sql; _auditWriter = auditWriter;
        _tenant = tenant; _serviceProvider = serviceProvider; _auditService = auditService ?? new RepositoryAuditService(auditWriter); }

    public IUserRepository Users => _users ??= new DapperUserRepository(_db, _sql, _auditWriter, this, _auditService);
    public IRoleRepository Roles => _roles ??= new DapperRoleRepository(_db, _sql, _auditWriter, this, _auditService);
    public IMenuRepository Menus => _menus ??= new DapperMenuRepository(_db, _sql, _auditWriter, this, _auditService);
    public ICompanyRepository Companies => _companies ??= new DapperCompanyRepository(_db, _sql, _auditWriter, this, _auditService);
    public IApprovalRequestRepository ApprovalRequests => _approvalRequests ??= new DapperApprovalRequestRepository(_db, _sql, _auditWriter, this, _auditService);
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
    public IRepository<InterestConfig> InterestConfigs => _interestConfigs ??= new DapperRepository<InterestConfig>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<AutoRenewConfig> AutoRenewConfigs => _autoRenewConfigs ??= new DapperRepository<AutoRenewConfig>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<AccountingSubject> AccountingSubjects => _accountingSubjects ??= new DapperRepository<AccountingSubject>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<JobSchedule> JobSchedules => _jobSchedules ??= new DapperRepository<JobSchedule>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<JobTemplate> JobTemplates => _jobTemplates ??= new DapperRepository<JobTemplate>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<JobScheduleExecution> JobScheduleExecutions => _jobScheduleExecutions ??= new DapperRepository<JobScheduleExecution>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ImportBatch> ImportBatches => _importBatches ??= new DapperRepository<ImportBatch>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<ImportBatchItem> ImportBatchItems => _importBatchItems ??= new DapperRepository<ImportBatchItem>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public IRepository<RoomPricingStandard> RoomPricingStandards => _roomPricingStandards ??= new DapperRepository<RoomPricingStandard>(_db, _auditWriter, tracker: this, tenant: _tenant);
    public ITenantRepository Tenants => _tenants ??= new DapperTenantRepository(_db, _sql, _auditWriter, this, _tenant);
    public IRepository<Journal> Journals => _journals ??= new DapperRepository<Journal>(_db, _auditWriter, tracker: this, tenant: _tenant);
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
    private IRepository<InterestConfig>? _interestConfigs;
    private IRepository<AutoRenewConfig>? _autoRenewConfigs;
    private IRepository<AccountingSubject>? _accountingSubjects;
    private IRepository<JobSchedule>? _jobSchedules;
    private IRepository<JobTemplate>? _jobTemplates;
    private IRepository<JobScheduleExecution>? _jobScheduleExecutions;
    private IRepository<ImportBatch>? _importBatches;
    private IRepository<ImportBatchItem>? _importBatchItems;
    private IRepository<RoomPricingStandard>? _roomPricingStandards;
    private ITenantRepository? _tenants;
    private IRepository<Journal>? _journals;
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

    /// <summary>
    /// 追踪实体的当前快照，用于后续差异比较
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="entity">实体对象</param>
    /// <param name="tableName">数据库表名</param>
    void IChangeTracker.Track<T>(T entity, string tableName)
    {
        var id = typeof(T).GetProperty("Id")?.GetValue(entity) is Guid g ? g : Guid.Empty;
        if (id == Guid.Empty) return;
        if (!_tracked.ContainsKey(tableName))
            _tracked[tableName] = new Dictionary<Guid, TrackedEntry>();
        _tracked[tableName][id] = new TrackedEntry(entity, EntityToDict(entity));
    }

    /// <summary>获取所有被追踪的脏数据条目（只读）</summary>
    IReadOnlyDictionary<string, Dictionary<Guid, TrackedEntry>> IChangeTracker.DirtyEntries
        => _tracked;

    /// <summary>清空变更追踪缓存</summary>
    void IChangeTracker.Clear() => _tracked.Clear();

    // ==================================================================
    // CommitAsync — 真正的变更持久化入口
    // ==================================================================

    /// <summary>
    /// 提交所有变更 — 遍历被追踪的脏数据，生成差异 UPDATE 并持久化
    /// </summary>
    /// <remarks>
    /// 提交策略：
    /// <list type="bullet">
    ///   <item><description>仅对有变化的字段生成 UPDATE SET 子句（差异更新）</description></item>
    ///   <item><description>所有变更在同一个事务中提交</description></item>
    ///   <item><description>审计日志在事务外写入，避免审计失败导致业务回滚</description></item>
    ///   <item><description>CommitAsync 是唯一支持 UPDATE 审计的入口；直接调用仓储的 UpdateAsync 也有独立的审计逻辑</description></item>
    /// </list>
    /// </remarks>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响的行数</returns>
    /// <exception cref="Exception">事务提交失败时回滚并重新抛出异常</exception>
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
                    // 在提交前填充 UpdatedBy/At/Ip/Hostname，确保业务表和审计表都记录操作人
                    _auditService.PopulateUpdatedFields(entry.Entity);

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

                    var rowCount = await conn.ExecuteAsync(sql, parms, tx);
                    affected += rowCount;

                    // 收集审计：记录全量快照（延迟写入避免与事务表竞争）
                    var updatedBy = entry.Entity.GetType().GetProperty("UpdatedBy")?.GetValue(entry.Entity) as Guid? ?? Guid.Empty;
                    if (updatedBy == Guid.Empty)
                    {
                        // 业务层未设 UpdatedBy 时兜底取 CreatedBy
                        updatedBy = entry.Entity.GetType().GetProperty("CreatedBy")?.GetValue(entry.Entity) as Guid? ?? Guid.Empty;
                    }
                    // 从请求上下文填充 UpdatedIp/UpdatedHostname（通过审计装饰器）
                    var (ip, hn) = _auditService.GetClientInfo();
                    if (ip != null) now["UpdatedIp"] = ip;
                    if (hn != null) now["UpdatedHostname"] = hn;
                    auditBatch.Add((tableName, id.ToString(), "Update", now, updatedBy));
                }
            }

            tx.Commit();

            // 审计日志在事务外写入（审计表不应影响业务事务）
            foreach (var (tn, eid, action, chg, uid) in auditBatch)
            {
                await _auditWriter.LogChangesAsync(tn, eid, action, chg, uid, ct);
            }

            // 在清除追踪前提取聚合根，用于后续领域事件分发
            var aggregates = _tracked.Values
                .SelectMany(dict => dict.Values)
                .Select(entry => entry.Entity)
                .OfType<AggregateRoot>()
                .ToList();

            _tracked.Clear();

            // 领域事件分发（仅在提交成功后）
            if (aggregates.Count > 0)
            {
                var dispatcher = _serviceProvider?.GetService<IDomainEventDispatcher>();
                if (dispatcher != null)
                {
                    await dispatcher.DispatchAsync(aggregates, ct);
                }
            }

            return affected;
        }
        catch
        {
            tx.Rollback();
            _tracked.Clear();
            throw;
        }
    }

    /// <summary>重新加载实体（当前实现为空操作）</summary>
    public Task ReloadAsync<T>(T entity, CancellationToken ct = default) where T : class => Task.CompletedTask;

    // ==================================================================
    // 其他方法
    // ==================================================================

    /// <summary>
    /// 根据编码查找审批类型
    /// </summary>
    /// <param name="code">审批类型编码</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批类型，未找到时返回 null</returns>
    public async Task<ApprovalType?> FindApprovalTypeByCodeAsync(string code, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return await conn.QuerySingleOrDefaultAsync<ApprovalType>(_sql.Get("Common.Select.ApprovalType.ByCode"), new { Code = code });
    }
    /// <summary>
    /// 查询导入批次及其明细项
    /// </summary>
    /// <remarks>使用 QueryMultipleAsync 实现主子表一次性加载</remarks>
    /// <param name="id">导入批次 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>含明细项的导入批次，未找到时返回 null</returns>
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

    /// <summary>
    /// 查询指定公司的欠款通知单（可选按账期筛选）
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="period">账期（可选，格式 yyyy-MM）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>欠款通知单列表</returns>
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

    /// <summary>
    /// 开启共享事务 — 创建供同一 UoW 内多个操作复用的连接+事务
    /// </summary>
    /// <remarks>
    /// 被 ExecuteSqlRawAsync 复用，可确保同一个 UoW 中的原始 SQL 操作在同一个事务中执行。
    /// 释放时通过 DapperTransaction 的回调自动清理共享连接引用。
    /// </remarks>
    /// <param name="ct">取消令牌</param>
    /// <returns>事务包装器</returns>
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

    /// <summary>
    /// 带重试的提交 — 在遇到死锁或临时异常时自动重试，提高分布式环境下的写入成功率
    /// </summary>
    /// <remarks>
    /// 重试策略：首次重试无延迟，后续每次递增等待时间（100ms × 重试次数）。
    /// 达到最大重试次数后仍失败则直接抛出异常，不再捕获。
    /// </remarks>
    /// <param name="maxRetries">最大重试次数，默认 3 次</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响的行数</returns>
    public async Task<int> CommitWithRetryAsync(int maxRetries = 3, CancellationToken ct = default)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await CommitAsync(ct);
            }
            catch (InvalidOperationException)
            {
                // 业务层异常（如状态守卫冲突）不重试，直接抛出
                throw;
            }
            catch (Exception ex) when (attempt < maxRetries && IsTransient(ex))
            {
                // 短暂等待后重试，首次重试无延迟
                if (attempt > 1)
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), ct);
            }
        }
        return await CommitAsync(ct);
    }

    private static bool IsTransient(Exception ex)
    {
        // SQL Server 可恢复的错误：死锁（1205）、超时（-2）、快照隔离冲突（3960）
        if (ex is Microsoft.Data.SqlClient.SqlException sqlEx)
            return sqlEx.Number is 1205 or -2 or 3960;
        if (ex.InnerException is Microsoft.Data.SqlClient.SqlException innerSqlEx)
            return innerSqlEx.Number is 1205 or -2 or 3960;
        // 超时异常
        if (ex is TaskCanceledException or OperationCanceledException)
            return true;
        return false;
    }

    /// <summary>
    /// 执行原始 SQL（参数为 IEnumerable）
    /// </summary>
    /// <remarks>
    /// 优先使用共享连接+事务（如果存在），否则创建新连接。
    /// 参数自动命名为 @p0, @p1, @p2...
    /// </remarks>
    /// <param name="sql">原始 SQL 语句</param>
    /// <param name="parameters">参数列表</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响的行数</returns>
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

    /// <summary>
    /// 执行原始 SQL（参数为匿名对象或 DynamicParameters）
    /// </summary>
    /// <remarks>优先使用共享连接+事务（如果存在）</remarks>
    /// <param name="sql">原始 SQL 语句</param>
    /// <param name="parameters">参数对象</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响的行数</returns>
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

    /// <summary>
    /// 释放共享事务和共享连接
    /// </summary>
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

    /// <summary>
    /// 将实体对象转换为字典（用于快照比较）
    /// </summary>
    /// <remarks>排除 DomainEvents、导航属性和只读计算属性</remarks>
    /// <param name="entity">实体对象</param>
    /// <returns>属性名→属性值的字典</returns>
    internal static Dictionary<string, object?> EntityToDict(object entity)
    {
        var dict = new Dictionary<string, object?>();
        var props = entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var p in props)
        {
            if (p.Name is "DomainEvents") continue;
            if (!p.CanWrite) continue; // 排除计算属性（如 IsVacant/IsRented）
            if (IsNavProp(p)) continue;
            dict[p.Name] = p.GetValue(entity);
        }
        return dict;
    }

    /// <summary>
    /// 计算两个字典的差异（用于变更追踪的字段级别差异检测）
    /// </summary>
    /// <param name="old">旧快照字典</param>
    /// <param name="now">当前值字典</param>
    /// <returns>发生变化的字段集合</returns>
    internal static Dictionary<string, object?> DiffDict(Dictionary<string, object?> old, Dictionary<string, object?> now)
    {
        var diff = new Dictionary<string, object?>();
        var exclude = new HashSet<string> { "UpdatedAt", "UpdatedBy", "UpdatedIp", "UpdatedHostname" };
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

    /// <summary>
    /// 判断属性是否为导航属性
    /// </summary>
    /// <remarks>Nullable&lt;T&gt; 视为标量值类型而非导航属性</remarks>
    /// <param name="p">属性信息</param>
    /// <returns>是导航属性返回 true</returns>
    private static bool IsNavProp(System.Reflection.PropertyInfo p)
    {
        var t = p.PropertyType;
        // 可空值类型 Nullable<T> 不是导航属性，应参与 INSERT/UPDATE
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            return false;
        return t == typeof(System.Collections.IList) || t.IsGenericType ||
               p.Name is "DomainEvents" or "Records" or "Roles";
    }
}
