using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Application.Services.Billing;
using RBS.Application.Services.Contract;
using RBS.Application.Services.Organization;
using RBS.Application.Services.Property;
using RBS.Application.Services.Approval;
using RBS.Application.Services.SystemConfig;

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
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IBuildingService, BuildingService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IContractService, ContractAppService>();
        services.AddScoped<IBillingService, BillingAppService>();
        services.AddScoped<IApprovalTypeService, ApprovalTypeService>();
        services.AddScoped<IHolidayCalendarService, HolidayCalendarService>();
        services.AddScoped<IFeeCodeService, FeeCodeService>();
        services.AddScoped<IPricingStandardService, PricingStandardService>();
        services.AddScoped<IPaymentChannelService, PaymentChannelService>();
        services.AddScoped<IFloorLevelBandService, FloorLevelBandService>();

        return services;
    }
}
