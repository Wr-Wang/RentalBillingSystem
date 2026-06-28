-- ===================================================================
-- 税率配置初始化数据
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

IF NOT EXISTS (SELECT 1 FROM [TaxRateConfigs] WHERE [Name] = N'增值税普通发票' AND [Rate] = 6)
INSERT INTO [TaxRateConfigs] ([Id],[Name],[Rate],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('B1111111-1111-1111-1111-111111111031',N'增值税普通发票',6,'2026-01-01',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [TaxRateConfigs] WHERE [Name] = N'增值税专用发票' AND [Rate] = 9)
INSERT INTO [TaxRateConfigs] ([Id],[Name],[Rate],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('B1111111-1111-1111-1111-111111111032',N'增值税专用发票',9,'2026-01-01',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [TaxRateConfigs] WHERE [Name] = N'小规模纳税人' AND [Rate] = 3)
INSERT INTO [TaxRateConfigs] ([Id],[Name],[Rate],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('B1111111-1111-1111-1111-111111111033',N'小规模纳税人',3,'2026-01-01',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [TaxRateConfigs] WHERE [Name] = N'简易征收' AND [Rate] = 5)
INSERT INTO [TaxRateConfigs] ([Id],[Name],[Rate],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('B1111111-1111-1111-1111-111111111034',N'简易征收',5,'2026-01-01',1,@Cid,@SysUserId,@Now);

PRINT N'税率数据初始化完成！';
GO
