using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Infrastructure.Data.Repositories;
using RBS.Core.DomainServices;
using RBS.Infrastructure.Data.Services;
using RBS.Application.EventHandlers;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Property;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.SystemConfig;
using DapperUnitOfWork = RBS.Infrastructure.Data.UnitOfWork.DapperUnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace RBS.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        // Dapper 连接工厂
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

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
        services.AddScoped<IRepository<FloorLevelBand>, DapperRepository<FloorLevelBand>>();
        services.AddScoped<IRepository<TaxRateConfig>, DapperRepository<TaxRateConfig>>();
        services.AddScoped<IRepository<AccountingSubject>, DapperRepository<AccountingSubject>>();

        // IUnitOfWork（Dapper 实现）
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(3);
                sqlOptions.CommandTimeout(60);
            });
        });


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
        services.AddScoped<IBillingDomainService, BillingDomainService>();
        services.AddScoped<IApprovalDomainService, ApprovalDomainService>();

        return services;
    }
}
