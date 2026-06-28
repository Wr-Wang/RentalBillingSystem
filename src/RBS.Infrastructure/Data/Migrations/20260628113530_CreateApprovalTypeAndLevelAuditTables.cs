using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateApprovalTypeAndLevelAuditTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE [ApprovalTypes_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NULL, [Code] nvarchar(50) NULL, [Description] nvarchar(200) NULL,
    [IsActive] bit NULL, [CompanyId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_ApprovalTypes_Audit_EntityId] ON [ApprovalTypes_Audit] ([Id]);
CREATE INDEX [IX_ApprovalTypes_Audit_ChangedAt] ON [ApprovalTypes_Audit] ([AuditChangedAt] DESC);

CREATE TABLE [ApprovalLevelConfigs_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [ApprovalTypeId] uniqueidentifier NULL, [Level] int NULL, [RoleId] uniqueidentifier NULL,
    [MinAmount] decimal(18,2) NULL, [MaxAmount] decimal(18,2) NULL, [CompanyId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_ApprovalLevelConfigs_Audit_EntityId] ON [ApprovalLevelConfigs_Audit] ([Id]);
CREATE INDEX [IX_ApprovalLevelConfigs_Audit_ChangedAt] ON [ApprovalLevelConfigs_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [ApprovalLevelConfigs_Audit];");
            migrationBuilder.Sql("DROP TABLE [ApprovalTypes_Audit];");
        }
    }
}
