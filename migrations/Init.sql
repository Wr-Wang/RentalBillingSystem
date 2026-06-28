IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AccountingSubjects] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [ParentCode] nvarchar(20) NULL,
    [Level] int NOT NULL DEFAULT 1,
    [Direction] nvarchar(10) NOT NULL DEFAULT N'Debit',
    [IsLeaf] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_AccountingSubjects] PRIMARY KEY ([Id])
);

CREATE TABLE [ApprovalLevelConfigs] (
    [Id] uniqueidentifier NOT NULL,
    [ApprovalTypeId] uniqueidentifier NOT NULL,
    [Level] int NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [MinAmount] decimal(18,2) NULL,
    [MaxAmount] decimal(18,2) NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_ApprovalLevelConfigs] PRIMARY KEY ([Id])
);

CREATE TABLE [ApprovalRequests] (
    [Id] uniqueidentifier NOT NULL,
    [ApprovalTypeId] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [TargetEntityId] uniqueidentifier NOT NULL,
    [TargetEntityType] nvarchar(50) NOT NULL,
    [CurrentLevel] int NOT NULL DEFAULT 1,
    [MaxLevel] int NOT NULL DEFAULT 1,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [LandlordId] uniqueidentifier NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_ApprovalRequests] PRIMARY KEY ([Id])
);

CREATE TABLE [ApprovalTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_ApprovalTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [BuildingFloorLevelConfigs] (
    [Id] uniqueidentifier NOT NULL,
    [BuildingId] uniqueidentifier NOT NULL,
    [FloorLevelBandId] uniqueidentifier NOT NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_BuildingFloorLevelConfigs] PRIMARY KEY ([Id])
);

CREATE TABLE [Buildings] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Code] nvarchar(50) NULL,
    [Address] nvarchar(500) NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Buildings] PRIMARY KEY ([Id])
);

CREATE TABLE [CollectionRecords] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [CollectionStageId] uniqueidentifier NOT NULL,
    [ContactResult] nvarchar(500) NULL,
    [Remark] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_CollectionRecords] PRIMARY KEY ([Id])
);

CREATE TABLE [CollectionStages] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [DaysOverdue] int NOT NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_CollectionStages] PRIMARY KEY ([Id])
);

CREATE TABLE [Contracts] (
    [Id] uniqueidentifier NOT NULL,
    [ContractNo] nvarchar(100) NOT NULL,
    [RoomId] uniqueidentifier NOT NULL,
    [RentAmount] decimal(18,2) NOT NULL,
    [DepositAmount] decimal(18,2) NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [PaymentCycle] nvarchar(20) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [TerminatedAt] datetime2 NULL,
    [TerminationReason] nvarchar(max) NULL,
    [SuspendedAt] datetime2 NULL,
    [ResumedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id])
);

CREATE TABLE [DebitNotes] (
    [Id] uniqueidentifier NOT NULL,
    [NoteNo] nvarchar(100) NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [Period] nvarchar(7) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_DebitNotes] PRIMARY KEY ([Id])
);

CREATE TABLE [DepositLogs] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Balance] decimal(18,2) NOT NULL,
    [Action] nvarchar(20) NOT NULL DEFAULT N'Create',
    [Remark] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_DepositLogs] PRIMARY KEY ([Id])
);

CREATE TABLE [FeeCodes] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [BillingMode] nvarchar(20) NOT NULL DEFAULT N'FixedAmount',
    [Unit] nvarchar(20) NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Category] nvarchar(50) NOT NULL DEFAULT N'Other',
    [IsRequired] bit NOT NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_FeeCodes] PRIMARY KEY ([Id])
);

CREATE TABLE [FeeCodeTemplates] (
    [Id] uniqueidentifier NOT NULL,
    [FeeCodeId] uniqueidentifier NOT NULL,
    [Description] nvarchar(200) NULL,
    [DefaultAmount] decimal(18,2) NOT NULL,
    [DefaultUnitPrice] decimal(18,4) NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_FeeCodeTemplates] PRIMARY KEY ([Id])
);

CREATE TABLE [FloorLevelBands] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [MinLevel] int NOT NULL,
    [MaxLevel] int NOT NULL,
    [Description] nvarchar(200) NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_FloorLevelBands] PRIMARY KEY ([Id])
);

CREATE TABLE [HolidayCalendars] (
    [Id] uniqueidentifier NOT NULL,
    [HolidayDate] date NOT NULL,
    [Name] nvarchar(100) NULL,
    [IsWorkingDay] bit NOT NULL DEFAULT CAST(0 AS bit),
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_HolidayCalendars] PRIMARY KEY ([Id])
);

CREATE TABLE [JobSchedules] (
    [Id] uniqueidentifier NOT NULL,
    [JobName] nvarchar(200) NOT NULL,
    [CronExpression] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Description] nvarchar(500) NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_JobSchedules] PRIMARY KEY ([Id])
);

CREATE TABLE [Landlords] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Code] nvarchar(50) NULL,
    [ContactPerson] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Landlords] PRIMARY KEY ([Id])
);

CREATE TABLE [LateFeeConfigs] (
    [Id] uniqueidentifier NOT NULL,
    [DailyRate] decimal(5,4) NOT NULL,
    [GraceDays] int NOT NULL DEFAULT 0,
    [MaxRate] decimal(5,2) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_LateFeeConfigs] PRIMARY KEY ([Id])
);

CREATE TABLE [Menus] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [PermissionCode] nvarchar(100) NULL,
    [Path] nvarchar(200) NULL,
    [Icon] nvarchar(50) NULL,
    [ParentId] uniqueidentifier NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Menus] PRIMARY KEY ([Id])
);

CREATE TABLE [MeterEstimationConfigs] (
    [Id] uniqueidentifier NOT NULL,
    [FeeCodeId] uniqueidentifier NOT NULL,
    [EstimatedUsage] decimal(18,4) NOT NULL,
    [Remark] nvarchar(200) NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_MeterEstimationConfigs] PRIMARY KEY ([Id])
);

CREATE TABLE [MeterReadings] (
    [Id] uniqueidentifier NOT NULL,
    [ContractFeeConfigId] uniqueidentifier NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [PreviousReading] decimal(18,4) NOT NULL,
    [CurrentReading] decimal(18,4) NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_MeterReadings] PRIMARY KEY ([Id])
);

CREATE TABLE [PaymentChannels] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_PaymentChannels] PRIMARY KEY ([Id])
);

CREATE TABLE [Receipts] (
    [Id] uniqueidentifier NOT NULL,
    [ReceiptNo] nvarchar(100) NOT NULL,
    [ContractId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NOT NULL,
    [ReceivedDate] date NOT NULL,
    [PaymentChannelId] uniqueidentifier NULL,
    [ReferenceNo] nvarchar(100) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [LandlordId] uniqueidentifier NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [RejectReason] nvarchar(max) NULL,
    [ConfirmedAt] datetime2 NULL,
    [ConfirmedBy] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Receipts] PRIMARY KEY ([Id])
);

CREATE TABLE [ReceivablePlans] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [FeeCodeId] uniqueidentifier NOT NULL,
    [Period] nvarchar(7) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Received] decimal(18,2) NOT NULL DEFAULT 0.0,
    [DueDate] date NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [RowVersion] rowversion NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_ReceivablePlans] PRIMARY KEY ([Id])
);

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);

CREATE TABLE [RoomFeeDefaults] (
    [Id] uniqueidentifier NOT NULL,
    [RoomId] uniqueidentifier NOT NULL,
    [FeeCodeId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_RoomFeeDefaults] PRIMARY KEY ([Id])
);

CREATE TABLE [RoomPricingStandards] (
    [Id] uniqueidentifier NOT NULL,
    [RoomTypeId] uniqueidentifier NOT NULL,
    [FloorLevelBandId] uniqueidentifier NOT NULL,
    [RentAmount] decimal(18,2) NOT NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_RoomPricingStandards] PRIMARY KEY ([Id])
);

CREATE TABLE [RoomTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_RoomTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [ScheduledTaskLogs] (
    [Id] uniqueidentifier NOT NULL,
    [TaskName] nvarchar(200) NOT NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [ErrorMessage] nvarchar(2000) NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_ScheduledTaskLogs] PRIMARY KEY ([Id])
);

CREATE TABLE [TaxRateConfigs] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Rate] decimal(5,2) NOT NULL,
    [EffectiveDate] date NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_TaxRateConfigs] PRIMARY KEY ([Id])
);

CREATE TABLE [Tenants] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [IdCard] nvarchar(18) NULL,
    [Phone] nvarchar(20) NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
);

CREATE TABLE [UserLandlordScopes] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_UserLandlordScopes] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(200) NOT NULL,
    [DisplayName] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NULL,
    [Email] nvarchar(200) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [HomeLandlordId] uniqueidentifier NULL,
    [IsSuperAdmin] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Vouchers] (
    [Id] uniqueidentifier NOT NULL,
    [VoucherNo] nvarchar(100) NOT NULL,
    [VoucherDate] date NOT NULL,
    [Description] nvarchar(500) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [SourceEntityId] uniqueidentifier NULL,
    [SourceEntityType] nvarchar(50) NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Vouchers] PRIMARY KEY ([Id])
);

CREATE TABLE [ApprovalRecords] (
    [Id] uniqueidentifier NOT NULL,
    [ApprovalRequestId] uniqueidentifier NOT NULL,
    [Level] int NOT NULL,
    [ApproverId] uniqueidentifier NOT NULL,
    [Action] nvarchar(20) NOT NULL,
    [Comment] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ApprovalRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApprovalRecords_ApprovalRequests_ApprovalRequestId] FOREIGN KEY ([ApprovalRequestId]) REFERENCES [ApprovalRequests] ([Id])
);

CREATE TABLE [Floors] (
    [Id] uniqueidentifier NOT NULL,
    [BuildingId] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Floors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Floors_Buildings_BuildingId] FOREIGN KEY ([BuildingId]) REFERENCES [Buildings] ([Id])
);

CREATE TABLE [ContractFeeConfigs] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [FeeCodeId] uniqueidentifier NOT NULL,
    [BillingMode] nvarchar(20) NOT NULL DEFAULT N'FixedAmount',
    [Amount] decimal(18,2) NOT NULL,
    [Unit] nvarchar(20) NULL,
    [UnitPrice] decimal(18,4) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_ContractFeeConfigs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractFeeConfigs_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id])
);

CREATE TABLE [ContractTenants] (
    [Id] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [IsPrimary] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ContractTenants] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractTenants_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id])
);

CREATE TABLE [DebitNoteItems] (
    [Id] uniqueidentifier NOT NULL,
    [DebitNoteId] uniqueidentifier NOT NULL,
    [FeeCodeId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_DebitNoteItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DebitNoteItems_DebitNotes_DebitNoteId] FOREIGN KEY ([DebitNoteId]) REFERENCES [DebitNotes] ([Id])
);

CREATE TABLE [ReceiptAllocations] (
    [Id] uniqueidentifier NOT NULL,
    [ReceiptId] uniqueidentifier NOT NULL,
    [ReceivablePlanId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ReceiptAllocations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReceiptAllocations_Receipts_ReceiptId] FOREIGN KEY ([ReceiptId]) REFERENCES [Receipts] ([Id])
);

CREATE TABLE [RoleMenus] (
    [Id] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [MenuId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_RoleMenus] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoleMenus_Menus_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menus] ([Id]),
    CONSTRAINT [FK_RoleMenus_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id])
);

CREATE TABLE [UserRoles] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]),
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [JournalEntries] (
    [Id] uniqueidentifier NOT NULL,
    [VoucherId] uniqueidentifier NOT NULL,
    [AccountingSubjectId] uniqueidentifier NOT NULL,
    [Direction] nvarchar(10) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Summary] nvarchar(200) NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JournalEntries_Vouchers_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [Vouchers] ([Id])
);

CREATE TABLE [Rooms] (
    [Id] uniqueidentifier NOT NULL,
    [BuildingId] uniqueidentifier NOT NULL,
    [FloorId] uniqueidentifier NOT NULL,
    [RoomNo] nvarchar(50) NOT NULL,
    [FullCode] nvarchar(100) NULL,
    [RoomTypeId] uniqueidentifier NULL,
    [Area] decimal(10,2) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Vacant',
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rooms_Floors_FloorId] FOREIGN KEY ([FloorId]) REFERENCES [Floors] ([Id])
);

CREATE UNIQUE INDEX [IX_AccountingSubjects_Code_LandlordId] ON [AccountingSubjects] ([Code], [LandlordId]);

CREATE INDEX [IX_AccountingSubjects_CreatedAt] ON [AccountingSubjects] ([CreatedAt]);

CREATE INDEX [IX_ApprovalLevelConfigs_CreatedAt] ON [ApprovalLevelConfigs] ([CreatedAt]);

CREATE INDEX [IX_ApprovalRecords_ApprovalRequestId] ON [ApprovalRecords] ([ApprovalRequestId]);

CREATE INDEX [IX_ApprovalRequests_CreatedAt] ON [ApprovalRequests] ([CreatedAt]);

CREATE INDEX [IX_ApprovalRequests_LandlordId_Status] ON [ApprovalRequests] ([LandlordId], [Status]);

CREATE INDEX [IX_ApprovalTypes_Code] ON [ApprovalTypes] ([Code]);

CREATE INDEX [IX_ApprovalTypes_CreatedAt] ON [ApprovalTypes] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_BuildingFloorLevelConfigs_BuildingId_FloorLevelBandId] ON [BuildingFloorLevelConfigs] ([BuildingId], [FloorLevelBandId]);

CREATE INDEX [IX_Buildings_CreatedAt] ON [Buildings] ([CreatedAt]);

CREATE INDEX [IX_Buildings_LandlordId] ON [Buildings] ([LandlordId]);

CREATE INDEX [IX_CollectionRecords_CreatedAt] ON [CollectionRecords] ([CreatedAt]);

CREATE INDEX [IX_CollectionStages_CreatedAt] ON [CollectionStages] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_ContractFeeConfigs_ContractId_FeeCodeId] ON [ContractFeeConfigs] ([ContractId], [FeeCodeId]);

CREATE INDEX [IX_ContractFeeConfigs_CreatedAt] ON [ContractFeeConfigs] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_Contracts_ContractNo] ON [Contracts] ([ContractNo]);

CREATE INDEX [IX_Contracts_CreatedAt] ON [Contracts] ([CreatedAt]);

CREATE INDEX [IX_Contracts_LandlordId] ON [Contracts] ([LandlordId]);

CREATE INDEX [IX_Contracts_LandlordId_Status] ON [Contracts] ([LandlordId], [Status]);

CREATE INDEX [IX_Contracts_Status] ON [Contracts] ([Status]);

CREATE UNIQUE INDEX [IX_ContractTenants_ContractId_TenantId] ON [ContractTenants] ([ContractId], [TenantId]);

CREATE INDEX [IX_DebitNoteItems_DebitNoteId] ON [DebitNoteItems] ([DebitNoteId]);

CREATE INDEX [IX_DebitNotes_CreatedAt] ON [DebitNotes] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_DebitNotes_NoteNo] ON [DebitNotes] ([NoteNo]);

CREATE INDEX [IX_DepositLogs_CreatedAt] ON [DepositLogs] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_FeeCodes_Code_LandlordId] ON [FeeCodes] ([Code], [LandlordId]);

CREATE INDEX [IX_FeeCodes_CreatedAt] ON [FeeCodes] ([CreatedAt]);

CREATE INDEX [IX_FeeCodeTemplates_CreatedAt] ON [FeeCodeTemplates] ([CreatedAt]);

CREATE INDEX [IX_FloorLevelBands_CreatedAt] ON [FloorLevelBands] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_Floors_BuildingId_Name] ON [Floors] ([BuildingId], [Name]);

CREATE INDEX [IX_Floors_CreatedAt] ON [Floors] ([CreatedAt]);

CREATE INDEX [IX_HolidayCalendars_CreatedAt] ON [HolidayCalendars] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_HolidayCalendars_HolidayDate_LandlordId] ON [HolidayCalendars] ([HolidayDate], [LandlordId]);

CREATE INDEX [IX_JobSchedules_CreatedAt] ON [JobSchedules] ([CreatedAt]);

CREATE INDEX [IX_JournalEntries_CreatedAt] ON [JournalEntries] ([CreatedAt]);

CREATE INDEX [IX_JournalEntries_VoucherId] ON [JournalEntries] ([VoucherId]);

CREATE UNIQUE INDEX [IX_Landlords_Code] ON [Landlords] ([Code]) WHERE [Code] IS NOT NULL;

CREATE INDEX [IX_Landlords_CreatedAt] ON [Landlords] ([CreatedAt]);

CREATE INDEX [IX_LateFeeConfigs_CreatedAt] ON [LateFeeConfigs] ([CreatedAt]);

CREATE INDEX [IX_Menus_CreatedAt] ON [Menus] ([CreatedAt]);

CREATE INDEX [IX_MeterEstimationConfigs_CreatedAt] ON [MeterEstimationConfigs] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_MeterReadings_ContractFeeConfigId_Year_Month] ON [MeterReadings] ([ContractFeeConfigId], [Year], [Month]);

CREATE INDEX [IX_MeterReadings_CreatedAt] ON [MeterReadings] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_PaymentChannels_Code_LandlordId] ON [PaymentChannels] ([Code], [LandlordId]);

CREATE INDEX [IX_PaymentChannels_CreatedAt] ON [PaymentChannels] ([CreatedAt]);

CREATE INDEX [IX_ReceiptAllocations_ReceiptId] ON [ReceiptAllocations] ([ReceiptId]);

CREATE INDEX [IX_Receipts_CreatedAt] ON [Receipts] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_Receipts_ReceiptNo] ON [Receipts] ([ReceiptNo]);

CREATE INDEX [IX_Receipts_Status_LandlordId] ON [Receipts] ([Status], [LandlordId]);

CREATE UNIQUE INDEX [IX_ReceivablePlans_Contract_Period_FeeCode] ON [ReceivablePlans] ([ContractId], [Period], [FeeCodeId]);

CREATE INDEX [IX_ReceivablePlans_CreatedAt] ON [ReceivablePlans] ([CreatedAt]);

CREATE INDEX [IX_RoleMenus_MenuId] ON [RoleMenus] ([MenuId]);

CREATE UNIQUE INDEX [IX_RoleMenus_RoleId_MenuId] ON [RoleMenus] ([RoleId], [MenuId]);

CREATE UNIQUE INDEX [IX_Roles_Code] ON [Roles] ([Code]);

CREATE INDEX [IX_Roles_CreatedAt] ON [Roles] ([CreatedAt]);

CREATE INDEX [IX_RoomFeeDefaults_CreatedAt] ON [RoomFeeDefaults] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_RoomFeeDefaults_RoomId_FeeCodeId] ON [RoomFeeDefaults] ([RoomId], [FeeCodeId]);

CREATE INDEX [IX_RoomPricingStandards_CreatedAt] ON [RoomPricingStandards] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_RoomPricingStandards_RoomTypeId_FloorLevelBandId_LandlordId] ON [RoomPricingStandards] ([RoomTypeId], [FloorLevelBandId], [LandlordId]);

CREATE INDEX [IX_Rooms_BuildingId_Status] ON [Rooms] ([BuildingId], [Status]);

CREATE INDEX [IX_Rooms_CreatedAt] ON [Rooms] ([CreatedAt]);

CREATE INDEX [IX_Rooms_FloorId] ON [Rooms] ([FloorId]);

CREATE INDEX [IX_Rooms_FullCode] ON [Rooms] ([FullCode]);

CREATE INDEX [IX_RoomTypes_CreatedAt] ON [RoomTypes] ([CreatedAt]);

CREATE INDEX [IX_ScheduledTaskLogs_CreatedAt] ON [ScheduledTaskLogs] ([CreatedAt]);

CREATE INDEX [IX_TaxRateConfigs_CreatedAt] ON [TaxRateConfigs] ([CreatedAt]);

CREATE INDEX [IX_Tenants_CreatedAt] ON [Tenants] ([CreatedAt]);

CREATE INDEX [IX_Tenants_LandlordId] ON [Tenants] ([LandlordId]);

CREATE INDEX [IX_Tenants_Phone] ON [Tenants] ([Phone]);

CREATE UNIQUE INDEX [IX_UserLandlordScopes_UserId_LandlordId] ON [UserLandlordScopes] ([UserId], [LandlordId]);

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);

CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles] ([UserId], [RoleId]);

CREATE INDEX [IX_Users_CreatedAt] ON [Users] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);

CREATE INDEX [IX_Vouchers_CreatedAt] ON [Vouchers] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_Vouchers_VoucherNo] ON [Vouchers] ([VoucherNo]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260627134111_Init', N'10.0.9');

COMMIT;
GO

