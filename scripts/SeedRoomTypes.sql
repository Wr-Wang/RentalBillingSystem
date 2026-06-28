-- ===================================================================
-- 房型测试数据
-- ===================================================================

DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'开间/单间')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111001',N'开间/单间',N'开放式一体的居住空间',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'一室一厅')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111002',N'一室一厅',N'一间卧室加独立客厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'两室一厅')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111003',N'两室一厅',N'两间卧室加独立客厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'两室两厅')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111004',N'两室两厅',N'两间卧室加独立客厅和餐厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'三室一厅')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111005',N'三室一厅',N'三间卧室加独立客厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'三室两厅')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111006',N'三室两厅',N'三间卧室加独立客厅和餐厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'四室及以上')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111007',N'四室及以上',N'四间及以上卧室',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'主卧')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111008',N'主卧',N'合租主卧（带独立卫生间）',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'次卧')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111009',N'次卧',N'合租次卧（共用卫生间）',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'公寓')
INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('D1111111-1111-1111-1111-111111111010',N'公寓',N'酒店式公寓/服务式公寓',1,@SysUserId,@Now);

PRINT N'房型数据初始化完成！';
GO
