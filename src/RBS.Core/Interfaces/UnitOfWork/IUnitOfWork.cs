namespace RBS.Core.Interfaces.UnitOfWork;

using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Import;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.Repositories;

/// <summary>
/// 工作单元接口 — 跨仓储事务一致性
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // 组织权限
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IMenuRepository Menus { get; }
    ICompanyRepository Companies { get; }

    // 房屋管理
    IRepository<HousingUnit> HousingUnits { get; }
    IRepository<RoomType> RoomTypes { get; }
    IRepository<FloorLevelBand> FloorLevelBands { get; }
    IRepository<RoomPricingStandard> RoomPricingStandards { get; }

    // 合同
    IContractRepository Contracts { get; }
    ITenantRepository Tenants { get; }

    // 费用/应收
    IFeeCodeRepository FeeCodes { get; }
    IReceivablePlanRepository ReceivablePlans { get; }

    // 收款
    IReceiptRepository Receipts { get; }
    IPaymentChannelRepository PaymentChannels { get; }

    // 抄表
    IMeterReadingRepository MeterReadings { get; }

    // 审批
    IApprovalRequestRepository ApprovalRequests { get; }
    IRepository<ApprovalType> ApprovalTypes { get; }
    IRepository<ApprovalLevelConfig> ApprovalLevelConfigs { get; }

    // 系统配置
    IHolidayCalendarRepository HolidayCalendars { get; }
    IRepository<TaxRateConfig> TaxRateConfigs { get; }
    IRepository<LateFeeConfig> LateFeeConfigs { get; }
    IRepository<AccountingSubject> AccountingSubjects { get; }
    IRepository<JobSchedule> JobSchedules { get; }
    IRepository<JobTemplate> JobTemplates { get; }
    IRepository<JobScheduleExecution> JobScheduleExecutions { get; }

    /// <summary>按 Code 查找审批类型（忽略公司过滤器，系统级查找）</summary>
    Task<ApprovalType?> FindApprovalTypeByCodeAsync(string code, CancellationToken ct = default);

    // ===== 导入 =====
    IRepository<ImportBatch> ImportBatches { get; }
    IRepository<ImportBatchItem> ImportBatchItems { get; }
    Task<ImportBatch?> GetImportBatchWithItemsAsync(Guid id, CancellationToken ct = default);

    /// <summary>提交所有变更（自动事务）</summary>
    Task<int> CommitAsync(CancellationToken ct = default);

    /// <summary>从数据库重新加载实体的所有属性（覆盖追踪中的值）</summary>
    Task ReloadAsync<T>(T entity, CancellationToken ct = default) where T : class;

    /// <summary>显式开启数据库事务</summary>
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>执行原始 SQL 命令（绕过 SaveChanges 管道，不触发拦截器）</summary>
    Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken ct = default);

    /// <summary>乐观锁失败时自动重试</summary>
    Task<int> CommitWithRetryAsync(int maxRetries = 3, CancellationToken ct = default);
}
