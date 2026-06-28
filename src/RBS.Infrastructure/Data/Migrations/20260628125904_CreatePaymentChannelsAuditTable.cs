using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatePaymentChannelsAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentChannels') AND name = 'LandlordId') EXEC sp_rename 'PaymentChannels.LandlordId', 'CompanyId', 'COLUMN';");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('PaymentChannels') AND name = 'IX_PaymentChannels_Code_LandlordId') EXEC sp_rename 'PaymentChannels.IX_PaymentChannels_Code_LandlordId', 'IX_PaymentChannels_Code_CompanyId', 'INDEX';");
            migrationBuilder.Sql(@"
CREATE TABLE [PaymentChannels_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NULL, [Code] nvarchar(50) NULL, [IsActive] bit NULL, [CompanyId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_PaymentChannels_Audit_EntityId] ON [PaymentChannels_Audit] ([Id]);
CREATE INDEX [IX_PaymentChannels_Audit_ChangedAt] ON [PaymentChannels_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [PaymentChannels_Audit];");
        }
    }
}
