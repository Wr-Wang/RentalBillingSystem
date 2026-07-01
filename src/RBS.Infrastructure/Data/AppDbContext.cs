using Microsoft.EntityFrameworkCore;

namespace RBS.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Core.Entities.Billing.DebitNote> DebitNotes => Set<Core.Entities.Billing.DebitNote>();
    public DbSet<Core.Entities.Accounting.Voucher> Vouchers => Set<Core.Entities.Accounting.Voucher>();
    public DbSet<Core.Entities.Accounting.JournalEntry> JournalEntries => Set<Core.Entities.Accounting.JournalEntry>();
    public DbSet<Core.Entities.Contract.ContractFeeConfig> ContractFeeConfigs => Set<Core.Entities.Contract.ContractFeeConfig>();
    public DbSet<Core.Entities.Billing.ReceivablePlan> ReceivablePlans => Set<Core.Entities.Billing.ReceivablePlan>();
    public DbSet<Core.Entities.Organization.Company> Companies => Set<Core.Entities.Organization.Company>();
    public DbSet<Core.Entities.Contract.Contract> Contracts => Set<Core.Entities.Contract.Contract>();
    public DbSet<Core.Entities.Billing.Receipt> Receipts => Set<Core.Entities.Billing.Receipt>();
    public DbSet<Core.Entities.Billing.DepositLog> DepositLogs => Set<Core.Entities.Billing.DepositLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
