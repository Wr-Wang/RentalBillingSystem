-- ===================================================================
-- 合同续签功能 — 数据库变更脚本
-- 包含：新表创建 + 现有表字段追加 + 审批类型种子数据
-- ===================================================================
-- 注意：本脚本应在 Init.sql 之后执行
-- ===================================================================

-- ===================================================================
-- 1. Contracts 表：新增续签链字段
-- ===================================================================
-- 先添加列（使用动态SQL避免在同一个批处理中引用不存在的列）
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'PreviousContractId')
BEGIN
    ALTER TABLE [Contracts] ADD [PreviousContractId] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'RenewalCount')
BEGIN
    ALTER TABLE [Contracts] ADD [RenewalCount] INT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'OriginalContractId')
BEGIN
    ALTER TABLE [Contracts] ADD [OriginalContractId] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'MarketPriceAtRenewal')
BEGIN
    ALTER TABLE [Contracts] ADD [MarketPriceAtRenewal] DECIMAL(18,2) NULL;
END
GO

-- 续签链索引（用 EXEC 避免编译时检查不存在的列）
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Contracts_PreviousContractId')
EXEC('CREATE INDEX [IX_Contracts_PreviousContractId] ON [Contracts]([PreviousContractId]) WHERE [PreviousContractId] IS NOT NULL');
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Contracts_OriginalContractId')
EXEC('CREATE INDEX [IX_Contracts_OriginalContractId] ON [Contracts]([OriginalContractId]) WHERE [OriginalContractId] IS NOT NULL');
GO

-- ===================================================================
-- 2. RenewalRequests 表：续签待审批数据
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[RenewalRequests]'))
CREATE TABLE [RenewalRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [OldContractId] UNIQUEIDENTIFIER NOT NULL,
    [NewContractId] UNIQUEIDENTIFIER NULL,             -- 审批通过后回填新合同ID
    [ContractNo] NVARCHAR(100) NOT NULL,                -- 新合同号
    [RenewalType] NVARCHAR(20) NOT NULL DEFAULT 'Standard',
    [PreviousRent] DECIMAL(18,2) NOT NULL,
    [NewRent] DECIMAL(18,2) NOT NULL,
    [NewEndDate] DATE NOT NULL,
    [DepositHandling] NVARCHAR(20) NOT NULL,            -- TRANSFER / NEW
    [OldDepositAmount] DECIMAL(18,2) NOT NULL,
    [NewDepositAmount] DECIMAL(18,2) NULL,
    [MarketReferencePrice] DECIMAL(18,2) NULL,
    [PaymentStatusCheck] BIT NOT NULL DEFAULT 0,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Draft',     -- Draft / PendingApproval / Approved / Rejected / Completed / Cancelled
    [Remark] NVARCHAR(500) NULL,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [FK_RenewalRequests_OldContract] FOREIGN KEY ([OldContractId]) REFERENCES [Contracts]([Id])
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RenewalRequests_OldContractId')
CREATE INDEX [IX_RenewalRequests_OldContractId] ON [RenewalRequests]([OldContractId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RenewalRequests_Status')
CREATE INDEX [IX_RenewalRequests_Status] ON [RenewalRequests]([Status])
  WHERE [Status] IN ('PendingApproval');

-- ===================================================================
-- 3. ChangeRequests 表：通用变更待审批主表（本轮建表，后续实现业务逻辑）
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ChangeRequests]'))
CREATE TABLE [ChangeRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ContractId] UNIQUEIDENTIFIER NOT NULL,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL,
    [ChangeType] NVARCHAR(30) NOT NULL,                -- MODIFY_RENT / MODIFY_TERMS / FEE_ADJUST / TERMINATE
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Draft',    -- Draft / PendingApproval / Approved / Rejected
    [EffectiveDate] DATE NULL,
    [Reason] NVARCHAR(500) NULL,
    [BatchId] UNIQUEIDENTIFIER NULL,
    [ApprovalRequestId] UNIQUEIDENTIFIER NULL,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [FK_ChangeRequests_Contract] FOREIGN KEY ([ContractId]) REFERENCES [Contracts]([Id])
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequests_ContractId')
CREATE INDEX [IX_ChangeRequests_ContractId] ON [ChangeRequests]([ContractId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequests_BatchId')
EXEC('CREATE INDEX [IX_ChangeRequests_BatchId] ON [ChangeRequests]([BatchId]) WHERE [BatchId] IS NOT NULL');
GO

-- ===================================================================
-- 4. ChangeRequestItems 表：通用变更明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ChangeRequestItems]'))
CREATE TABLE [ChangeRequestItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ChangeRequestId] UNIQUEIDENTIFIER NOT NULL,
    [TargetType] NVARCHAR(20) NOT NULL,                -- Contract / ContractFeeConfig / ContractTenant
    [TargetId] UNIQUEIDENTIFIER NULL,
    [FieldName] NVARCHAR(50) NOT NULL,
    [OldValue] NVARCHAR(100) NULL,
    [NewValue] NVARCHAR(100) NOT NULL,
    [OldValueDecimal] DECIMAL(18,2) NULL,
    [NewValueDecimal] DECIMAL(18,2) NULL,
    CONSTRAINT [FK_ChangeRequestItems_ChangeRequest] FOREIGN KEY ([ChangeRequestId]) REFERENCES [ChangeRequests]([Id])
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequestItems_ChangeRequestId')
CREATE INDEX [IX_ChangeRequestItems_ChangeRequestId] ON [ChangeRequestItems]([ChangeRequestId]);

-- ===================================================================
-- 5. ApprovalRequests 表：新增 ContractId 字段（统一并发控制）
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ApprovalRequests]') AND name = 'ContractId')
BEGIN
    ALTER TABLE [ApprovalRequests] ADD [ContractId] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApprovalRequests_ContractId_Status')
EXEC('CREATE INDEX [IX_ApprovalRequests_ContractId_Status] ON [ApprovalRequests]([ContractId], [Status]) WHERE [ContractId] IS NOT NULL AND [Status] = N''Pending''');
GO

-- ===================================================================
-- 6. 审批类型种子数据：CONTRACT_RENEW（合同续签）
-- ===================================================================
-- 使用与 SeedApprovalTypes.sql 相同的固定ID逻辑
-- CONTRACT_RENEW 类型ID: F1111111-1111-1111-1111-111111111007
DECLARE @Now DATETIME2 = GETUTCDATE();
DECLARE @SysUserId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid UNIQUEIDENTIFIER = 'A1111111-1111-1111-1111-111111111001';

-- 6a. 插入审批类型
IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_RENEW')
INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F1111111-1111-1111-1111-111111111007',N'合同续签','CONTRACT_RENEW',N'合同续签需要审批，根据月租金额自动路由审批级别',1,@Cid,@SysUserId,@Now);

-- 6b. 插入审批级别（3级）
DECLARE @R_OpsSup UNIQUEIDENTIFIER = (SELECT [Id] FROM [Roles] WHERE [Code] = 'OpsSupervisor');
DECLARE @R_DeptMgr UNIQUEIDENTIFIER = (SELECT [Id] FROM [Roles] WHERE [Code] = 'DeptManager');
DECLARE @R_GenMgr UNIQUEIDENTIFIER = (SELECT [Id] FROM [Roles] WHERE [Code] = 'GeneralManager');

-- 1级：运营主管（月租≤5000）
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111007' AND [Level] = 1)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111014','F1111111-1111-1111-1111-111111111007',1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);

-- 2级：部门经理（月租5000~50000）
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111007' AND [Level] = 2)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111015','F1111111-1111-1111-1111-111111111007',2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);

-- 3级：总经理（月租≥50000）
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111007' AND [Level] = 3)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111016','F1111111-1111-1111-1111-111111111007',3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

-- ===================================================================
-- 7. Contracts 表：新增 AutoRenew 字段
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Contracts]') AND name = 'AutoRenew')
BEGIN
    ALTER TABLE [Contracts] ADD [AutoRenew] BIT NOT NULL DEFAULT 1;
END
GO

-- ===================================================================
-- 8. Notifications 表：站内通知
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Notifications]'))
CREATE TABLE [Notifications] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Category] NVARCHAR(20) NOT NULL,             -- Approval / Renewal / Collection / System
    [Title] NVARCHAR(200) NOT NULL,
    [Content] NVARCHAR(500) NULL,
    [ReferenceType] NVARCHAR(50) NULL,
    [ReferenceId] UNIQUEIDENTIFIER NULL,
    [IsRead] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE())
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notifications_UserId')
CREATE INDEX [IX_Notifications_UserId] ON [Notifications]([UserId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notifications_UserId_Category')
CREATE INDEX [IX_Notifications_UserId_Category] ON [Notifications]([UserId], [Category]);
GO

-- ===================================================================
-- 9. AutoRenewConfig 表：续签策略配置
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[AutoRenewConfig]'))
CREATE TABLE [AutoRenewConfig] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL,
    [AdvanceDays] INT NOT NULL DEFAULT 7,
    [RentRule] NVARCHAR(20) NOT NULL DEFAULT 'Same',       -- Same / Percentage / MarketPrice
    [RentIncreasePercent] DECIMAL(5,2) NULL,
    [TermRule] NVARCHAR(20) NOT NULL DEFAULT 'Same',       -- Same / FixedMonths
    [TermMonths] INT NULL,
    [OverdueAction] NVARCHAR(20) NOT NULL DEFAULT 'Block', -- Block / WarnAndContinue / Skip
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] DATETIME2 NULL
);
GO

PRINT N'合同续签功能数据库变更完成！';
GO