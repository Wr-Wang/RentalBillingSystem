-- ===================================================================
-- 审批类型配置初始化数据
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'BATCH_IMPORT_ROOMS')
INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F1111111-1111-1111-1111-111111111001',N'批量导入房屋','BATCH_IMPORT_ROOMS',N'批量导入房屋数据需要审批',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_CREATE')
INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F1111111-1111-1111-1111-111111111002',N'新建合同','CONTRACT_CREATE',N'新建租赁合同需要审批',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_TERMINATE')
INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F1111111-1111-1111-1111-111111111003',N'提前解约','CONTRACT_TERMINATE',N'合同提前终止需要审批',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'RECEIPT_REVERSE')
INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F1111111-1111-1111-1111-111111111004',N'收款冲销','RECEIPT_REVERSE',N'收款冲销操作需要审批',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'DISCOUNT')
INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F1111111-1111-1111-1111-111111111005',N'应收减免','DISCOUNT',N'应收费用减免需要审批',1,@Cid,@SysUserId,@Now);

-- ==================== 审批级别数据 ====================
-- 动态获取角色 ID
DECLARE @R_OpsSup uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'OpsSupervisor');
DECLARE @R_DeptMgr uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'DeptManager');
DECLARE @R_FinSup uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'FinanceSupervisor');
DECLARE @R_FinDir uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'FinanceDirector');
DECLARE @R_GenMgr uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'GeneralManager');

-- 批量导入房屋：2级（运营主管→部门经理）
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111001' AND [Level] = 1)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111001','F1111111-1111-1111-1111-111111111001',1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111001' AND [Level] = 2)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111002','F1111111-1111-1111-1111-111111111001',2,@R_DeptMgr,NULL,NULL,@Cid,@SysUserId,@Now);

-- 新建合同：1级（运营主管）
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111002' AND [Level] = 1)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111003','F1111111-1111-1111-1111-111111111002',1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

-- 提前解约：3级（运营主管≤5k → 部门经理5k-50k → 总经理≥50k）
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111003' AND [Level] = 1)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111004','F1111111-1111-1111-1111-111111111003',1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111003' AND [Level] = 2)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111005','F1111111-1111-1111-1111-111111111003',2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111003' AND [Level] = 3)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111006','F1111111-1111-1111-1111-111111111003',3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

-- 收款冲销：2级（财务主管→财务总监）
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111004' AND [Level] = 1)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111007','F1111111-1111-1111-1111-111111111004',1,@R_FinSup,0,50000,@Cid,@SysUserId,@Now);
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111004' AND [Level] = 2)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111008','F1111111-1111-1111-1111-111111111004',2,@R_FinDir,50000,99999999,@Cid,@SysUserId,@Now);

-- 应收减免：3级（财务主管≤5k → 部门经理5k-50k → 总经理≥50k）
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111005' AND [Level] = 1)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111009','F1111111-1111-1111-1111-111111111005',1,@R_FinSup,0,5000,@Cid,@SysUserId,@Now);
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111005' AND [Level] = 2)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111010','F1111111-1111-1111-1111-111111111005',2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);
IF NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = 'F1111111-1111-1111-1111-111111111005' AND [Level] = 3)
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('F2111111-1111-1111-1111-111111111011','F1111111-1111-1111-1111-111111111005',3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

PRINT N'审批类型及级别数据初始化完成！';
GO
