-- ===================================================================
-- 调度任务初始化数据
-- 1. 任务模板（JobTemplates）
-- 2. 公司任务实例（JobSchedules） — 简化调度：ScheduleType + Hour + Minute
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

-- ===== 1. 任务模板（不存在时插入） =====
IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'MonthlyFeeBill')
INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[DefaultDayOfMonth],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (NEWID(),'MonthlyFeeBill',N'📅 月度应收生成','月度应收','Monthly',8,0,25,N'每月25日 08:00 生成月度应收账单','Calendar','Billing',1,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'LateFeeCalc')
INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (NEWID(),'LateFeeCalc',N'💰 滞纳金计算','滞纳金','Daily',2,0,N'每天 02:00 计算滞纳金','Money','Billing',2,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'AutoRenew')
INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (NEWID(),'AutoRenew',N'🔄 自动续签','续签','Daily',0,0,N'每天 00:00 自动续签到期的合同','RefreshRight','Contract',3,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'Collection')
INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (NEWID(),'Collection',N'📢 催缴任务','催缴','Daily',9,0,N'每天 09:00 执行催缴任务','Bell','Collection',4,1,@SysUserId,@Now);

-- ===== 2. 公司任务实例（从模板创建） =====
DELETE FROM [JobSchedules] WHERE [Id] IN (
    'B1111111-1111-1111-1111-111111111051',
    'B1111111-1111-1111-1111-111111111052',
    'B1111111-1111-1111-1111-111111111053',
    'B1111111-1111-1111-1111-111111111054'
);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = N'📅 月度应收生成' AND [CompanyId] = @Cid)
INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[DayOfMonth],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
VALUES (NEWID(),N'📅 月度应收生成','Monthly',8,0,25,N'每月25日 08:00 生成月度应收账单',1,@Cid,'MonthlyFeeBill',@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = N'💰 滞纳金计算' AND [CompanyId] = @Cid)
INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
VALUES (NEWID(),N'💰 滞纳金计算','Daily',2,0,N'每天 02:00 计算滞纳金',1,@Cid,'LateFeeCalc',@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = N'🔄 自动续签' AND [CompanyId] = @Cid)
INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
VALUES (NEWID(),N'🔄 自动续签','Daily',0,0,N'每天 00:00 自动续签到期的合同',1,@Cid,'AutoRenew',@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = N'📢 催缴任务' AND [CompanyId] = @Cid)
INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
VALUES (NEWID(),N'📢 催缴任务','Daily',9,0,N'每天 09:00 执行催缴任务',1,@Cid,'Collection',@SysUserId,@Now);

PRINT N'调度任务初始化完成！共创建 4 条模板 + 4 条公司实例。';
GO
