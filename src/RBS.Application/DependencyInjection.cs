using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Application.Services.Billing;
using RBS.Application.Services.Contract;
using RBS.Application.Services.Organization;
using RBS.Application.Services.Property;

namespace RBS.Application;

/// <summary>
/// 应用层依赖注入注册
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // 应用服务
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBuildingService, BuildingService>();
        services.AddScoped<IContractService, ContractAppService>();
        services.AddScoped<IBillingService, BillingAppService>();

        return services;
    }
}
