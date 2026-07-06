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
