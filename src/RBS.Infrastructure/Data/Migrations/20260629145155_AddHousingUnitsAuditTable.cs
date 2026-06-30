using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHousingUnitsAuditTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'HousingUnits_Audit' AND type = 'U')
CREATE TABLE [HousingUnits_Audit] (
    [Id] nvarchar(50) NOT NULL,
    [AuditAction] nvarchar(20) NOT NULL,
    [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL,
    [AuditChangedBy] uniqueidentifier NOT NULL,
    [BuildingName] nvarchar(200) NULL,
    [BuildingCode] nvarchar(50) NULL,
    [BuildingAddress] nvarchar(500) NULL,
    [CompanyId] uniqueidentifier NULL,
    [FloorName] nvarchar(100) NULL,
    [FloorSortOrder] int NULL,
    [UnitNo] nvarchar(50) NULL,
    [FullCode] nvarchar(100) NULL,
    [RoomTypeId] uniqueidentifier NULL,
    [Area] decimal(10,2) NULL,
    [Orientation] nvarchar(20) NULL,
    [BaseRentAmount] decimal(10,2) NULL,
    [Status] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_HousingUnits_Audit_EntityId] ON [HousingUnits_Audit] ([Id]);
CREATE INDEX [IX_HousingUnits_Audit_ChangedAt] ON [HousingUnits_Audit] ([AuditChangedAt] DESC);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS [HousingUnits_Audit];");
        }
    }
}
