using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateMenusAndRolesAuditTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Menus_Audit
            migrationBuilder.Sql(@"
CREATE TABLE [Menus_Audit] (
    [Id] nvarchar(50) NOT NULL,
    [AuditAction] nvarchar(20) NOT NULL,
    [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL,
    [AuditChangedBy] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NULL,
    [PermissionCode] nvarchar(100) NULL,
    [Path] nvarchar(200) NULL,
    [Icon] nvarchar(50) NULL,
    [ParentId] uniqueidentifier NULL,
    [SortOrder] int NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_Menus_Audit_EntityId] ON [Menus_Audit] ([Id]);
CREATE INDEX [IX_Menus_Audit_ChangedAt] ON [Menus_Audit] ([AuditChangedAt] DESC);
");

            // Roles_Audit
            migrationBuilder.Sql(@"
CREATE TABLE [Roles_Audit] (
    [Id] nvarchar(50) NOT NULL,
    [AuditAction] nvarchar(20) NOT NULL,
    [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL,
    [AuditChangedBy] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NULL,
    [Code] nvarchar(50) NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_Roles_Audit_EntityId] ON [Roles_Audit] ([Id]);
CREATE INDEX [IX_Roles_Audit_ChangedAt] ON [Roles_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [Menus_Audit];");
            migrationBuilder.Sql("DROP TABLE [Roles_Audit];");
        }
    }
}
