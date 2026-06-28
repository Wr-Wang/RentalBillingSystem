namespace RBS.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using RBS.Core.Entities.Base;
using RBS.Core.Entities.Organization;
using RBS.Core.Entities.Property;
using RBS.Core.Entities.Contract;
using RBS.Core.Entities.Billing;
using RBS.Core.Entities.Approval;
using RBS.Core.Entities.Accounting;
using RBS.Core.Entities.SystemConfig;
using RBS.Core.Interfaces.Services;
using RBS.Infrastructure.Data.Extensions;
using RBS.Infrastructure.Data.Interceptors;
using System.Linq.Expressions;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantService _tenantService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService,
        ITenantService tenantService)
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantService = tenantService;
    }

    // ===== 组织权限 =====
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();
    public DbSet<Landlord> Landlords => Set<Landlord>();
    public DbSet<UserLandlordScope> UserLandlordScopes => Set<UserLandlordScope>();

    // ===== 房屋管理 =====
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<FloorLevelBand> FloorLevelBands => Set<FloorLevelBand>();
    public DbSet<BuildingFloorLevelConfig> BuildingFloorLevelConfigs => Set<BuildingFloorLevelConfig>();
    public DbSet<RoomPricingStandard> RoomPricingStandards => Set<RoomPricingStandard>();

    // ===== 合同管理 =====
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractTenant> ContractTenants => Set<ContractTenant>();
    public DbSet<ContractFeeConfig> ContractFeeConfigs => Set<ContractFeeConfig>();
    public DbSet<RoomFeeDefault> RoomFeeDefaults => Set<RoomFeeDefault>();

    // ===== 租客 =====
    public DbSet<Tenant> Tenants => Set<Tenant>();

    // ===== 费用 =====
    public DbSet<FeeCode> FeeCodes => Set<FeeCode>();
    public DbSet<FeeCodeTemplate> FeeCodeTemplates => Set<FeeCodeTemplate>();

    // ===== 抄表 =====
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<MeterEstimationConfig> MeterEstimationConfigs => Set<MeterEstimationConfig>();

    // ===== 应收/账单 =====
    public DbSet<ReceivablePlan> ReceivablePlans => Set<ReceivablePlan>();
    public DbSet<DebitNote> DebitNotes => Set<DebitNote>();
    public DbSet<DebitNoteItem> DebitNoteItems => Set<DebitNoteItem>();

    // ===== 收款 =====
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptAllocation> ReceiptAllocations => Set<ReceiptAllocation>();
    public DbSet<PaymentChannel> PaymentChannels => Set<PaymentChannel>();

    // ===== 押金 =====
    public DbSet<DepositLog> DepositLogs => Set<DepositLog>();

    // ===== 催缴 =====
    public DbSet<CollectionStage> CollectionStages => Set<CollectionStage>();
    public DbSet<CollectionRecord> CollectionRecords => Set<CollectionRecord>();

    // ===== 审批 =====
    public DbSet<ApprovalType> ApprovalTypes => Set<ApprovalType>();
    public DbSet<ApprovalLevelConfig> ApprovalLevelConfigs => Set<ApprovalLevelConfig>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalRecord> ApprovalRecords => Set<ApprovalRecord>();

    // ===== 会计 =====
    public DbSet<AccountingSubject> AccountingSubjects => Set<AccountingSubject>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    // ===== 配置 =====
    public DbSet<HolidayCalendar> HolidayCalendars => Set<HolidayCalendar>();
    public DbSet<TaxRateConfig> TaxRateConfigs => Set<TaxRateConfig>();
    public DbSet<LateFeeConfig> LateFeeConfigs => Set<LateFeeConfig>();

    // ===== 调度 =====
    public DbSet<ScheduledTaskLog> ScheduledTaskLogs => Set<ScheduledTaskLog>();
    public DbSet<JobSchedule> JobSchedules => Set<JobSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 全局配置：禁用级联删除
        modelBuilder.UseLogicalForeignKeys();

        // 按模块加载 Entity 配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // 应用多房东查询过滤器
        ApplyLandlordQueryFilter(modelBuilder);
    }

    private void ApplyLandlordQueryFilter(ModelBuilder modelBuilder)
    {
        var effectiveLandlordId = _tenantService.EffectiveLandlordId;
        if (effectiveLandlordId == null) return; // 超管查看全部数据时不过滤

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasLandlord).IsAssignableFrom(entityType.ClrType)
                && entityType.FindProperty(nameof(IHasLandlord.LandlordId)) != null)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(IHasLandlord.LandlordId));
                var landlordId = Expression.Constant(effectiveLandlordId.Value);

                var lambda = Expression.Lambda(
                    Expression.Equal(property, landlordId),
                    parameter);

                entityType.SetQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var utcNow = DateTime.Now;

        // 审计字段自动注入
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreated(userId, utcNow, null, null);
                    break;
                case EntityState.Modified:
                    entry.Entity.SetUpdated(userId, utcNow, null, null);
                    entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedIp)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedHostname)).IsModified = false;
                    break;
            }
        }

        // 关联表简化审计
        foreach (var entry in ChangeTracker.Entries<AssociationEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreated(userId, utcNow);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
