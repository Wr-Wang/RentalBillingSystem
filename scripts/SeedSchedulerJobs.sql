-- ===================================================================
-- 调度任务初始化数据
-- 基于前端 Mock 数据：MonthlyFeeBillJob / LateFeeCalcJob / AutoRenewJob / CollectionJob
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = 'MonthlyFeeBillJob')
INSERT INTO [JobSchedules] ([Id],[JobName],[CronExpression],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('B1111111-1111-1111-1111-111111111051','MonthlyFeeBillJob','0 0 8 25 * ?',N'每月25日 08:00 生成月度应收账单',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = 'LateFeeCalcJob')
INSERT INTO [JobSchedules] ([Id],[JobName],[CronExpression],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('B1111111-1111-1111-1111-111111111052','LateFeeCalcJob','0 0 2 * * ?',N'每天 02:00 计算滞纳金',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = 'AutoRenewJob')
INSERT INTO [JobSchedules] ([Id],[JobName],[CronExpression],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('B1111111-1111-1111-1111-111111111053','AutoRenewJob','0 0 0 * * ?',N'每天 00:00 自动续签到期的合同',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = 'CollectionJob')
INSERT INTO [JobSchedules] ([Id],[JobName],[CronExpression],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('B1111111-1111-1111-1111-111111111054','CollectionJob','0 0 9 * * ?',N'每天 09:00 执行催缴任务',1,@Cid,@SysUserId,@Now);

PRINT N'调度任务数据初始化完成！';
GO
