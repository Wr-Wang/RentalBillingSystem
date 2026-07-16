namespace RBS.Core.Interfaces.UnitOfWork;

using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Import;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Banking;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.Repositories;

/// <summary>
/// 工作单元接口 — 跨仓储事务一致性。
/// 协调多个仓储的写入操作，确保所有变更在同一个数据库事务中提交或回滚。
/// 聚合所有领域仓储的访问入口，同时提供事务管理、原始 SQL 执行、
/// 乐观锁重试等基础设施能力。
/// Core 层通过此接口解耦对具体 ORM（EF Core / Dapper）的依赖。
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ===== 组织权限 =====

    /// <summary>用户仓储</summary>
    IUserRepository Users { get; }
    /// <summary>角色仓储</summary>
    IRoleRepository Roles { get; }
    /// <summary>菜单仓储</summary>
    IMenuRepository Menus { get; }
    /// <summary>公司仓储</summary>
    ICompanyRepository Companies { get; }

    // ===== 房屋管理 =====

    /// <summary>房屋单元仓储</summary>
    IRepository<HousingUnit> HousingUnits { get; }
    /// <summary>房型仓储</summary>
    IRepository<RoomType> RoomTypes { get; }
    /// <summary>楼层系数（楼层差价带）仓储</summary>
    IRepository<FloorLevelBand> FloorLevelBands { get; }
    /// <summary>房间定价标准仓储</summary>
    IRepository<RoomPricingStandard> RoomPricingStandards { get; }

    // ===== 合同 =====

    /// <summary>合同仓储</summary>
    IContractRepository Contracts { get; }
    /// <summary>租客仓储</summary>
    ITenantRepository Tenants { get; }
    /// <summary>续签请求仓储</summary>
    IRenewalRequestRepository RenewalRequests { get; }

    // ===== 费用/应收 =====

    /// <summary>费用项目仓储</summary>
    IFeeCodeRepository FeeCodes { get; }
    /// <summary>费用模板仓储</summary>
    IRepository<FeeCodeTemplate> FeeCodeTemplates { get; }
    /// <summary>日记账仓储（不可变的出账记录）</summary>
    IRepository<Journal> Journals { get; }

    // ===== 收款 =====

    /// <summary>收款单仓储</summary>
    IReceiptRepository Receipts { get; }
    /// <summary>支付渠道仓储</summary>
    IPaymentChannelRepository PaymentChannels { get; }

    // ===== 押金 =====

    /// <summary>押金变动日志仓储</summary>
    IRepository<DepositLog> DepositLogs { get; }

    // ===== 银行对账 =====

    /// <summary>银行流水仓储</summary>
    IRepository<BankStatement> BankStatements { get; }
    /// <summary>银行对账记录仓储</summary>
    IRepository<BankReconciliation> BankReconciliations { get; }
    /// <summary>银行匹配结果仓储</summary>
    IRepository<BankMatch> BankMatches { get; }

    // ===== 催缴 =====

    /// <summary>催缴阶段配置仓储</summary>
    IRepository<CollectionStage> CollectionStages { get; }
    /// <summary>催缴记录仓储</summary>
    IRepository<CollectionRecord> CollectionRecords { get; }

    // ===== 账单 =====

    /// <summary>账单（DebitNote）仓储</summary>
    IRepository<DebitNote> DebitNotes { get; }

    /// <summary>
    /// 根据公司和账期查询账单列表。
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="period">账期，格式"yyyy-MM"，可选</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>账单列表</returns>
    Task<List<DebitNote>> GetDebitNotesByCompanyAsync(Guid companyId, string? period = null, CancellationToken ct = default);

    /// <summary>
    /// 根据租客 ID 查询该租客相关的所有账单。
    /// </summary>
    /// <param name="tenantId">租客 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>账单列表</returns>
    Task<List<DebitNote>> GetDebitNotesByTenantAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// 根据账单 ID 获取账单明细项。
    /// </summary>
    /// <param name="debitNoteId">账单 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>账单明细项列表</returns>
    Task<List<DebitNoteItem>> GetDebitNoteItemsAsync(Guid debitNoteId, CancellationToken ct = default);

    // ===== 抄表 =====

    /// <summary>抄表记录仓储</summary>
    IMeterReadingRepository MeterReadings { get; }

    // ===== 审批 =====

    /// <summary>审批请求仓储</summary>
    IApprovalRequestRepository ApprovalRequests { get; }
    /// <summary>审批类型配置仓储</summary>
    IRepository<ApprovalType> ApprovalTypes { get; }
    /// <summary>审批级别配置仓储</summary>
    IRepository<ApprovalLevelConfig> ApprovalLevelConfigs { get; }
    /// <summary>审批业务数据仓储</summary>
    IApprovalBizDataRepository ApprovalBizData { get; }
    /// <summary>审批调价明细仓储</summary>
    IApprovalFeeItemRepository ApprovalFeeItems { get; }

    // ===== 审批闭环暂存表 =====

    /// <summary>合同创建请求暂存仓储</summary>
    IRepository<ContractCreateRequest> ContractCreateRequests { get; }
    /// <summary>合同创建请求租客暂存仓储</summary>
    IRepository<ContractCreateRequestTenant> ContractCreateRequestTenants { get; }
    /// <summary>合同创建请求费用暂存仓储</summary>
    IRepository<ContractCreateRequestFee> ContractCreateRequestFees { get; }
    /// <summary>合同变更请求暂存仓储</summary>
    IRepository<ContractModifyRequest> ContractModifyRequests { get; }
    /// <summary>租客创建请求暂存仓储</summary>
    IRepository<TenantCreateRequest> TenantCreateRequests { get; }
    /// <summary>补充费用申请暂存仓储</summary>
    IRepository<SupplementaryFeeRequest> SupplementaryFeeRequests { get; }
    /// <summary>补充费用申请明细暂存仓储</summary>
    IRepository<SupplementaryFeeRequestItem> SupplementaryFeeRequestItems { get; }
    /// <summary>应收生成请求暂存仓储</summary>
    IRepository<ReceivableGenerateRequest> ReceivableGenerateRequests { get; }
    /// <summary>应收生成请求明细暂存仓储</summary>
    IRepository<ReceivableGenerateRequestItem> ReceivableGenerateRequestItems { get; }

    // ===== 会计 =====

    /// <summary>会计期间仓储</summary>
    IRepository<AccountingPeriod> AccountingPeriods { get; }

    // ===== 系统配置 =====

    /// <summary>节假日日历仓储</summary>
    IHolidayCalendarRepository HolidayCalendars { get; }
    /// <summary>税率配置仓储</summary>
    IRepository<TaxRateConfig> TaxRateConfigs { get; }
    /// <summary>滞纳金配置仓储</summary>
    IRepository<LateFeeConfig> LateFeeConfigs { get; }
    /// <summary>会计科目仓储</summary>
    IRepository<AccountingSubject> AccountingSubjects { get; }
    /// <summary>定时任务调度配置仓储</summary>
    IRepository<JobSchedule> JobSchedules { get; }
    /// <summary>任务模板仓储</summary>
    IRepository<JobTemplate> JobTemplates { get; }
    /// <summary>任务调度执行记录仓储</summary>
    IRepository<JobScheduleExecution> JobScheduleExecutions { get; }
    /// <summary>自动续签配置仓储</summary>
    IRepository<AutoRenewConfig> AutoRenewConfigs { get; }

    /// <summary>
    /// 按 Code 查找审批类型（系统级查找，忽略公司过滤器）。
    /// </summary>
    /// <param name="code">审批类型代码（如"ContractTermination"、"FeeChange"等）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审批类型实体，未找到时返回 null</returns>
    Task<ApprovalType?> FindApprovalTypeByCodeAsync(string code, CancellationToken ct = default);

    // ===== 导入 =====

    /// <summary>导入批次仓储</summary>
    IRepository<ImportBatch> ImportBatches { get; }
    /// <summary>导入批次明细仓储</summary>
    IRepository<ImportBatchItem> ImportBatchItems { get; }

    /// <summary>
    /// 根据导入批次 ID 获取导入批次及其所有明细项。
    /// </summary>
    /// <param name="id">导入批次 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>包含明细项的导入批次实体，未找到时返回 null</returns>
    Task<ImportBatch?> GetImportBatchWithItemsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 提交所有变更（自动事务）。
    /// 将所有通过仓储执行的添加、更新、删除操作持久化到数据库，
    /// 所有操作在同一个数据库事务中完成。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响的行数</returns>
    Task<int> CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// 从数据库重新加载实体的所有属性（覆盖追踪中的值）。
    /// 用于在检测到乐观锁冲突或需要丢弃当前变更重新加载的场景。
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="entity">需重新加载的实体</param>
    /// <param name="ct">取消令牌</param>
    Task ReloadAsync<T>(T entity, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 显式开启数据库事务。
    /// 用于需要手动控制事务范围的场景（如跨多个 CommitAsync 调用的批量操作）。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>事务实例，调用方需确保最终提交或回滚</returns>
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// 执行原始 SQL 命令（绕过 SaveChanges 管道，不触发拦截器）。
    /// 用于需要直接执行 SQL 语句的场景，如批量更新、存储过程调用等。
    /// </summary>
    /// <param name="sql">要执行的 SQL 语句</param>
    /// <param name="parameters">SQL 参数（IEnumerable 格式）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响的行数</returns>
    Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken ct = default);

    /// <summary>
    /// 执行原始 SQL 命令（绕过 SaveChanges 管道，不触发拦截器）。
    /// 匿名对象参数的重载版本，适用于 Dapper 风格的参数传递。
    /// </summary>
    /// <param name="sql">要执行的 SQL 语句</param>
    /// <param name="parameters">SQL 参数（匿名对象格式，如 new { Id = 1 }）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响的行数</returns>
    Task<int> ExecuteSqlRawAsync(string sql, object parameters, CancellationToken ct = default);

    /// <summary>
    /// 乐观锁失败时自动重试。
    /// 当 CommitAsync 因乐观锁冲突（DbUpdateConcurrencyException）失败时，
    /// 自动进行重试，最多重试指定次数。
    /// </summary>
    /// <param name="maxRetries">最大重试次数，默认 3 次</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响的行数</returns>
    Task<int> CommitWithRetryAsync(int maxRetries = 3, CancellationToken ct = default);
}
