-- ===================================================================
-- 滞纳金配置种子数据（兼容 LandlordId/CompanyId 列名）
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

-- 统一列名：将 LandlordId 重命名为 CompanyId（幂等）
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LateFeeConfigs') AND name = 'LandlordId')
    EXEC sp_rename 'LateFeeConfigs.LandlordId', 'CompanyId', 'COLUMN';

IF NOT EXISTS (SELECT 1 FROM [LateFeeConfigs] WHERE [CompanyId] = @Cid AND [IsActive] = 1)
INSERT INTO [LateFeeConfigs] ([Id],[DailyRate],[GraceDays],[MaxRate],[MinAmount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES (NEWID(),0.0005,3,100.00,1.00,'2026-01-01',1,@Cid,@SysUserId,@Now);
GO
