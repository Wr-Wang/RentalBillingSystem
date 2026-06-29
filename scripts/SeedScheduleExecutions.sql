-- ===================================================================
-- 排期执行实例种子数据（JobScheduleExecutions）
-- 为 4 个公司任务生成未来 3 个月的排期，含部分调整示例
-- 通过 JobName 动态查找 JobScheduleId，不依赖固定 GUID
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

-- 从 JobSchedules 动态查找各任务的 ID
DECLARE @MonthlyFeeBillId uniqueidentifier;
DECLARE @LateFeeCalcId    uniqueidentifier;
DECLARE @AutoRenewId      uniqueidentifier;
DECLARE @CollectionId     uniqueidentifier;

SELECT @MonthlyFeeBillId = [Id] FROM [JobSchedules] WHERE [JobName] = N'📅 月度应收生成' AND [CompanyId] = @Cid;
SELECT @LateFeeCalcId    = [Id] FROM [JobSchedules] WHERE [JobName] = N'💰 滞纳金计算' AND [CompanyId] = @Cid;
SELECT @AutoRenewId      = [Id] FROM [JobSchedules] WHERE [JobName] = N'🔄 自动续签' AND [CompanyId] = @Cid;
SELECT @CollectionId     = [Id] FROM [JobSchedules] WHERE [JobName] = N'📢 催缴任务' AND [CompanyId] = @Cid;

-- 如果任务不存在则跳过
IF @MonthlyFeeBillId IS NULL PRINT N'⚠️ 未找到任务：📅 月度应收生成，跳过排期生成';
IF @LateFeeCalcId IS NULL    PRINT N'⚠️ 未找到任务：💰 滞纳金计算，跳过排期生成';
IF @AutoRenewId IS NULL      PRINT N'⚠️ 未找到任务：🔄 自动续签，跳过排期生成';
IF @CollectionId IS NULL     PRINT N'⚠️ 未找到任务：📢 催缴任务，跳过排期生成';

-- ===== 1. 📅 月度应收生成 =====
-- Cron: 每月 25 日 08:00
-- 本月 25 日是周六 → 已调整到 24 日（周五）
IF @MonthlyFeeBillId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @MonthlyFeeBillId AND [Month] = '2026-07' AND [IsCustom] = 0)
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@MonthlyFeeBillId,@Cid,'2026-07-24T08:00:00','2026-07-25T08:00:00','2026-07','Pending',N'25日逢周六，提前至24日',1,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @MonthlyFeeBillId AND [Month] = '2026-08' AND [IsCustom] = 0)
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@MonthlyFeeBillId,@Cid,'2026-08-25T08:00:00','2026-08-25T08:00:00','2026-08','Pending',N'默认',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @MonthlyFeeBillId AND [Month] = '2026-09' AND [IsCustom] = 0)
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@MonthlyFeeBillId,@Cid,'2026-09-25T08:00:00','2026-09-25T08:00:00','2026-09','Pending',N'默认',0,0,@SysUserId,@Now);

-- 自定义排期：月中临时加跑
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @MonthlyFeeBillId AND [Month] = '2026-07' AND [IsCustom] = 1)
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@MonthlyFeeBillId,@Cid,'2026-07-15T14:30:00',NULL,'2026-07','Pending',N'月中临时加跑一次核对',1,1,@SysUserId,@Now);
END

-- ===== 2. 💰 滞纳金计算 =====
IF @LateFeeCalcId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @LateFeeCalcId AND [Month] = '2026-07')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@LateFeeCalcId,@Cid,'2026-07-01T02:00:00','2026-07-01T02:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @LateFeeCalcId AND [Month] = '2026-08')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@LateFeeCalcId,@Cid,'2026-08-01T02:00:00','2026-08-01T02:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @LateFeeCalcId AND [Month] = '2026-09')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@LateFeeCalcId,@Cid,'2026-09-01T02:00:00','2026-09-01T02:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now);
END

-- ===== 3. 🔄 自动续签 =====
IF @AutoRenewId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @AutoRenewId AND [Month] = '2026-07')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@AutoRenewId,@Cid,'2026-07-01T00:00:00','2026-07-01T00:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @AutoRenewId AND [Month] = '2026-08')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@AutoRenewId,@Cid,'2026-08-01T00:00:00','2026-08-01T00:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @AutoRenewId AND [Month] = '2026-09')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@AutoRenewId,@Cid,'2026-09-01T00:00:00','2026-09-01T00:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now);
END

-- ===== 4. 📢 催缴任务 =====
IF @CollectionId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @CollectionId AND [Month] = '2026-06')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@CollectionId,@Cid,'2026-06-01T09:00:00','2026-06-01T09:00:00','2026-06','Success',N'已执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @CollectionId AND [Month] = '2026-07')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@CollectionId,@Cid,'2026-07-01T09:00:00','2026-07-01T09:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @CollectionId AND [Month] = '2026-08')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@CollectionId,@Cid,'2026-08-01T09:00:00','2026-08-01T09:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId] = @CollectionId AND [Month] = '2026-09')
INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
VALUES (NEWID(),@CollectionId,@Cid,'2026-09-01T09:00:00','2026-09-01T09:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now);
END

PRINT N'排期种子数据初始化完成！';
GO
