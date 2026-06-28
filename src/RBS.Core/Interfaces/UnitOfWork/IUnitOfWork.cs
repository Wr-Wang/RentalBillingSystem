namespace RBS.Core.Interfaces.UnitOfWork;

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
    IBuildingRepository Buildings { get; }
    IRoomRepository Rooms { get; }

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

    // 系统配置
    IHolidayCalendarRepository HolidayCalendars { get; }

    /// <summary>提交所有变更（自动事务）</summary>
    Task<int> CommitAsync(CancellationToken ct = default);

    /// <summary>显式开启数据库事务</summary>
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>乐观锁失败时自动重试</summary>
    Task<int> CommitWithRetryAsync(int maxRetries = 3, CancellationToken ct = default);
}
