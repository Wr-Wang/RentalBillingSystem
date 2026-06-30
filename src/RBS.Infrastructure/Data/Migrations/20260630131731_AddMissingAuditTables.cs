using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <summary>
    /// 补建所有缺失的 _Audit 审计表
    /// 这些表被 MirrorAuditInterceptor 在 SaveChanges 时写入。
    /// 缺失的表会导致 ExecuteSqlRawAsync 抛出 SqlException（错误 208），
    /// 进而破坏当前事务、引发 DbUpdateConcurrencyException。
    /// </summary>
    public partial class AddMissingAuditTables : Migration
    {
        private const string AuditBaseColumns = @"
    [Id] nvarchar(50) NOT NULL,
    [AuditAction] nvarchar(20) NOT NULL,
    [AuditVersionNo] int NOT NULL DEFAULT 1,
    [AuditChangedAt] datetime2 NOT NULL,
    [AuditChangedBy] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateAuditTable(migrationBuilder, "ApprovalRequests", @"
    [ApprovalTypeId] uniqueidentifier NULL,
    [Title] nvarchar(200) NULL,
    [Description] nvarchar(1000) NULL,
    [TargetEntityId] uniqueidentifier NULL,
    [TargetEntityType] nvarchar(50) NULL,
    [CurrentLevel] int NULL,
    [MaxLevel] int NULL,
    [Status] nvarchar(20) NULL,
    [CompanyId] uniqueidentifier NULL");

            CreateAuditTable(migrationBuilder, "Contracts", @"
    [ContractNo] nvarchar(100) NULL,
    [RoomId] uniqueidentifier NULL,
    [RentAmount] decimal(18,2) NULL,
    [DepositAmount] decimal(18,2) NULL,
    [StartDate] date NULL,
    [EndDate] date NULL,
    [PaymentCycle] nvarchar(20) NULL,
    [Status] nvarchar(20) NULL,
    [CompanyId] uniqueidentifier NULL,
    [TerminatedAt] datetime2 NULL,
    [TerminationReason] nvarchar(max) NULL,
    [SuspendedAt] datetime2 NULL,
    [ResumedAt] datetime2 NULL");

            CreateAuditTable(migrationBuilder, "Tenants", @"
    [Name] nvarchar(100) NULL,
    [IdCard] nvarchar(18) NULL,
    [Phone] nvarchar(20) NULL,
    [CompanyId] uniqueidentifier NULL,
    [IsActive] bit NULL");

            CreateAuditTable(migrationBuilder, "ContractFeeConfigs", @"
    [ContractId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [BillingMode] nvarchar(20) NULL,
    [Amount] decimal(18,2) NULL,
    [Unit] nvarchar(20) NULL,
    [UnitPrice] decimal(18,4) NULL,
    [IsActive] bit NULL");

            CreateAuditTable(migrationBuilder, "DebitNotes", @"
    [NoteNo] nvarchar(100) NULL,
    [ContractId] uniqueidentifier NULL,
    [Period] nvarchar(7) NULL,
    [TotalAmount] decimal(18,2) NULL,
    [Status] nvarchar(20) NULL");

            CreateAuditTable(migrationBuilder, "ReceivablePlans", @"
    [ContractId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [Period] nvarchar(7) NULL,
    [Amount] decimal(18,2) NULL,
    [Received] decimal(18,2) NULL,
    [DueDate] date NULL,
    [Status] nvarchar(20) NULL");

            CreateAuditTable(migrationBuilder, "Receipts", @"
    [ReceiptNo] nvarchar(100) NULL,
    [ContractId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NULL,
    [ReceivedDate] date NULL,
    [PaymentChannelId] uniqueidentifier NULL,
    [ReferenceNo] nvarchar(100) NULL,
    [Status] nvarchar(20) NULL,
    [CompanyId] uniqueidentifier NULL,
    [RejectReason] nvarchar(max) NULL,
    [ConfirmedAt] datetime2 NULL,
    [ConfirmedBy] uniqueidentifier NULL");

            CreateAuditTable(migrationBuilder, "DepositLogs", @"
    [ContractId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NULL,
    [Balance] decimal(18,2) NULL,
    [Action] nvarchar(20) NULL,
    [Remark] nvarchar(500) NULL");

            CreateAuditTable(migrationBuilder, "CollectionStages", @"
    [Name] nvarchar(100) NULL,
    [DaysOverdue] int NULL,
    [SortOrder] int NULL,
    [IsActive] bit NULL,
    [CompanyId] uniqueidentifier NULL");

            CreateAuditTable(migrationBuilder, "CollectionRecords", @"
    [ContractId] uniqueidentifier NULL,
    [CollectionStageId] uniqueidentifier NULL,
    [ContactResult] nvarchar(500) NULL,
    [Remark] nvarchar(500) NULL");

            CreateAuditTable(migrationBuilder, "MeterReadings", @"
    [ContractFeeConfigId] uniqueidentifier NULL,
    [Year] int NULL,
    [Month] int NULL,
    [PreviousReading] decimal(18,4) NULL,
    [CurrentReading] decimal(18,4) NULL,
    [Status] nvarchar(20) NULL");

            CreateAuditTable(migrationBuilder, "MeterEstimationConfigs", @"
    [FeeCodeId] uniqueidentifier NULL,
    [EstimatedUsage] decimal(18,4) NULL,
    [Remark] nvarchar(200) NULL,
    [CompanyId] uniqueidentifier NULL");

            CreateAuditTable(migrationBuilder, "FeeCodeTemplates", @"
    [FeeCodeId] uniqueidentifier NULL,
    [Description] nvarchar(200) NULL,
    [DefaultAmount] decimal(18,2) NULL,
    [DefaultUnitPrice] decimal(18,4) NULL,
    [CompanyId] uniqueidentifier NULL");

            CreateAuditTable(migrationBuilder, "ImportBatches", @"
    [CompanyId] uniqueidentifier NULL,
    [ImportType] nvarchar(50) NULL,
    [FileName] nvarchar(255) NULL,
    [TotalRows] int NULL,
    [ValidRows] int NULL,
    [FailedRows] int NULL,
    [Status] nvarchar(50) NULL,
    [ApprovalRequestId] uniqueidentifier NULL");

            CreateAuditTable(migrationBuilder, "ImportBatchItems", @"
    [ImportBatchId] uniqueidentifier NULL,
    [ImportType] nvarchar(50) NULL,
    [RowIndex] int NULL,
    [IsValid] bit NULL,
    [ErrorCode] nvarchar(50) NULL,
    [ErrorMessage] nvarchar(500) NULL,
    [FixSuggestion] nvarchar(500) NULL");

            CreateAuditTable(migrationBuilder, "Vouchers", @"
    [VoucherNo] nvarchar(100) NULL,
    [VoucherDate] date NULL,
    [Description] nvarchar(500) NULL,
    [Status] nvarchar(20) NULL,
    [SourceEntityId] uniqueidentifier NULL,
    [SourceEntityType] nvarchar(50) NULL");

            CreateAuditTable(migrationBuilder, "JournalEntries", @"
    [VoucherId] uniqueidentifier NULL,
    [AccountingSubjectId] uniqueidentifier NULL,
    [Direction] nvarchar(10) NULL,
    [Amount] decimal(18,2) NULL,
    [Summary] nvarchar(200) NULL");

            CreateAuditTable(migrationBuilder, "HolidayCalendars", @"
    [HolidayDate] date NULL,
    [Name] nvarchar(100) NULL,
    [IsWorkingDay] bit NULL,
    [CompanyId] uniqueidentifier NULL");

            CreateAuditTable(migrationBuilder, "JobTemplates", @"
    [Code] nvarchar(50) NULL,
    [DisplayName] nvarchar(100) NULL,
    [ShortName] nvarchar(50) NULL,
    [DefaultScheduleType] nvarchar(10) NULL,
    [DefaultHour] int NULL,
    [DefaultMinute] int NULL,
    [DefaultDayOfMonth] int NULL,
    [Description] nvarchar(500) NULL,
    [Icon] nvarchar(50) NULL,
    [Category] nvarchar(50) NULL,
    [SortOrder] int NULL,
    [IsActive] bit NULL");
        }

        private void CreateAuditTable(MigrationBuilder builder, string tableName, string entityColumns)
        {
            var auditTable = $"{tableName}_Audit";
            builder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = '{auditTable}' AND type = 'U')
CREATE TABLE [{auditTable}] (" + AuditBaseColumns + "," + entityColumns + @"
);");

            builder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{tableName}_Audit_EntityId' AND object_id = OBJECT_ID('{auditTable}'))
    CREATE INDEX [IX_{tableName}_Audit_EntityId] ON [{auditTable}] ([Id]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_{tableName}_Audit_ChangedAt' AND object_id = OBJECT_ID('{auditTable}'))
    CREATE INDEX [IX_{tableName}_Audit_ChangedAt] ON [{auditTable}] ([AuditChangedAt] DESC);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string[] tables = [
                "ApprovalRequests", "Contracts", "Tenants", "ContractFeeConfigs",
                "DebitNotes", "ReceivablePlans", "Receipts", "DepositLogs",
                "CollectionStages", "CollectionRecords", "MeterReadings",
                "MeterEstimationConfigs", "FeeCodeTemplates", "ImportBatches",
                "ImportBatchItems", "Vouchers", "JournalEntries",
                "HolidayCalendars", "JobTemplates"
            ];

            foreach (var table in tables)
            {
                migrationBuilder.Sql($"DROP TABLE IF EXISTS [{table}_Audit];");
            }
        }
    }
}

