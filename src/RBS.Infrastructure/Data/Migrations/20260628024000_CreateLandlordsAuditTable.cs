using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateLandlordsAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE [Landlords_Audit] (
    -- 审计字段（由 MirrorAuditInterceptor 写入）
    [Id] nvarchar(50) NOT NULL,
    [AuditAction] nvarchar(20) NOT NULL,
    [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL,
    [AuditChangedBy] uniqueidentifier NOT NULL,

    -- 公司表全字段快照
    [Name] nvarchar(200) NULL,
    [Code] nvarchar(50) NULL,
    [ContactPerson] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Address] nvarchar(500) NULL,
    [IdType] nvarchar(30) NULL,
    [IdNumber] nvarchar(50) NULL,
    [BankName] nvarchar(200) NULL,
    [BankAccount] nvarchar(50) NULL,
    [BankAccountName] nvarchar(100) NULL,
    [SettlementCycle] nvarchar(20) NULL,
    [SettlementDay] int NULL,
    [CommissionRate] decimal(5,2) NULL,
    [Remark] nvarchar(500) NULL,
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
CREATE INDEX [IX_Landlords_Audit_EntityId] ON [Landlords_Audit] ([Id]);
CREATE INDEX [IX_Landlords_Audit_ChangedAt] ON [Landlords_Audit] ([AuditChangedAt] DESC);
CREATE INDEX [IX_Landlords_Audit_Action] ON [Landlords_Audit] ([AuditAction]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [Landlords_Audit];");
        }
    }
}
