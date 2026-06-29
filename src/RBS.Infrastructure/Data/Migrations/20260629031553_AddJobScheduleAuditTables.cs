using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobScheduleAuditTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE [JobSchedulingExecutions_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [JobScheduleId] uniqueidentifier NULL, [CompanyId] uniqueidentifier NULL,
    [TargetDate] datetime2 NULL, [OriginalDate] datetime2 NULL, [Month] nvarchar(7) NULL,
    [Status] nvarchar(20) NULL, [Reason] nvarchar(500) NULL,
    [IsAdjusted] bit NULL, [IsCustom] bit NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_JobSchedulingExecutions_Audit_EntityId] ON [JobSchedulingExecutions_Audit] ([Id]);
CREATE INDEX [IX_JobSchedulingExecutions_Audit_ChangedAt] ON [JobSchedulingExecutions_Audit] ([AuditChangedAt] DESC);
");
            migrationBuilder.Sql(@"
CREATE TABLE [JobTemplates_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NULL, [DisplayName] nvarchar(100) NULL, [ShortName] nvarchar(50) NULL,
    [DefaultCronExpression] nvarchar(100) NULL, [Description] nvarchar(500) NULL,
    [Icon] nvarchar(50) NULL, [Category] nvarchar(50) NULL,
    [SortOrder] int NULL, [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_JobTemplates_Audit_EntityId] ON [JobTemplates_Audit] ([Id]);
CREATE INDEX [IX_JobTemplates_Audit_ChangedAt] ON [JobTemplates_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS [JobSchedulingExecutions_Audit];");
            migrationBuilder.Sql("DROP TABLE IF EXISTS [JobTemplates_Audit];");
        }
    }
}
