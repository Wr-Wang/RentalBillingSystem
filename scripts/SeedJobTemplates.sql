-- ===================================================================
-- 任务模板种子数据
-- 基于系统预置的 4 类定时任务
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'MonthlyFeeBill')
INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (NEWID(),'MonthlyFeeBill',N'📅 月度应收生成','月度应收','0 0 8 25 * ?',N'每月25日 08:00 生成月度应收账单','Calendar','Billing',1,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'LateFeeCalc')
INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (NEWID(),'LateFeeCalc',N'💰 滞纳金计算','滞纳金','0 0 2 * * ?',N'每天 02:00 计算滞纳金','Money','Billing',2,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'AutoRenew')
INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (NEWID(),'AutoRenew',N'🔄 自动续签','续签','0 0 0 * * ?',N'每天 00:00 自动续签到期的合同','RefreshRight','Contract',3,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'Collection')
INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (NEWID(),'Collection',N'📢 催缴任务','催缴','0 0 9 * * ?',N'每天 09:00 执行催缴任务','Bell','Collection',4,1,@SysUserId,@Now);

PRINT N'任务模板种子数据初始化完成！';
GO
