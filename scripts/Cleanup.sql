-- ===================================================================
-- Cleanup.sql - 清空所有表数据
-- ===================================================================

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Companies_Audit]'))
    DELETE FROM [Companies_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Users_Audit]'))
    DELETE FROM [Users_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Roles_Audit]'))
    DELETE FROM [Roles_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Menus_Audit]'))
    DELETE FROM [Menus_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalTypes_Audit]'))
    DELETE FROM [ApprovalTypes_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs_Audit]'))
    DELETE FROM [ApprovalLevelConfigs_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRequests_Audit]'))
    DELETE FROM [ApprovalRequests_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRecords_Audit]'))
    DELETE FROM [ApprovalRecords_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HousingUnits_Audit]'))
    DELETE FROM [HousingUnits_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomTypes_Audit]'))
    DELETE FROM [RoomTypes_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FloorLevelBands_Audit]'))
    DELETE FROM [FloorLevelBands_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomPricingStandards_Audit]'))
    DELETE FROM [RoomPricingStandards_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Tenants_Audit]'))
    DELETE FROM [Tenants_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Contracts_Audit]'))
    DELETE FROM [Contracts_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs_Audit]'))
    DELETE FROM [ContractFeeConfigs_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RenewalRequests_Audit]'))
    DELETE FROM [RenewalRequests_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodes_Audit]'))
    DELETE FROM [FeeCodes_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates_Audit]'))
    DELETE FROM [FeeCodeTemplates_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaxRateConfigs_Audit]'))
    DELETE FROM [TaxRateConfigs_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Journals_Audit]'))
    DELETE FROM [Journals_Audit]
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivablePlans_Audit]'))
    DELETE FROM [ReceivablePlans_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNotes_Audit]'))
    DELETE FROM [DebitNotes_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[PaymentChannels_Audit]'))
    DELETE FROM [PaymentChannels_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Receipts_Audit]'))
    DELETE FROM [Receipts_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DepositLogs_Audit]'))
    DELETE FROM [DepositLogs_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionStages_Audit]'))
    DELETE FROM [CollectionStages_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AccountingSubjects_Audit]'))
    DELETE FROM [AccountingSubjects_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Vouchers_Audit]'))
    DELETE FROM [Vouchers_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JournalEntries_Audit]'))
    DELETE FROM [JournalEntries_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankStatements_Audit]'))
    DELETE FROM [BankStatements_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobSchedules_Audit]'))
    DELETE FROM [JobSchedules_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobTemplates_Audit]'))
    DELETE FROM [JobTemplates_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HolidayCalendars_Audit]'))
    DELETE FROM [HolidayCalendars_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[InterestConfigs_Audit]'))
    DELETE FROM [InterestConfigs_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatches_Audit]'))
    DELETE FROM [ImportBatches_Audit]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Users]'))
    DELETE FROM [Users]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Roles]'))
    DELETE FROM [Roles]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[UserRoles]'))
    DELETE FROM [UserRoles]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Menus]'))
    DELETE FROM [Menus]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoleMenus]'))
    DELETE FROM [RoleMenus]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AuditLogs]'))
    DELETE FROM [AuditLogs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalTypes]'))
    DELETE FROM [ApprovalTypes]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs]'))
    DELETE FROM [ApprovalLevelConfigs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRequests]'))
    DELETE FROM [ApprovalRequests]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRecords]'))
    DELETE FROM [ApprovalRecords]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalBizData]'))
    DELETE FROM [ApprovalBizData]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalFeeItems]'))
    DELETE FROM [ApprovalFeeItems]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HousingUnits]'))
    DELETE FROM [HousingUnits]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomTypes]'))
    DELETE FROM [RoomTypes]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FloorLevelBands]'))
    DELETE FROM [FloorLevelBands]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomPricingStandards]'))
    DELETE FROM [RoomPricingStandards]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Tenants]'))
    DELETE FROM [Tenants]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Contracts]'))
    DELETE FROM [Contracts]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractTenants]'))
    DELETE FROM [ContractTenants]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs]'))
    DELETE FROM [ContractFeeConfigs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ChangeHistory]'))
    DELETE FROM [ChangeHistory]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RenewalRequests]'))
    DELETE FROM [RenewalRequests]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodes]'))
    DELETE FROM [FeeCodes]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates]'))
    DELETE FROM [FeeCodeTemplates]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[MeterEstimationConfigs]'))
    DELETE FROM [MeterEstimationConfigs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaxRateConfigs]'))
    DELETE FROM [TaxRateConfigs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Journals]'))
    DELETE FROM [Journals]
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[GeneralLedgerBalances]'))
    DELETE FROM [GeneralLedgerBalances]
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[PrepaidDetails]'))
    DELETE FROM [PrepaidDetails]
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivablePlans]'))
    DELETE FROM [ReceivablePlans]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNotes]'))
    DELETE FROM [DebitNotes]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNoteItems]'))
    DELETE FROM [DebitNoteItems]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AutoRenewConfigs]'))
    DELETE FROM [AutoRenewConfigs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[PaymentChannels]'))
    DELETE FROM [PaymentChannels]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Receipts]'))
    DELETE FROM [Receipts]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceiptAllocations]'))
    DELETE FROM [ReceiptAllocations]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DepositLogs]'))
    DELETE FROM [DepositLogs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionStages]'))
    DELETE FROM [CollectionStages]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionRecords]'))
    DELETE FROM [CollectionRecords]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AccountingSubjects]'))
    DELETE FROM [AccountingSubjects]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Vouchers]'))
    DELETE FROM [Vouchers]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JournalEntries]'))
    DELETE FROM [JournalEntries]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankMatches]'))
    DELETE FROM [BankMatches]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankReconciliations]'))
    DELETE FROM [BankReconciliations]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankStatements]'))
    DELETE FROM [BankStatements]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobSchedules]'))
    DELETE FROM [JobSchedules]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobScheduleExecutions]'))
    DELETE FROM [JobScheduleExecutions]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ExecutionHeartbeats]'))
    DELETE FROM [ExecutionHeartbeats]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobTemplates]'))
    DELETE FROM [JobTemplates]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApiLogs]'))
    DELETE FROM [ApiLogs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaskLogs]'))
    DELETE FROM [TaskLogs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaskStepLogs]'))
    DELETE FROM [TaskStepLogs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[SystemLogs]'))
    DELETE FROM [SystemLogs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HolidayCalendars]'))
    DELETE FROM [HolidayCalendars]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[InterestConfigs]'))
    DELETE FROM [InterestConfigs]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Notifications]'))
    DELETE FROM [Notifications]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatches]'))
    DELETE FROM [ImportBatches]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatchItems]'))
    DELETE FROM [ImportBatchItems]
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Companies]'))
    DELETE FROM [Companies]
GO

-- 共 87 张表
