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
DECLARE @defaultSchema AS sysname;
SET @defaultSchema = SCHEMA_NAME();
DECLARE @description AS sql_variant;
SET @description = N'科目编码';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'AccountingSubjects', 'COLUMN', N'Code';
SET @description = N'科目名称';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'AccountingSubjects', 'COLUMN', N'Name';
SET @description = N'父级科目编码';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'AccountingSubjects', 'COLUMN', N'ParentCode';
SET @description = N'科目层级';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'AccountingSubjects', 'COLUMN', N'Level';
SET @description = N'借贷方向（Debit借方/Credit贷方）';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'AccountingSubjects', 'COLUMN', N'Direction';
SET @description = N'是否末级科目';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'AccountingSubjects', 'COLUMN', N'IsLeaf';
SET @description = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'AccountingSubjects', 'COLUMN', N'IsActive';
SET @description = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'AccountingSubjects', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema1 AS sysname;
SET @defaultSchema1 = SCHEMA_NAME();
DECLARE @description1 AS sql_variant;
SET @description1 = N'审批类型ID';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'ApprovalTypeId';
SET @description1 = N'审批级别序号';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'Level';
SET @description1 = N'审批角色ID';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'RoleId';
SET @description1 = N'金额下限（满足此金额范围才需本级别审批）';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'MinAmount';
SET @description1 = N'金额上限';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'MaxAmount';
SET @description1 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema2 AS sysname;
SET @defaultSchema2 = SCHEMA_NAME();
DECLARE @description2 AS sql_variant;
SET @description2 = N'审批类型ID';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'ApprovalTypeId';
SET @description2 = N'审批标题';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'Title';
SET @description2 = N'审批申请描述';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'Description';
SET @description2 = N'目标业务实体ID';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'TargetEntityId';
SET @description2 = N'目标业务实体类型';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'TargetEntityType';
SET @description2 = N'当前审批级别';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'CurrentLevel';
SET @description2 = N'最大审批级别';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'MaxLevel';
SET @description2 = N'审批状态（Pending/Approved/Rejected/Cancelled）';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'Status';
SET @description2 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'LandlordId';
SET @description2 = N'乐观锁版本号';
EXEC sp_addextendedproperty 'MS_Description', @description2, 'SCHEMA', @defaultSchema2, 'TABLE', N'ApprovalRequests', 'COLUMN', N'RowVersion';

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
DECLARE @defaultSchema3 AS sysname;
SET @defaultSchema3 = SCHEMA_NAME();
DECLARE @description3 AS sql_variant;
SET @description3 = N'审批类型名称';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'ApprovalTypes', 'COLUMN', N'Name';
SET @description3 = N'审批类型编码';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'ApprovalTypes', 'COLUMN', N'Code';
SET @description3 = N'审批类型描述';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'ApprovalTypes', 'COLUMN', N'Description';
SET @description3 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'ApprovalTypes', 'COLUMN', N'IsActive';
SET @description3 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description3, 'SCHEMA', @defaultSchema3, 'TABLE', N'ApprovalTypes', 'COLUMN', N'LandlordId';

CREATE TABLE [BuildingFloorLevelConfigs] (
    [Id] uniqueidentifier NOT NULL,
    [BuildingId] uniqueidentifier NOT NULL,
    [FloorLevelBandId] uniqueidentifier NOT NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_BuildingFloorLevelConfigs] PRIMARY KEY ([Id])
);
DECLARE @defaultSchema4 AS sysname;
SET @defaultSchema4 = SCHEMA_NAME();
DECLARE @description4 AS sql_variant;
SET @description4 = N'楼宇ID';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'BuildingFloorLevelConfigs', 'COLUMN', N'BuildingId';
SET @description4 = N'楼层级别ID';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'BuildingFloorLevelConfigs', 'COLUMN', N'FloorLevelBandId';
SET @description4 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description4, 'SCHEMA', @defaultSchema4, 'TABLE', N'BuildingFloorLevelConfigs', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema5 AS sysname;
SET @defaultSchema5 = SCHEMA_NAME();
DECLARE @description5 AS sql_variant;
SET @description5 = N'楼宇名称';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'Buildings', 'COLUMN', N'Name';
SET @description5 = N'楼宇编码';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'Buildings', 'COLUMN', N'Code';
SET @description5 = N'楼宇地址';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'Buildings', 'COLUMN', N'Address';
SET @description5 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'Buildings', 'COLUMN', N'LandlordId';
SET @description5 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description5, 'SCHEMA', @defaultSchema5, 'TABLE', N'Buildings', 'COLUMN', N'IsActive';

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
DECLARE @defaultSchema6 AS sysname;
SET @defaultSchema6 = SCHEMA_NAME();
DECLARE @description6 AS sql_variant;
SET @description6 = N'合同ID';
EXEC sp_addextendedproperty 'MS_Description', @description6, 'SCHEMA', @defaultSchema6, 'TABLE', N'CollectionRecords', 'COLUMN', N'ContractId';
SET @description6 = N'催缴阶段ID';
EXEC sp_addextendedproperty 'MS_Description', @description6, 'SCHEMA', @defaultSchema6, 'TABLE', N'CollectionRecords', 'COLUMN', N'CollectionStageId';
SET @description6 = N'联系结果';
EXEC sp_addextendedproperty 'MS_Description', @description6, 'SCHEMA', @defaultSchema6, 'TABLE', N'CollectionRecords', 'COLUMN', N'ContactResult';
SET @description6 = N'备注';
EXEC sp_addextendedproperty 'MS_Description', @description6, 'SCHEMA', @defaultSchema6, 'TABLE', N'CollectionRecords', 'COLUMN', N'Remark';

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
DECLARE @defaultSchema7 AS sysname;
SET @defaultSchema7 = SCHEMA_NAME();
DECLARE @description7 AS sql_variant;
SET @description7 = N'催缴阶段名称';
EXEC sp_addextendedproperty 'MS_Description', @description7, 'SCHEMA', @defaultSchema7, 'TABLE', N'CollectionStages', 'COLUMN', N'Name';
SET @description7 = N'逾期天数触发条件';
EXEC sp_addextendedproperty 'MS_Description', @description7, 'SCHEMA', @defaultSchema7, 'TABLE', N'CollectionStages', 'COLUMN', N'DaysOverdue';
SET @description7 = N'排序号';
EXEC sp_addextendedproperty 'MS_Description', @description7, 'SCHEMA', @defaultSchema7, 'TABLE', N'CollectionStages', 'COLUMN', N'SortOrder';
SET @description7 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description7, 'SCHEMA', @defaultSchema7, 'TABLE', N'CollectionStages', 'COLUMN', N'IsActive';
SET @description7 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description7, 'SCHEMA', @defaultSchema7, 'TABLE', N'CollectionStages', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema8 AS sysname;
SET @defaultSchema8 = SCHEMA_NAME();
DECLARE @description8 AS sql_variant;
SET @description8 = N'合同编号，自动生成';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'ContractNo';
SET @description8 = N'租金金额';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'RentAmount';
SET @description8 = N'押金金额';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'DepositAmount';
SET @description8 = N'合同开始日期';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'StartDate';
SET @description8 = N'合同结束日期';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'EndDate';
SET @description8 = N'付款周期（Monthly/Quarterly/Yearly）';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'PaymentCycle';
SET @description8 = N'合同状态（Draft/Active/Suspended/Terminated等）';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'Status';
SET @description8 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'LandlordId';
SET @description8 = N'乐观锁版本号';
EXEC sp_addextendedproperty 'MS_Description', @description8, 'SCHEMA', @defaultSchema8, 'TABLE', N'Contracts', 'COLUMN', N'RowVersion';

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
DECLARE @defaultSchema9 AS sysname;
SET @defaultSchema9 = SCHEMA_NAME();
DECLARE @description9 AS sql_variant;
SET @description9 = N'账单编号';
EXEC sp_addextendedproperty 'MS_Description', @description9, 'SCHEMA', @defaultSchema9, 'TABLE', N'DebitNotes', 'COLUMN', N'NoteNo';
SET @description9 = N'合同ID';
EXEC sp_addextendedproperty 'MS_Description', @description9, 'SCHEMA', @defaultSchema9, 'TABLE', N'DebitNotes', 'COLUMN', N'ContractId';
SET @description9 = N'账单账期';
EXEC sp_addextendedproperty 'MS_Description', @description9, 'SCHEMA', @defaultSchema9, 'TABLE', N'DebitNotes', 'COLUMN', N'Period';
SET @description9 = N'账单总金额';
EXEC sp_addextendedproperty 'MS_Description', @description9, 'SCHEMA', @defaultSchema9, 'TABLE', N'DebitNotes', 'COLUMN', N'TotalAmount';
SET @description9 = N'状态（Draft草稿/Issued已发布）';
EXEC sp_addextendedproperty 'MS_Description', @description9, 'SCHEMA', @defaultSchema9, 'TABLE', N'DebitNotes', 'COLUMN', N'Status';

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
DECLARE @defaultSchema10 AS sysname;
SET @defaultSchema10 = SCHEMA_NAME();
DECLARE @description10 AS sql_variant;
SET @description10 = N'合同ID';
EXEC sp_addextendedproperty 'MS_Description', @description10, 'SCHEMA', @defaultSchema10, 'TABLE', N'DepositLogs', 'COLUMN', N'ContractId';
SET @description10 = N'押金变动金额';
EXEC sp_addextendedproperty 'MS_Description', @description10, 'SCHEMA', @defaultSchema10, 'TABLE', N'DepositLogs', 'COLUMN', N'Amount';
SET @description10 = N'押金余额';
EXEC sp_addextendedproperty 'MS_Description', @description10, 'SCHEMA', @defaultSchema10, 'TABLE', N'DepositLogs', 'COLUMN', N'Balance';
SET @description10 = N'操作类型（Create创建/Return退还/Deduct扣除）';
EXEC sp_addextendedproperty 'MS_Description', @description10, 'SCHEMA', @defaultSchema10, 'TABLE', N'DepositLogs', 'COLUMN', N'Action';
SET @description10 = N'备注说明';
EXEC sp_addextendedproperty 'MS_Description', @description10, 'SCHEMA', @defaultSchema10, 'TABLE', N'DepositLogs', 'COLUMN', N'Remark';

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
DECLARE @defaultSchema11 AS sysname;
SET @defaultSchema11 = SCHEMA_NAME();
DECLARE @description11 AS sql_variant;
SET @description11 = N'费用编码（如 RENT/WATER/ELECTRIC）';
EXEC sp_addextendedproperty 'MS_Description', @description11, 'SCHEMA', @defaultSchema11, 'TABLE', N'FeeCodes', 'COLUMN', N'Code';
SET @description11 = N'费用名称（如 房租费/水费/电费）';
EXEC sp_addextendedproperty 'MS_Description', @description11, 'SCHEMA', @defaultSchema11, 'TABLE', N'FeeCodes', 'COLUMN', N'Name';
SET @description11 = N'计费模式';
EXEC sp_addextendedproperty 'MS_Description', @description11, 'SCHEMA', @defaultSchema11, 'TABLE', N'FeeCodes', 'COLUMN', N'BillingMode';
SET @description11 = N'计量单位（元/吨、元/度）';
EXEC sp_addextendedproperty 'MS_Description', @description11, 'SCHEMA', @defaultSchema11, 'TABLE', N'FeeCodes', 'COLUMN', N'Unit';
SET @description11 = N'排序号';
EXEC sp_addextendedproperty 'MS_Description', @description11, 'SCHEMA', @defaultSchema11, 'TABLE', N'FeeCodes', 'COLUMN', N'SortOrder';
SET @description11 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description11, 'SCHEMA', @defaultSchema11, 'TABLE', N'FeeCodes', 'COLUMN', N'IsActive';
SET @description11 = N'费用分类（Core核心/Utility公共事业）';
EXEC sp_addextendedproperty 'MS_Description', @description11, 'SCHEMA', @defaultSchema11, 'TABLE', N'FeeCodes', 'COLUMN', N'Category';
SET @description11 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description11, 'SCHEMA', @defaultSchema11, 'TABLE', N'FeeCodes', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema12 AS sysname;
SET @defaultSchema12 = SCHEMA_NAME();
DECLARE @description12 AS sql_variant;
SET @description12 = N'费用项目ID';
EXEC sp_addextendedproperty 'MS_Description', @description12, 'SCHEMA', @defaultSchema12, 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'FeeCodeId';
SET @description12 = N'模板描述';
EXEC sp_addextendedproperty 'MS_Description', @description12, 'SCHEMA', @defaultSchema12, 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'Description';
SET @description12 = N'默认金额';
EXEC sp_addextendedproperty 'MS_Description', @description12, 'SCHEMA', @defaultSchema12, 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'DefaultAmount';
SET @description12 = N'默认单价';
EXEC sp_addextendedproperty 'MS_Description', @description12, 'SCHEMA', @defaultSchema12, 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'DefaultUnitPrice';
SET @description12 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description12, 'SCHEMA', @defaultSchema12, 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema13 AS sysname;
SET @defaultSchema13 = SCHEMA_NAME();
DECLARE @description13 AS sql_variant;
SET @description13 = N'楼层级别名称（低区/中区/高区）';
EXEC sp_addextendedproperty 'MS_Description', @description13, 'SCHEMA', @defaultSchema13, 'TABLE', N'FloorLevelBands', 'COLUMN', N'Name';
SET @description13 = N'楼层级别描述';
EXEC sp_addextendedproperty 'MS_Description', @description13, 'SCHEMA', @defaultSchema13, 'TABLE', N'FloorLevelBands', 'COLUMN', N'Description';

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
DECLARE @defaultSchema14 AS sysname;
SET @defaultSchema14 = SCHEMA_NAME();
DECLARE @description14 AS sql_variant;
SET @description14 = N'日期';
EXEC sp_addextendedproperty 'MS_Description', @description14, 'SCHEMA', @defaultSchema14, 'TABLE', N'HolidayCalendars', 'COLUMN', N'HolidayDate';
SET @description14 = N'节假日名称';
EXEC sp_addextendedproperty 'MS_Description', @description14, 'SCHEMA', @defaultSchema14, 'TABLE', N'HolidayCalendars', 'COLUMN', N'Name';
SET @description14 = N'是否工作日（false=放假/true=调休上班）';
EXEC sp_addextendedproperty 'MS_Description', @description14, 'SCHEMA', @defaultSchema14, 'TABLE', N'HolidayCalendars', 'COLUMN', N'IsWorkingDay';
SET @description14 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description14, 'SCHEMA', @defaultSchema14, 'TABLE', N'HolidayCalendars', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema15 AS sysname;
SET @defaultSchema15 = SCHEMA_NAME();
DECLARE @description15 AS sql_variant;
SET @description15 = N'作业名称';
EXEC sp_addextendedproperty 'MS_Description', @description15, 'SCHEMA', @defaultSchema15, 'TABLE', N'JobSchedules', 'COLUMN', N'JobName';
SET @description15 = N'Cron 表达式';
EXEC sp_addextendedproperty 'MS_Description', @description15, 'SCHEMA', @defaultSchema15, 'TABLE', N'JobSchedules', 'COLUMN', N'CronExpression';
SET @description15 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description15, 'SCHEMA', @defaultSchema15, 'TABLE', N'JobSchedules', 'COLUMN', N'IsActive';
SET @description15 = N'作业描述';
EXEC sp_addextendedproperty 'MS_Description', @description15, 'SCHEMA', @defaultSchema15, 'TABLE', N'JobSchedules', 'COLUMN', N'Description';
SET @description15 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description15, 'SCHEMA', @defaultSchema15, 'TABLE', N'JobSchedules', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema16 AS sysname;
SET @defaultSchema16 = SCHEMA_NAME();
DECLARE @description16 AS sql_variant;
SET @description16 = N'公司名称';
EXEC sp_addextendedproperty 'MS_Description', @description16, 'SCHEMA', @defaultSchema16, 'TABLE', N'Landlords', 'COLUMN', N'Name';
SET @description16 = N'公司编码';
EXEC sp_addextendedproperty 'MS_Description', @description16, 'SCHEMA', @defaultSchema16, 'TABLE', N'Landlords', 'COLUMN', N'Code';
SET @description16 = N'联系人';
EXEC sp_addextendedproperty 'MS_Description', @description16, 'SCHEMA', @defaultSchema16, 'TABLE', N'Landlords', 'COLUMN', N'ContactPerson';
SET @description16 = N'联系电话';
EXEC sp_addextendedproperty 'MS_Description', @description16, 'SCHEMA', @defaultSchema16, 'TABLE', N'Landlords', 'COLUMN', N'Phone';
SET @description16 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description16, 'SCHEMA', @defaultSchema16, 'TABLE', N'Landlords', 'COLUMN', N'IsActive';

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
DECLARE @defaultSchema17 AS sysname;
SET @defaultSchema17 = SCHEMA_NAME();
DECLARE @description17 AS sql_variant;
SET @description17 = N'日利率（如 0.0005 表示日息万分之五）';
EXEC sp_addextendedproperty 'MS_Description', @description17, 'SCHEMA', @defaultSchema17, 'TABLE', N'LateFeeConfigs', 'COLUMN', N'DailyRate';
SET @description17 = N'宽限天数';
EXEC sp_addextendedproperty 'MS_Description', @description17, 'SCHEMA', @defaultSchema17, 'TABLE', N'LateFeeConfigs', 'COLUMN', N'GraceDays';
SET @description17 = N'滞纳金上限（百分比）';
EXEC sp_addextendedproperty 'MS_Description', @description17, 'SCHEMA', @defaultSchema17, 'TABLE', N'LateFeeConfigs', 'COLUMN', N'MaxRate';
SET @description17 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description17, 'SCHEMA', @defaultSchema17, 'TABLE', N'LateFeeConfigs', 'COLUMN', N'IsActive';
SET @description17 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description17, 'SCHEMA', @defaultSchema17, 'TABLE', N'LateFeeConfigs', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema18 AS sysname;
SET @defaultSchema18 = SCHEMA_NAME();
DECLARE @description18 AS sql_variant;
SET @description18 = N'菜单名称';
EXEC sp_addextendedproperty 'MS_Description', @description18, 'SCHEMA', @defaultSchema18, 'TABLE', N'Menus', 'COLUMN', N'Name';
SET @description18 = N'权限代码，用于接口鉴权';
EXEC sp_addextendedproperty 'MS_Description', @description18, 'SCHEMA', @defaultSchema18, 'TABLE', N'Menus', 'COLUMN', N'PermissionCode';
SET @description18 = N'前端路由路径';
EXEC sp_addextendedproperty 'MS_Description', @description18, 'SCHEMA', @defaultSchema18, 'TABLE', N'Menus', 'COLUMN', N'Path';
SET @description18 = N'菜单图标类名';
EXEC sp_addextendedproperty 'MS_Description', @description18, 'SCHEMA', @defaultSchema18, 'TABLE', N'Menus', 'COLUMN', N'Icon';
SET @description18 = N'排序号';
EXEC sp_addextendedproperty 'MS_Description', @description18, 'SCHEMA', @defaultSchema18, 'TABLE', N'Menus', 'COLUMN', N'SortOrder';
SET @description18 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description18, 'SCHEMA', @defaultSchema18, 'TABLE', N'Menus', 'COLUMN', N'IsActive';

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
DECLARE @defaultSchema19 AS sysname;
SET @defaultSchema19 = SCHEMA_NAME();
DECLARE @description19 AS sql_variant;
SET @description19 = N'费用项目ID';
EXEC sp_addextendedproperty 'MS_Description', @description19, 'SCHEMA', @defaultSchema19, 'TABLE', N'MeterEstimationConfigs', 'COLUMN', N'FeeCodeId';
SET @description19 = N'预估用量';
EXEC sp_addextendedproperty 'MS_Description', @description19, 'SCHEMA', @defaultSchema19, 'TABLE', N'MeterEstimationConfigs', 'COLUMN', N'EstimatedUsage';
SET @description19 = N'备注说明';
EXEC sp_addextendedproperty 'MS_Description', @description19, 'SCHEMA', @defaultSchema19, 'TABLE', N'MeterEstimationConfigs', 'COLUMN', N'Remark';
SET @description19 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description19, 'SCHEMA', @defaultSchema19, 'TABLE', N'MeterEstimationConfigs', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema20 AS sysname;
SET @defaultSchema20 = SCHEMA_NAME();
DECLARE @description20 AS sql_variant;
SET @description20 = N'合同费用配置ID';
EXEC sp_addextendedproperty 'MS_Description', @description20, 'SCHEMA', @defaultSchema20, 'TABLE', N'MeterReadings', 'COLUMN', N'ContractFeeConfigId';
SET @description20 = N'抄表年份';
EXEC sp_addextendedproperty 'MS_Description', @description20, 'SCHEMA', @defaultSchema20, 'TABLE', N'MeterReadings', 'COLUMN', N'Year';
SET @description20 = N'抄表月份';
EXEC sp_addextendedproperty 'MS_Description', @description20, 'SCHEMA', @defaultSchema20, 'TABLE', N'MeterReadings', 'COLUMN', N'Month';
SET @description20 = N'上次读数';
EXEC sp_addextendedproperty 'MS_Description', @description20, 'SCHEMA', @defaultSchema20, 'TABLE', N'MeterReadings', 'COLUMN', N'PreviousReading';
SET @description20 = N'本次读数';
EXEC sp_addextendedproperty 'MS_Description', @description20, 'SCHEMA', @defaultSchema20, 'TABLE', N'MeterReadings', 'COLUMN', N'CurrentReading';
SET @description20 = N'状态（Draft草稿/Confirmed已确认）';
EXEC sp_addextendedproperty 'MS_Description', @description20, 'SCHEMA', @defaultSchema20, 'TABLE', N'MeterReadings', 'COLUMN', N'Status';

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
DECLARE @defaultSchema21 AS sysname;
SET @defaultSchema21 = SCHEMA_NAME();
DECLARE @description21 AS sql_variant;
SET @description21 = N'支付通道名称';
EXEC sp_addextendedproperty 'MS_Description', @description21, 'SCHEMA', @defaultSchema21, 'TABLE', N'PaymentChannels', 'COLUMN', N'Name';
SET @description21 = N'支付通道编码';
EXEC sp_addextendedproperty 'MS_Description', @description21, 'SCHEMA', @defaultSchema21, 'TABLE', N'PaymentChannels', 'COLUMN', N'Code';
SET @description21 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description21, 'SCHEMA', @defaultSchema21, 'TABLE', N'PaymentChannels', 'COLUMN', N'IsActive';
SET @description21 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description21, 'SCHEMA', @defaultSchema21, 'TABLE', N'PaymentChannels', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema22 AS sysname;
SET @defaultSchema22 = SCHEMA_NAME();
DECLARE @description22 AS sql_variant;
SET @description22 = N'收款单号';
EXEC sp_addextendedproperty 'MS_Description', @description22, 'SCHEMA', @defaultSchema22, 'TABLE', N'Receipts', 'COLUMN', N'ReceiptNo';
SET @description22 = N'收款金额';
EXEC sp_addextendedproperty 'MS_Description', @description22, 'SCHEMA', @defaultSchema22, 'TABLE', N'Receipts', 'COLUMN', N'Amount';
SET @description22 = N'收款日期';
EXEC sp_addextendedproperty 'MS_Description', @description22, 'SCHEMA', @defaultSchema22, 'TABLE', N'Receipts', 'COLUMN', N'ReceivedDate';
SET @description22 = N'外部参考号（银行流水号等）';
EXEC sp_addextendedproperty 'MS_Description', @description22, 'SCHEMA', @defaultSchema22, 'TABLE', N'Receipts', 'COLUMN', N'ReferenceNo';
SET @description22 = N'状态（Pending待确认/Confirmed已确认/Rejected已驳回）';
EXEC sp_addextendedproperty 'MS_Description', @description22, 'SCHEMA', @defaultSchema22, 'TABLE', N'Receipts', 'COLUMN', N'Status';
SET @description22 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description22, 'SCHEMA', @defaultSchema22, 'TABLE', N'Receipts', 'COLUMN', N'LandlordId';
SET @description22 = N'乐观锁版本号';
EXEC sp_addextendedproperty 'MS_Description', @description22, 'SCHEMA', @defaultSchema22, 'TABLE', N'Receipts', 'COLUMN', N'RowVersion';

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
DECLARE @defaultSchema23 AS sysname;
SET @defaultSchema23 = SCHEMA_NAME();
DECLARE @description23 AS sql_variant;
SET @description23 = N'合同ID';
EXEC sp_addextendedproperty 'MS_Description', @description23, 'SCHEMA', @defaultSchema23, 'TABLE', N'ReceivablePlans', 'COLUMN', N'ContractId';
SET @description23 = N'费用项目ID';
EXEC sp_addextendedproperty 'MS_Description', @description23, 'SCHEMA', @defaultSchema23, 'TABLE', N'ReceivablePlans', 'COLUMN', N'FeeCodeId';
SET @description23 = N'账期（如 2026-06）';
EXEC sp_addextendedproperty 'MS_Description', @description23, 'SCHEMA', @defaultSchema23, 'TABLE', N'ReceivablePlans', 'COLUMN', N'Period';
SET @description23 = N'应收金额';
EXEC sp_addextendedproperty 'MS_Description', @description23, 'SCHEMA', @defaultSchema23, 'TABLE', N'ReceivablePlans', 'COLUMN', N'Amount';
SET @description23 = N'已收金额';
EXEC sp_addextendedproperty 'MS_Description', @description23, 'SCHEMA', @defaultSchema23, 'TABLE', N'ReceivablePlans', 'COLUMN', N'Received';
SET @description23 = N'到期日';
EXEC sp_addextendedproperty 'MS_Description', @description23, 'SCHEMA', @defaultSchema23, 'TABLE', N'ReceivablePlans', 'COLUMN', N'DueDate';
SET @description23 = N'状态（Pending/Partial/Paid/Overdue）';
EXEC sp_addextendedproperty 'MS_Description', @description23, 'SCHEMA', @defaultSchema23, 'TABLE', N'ReceivablePlans', 'COLUMN', N'Status';
SET @description23 = N'乐观锁版本号';
EXEC sp_addextendedproperty 'MS_Description', @description23, 'SCHEMA', @defaultSchema23, 'TABLE', N'ReceivablePlans', 'COLUMN', N'RowVersion';

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
DECLARE @defaultSchema24 AS sysname;
SET @defaultSchema24 = SCHEMA_NAME();
DECLARE @description24 AS sql_variant;
SET @description24 = N'角色名称';
EXEC sp_addextendedproperty 'MS_Description', @description24, 'SCHEMA', @defaultSchema24, 'TABLE', N'Roles', 'COLUMN', N'Name';
SET @description24 = N'角色编码（如 Admin/OpsSupervisor）';
EXEC sp_addextendedproperty 'MS_Description', @description24, 'SCHEMA', @defaultSchema24, 'TABLE', N'Roles', 'COLUMN', N'Code';
SET @description24 = N'角色描述';
EXEC sp_addextendedproperty 'MS_Description', @description24, 'SCHEMA', @defaultSchema24, 'TABLE', N'Roles', 'COLUMN', N'Description';
SET @description24 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description24, 'SCHEMA', @defaultSchema24, 'TABLE', N'Roles', 'COLUMN', N'IsActive';

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
DECLARE @defaultSchema25 AS sysname;
SET @defaultSchema25 = SCHEMA_NAME();
DECLARE @description25 AS sql_variant;
SET @description25 = N'房间ID';
EXEC sp_addextendedproperty 'MS_Description', @description25, 'SCHEMA', @defaultSchema25, 'TABLE', N'RoomFeeDefaults', 'COLUMN', N'RoomId';
SET @description25 = N'费用项目ID';
EXEC sp_addextendedproperty 'MS_Description', @description25, 'SCHEMA', @defaultSchema25, 'TABLE', N'RoomFeeDefaults', 'COLUMN', N'FeeCodeId';
SET @description25 = N'默认金额';
EXEC sp_addextendedproperty 'MS_Description', @description25, 'SCHEMA', @defaultSchema25, 'TABLE', N'RoomFeeDefaults', 'COLUMN', N'Amount';
SET @description25 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description25, 'SCHEMA', @defaultSchema25, 'TABLE', N'RoomFeeDefaults', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema26 AS sysname;
SET @defaultSchema26 = SCHEMA_NAME();
DECLARE @description26 AS sql_variant;
SET @description26 = N'房型ID';
EXEC sp_addextendedproperty 'MS_Description', @description26, 'SCHEMA', @defaultSchema26, 'TABLE', N'RoomPricingStandards', 'COLUMN', N'RoomTypeId';
SET @description26 = N'楼层级别ID';
EXEC sp_addextendedproperty 'MS_Description', @description26, 'SCHEMA', @defaultSchema26, 'TABLE', N'RoomPricingStandards', 'COLUMN', N'FloorLevelBandId';
SET @description26 = N'标准租金';
EXEC sp_addextendedproperty 'MS_Description', @description26, 'SCHEMA', @defaultSchema26, 'TABLE', N'RoomPricingStandards', 'COLUMN', N'RentAmount';
SET @description26 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description26, 'SCHEMA', @defaultSchema26, 'TABLE', N'RoomPricingStandards', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema27 AS sysname;
SET @defaultSchema27 = SCHEMA_NAME();
DECLARE @description27 AS sql_variant;
SET @description27 = N'房型名称（整租/合租等）';
EXEC sp_addextendedproperty 'MS_Description', @description27, 'SCHEMA', @defaultSchema27, 'TABLE', N'RoomTypes', 'COLUMN', N'Name';
SET @description27 = N'房型描述';
EXEC sp_addextendedproperty 'MS_Description', @description27, 'SCHEMA', @defaultSchema27, 'TABLE', N'RoomTypes', 'COLUMN', N'Description';
SET @description27 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description27, 'SCHEMA', @defaultSchema27, 'TABLE', N'RoomTypes', 'COLUMN', N'IsActive';

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
DECLARE @defaultSchema28 AS sysname;
SET @defaultSchema28 = SCHEMA_NAME();
DECLARE @description28 AS sql_variant;
SET @description28 = N'任务名称';
EXEC sp_addextendedproperty 'MS_Description', @description28, 'SCHEMA', @defaultSchema28, 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'TaskName';
SET @description28 = N'执行状态（Pending/Running/Completed/Failed）';
EXEC sp_addextendedproperty 'MS_Description', @description28, 'SCHEMA', @defaultSchema28, 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'Status';
SET @description28 = N'错误信息';
EXEC sp_addextendedproperty 'MS_Description', @description28, 'SCHEMA', @defaultSchema28, 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'ErrorMessage';
SET @description28 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description28, 'SCHEMA', @defaultSchema28, 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema29 AS sysname;
SET @defaultSchema29 = SCHEMA_NAME();
DECLARE @description29 AS sql_variant;
SET @description29 = N'税率名称';
EXEC sp_addextendedproperty 'MS_Description', @description29, 'SCHEMA', @defaultSchema29, 'TABLE', N'TaxRateConfigs', 'COLUMN', N'Name';
SET @description29 = N'税率（百分比）';
EXEC sp_addextendedproperty 'MS_Description', @description29, 'SCHEMA', @defaultSchema29, 'TABLE', N'TaxRateConfigs', 'COLUMN', N'Rate';
SET @description29 = N'生效日期';
EXEC sp_addextendedproperty 'MS_Description', @description29, 'SCHEMA', @defaultSchema29, 'TABLE', N'TaxRateConfigs', 'COLUMN', N'EffectiveDate';
SET @description29 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description29, 'SCHEMA', @defaultSchema29, 'TABLE', N'TaxRateConfigs', 'COLUMN', N'IsActive';
SET @description29 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description29, 'SCHEMA', @defaultSchema29, 'TABLE', N'TaxRateConfigs', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema30 AS sysname;
SET @defaultSchema30 = SCHEMA_NAME();
DECLARE @description30 AS sql_variant;
SET @description30 = N'租客姓名';
EXEC sp_addextendedproperty 'MS_Description', @description30, 'SCHEMA', @defaultSchema30, 'TABLE', N'Tenants', 'COLUMN', N'Name';
SET @description30 = N'身份证号';
EXEC sp_addextendedproperty 'MS_Description', @description30, 'SCHEMA', @defaultSchema30, 'TABLE', N'Tenants', 'COLUMN', N'IdCard';
SET @description30 = N'联系电话';
EXEC sp_addextendedproperty 'MS_Description', @description30, 'SCHEMA', @defaultSchema30, 'TABLE', N'Tenants', 'COLUMN', N'Phone';
SET @description30 = N'所属公司ID';
EXEC sp_addextendedproperty 'MS_Description', @description30, 'SCHEMA', @defaultSchema30, 'TABLE', N'Tenants', 'COLUMN', N'LandlordId';
SET @description30 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description30, 'SCHEMA', @defaultSchema30, 'TABLE', N'Tenants', 'COLUMN', N'IsActive';

CREATE TABLE [UserLandlordScopes] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [LandlordId] uniqueidentifier NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_UserLandlordScopes] PRIMARY KEY ([Id])
);
DECLARE @defaultSchema31 AS sysname;
SET @defaultSchema31 = SCHEMA_NAME();
DECLARE @description31 AS sql_variant;
SET @description31 = N'用户ID';
EXEC sp_addextendedproperty 'MS_Description', @description31, 'SCHEMA', @defaultSchema31, 'TABLE', N'UserLandlordScopes', 'COLUMN', N'UserId';
SET @description31 = N'公司ID，用户可操作的数据范围';
EXEC sp_addextendedproperty 'MS_Description', @description31, 'SCHEMA', @defaultSchema31, 'TABLE', N'UserLandlordScopes', 'COLUMN', N'LandlordId';

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
DECLARE @defaultSchema32 AS sysname;
SET @defaultSchema32 = SCHEMA_NAME();
DECLARE @description32 AS sql_variant;
SET @description32 = N'登录用户名，全局唯一';
EXEC sp_addextendedproperty 'MS_Description', @description32, 'SCHEMA', @defaultSchema32, 'TABLE', N'Users', 'COLUMN', N'Username';
SET @description32 = N'密码哈希值';
EXEC sp_addextendedproperty 'MS_Description', @description32, 'SCHEMA', @defaultSchema32, 'TABLE', N'Users', 'COLUMN', N'PasswordHash';
SET @description32 = N'用户显示名称/姓名';
EXEC sp_addextendedproperty 'MS_Description', @description32, 'SCHEMA', @defaultSchema32, 'TABLE', N'Users', 'COLUMN', N'DisplayName';
SET @description32 = N'手机号';
EXEC sp_addextendedproperty 'MS_Description', @description32, 'SCHEMA', @defaultSchema32, 'TABLE', N'Users', 'COLUMN', N'Phone';
SET @description32 = N'电子邮箱';
EXEC sp_addextendedproperty 'MS_Description', @description32, 'SCHEMA', @defaultSchema32, 'TABLE', N'Users', 'COLUMN', N'Email';
SET @description32 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description32, 'SCHEMA', @defaultSchema32, 'TABLE', N'Users', 'COLUMN', N'IsActive';
SET @description32 = N'是否为超级管理员';
EXEC sp_addextendedproperty 'MS_Description', @description32, 'SCHEMA', @defaultSchema32, 'TABLE', N'Users', 'COLUMN', N'IsSuperAdmin';

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
DECLARE @defaultSchema33 AS sysname;
SET @defaultSchema33 = SCHEMA_NAME();
DECLARE @description33 AS sql_variant;
SET @description33 = N'凭证编号';
EXEC sp_addextendedproperty 'MS_Description', @description33, 'SCHEMA', @defaultSchema33, 'TABLE', N'Vouchers', 'COLUMN', N'VoucherNo';
SET @description33 = N'凭证日期';
EXEC sp_addextendedproperty 'MS_Description', @description33, 'SCHEMA', @defaultSchema33, 'TABLE', N'Vouchers', 'COLUMN', N'VoucherDate';
SET @description33 = N'凭证摘要';
EXEC sp_addextendedproperty 'MS_Description', @description33, 'SCHEMA', @defaultSchema33, 'TABLE', N'Vouchers', 'COLUMN', N'Description';
SET @description33 = N'凭证状态（Draft草稿/Posted已过账/Audited已审核）';
EXEC sp_addextendedproperty 'MS_Description', @description33, 'SCHEMA', @defaultSchema33, 'TABLE', N'Vouchers', 'COLUMN', N'Status';
SET @description33 = N'来源业务实体类型';
EXEC sp_addextendedproperty 'MS_Description', @description33, 'SCHEMA', @defaultSchema33, 'TABLE', N'Vouchers', 'COLUMN', N'SourceEntityType';
SET @description33 = N'乐观锁版本号';
EXEC sp_addextendedproperty 'MS_Description', @description33, 'SCHEMA', @defaultSchema33, 'TABLE', N'Vouchers', 'COLUMN', N'RowVersion';

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
DECLARE @defaultSchema34 AS sysname;
SET @defaultSchema34 = SCHEMA_NAME();
DECLARE @description34 AS sql_variant;
SET @description34 = N'审批请求ID';
EXEC sp_addextendedproperty 'MS_Description', @description34, 'SCHEMA', @defaultSchema34, 'TABLE', N'ApprovalRecords', 'COLUMN', N'ApprovalRequestId';
SET @description34 = N'审批级别';
EXEC sp_addextendedproperty 'MS_Description', @description34, 'SCHEMA', @defaultSchema34, 'TABLE', N'ApprovalRecords', 'COLUMN', N'Level';
SET @description34 = N'审批人ID';
EXEC sp_addextendedproperty 'MS_Description', @description34, 'SCHEMA', @defaultSchema34, 'TABLE', N'ApprovalRecords', 'COLUMN', N'ApproverId';
SET @description34 = N'审批动作（Approved通过/Rejected驳回）';
EXEC sp_addextendedproperty 'MS_Description', @description34, 'SCHEMA', @defaultSchema34, 'TABLE', N'ApprovalRecords', 'COLUMN', N'Action';
SET @description34 = N'审批意见';
EXEC sp_addextendedproperty 'MS_Description', @description34, 'SCHEMA', @defaultSchema34, 'TABLE', N'ApprovalRecords', 'COLUMN', N'Comment';

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
DECLARE @defaultSchema35 AS sysname;
SET @defaultSchema35 = SCHEMA_NAME();
DECLARE @description35 AS sql_variant;
SET @description35 = N'所属楼宇ID';
EXEC sp_addextendedproperty 'MS_Description', @description35, 'SCHEMA', @defaultSchema35, 'TABLE', N'Floors', 'COLUMN', N'BuildingId';
SET @description35 = N'楼层名称（如 1层、2层）';
EXEC sp_addextendedproperty 'MS_Description', @description35, 'SCHEMA', @defaultSchema35, 'TABLE', N'Floors', 'COLUMN', N'Name';
SET @description35 = N'楼层排序号';
EXEC sp_addextendedproperty 'MS_Description', @description35, 'SCHEMA', @defaultSchema35, 'TABLE', N'Floors', 'COLUMN', N'SortOrder';

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
DECLARE @defaultSchema36 AS sysname;
SET @defaultSchema36 = SCHEMA_NAME();
DECLARE @description36 AS sql_variant;
SET @description36 = N'合同ID';
EXEC sp_addextendedproperty 'MS_Description', @description36, 'SCHEMA', @defaultSchema36, 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'ContractId';
SET @description36 = N'费用项目ID';
EXEC sp_addextendedproperty 'MS_Description', @description36, 'SCHEMA', @defaultSchema36, 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'FeeCodeId';
SET @description36 = N'计费模式（FixedAmount固定金额/MeterBased抄表）';
EXEC sp_addextendedproperty 'MS_Description', @description36, 'SCHEMA', @defaultSchema36, 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'BillingMode';
SET @description36 = N'金额';
EXEC sp_addextendedproperty 'MS_Description', @description36, 'SCHEMA', @defaultSchema36, 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'Amount';
SET @description36 = N'计量单位';
EXEC sp_addextendedproperty 'MS_Description', @description36, 'SCHEMA', @defaultSchema36, 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'Unit';
SET @description36 = N'单价';
EXEC sp_addextendedproperty 'MS_Description', @description36, 'SCHEMA', @defaultSchema36, 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'UnitPrice';
SET @description36 = N'是否启用';
EXEC sp_addextendedproperty 'MS_Description', @description36, 'SCHEMA', @defaultSchema36, 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'IsActive';

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
DECLARE @defaultSchema37 AS sysname;
SET @defaultSchema37 = SCHEMA_NAME();
DECLARE @description37 AS sql_variant;
SET @description37 = N'合同ID';
EXEC sp_addextendedproperty 'MS_Description', @description37, 'SCHEMA', @defaultSchema37, 'TABLE', N'ContractTenants', 'COLUMN', N'ContractId';
SET @description37 = N'租客ID';
EXEC sp_addextendedproperty 'MS_Description', @description37, 'SCHEMA', @defaultSchema37, 'TABLE', N'ContractTenants', 'COLUMN', N'TenantId';
SET @description37 = N'是否主租客';
EXEC sp_addextendedproperty 'MS_Description', @description37, 'SCHEMA', @defaultSchema37, 'TABLE', N'ContractTenants', 'COLUMN', N'IsPrimary';

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
DECLARE @defaultSchema38 AS sysname;
SET @defaultSchema38 = SCHEMA_NAME();
DECLARE @description38 AS sql_variant;
SET @description38 = N'账单ID';
EXEC sp_addextendedproperty 'MS_Description', @description38, 'SCHEMA', @defaultSchema38, 'TABLE', N'DebitNoteItems', 'COLUMN', N'DebitNoteId';
SET @description38 = N'费用项目ID';
EXEC sp_addextendedproperty 'MS_Description', @description38, 'SCHEMA', @defaultSchema38, 'TABLE', N'DebitNoteItems', 'COLUMN', N'FeeCodeId';
SET @description38 = N'费用金额';
EXEC sp_addextendedproperty 'MS_Description', @description38, 'SCHEMA', @defaultSchema38, 'TABLE', N'DebitNoteItems', 'COLUMN', N'Amount';

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
DECLARE @defaultSchema39 AS sysname;
SET @defaultSchema39 = SCHEMA_NAME();
DECLARE @description39 AS sql_variant;
SET @description39 = N'收款单ID';
EXEC sp_addextendedproperty 'MS_Description', @description39, 'SCHEMA', @defaultSchema39, 'TABLE', N'ReceiptAllocations', 'COLUMN', N'ReceiptId';
SET @description39 = N'应收计划ID';
EXEC sp_addextendedproperty 'MS_Description', @description39, 'SCHEMA', @defaultSchema39, 'TABLE', N'ReceiptAllocations', 'COLUMN', N'ReceivablePlanId';
SET @description39 = N'分配金额';
EXEC sp_addextendedproperty 'MS_Description', @description39, 'SCHEMA', @defaultSchema39, 'TABLE', N'ReceiptAllocations', 'COLUMN', N'Amount';

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
DECLARE @defaultSchema40 AS sysname;
SET @defaultSchema40 = SCHEMA_NAME();
DECLARE @description40 AS sql_variant;
SET @description40 = N'角色ID';
EXEC sp_addextendedproperty 'MS_Description', @description40, 'SCHEMA', @defaultSchema40, 'TABLE', N'RoleMenus', 'COLUMN', N'RoleId';
SET @description40 = N'菜单ID';
EXEC sp_addextendedproperty 'MS_Description', @description40, 'SCHEMA', @defaultSchema40, 'TABLE', N'RoleMenus', 'COLUMN', N'MenuId';

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
DECLARE @defaultSchema41 AS sysname;
SET @defaultSchema41 = SCHEMA_NAME();
DECLARE @description41 AS sql_variant;
SET @description41 = N'用户ID';
EXEC sp_addextendedproperty 'MS_Description', @description41, 'SCHEMA', @defaultSchema41, 'TABLE', N'UserRoles', 'COLUMN', N'UserId';
SET @description41 = N'角色ID';
EXEC sp_addextendedproperty 'MS_Description', @description41, 'SCHEMA', @defaultSchema41, 'TABLE', N'UserRoles', 'COLUMN', N'RoleId';

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
DECLARE @defaultSchema42 AS sysname;
SET @defaultSchema42 = SCHEMA_NAME();
DECLARE @description42 AS sql_variant;
SET @description42 = N'凭证ID';
EXEC sp_addextendedproperty 'MS_Description', @description42, 'SCHEMA', @defaultSchema42, 'TABLE', N'JournalEntries', 'COLUMN', N'VoucherId';
SET @description42 = N'会计科目ID';
EXEC sp_addextendedproperty 'MS_Description', @description42, 'SCHEMA', @defaultSchema42, 'TABLE', N'JournalEntries', 'COLUMN', N'AccountingSubjectId';
SET @description42 = N'借贷方向（Debit/Credit）';
EXEC sp_addextendedproperty 'MS_Description', @description42, 'SCHEMA', @defaultSchema42, 'TABLE', N'JournalEntries', 'COLUMN', N'Direction';
SET @description42 = N'金额';
EXEC sp_addextendedproperty 'MS_Description', @description42, 'SCHEMA', @defaultSchema42, 'TABLE', N'JournalEntries', 'COLUMN', N'Amount';
SET @description42 = N'分录摘要说明';
EXEC sp_addextendedproperty 'MS_Description', @description42, 'SCHEMA', @defaultSchema42, 'TABLE', N'JournalEntries', 'COLUMN', N'Summary';

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
DECLARE @defaultSchema43 AS sysname;
SET @defaultSchema43 = SCHEMA_NAME();
DECLARE @description43 AS sql_variant;
SET @description43 = N'所属楼宇ID';
EXEC sp_addextendedproperty 'MS_Description', @description43, 'SCHEMA', @defaultSchema43, 'TABLE', N'Rooms', 'COLUMN', N'BuildingId';
SET @description43 = N'所属楼层ID';
EXEC sp_addextendedproperty 'MS_Description', @description43, 'SCHEMA', @defaultSchema43, 'TABLE', N'Rooms', 'COLUMN', N'FloorId';
SET @description43 = N'房间编号（如 101）';
EXEC sp_addextendedproperty 'MS_Description', @description43, 'SCHEMA', @defaultSchema43, 'TABLE', N'Rooms', 'COLUMN', N'RoomNo';
SET @description43 = N'房间完整编码（如 A栋-1层-101）';
EXEC sp_addextendedproperty 'MS_Description', @description43, 'SCHEMA', @defaultSchema43, 'TABLE', N'Rooms', 'COLUMN', N'FullCode';
SET @description43 = N'房间面积（平方米）';
EXEC sp_addextendedproperty 'MS_Description', @description43, 'SCHEMA', @defaultSchema43, 'TABLE', N'Rooms', 'COLUMN', N'Area';
SET @description43 = N'房间状态：Vacant空置/Rented已租/Maintenance维修';
EXEC sp_addextendedproperty 'MS_Description', @description43, 'SCHEMA', @defaultSchema43, 'TABLE', N'Rooms', 'COLUMN', N'Status';

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
VALUES (N'20260627140132_Init', N'10.0.9');

COMMIT;
GO

