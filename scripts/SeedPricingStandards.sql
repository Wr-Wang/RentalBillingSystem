-- ===================================================================
-- 定价标准初始化数据
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

-- 获取房型 ID
DECLARE @StudioId uniqueidentifier = (SELECT [Id] FROM [RoomTypes] WHERE [Name] = N'开间/单间');
DECLARE @OneBrId uniqueidentifier = (SELECT [Id] FROM [RoomTypes] WHERE [Name] = N'一室一厅');
DECLARE @TwoBrId uniqueidentifier = (SELECT [Id] FROM [RoomTypes] WHERE [Name] = N'两室一厅');
DECLARE @ThreeBrId uniqueidentifier = (SELECT [Id] FROM [RoomTypes] WHERE [Name] = N'三室一厅');

-- 楼层级别（硬编码ID，需要根据实际数据调整或通过 FloorLevelBands 表查询）
-- 注意：如果 FloorLevelBands 表没有种子数据，需要先插入
IF NOT EXISTS (SELECT 1 FROM [FloorLevelBands])
BEGIN
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111011',N'低层',1,5,N'1-5层',@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111012',N'中层',6,12,N'6-12层',@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111013',N'高层',13,17,N'13-17层',@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111014',N'顶层',18,99,N'顶层',@SysUserId,@Now);
END

DECLARE @LowId uniqueidentifier = (SELECT [Id] FROM [FloorLevelBands] WHERE [Name] = N'低层');
DECLARE @MidId uniqueidentifier = (SELECT [Id] FROM [FloorLevelBands] WHERE [Name] = N'中层');
DECLARE @HighId uniqueidentifier = (SELECT [Id] FROM [FloorLevelBands] WHERE [Name] = N'高层');

-- 插入定价数据（房型 × 楼层级别 × 租金）
IF @TwoBrId IS NOT NULL AND @LowId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @TwoBrId AND [FloorLevelBandId] = @LowId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111021',@TwoBrId,@LowId,4800,@Cid,@SysUserId,@Now);
IF @TwoBrId IS NOT NULL AND @MidId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @TwoBrId AND [FloorLevelBandId] = @MidId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111022',@TwoBrId,@MidId,5200,@Cid,@SysUserId,@Now);
IF @TwoBrId IS NOT NULL AND @HighId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @TwoBrId AND [FloorLevelBandId] = @HighId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111023',@TwoBrId,@HighId,5600,@Cid,@SysUserId,@Now);

IF @OneBrId IS NOT NULL AND @LowId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @OneBrId AND [FloorLevelBandId] = @LowId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111024',@OneBrId,@LowId,3200,@Cid,@SysUserId,@Now);
IF @OneBrId IS NOT NULL AND @MidId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @OneBrId AND [FloorLevelBandId] = @MidId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111025',@OneBrId,@MidId,3500,@Cid,@SysUserId,@Now);
IF @OneBrId IS NOT NULL AND @HighId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @OneBrId AND [FloorLevelBandId] = @HighId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111026',@OneBrId,@HighId,3800,@Cid,@SysUserId,@Now);

IF @StudioId IS NOT NULL AND @LowId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @StudioId AND [FloorLevelBandId] = @LowId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111027',@StudioId,@LowId,2500,@Cid,@SysUserId,@Now);
IF @StudioId IS NOT NULL AND @MidId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @StudioId AND [FloorLevelBandId] = @MidId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111028',@StudioId,@MidId,2800,@Cid,@SysUserId,@Now);

PRINT N'定价标准数据初始化完成！';
GO
