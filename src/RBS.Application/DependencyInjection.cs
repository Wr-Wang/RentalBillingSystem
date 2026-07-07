using Microsoft.Extensions.DependencyInjection;
using RBS.Application.Common.Interfaces;
using RBS.Application.Services.Billing;
using RBS.Application.Services.Contract;
using RBS.Application.Services.Import;
using RBS.Application.Services.Organization;
using RBS.Core.Interfaces.Services;
using RBS.Application.Services.Property;
using RBS.Application.Services.Approval;
using RBS.Application.Services.SystemConfig;
using RBS.Application.Services.Accounting;
using RBS.Application.Services.Reporting;
using RBS.Application.Services.Scheduling;

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
        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IContractService, ContractAppService>();
        services.AddScoped<IRenewalService, RenewalService>();
        services.AddScoped<IBillingService, BillingAppService>();
        services.AddScoped<IApprovalTypeService, ApprovalTypeService>();
        services.AddScoped<IHolidayCalendarService, HolidayCalendarService>();
        services.AddScoped<IFeeCodeService, FeeCodeService>();
        services.AddScoped<ITaxRateConfigService, TaxRateConfigService>();
        services.AddScoped<IAccountingSubjectService, AccountingSubjectService>();
        services.AddScoped<ISchedulerService, SchedulerService>();
        services.AddScoped<IJobTemplateService, JobTemplateService>();
        services.AddScoped<IJobScheduleExecutionService, JobScheduleExecutionService>();
        services.AddScoped<IPricingStandardService, PricingStandardService>();
        services.AddScoped<IPaymentChannelService, PaymentChannelService>();
        services.AddScoped<IFloorLevelBandService, FloorLevelBandService>();
        services.AddScoped<ILateFeeConfigService, LateFeeConfigService>();
        services.AddScoped<IAutoRenewConfigService, AutoRenewConfigService>();
        services.AddScoped<IHousingUnitService, HousingUnitService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IImportTypeHandler, HousingUnitImportHandler>();
        services.AddScoped<IReceivableGenerationService, ReceivableGenerationService>();
        services.AddScoped<IDepositService, DepositService>();
        services.AddScoped<IDebitNoteService, DebitNoteService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IBankingService, BankingService>();
        services.AddScoped<IContractTimelineService, ContractTimelineService>();
        services.AddScoped<IAutoVoucherService, AutoVoucherService>();
        services.AddScoped<IReportingService, ReportingService>();
		services.AddScoped<IJournalGenerationService, JournalGenerationService>();

        // 调度执行监控
        services.AddScoped<ITaskMonitorService, TaskMonitorService>();

        // 任务步骤日志
        services.AddScoped<ITaskStepLogger, TaskStepLogger>();

        // 定时作业
        services.AddTransient<IScheduledJob, BillJob>();
        services.AddTransient<IScheduledJob, SettleJob>();
        services.AddTransient<IScheduledJob, AutoRenewJob>();
        services.AddTransient<IScheduledJob, CollectionJob>();
        services.AddTransient<IScheduledJob, RenewalReminderJob>();

        return services;
    }
}
