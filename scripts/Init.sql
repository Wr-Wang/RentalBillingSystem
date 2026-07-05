-- ===================================================================
-- 1. AccountingSubjects - 会计科目表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AccountingSubjects]'))
CREATE TABLE [AccountingSubjects] (
    [Id] uniqueidentifier NULL,
    [Code] nvarchar(20) NULL,
    [Name] nvarchar(100) NULL,
    [ParentCode] nvarchar(20) NULL,
    [Level] int DEFAULT ((1)),
    [Direction] nvarchar(10) DEFAULT (N'Debit'),
    [IsLeaf] bit DEFAULT (CONVERT([bit],(1))),
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_AccountingSubjects] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'科目编码', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'Code';
EXEC sp_addextendedproperty 'MS_Description', N'科目名称', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'父级科目编码', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'ParentCode';
EXEC sp_addextendedproperty 'MS_Description', N'科目层级', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'Level';
EXEC sp_addextendedproperty 'MS_Description', N'借贷方向（Debit借方/Credit贷方）', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'Direction';
EXEC sp_addextendedproperty 'MS_Description', N'是否末级科目', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'IsLeaf';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'AccountingSubjects', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 2. AccountingSubjects_Audit - 会计科目审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AccountingSubjects_Audit]'))
CREATE TABLE [AccountingSubjects_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Code] nvarchar(20) NULL,
    [Name] nvarchar(100) NULL,
    [ParentCode] nvarchar(20) NULL,
    [Level] int NULL,
    [Direction] nvarchar(10) NULL,
    [IsLeaf] bit NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 3. ApiLogs - API请求日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApiLogs]'))
CREATE TABLE [ApiLogs] (
    [Id] uniqueidentifier DEFAULT (newsequentialid()),
    [ApiPath] nvarchar(500) NULL,
    [HttpMethod] nvarchar(10) NULL,
    [StatusCode] int NULL,
    [RequestBody] nvarchar(MAX) NULL,
    [ResponseBody] nvarchar(MAX) NULL,
    [RequestHeaders] nvarchar(MAX) NULL,
    [ClientIp] nvarchar(50) NULL,
    [UserId] uniqueidentifier NULL,
    [UserDisplayName] nvarchar(100) NULL,
    [DurationMs] int DEFAULT ((0)),
    [QueryString] nvarchar(2000) NULL,
    [UserAgent] nvarchar(500) NULL,
    [RequestAt] datetime2 DEFAULT (getutcdate()),
    [ResponseAt] datetime2 NULL,
    CONSTRAINT [PK_ApiLogs] PRIMARY KEY ([Id])
);
GO

-- 为已有表补充缺失列（幂等）
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApiLogs]') AND name='UserDisplayName')
    ALTER TABLE [ApiLogs] ADD [UserDisplayName] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApiLogs]') AND name='QueryString')
    ALTER TABLE [ApiLogs] ADD [QueryString] nvarchar(2000) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApiLogs]') AND name='UserAgent')
    ALTER TABLE [ApiLogs] ADD [UserAgent] nvarchar(500) NULL;
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'API路径', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'ApiPath';
EXEC sp_addextendedproperty 'MS_Description', N'HTTP方法', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'HttpMethod';
EXEC sp_addextendedproperty 'MS_Description', N'状态码', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'StatusCode';
EXEC sp_addextendedproperty 'MS_Description', N'请求体', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'RequestBody';
EXEC sp_addextendedproperty 'MS_Description', N'响应体', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'ResponseBody';
EXEC sp_addextendedproperty 'MS_Description', N'请求头', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'RequestHeaders';
EXEC sp_addextendedproperty 'MS_Description', N'客户端IP', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'ClientIp';
EXEC sp_addextendedproperty 'MS_Description', N'用户ID', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'UserId';
EXEC sp_addextendedproperty 'MS_Description', N'用户显示名', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'UserDisplayName';
EXEC sp_addextendedproperty 'MS_Description', N'耗时(毫秒)', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'DurationMs';
EXEC sp_addextendedproperty 'MS_Description', N'查询参数', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'QueryString';
EXEC sp_addextendedproperty 'MS_Description', N'用户代理', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'UserAgent';
EXEC sp_addextendedproperty 'MS_Description', N'请求时间', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'RequestAt';
EXEC sp_addextendedproperty 'MS_Description', N'响应时间', 'SCHEMA', 'dbo', 'TABLE', N'ApiLogs', 'COLUMN', N'ResponseAt';
GO

-- ===================================================================
-- 4. ApprovalLevelConfigs - 审批级别配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs]'))
CREATE TABLE [ApprovalLevelConfigs] (
    [Id] uniqueidentifier NULL,
    [ApprovalTypeId] uniqueidentifier NULL,
    [Level] int NULL,
    [RoleId] uniqueidentifier NULL,
    [MinAmount] decimal(18,2) NULL,
    [MaxAmount] decimal(18,2) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_ApprovalLevelConfigs] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'审批类型ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'ApprovalTypeId';
EXEC sp_addextendedproperty 'MS_Description', N'审批级别序号', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'Level';
EXEC sp_addextendedproperty 'MS_Description', N'审批角色ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'RoleId';
EXEC sp_addextendedproperty 'MS_Description', N'金额下限（满足此金额范围才需本级别审批）', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'MinAmount';
EXEC sp_addextendedproperty 'MS_Description', N'金额上限', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'MaxAmount';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalLevelConfigs', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 5. ApprovalLevelConfigs_Audit - 审批级别配置审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs_Audit]'))
CREATE TABLE [ApprovalLevelConfigs_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ApprovalTypeId] uniqueidentifier NULL,
    [Level] int NULL,
    [RoleId] uniqueidentifier NULL,
    [MinAmount] decimal(18,0) NULL,
    [MaxAmount] decimal(18,0) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 6. ApprovalRecords - 审批记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRecords]'))
CREATE TABLE [ApprovalRecords] (
    [Id] uniqueidentifier NULL,
    [ApprovalRequestId] uniqueidentifier NULL,
    [Level] int NULL,
    [ApproverId] uniqueidentifier NULL,
    [Action] nvarchar(20) NULL,
    [Comment] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_ApprovalRecords] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRecords', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'审批请求ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRecords', 'COLUMN', N'ApprovalRequestId';
EXEC sp_addextendedproperty 'MS_Description', N'审批级别', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRecords', 'COLUMN', N'Level';
EXEC sp_addextendedproperty 'MS_Description', N'审批人ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRecords', 'COLUMN', N'ApproverId';
EXEC sp_addextendedproperty 'MS_Description', N'审批动作（Approved通过/Rejected驳回）', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRecords', 'COLUMN', N'Action';
EXEC sp_addextendedproperty 'MS_Description', N'审批意见', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRecords', 'COLUMN', N'Comment';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRecords', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRecords', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 7. ApprovalRecords_Audit - 审批记录审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRecords_Audit]'))
CREATE TABLE [ApprovalRecords_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ApprovalRequestId] uniqueidentifier NULL,
    [Level] int NULL,
    [ApproverId] uniqueidentifier NULL,
    [Action] nvarchar(20) NULL,
    [Comment] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 8a. ApprovalBizData - 审批业务数据表（合同操作闭环）
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalBizData]'))
CREATE TABLE [ApprovalBizData] (
    [Id] uniqueidentifier NOT NULL,
    [ApprovalRequestId] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [ContractNo] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [ChangeType] nvarchar(30) NOT NULL,
    [EffectiveDate] date NULL,
    [OldAmount] decimal(18,2) NULL,
    [NewAmount] decimal(18,2) NULL,
    [Reason] nvarchar(500) NULL,
    [TerminateType] nvarchar(20) NULL,
    [ActualEndDate] date NULL,
    [DepositReturn] nvarchar(20) NULL,
    [IsProcessed] bit NOT NULL DEFAULT 0,
    [ProcessedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ApprovalBizData] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [IX_ApprovalBizData_ApprovalRequestId]
    ON [ApprovalBizData]([ApprovalRequestId]) WHERE [ApprovalRequestId] IS NOT NULL;
CREATE INDEX [IX_ApprovalBizData_ContractId] ON [ApprovalBizData]([ContractId]);
GO

-- ===================================================================
-- 8b. ApprovalFeeItems - 审批调价明细表（合同操作闭环）
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalFeeItems]'))
CREATE TABLE [ApprovalFeeItems] (
    [Id] uniqueidentifier NOT NULL,
    [ApprovalRequestId] uniqueidentifier NOT NULL,
    [ContractId] uniqueidentifier NOT NULL,
    [FeeCodeId] uniqueidentifier NOT NULL,
    [FeeName] nvarchar(100) NOT NULL,
    [OldAmount] decimal(18,2) NOT NULL,
    [NewAmount] decimal(18,2) NOT NULL,
    [BillingMode] nvarchar(20) NOT NULL,
    [Unit] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_ApprovalFeeItems] PRIMARY KEY ([Id])
);
CREATE INDEX [IX_ApprovalFeeItems_ApprovalRequestId] ON [ApprovalFeeItems]([ApprovalRequestId]);
GO

-- ===================================================================
-- 8. ApprovalRequests - 审批申请表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRequests]'))
CREATE TABLE [ApprovalRequests] (
    [Id] uniqueidentifier NULL,
    [ApprovalTypeId] uniqueidentifier NULL,
    [Title] nvarchar(200) NULL,
    [Description] nvarchar(1000) NULL,
    [TargetEntityId] uniqueidentifier NULL,
    [TargetEntityType] nvarchar(50) NULL,
    [CurrentLevel] int DEFAULT ((1)),
    [MaxLevel] int DEFAULT ((1)),
    [Status] nvarchar(20) DEFAULT (N'Pending'),
    [RowVersion] timestamp NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompletedAt] datetime2 NULL,
    [ContractId] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_ApprovalRequests] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'审批类型ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'ApprovalTypeId';
EXEC sp_addextendedproperty 'MS_Description', N'审批标题', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'Title';
EXEC sp_addextendedproperty 'MS_Description', N'审批申请描述', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'目标业务实体ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'TargetEntityId';
EXEC sp_addextendedproperty 'MS_Description', N'目标业务实体类型', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'TargetEntityType';
EXEC sp_addextendedproperty 'MS_Description', N'当前审批级别', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'CurrentLevel';
EXEC sp_addextendedproperty 'MS_Description', N'最大审批级别', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'MaxLevel';
EXEC sp_addextendedproperty 'MS_Description', N'审批状态（Pending/Approved/Rejected/Cancelled）', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'乐观锁版本号', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'RowVersion';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'审批完成时间（终审通过/驳回时写入，仅一次）', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'CompletedAt';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID（用于并发控制）', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalRequests', 'COLUMN', N'CompanyId';
GO

-- 为已有 ApprovalRequests 表补充缺失列（幂等）
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApprovalRequests]') AND name='ContractId')
    ALTER TABLE [ApprovalRequests] ADD [ContractId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ApprovalRequests]') AND name='CompletedAt')
    ALTER TABLE [ApprovalRequests] ADD [CompletedAt] datetime2 NULL;
GO

-- ===================================================================
-- 9. ApprovalRequests_Audit - 审批申请审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRequests_Audit]'))
CREATE TABLE [ApprovalRequests_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ApprovalTypeId] uniqueidentifier NULL,
    [Title] nvarchar(200) NULL,
    [Description] nvarchar(1000) NULL,
    [TargetEntityId] uniqueidentifier NULL,
    [TargetEntityType] nvarchar(50) NULL,
    [CurrentLevel] int NULL,
    [MaxLevel] int NULL,
    [Status] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 10. ApprovalTypes - 审批类型表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalTypes]'))
CREATE TABLE [ApprovalTypes] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Code] nvarchar(50) NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_ApprovalTypes] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'审批类型名称', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'审批类型编码', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'Code';
EXEC sp_addextendedproperty 'MS_Description', N'审批类型描述', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'ApprovalTypes', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 11. ApprovalTypes_Audit - 审批类型审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalTypes_Audit]'))
CREATE TABLE [ApprovalTypes_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Code] nvarchar(50) NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 12. AutoRenewConfig_Audit
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AutoRenewConfig_Audit]'))
CREATE TABLE [AutoRenewConfig_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [AdvanceDays] int NULL,
    [RentRule] nvarchar(20) NULL,
    [RentIncreasePercent] decimal(18,0) NULL,
    [TermRule] nvarchar(20) NULL,
    [TermMonths] int NULL,
    [OverdueAction] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 13. AutoRenewConfigs - 自动续签配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AutoRenewConfigs]'))
CREATE TABLE [AutoRenewConfigs] (
    [Id] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [AdvanceDays] int DEFAULT ((7)),
    [RentRule] nvarchar(20) DEFAULT ('Same'),
    [RentIncreasePercent] decimal(5,2) NULL,
    [TermRule] nvarchar(20) DEFAULT ('Same'),
    [TermMonths] int NULL,
    [OverdueAction] nvarchar(20) DEFAULT ('Block'),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getdate()),
    CONSTRAINT [PK_AutoRenewConfigs] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'提前天数', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'AdvanceDays';
EXEC sp_addextendedproperty 'MS_Description', N'租金规则', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'RentRule';
EXEC sp_addextendedproperty 'MS_Description', N'租金涨幅', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'RentIncreasePercent';
EXEC sp_addextendedproperty 'MS_Description', N'期限规则', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'TermRule';
EXEC sp_addextendedproperty 'MS_Description', N'续签月数', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'TermMonths';
EXEC sp_addextendedproperty 'MS_Description', N'欠费处理', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'OverdueAction';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'AutoRenewConfigs', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 14. BankMatches - 银行匹配记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankMatches]'))
CREATE TABLE [BankMatches] (
    [Id] uniqueidentifier NULL,
    [BankStatementId] uniqueidentifier NULL,
    [InternalDocumentId] uniqueidentifier NULL,
    [DocumentType] nvarchar(20) DEFAULT ('Receipt'),
    [MatchedAmount] decimal(18,2) DEFAULT ((0)),
    [MatchMethod] nvarchar(20) DEFAULT ('Manual'),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_BankMatches] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'BankMatches', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'银行流水ID', 'SCHEMA', 'dbo', 'TABLE', N'BankMatches', 'COLUMN', N'BankStatementId';
EXEC sp_addextendedproperty 'MS_Description', N'内部单据ID', 'SCHEMA', 'dbo', 'TABLE', N'BankMatches', 'COLUMN', N'InternalDocumentId';
EXEC sp_addextendedproperty 'MS_Description', N'单据类型', 'SCHEMA', 'dbo', 'TABLE', N'BankMatches', 'COLUMN', N'DocumentType';
EXEC sp_addextendedproperty 'MS_Description', N'匹配金额', 'SCHEMA', 'dbo', 'TABLE', N'BankMatches', 'COLUMN', N'MatchedAmount';
EXEC sp_addextendedproperty 'MS_Description', N'匹配方式', 'SCHEMA', 'dbo', 'TABLE', N'BankMatches', 'COLUMN', N'MatchMethod';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'BankMatches', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'BankMatches', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 15. BankReconciliations - 银行余额调节表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankReconciliations]'))
CREATE TABLE [BankReconciliations] (
    [Id] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [StartDate] date NULL,
    [EndDate] date NULL,
    [Status] nvarchar(20) DEFAULT ('InProgress'),
    [OpeningBalance] decimal(18,2) DEFAULT ((0)),
    [ClosingBalance] decimal(18,2) DEFAULT ((0)),
    [StatementTotal] decimal(18,2) DEFAULT ((0)),
    [SystemTotal] decimal(18,2) DEFAULT ((0)),
    [CompletedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_BankReconciliations] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'开始日期', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'StartDate';
EXEC sp_addextendedproperty 'MS_Description', N'结束日期', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'EndDate';
EXEC sp_addextendedproperty 'MS_Description', N'状态', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'期初余额', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'OpeningBalance';
EXEC sp_addextendedproperty 'MS_Description', N'期末余额', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'ClosingBalance';
EXEC sp_addextendedproperty 'MS_Description', N'银行总额', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'StatementTotal';
EXEC sp_addextendedproperty 'MS_Description', N'系统总额', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'SystemTotal';
EXEC sp_addextendedproperty 'MS_Description', N'完成时间', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'CompletedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'BankReconciliations', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 16. BankStatements - 银行流水表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankStatements]'))
CREATE TABLE [BankStatements] (
    [Id] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [TransactionDate] date NULL,
    [Amount] decimal(18,2) NULL,
    [Balance] decimal(18,2) DEFAULT ((0)),
    [Description] nvarchar(MAX) NULL,
    [ReferenceNo] nvarchar(100) NULL,
    [Counterparty] nvarchar(200) NULL,
    [Status] nvarchar(20) DEFAULT ('Unmatched'),
    [ImportBatchId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getdate()),
    CONSTRAINT [PK_BankStatements] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'交易日期', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'TransactionDate';
EXEC sp_addextendedproperty 'MS_Description', N'金额', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'Amount';
EXEC sp_addextendedproperty 'MS_Description', N'余额', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'Balance';
EXEC sp_addextendedproperty 'MS_Description', N'描述', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'参考号', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'ReferenceNo';
EXEC sp_addextendedproperty 'MS_Description', N'对方账户', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'Counterparty';
EXEC sp_addextendedproperty 'MS_Description', N'状态', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'导入批次ID', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'ImportBatchId';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'BankStatements', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 17. BankStatements_Audit - 银行流水审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankStatements_Audit]'))
CREATE TABLE [BankStatements_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [TransactionDate] date NULL,
    [Amount] decimal(18,0) NULL,
    [Balance] decimal(18,0) NULL,
    [Description] nvarchar(MAX) NULL,
    [ReferenceNo] nvarchar(100) NULL,
    [Counterparty] nvarchar(200) NULL,
    [Status] nvarchar(20) NULL,
    [ImportBatchId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 18. BuildingFloorLevelConfigs_Audit
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BuildingFloorLevelConfigs_Audit]'))
CREATE TABLE [BuildingFloorLevelConfigs_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [BuildingId] uniqueidentifier NULL,
    [FloorLevelBandId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 19. Buildings_Audit
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Buildings_Audit]'))
CREATE TABLE [Buildings_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(200) NULL,
    [Code] nvarchar(50) NULL,
    [Address] nvarchar(500) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 20. ChangeRequestItems - 合同变更明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ChangeRequestItems]'))
CREATE TABLE [ChangeRequestItems] (
    [Id] uniqueidentifier NULL,
    [ChangeRequestId] uniqueidentifier NULL,
    [TargetType] nvarchar(20) NULL,
    [TargetId] uniqueidentifier NULL,
    [FieldName] nvarchar(50) NULL,
    [OldValue] nvarchar(100) NULL,
    [NewValue] nvarchar(100) NULL,
    [OldValueDecimal] decimal(18,2) NULL,
    [NewValueDecimal] decimal(18,2) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_ChangeRequestItems] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'变更请求ID', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'ChangeRequestId';
EXEC sp_addextendedproperty 'MS_Description', N'目标类型', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'TargetType';
EXEC sp_addextendedproperty 'MS_Description', N'目标ID', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'TargetId';
EXEC sp_addextendedproperty 'MS_Description', N'字段名称', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'FieldName';
EXEC sp_addextendedproperty 'MS_Description', N'旧值', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'OldValue';
EXEC sp_addextendedproperty 'MS_Description', N'新值', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'NewValue';
EXEC sp_addextendedproperty 'MS_Description', N'旧值(数值)', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'OldValueDecimal';
EXEC sp_addextendedproperty 'MS_Description', N'新值(数值)', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'NewValueDecimal';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequestItems', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 21. ChangeRequests - 合同变更请求表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ChangeRequests]'))
CREATE TABLE [ChangeRequests] (
    [Id] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [ChangeType] nvarchar(30) NULL,
    [Status] nvarchar(20) DEFAULT ('Draft'),
    [EffectiveDate] date NULL,
    [Reason] nvarchar(500) NULL,
    [BatchId] uniqueidentifier NULL,
    [ApprovalRequestId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ChangeRequests] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'变更类型', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'ChangeType';
EXEC sp_addextendedproperty 'MS_Description', N'状态', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'生效日期', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'EffectiveDate';
EXEC sp_addextendedproperty 'MS_Description', N'原因', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'Reason';
EXEC sp_addextendedproperty 'MS_Description', N'批次ID', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'BatchId';
EXEC sp_addextendedproperty 'MS_Description', N'审批请求ID', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'ApprovalRequestId';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'ChangeRequests', 'COLUMN', N'UpdatedAt';
GO

-- ===================================================================
-- 22. CollectionRecords - 催缴记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionRecords]'))
CREATE TABLE [CollectionRecords] (
    [Id] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [CollectionStageId] uniqueidentifier NULL,
    [ContactResult] nvarchar(500) NULL,
    [Remark] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_CollectionRecords] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'催缴阶段ID', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'CollectionStageId';
EXEC sp_addextendedproperty 'MS_Description', N'联系结果', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'ContactResult';
EXEC sp_addextendedproperty 'MS_Description', N'备注', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'Remark';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'CollectionRecords', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 23. CollectionStages - 催缴阶段配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionStages]'))
CREATE TABLE [CollectionStages] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [DaysOverdue] int NULL,
    [SortOrder] int DEFAULT ((0)),
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_CollectionStages] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'催缴阶段名称', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'逾期天数触发条件', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'DaysOverdue';
EXEC sp_addextendedproperty 'MS_Description', N'排序号', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'SortOrder';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'CollectionStages', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 24. CollectionStages_Audit - 催缴阶段配置审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionStages_Audit]'))
CREATE TABLE [CollectionStages_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [DaysOverdue] int NULL,
    [SortOrder] int NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 25. Companies - 公司表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Companies]'))
CREATE TABLE [Companies] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(200) NULL,
    [Code] nvarchar(50) NULL,
    [ContactPerson] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Address] nvarchar(500) NULL,
    [IsActive] bit DEFAULT ((1)),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getdate()),
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'名称', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'编码', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'Code';
EXEC sp_addextendedproperty 'MS_Description', N'联系人', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'ContactPerson';
EXEC sp_addextendedproperty 'MS_Description', N'联系电话', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'Phone';
EXEC sp_addextendedproperty 'MS_Description', N'地址', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'Address';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'Companies', 'COLUMN', N'UpdatedAt';
GO

-- ===================================================================
-- 26. Companies_Audit - 公司审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Companies_Audit]'))
CREATE TABLE [Companies_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(200) NULL,
    [Code] nvarchar(50) NULL,
    [ContactPerson] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Address] nvarchar(500) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 27. ContractFeeConfigs - 合同费用配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs]'))
CREATE TABLE [ContractFeeConfigs] (
    [Id] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [BillingMode] nvarchar(20) DEFAULT (N'FixedAmount'),
    [Amount] decimal(18,2) NULL,
    [Unit] nvarchar(20) NULL,
    [UnitPrice] decimal(18,4) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [EffectiveDate] date NULL,
    [ExpiryDate] date NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_ContractFeeConfigs] PRIMARY KEY ([Id])
);
GO

-- 为已有 ContractFeeConfigs 表补充缺失列（幂等）
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs]') AND name='EffectiveDate')
    ALTER TABLE [ContractFeeConfigs] ADD [EffectiveDate] date NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs]') AND name='ExpiryDate')
    ALTER TABLE [ContractFeeConfigs] ADD [ExpiryDate] date NULL;
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'费用项目ID', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'FeeCodeId';
EXEC sp_addextendedproperty 'MS_Description', N'计费模式（FixedAmount固定金额/MeterBased抄表）', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'BillingMode';
EXEC sp_addextendedproperty 'MS_Description', N'金额', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'Amount';
EXEC sp_addextendedproperty 'MS_Description', N'计量单位', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'Unit';
EXEC sp_addextendedproperty 'MS_Description', N'单价', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'UnitPrice';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'生效日期', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'EffectiveDate';
EXEC sp_addextendedproperty 'MS_Description', N'到期日期，NULL 表示当前生效', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'ExpiryDate';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'ContractFeeConfigs', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 28. ContractFeeConfigs_Audit - 合同费用配置审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs_Audit]'))
CREATE TABLE [ContractFeeConfigs_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [BillingMode] nvarchar(20) NULL,
    [Amount] decimal(18,0) NULL,
    [Unit] nvarchar(20) NULL,
    [UnitPrice] decimal(18,0) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 29. Contracts - 合同表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Contracts]'))
CREATE TABLE [Contracts] (
    [Id] uniqueidentifier NULL,
    [ContractNo] nvarchar(100) NULL,
    [RoomId] uniqueidentifier NULL,
    [RentAmount] decimal(18,2) NULL,
    [DepositAmount] decimal(18,2) NULL,
    [StartDate] date NULL,
    [EndDate] date NULL,
    [PaymentCycle] nvarchar(20) NULL,
    [Status] nvarchar(20) NULL,
    [RowVersion] timestamp NULL,
    [TerminatedAt] datetime2 NULL,
    [TerminationReason] nvarchar(MAX) NULL,
    [SuspendedAt] datetime2 NULL,
    [ResumedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'合同编号，自动生成', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'ContractNo';
EXEC sp_addextendedproperty 'MS_Description', N'房间ID', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'RoomId';
EXEC sp_addextendedproperty 'MS_Description', N'租金金额', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'RentAmount';
EXEC sp_addextendedproperty 'MS_Description', N'押金金额', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'DepositAmount';
EXEC sp_addextendedproperty 'MS_Description', N'合同开始日期', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'StartDate';
EXEC sp_addextendedproperty 'MS_Description', N'合同结束日期', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'EndDate';
EXEC sp_addextendedproperty 'MS_Description', N'付款周期（Monthly/Quarterly/Yearly）', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'PaymentCycle';
EXEC sp_addextendedproperty 'MS_Description', N'合同状态（Draft/Active/Suspended/Terminated等）', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'乐观锁版本号', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'RowVersion';
EXEC sp_addextendedproperty 'MS_Description', N'终止原因', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'TerminationReason';
EXEC sp_addextendedproperty 'MS_Description', N'暂停时间', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'SuspendedAt';
EXEC sp_addextendedproperty 'MS_Description', N'恢复时间', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'ResumedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'Contracts', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 30. Contracts_Audit - 合同审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Contracts_Audit]'))
CREATE TABLE [Contracts_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ContractNo] nvarchar(100) NULL,
    [RoomId] uniqueidentifier NULL,
    [RentAmount] decimal(18,0) NULL,
    [DepositAmount] decimal(18,0) NULL,
    [StartDate] date NULL,
    [EndDate] date NULL,
    [PaymentCycle] nvarchar(20) NULL,
    [Status] nvarchar(20) NULL,
    [TerminatedAt] datetime2 NULL,
    [TerminationReason] nvarchar(MAX) NULL,
    [SuspendedAt] datetime2 NULL,
    [ResumedAt] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 31. ContractTenants - 合同租客关联表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractTenants]'))
CREATE TABLE [ContractTenants] (
    [Id] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    [IsPrimary] bit DEFAULT (CONVERT([bit],(0))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_ContractTenants] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ContractTenants', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID', 'SCHEMA', 'dbo', 'TABLE', N'ContractTenants', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'租客ID', 'SCHEMA', 'dbo', 'TABLE', N'ContractTenants', 'COLUMN', N'TenantId';
EXEC sp_addextendedproperty 'MS_Description', N'是否主租客', 'SCHEMA', 'dbo', 'TABLE', N'ContractTenants', 'COLUMN', N'IsPrimary';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ContractTenants', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ContractTenants', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 32. ContractTenants_Audit - 合同租客关联审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractTenants_Audit]'))
CREATE TABLE [ContractTenants_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [TenantId] uniqueidentifier NULL,
    [IsPrimary] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 33. DebitNoteItems - 账单明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNoteItems]'))
CREATE TABLE [DebitNoteItems] (
    [Id] uniqueidentifier NULL,
    [DebitNoteId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [FeeName] nvarchar(100) NULL,
    [Amount] decimal(18,2) NULL,
    [Received] decimal(18,2) NOT NULL DEFAULT 0,
    [BillingMode] nvarchar(20) NULL,
    [Unit] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_DebitNoteItems] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'DebitNoteItems', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'账单ID', 'SCHEMA', 'dbo', 'TABLE', N'DebitNoteItems', 'COLUMN', N'DebitNoteId';
EXEC sp_addextendedproperty 'MS_Description', N'费用项目ID', 'SCHEMA', 'dbo', 'TABLE', N'DebitNoteItems', 'COLUMN', N'FeeCodeId';
EXEC sp_addextendedproperty 'MS_Description', N'费用金额', 'SCHEMA', 'dbo', 'TABLE', N'DebitNoteItems', 'COLUMN', N'Amount';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'DebitNoteItems', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'DebitNoteItems', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 34. DebitNoteItems_Audit - 账单明细审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNoteItems_Audit]'))
CREATE TABLE [DebitNoteItems_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [DebitNoteId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [Amount] decimal(18,0) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 35. DebitNotes - 账单表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNotes]'))
CREATE TABLE [DebitNotes] (
    [Id] uniqueidentifier NULL,
    [NoteNo] nvarchar(100) NULL,
    [ContractId] uniqueidentifier NULL,
    [Period] nvarchar(7) NULL,
    [TotalAmount] decimal(18,2) NULL,
    [TotalReceived] decimal(18,2) NOT NULL DEFAULT 0,
    [TotalPrepaid] decimal(18,2) NOT NULL DEFAULT 0,
    [BalanceDue] decimal(18,2) NOT NULL DEFAULT 0,
    [Status] nvarchar(20) DEFAULT (N'Draft'),
    [IsHistorical] bit NOT NULL DEFAULT 0,
    [DueDate] date NULL,
    [ContractNo] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    [RoomFullCode] nvarchar(200) NULL,
    [TenantName] nvarchar(100) NULL,
    [GeneratedAt] datetime2 NULL,
    [CancelledAt] datetime2 NULL,
    [CancelledBy] uniqueidentifier NULL,
    [CancelReason] nvarchar(500) NULL,
    [BillJobTaskLogId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_DebitNotes] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'账单编号', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'NoteNo';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'账单账期', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'Period';
EXEC sp_addextendedproperty 'MS_Description', N'账单总金额', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'TotalAmount';
EXEC sp_addextendedproperty 'MS_Description', N'状态（Draft草稿/Issued已发布）', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'DebitNotes', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 36. DebitNotes_Audit - 账单审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNotes_Audit]'))
CREATE TABLE [DebitNotes_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [NoteNo] nvarchar(100) NULL,
    [ContractId] uniqueidentifier NULL,
    [Period] nvarchar(7) NULL,
    [TotalAmount] decimal(18,0) NULL,
    [Status] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 37. DepositLogs - 押金变动日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DepositLogs]'))
CREATE TABLE [DepositLogs] (
    [Id] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NULL,
    [Balance] decimal(18,2) NULL,
    [Action] nvarchar(20) DEFAULT (N'Create'),
    [Remark] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_DepositLogs] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'押金变动金额', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'Amount';
EXEC sp_addextendedproperty 'MS_Description', N'押金余额', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'Balance';
EXEC sp_addextendedproperty 'MS_Description', N'操作类型（Create创建/Return退还/Deduct扣除）', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'Action';
EXEC sp_addextendedproperty 'MS_Description', N'备注说明', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'Remark';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'DepositLogs', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 38. DepositLogs_Audit - 押金变动日志审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DepositLogs_Audit]'))
CREATE TABLE [DepositLogs_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [Amount] decimal(18,0) NULL,
    [Balance] decimal(18,0) NULL,
    [Action] nvarchar(20) NULL,
    [Remark] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 39. FeeCodes - 收费项目表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodes]'))
CREATE TABLE [FeeCodes] (
    [Id] uniqueidentifier NULL,
    [Code] nvarchar(50) NULL,
    [Name] nvarchar(100) NULL,
    [BillingMode] nvarchar(20) DEFAULT (N'FixedAmount'),
    [Unit] nvarchar(20) NULL,
    [SortOrder] int DEFAULT ((0)),
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [Category] nvarchar(50) DEFAULT (N'Other'),
    [IsRequired] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_FeeCodes] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'费用编码（如 RENT/WATER/ELECTRIC）', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'Code';
EXEC sp_addextendedproperty 'MS_Description', N'费用名称（如 房租费/水费/电费）', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'计费模式', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'BillingMode';
EXEC sp_addextendedproperty 'MS_Description', N'计量单位（元/吨、元/度）', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'Unit';
EXEC sp_addextendedproperty 'MS_Description', N'排序号', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'SortOrder';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'费用分类（Core核心/Utility公共事业）', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'Category';
EXEC sp_addextendedproperty 'MS_Description', N'是否必填', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'IsRequired';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodes', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 40. FeeCodes_Audit - 收费项目审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodes_Audit]'))
CREATE TABLE [FeeCodes_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Code] nvarchar(50) NULL,
    [Name] nvarchar(100) NULL,
    [BillingMode] nvarchar(20) NULL,
    [Unit] nvarchar(20) NULL,
    [SortOrder] int NULL,
    [IsActive] bit NULL,
    [Category] nvarchar(50) NULL,
    [IsRequired] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 41. FeeCodeTemplates - 费用科目模板表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates]'))
CREATE TABLE [FeeCodeTemplates] (
    [Id] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [Description] nvarchar(200) NULL,
    [DefaultAmount] decimal(18,2) NULL,
    [DefaultUnitPrice] decimal(18,4) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_FeeCodeTemplates] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'费用项目ID', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'FeeCodeId';
EXEC sp_addextendedproperty 'MS_Description', N'模板描述', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'默认金额', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'DefaultAmount';
EXEC sp_addextendedproperty 'MS_Description', N'默认单价', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'DefaultUnitPrice';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'FeeCodeTemplates', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 42. FeeCodeTemplates_Audit - 费用科目模板审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates_Audit]'))
CREATE TABLE [FeeCodeTemplates_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [Description] nvarchar(200) NULL,
    [DefaultAmount] decimal(18,0) NULL,
    [DefaultUnitPrice] decimal(18,0) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 43. FloorLevelBands - 楼层级别表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FloorLevelBands]'))
CREATE TABLE [FloorLevelBands] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(50) NULL,
    [MinLevel] int NULL,
    [MaxLevel] int NULL,
    [Description] nvarchar(200) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_FloorLevelBands] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'楼层级别名称（低区/中区/高区）', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'最小级别', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'MinLevel';
EXEC sp_addextendedproperty 'MS_Description', N'最大级别', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'MaxLevel';
EXEC sp_addextendedproperty 'MS_Description', N'楼层级别描述', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'FloorLevelBands', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 44. FloorLevelBands_Audit - 楼层级别审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FloorLevelBands_Audit]'))
CREATE TABLE [FloorLevelBands_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(50) NULL,
    [MinLevel] int NULL,
    [MaxLevel] int NULL,
    [Description] nvarchar(200) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 45. Floors_Audit
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Floors_Audit]'))
CREATE TABLE [Floors_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [BuildingId] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [SortOrder] int NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 46. HolidayCalendars - 节假日日历表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HolidayCalendars]'))
CREATE TABLE [HolidayCalendars] (
    [Id] uniqueidentifier NULL,
    [HolidayDate] date NULL,
    [Name] nvarchar(100) NULL,
    [IsWorkingDay] bit DEFAULT (CONVERT([bit],(0))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_HolidayCalendars] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'日期', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'HolidayDate';
EXEC sp_addextendedproperty 'MS_Description', N'节假日名称', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'是否工作日（false=放假/true=调休上班）', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'IsWorkingDay';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'HolidayCalendars', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 47. HolidayCalendars_Audit - 节假日日历审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HolidayCalendars_Audit]'))
CREATE TABLE [HolidayCalendars_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [HolidayDate] date NULL,
    [Name] nvarchar(100) NULL,
    [IsWorkingDay] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 48. HousingUnits - 房源表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HousingUnits]'))
CREATE TABLE [HousingUnits] (
    [Id] uniqueidentifier NULL,
    [BuildingName] nvarchar(200) NULL,
    [BuildingCode] nvarchar(50) NULL,
    [BuildingAddress] nvarchar(500) NULL,
    [CompanyId] uniqueidentifier NULL,
    [FloorName] nvarchar(100) NULL,
    [FloorSortOrder] int NULL,
    [UnitNo] nvarchar(50) NULL,
    [FullCode] nvarchar(100) NULL,
    [RoomTypeId] uniqueidentifier NULL,
    [Area] decimal(10,2) NULL,
    [Orientation] nvarchar(20) NULL,
    [BaseRentAmount] decimal(18,2) NULL,
    [Status] nvarchar(20) DEFAULT ('Vacant'),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getdate()),
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_HousingUnits] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'楼宇名称', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'BuildingName';
EXEC sp_addextendedproperty 'MS_Description', N'楼宇编码', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'BuildingCode';
EXEC sp_addextendedproperty 'MS_Description', N'楼宇地址', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'BuildingAddress';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'楼层名称', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'FloorName';
EXEC sp_addextendedproperty 'MS_Description', N'楼层排序', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'FloorSortOrder';
EXEC sp_addextendedproperty 'MS_Description', N'房间编号', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'UnitNo';
EXEC sp_addextendedproperty 'MS_Description', N'完整编码', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'FullCode';
EXEC sp_addextendedproperty 'MS_Description', N'房型ID', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'RoomTypeId';
EXEC sp_addextendedproperty 'MS_Description', N'面积', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'Area';
EXEC sp_addextendedproperty 'MS_Description', N'朝向', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'Orientation';
EXEC sp_addextendedproperty 'MS_Description', N'基础租金', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'BaseRentAmount';
EXEC sp_addextendedproperty 'MS_Description', N'状态', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'HousingUnits', 'COLUMN', N'UpdatedAt';
GO

-- ===================================================================
-- 49. HousingUnits_Audit - 房源审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HousingUnits_Audit]'))
CREATE TABLE [HousingUnits_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [BuildingName] nvarchar(200) NULL,
    [BuildingCode] nvarchar(50) NULL,
    [BuildingAddress] nvarchar(500) NULL,
    [CompanyId] uniqueidentifier NULL,
    [FloorName] nvarchar(100) NULL,
    [FloorSortOrder] int NULL,
    [UnitNo] nvarchar(50) NULL,
    [FullCode] nvarchar(100) NULL,
    [RoomTypeId] uniqueidentifier NULL,
    [Area] decimal(18,0) NULL,
    [Orientation] nvarchar(20) NULL,
    [BaseRentAmount] decimal(18,0) NULL,
    [Status] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 50. ImportBatches - 导入批次表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatches]'))
CREATE TABLE [ImportBatches] (
    [Id] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [ImportType] nvarchar(50) NULL,
    [FileName] nvarchar(200) NULL,
    [TotalRows] int DEFAULT ((0)),
    [ValidRows] int DEFAULT ((0)),
    [FailedRows] int DEFAULT ((0)),
    [Status] nvarchar(20) DEFAULT ('PendingApproval'),
    [ApprovalRequestId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getdate()),
    CONSTRAINT [PK_ImportBatches] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'导入类型', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'ImportType';
EXEC sp_addextendedproperty 'MS_Description', N'文件名', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'FileName';
EXEC sp_addextendedproperty 'MS_Description', N'总行数', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'TotalRows';
EXEC sp_addextendedproperty 'MS_Description', N'有效行数', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'ValidRows';
EXEC sp_addextendedproperty 'MS_Description', N'失败行数', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'FailedRows';
EXEC sp_addextendedproperty 'MS_Description', N'状态', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'审批请求ID', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'ApprovalRequestId';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatches', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 51. ImportBatches_Audit - 导入批次审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatches_Audit]'))
CREATE TABLE [ImportBatches_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [ImportType] nvarchar(50) NULL,
    [FileName] nvarchar(200) NULL,
    [TotalRows] int NULL,
    [ValidRows] int NULL,
    [FailedRows] int NULL,
    [Status] nvarchar(20) NULL,
    [ApprovalRequestId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 52. ImportBatchItems - 导入批次明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatchItems]'))
CREATE TABLE [ImportBatchItems] (
    [Id] uniqueidentifier NULL,
    [ImportBatchId] uniqueidentifier NULL,
    [ImportType] nvarchar(50) NULL,
    [RowIndex] int NULL,
    [Status] nvarchar(20) DEFAULT ('Pending'),
    [ErrorMsg] nvarchar(500) NULL,
    [RawData] nvarchar(MAX) NULL,
    [CompanyId] uniqueidentifier NULL,
    [BuildingName] nvarchar(200) NULL,
    [BuildingCode] nvarchar(50) NULL,
    [BuildingAddress] nvarchar(500) NULL,
    [FloorName] nvarchar(100) NULL,
    [FloorSortOrder] int NULL,
    [UnitNo] nvarchar(50) NULL,
    [FullCode] nvarchar(100) NULL,
    [RoomTypeId] uniqueidentifier NULL,
    [Area] decimal(10,2) NULL,
    [Orientation] nvarchar(20) NULL,
    [BaseRentAmount] decimal(18,2) NULL,
    [TenantName] nvarchar(100) NULL,
    [TenantPhone] nvarchar(20) NULL,
    [ContractStartDate] date NULL,
    [ContractEndDate] date NULL,
    [RentAmount] decimal(18,2) NULL,
    [DepositAmount] decimal(18,2) NULL,
    [PaymentCycle] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_ImportBatchItems] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'导入批次ID', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'ImportBatchId';
EXEC sp_addextendedproperty 'MS_Description', N'导入类型', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'ImportType';
EXEC sp_addextendedproperty 'MS_Description', N'行号', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'RowIndex';
EXEC sp_addextendedproperty 'MS_Description', N'状态', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'错误信息', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'ErrorMsg';
EXEC sp_addextendedproperty 'MS_Description', N'原始数据', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'RawData';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'楼宇名称', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'BuildingName';
EXEC sp_addextendedproperty 'MS_Description', N'楼宇编码', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'BuildingCode';
EXEC sp_addextendedproperty 'MS_Description', N'楼宇地址', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'BuildingAddress';
EXEC sp_addextendedproperty 'MS_Description', N'楼层名称', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'FloorName';
EXEC sp_addextendedproperty 'MS_Description', N'楼层排序', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'FloorSortOrder';
EXEC sp_addextendedproperty 'MS_Description', N'房间编号', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'UnitNo';
EXEC sp_addextendedproperty 'MS_Description', N'完整编码', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'FullCode';
EXEC sp_addextendedproperty 'MS_Description', N'房型ID', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'RoomTypeId';
EXEC sp_addextendedproperty 'MS_Description', N'面积', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'Area';
EXEC sp_addextendedproperty 'MS_Description', N'朝向', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'Orientation';
EXEC sp_addextendedproperty 'MS_Description', N'基础租金', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'BaseRentAmount';
EXEC sp_addextendedproperty 'MS_Description', N'租客姓名', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'TenantName';
EXEC sp_addextendedproperty 'MS_Description', N'租客电话', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'TenantPhone';
EXEC sp_addextendedproperty 'MS_Description', N'合同开始日期', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'ContractStartDate';
EXEC sp_addextendedproperty 'MS_Description', N'合同结束日期', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'ContractEndDate';
EXEC sp_addextendedproperty 'MS_Description', N'租金金额', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'RentAmount';
EXEC sp_addextendedproperty 'MS_Description', N'押金金额', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'DepositAmount';
EXEC sp_addextendedproperty 'MS_Description', N'付款周期', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'PaymentCycle';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ImportBatchItems', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 53. JobScheduleExecutions - 调度执行记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobScheduleExecutions]'))
CREATE TABLE [JobScheduleExecutions] (
    [Id] uniqueidentifier NULL,
    [JobScheduleId] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [TargetDate] datetime2 NULL,
    [OriginalDate] datetime2 NULL,
    [Month] nvarchar(7) NULL,
    [Status] nvarchar(20) DEFAULT ('Pending'),
    [Reason] nvarchar(500) NULL,
    [IsAdjusted] bit DEFAULT ((0)),
    [IsCustom] bit DEFAULT ((0)),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getdate()),
    CONSTRAINT [PK_JobScheduleExecutions] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'调度任务ID', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'JobScheduleId';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'目标时间', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'TargetDate';
EXEC sp_addextendedproperty 'MS_Description', N'原始时间', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'OriginalDate';
EXEC sp_addextendedproperty 'MS_Description', N'月份', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'Month';
EXEC sp_addextendedproperty 'MS_Description', N'状态', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'原因', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'Reason';
EXEC sp_addextendedproperty 'MS_Description', N'是否已调整', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'IsAdjusted';
EXEC sp_addextendedproperty 'MS_Description', N'是否自定义', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'IsCustom';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'JobScheduleExecutions', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 54. JobSchedules - 调度任务表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobSchedules]'))
CREATE TABLE [JobSchedules] (
    [Id] uniqueidentifier NULL,
    [JobName] nvarchar(200) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [Description] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_JobSchedules] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'作业名称', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'JobName';
EXEC sp_addextendedproperty 'MS_Description', N'Cron 表达式', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'CronExpression';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'作业描述', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'JobSchedules', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 55. JobSchedules_Audit - 调度任务审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobSchedules_Audit]'))
CREATE TABLE [JobSchedules_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [JobName] nvarchar(200) NULL,
    [IsActive] bit NULL,
    [Description] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 56. JobTemplates - 任务模板表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobTemplates]'))
CREATE TABLE [JobTemplates] (
    [Id] uniqueidentifier NULL,
    [Code] nvarchar(50) NULL,
    [DisplayName] nvarchar(100) NULL,
    [ShortName] nvarchar(50) NULL,
    [DefaultCronExpression] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [Icon] nvarchar(50) NULL,
    [Category] nvarchar(50) NULL,
    [SortOrder] int DEFAULT ((0)),
    [IsActive] bit DEFAULT ((1)),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getdate()),
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_JobTemplates] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'编码', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'Code';
EXEC sp_addextendedproperty 'MS_Description', N'显示名称', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'DisplayName';
EXEC sp_addextendedproperty 'MS_Description', N'简称', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'ShortName';
EXEC sp_addextendedproperty 'MS_Description', N'默认Cron表达式', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'DefaultCronExpression';
EXEC sp_addextendedproperty 'MS_Description', N'描述', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'图标', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'Icon';
EXEC sp_addextendedproperty 'MS_Description', N'分类', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'Category';
EXEC sp_addextendedproperty 'MS_Description', N'排序号', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'SortOrder';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'JobTemplates', 'COLUMN', N'UpdatedAt';
GO

-- ===================================================================
-- 57. JobTemplates_Audit - 任务模板审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobTemplates_Audit]'))
CREATE TABLE [JobTemplates_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Code] nvarchar(50) NULL,
    [DisplayName] nvarchar(100) NULL,
    [ShortName] nvarchar(50) NULL,
    [DefaultCronExpression] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [Icon] nvarchar(50) NULL,
    [Category] nvarchar(50) NULL,
    [SortOrder] int NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 58. JournalEntries - 凭证分录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JournalEntries]'))
CREATE TABLE [JournalEntries] (
    [Id] uniqueidentifier NULL,
    [VoucherId] uniqueidentifier NULL,
    [AccountingSubjectId] uniqueidentifier NULL,
    [Direction] nvarchar(10) NULL,
    [Amount] decimal(18,2) NULL,
    [Summary] nvarchar(200) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'凭证ID', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'VoucherId';
EXEC sp_addextendedproperty 'MS_Description', N'会计科目ID', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'AccountingSubjectId';
EXEC sp_addextendedproperty 'MS_Description', N'借贷方向（Debit/Credit）', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'Direction';
EXEC sp_addextendedproperty 'MS_Description', N'金额', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'Amount';
EXEC sp_addextendedproperty 'MS_Description', N'分录摘要说明', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'Summary';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'JournalEntries', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 59. JournalEntries_Audit - 凭证分录审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JournalEntries_Audit]'))
CREATE TABLE [JournalEntries_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [VoucherId] uniqueidentifier NULL,
    [AccountingSubjectId] uniqueidentifier NULL,
    [Direction] nvarchar(10) NULL,
    [Amount] decimal(18,0) NULL,
    [Summary] nvarchar(200) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 60. LateFeeConfigs - 滞纳金配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[LateFeeConfigs]'))
CREATE TABLE [LateFeeConfigs] (
    [Id] uniqueidentifier NULL,
    [DailyRate] decimal(5,4) NULL,
    [GraceDays] int DEFAULT ((0)),
    [MaxRate] decimal(5,2) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_LateFeeConfigs] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'日利率（如 0.0005 表示日息万分之五）', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'DailyRate';
EXEC sp_addextendedproperty 'MS_Description', N'宽限天数', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'GraceDays';
EXEC sp_addextendedproperty 'MS_Description', N'滞纳金上限（百分比）', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'MaxRate';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'LateFeeConfigs', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 61. LateFeeConfigs_Audit - 滞纳金配置审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[LateFeeConfigs_Audit]'))
CREATE TABLE [LateFeeConfigs_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [DailyRate] decimal(18,0) NULL,
    [GraceDays] int NULL,
    [MaxRate] decimal(18,0) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 62. Menus - 菜单权限表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Menus]'))
CREATE TABLE [Menus] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [PermissionCode] nvarchar(100) NULL,
    [Path] nvarchar(200) NULL,
    [Icon] nvarchar(50) NULL,
    [ParentId] uniqueidentifier NULL,
    [SortOrder] int DEFAULT ((0)),
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Menus] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'菜单名称', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'权限代码，用于接口鉴权', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'PermissionCode';
EXEC sp_addextendedproperty 'MS_Description', N'前端路由路径', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'Path';
EXEC sp_addextendedproperty 'MS_Description', N'菜单图标类名', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'Icon';
EXEC sp_addextendedproperty 'MS_Description', N'父级ID', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'ParentId';
EXEC sp_addextendedproperty 'MS_Description', N'排序号', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'SortOrder';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'Menus', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 63. Menus_Audit - 菜单权限审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Menus_Audit]'))
CREATE TABLE [Menus_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [PermissionCode] nvarchar(100) NULL,
    [Path] nvarchar(200) NULL,
    [Icon] nvarchar(50) NULL,
    [ParentId] uniqueidentifier NULL,
    [SortOrder] int NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 64. MeterReadings - 抄表记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[MeterReadings]'))
CREATE TABLE [MeterReadings] (
    [Id] uniqueidentifier NULL,
    [ContractFeeConfigId] uniqueidentifier NULL,
    [Year] int NULL,
    [Month] int NULL,
    [PreviousReading] decimal(18,4) NULL,
    [CurrentReading] decimal(18,4) NULL,
    [Status] nvarchar(20) DEFAULT (N'Draft'),
    [IsHistorical] bit NOT NULL DEFAULT 0,
    [DueDate] date NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_MeterReadings] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'合同费用配置ID', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'ContractFeeConfigId';
EXEC sp_addextendedproperty 'MS_Description', N'抄表年份', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'Year';
EXEC sp_addextendedproperty 'MS_Description', N'抄表月份', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'Month';
EXEC sp_addextendedproperty 'MS_Description', N'上次读数', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'PreviousReading';
EXEC sp_addextendedproperty 'MS_Description', N'本次读数', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'CurrentReading';
EXEC sp_addextendedproperty 'MS_Description', N'状态（Draft草稿/Confirmed已确认）', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'MeterReadings', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 65. Notifications
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Notifications]'))
CREATE TABLE [Notifications] (
    [Id] uniqueidentifier DEFAULT (newsequentialid()),
    [UserId] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [Category] nvarchar(20) NULL,
    [Title] nvarchar(200) NULL,
    [Content] nvarchar(500) NULL,
    [ReferenceType] nvarchar(50) NULL,
    [ReferenceId] uniqueidentifier NULL,
    [IsRead] bit DEFAULT ((0)),
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'用户ID', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'UserId';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'CompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'分类', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'Category';
EXEC sp_addextendedproperty 'MS_Description', N'标题', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'Title';
EXEC sp_addextendedproperty 'MS_Description', N'内容', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'Content';
EXEC sp_addextendedproperty 'MS_Description', N'关联类型', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'ReferenceType';
EXEC sp_addextendedproperty 'MS_Description', N'关联ID', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'ReferenceId';
EXEC sp_addextendedproperty 'MS_Description', N'是否已读', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'IsRead';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Notifications', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 66. PaymentChannels - 支付通道表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[PaymentChannels]'))
CREATE TABLE [PaymentChannels] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Code] nvarchar(50) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_PaymentChannels] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'支付通道名称', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'支付通道编码', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'Code';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'PaymentChannels', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 67. PaymentChannels_Audit - 支付通道审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[PaymentChannels_Audit]'))
CREATE TABLE [PaymentChannels_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Code] nvarchar(50) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 68. ReceiptAllocations - 收款分配表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceiptAllocations]'))
CREATE TABLE [ReceiptAllocations] (
    [Id] uniqueidentifier NULL,
    [ReceiptId] uniqueidentifier NULL,
    [ReceivablePlanId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_ReceiptAllocations] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ReceiptAllocations', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'收款单ID', 'SCHEMA', 'dbo', 'TABLE', N'ReceiptAllocations', 'COLUMN', N'ReceiptId';
EXEC sp_addextendedproperty 'MS_Description', N'应收计划ID', 'SCHEMA', 'dbo', 'TABLE', N'ReceiptAllocations', 'COLUMN', N'ReceivablePlanId';
EXEC sp_addextendedproperty 'MS_Description', N'分配金额', 'SCHEMA', 'dbo', 'TABLE', N'ReceiptAllocations', 'COLUMN', N'Amount';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ReceiptAllocations', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ReceiptAllocations', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 69. ReceiptAllocations_Audit - 收款分配审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceiptAllocations_Audit]'))
CREATE TABLE [ReceiptAllocations_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ReceiptId] uniqueidentifier NULL,
    [ReceivablePlanId] uniqueidentifier NULL,
    [Amount] decimal(18,0) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 70. Receipts - 收款单表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Receipts]'))
CREATE TABLE [Receipts] (
    [Id] uniqueidentifier NULL,
    [ReceiptNo] nvarchar(100) NULL,
    [ContractId] uniqueidentifier NULL,
    [Amount] decimal(18,2) NULL,
    [ReceivedDate] date NULL,
    [PaymentChannelId] uniqueidentifier NULL,
    [ReferenceNo] nvarchar(100) NULL,
    [Status] nvarchar(20) DEFAULT (N'Pending'),
    [RowVersion] timestamp NULL,
    [RejectReason] nvarchar(MAX) NULL,
    [ConfirmedAt] datetime2 NULL,
    [ConfirmedBy] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_Receipts] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'收款单号', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'ReceiptNo';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'收款金额', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'Amount';
EXEC sp_addextendedproperty 'MS_Description', N'收款日期', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'ReceivedDate';
EXEC sp_addextendedproperty 'MS_Description', N'支付通道ID', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'PaymentChannelId';
EXEC sp_addextendedproperty 'MS_Description', N'外部参考号（银行流水号等）', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'ReferenceNo';
EXEC sp_addextendedproperty 'MS_Description', N'状态（Pending待确认/Confirmed已确认/Rejected已驳回）', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'乐观锁版本号', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'RowVersion';
EXEC sp_addextendedproperty 'MS_Description', N'确认时间', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'ConfirmedAt';
EXEC sp_addextendedproperty 'MS_Description', N'确认人', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'ConfirmedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'Receipts', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 71. Receipts_Audit - 收款单审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Receipts_Audit]'))
CREATE TABLE [Receipts_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ReceiptNo] nvarchar(100) NULL,
    [ContractId] uniqueidentifier NULL,
    [Amount] decimal(18,0) NULL,
    [ReceivedDate] date NULL,
    [PaymentChannelId] uniqueidentifier NULL,
    [ReferenceNo] nvarchar(100) NULL,
    [Status] nvarchar(20) NULL,
    [RejectReason] nvarchar(MAX) NULL,
    [ConfirmedAt] datetime2 NULL,
    [ConfirmedBy] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 72. ReceivablePlans - 应收计划表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivablePlans]'))
CREATE TABLE [ReceivablePlans] (
    [Id] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [Period] nvarchar(7) NULL,
    [Amount] decimal(18,2) NULL,
    [Received] decimal(18,2) DEFAULT ((0.0)),
    [DueDate] date NULL,
    [Status] nvarchar(20) DEFAULT (N'Pending'),
    [RowVersion] timestamp NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_ReceivablePlans] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'合同ID', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'ContractId';
EXEC sp_addextendedproperty 'MS_Description', N'费用项目ID', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'FeeCodeId';
EXEC sp_addextendedproperty 'MS_Description', N'账期（如 2026-06）', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'Period';
EXEC sp_addextendedproperty 'MS_Description', N'应收金额', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'Amount';
EXEC sp_addextendedproperty 'MS_Description', N'已收金额', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'Received';
EXEC sp_addextendedproperty 'MS_Description', N'到期日', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'DueDate';
EXEC sp_addextendedproperty 'MS_Description', N'状态（Pending/Partial/Paid/Overdue）', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'乐观锁版本号', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'RowVersion';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'ReceivablePlans', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 73. ReceivablePlans_Audit - 应收计划审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivablePlans_Audit]'))
CREATE TABLE [ReceivablePlans_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [ContractId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [Period] nvarchar(7) NULL,
    [Amount] decimal(18,0) NULL,
    [Received] decimal(18,0) NULL,
    [DueDate] date NULL,
    [Status] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 74. RenewalRequests - 续签请求表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RenewalRequests]'))
CREATE TABLE [RenewalRequests] (
    [Id] uniqueidentifier NULL,
    [OldContractId] uniqueidentifier NULL,
    [NewContractId] uniqueidentifier NULL,
    [ContractNo] nvarchar(100) NULL,
    [RenewalType] nvarchar(20) DEFAULT ('Standard'),
    [PreviousRent] decimal(18,2) NULL,
    [NewRent] decimal(18,2) NULL,
    [NewEndDate] date NULL,
    [DepositHandling] nvarchar(20) NULL,
    [OldDepositAmount] decimal(18,2) NULL,
    [NewDepositAmount] decimal(18,2) NULL,
    [MarketReferencePrice] decimal(18,2) NULL,
    [PaymentStatusCheck] bit DEFAULT ((0)),
    [Status] nvarchar(20) DEFAULT ('Draft'),
    [Remark] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_RenewalRequests] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'原合同ID', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'OldContractId';
EXEC sp_addextendedproperty 'MS_Description', N'新合同ID', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'NewContractId';
EXEC sp_addextendedproperty 'MS_Description', N'合同编号', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'ContractNo';
EXEC sp_addextendedproperty 'MS_Description', N'续签类型', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'RenewalType';
EXEC sp_addextendedproperty 'MS_Description', N'原租金', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'PreviousRent';
EXEC sp_addextendedproperty 'MS_Description', N'新租金', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'NewRent';
EXEC sp_addextendedproperty 'MS_Description', N'新到期日', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'NewEndDate';
EXEC sp_addextendedproperty 'MS_Description', N'押金处理方式', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'DepositHandling';
EXEC sp_addextendedproperty 'MS_Description', N'原押金', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'OldDepositAmount';
EXEC sp_addextendedproperty 'MS_Description', N'新押金', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'NewDepositAmount';
EXEC sp_addextendedproperty 'MS_Description', N'市场参考价', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'MarketReferencePrice';
EXEC sp_addextendedproperty 'MS_Description', N'是否检查欠费', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'PaymentStatusCheck';
EXEC sp_addextendedproperty 'MS_Description', N'状态', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'备注', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'Remark';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'RenewalRequests', 'COLUMN', N'UpdatedHostname';
GO

-- 为已有 RenewalRequests 表补充缺失列（幂等）
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RenewalRequests]') AND name='CreatedIp')
    ALTER TABLE [RenewalRequests] ADD [CreatedIp] nvarchar(50) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RenewalRequests]') AND name='CreatedHostname')
    ALTER TABLE [RenewalRequests] ADD [CreatedHostname] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RenewalRequests]') AND name='UpdatedIp')
    ALTER TABLE [RenewalRequests] ADD [UpdatedIp] nvarchar(50) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RenewalRequests]') AND name='UpdatedHostname')
    ALTER TABLE [RenewalRequests] ADD [UpdatedHostname] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RenewalRequests]') AND name='UpdatedBy')
    ALTER TABLE [RenewalRequests] ADD [UpdatedBy] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[RenewalRequests]') AND name='UpdatedAt')
    ALTER TABLE [RenewalRequests] ADD [UpdatedAt] datetime2 NULL;
GO

-- ===================================================================
-- 75. RenewalRequests_Audit - 续签请求审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RenewalRequests_Audit]'))
CREATE TABLE [RenewalRequests_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [OldContractId] uniqueidentifier NULL,
    [NewContractId] uniqueidentifier NULL,
    [ContractNo] nvarchar(100) NULL,
    [RenewalType] nvarchar(20) NULL,
    [PreviousRent] decimal(18,0) NULL,
    [NewRent] decimal(18,0) NULL,
    [NewEndDate] date NULL,
    [DepositHandling] nvarchar(20) NULL,
    [OldDepositAmount] decimal(18,0) NULL,
    [NewDepositAmount] decimal(18,0) NULL,
    [MarketReferencePrice] decimal(18,0) NULL,
    [PaymentStatusCheck] bit NULL,
    [Status] nvarchar(20) NULL,
    [Remark] nvarchar(500) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 76. RoleMenus - 角色菜单关联表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoleMenus]'))
CREATE TABLE [RoleMenus] (
    [Id] uniqueidentifier NULL,
    [RoleId] uniqueidentifier NULL,
    [MenuId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_RoleMenus] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'RoleMenus', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'角色ID', 'SCHEMA', 'dbo', 'TABLE', N'RoleMenus', 'COLUMN', N'RoleId';
EXEC sp_addextendedproperty 'MS_Description', N'菜单ID', 'SCHEMA', 'dbo', 'TABLE', N'RoleMenus', 'COLUMN', N'MenuId';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'RoleMenus', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'RoleMenus', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 77. RoleMenus_Audit - 角色菜单关联审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoleMenus_Audit]'))
CREATE TABLE [RoleMenus_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [RoleId] uniqueidentifier NULL,
    [MenuId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 78. Roles - 角色表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Roles]'))
CREATE TABLE [Roles] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Code] nvarchar(50) NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'角色名称', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'角色编码（如 Admin/OpsSupervisor）', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'Code';
EXEC sp_addextendedproperty 'MS_Description', N'角色描述', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'Roles', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 79. Roles_Audit - 角色审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Roles_Audit]'))
CREATE TABLE [Roles_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Code] nvarchar(50) NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 80. RoomFeeDefaults_Audit
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomFeeDefaults_Audit]'))
CREATE TABLE [RoomFeeDefaults_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [RoomId] uniqueidentifier NULL,
    [FeeCodeId] uniqueidentifier NULL,
    [Amount] decimal(18,0) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 81. RoomPricingStandards - 房间定价标准表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomPricingStandards]'))
CREATE TABLE [RoomPricingStandards] (
    [Id] uniqueidentifier NULL,
    [RoomTypeId] uniqueidentifier NULL,
    [FloorLevelBandId] uniqueidentifier NULL,
    [RentAmount] decimal(18,2) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_RoomPricingStandards] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'房型ID', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'RoomTypeId';
EXEC sp_addextendedproperty 'MS_Description', N'楼层级别ID', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'FloorLevelBandId';
EXEC sp_addextendedproperty 'MS_Description', N'标准租金', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'RentAmount';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'RoomPricingStandards', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 82. RoomPricingStandards_Audit - 房间定价标准审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomPricingStandards_Audit]'))
CREATE TABLE [RoomPricingStandards_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [RoomTypeId] uniqueidentifier NULL,
    [FloorLevelBandId] uniqueidentifier NULL,
    [RentAmount] decimal(18,0) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 83. Rooms_Audit
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Rooms_Audit]'))
CREATE TABLE [Rooms_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [BuildingId] uniqueidentifier NULL,
    [FloorId] uniqueidentifier NULL,
    [RoomNo] nvarchar(50) NULL,
    [FullCode] nvarchar(100) NULL,
    [RoomTypeId] uniqueidentifier NULL,
    [Area] decimal(18,0) NULL,
    [Status] nvarchar(20) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 84. RoomTypes - 房型表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomTypes]'))
CREATE TABLE [RoomTypes] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_RoomTypes] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'房型名称（整租/合租等）', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'房型描述', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'RoomTypes', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 85. RoomTypes_Audit - 房型审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomTypes_Audit]'))
CREATE TABLE [RoomTypes_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 86. ScheduledTaskLogs - 调度任务日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ScheduledTaskLogs]'))
CREATE TABLE [ScheduledTaskLogs] (
    [Id] uniqueidentifier NULL,
    [TaskName] nvarchar(200) NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [Status] nvarchar(20) DEFAULT (N'Pending'),
    [ErrorMessage] nvarchar(2000) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_ScheduledTaskLogs] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'任务名称', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'TaskName';
EXEC sp_addextendedproperty 'MS_Description', N'开始时间', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'StartedAt';
EXEC sp_addextendedproperty 'MS_Description', N'完成时间', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'CompletedAt';
EXEC sp_addextendedproperty 'MS_Description', N'执行状态（Pending/Running/Completed/Failed）', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'错误信息', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'ErrorMessage';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'ScheduledTaskLogs', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 87. TaskLogs - 任务执行日志表（替换旧 ScheduledTaskLogs）
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaskLogs]'))
CREATE TABLE [TaskLogs] (
    [Id]              uniqueidentifier   NOT NULL,
    [TaskName]        nvarchar(50)       NOT NULL,
    [CompanyId]       uniqueidentifier   NOT NULL,
    [ContractId]      uniqueidentifier   NULL,
    [TargetMonth]     nvarchar(7)        NOT NULL,
    [TriggerType]     nvarchar(20)       NOT NULL DEFAULT 'Scheduled',
    [RunMode]         nvarchar(20)       NOT NULL DEFAULT 'Execute',
    [Status]          nvarchar(20)       NOT NULL DEFAULT 'Running',
    [Params]          nvarchar(max)      NULL,
    [StartedAt]       datetime2          NOT NULL,
    [CompletedAt]     datetime2          NULL,
    [TotalDurationMs] int                NULL,
    [TotalCount]      int                NULL,
    [SuccessCount]    int                NULL,
    [FailCount]       int                NULL,
    [WarningCount]    int                NULL,
    [Summary]         nvarchar(500)      NULL,
    [HeartbeatAt]     datetime2          NULL,
    [ResultData]      nvarchar(max)      NULL,
    [ErrorMessage]    nvarchar(2000)     NULL,
    [CreatedAt]       datetime2          NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_TaskLogs] PRIMARY KEY ([Id])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_TaskLogs_Lock')
CREATE UNIQUE INDEX [IX_TaskLogs_Lock]
    ON [TaskLogs]([TaskName], [CompanyId], [TargetMonth])
    WHERE [Status] IN ('Running', 'RollingBack');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_TaskLogs_Heartbeat')
CREATE INDEX [IX_TaskLogs_Heartbeat] ON [TaskLogs]([HeartbeatAt])
    WHERE [Status] IN ('Running', 'RollingBack');
GO

-- ===================================================================
-- 88. TaskStepLogs - 任务步骤日志表（★新增）
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaskStepLogs]'))
CREATE TABLE [TaskStepLogs] (
    [Id]              uniqueidentifier   NOT NULL,
    [TaskLogId]       uniqueidentifier   NOT NULL,
    [StepName]        nvarchar(50)       NOT NULL,
    [StepDisplayName] nvarchar(100)      NOT NULL,
    [ParentId]        uniqueidentifier   NULL,
    [SortOrder]       int                NOT NULL DEFAULT 0,
    [Status]          nvarchar(20)       NOT NULL DEFAULT 'Running',
    [StartedAt]       datetime2          NOT NULL,
    [CompletedAt]     datetime2          NULL,
    [DurationMs]      int                NULL,
    [AffectedCount]   int                NULL,
    [Message]         nvarchar(500)      NULL,
    [ErrorMessage]    nvarchar(2000)     NULL,
    CONSTRAINT [PK_TaskStepLogs] PRIMARY KEY ([Id])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_TaskStepLogs_LogId')
CREATE INDEX [IX_TaskStepLogs_LogId] ON [TaskStepLogs]([TaskLogId]);
GO

-- ===================================================================
-- 89. SystemLogs - 系统日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[SystemLogs]'))
CREATE TABLE [SystemLogs] (
    [Id] uniqueidentifier DEFAULT (newsequentialid()),
    [Level] nvarchar(20) NULL,
    [Message] nvarchar(MAX) NULL,
    [Exception] nvarchar(MAX) NULL,
    [Source] nvarchar(200) NULL,
    [Path] nvarchar(500) NULL,
    [Method] nvarchar(10) NULL,
    [IpAddress] nvarchar(50) NULL,
    [UserAgent] nvarchar(500) NULL,
    [UserId] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [UserDisplayName] nvarchar(100) NULL,
    CONSTRAINT [PK_SystemLogs] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'层级', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'Level';
EXEC sp_addextendedproperty 'MS_Description', N'日志消息', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'Message';
EXEC sp_addextendedproperty 'MS_Description', N'异常信息', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'Exception';
EXEC sp_addextendedproperty 'MS_Description', N'来源', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'Source';
EXEC sp_addextendedproperty 'MS_Description', N'路径', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'Path';
EXEC sp_addextendedproperty 'MS_Description', N'方法', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'Method';
EXEC sp_addextendedproperty 'MS_Description', N'客户端IP', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'IpAddress';
EXEC sp_addextendedproperty 'MS_Description', N'用户代理', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'UserAgent';
EXEC sp_addextendedproperty 'MS_Description', N'用户ID', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'UserId';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'用户显示名', 'SCHEMA', 'dbo', 'TABLE', N'SystemLogs', 'COLUMN', N'UserDisplayName';
GO

-- ===================================================================
-- 88. TaxRateConfigs - 税率配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaxRateConfigs]'))
CREATE TABLE [TaxRateConfigs] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Rate] decimal(5,2) NULL,
    [EffectiveDate] date NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_TaxRateConfigs] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'税率名称', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'税率（百分比）', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'Rate';
EXEC sp_addextendedproperty 'MS_Description', N'生效日期', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'EffectiveDate';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'TaxRateConfigs', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 89. TaxRateConfigs_Audit - 税率配置审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaxRateConfigs_Audit]'))
CREATE TABLE [TaxRateConfigs_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [Rate] decimal(18,0) NULL,
    [EffectiveDate] date NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 90. Tenants - 租客表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Tenants]'))
CREATE TABLE [Tenants] (
    [Id] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [IdCard] nvarchar(18) NULL,
    [Phone] nvarchar(20) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'租客姓名', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'Name';
EXEC sp_addextendedproperty 'MS_Description', N'身份证号', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'IdCard';
EXEC sp_addextendedproperty 'MS_Description', N'联系电话', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'Phone';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'UpdatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'公司ID', 'SCHEMA', 'dbo', 'TABLE', N'Tenants', 'COLUMN', N'CompanyId';
GO

-- ===================================================================
-- 91. Tenants_Audit - 租客审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Tenants_Audit]'))
CREATE TABLE [Tenants_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Name] nvarchar(100) NULL,
    [IdCard] nvarchar(18) NULL,
    [Phone] nvarchar(20) NULL,
    [IsActive] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 92. UserLandlordScopes_Audit
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[UserLandlordScopes_Audit]'))
CREATE TABLE [UserLandlordScopes_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [UserId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CompanyId] uniqueidentifier DEFAULT 
);
GO

GO

-- ===================================================================
-- 93. UserRoles - 用户角色关联表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[UserRoles]'))
CREATE TABLE [UserRoles] (
    [Id] uniqueidentifier NULL,
    [UserId] uniqueidentifier NULL,
    [RoleId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'UserRoles', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'用户ID', 'SCHEMA', 'dbo', 'TABLE', N'UserRoles', 'COLUMN', N'UserId';
EXEC sp_addextendedproperty 'MS_Description', N'角色ID', 'SCHEMA', 'dbo', 'TABLE', N'UserRoles', 'COLUMN', N'RoleId';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'UserRoles', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'UserRoles', 'COLUMN', N'CreatedAt';
GO

-- ===================================================================
-- 94. UserRoles_Audit - 用户角色关联审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[UserRoles_Audit]'))
CREATE TABLE [UserRoles_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [UserId] uniqueidentifier NULL,
    [RoleId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT 
);
GO

GO

-- ===================================================================
-- 95. Users - 用户表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Users]'))
CREATE TABLE [Users] (
    [Id] uniqueidentifier NULL,
    [Username] nvarchar(50) NULL,
    [PasswordHash] nvarchar(200) NULL,
    [DisplayName] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Email] nvarchar(200) NULL,
    [IsActive] bit DEFAULT (CONVERT([bit],(1))),
    [HomeCompanyId] uniqueidentifier NULL,
    [DefaultCompanyId] uniqueidentifier NULL,
    [IsSuperAdmin] bit DEFAULT (CONVERT([bit],(0))),
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

-- 为已有 Users 表补充缺失列（幂等）
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Users]') AND name='DefaultCompanyId')
    ALTER TABLE [Users] ADD [DefaultCompanyId] uniqueidentifier NULL;
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'登录用户名，全局唯一', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'Username';
EXEC sp_addextendedproperty 'MS_Description', N'密码哈希值', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'PasswordHash';
EXEC sp_addextendedproperty 'MS_Description', N'用户显示名称/姓名', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'DisplayName';
EXEC sp_addextendedproperty 'MS_Description', N'手机号', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'Phone';
EXEC sp_addextendedproperty 'MS_Description', N'电子邮箱', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'Email';
EXEC sp_addextendedproperty 'MS_Description', N'是否启用', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'IsActive';
EXEC sp_addextendedproperty 'MS_Description', N'所属公司ID', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'HomeCompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'当前默认公司ID', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'DefaultCompanyId';
EXEC sp_addextendedproperty 'MS_Description', N'是否为超级管理员', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'IsSuperAdmin';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'Users', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 96. Users_Audit - 用户审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Users_Audit]'))
CREATE TABLE [Users_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [Username] nvarchar(50) NULL,
    [PasswordHash] nvarchar(200) NULL,
    [DisplayName] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Email] nvarchar(200) NULL,
    [IsActive] bit NULL,
    [HomeCompanyId] uniqueidentifier NULL,
    [IsSuperAdmin] bit NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

-- ===================================================================
-- 97. Vouchers - 凭证表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Vouchers]'))
CREATE TABLE [Vouchers] (
    [Id] uniqueidentifier NULL,
    [VoucherNo] nvarchar(100) NULL,
    [VoucherDate] date NULL,
    [Description] nvarchar(500) NULL,
    [Status] nvarchar(20) DEFAULT (N'Draft'),
    [IsHistorical] bit NOT NULL DEFAULT 0,
    [DueDate] date NULL,
    [SourceEntityId] uniqueidentifier NULL,
    [SourceEntityType] nvarchar(50) NULL,
    [RowVersion] timestamp NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 DEFAULT (getutcdate()),
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) NULL,
    CONSTRAINT [PK_Vouchers] PRIMARY KEY ([Id])
);
GO

EXEC sp_addextendedproperty 'MS_Description', N'ID', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'Id';
EXEC sp_addextendedproperty 'MS_Description', N'凭证编号', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'VoucherNo';
EXEC sp_addextendedproperty 'MS_Description', N'凭证日期', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'VoucherDate';
EXEC sp_addextendedproperty 'MS_Description', N'凭证摘要', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'Description';
EXEC sp_addextendedproperty 'MS_Description', N'凭证状态（Draft草稿/Posted已过账/Audited已审核）', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'Status';
EXEC sp_addextendedproperty 'MS_Description', N'来源实体ID', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'SourceEntityId';
EXEC sp_addextendedproperty 'MS_Description', N'来源业务实体类型', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'SourceEntityType';
EXEC sp_addextendedproperty 'MS_Description', N'乐观锁版本号', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'RowVersion';
EXEC sp_addextendedproperty 'MS_Description', N'创建人', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'CreatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'创建时间', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'CreatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'创建IP', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'CreatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'创建主机名', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'CreatedHostname';
EXEC sp_addextendedproperty 'MS_Description', N'更新人', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'UpdatedBy';
EXEC sp_addextendedproperty 'MS_Description', N'更新时间', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'UpdatedAt';
EXEC sp_addextendedproperty 'MS_Description', N'更新IP', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'UpdatedIp';
EXEC sp_addextendedproperty 'MS_Description', N'更新主机名', 'SCHEMA', 'dbo', 'TABLE', N'Vouchers', 'COLUMN', N'UpdatedHostname';
GO

-- ===================================================================
-- 98. Vouchers_Audit - 凭证审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Vouchers_Audit]'))
CREATE TABLE [Vouchers_Audit] (
    [Id] nvarchar(50) NULL,
    [AuditAction] nvarchar(20) NULL,
    [AuditVersionNo] int NULL,
    [AuditChangedAt] datetime2 NULL,
    [AuditChangedBy] uniqueidentifier NULL,
    [VoucherNo] nvarchar(100) NULL,
    [VoucherDate] date NULL,
    [Description] nvarchar(500) NULL,
    [Status] nvarchar(20) NULL,
    [SourceEntityId] uniqueidentifier NULL,
    [SourceEntityType] nvarchar(50) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NULL,
    [CreatedIp] nvarchar(50) NULL,
    [CreatedHostname] nvarchar(100) NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedIp] nvarchar(50) NULL,
    [UpdatedHostname] nvarchar(100) DEFAULT 
);
GO

GO

