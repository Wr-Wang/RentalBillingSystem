-- ===================================================================
-- Phase 1 — 数据库补表补字段
-- 执行顺序：在 Init.sql + RenewalSchema.sql 之后运行
-- 幂等：所有操作都有 IF NOT EXISTS 守卫
-- ===================================================================

PRINT N'===== Phase 1 Migration Start =====';
GO

-- ===================================================================
-- 1.1 Contracts 表补字段（加固：已存在则跳过）
-- ===================================================================
PRINT N'--- 1.1 Contracts 补字段 ---';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'PreviousContractId')
BEGIN
    ALTER TABLE [Contracts] ADD [PreviousContractId] UNIQUEIDENTIFIER NULL;
    PRINT N'  + PreviousContractId';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'RenewalCount')
BEGIN
    ALTER TABLE [Contracts] ADD [RenewalCount] INT NOT NULL DEFAULT 0;
    PRINT N'  + RenewalCount';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'OriginalContractId')
BEGIN
    ALTER TABLE [Contracts] ADD [OriginalContractId] UNIQUEIDENTIFIER NULL;
    PRINT N'  + OriginalContractId';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'MarketPriceAtRenewal')
BEGIN
    ALTER TABLE [Contracts] ADD [MarketPriceAtRenewal] DECIMAL(18,2) NULL;
    PRINT N'  + MarketPriceAtRenewal';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'AutoRenew')
BEGIN
    ALTER TABLE [Contracts] ADD [AutoRenew] BIT NOT NULL DEFAULT 1;
    PRINT N'  + AutoRenew';
END
GO

-- 续签链索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Contracts_PreviousContractId')
    EXEC('CREATE INDEX [IX_Contracts_PreviousContractId] ON [Contracts]([PreviousContractId]) WHERE [PreviousContractId] IS NOT NULL');
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Contracts_OriginalContractId')
    EXEC('CREATE INDEX [IX_Contracts_OriginalContractId] ON [Contracts]([OriginalContractId]) WHERE [OriginalContractId] IS NOT NULL');
GO

-- CompanyId：如果 Contracts 表还用的是 LandlordId，加 CompanyId 列并迁移数据
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'CompanyId')
BEGIN
    ALTER TABLE [Contracts] ADD [CompanyId] UNIQUEIDENTIFIER NULL;
    UPDATE [Contracts] SET [CompanyId] = [LandlordId] WHERE [CompanyId] IS NULL;
    ALTER TABLE [Contracts] ALTER COLUMN [CompanyId] UNIQUEIDENTIFIER NOT NULL;
    PRINT N'  + CompanyId (migrated from LandlordId)';
END
GO

-- ===================================================================
-- 1.2 新建表 — 导入批次
-- ===================================================================
PRINT N'--- 1.2 新建表 ---';

-- ImportBatches
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ImportBatches]'))
CREATE TABLE [ImportBatches] (
    [Id]                UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [CompanyId]         UNIQUEIDENTIFIER NOT NULL,
    [ImportType]        NVARCHAR(50)  NOT NULL,                      -- HousingUnit / Tenant / ...
    [FileName]          NVARCHAR(200) NOT NULL,
    [TotalRows]         INT NOT NULL DEFAULT 0,
    [ValidRows]         INT NOT NULL DEFAULT 0,
    [FailedRows]        INT NOT NULL DEFAULT 0,
    [Status]            NVARCHAR(20) NOT NULL DEFAULT 'PendingApproval',  -- PendingApproval / Approved / Rejected
    [ApprovalRequestId] UNIQUEIDENTIFIER NULL,
    [CreatedBy]         UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedAt]         DATETIME2 NULL,
);
PRINT N'  + ImportBatches';
GO

-- ImportBatchItems（包含 HousingUnit 导入行所需的全部字段）
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ImportBatchItems]'))
CREATE TABLE [ImportBatchItems] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ImportBatchId]       UNIQUEIDENTIFIER NOT NULL,
    [ImportType]          NVARCHAR(50)  NOT NULL,                    -- HousingUnit / Tenant / ...
    [RowIndex]            INT NOT NULL,
    [IsValid]             BIT NOT NULL DEFAULT 1,
    [ErrorCode]           NVARCHAR(50)  NULL,
    [ErrorMessage]        NVARCHAR(500) NULL,
    [FixSuggestion]       NVARCHAR(500) NULL,

    -- 房源导入特有字段（HousingUnit 类型使用）
    [BuildingName]        NVARCHAR(200) NULL,
    [BuildingCode]        NVARCHAR(50)  NULL,
    [BuildingAddress]     NVARCHAR(500) NULL,
    [FloorName]           NVARCHAR(100) NULL,
    [FloorSortOrder]      INT NULL,
    [UnitNo]              NVARCHAR(50)  NULL,
    [FullCode]            NVARCHAR(100) NULL,
    [RoomTypeId]          UNIQUEIDENTIFIER NULL,
    [RoomTypeName]        NVARCHAR(100) NULL,
    [Area]                DECIMAL(10,2) NULL,
    [Orientation]         NVARCHAR(20)  NULL,
    [BaseRentAmount]      DECIMAL(18,2) NULL,
    [PriceWarning]        NVARCHAR(200) NULL,

    [CreatedBy]           UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]           DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [FK_ImportBatchItems_Batch] FOREIGN KEY ([ImportBatchId]) REFERENCES [ImportBatches]([Id])
);
PRINT N'  + ImportBatchItems';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ImportBatchItems_ImportBatchId')
    CREATE INDEX [IX_ImportBatchItems_ImportBatchId] ON [ImportBatchItems]([ImportBatchId]);
GO

-- JobTemplates（任务模板字典）
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[JobTemplates]'))
CREATE TABLE [JobTemplates] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Code]                NVARCHAR(50)  NOT NULL,                    -- MonthlyFeeBill / LateFeeCalc / AutoRenew / Collection / RenewalReminder
    [DisplayName]         NVARCHAR(100) NOT NULL,
    [ShortName]           NVARCHAR(20)  NOT NULL,
    [DefaultScheduleType] NVARCHAR(20)  NOT NULL DEFAULT 'Monthly',  -- Daily / Monthly
    [DefaultHour]         INT NOT NULL DEFAULT 8,
    [DefaultMinute]       INT NOT NULL DEFAULT 0,
    [DefaultDayOfMonth]   INT NULL,
    [Description]         NVARCHAR(500) NULL,
    [Icon]                NVARCHAR(50)  NULL,
    [Category]            NVARCHAR(50)  NOT NULL DEFAULT '',
    [SortOrder]           INT NOT NULL DEFAULT 0,
    [IsActive]            BIT NOT NULL DEFAULT 1,
    [CreatedBy]           UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]           DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedBy]           UNIQUEIDENTIFIER NULL,
    [UpdatedAt]           DATETIME2 NULL,
);
PRINT N'  + JobTemplates';
GO

-- JobScheduleExecutions（排期执行实例）
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[JobScheduleExecutions]'))
CREATE TABLE [JobScheduleExecutions] (
    [Id]                UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [JobScheduleId]     UNIQUEIDENTIFIER NOT NULL,
    [CompanyId]         UNIQUEIDENTIFIER NOT NULL,
    [TargetDate]        DATETIME2 NOT NULL,
    [OriginalDate]      DATETIME2 NULL,
    [Month]             NVARCHAR(7)  NOT NULL,                       -- yyyy-MM
    [Status]            NVARCHAR(20) NOT NULL DEFAULT 'Pending',     -- Pending / Running / Completed / Failed / Cancelled
    [Reason]            NVARCHAR(500) NULL,
    [IsAdjusted]        BIT NOT NULL DEFAULT 0,
    [IsCustom]          BIT NOT NULL DEFAULT 0,
    [CreatedBy]         UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedBy]         UNIQUEIDENTIFIER NULL,
    [UpdatedAt]         DATETIME2 NULL,
    CONSTRAINT [FK_JobScheduleExecutions_Schedule] FOREIGN KEY ([JobScheduleId]) REFERENCES [JobSchedules]([Id])
);
PRINT N'  + JobScheduleExecutions';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JobScheduleExecutions_JobScheduleId')
    CREATE INDEX [IX_JobScheduleExecutions_JobScheduleId] ON [JobScheduleExecutions]([JobScheduleId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JobScheduleExecutions_Month_CompanyId')
    CREATE INDEX [IX_JobScheduleExecutions_Month_CompanyId] ON [JobScheduleExecutions]([Month], [CompanyId]);
GO

-- ===================================================================
-- 1.3 ScheduledTaskLogs 补字段
-- ===================================================================
PRINT N'--- 1.3 ScheduledTaskLogs 补字段 ---';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ScheduledTaskLogs]') AND name = 'TargetMonth')
BEGIN
    ALTER TABLE [ScheduledTaskLogs] ADD [TargetMonth] NVARCHAR(7) NULL;
    PRINT N'  + TargetMonth';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ScheduledTaskLogs]') AND name = 'HeartbeatAt')
BEGIN
    ALTER TABLE [ScheduledTaskLogs] ADD [HeartbeatAt] DATETIME2 NULL;
    PRINT N'  + HeartbeatAt';
END
GO

-- 执行互斥唯一约束：TaskName + CompanyId + TargetMonth
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduledTaskLogs_TaskName_CompanyId_TargetMonth')
    EXEC('CREATE UNIQUE INDEX [IX_ScheduledTaskLogs_TaskName_CompanyId_TargetMonth]
          ON [ScheduledTaskLogs]([TaskName], [CompanyId], [TargetMonth])
          WHERE [TargetMonth] IS NOT NULL');
GO

-- ===================================================================
-- 1.4 审计镜像表（18 张 _Audit 影子表）
-- ===================================================================
PRINT N'--- 1.4 创建审计镜像表 ---';

CREATE OR ALTER PROCEDURE [dbo].[CreateAuditTableIfNotExists]
    @TableName NVARCHAR(128)
AS
BEGIN
    DECLARE @AuditTableName NVARCHAR(128) = @TableName + N'_Audit';
    IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(@AuditTableName))
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = N'
        CREATE TABLE [dbo].' + QUOTENAME(@AuditTableName) + N' (
            [AuditId]       BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
            [AuditAction]   NVARCHAR(10)  NOT NULL,   -- INSERT / UPDATE / DELETE
            [AuditBy]       UNIQUEIDENTIFIER NULL,
            [AuditAt]       DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
            [AuditIp]       NVARCHAR(50) NULL,
            [RowVersion]    ROWVERSION NULL,
            [Id]            UNIQUEIDENTIFIER NOT NULL,
            -- 其余列由触发器按需写入（不固定列数）
        );';
        EXEC sp_executesql @Sql;
        PRINT N'  + ' + @AuditTableName;
    END
END
GO

-- 为18张跟踪实体创建 Audit 表
DECLARE @Tables NVARCHAR(128);
DECLARE @Cursor CURSOR;
SET @Cursor = CURSOR FOR
    SELECT VALUE FROM STRING_SPLIT(
        'ApprovalRequests,ApprovalTypes,ApprovalLevelConfigs,Buildings,Floors,Rooms,'
      + 'Companies,Contracts,FeeCodes,MeterReadings,ReceivablePlans,Receipts,'
      + 'Roles,Tenants,Users,Vouchers,JournalEntries,DebitNotes', ',');
OPEN @Cursor;
FETCH NEXT FROM @Cursor INTO @Tables;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC [dbo].[CreateAuditTableIfNotExists] @Tables;
    FETCH NEXT FROM @Cursor INTO @Tables;
END
CLOSE @Cursor; DEALLOCATE @Cursor;
GO

-- 清理临时存储过程
DROP PROCEDURE IF EXISTS [dbo].[CreateAuditTableIfNotExists];
GO

-- ===================================================================
-- 1.5 审批类型种子数据（如 RenewalSchema.sql 未运行）
-- ===================================================================
PRINT N'--- 1.5 审批类型种子补充 ---';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_RENEW')
BEGIN
    DECLARE @Now DATETIME2 = GETUTCDATE();
    DECLARE @SysUserId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000';
    DECLARE @Cid UNIQUEIDENTIFIER = 'A1111111-1111-1111-1111-111111111001';

    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('F1111111-1111-1111-1111-111111111007',N'合同续签','CONTRACT_RENEW',
            N'合同续签需要审批，根据月租金额自动路由审批级别',1,@Cid,@SysUserId,@Now);

    DECLARE @R_OpsSup UNIQUEIDENTIFIER = (SELECT [Id] FROM [Roles] WHERE [Code] = 'OpsSupervisor');
    DECLARE @R_DeptMgr UNIQUEIDENTIFIER = (SELECT [Id] FROM [Roles] WHERE [Code] = 'DeptManager');
    DECLARE @R_GenMgr UNIQUEIDENTIFIER = (SELECT [Id] FROM [Roles] WHERE [Code] = 'GeneralManager');

    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('F2111111-1111-1111-1111-111111111014','F1111111-1111-1111-1111-111111111007',1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('F2111111-1111-1111-1111-111111111015','F1111111-1111-1111-1111-111111111007',2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('F2111111-1111-1111-1111-111111111016','F1111111-1111-1111-1111-111111111007',3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

    PRINT N'  + CONTRACT_RENEW approval type with 3 levels';
END
GO

-- ===================================================================
-- 1.6 JobTemplates 种子数据（5 个 Job 模板）
-- ===================================================================
PRINT N'--- 1.6 JobTemplates 种子数据 ---';

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'MonthlyFeeBill')
BEGIN
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[DefaultDayOfMonth],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES
    (NEWID(),'MonthlyFeeBill', N'月账单生成',   N'月账单','Monthly',8,0,25, N'按月生成应收计划',  N'receipt_long',   N'Billing', 1,1,'00000000-0000-0000-0000-000000000000',GETUTCDATE()),
    (NEWID(),'LateFeeCalc',    N'滞纳金计算',   N'滞纳金','Daily',  2,0,NULL,N'每日计算逾期滞纳金',   N'money_off',     N'Penalty', 2,1,'00000000-0000-0000-0000-000000000000',GETUTCDATE()),
    (NEWID(),'AutoRenew',      N'自动续签',     N'续签',  'Daily',  8,0,NULL,N'检查到期合同自动续签',  N'autorenew',     N'Renewal', 3,1,'00000000-0000-0000-0000-000000000000',GETUTCDATE()),
    (NEWID(),'Collection',     N'催缴任务',     N'催缴',  'Daily',  9,0,NULL,N'按逾期阶段触发催缴',   N'contact_support',N'Collection',4,1,'00000000-0000-0000-0000-000000000000',GETUTCDATE()),
    (NEWID(),'RenewalReminder',N'续签提醒',     N'续签提醒','Daily',8,0,NULL,N'提前14天提醒合同到期', N'notifications', N'Renewal', 5,1,'00000000-0000-0000-0000-000000000000',GETUTCDATE());
    PRINT N'  + 5 JobTemplates seeded';
END
GO

PRINT N'===== Phase 1 Migration Complete =====';
GO

-- ===================================================================
-- Phase 4.3 — 银行对账表（追加在 Phase1 脚本末尾）
-- ===================================================================
PRINT N'--- 4.3 银行对账表 ---';
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[BankStatements]'))
CREATE TABLE [BankStatements] (
    [Id]              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [CompanyId]       UNIQUEIDENTIFIER NOT NULL,
    [TransactionDate] DATE NOT NULL,
    [Amount]          DECIMAL(18,2) NOT NULL,
    [Balance]         DECIMAL(18,2) NOT NULL,
    [Description]     NVARCHAR(500) NULL,
    [ReferenceNo]     NVARCHAR(100) NULL,
    [Counterparty]    NVARCHAR(200) NULL,
    [Status]          NVARCHAR(20) NOT NULL DEFAULT 'Unmatched',
    [ImportBatchId]   UNIQUEIDENTIFIER NULL,
    [CreatedBy]       UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]       DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedBy]       UNIQUEIDENTIFIER NULL,
    [UpdatedAt]       DATETIME2 NULL,
);
PRINT N'  + BankStatements';
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[BankReconciliations]'))
CREATE TABLE [BankReconciliations] (
    [Id]              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [CompanyId]       UNIQUEIDENTIFIER NOT NULL,
    [StartDate]       DATE NOT NULL,
    [EndDate]         DATE NOT NULL,
    [Status]          NVARCHAR(20) NOT NULL DEFAULT 'InProgress',
    [OpeningBalance]  DECIMAL(18,2) NOT NULL DEFAULT 0,
    [ClosingBalance]  DECIMAL(18,2) NOT NULL DEFAULT 0,
    [StatementTotal]  DECIMAL(18,2) NOT NULL DEFAULT 0,
    [SystemTotal]     DECIMAL(18,2) NOT NULL DEFAULT 0,
    [CompletedAt]     DATETIME2 NULL,
    [CreatedBy]       UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]       DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedBy]       UNIQUEIDENTIFIER NULL,
    [UpdatedAt]       DATETIME2 NULL,
);
PRINT N'  + BankReconciliations';
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[BankMatches]'))
CREATE TABLE [BankMatches] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [BankStatementId]     UNIQUEIDENTIFIER NOT NULL,
    [InternalDocumentId]  UNIQUEIDENTIFIER NOT NULL,
    [DocumentType]        NVARCHAR(20) NOT NULL DEFAULT 'Receipt',
    [MatchedAmount]       DECIMAL(18,2) NOT NULL,
    [MatchMethod]         NVARCHAR(20) NOT NULL DEFAULT 'Manual',
    [CreatedBy]           UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]           DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [FK_BankMatches_Statement] FOREIGN KEY ([BankStatementId]) REFERENCES [BankStatements]([Id])
);
PRINT N'  + BankMatches';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BankStatements_CompanyId_Date')
    CREATE INDEX [IX_BankStatements_CompanyId_Date] ON [BankStatements]([CompanyId], [TransactionDate]);
GO

PRINT N'===== Phase 4.3 Bank Reconciliation Tables Created =====';
GO

-- ===================================================================
-- Phase 4.4 — FeeCodeTemplates 补会计科目字段
-- ===================================================================
PRINT N'--- 4.4 FeeCodeTemplates 补会计科目字段 ---';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[FeeCodeTemplates]') AND name = 'DebitSubjectId')
    ALTER TABLE [FeeCodeTemplates] ADD [DebitSubjectId] UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[FeeCodeTemplates]') AND name = 'CreditSubjectId')
    ALTER TABLE [FeeCodeTemplates] ADD [CreditSubjectId] UNIQUEIDENTIFIER NULL;
GO
PRINT N'  + DebitSubjectId, CreditSubjectId added to FeeCodeTemplates';
GO

PRINT N'===== Phase 4.4 Accounting Auto-Voucher Setup Complete =====';
GO
