-- ===================================================================
-- 支付通道初始化数据
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';

IF NOT EXISTS (SELECT 1 FROM [PaymentChannels] WHERE [Code] = 'ALIPAY')
INSERT INTO [PaymentChannels] ([Id],[Name],[Code],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111011',N'支付宝','ALIPAY',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [PaymentChannels] WHERE [Code] = 'WECHAT')
INSERT INTO [PaymentChannels] ([Id],[Name],[Code],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111012',N'微信支付','WECHAT',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [PaymentChannels] WHERE [Code] = 'BANK')
INSERT INTO [PaymentChannels] ([Id],[Name],[Code],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111013',N'银行转账','BANK',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [PaymentChannels] WHERE [Code] = 'CASH')
INSERT INTO [PaymentChannels] ([Id],[Name],[Code],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES ('C1111111-1111-1111-1111-111111111014',N'现金','CASH',1,@Cid,@SysUserId,@Now);

PRINT N'支付通道数据初始化完成！';
GO
