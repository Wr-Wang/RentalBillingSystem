using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Application.Services.Region;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Infrastructure.Data.Repositories;
using RBS.Infrastructure.Data.SqlMaps;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Accounting;
using RBS.Infrastructure.Data.Services;
using RBS.Application.Services.SystemConfig;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Property;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data.TypeHandlers;
using RBS.Infrastructure.Data.Configs;
using RBS.Infrastructure.PdfGeneration;
using RBS.Infrastructure.Scheduling;
using DapperUnitOfWork = RBS.Infrastructure.Data.UnitOfWork.DapperUnitOfWork;

namespace RBS.Infrastructure.Data;

/// <summary>
/// 基础设施层依赖注入配置
/// </summary>
/// <remarks>
/// 注册顺序：
/// 1. Dapper 类型处理器（值对象自动转换）
/// 2. 连接工厂（单例）
/// 3. SQL 映射加载器（单例）
/// 4. Dapper 仓储（Scoped）
/// 5. 工作单元（Scoped）
/// 6. 多租户/审计/领域服务
/// 7. PDF 生成/调度引擎
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// 注册基础设施层所有服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">应用配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddInfrastructureData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        // ===== Dapper 类型处理器（值对象自动转换） =====
        // 注册值对象类型处理器，实现 Dapper 与领域层的类型适配
        ValueObjectHandlers.Register();

        // ===== Dapper 连接工厂（单例） =====
        // 所有仓储共享同一个连接工厂，但每次 CreateConnection 创建新连接
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

        // ===== SQL 映射加载器（单例） =====
        // 启动时从 SqlMaps.xml 加载全部 SQL 到内存，运行时只读
        services.AddSingleton<ISqlLoader>(sp =>
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "Data", "SqlMaps", "SqlMaps.xml");
            if (!File.Exists(xmlPath))
                xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SqlMaps", "SqlMaps.xml");
            return new SqlLoader(xmlPath);
        });

        // ===== Dapper 仓储（Scoped） =====
        // 每种仓储对应一个接口，支持依赖注入
        services.AddScoped<IUserRepository, DapperUserRepository>();
        services.AddScoped<IRoleRepository, DapperRoleRepository>();
        services.AddScoped<IMenuRepository, DapperMenuRepository>();
        services.AddScoped<ICompanyRepository, DapperCompanyRepository>();
        services.AddScoped<IFeeCodeRepository, DapperFeeCodeRepository>();
        services.AddScoped<IPaymentChannelRepository, DapperPaymentChannelRepository>();
        services.AddScoped<IHolidayCalendarRepository, DapperHolidayCalendarRepository>();
        // 标准实体使用泛型仓储，无需额外实现
        services.AddScoped<IRepository<HousingUnit>, DapperRepository<HousingUnit>>();
        services.AddScoped<IRepository<RoomType>, DapperRepository<RoomType>>();
        services.AddScoped<IRepository<ApprovalType>, DapperRepository<ApprovalType>>();
        services.AddScoped<IRepository<ApprovalLevelConfig>, DapperRepository<ApprovalLevelConfig>>();
        services.AddScoped<IApprovalBizDataRepository, DapperApprovalBizDataRepository>();
        services.AddScoped<IApprovalFeeItemRepository, DapperApprovalFeeItemRepository>();
        services.AddScoped<IRepository<FloorLevelBand>, DapperRepository<FloorLevelBand>>();
        services.AddScoped<IRepository<TaxRateConfig>, DapperRepository<TaxRateConfig>>();
        services.AddScoped<IRepository<AccountingSubject>, DapperRepository<AccountingSubject>>();
        services.AddScoped<IGLBalanceRepository, DapperGLBalanceRepository>();

        // ===== IUnitOfWork（Scoped） =====
        // 工作单元聚合所有仓储，支持变更追踪和事务提交
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();

        // ===== 任务调度仓储（Scoped） =====
        // 用于调度系统的任务日志和步骤日志管理
        services.AddScoped<ITaskLogRepository, DapperTaskLogRepository>();
        services.AddScoped<ITaskStepLogRepository, DapperTaskStepLogRepository>();
        services.AddScoped<IBillJobFailedContractRepository, DapperBillJobFailedContractRepository>();

        // ===== 多租户（Scoped） =====
        // 基于 HttpContext 的 CompanyId 解析，每个请求一个实例
        services.AddScoped<ITenantService, TenantService>();

        // ===== 审计（Scoped） =====
        // 审计查询服务和日志写入器（独立连接，失败不影响主操作）
        services.AddScoped<RBS.Application.Common.Interfaces.IAuditService, AuditService>();
        services.AddScoped<IAuditLogWriter>(sp =>
            new AuditLogWriter(connectionString, sp.GetRequiredService<ILogger<AuditLogWriter>>()));

        // ===== 客户端信息服务（用于审计 IP/主机名捕获） =====
        // 注意：IHttpContextAccessor 由 API 层 Program.cs 注册，此处只需注册 IClientInfoService
        // 如果 IHttpContextAccessor 不可用（如后台任务），IClientInfoService 返回 null

        // ===== 审计装饰器（统一所有仓储的审计逻辑） =====
        services.AddScoped<RepositoryAuditService>();

        // ===== 审计字段配置（Singleton — 启动时从 JSON 加载） =====
        var configJsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "Configs", "audit-field-config.json");
        if (!File.Exists(configJsonPath))
            configJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Configs", "audit-field-config.json");
        services.AddSingleton(new AuditFieldConfigLoader(configJsonPath));

        // ===== 领域服务（Scoped） =====
        services.AddScoped<IContractDomainService, ContractDomainService>();
        services.AddScoped<IBillingDomainService, BillingDomainService>();
        services.AddScoped<IApprovalDomainService, ApprovalDomainService>();
        services.AddScoped<IPropertyDomainService, PropertyDomainService>();

        // ===== 通知服务（Scoped） =====
        services.AddScoped<INotificationService, NotificationService>();

        // ===== 行政区划 API（有 Key 用高德，无 Key 用桩） =====
            var amapKey = configuration.GetSection("Amap:ApiKey")?.Value;
            if (!string.IsNullOrWhiteSpace(amapKey))
            {
                services.AddScoped<IRegionApiService>(sp =>
                    new AmapRegionApiService(amapKey, sp.GetRequiredService<ILogger<AmapRegionApiService>>()));
                services.AddScoped<RegionApiStubService>();
            }
            else
            {
                services.AddScoped<IRegionApiService, RegionApiStubService>();
            }

            // ===== 领域事件调度器（Scoped） =====
        // 在 UoW 提交成功后自动分发聚合根上的领域事件
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // ===== 批量插入器（Scoped，用 SqlBulkCopy 加速大数据量写入） =====
        services.AddScoped<IBulkInserter, BulkInserter>();

        // ===== PDF 生成（Scoped） =====
        // 使用 QuestPDF 渲染欠款通知单
        services.AddScoped<IBillPdfGenerator, BillPdfGenerator>();

        // ===== 调度引擎（Singleton Hosted Services） =====
        // SchedulingHostedService：每 60 秒轮询，触发到期作业
        // JobScheduleGenerator：每小时扫描，按规则生成执行记录
        services.AddHostedService<SchedulingHostedService>();
        services.AddHostedService<JobScheduleGenerator>();

        return services;
    }
}
