-- ===================================================================
-- 会计科目初始化数据（标准会计科目表）
-- 通过 ParentCode 建立层级关系
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

-- 一级科目（资产类）
IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '1001')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111041','1001',N'库存现金','Debit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '1002')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111042','1002',N'银行存款','Debit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '1122')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111043','1122',N'应收账款','Debit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '1131')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111044','1131',N'其他应收款','Debit',1,0,@Cid,@SysUserId,@Now);

-- 二级科目（应收账款明细）
IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '112201')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[ParentCode],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111045','112201',N'应收房租','1122','Debit',2,1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '112202')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[ParentCode],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111046','112202',N'应收押金','1122','Debit',2,1,@Cid,@SysUserId,@Now);

-- 一级科目（负债类）
IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '2001')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111047','2001',N'短期借款','Credit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '2202')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111048','2202',N'应付账款','Credit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '2221')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111049','2221',N'应交税费','Credit',1,0,@Cid,@SysUserId,@Now);

-- 二级科目（应交税费明细）
IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '222101')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[ParentCode],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111050','222101',N'应交增值税','2221','Credit',2,1,@Cid,@SysUserId,@Now);

-- 一级科目（收入类）
IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '6001')
INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111051','6001',N'主营业务收入','Credit',1,1,@Cid,@SysUserId,@Now);

PRINT N'会计科目数据初始化完成！';
GO
