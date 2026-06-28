-- ===================================================================
-- 收费项目初始化数据
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'RENT')
INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('E1111111-1111-1111-1111-111111111001','RENT',N'房租费','FixedAmount',1,'Rent',1,1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'WATER')
INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[Unit],[SortOrder],[Category],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('E1111111-1111-1111-1111-111111111002','WATER',N'水费','MeterBased',N'元/吨',2,'Utility',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'ELECTRIC')
INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[Unit],[SortOrder],[Category],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('E1111111-1111-1111-1111-111111111003','ELECTRIC',N'电费','MeterBased',N'元/度',3,'Utility',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'GAS')
INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[Unit],[SortOrder],[Category],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('E1111111-1111-1111-1111-111111111004','GAS',N'燃气费','MeterBased',N'元/方',4,'Utility',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'MANAGEMENT')
INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('E1111111-1111-1111-1111-111111111005','MANAGEMENT',N'物业管理费','FixedAmount',5,'Property',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'SANITATION')
INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('E1111111-1111-1111-1111-111111111006','SANITATION',N'卫生费','FixedAmount',6,'Property',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'INTERNET')
INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('E1111111-1111-1111-1111-111111111007','INTERNET',N'网费','FixedAmount',7,'Other',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'LATE_FEE')
INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('E1111111-1111-1111-1111-111111111008','LATE_FEE',N'滞纳金','FixedAmount',99,'Other',1,0,@Cid,@SysUserId,@Now);

PRINT N'收费项目数据初始化完成！';
GO
