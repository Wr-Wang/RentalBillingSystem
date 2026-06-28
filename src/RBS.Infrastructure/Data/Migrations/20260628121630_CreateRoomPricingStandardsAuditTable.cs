using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateRoomPricingStandardsAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 重命名列 LandlordId -> CompanyId
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RoomPricingStandards') AND name = 'LandlordId') EXEC sp_rename 'RoomPricingStandards.LandlordId', 'CompanyId', 'COLUMN';");
            // 创建审计表
            migrationBuilder.Sql(@"
CREATE TABLE [RoomPricingStandards_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [RoomTypeId] uniqueidentifier NULL, [FloorLevelBandId] uniqueidentifier NULL,
    [RentAmount] decimal(18,2) NULL, [CompanyId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_RoomPricingStandards_Audit_EntityId] ON [RoomPricingStandards_Audit] ([Id]);
CREATE INDEX [IX_RoomPricingStandards_Audit_ChangedAt] ON [RoomPricingStandards_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [RoomPricingStandards_Audit];");
        }
    }
}
