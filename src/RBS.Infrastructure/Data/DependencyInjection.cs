using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Infrastructure.Data.Interceptors;
using RBS.Infrastructure.Data.Repositories;
using RBS.Core.DomainServices;
using RBS.Infrastructure.Data.Services;
using RBS.Application.EventHandlers;
using RBS.Core.Common;
using RBS.Core.Entities.Base;
using UnitOfWorkImpl = RBS.Infrastructure.Data.UnitOfWork.UnitOfWork;

namespace RBS.Infrastructure.Data;

/// <summary>
/// 依赖注入注册扩展方法
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(60);
                });

            options.AddInterceptors(
                sp.GetRequiredService<MirrorAuditInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcher>());
        });

        // 拦截器
        services.AddScoped<MirrorAuditInterceptor>();
        services.AddScoped<DomainEventDispatcher>();

        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWorkImpl>();

        // 仓储
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IFeeCodeRepository, FeeCodeRepository>();
        services.AddScoped<IReceivablePlanRepository, ReceivablePlanRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IPaymentChannelRepository, PaymentChannelRepository>();
        services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
        services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();
        services.AddScoped<IHolidayCalendarRepository, HolidayCalendarRepository>();

        // 多租户
        services.AddScoped<ITenantService, TenantService>();

        // 审计
        services.AddScoped<IAuditService, AuditService>();

        // 领域服务
        services.AddScoped<IContractDomainService, ContractDomainService>();

        // 领域事件处理器
        services.AddScoped<IEventHandler<ApprovalCompletedEvent>, ApprovalCompletedEventHandler>();
        services.AddScoped<IBillingDomainService, BillingDomainService>();
        services.AddScoped<IApprovalDomainService, ApprovalDomainService>();

        return services;
    }
}
