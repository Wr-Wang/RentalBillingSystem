using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateFloorLevelBandsAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE [FloorLevelBands_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NULL, [MinLevel] int NULL, [MaxLevel] int NULL, [Description] nvarchar(200) NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_FloorLevelBands_Audit_EntityId] ON [FloorLevelBands_Audit] ([Id]);
CREATE INDEX [IX_FloorLevelBands_Audit_ChangedAt] ON [FloorLevelBands_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [FloorLevelBands_Audit];");
        }
    }
}
