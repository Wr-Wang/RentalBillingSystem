-- ===================================================================
-- AddAuditChangedFields.sql - 为所有 _Audit 审计表增加 AuditChangedFields 列
--
-- AuditChangedFields 存储规则：
--   INSERT: NULL（表示"新建"，前端展示所有关键字段）
--   UPDATE: 逗号分隔的变更字段名（如 "Status,Amount,StartDate"）
--   DELETE: 'DELETED'（表示"删除"，前端展示所有关键字段）
-- ===================================================================

-- Companies
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Companies_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [Companies_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- Users
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Users_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [Users_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- Roles
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Roles_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [Roles_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- Menus
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Menus_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [Menus_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- ApprovalTypes
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApprovalTypes_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [ApprovalTypes_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- ApprovalLevelConfigs
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [ApprovalLevelConfigs_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- ApprovalRequests
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApprovalRequests_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [ApprovalRequests_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- ApprovalRecords
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApprovalRecords_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [ApprovalRecords_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- HousingUnits
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[HousingUnits_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [HousingUnits_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- RoomTypes
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RoomTypes_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [RoomTypes_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- FloorLevelBands
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[FloorLevelBands_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [FloorLevelBands_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- RoomPricingStandards
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RoomPricingStandards_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [RoomPricingStandards_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- Tenants
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Tenants_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [Tenants_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- Contracts
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Contracts_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [Contracts_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- ContractFeeConfigs
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [ContractFeeConfigs_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- RenewalRequests
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RenewalRequests_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [RenewalRequests_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- FeeCodes
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[FeeCodes_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [FeeCodes_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- FeeCodeTemplates
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [FeeCodeTemplates_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- TaxRateConfigs
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[TaxRateConfigs_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [TaxRateConfigs_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- Journals
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Journals_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [Journals_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- DebitNotes
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[DebitNotes_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [DebitNotes_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- PaymentChannels
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[PaymentChannels_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [PaymentChannels_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- Receipts
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Receipts_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [Receipts_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- DepositLogs
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[DepositLogs_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [DepositLogs_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- CollectionStages
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[CollectionStages_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [CollectionStages_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- CollectionRecords
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[CollectionRecords_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [CollectionRecords_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- AccountingSubjects
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[AccountingSubjects_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [AccountingSubjects_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- BankStatements
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[BankStatements_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [BankStatements_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- JobSchedules
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[JobSchedules_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [JobSchedules_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- JobTemplates
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[JobTemplates_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [JobTemplates_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- JobScheduleExecutions
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[JobScheduleExecutions_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [JobScheduleExecutions_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- HolidayCalendars
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[HolidayCalendars_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [HolidayCalendars_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- InterestConfigs
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[InterestConfigs_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [InterestConfigs_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- ImportBatches
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ImportBatches_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [ImportBatches_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- ImportBatchItems
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ImportBatchItems_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [ImportBatchItems_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

-- UserRoles
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[UserRoles_Audit]') AND name=N'AuditChangedFields')
    ALTER TABLE [UserRoles_Audit] ADD [AuditChangedFields] NVARCHAR(MAX) NULL;

PRINT '所有 _Audit 表已增加 AuditChangedFields 列。';
GO
