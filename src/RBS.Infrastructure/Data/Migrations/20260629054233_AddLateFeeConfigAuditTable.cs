using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    public partial class AddLateFeeConfigAuditTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE [LateFeeConfigs_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [DailyRate] decimal(7,5) NULL, [GraceDays] int NULL, [MaxRate] decimal(5,2) NULL,
    [MinAmount] decimal(18,2) NULL, [EffectiveDate] date NULL,
    [IsActive] bit NULL, [CompanyId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_LateFeeConfigs_Audit_EntityId] ON [LateFeeConfigs_Audit] ([Id]);
CREATE INDEX [IX_LateFeeConfigs_Audit_ChangedAt] ON [LateFeeConfigs_Audit] ([AuditChangedAt] DESC);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS [LateFeeConfigs_Audit];");
        }
    }
}
