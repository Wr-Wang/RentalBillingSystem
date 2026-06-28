using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateFeeCodesAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE [FeeCodes_Audit] (
    [Id] nvarchar(50) NOT NULL,
    [AuditAction] nvarchar(20) NOT NULL,
    [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL,
    [AuditChangedBy] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NULL,
    [Name] nvarchar(100) NULL,
    [BillingMode] nvarchar(20) NULL,
    [Unit] nvarchar(20) NULL,
    [SortOrder] int NULL,
    [IsActive] bit NULL,
    [Category] nvarchar(50) NULL,
    [IsRequired] bit NULL,
    [CompanyId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_FeeCodes_Audit_EntityId] ON [FeeCodes_Audit] ([Id]);
CREATE INDEX [IX_FeeCodes_Audit_ChangedAt] ON [FeeCodes_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [FeeCodes_Audit];");
        }
    }
}
