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

PRINT N'审批类型数据初始化完成！';
GO
