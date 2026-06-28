using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateAccountingSubjectsAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AccountingSubjects') AND name = 'LandlordId') EXEC sp_rename 'AccountingSubjects.LandlordId', 'CompanyId', 'COLUMN';");
            migrationBuilder.Sql(@"
CREATE TABLE [AccountingSubjects_Audit] (
    [Id] nvarchar(50) NOT NULL, [AuditAction] nvarchar(20) NOT NULL, [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL, [AuditChangedBy] uniqueidentifier NOT NULL,
    [Code] nvarchar(20) NULL, [Name] nvarchar(100) NULL, [ParentCode] nvarchar(20) NULL,
    [Level] int NULL, [Direction] nvarchar(10) NULL, [IsLeaf] bit NULL, [IsActive] bit NULL,
    [CompanyId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL, [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL, [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL, [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL, [UpdatedHostname] nvarchar(100) NULL
);
CREATE INDEX [IX_AccountingSubjects_Audit_EntityId] ON [AccountingSubjects_Audit] ([Id]);
CREATE INDEX [IX_AccountingSubjects_Audit_ChangedAt] ON [AccountingSubjects_Audit] ([AuditChangedAt] DESC);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE [AccountingSubjects_Audit];");
        }
    }
}
