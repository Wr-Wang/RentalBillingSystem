-- ===================================================================
-- SeedBiz.sql - 业务种子数据（任务调度、催缴、房源、合同等）
-- 依赖 SeedBase.sql 必须先执行完毕
-- 所有 GUID 均通过 NEWID() 动态生成
-- ===================================================================

-- ===================================================================
-- 13. 任务模板（Cron 表达式版本）
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'MonthlyFeeBill')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[DefaultDayOfMonth],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'MonthlyFeeBill',N'月度应收生成',N'月度应收','Monthly',20,0,25,N'每月25日 20:00 生成月度应收账单','Calendar','Billing',1,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'LateFeeCalc')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[DefaultDayOfMonth],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'LateFeeCalc',N'月度结算',N'结算','Monthly',22,0,1,N'每月1日 22:00 执行月度结算：预收冲抵应收 → 计算滞纳金并生成凭证 → 标记逾期状态','Money','Billing',2,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'AutoRenew')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'AutoRenew',N'自动续签',N'续签','Daily',0,0,N'每天 00:00 自动续签到期的合同','RefreshRight','Contract',3,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'Collection')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[DefaultDayOfMonth],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'Collection',N'催缴任务',N'催缴','Monthly',21,0,15,N'每月15日 21:00 按逾期阶段自动创建催缴记录（检查逾期天数匹配催缴阶段，去重创建）','Bell','Collection',4,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'RenewalReminder')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'RenewalReminder',N'续签提醒',N'续签提醒','Daily',8,0,N'每天 08:00 提醒运营人员合同即将到期','Notifications','Renewal',5,1,@SysUserId,@Now);

PRINT N'任务模板种子数据初始化完成！';
GO

-- ===================================================================
-- 14. 调度任务 — JobName 必须匹配 IScheduledJob.JobName
--     BillJob | SettleJob | AutoRenewJob | CollectionJob | RenewalReminderJob
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

DELETE FROM [JobSchedules] WHERE [CompanyId] = @Cid AND [JobName] IN (
  'BillJob', 'SettleJob', 'AutoRenewJob', 'CollectionJob', 'RenewalReminderJob');

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName]=N'AutoRenewJob' AND [CompanyId]=@Cid)
    INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'AutoRenewJob','Daily',0,0,N'每天 00:00 自动续签到期的合同（到期前7天）',1,@Cid,'AutoRenew',@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName]=N'BillJob' AND [CompanyId]=@Cid)
    INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[DayOfMonth],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'BillJob','Monthly',20,0,25,N'每月25日 20:00 为所有生效合同生成当月应收计划',1,@Cid,'MonthlyFeeBill',@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName]=N'CollectionJob' AND [CompanyId]=@Cid)
    INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[DayOfMonth],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'CollectionJob','Monthly',21,0,15,N'每月15日 21:00 按逾期阶段自动创建催缴记录：检查所有逾期应收计划，按逾期天数匹配催缴阶段（逾1~7天→短信提醒、逾8~30天→电话催缴、逾31天以上→上门催收），同一合同同一阶段不重复创建',1,@Cid,'Collection',@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName]=N'RenewalReminderJob' AND [CompanyId]=@Cid)
    INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'RenewalReminderJob','Daily',8,0,N'每天 08:00 提醒运营人员合同即将到期（提前14天）',1,@Cid,'RenewalReminder',@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName]=N'SettleJob' AND [CompanyId]=@Cid)
    INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[DayOfMonth],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'SettleJob','Monthly',22,0,1,N'每月1日 22:00 执行月度结算：①预收账款冲抵应收账款 ②按日利率0.05%、上限90天计算逾期滞纳金并生成会计凭证(借:1122/贷:6051) ③标记逾期应收计划状态',1,@Cid,'LateFeeCalc',@SysUserId,@Now);

PRINT N'调度任务初始化完成！';
GO

-- ===================================================================
-- 15. 排期执行实例 — 预先生成几个月的待执行排期方便测试
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

DECLARE @BillJobId       uniqueidentifier; SELECT @BillJobId       = [Id] FROM [JobSchedules] WHERE [JobName]='BillJob'       AND [CompanyId]=@Cid;
DECLARE @SettleJobId     uniqueidentifier; SELECT @SettleJobId     = [Id] FROM [JobSchedules] WHERE [JobName]='SettleJob'     AND [CompanyId]=@Cid;
DECLARE @AutoRenewJobId  uniqueidentifier; SELECT @AutoRenewJobId  = [Id] FROM [JobSchedules] WHERE [JobName]='AutoRenewJob'      AND [CompanyId]=@Cid;
DECLARE @CollectionJobId uniqueidentifier; SELECT @CollectionJobId = [Id] FROM [JobSchedules] WHERE [JobName]='CollectionJob'     AND [CompanyId]=@Cid;
DECLARE @RenewalReminderJobId uniqueidentifier; SELECT @RenewalReminderJobId = [Id] FROM [JobSchedules] WHERE [JobName]='RenewalReminderJob' AND [CompanyId]=@Cid;

-- MonthlyFeeBill: 每月25日 20:00（3个月）
IF @BillJobId IS NOT NULL BEGIN
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@BillJobId,@Cid,'2026-07-25T20:00:00','2026-07-25T20:00:00','2026-07','Pending',N'默认',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@BillJobId AND [Month]='2026-07' AND [IsCustom]=0);
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@BillJobId,@Cid,'2026-08-25T20:00:00','2026-08-25T20:00:00','2026-08','Pending',N'默认',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@BillJobId AND [Month]='2026-08' AND [IsCustom]=0);
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@BillJobId,@Cid,'2026-09-25T20:00:00','2026-09-25T20:00:00','2026-09','Pending',N'默认',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@BillJobId AND [Month]='2026-09' AND [IsCustom]=0);
END

-- SettleJob: 每月1日 22:00（3个月）
IF @SettleJobId IS NOT NULL BEGIN
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@SettleJobId,@Cid,'2026-07-01T22:00:00','2026-07-01T22:00:00','2026-07','Pending',N'月度结算',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@SettleJobId AND [Month]='2026-07');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@SettleJobId,@Cid,'2026-08-01T22:00:00','2026-08-01T22:00:00','2026-08','Pending',N'月度结算',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@SettleJobId AND [Month]='2026-08');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@SettleJobId,@Cid,'2026-09-01T22:00:00','2026-09-01T22:00:00','2026-09','Pending',N'月度结算',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@SettleJobId AND [Month]='2026-09');
END

-- AutoRenew: 每天 00:00（每月1条示例）
IF @AutoRenewJobId IS NOT NULL BEGIN
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@AutoRenewJobId,@Cid,'2026-07-01T00:00:00','2026-07-01T00:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@AutoRenewJobId AND [Month]='2026-07');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@AutoRenewJobId,@Cid,'2026-08-01T00:00:00','2026-08-01T00:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@AutoRenewJobId AND [Month]='2026-08');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@AutoRenewJobId,@Cid,'2026-09-01T00:00:00','2026-09-01T00:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@AutoRenewJobId AND [Month]='2026-09');
END

-- Collection: 每月15日 21:00（含一条历史成功记录）
IF @CollectionJobId IS NOT NULL BEGIN
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@CollectionJobId,@Cid,'2026-06-15T21:00:00','2026-06-15T21:00:00','2026-06','Success',N'月度催缴',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@CollectionJobId AND [Month]='2026-06');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@CollectionJobId,@Cid,'2026-07-15T21:00:00','2026-07-15T21:00:00','2026-07','Pending',N'月度催缴',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@CollectionJobId AND [Month]='2026-07');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@CollectionJobId,@Cid,'2026-08-15T21:00:00','2026-08-15T21:00:00','2026-08','Pending',N'月度催缴',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@CollectionJobId AND [Month]='2026-08');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@CollectionJobId,@Cid,'2026-09-15T21:00:00','2026-09-15T21:00:00','2026-09','Pending',N'月度催缴',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@CollectionJobId AND [Month]='2026-09');
END

-- RenewalReminder: 每天 08:00
IF @RenewalReminderJobId IS NOT NULL BEGIN
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@RenewalReminderJobId,@Cid,'2026-07-01T08:00:00','2026-07-01T08:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@RenewalReminderJobId AND [Month]='2026-07');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@RenewalReminderJobId,@Cid,'2026-08-01T08:00:00','2026-08-01T08:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@RenewalReminderJobId AND [Month]='2026-08');
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
SELECT NEWID(),@RenewalReminderJobId,@Cid,'2026-09-01T08:00:00','2026-09-01T08:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now
WHERE NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@RenewalReminderJobId AND [Month]='2026-09');
END

PRINT N'排期种子数据初始化完成！';
GO

-- ===================================================================
-- 16. 催缴阶段
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [CollectionStages] WHERE [StageName]=N'逾期提醒' AND [CompanyId]=@Cid)
    INSERT INTO [CollectionStages] ([Id],[StageNo],[StageName],[OverdueDaysFrom],[OverdueDaysTo],[ActionType],[IsAuto],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),1,N'逾期提醒',0,7,'SMS',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [CollectionStages] WHERE [StageName]=N'电话催缴' AND [CompanyId]=@Cid)
    INSERT INTO [CollectionStages] ([Id],[StageNo],[StageName],[OverdueDaysFrom],[OverdueDaysTo],[ActionType],[IsAuto],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),2,N'电话催缴',8,15,'CALL',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [CollectionStages] WHERE [StageName]=N'上门催缴' AND [CompanyId]=@Cid)
    INSERT INTO [CollectionStages] ([Id],[StageNo],[StageName],[OverdueDaysFrom],[OverdueDaysTo],[ActionType],[IsAuto],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),3,N'上门催缴',16,30,'VISIT',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [CollectionStages] WHERE [StageName]=N'律师函' AND [CompanyId]=@Cid)
    INSERT INTO [CollectionStages] ([Id],[StageNo],[StageName],[OverdueDaysFrom],[OverdueDaysTo],[ActionType],[IsAuto],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),4,N'律师函',31,99999,'LEGAL',1,@Cid,@SysUserId,@Now);

PRINT N'催缴阶段种子数据初始化完成！';
GO

-- ===================================================================
-- 17. 滞纳金配置
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('LateFeeConfigs') AND name='LandlordId')
    EXEC sp_rename 'LateFeeConfigs.LandlordId', 'CompanyId', 'COLUMN';

IF NOT EXISTS (SELECT 1 FROM [LateFeeConfigs] WHERE [CompanyId]=@Cid AND [IsActive]=1)
INSERT INTO [LateFeeConfigs] ([Id],[DailyRate],[GracePeriodDays],[MaxPercentOfPrincipal],[MinLateFeeAmount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES (NEWID(),0.0005,3,100.00,1.00,'2026-01-01',1,@Cid,@SysUserId,@Now);
GO

-- ===================================================================
-- 18. 房源（含完整属性：面积、朝向、基础租金、房型）
-- ===================================================================
DECLARE @Sys uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Now datetime2 = GETDATE();
DECLARE @GS001Id uniqueidentifier; SELECT @GS001Id = [Id] FROM [Companies] WHERE [Code] = 'GS001';
DECLARE @Studio uniqueidentifier; SELECT @Studio = [Id] FROM [RoomTypes] WHERE [Name] = N'一室一厅';
DECLARE @TwoBr uniqueidentifier; SELECT @TwoBr = [Id] FROM [RoomTypes] WHERE [Name] = N'两室一厅';
DECLARE @ThreeBr uniqueidentifier; SELECT @ThreeBr = [Id] FROM [RoomTypes] WHERE [Name] = N'三室一厅';
DECLARE @ThreeBrTwo uniqueidentifier; SELECT @ThreeBrTwo = [Id] FROM [RoomTypes] WHERE [Name] = N'三室两厅';

DELETE FROM HousingUnits;

-- A栋（6层×2户=12套）- 上海浦东 GS001
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'1层',1,N'101',N'A栋-1层-101',@TwoBr,85,N'南',5500,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'1层',1,N'102',N'A栋-1层-102',@Studio,65,N'北',4200,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'2层',2,N'201',N'A栋-2层-201',@TwoBr,95,N'南',6000,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'2层',2,N'202',N'A栋-2层-202',@Studio,70,N'东',4500,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'3层',3,N'301',N'A栋-3层-301',@ThreeBr,125,N'南',7800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'3层',3,N'302',N'A栋-3层-302',@TwoBr,90,N'西',5800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'4层',4,N'401',N'A栋-4层-401',@ThreeBrTwo,145,N'南',8800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'4层',4,N'402',N'A栋-4层-402',@TwoBr,95,N'北',6200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'5层',5,N'501',N'A栋-5层-501',@ThreeBr,130,N'南',8200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'5层',5,N'502',N'A栋-5层-502',@TwoBr,100,N'东',6500,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'6层',6,N'601',N'A栋-6层-601',@ThreeBrTwo,150,N'南',9500,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'A栋',N'A',N'上海市浦东新区陆家嘴金融中心A座',@GS001Id,N'6层',6,N'602',N'A栋-6层-602',@ThreeBr,120,N'西',7500,'Vacant',@Sys,@Now);

-- B栋（12套）- 上海浦东 GS001
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'1层',1,N'101',N'B栋-1层-101',@Studio,60,N'南',4000,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'1层',1,N'102',N'B栋-1层-102',@TwoBr,85,N'北',5300,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'2层',2,N'201',N'B栋-2层-201',@TwoBr,95,N'南',6200,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'2层',2,N'202',N'B栋-2层-202',@Studio,65,N'东',4300,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'3层',3,N'301',N'B栋-3层-301',@ThreeBr,128,N'南',7900,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'3层',3,N'302',N'B栋-3层-302',@TwoBr,88,N'西',5600,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'4层',4,N'401',N'B栋-4层-401',@ThreeBrTwo,140,N'南',8500,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'4层',4,N'402',N'B栋-4层-402',@TwoBr,92,N'北',6000,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'5层',5,N'501',N'B栋-5层-501',@ThreeBr,135,N'南',8600,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'5层',5,N'502',N'B栋-5层-502',@TwoBr,105,N'东',6800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'6层',6,N'601',N'B栋-6层-601',@ThreeBrTwo,155,N'南',9800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'B栋',N'B',N'上海市浦东新区陆家嘴金融中心B座',@GS001Id,N'6层',6,N'602',N'B栋-6层-602',@ThreeBr,125,N'西',7800,'Vacant',@Sys,@Now);

-- C栋（12套）- 上海浦东 GS001
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'1层',1,N'101',N'C栋-1层-101',@Studio,65,N'南',4200,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'1层',1,N'102',N'C栋-1层-102',@TwoBr,82,N'北',5200,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'2层',2,N'201',N'C栋-2层-201',@TwoBr,92,N'南',6000,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'2层',2,N'202',N'C栋-2层-202',@Studio,68,N'东',4400,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'3层',3,N'301',N'C栋-3层-301',@ThreeBr,122,N'南',7600,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'3层',3,N'302',N'C栋-3层-302',@TwoBr,90,N'西',5800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'4层',4,N'401',N'C栋-4层-401',@ThreeBrTwo,138,N'南',8400,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'4层',4,N'402',N'C栋-4层-402',@TwoBr,95,N'北',6200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'5层',5,N'501',N'C栋-5层-501',@ThreeBr,130,N'南',8100,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'5层',5,N'502',N'C栋-5层-502',@TwoBr,100,N'东',6500,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'6层',6,N'601',N'C栋-6层-601',@ThreeBrTwo,148,N'南',9200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'C栋',N'C',N'上海市浦东新区陆家嘴金融中心C座',@GS001Id,N'6层',6,N'602',N'C栋-6层-602',@ThreeBr,125,N'西',7800,'Vacant',@Sys,@Now);

SELECT COUNT(*) AS TotalHousingUnits FROM HousingUnits;
GO

-- ===================================================================
-- 19. 合同（依赖房源 + 租客 + 收费项目）
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';
DECLARE @ZhangsanUserId uniqueidentifier; SELECT @ZhangsanUserId = [Id] FROM [Users] WHERE [Username] = 'zhangsan';

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code]='RENT' AND [CompanyId]=@Cid)
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'RENT',N'房租费','FixedAmount',1,'Rent','Recurring',1,1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code]='MANAGEMENT' AND [CompanyId]=@Cid)
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'MANAGEMENT',N'物业管理费','FixedAmount',5,'Property','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code]='DEPOSIT' AND [CompanyId]=@Cid)
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'DEPOSIT',N'押金','FixedAmount',6,'Deposit','OneTime',1,1,@Cid,@SysUserId,@Now);

DECLARE @TenantZS uniqueidentifier;
DECLARE @TenantLS uniqueidentifier;
DECLARE @TenantWW uniqueidentifier;

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [Name]=N'张三')
    INSERT INTO [Tenants] ([Id],[Name],[Phone],[IdentityNo],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'张三',N'13800138001',N'310101199001011234',@Cid,@SysUserId,@Now);
SELECT @TenantZS = [Id] FROM [Tenants] WHERE [Name]=N'张三';

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [Name]=N'李四')
    INSERT INTO [Tenants] ([Id],[Name],[Phone],[IdentityNo],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'李四',N'13800138002',N'310101199002022345',@Cid,@SysUserId,@Now);
SELECT @TenantLS = [Id] FROM [Tenants] WHERE [Name]=N'李四';

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [Name]=N'王五')
    INSERT INTO [Tenants] ([Id],[Name],[Phone],[IdentityNo],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'王五',N'13800138003',N'310101199003033456',@Cid,@SysUserId,@Now);
SELECT @TenantWW = [Id] FROM [Tenants] WHERE [Name]=N'王五';


DECLARE @Contract1 uniqueidentifier;
DECLARE @Contract2 uniqueidentifier;
DECLARE @Contract3 uniqueidentifier;

SET @Contract1 = NEWID();
INSERT INTO [Contracts] ([Id],[ContractNo],[RoomId],[StartDate],[EndDate],[PaymentCycle],[Status],[CompanyId],[CreatedBy],[CreatedAt])
SELECT @Contract1,'HT-2026-001',Id,'2026-01-01','2027-12-31','Monthly','Active',@Cid,@ZhangsanUserId,@Now
FROM HousingUnits WHERE UnitNo='101' AND CompanyId=@Cid ORDER BY FullCode OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;

SET @Contract2 = NEWID();
INSERT INTO [Contracts] ([Id],[ContractNo],[RoomId],[StartDate],[EndDate],[PaymentCycle],[Status],[CompanyId],[CreatedBy],[CreatedAt])
SELECT @Contract2,'HT-2026-002',Id,'2026-02-01','2027-01-31','Monthly','Active',@Cid,@ZhangsanUserId,@Now
FROM HousingUnits WHERE UnitNo='102' AND CompanyId=@Cid ORDER BY FullCode OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;

SET @Contract3 = NEWID();
INSERT INTO [Contracts] ([Id],[ContractNo],[RoomId],[StartDate],[EndDate],[PaymentCycle],[Status],[CompanyId],[CreatedBy],[CreatedAt])
SELECT @Contract3,'HT-2026-003',Id,'2026-03-15','2027-03-14','Monthly','Active',@Cid,@ZhangsanUserId,@Now
FROM HousingUnits WHERE UnitNo='201' AND CompanyId=@Cid ORDER BY FullCode OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;


IF @Contract1 IS NOT NULL AND @TenantZS IS NOT NULL
    INSERT INTO [ContractTenants] ([ContractId],[TenantId],[IsPrimary],[CreatedBy],[CreatedAt])
    VALUES (@Contract1,@TenantZS,1,@SysUserId,@Now);

IF @Contract2 IS NOT NULL AND @TenantLS IS NOT NULL
    INSERT INTO [ContractTenants] ([ContractId],[TenantId],[IsPrimary],[CreatedBy],[CreatedAt])
    VALUES (@Contract2,@TenantLS,1,@SysUserId,@Now);

IF @Contract3 IS NOT NULL AND @TenantWW IS NOT NULL
    INSERT INTO [ContractTenants] ([ContractId],[TenantId],[IsPrimary],[CreatedBy],[CreatedAt])
    VALUES (@Contract3,@TenantWW,1,@SysUserId,@Now);


DECLARE @RentFeeCodeId uniqueidentifier; SELECT @RentFeeCodeId=[Id] FROM [FeeCodes] WHERE [Code]='RENT' AND [CompanyId]=@Cid;

DECLARE @RentAmount1 DECIMAL(18,2); SELECT @RentAmount1 = hu.BaseRentAmount FROM [Contracts] c JOIN [HousingUnits] hu ON hu.Id = c.RoomId WHERE c.[Id] = @Contract1;
DECLARE @RentAmount2 DECIMAL(18,2); SELECT @RentAmount2 = hu.BaseRentAmount FROM [Contracts] c JOIN [HousingUnits] hu ON hu.Id = c.RoomId WHERE c.[Id] = @Contract2;
DECLARE @RentAmount3 DECIMAL(18,2); SELECT @RentAmount3 = hu.BaseRentAmount FROM [Contracts] c JOIN [HousingUnits] hu ON hu.Id = c.RoomId WHERE c.[Id] = @Contract3;

IF @Contract1 IS NOT NULL AND @RentFeeCodeId IS NOT NULL AND @RentAmount1 IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract1,@RentFeeCodeId,'FixedAmount',@RentAmount1,'2026-01-01',1,@Cid,@SysUserId,@Now);

IF @Contract2 IS NOT NULL AND @RentFeeCodeId IS NOT NULL AND @RentAmount2 IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract2,@RentFeeCodeId,'FixedAmount',@RentAmount2,'2026-02-01',1,@Cid,@SysUserId,@Now);

IF @Contract3 IS NOT NULL AND @RentFeeCodeId IS NOT NULL AND @RentAmount3 IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract3,@RentFeeCodeId,'FixedAmount',@RentAmount3,'2026-03-15',1,@Cid,@SysUserId,@Now);


-- ===== 押金配置（一次性收费，按2倍月租金）=====
DECLARE @DepositFeeCodeId uniqueidentifier; SELECT @DepositFeeCodeId=[Id] FROM [FeeCodes] WHERE [Code]='DEPOSIT' AND [CompanyId]=@Cid;

IF @Contract1 IS NOT NULL AND @DepositFeeCodeId IS NOT NULL AND @RentAmount1 IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract1,@DepositFeeCodeId,'FixedAmount',@RentAmount1 * 2,'2026-01-01',1,@Cid,@SysUserId,@Now);

IF @Contract2 IS NOT NULL AND @DepositFeeCodeId IS NOT NULL AND @RentAmount2 IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract2,@DepositFeeCodeId,'FixedAmount',@RentAmount2 * 2,'2026-02-01',1,@Cid,@SysUserId,@Now);

IF @Contract3 IS NOT NULL AND @DepositFeeCodeId IS NOT NULL AND @RentAmount3 IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract3,@DepositFeeCodeId,'FixedAmount',@RentAmount3 * 2,'2026-03-15',1,@Cid,@SysUserId,@Now);


PRINT 'SeedAll.sql 全量种子数据执行完成！';
GO
-- ===================================================================
-- SeedBiz.sql - 结束
-- ===================================================================
