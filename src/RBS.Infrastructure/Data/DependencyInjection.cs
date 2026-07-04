using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Infrastructure.Data.Repositories;
using RBS.Infrastructure.Data.SqlMaps;
using RBS.Core.DomainServices;
using RBS.Core.Entities.Accounting;
using RBS.Infrastructure.Data.Services;
using RBS.Application.EventHandlers;
using RBS.Application.Services.SystemConfig;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Property;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.SystemConfig;
using RBS.Infrastructure.Data.TypeHandlers;
using RBS.Infrastructure.PdfGeneration;
using RBS.Infrastructure.Scheduling;
using DapperUnitOfWork = RBS.Infrastructure.Data.UnitOfWork.DapperUnitOfWork;

namespace RBS.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        // Dapper 类型处理器（值对象自动转换）
        ValueObjectHandlers.Register();

        // Dapper 连接工厂
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

        // SQL 映射加载器
        services.AddSingleton<ISqlLoader>(sp =>
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "Data", "SqlMaps", "SqlMaps.xml");
            if (!File.Exists(xmlPath))
                xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SqlMaps", "SqlMaps.xml");
            return new SqlLoader(xmlPath);
        });

        // ===== Dapper 仓储 =====
        services.AddScoped<IUserRepository, DapperUserRepository>();
        services.AddScoped<IRoleRepository, DapperRoleRepository>();
        services.AddScoped<IMenuRepository, DapperMenuRepository>();
        services.AddScoped<ICompanyRepository, DapperCompanyRepository>();
        services.AddScoped<IFeeCodeRepository, DapperFeeCodeRepository>();
        services.AddScoped<IPaymentChannelRepository, DapperPaymentChannelRepository>();
        services.AddScoped<IHolidayCalendarRepository, DapperHolidayCalendarRepository>();
        services.AddScoped<IRepository<HousingUnit>, DapperRepository<HousingUnit>>();
        services.AddScoped<IRepository<RoomType>, DapperRepository<RoomType>>();
        services.AddScoped<IRepository<ApprovalType>, DapperRepository<ApprovalType>>();
        services.AddScoped<IRepository<ApprovalLevelConfig>, DapperRepository<ApprovalLevelConfig>>();
        services.AddScoped<IApprovalBizDataRepository, DapperApprovalBizDataRepository>();
        services.AddScoped<IApprovalFeeItemRepository, DapperApprovalFeeItemRepository>();
        services.AddScoped<IRepository<FloorLevelBand>, DapperRepository<FloorLevelBand>>();
        services.AddScoped<IRepository<TaxRateConfig>, DapperRepository<TaxRateConfig>>();
        services.AddScoped<IRepository<AccountingSubject>, DapperRepository<AccountingSubject>>();

        // IUnitOfWork（Dapper 实现）
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();

        // 多租户
        services.AddScoped<ITenantService, TenantService>();

        // 审计
        services.AddScoped<RBS.Application.Common.Interfaces.IAuditService, AuditService>();
        services.AddScoped<IAuditLogWriter>(sp =>
            new AuditLogWriter(connectionString));

        // 领域服务
        services.AddScoped<IContractDomainService, ContractDomainService>();

        // 领域事件处理器
        services.AddScoped<IEventHandler<ApprovalCompletedEvent>, ApprovalCompletedEventHandler>();
        services.AddScoped<IEventHandler<ApprovalSubmittedEvent>, ApprovalSubmittedEventHandler>();
        services.AddScoped<IEventHandler<ApprovalLevelAdvancedEvent>, ApprovalLevelAdvancedEventHandler>();
        services.AddScoped<IEventHandler<ContractSuspendedEvent>, ContractSuspendedEventHandler>();
        services.AddScoped<IEventHandler<ContractResumedEvent>, ContractResumedEventHandler>();
        services.AddScoped<IEventHandler<ContractRentAdjustedEvent>, ContractRentAdjustedEventHandler>();
        services.AddScoped<IBillingDomainService, BillingDomainService>();
        services.AddScoped<IApprovalDomainService, ApprovalDomainService>();

        // 通知服务
        services.AddScoped<INotificationService, NotificationService>();

        // PDF 生成
        services.AddScoped<IBillPdfGenerator, BillPdfGenerator>();

        // 调度引擎
        services.AddHostedService<SchedulingHostedService>();

        return services;
    }
}
