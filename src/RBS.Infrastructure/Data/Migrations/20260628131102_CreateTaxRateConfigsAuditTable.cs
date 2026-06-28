using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTaxRateConfigsAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TaxRateConfigs') AND name = 'LandlordId') EXEC sp_rename 'TaxRateConfigs.LandlordId', 'CompanyId', 'COLUMN';");
            migrationBuilder.Sql(@"
CREATE TABLE [TaxRateConfigs_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NULL, [Rate] decimal(5,2) NULL, [EffectiveDate] date NULL,
    [IsActive] bit NULL, [CompanyId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_TaxRateConfigs_Audit_EntityId] ON [TaxRateConfigs_Audit] ([Id]);
CREATE INDEX [IX_TaxRateConfigs_Audit_ChangedAt] ON [TaxRateConfigs_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [TaxRateConfigs_Audit];");
        }
    }
}
