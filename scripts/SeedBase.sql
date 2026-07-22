-- ===================================================================
-- SeedBase.sql - 基础种子数据（公司、角色、用户、字典、配置）
-- 合并自所有独立种子文件，按数据依赖关系排列
-- 仅保留 GS001 公司，其余公司数据已移除
-- ===================================================================

-- SeedBase.sql - 基础种子数据（单公司精简版）
-- 合并自所有独立种子文件，按数据依赖关系排列
-- 仅保留 GS001 公司，其余公司数据已移除
-- ===================================================================

-- ===================================================================
-- 1. 公司 + 2. 角色 + 3. 用户
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

-- ======== 1. 公司 ========
DECLARE @GS001Id uniqueidentifier;


IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS001')
    INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'上海茂源置业有限公司','GS001',N'张建国','13912345678',N'上海市浦东新区陆家嘴金融中心A座',1,@SysUserId,@Now);
SELECT @GS001Id = [Id] FROM [Companies] WHERE [Code] = 'GS001';




PRINT N'公司数据初始化完成';

-- ======== 2. 角色 ========
DECLARE @R_AdminId uniqueidentifier;
DECLARE @R_OpsSupId uniqueidentifier;
DECLARE @R_OperId uniqueidentifier;
DECLARE @R_FinSupId uniqueidentifier;
DECLARE @R_FinDirId uniqueidentifier;
DECLARE @R_AccId uniqueidentifier;
DECLARE @R_DeptMgrId uniqueidentifier;
DECLARE @R_GenMgrId uniqueidentifier;
DECLARE @R_LegalId uniqueidentifier;
DECLARE @R_LandlordId uniqueidentifier;

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Admin')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'系统管理员','Admin',N'系统配置、用户管理、审批流程',1,@SysUserId,@Now);
SELECT @R_AdminId = [Id] FROM [Roles] WHERE [Code] = 'Admin';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'OpsSupervisor')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'运营主管','OpsSupervisor',N'审核合同、费用、抄表等日常运营事务',1,@SysUserId,@Now);
SELECT @R_OpsSupId = [Id] FROM [Roles] WHERE [Code] = 'OpsSupervisor';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Operator')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'运营人员','Operator',N'日常房屋、合同、租客等操作',1,@SysUserId,@Now);
SELECT @R_OperId = [Id] FROM [Roles] WHERE [Code] = 'Operator';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'FinanceSupervisor')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'财务主管','FinanceSupervisor',N'审核收款、会计、对账等财务事务',1,@SysUserId,@Now);
SELECT @R_FinSupId = [Id] FROM [Roles] WHERE [Code] = 'FinanceSupervisor';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'FinanceDirector')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'财务总监','FinanceDirector',N'财务报表审核、资金调度审批',1,@SysUserId,@Now);
SELECT @R_FinDirId = [Id] FROM [Roles] WHERE [Code] = 'FinanceDirector';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Accountant')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'会计','Accountant',N'日常记账、凭证处理',1,@SysUserId,@Now);
SELECT @R_AccId = [Id] FROM [Roles] WHERE [Code] = 'Accountant';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'DeptManager')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'部门经理','DeptManager',N'部门业务审批',1,@SysUserId,@Now);
SELECT @R_DeptMgrId = [Id] FROM [Roles] WHERE [Code] = 'DeptManager';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'GeneralManager')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'总经理','GeneralManager',N'公司级业务审批、决策',1,@SysUserId,@Now);
SELECT @R_GenMgrId = [Id] FROM [Roles] WHERE [Code] = 'GeneralManager';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Legal')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'法务','Legal',N'合同法务审核、纠纷处理',1,@SysUserId,@Now);
SELECT @R_LegalId = [Id] FROM [Roles] WHERE [Code] = 'Legal';

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Landlord')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'公司账号（只读）','Landlord',N'归属公司账号，仅可查看本公司数据',1,@SysUserId,@Now);
SELECT @R_LandlordId = [Id] FROM [Roles] WHERE [Code] = 'Landlord';

PRINT N'角色数据初始化完成';

-- ======== 3. 用户 ========
DECLARE @U_AdminId uniqueidentifier;
DECLARE @U_ZhangsanId uniqueidentifier;
DECLARE @U_LisiId uniqueidentifier;
DECLARE @U_WangwuId uniqueidentifier;
DECLARE @U_ZhaoliuId uniqueidentifier;
DECLARE @U_CompanyAId uniqueidentifier;

-- admin
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'admin')
BEGIN
    SET @U_AdminId = NEWID();
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[CreatedBy],[CreatedAt])
    VALUES (@U_AdminId,'admin','$2b$12$ZnELF2QgBXIilzbMir2uh.RWwIZYXExVvq4camCr.tNfzPH7SShk.',N'系统管理员','13800138000','admin@rental.com',1,1,@SysUserId,@Now);
    IF @R_AdminId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),@U_AdminId,@R_AdminId,@SysUserId,@Now);
END
ELSE
    SELECT @U_AdminId = [Id] FROM [Users] WHERE [Username] = 'admin';

-- zhangsan
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'zhangsan')
BEGIN
    SET @U_ZhangsanId = NEWID();
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_ZhangsanId,'zhangsan','$2b$12$ZnELF2QgBXIilzbMir2uh.RWwIZYXExVvq4camCr.tNfzPH7SShk.',N'张山','13800138001','zhangsan@rental.com',1,0,@GS001Id,@SysUserId,@Now);
    IF @R_OpsSupId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),@U_ZhangsanId,@R_OpsSupId,@SysUserId,@Now);
END
ELSE
    SELECT @U_ZhangsanId = [Id] FROM [Users] WHERE [Username] = 'zhangsan';

-- lisi
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'lisi')
BEGIN
    SET @U_LisiId = NEWID();
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_LisiId,'lisi','$2b$12$ZnELF2QgBXIilzbMir2uh.RWwIZYXExVvq4camCr.tNfzPH7SShk.',N'李思','13800138002','lisi@rental.com',1,0,@GS001Id,@SysUserId,@Now);
    IF @R_OperId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),@U_LisiId,@R_OperId,@SysUserId,@Now);
END
ELSE
    SELECT @U_LisiId = [Id] FROM [Users] WHERE [Username] = 'lisi';

-- wangwu
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'wangwu')
BEGIN
    SET @U_WangwuId = NEWID();
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_WangwuId,'wangwu','$2b$12$ZnELF2QgBXIilzbMir2uh.RWwIZYXExVvq4camCr.tNfzPH7SShk.',N'王武','13800138003','wangwu@rental.com',1,0,@GS001Id,@SysUserId,@Now);
    IF @R_FinSupId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),@U_WangwuId,@R_FinSupId,@SysUserId,@Now);
END
ELSE
    SELECT @U_WangwuId = [Id] FROM [Users] WHERE [Username] = 'wangwu';

-- zhaoliu
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'zhaoliu')
BEGIN
    SET @U_ZhaoliuId = NEWID();
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_ZhaoliuId,'zhaoliu','$2b$12$ZnELF2QgBXIilzbMir2uh.RWwIZYXExVvq4camCr.tNfzPH7SShk.',N'赵柳','13800138004','zhaoliu@rental.com',1,0,@GS001Id,@SysUserId,@Now);
    IF @R_AccId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),@U_ZhaoliuId,@R_AccId,@SysUserId,@Now);
END
ELSE
    SELECT @U_ZhaoliuId = [Id] FROM [Users] WHERE [Username] = 'zhaoliu';

-- company_a
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'company_a')
BEGIN
    SET @U_CompanyAId = NEWID();
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_CompanyAId,'company_a','$2b$12$ZnELF2QgBXIilzbMir2uh.RWwIZYXExVvq4camCr.tNfzPH7SShk.',N'张建国（茂源）','13912345678','company_a@rental.com',1,0,@GS001Id,@SysUserId,@Now);
    IF @R_LandlordId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),@U_CompanyAId,@R_LandlordId,@SysUserId,@Now);
END
ELSE
    SELECT @U_CompanyAId = [Id] FROM [Users] WHERE [Username] = 'company_a';

PRINT N'用户数据初始化完成';
SELECT 'Companies' AS [Table], COUNT(*) AS [Count] FROM [Companies]
UNION ALL SELECT 'Roles', COUNT(*) FROM [Roles]
UNION ALL SELECT 'Users', COUNT(*) FROM [Users]
UNION ALL SELECT 'UserRoles', COUNT(*) FROM [UserRoles];
GO

-- ===================================================================
-- 4. 房型数据
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'开间/单间')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'整租','Studio',N'开间/单间',N'开放式一体的居住空间',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'一室一厅')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'整租','1BR1L',N'一室一厅',N'一间卧室加独立客厅',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'两室一厅')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'整租','2BR1L',N'两室一厅',N'两间卧室加独立客厅',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'两室两厅')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'整租','2BR2L',N'两室两厅',N'两间卧室加独立客厅和餐厅',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'三室一厅')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'整租','3BR1L',N'三室一厅',N'三间卧室加独立客厅',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'三室两厅')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'整租','3BR2L',N'三室两厅',N'三间卧室加独立客厅和餐厅',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'四室及以上')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'整租','4BR',N'四室及以上',N'四间及以上卧室',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'主卧')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'合租','Master',N'主卧',N'合租主卧（带独立卫生间）',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'次卧')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'合租','Secondary',N'次卧',N'合租次卧（共用卫生间）',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'公寓')
    INSERT INTO [RoomTypes] ([Id],[Category],[Code],[Name],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'整租','Apartment',N'公寓',N'酒店式公寓/服务式公寓',1,@Cid,@SysUserId,@Now);

DECLARE @StudioId uniqueidentifier; SELECT @StudioId = [Id] FROM [RoomTypes] WHERE [Name] = N'开间/单间';
DECLARE @OneBrId uniqueidentifier; SELECT @OneBrId = [Id] FROM [RoomTypes] WHERE [Name] = N'一室一厅';
DECLARE @TwoBrId uniqueidentifier; SELECT @TwoBrId = [Id] FROM [RoomTypes] WHERE [Name] = N'两室一厅';
DECLARE @ThreeBrId uniqueidentifier; SELECT @ThreeBrId = [Id] FROM [RoomTypes] WHERE [Name] = N'三室一厅';

PRINT N'房型数据初始化完成！';
GO

-- ===================================================================
-- 5. 收费项目
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'RENT')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'RENT',N'房租费','FixedAmount',1,'Rent','Recurring',1,1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'WATER')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[Unit],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'WATER',N'水费','MeterBased',N'元/吨',2,'Utility','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'ELECTRIC')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[Unit],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'ELECTRIC',N'电费','MeterBased',N'元/度',3,'Utility','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'GAS')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[Unit],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'GAS',N'燃气费','MeterBased',N'元/方',4,'Utility','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'MANAGEMENT')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'MANAGEMENT',N'物业管理费','FixedAmount',5,'Property','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'SANITATION')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'SANITATION',N'卫生费','FixedAmount',6,'Property','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'SECURITY')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'SECURITY',N'安保费','FixedAmount',7,'Property','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'GARBAGE')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'GARBAGE',N'垃圾清运费','FixedAmount',8,'Property','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'HEATING')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'HEATING',N'取暖费','FixedAmount',9,'Utility','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'INTERNET')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'INTERNET',N'网费','FixedAmount',10,'Other','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'TV')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'TV',N'电视费','FixedAmount',11,'Other','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'PARKING')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'PARKING',N'停车费','FixedAmount',12,'Other','Recurring',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'INTEREST')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'INTEREST',N'利息','FixedAmount',99,'Other','Recurring',1,0,@Cid,@SysUserId,@Now);

-- ===== 一次性收费 (OneTime) =====
IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'DEPOSIT')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'DEPOSIT',N'押金','FixedAmount',1,'Property','OneTime',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'DECORATION_DEPOSIT')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'DECORATION_DEPOSIT',N'装修押金','FixedAmount',2,'Property','OneTime',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'KEY_DEPOSIT')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'KEY_DEPOSIT',N'钥匙押金','FixedAmount',3,'Other','OneTime',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'CLEANING')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'CLEANING',N'清洁费','FixedAmount',4,'Other','OneTime',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'MOVING')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'MOVING',N'搬运费','FixedAmount',5,'Other','OneTime',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'PENALTY')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'PENALTY',N'违约金','FixedAmount',6,'Other','OneTime',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'COMPENSATION')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'COMPENSATION',N'赔偿金','FixedAmount',7,'Other','OneTime',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'AC_OVERTIME')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'AC_OVERTIME',N'空调加时费','FixedAmount',9,'Other','OneTime',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'AIR_CONDITIONING')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'AIR_CONDITIONING',N'空调移机费','FixedAmount',8,'Other','OneTime',1,0,@Cid,@SysUserId,@Now);

PRINT N'收费项目数据初始化完成！';
GO

-- ===================================================================
-- 6. 定价标准
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';
DECLARE @StudioId uniqueidentifier; SELECT @StudioId = [Id] FROM [RoomTypes] WHERE [Name] = N'开间/单间';
DECLARE @OneBrId uniqueidentifier; SELECT @OneBrId = [Id] FROM [RoomTypes] WHERE [Name] = N'一室一厅';
DECLARE @TwoBrId uniqueidentifier; SELECT @TwoBrId = [Id] FROM [RoomTypes] WHERE [Name] = N'两室一厅';
DECLARE @ThreeBrId uniqueidentifier; SELECT @ThreeBrId = [Id] FROM [RoomTypes] WHERE [Name] = N'三室一厅';

IF NOT EXISTS (SELECT 1 FROM [FloorLevelBands])
BEGIN
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'低层',1,5,N'低层',@Cid,@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'中层',6,12,N'中层',@Cid,@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'高层',13,17,N'高层',@Cid,@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'顶层',18,99,N'顶层',@Cid,@SysUserId,@Now);
END

DECLARE @LowId uniqueidentifier; SELECT @LowId = [Id] FROM [FloorLevelBands] WHERE [Name] = N'低层';
DECLARE @MidId uniqueidentifier; SELECT @MidId = [Id] FROM [FloorLevelBands] WHERE [Name] = N'中层';
DECLARE @HighId uniqueidentifier; SELECT @HighId = [Id] FROM [FloorLevelBands] WHERE [Name] = N'高层';

IF @TwoBrId IS NOT NULL AND @LowId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @TwoBrId AND [FloorLevelBandId] = @LowId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@TwoBrId,@LowId,4800,@Cid,@SysUserId,@Now);

IF @TwoBrId IS NOT NULL AND @MidId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @TwoBrId AND [FloorLevelBandId] = @MidId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@TwoBrId,@MidId,5200,@Cid,@SysUserId,@Now);

IF @TwoBrId IS NOT NULL AND @HighId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @TwoBrId AND [FloorLevelBandId] = @HighId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@TwoBrId,@HighId,5600,@Cid,@SysUserId,@Now);

IF @OneBrId IS NOT NULL AND @LowId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @OneBrId AND [FloorLevelBandId] = @LowId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@OneBrId,@LowId,3200,@Cid,@SysUserId,@Now);

IF @OneBrId IS NOT NULL AND @MidId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @OneBrId AND [FloorLevelBandId] = @MidId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@OneBrId,@MidId,3500,@Cid,@SysUserId,@Now);

IF @OneBrId IS NOT NULL AND @HighId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @OneBrId AND [FloorLevelBandId] = @HighId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@OneBrId,@HighId,3800,@Cid,@SysUserId,@Now);

IF @StudioId IS NOT NULL AND @LowId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @StudioId AND [FloorLevelBandId] = @LowId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@StudioId,@LowId,2500,@Cid,@SysUserId,@Now);

IF @StudioId IS NOT NULL AND @MidId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [RoomPricingStandards] WHERE [RoomTypeId] = @StudioId AND [FloorLevelBandId] = @MidId)
    INSERT INTO [RoomPricingStandards] ([Id],[RoomTypeId],[FloorLevelBandId],[RentAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@StudioId,@MidId,2800,@Cid,@SysUserId,@Now);

PRINT N'定价标准数据初始化完成！';
GO

-- ===================================================================
-- 7. 支付通道
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [PaymentChannels] WHERE [Code] = 'ALIPAY')
    INSERT INTO [PaymentChannels] ([Id],[Name],[Code],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'支付宝','ALIPAY',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [PaymentChannels] WHERE [Code] = 'WECHAT')
    INSERT INTO [PaymentChannels] ([Id],[Name],[Code],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'微信支付','WECHAT',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [PaymentChannels] WHERE [Code] = 'BANK')
    INSERT INTO [PaymentChannels] ([Id],[Name],[Code],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'银行转账','BANK',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [PaymentChannels] WHERE [Code] = 'CASH')
    INSERT INTO [PaymentChannels] ([Id],[Name],[Code],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'现金','CASH',1,@Cid,@SysUserId,@Now);

PRINT N'支付通道数据初始化完成！';
GO

-- ===================================================================
-- 8. 税率配置
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [TaxRateConfigs] WHERE [Name] = N'增值税普通发票' AND [Rate] = 6)
    INSERT INTO [TaxRateConfigs] ([Id],[Name],[Rate],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'增值税普通发票',6,'2026-01-01',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [TaxRateConfigs] WHERE [Name] = N'增值税专用发票' AND [Rate] = 9)
    INSERT INTO [TaxRateConfigs] ([Id],[Name],[Rate],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'增值税专用发票',9,'2026-01-01',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [TaxRateConfigs] WHERE [Name] = N'小规模纳税人' AND [Rate] = 3)
    INSERT INTO [TaxRateConfigs] ([Id],[Name],[Rate],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'小规模纳税人',3,'2026-01-01',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [TaxRateConfigs] WHERE [Name] = N'简易征收' AND [Rate] = 5)
    INSERT INTO [TaxRateConfigs] ([Id],[Name],[Rate],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'简易征收',5,'2026-01-01',1,@Cid,@SysUserId,@Now);

PRINT N'税率数据初始化完成！';
GO

-- ===================================================================
-- 9. 会计科目
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '1001')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'1001',N'库存现金','Debit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '1002')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'1002',N'银行存款','Debit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '1122')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'1122',N'应收账款','Debit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '1131')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'1131',N'其他应收款','Debit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '112201')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[ParentCode],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'112201',N'应收房租','1122','Debit',2,1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '112202')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[ParentCode],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'112202',N'应收押金','1122','Debit',2,1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '2001')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'2001',N'短期借款','Credit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '2202')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'2202',N'应付账款','Credit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '2221')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'2221',N'应交税费','Credit',1,0,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '222101')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[ParentCode],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'222101',N'应交增值税','2221','Credit',2,1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [AccountingSubjects] WHERE [Code] = '6001')
    INSERT INTO [AccountingSubjects] ([Id],[Code],[Name],[Direction],[Level],[IsLeaf],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'6001',N'主营业务收入','Credit',1,1,@Cid,@SysUserId,@Now);

PRINT N'会计科目数据初始化完成！';
GO

-- ===================================================================
-- 10. 审批类型 + 级别
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

DECLARE @AT_BatchImport uniqueidentifier;
DECLARE @AT_ContractCreate uniqueidentifier;
DECLARE @AT_ContractTerminate uniqueidentifier;
DECLARE @AT_ReceiptReverse uniqueidentifier;
DECLARE @AT_Discount uniqueidentifier;
DECLARE @AT_ContractModify uniqueidentifier;
DECLARE @AT_ContractRenew uniqueidentifier;
DECLARE @AT_ContractFeeChange uniqueidentifier;
DECLARE @AT_ChangeRequest uniqueidentifier;

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'BATCH_IMPORT_ROOMS')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'批量导入房屋','BATCH_IMPORT_ROOMS',N'批量导入房屋数据需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_BatchImport = [Id] FROM [ApprovalTypes] WHERE [Code] = 'BATCH_IMPORT_ROOMS';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_CREATE')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'新建合同','CONTRACT_CREATE',N'新建租赁合同需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_ContractCreate = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_CREATE';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_TERMINATE')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'提前解约','CONTRACT_TERMINATE',N'合同提前终止需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_ContractTerminate = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_TERMINATE';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'RECEIPT_REVERSE')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'收款冲销','RECEIPT_REVERSE',N'收款冲销操作需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_ReceiptReverse = [Id] FROM [ApprovalTypes] WHERE [Code] = 'RECEIPT_REVERSE';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'DISCOUNT')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'应收减免','DISCOUNT',N'应收费用减免需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_Discount = [Id] FROM [ApprovalTypes] WHERE [Code] = 'DISCOUNT';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_MODIFY')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'修改合同租金','CONTRACT_MODIFY',N'修改合同租金需要审批，金额越大审批级别越高。',1,@Cid,@SysUserId,@Now);
SELECT @AT_ContractModify = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_MODIFY';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_MODIFY_OTHER')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'修改合同信息','CONTRACT_MODIFY_OTHER',N'修改合同起止日期、付款周期等信息需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_ContractModifyOther = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_MODIFY_OTHER';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_RENEW')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'合同续签','CONTRACT_RENEW',N'合同续签需要审批，根据月租金额自动路由审批级别',1,@Cid,@SysUserId,@Now);
SELECT @AT_ContractRenew = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_RENEW';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_GENERATE_RECEIVABLE')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'生成应收','CONTRACT_GENERATE_RECEIVABLE',N'手动生成应收计划需要审批',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_SUSPEND')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'暂停合同','CONTRACT_SUSPEND',N'暂停合同需要审批',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_TENANT_CHANGE')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'合同租客变更','CONTRACT_TENANT_CHANGE',N'合同增删租客需要审批',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_SUPPLEMENTARY_FEE')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'补充收费','CONTRACT_SUPPLEMENTARY_FEE',N'补充追溯收费需要审批',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_FEE_CHANGE')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'费用调价','CONTRACT_FEE_CHANGE',N'调整合同中的费用单价需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_ContractFeeChange = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_FEE_CHANGE';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CHANGE_REQUEST')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'合同变更','CHANGE_REQUEST',N'合同信息变更请求需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_ChangeRequest = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CHANGE_REQUEST';

DECLARE @R_OpsSup uniqueidentifier; SELECT @R_OpsSup = [Id] FROM [Roles] WHERE [Code] = 'OpsSupervisor';
DECLARE @R_DeptMgr uniqueidentifier; SELECT @R_DeptMgr = [Id] FROM [Roles] WHERE [Code] = 'DeptManager';
DECLARE @R_FinSup uniqueidentifier; SELECT @R_FinSup = [Id] FROM [Roles] WHERE [Code] = 'FinanceSupervisor';
DECLARE @R_FinDir uniqueidentifier; SELECT @R_FinDir = [Id] FROM [Roles] WHERE [Code] = 'FinanceDirector';
DECLARE @R_GenMgr uniqueidentifier; SELECT @R_GenMgr = [Id] FROM [Roles] WHERE [Code] = 'GeneralManager';

IF @AT_BatchImport IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_BatchImport AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_BatchImport,1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_BatchImport IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_BatchImport AND [LevelNo] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_BatchImport,2,@R_DeptMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_BatchImport IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_BatchImport AND [LevelNo] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_BatchImport,3,@R_GenMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractCreate IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractCreate AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractCreate,1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractCreate IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractCreate AND [LevelNo] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractCreate,2,@R_DeptMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractCreate IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractCreate AND [LevelNo] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractCreate,3,@R_GenMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractTerminate IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractTerminate AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractTerminate,1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);

IF @AT_ContractTerminate IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractTerminate AND [LevelNo] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractTerminate,2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);

IF @AT_ContractTerminate IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractTerminate AND [LevelNo] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractTerminate,3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

IF @AT_ReceiptReverse IS NOT NULL AND @R_FinSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ReceiptReverse AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ReceiptReverse,1,@R_FinSup,0,50000,@Cid,@SysUserId,@Now);

IF @AT_ReceiptReverse IS NOT NULL AND @R_FinDir IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ReceiptReverse AND [LevelNo] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ReceiptReverse,2,@R_FinDir,50000,99999999,@Cid,@SysUserId,@Now);

IF @AT_ReceiptReverse IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ReceiptReverse AND [LevelNo] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ReceiptReverse,3,@R_GenMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_Discount IS NOT NULL AND @R_FinSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_Discount AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_Discount,1,@R_FinSup,0,5000,@Cid,@SysUserId,@Now);

IF @AT_Discount IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_Discount AND [LevelNo] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_Discount,2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);

IF @AT_Discount IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_Discount AND [LevelNo] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_Discount,3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

IF @AT_ContractModify IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractModify AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractModify,1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);

IF @AT_ContractModify IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractModify AND [LevelNo] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractModify,2,@R_DeptMgr,5000,99999999,@Cid,@SysUserId,@Now);

IF @AT_ContractModifyOther IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractModifyOther AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractModifyOther,1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ChangeRequest IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ChangeRequest AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ChangeRequest,1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractFeeChange IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractFeeChange AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractFeeChange,1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractRenew IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractRenew AND [LevelNo] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractRenew,1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);

IF @AT_ContractRenew IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractRenew AND [LevelNo] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractRenew,2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);

IF @AT_ContractRenew IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractRenew AND [LevelNo] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[LevelNo],[ApproverRoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractRenew,3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

PRINT N'审批类型及级别数据初始化完成！';
GO
-- ===================================================================
-- 11. 菜单权限
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

DECLARE @AdminRoleId uniqueidentifier; SELECT @AdminRoleId = [Id] FROM [Roles] WHERE [Code] = 'Admin';
IF @AdminRoleId IS NULL
BEGIN
    SET @AdminRoleId = NEWID();
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (@AdminRoleId,N'系统管理员','Admin',N'系统配置、用户管理、审批流程',1,@SysUserId,@Now);
END

DELETE FROM [RoleMenus];
DELETE FROM [Menus];
PRINT N'已清除旧菜单数据';

DECLARE @M_Dashboard uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Dashboard,N'仪表盘','dashboard:view','/dashboard','DataAnalysis',NULL,1,1,@SysUserId,@Now);

DECLARE @M_Building uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Building,N'房屋管理','building:view','/buildings','HomeFilled',NULL,2,1,@SysUserId,@Now);

DECLARE @M_Contract uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract,N'合同管理','contract:view','/contracts','Document',NULL,3,1,@SysUserId,@Now);

DECLARE @M_Receipt uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Receipt,N'收款管理','receipt:view','/receipts','Money',NULL,4,1,@SysUserId,@Now);

DECLARE @M_Bill uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bill,N'账单管理','bill:view','/bills','DocumentCopy',NULL,5,1,@SysUserId,@Now);

DECLARE @M_Tenant uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Tenant,N'租客管理','tenant:view','/tenants','UserFilled',NULL,6,1,@SysUserId,@Now);

DECLARE @M_Collection uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Collection,N'催缴管理','collection:view','/collection','BellFilled',NULL,7,1,@SysUserId,@Now);

DECLARE @M_Meter uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Meter,N'抄表管理','meter:view','/meter','Reading',NULL,8,1,@SysUserId,@Now);

DECLARE @M_Approval uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Approval,N'审批中心','approval:view','/approvals','CircleCheck',NULL,9,1,@SysUserId,@Now);

DECLARE @M_Notification uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Notification,N'通知中心','notification:view','/notifications','Bell',NULL,10,1,@SysUserId,@Now);

DECLARE @M_Report uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Report,N'财务报表','report:view','/reports','TrendCharts',NULL,11,1,@SysUserId,@Now);

DECLARE @M_Accounting uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Accounting,N'会计核算','accounting:view','/accounting','Files',NULL,13,1,@SysUserId,@Now);

DECLARE @M_Bank uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bank,N'银企直连','bank:view','/bank','Link',NULL,14,1,@SysUserId,@Now);

DECLARE @M_CompanyOverview uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_CompanyOverview,N'多公司总览','companyoverview:view','/reports/companyoverview','DataAnalysis',NULL,14,1,@SysUserId,@Now);

DECLARE @M_Audit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Audit,N'变更审计','audit:view','/audit','Search',NULL,15,1,@SysUserId,@Now);

DECLARE @M_System uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System,N'系统设置','system:view','/system','Setting',NULL,99,1,@SysUserId,@Now);

DECLARE @M_Building_List uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Building_List,N'房间列表','building:list','/buildings',@M_Building,1,1,@SysUserId,@Now);

DECLARE @M_Building_Detail uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Building_Detail,N'房间详情','building:detail','/buildings/room/:id',@M_Building,2,1,@SysUserId,@Now);

DECLARE @M_Building_Import uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Building_Import,N'批量导入','building:import','/buildings/import',@M_Building,3,1,@SysUserId,@Now);

DECLARE @M_Building_Create uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Building_Create,N'新增楼宇','building:create',NULL,@M_Building,10,1,@SysUserId,@Now);

DECLARE @M_Building_Edit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Building_Edit,N'编辑楼宇','building:edit',NULL,@M_Building,11,1,@SysUserId,@Now);

DECLARE @M_Building_Delete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Building_Delete,N'删除楼宇','building:delete',NULL,@M_Building,12,1,@SysUserId,@Now);

DECLARE @M_Building_ChangeStatus uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Building_ChangeStatus,N'房间状态变更','building:changestatus',NULL,@M_Building,13,1,@SysUserId,@Now);

DECLARE @M_Contract_List uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_List,N'合同列表','contract:list','/contracts',@M_Contract,1,1,@SysUserId,@Now);

DECLARE @M_Contract_Create uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_Create,N'新建合同','contract:create','/contracts/create',@M_Contract,2,1,@SysUserId,@Now);

DECLARE @M_Contract_Detail uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_Detail,N'合同详情','contract:detail','/contracts/:id',@M_Contract,3,1,@SysUserId,@Now);

DECLARE @M_Contract_Edit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_Edit,N'编辑合同','contract:edit',NULL,@M_Contract,10,1,@SysUserId,@Now);

DECLARE @M_Contract_Terminate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_Terminate,N'终止合同','contract:terminate',NULL,@M_Contract,11,1,@SysUserId,@Now);

DECLARE @M_Contract_Renew uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_Renew,N'续签合同','contract:renew',NULL,@M_Contract,12,1,@SysUserId,@Now);

DECLARE @M_Contract_ToggleStatus uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_ToggleStatus,N'暂停/恢复合同','contract:togglestatus',NULL,@M_Contract,13,1,@SysUserId,@Now);

DECLARE @M_Contract_AdjustRent uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_AdjustRent,N'租金调整','contract:adjustrent',NULL,@M_Contract,14,1,@SysUserId,@Now);

DECLARE @M_Contract_AdjustFee uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Contract_AdjustFee,N'费用调价','contract:adjustfee',NULL,@M_Contract,15,1,@SysUserId,@Now);

DECLARE @M_Receipt_List uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Receipt_List,N'收款列表','receipt:list','/receipts',@M_Receipt,1,1,@SysUserId,@Now);

DECLARE @M_Receipt_Register uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Receipt_Register,N'收款登记','receipt:register','/receipts/register',@M_Receipt,2,1,@SysUserId,@Now);

DECLARE @M_Receipt_Confirm uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Receipt_Confirm,N'收款确认','receipt:confirm','/receipts/confirm',@M_Receipt,3,1,@SysUserId,@Now);

DECLARE @M_Receipt_ConfirmAmount uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Receipt_ConfirmAmount,N'确认到账','receipt:confirmamount',NULL,@M_Receipt,10,1,@SysUserId,@Now);

DECLARE @M_Receipt_Reject uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Receipt_Reject,N'驳回收款','receipt:reject',NULL,@M_Receipt,11,1,@SysUserId,@Now);

DECLARE @M_Receipt_Reverse uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Receipt_Reverse,N'收款冲销','receipt:reverse',NULL,@M_Receipt,12,1,@SysUserId,@Now);

DECLARE @M_Receipt_Deposit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Receipt_Deposit,N'押金退还/扣款','receipt:deposit',NULL,@M_Receipt,13,1,@SysUserId,@Now);

DECLARE @M_Bill_List uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bill_List,N'账单列表','bill:list','/bills',@M_Bill,1,1,@SysUserId,@Now);

DECLARE @M_Bill_Generate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bill_Generate,N'生成账单','bill:generate','/bills/generate',@M_Bill,2,1,@SysUserId,@Now);

DECLARE @M_Bill_Preview uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bill_Preview,N'账单预览','bill:preview','/bills/preview/:id',@M_Bill,3,1,@SysUserId,@Now);

DECLARE @M_Bill_ExportPdf uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bill_ExportPdf,N'批量导出PDF','bill:exportpdf',NULL,@M_Bill,10,1,@SysUserId,@Now);

DECLARE @M_Bill_Print uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bill_Print,N'打印账单','bill:print',NULL,@M_Bill,11,1,@SysUserId,@Now);

DECLARE @M_Tenant_List uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Tenant_List,N'租客列表','tenant:list','/tenants',@M_Tenant,1,1,@SysUserId,@Now);

DECLARE @M_Tenant_Detail uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Tenant_Detail,N'租客详情','tenant:detail','/tenants/:id',@M_Tenant,2,1,@SysUserId,@Now);

DECLARE @M_Tenant_Create uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Tenant_Create,N'新增租客','tenant:create',NULL,@M_Tenant,10,1,@SysUserId,@Now);

DECLARE @M_Tenant_Edit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Tenant_Edit,N'编辑租客','tenant:edit',NULL,@M_Tenant,11,1,@SysUserId,@Now);

DECLARE @M_Tenant_Delete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Tenant_Delete,N'删除租客','tenant:delete',NULL,@M_Tenant,12,1,@SysUserId,@Now);

DECLARE @M_Collection_Overview uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Collection_Overview,N'催缴概览','collection:overview','/collection',@M_Collection,1,1,@SysUserId,@Now);

DECLARE @M_Collection_Config uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Collection_Config,N'催缴配置','collection:config','/collection/config',@M_Collection,2,1,@SysUserId,@Now);

DECLARE @M_Collection_Records uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Collection_Records,N'催缴记录','collection:records','/collection/records',@M_Collection,3,1,@SysUserId,@Now);

DECLARE @M_Collection_Send uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Collection_Send,N'发送催缴','collection:send',NULL,@M_Collection,10,1,@SysUserId,@Now);

DECLARE @M_Meter_List uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Meter_List,N'抄表记录','meter:list','/meter',@M_Meter,1,1,@SysUserId,@Now);

DECLARE @M_Meter_Import uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Meter_Import,N'Excel批量导入','meter:import',NULL,@M_Meter,10,1,@SysUserId,@Now);

DECLARE @M_Meter_Estimate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Meter_Estimate,N'逾期估读','meter:estimate',NULL,@M_Meter,11,1,@SysUserId,@Now);

DECLARE @M_Meter_SaveReadings uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Meter_SaveReadings,N'保存/确认抄表','meter:savereadings',NULL,@M_Meter,12,1,@SysUserId,@Now);

DECLARE @M_Approval_Pending uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Approval_Pending,N'待审批','approval:pending','/approvals',@M_Approval,1,1,@SysUserId,@Now);

DECLARE @M_Approval_MyRequests uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Approval_MyRequests,N'我的提交','approval:myrequests','/approvals/myrequests',@M_Approval,2,1,@SysUserId,@Now);

DECLARE @M_Approval_History uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Approval_History,N'审批历史','approval:history','/approvals/history',@M_Approval,3,1,@SysUserId,@Now);

DECLARE @M_Approval_Approve uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Approval_Approve,N'通过审批','approval:approve',NULL,@M_Approval,10,1,@SysUserId,@Now);

DECLARE @M_Approval_Reject uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Approval_Reject,N'驳回审批','approval:reject',NULL,@M_Approval,11,1,@SysUserId,@Now);

DECLARE @M_Notification_MarkAllRead uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Notification_MarkAllRead,N'全部标记已读','notification:markallread',NULL,@M_Notification,10,1,@SysUserId,@Now);

DECLARE @M_Report_CollectionRate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Report_CollectionRate,N'收租率统计','report:collectionrate','/reports/collectionrate',@M_Report,1,1,@SysUserId,@Now);

DECLARE @M_Report_OverdueDetail uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Report_OverdueDetail,N'欠费明细表','report:overduedetail','/reports/overduedetail',@M_Report,2,1,@SysUserId,@Now);

DECLARE @M_Report_DailyReceipt uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Report_DailyReceipt,N'收款日报','report:dailyreceipt','/reports/dailyreceipt',@M_Report,3,1,@SysUserId,@Now);

DECLARE @M_Report_MonthlyReceipt uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Report_MonthlyReceipt,N'收款月报','report:monthlyreceipt','/reports/monthlyreceipt',@M_Report,4,1,@SysUserId,@Now);

DECLARE @M_Report_FeeRevenue uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Report_FeeRevenue,N'费用收入统计','report:feerevenue','/reports/feerevenue',@M_Report,5,1,@SysUserId,@Now);

DECLARE @M_Report_OccupancyRate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Report_OccupancyRate,N'出租率统计','report:occupancyrate','/reports/occupancyrate',@M_Report,6,1,@SysUserId,@Now);

DECLARE @M_Report_Export uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Report_Export,N'导出报表Excel','report:export',NULL,@M_Report,10,1,@SysUserId,@Now);

DECLARE @M_Accounting_Subjects uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Accounting_Subjects,N'科目表','accounting:subjects','/accounting/subjects',@M_Accounting,1,1,@SysUserId,@Now);

DECLARE @M_Journal uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Journal,N'日记账','journal:view','/journals','Notebook',NULL,11,1,@SysUserId,@Now);

-- 总账（一级菜单）
DECLARE @M_GL uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_GL,N'总账管理','gl:view','/gl','DataBoard',NULL,12,1,@SysUserId,@Now);

-- 凭证管理菜单已删除（系统不再使用 Voucher）




DECLARE @M_Accounting_SubjectCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Accounting_SubjectCreate,N'新增科目','accounting:subjectcreate',NULL,@M_Accounting,10,1,@SysUserId,@Now);

DECLARE @M_Accounting_Post uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Accounting_Post,N'过账','accounting:post',NULL,@M_Accounting,11,1,@SysUserId,@Now);

DECLARE @M_Accounting_Reverse uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Accounting_Reverse,N'冲销凭证','accounting:reverse',NULL,@M_Accounting,12,1,@SysUserId,@Now);

DECLARE @M_Bank_Import uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bank_Import,N'流水导入','bank:import','/bank/import',@M_Bank,1,1,@SysUserId,@Now);

DECLARE @M_Bank_Match uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bank_Match,N'自动匹配','bank:match','/bank/match',@M_Bank,2,1,@SysUserId,@Now);

DECLARE @M_Bank_Reconciliation uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bank_Reconciliation,N'余额调节表','bank:reconciliation','/bank/reconciliation',@M_Bank,3,1,@SysUserId,@Now);

DECLARE @M_Bank_ConfirmImport uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bank_ConfirmImport,N'确认导入','bank:confirmimport',NULL,@M_Bank,10,1,@SysUserId,@Now);

DECLARE @M_Bank_ManualMatch uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bank_ManualMatch,N'手动匹配','bank:manualmatch',NULL,@M_Bank,11,1,@SysUserId,@Now);

DECLARE @M_System_UserMgmt uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_UserMgmt,N'用户管理','system:user','/system/organization/users','User',@M_System,1,1,@SysUserId,@Now);

DECLARE @M_System_RoleMgmt uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_RoleMgmt,N'角色管理','system:role','/system/organization/roles','Avatar',@M_System,2,1,@SysUserId,@Now);

DECLARE @M_System_CompanyMgmt uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_CompanyMgmt,N'公司管理','system:company','/system/companies','OfficeBuilding',@M_System,4,1,@SysUserId,@Now);

DECLARE @M_System_MenuMgmt uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_MenuMgmt,N'菜单权限配置','system:menu','/system/menus','Menu',@M_System,5,1,@SysUserId,@Now);

DECLARE @M_System_ApprovalType uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_ApprovalType,N'审批类型配置','system:approvaltype','/system/approvaltypes','CircleCheck',@M_System,6,1,@SysUserId,@Now);

DECLARE @M_System_FeeCode uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_FeeCode,N'收费项目管理','system:feecode','/system/feecodes','Coin',@M_System,8,1,@SysUserId,@Now);

DECLARE @M_System_RoomType uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_RoomType,N'房型管理','system:roomtype','/system/roomtypes','Grid',@M_System,9,1,@SysUserId,@Now);

DECLARE @M_System_Pricing uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_Pricing,N'定价标准管理','system:pricing','/system/pricing','PriceTag',@M_System,10,1,@SysUserId,@Now);

DECLARE @M_System_PaymentChannel uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_PaymentChannel,N'支付通道管理','system:paymentchannel','/system/paymentchannels','CreditCard',@M_System,11,1,@SysUserId,@Now);

DECLARE @M_System_TaxRate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_TaxRate,N'税率配置','system:taxrate','/system/taxrates','CollectionTag',@M_System,12,1,@SysUserId,@Now);

DECLARE @M_System_AccountingSubject uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_AccountingSubject,N'会计科目管理','system:accountingsubject','/system/accountingsubjects','DataBoard',@M_System,13,1,@SysUserId,@Now);

DECLARE @M_System_Holiday uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_Holiday,N'节假日管理','system:holiday','/system/holidays','Calendar',@M_System,14,1,@SysUserId,@Now);

DECLARE @M_System_Interest uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_Interest,N'利息配置','system:interest','/system/interest','WarningFilled',@M_System,15,1,@SysUserId,@Now);

DECLARE @M_System_Scheduler uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_Scheduler,N'调度任务管理','system:scheduler','/system/scheduler','Timer',@M_System,16,1,@SysUserId,@Now);

DECLARE @M_System_Logs uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_Logs,N'系统日志','system:logs','/system/logs','Document',@M_System,17,1,@SysUserId,@Now);

DECLARE @M_System_ApiLogs uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_ApiLogs,N'API 日志','system:apilogs','/system/apilogs','Monitor',@M_System,18,1,@SysUserId,@Now);

DECLARE @M_System_UserCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_UserCreate,N'新增用户','system:usercreate',@M_System_UserMgmt,10,1,@SysUserId,@Now);

DECLARE @M_System_UserEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_UserEdit,N'编辑用户','system:useredit',@M_System_UserMgmt,11,1,@SysUserId,@Now);

DECLARE @M_System_RoleCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_RoleCreate,N'新增角色','system:rolecreate',@M_System_RoleMgmt,10,1,@SysUserId,@Now);

DECLARE @M_System_RoleAssignMenu uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_RoleAssignMenu,N'分配菜单权限','system:roleassignmenu',@M_System_RoleMgmt,11,1,@SysUserId,@Now);

DECLARE @M_System_CompanyCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_CompanyCreate,N'新增公司','system:companycreate',@M_System_CompanyMgmt,10,1,@SysUserId,@Now);

DECLARE @M_System_CompanyEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_CompanyEdit,N'编辑公司','system:companyedit',@M_System_CompanyMgmt,11,1,@SysUserId,@Now);

DECLARE @M_System_CompanyCreateUser uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_CompanyCreateUser,N'创建公司账号','system:companycreateuser',@M_System_CompanyMgmt,12,1,@SysUserId,@Now);

DECLARE @M_System_MenuCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_MenuCreate,N'新增菜单','system:menucreate',@M_System_MenuMgmt,10,1,@SysUserId,@Now);

DECLARE @M_System_MenuEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_MenuEdit,N'编辑菜单','system:menuedit',@M_System_MenuMgmt,11,1,@SysUserId,@Now);

DECLARE @M_System_MenuDelete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_MenuDelete,N'删除菜单','system:menudelete',@M_System_MenuMgmt,12,1,@SysUserId,@Now);

DECLARE @M_System_ApprovalTypeCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_ApprovalTypeCreate,N'新增审批类型','system:approvaltypecreate',@M_System_ApprovalType,10,1,@SysUserId,@Now);

DECLARE @M_System_ApprovalTypeEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_ApprovalTypeEdit,N'编辑审批类型','system:approvaltypeedit',@M_System_ApprovalType,11,1,@SysUserId,@Now);

DECLARE @M_System_ApprovalTypeLevel uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_ApprovalTypeLevel,N'级别配置','system:approvaltypelevel',@M_System_ApprovalType,12,1,@SysUserId,@Now);

DECLARE @M_System_ApprovalLevelCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_ApprovalLevelCreate,N'新增级别','system:approvallevelcreate',@M_System_ApprovalType,15,1,@SysUserId,@Now);

DECLARE @M_System_ApprovalLevelEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_ApprovalLevelEdit,N'编辑级别','system:approvalleveledit',@M_System_ApprovalType,16,1,@SysUserId,@Now);

DECLARE @M_System_ApprovalLevelDelete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_ApprovalLevelDelete,N'删除级别','system:approvalleveldelete',@M_System_ApprovalType,17,1,@SysUserId,@Now);

DECLARE @M_System_FeeCodeCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_FeeCodeCreate,N'新增费用','system:feecodecreate',@M_System_FeeCode,10,1,@SysUserId,@Now);

DECLARE @M_System_FeeCodeEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_FeeCodeEdit,N'编辑费用','system:feecodeedit',@M_System_FeeCode,11,1,@SysUserId,@Now);

DECLARE @M_System_FeeCodeTemplate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_FeeCodeTemplate,N'科目模板配置','system:feecodetemplate',@M_System_FeeCode,12,1,@SysUserId,@Now);

DECLARE @M_System_RoomTypeCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_RoomTypeCreate,N'新增房型','system:roomtypecreate',@M_System_RoomType,10,1,@SysUserId,@Now);

DECLARE @M_System_RoomTypeEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_RoomTypeEdit,N'编辑房型','system:roomtypeedit',@M_System_RoomType,11,1,@SysUserId,@Now);

DECLARE @M_System_RoomTypeDelete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_RoomTypeDelete,N'删除房型','system:roomtypedelete',@M_System_RoomType,12,1,@SysUserId,@Now);

DECLARE @M_System_PricingCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_PricingCreate,N'新增定价','system:pricingcreate',@M_System_Pricing,10,1,@SysUserId,@Now);

DECLARE @M_System_PricingEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_PricingEdit,N'编辑定价','system:pricingedit',@M_System_Pricing,11,1,@SysUserId,@Now);

DECLARE @M_System_PricingDelete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_PricingDelete,N'删除定价','system:pricingdelete',@M_System_Pricing,12,1,@SysUserId,@Now);

DECLARE @M_System_FloorLevelCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_FloorLevelCreate,N'新增楼层级别','system:floorlevelcreate',@M_System_Pricing,15,1,@SysUserId,@Now);

DECLARE @M_System_FloorLevelEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_FloorLevelEdit,N'编辑楼层级别','system:floorleveledit',@M_System_Pricing,16,1,@SysUserId,@Now);

DECLARE @M_System_FloorLevelDelete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_FloorLevelDelete,N'删除楼层级别','system:floorleveldelete',@M_System_Pricing,17,1,@SysUserId,@Now);

DECLARE @M_System_PaymentChannelCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_PaymentChannelCreate,N'新增通道','system:paymentchannelcreate',@M_System_PaymentChannel,10,1,@SysUserId,@Now);

DECLARE @M_System_PaymentChannelEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_PaymentChannelEdit,N'编辑通道','system:paymentchanneledit',@M_System_PaymentChannel,11,1,@SysUserId,@Now);

DECLARE @M_System_TaxRateCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_TaxRateCreate,N'新增税率','system:taxratecreate',@M_System_TaxRate,10,1,@SysUserId,@Now);

DECLARE @M_System_TaxRateEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_TaxRateEdit,N'编辑税率','system:taxrateedit',@M_System_TaxRate,11,1,@SysUserId,@Now);

DECLARE @M_System_SubjectCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SubjectCreate,N'新增科目','system:accountingsubjectcreate',@M_System_AccountingSubject,10,1,@SysUserId,@Now);

DECLARE @M_System_SubjectEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SubjectEdit,N'编辑科目','system:accountingsubjectedit',@M_System_AccountingSubject,11,1,@SysUserId,@Now);

DECLARE @M_System_HolidayCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_HolidayCreate,N'新增节假日','system:holidaycreate',@M_System_Holiday,10,1,@SysUserId,@Now);

DECLARE @M_System_HolidayImport uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_HolidayImport,N'导入节假日','system:holidayimport',@M_System_Holiday,11,1,@SysUserId,@Now);

DECLARE @M_System_HolidayEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_HolidayEdit,N'编辑节假日','system:holidayedit',@M_System_Holiday,12,1,@SysUserId,@Now);

DECLARE @M_System_HolidayDelete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_HolidayDelete,N'删除节假日','system:holidaydelete',@M_System_Holiday,13,1,@SysUserId,@Now);

DECLARE @M_System_InterestSave uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_InterestSave,N'保存配置','system:interestsave',@M_System_Interest,10,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerConfig uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerConfig,N'调度配置','system:schedulerconfig',@M_System_Scheduler,10,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerEdit,N'编辑调度任务','system:scheduleredit',@M_System_Scheduler,11,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerGenerate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerGenerate,N'批量生成排期','system:schedulergenerate',@M_System_Scheduler,12,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerAdd uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerAdd,N'添加自定义排期','system:scheduleradd',@M_System_Scheduler,13,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerViewLog uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerViewLog,N'查看日志','system:schedulerviewlog',@M_System_Scheduler,14,1,@SysUserId,@Now);


DECLARE @M_System_SchedulerCreate uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerCreate,N'新增调度任务','system:schedulercreate',@M_System_Scheduler,19,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerDelete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerDelete,N'删除调度任务','system:schedulerdelete',@M_System_Scheduler,20,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerExecute uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerExecute,N'手动执行任务','system:schedulerexecute',@M_System_Scheduler,21,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerExecEdit uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerExecEdit,N'编辑执行排期','system:schedulerexcedit',@M_System_Scheduler,22,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerExecDelete uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerExecDelete,N'删除执行排期','system:schedulerexecdelete',@M_System_Scheduler,23,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerReverse uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerReverse,N'反转出账','system:schedulerreverse',@M_System_Scheduler,24,1,@SysUserId,@Now);

DECLARE @M_System_SchedulerBatchDel uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerBatchDel,N'批量删除执行排期','system:schedulerexecbatchdelete',@M_System_Scheduler,25,1,@SysUserId,@Now);

-- ===== 调度执行监控（菜单项）=====
DECLARE @M_System_SchedulerMonitor uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_SchedulerMonitor,N'调度执行监控','system:monitor:view','/system/scheduler/monitor','DataLine',@M_System,17,1,@SysUserId,@Now);

DECLARE @M_System_MonitorRetry uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_MonitorRetry,N'重试任务','system:monitor:retry',@M_System_SchedulerMonitor,10,1,@SysUserId,@Now);

DECLARE @M_System_MonitorLogs uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_MonitorLogs,N'执行日志','system:monitor:logs','/system/scheduler/monitor/logs',@M_System_SchedulerMonitor,11,1,@SysUserId,@Now);

IF @AdminRoleId IS NOT NULL
    INSERT INTO [RoleMenus] ([Id],[RoleId],[MenuId],[CreatedBy],[CreatedAt])
    SELECT NEWID(), @AdminRoleId, @M_System_ApiLogs, @SysUserId, @Now
    WHERE NOT EXISTS (SELECT 1 FROM [RoleMenus] WHERE [RoleId] = @AdminRoleId AND [MenuId] = @M_System_ApiLogs);

INSERT INTO [RoleMenus] ([Id], [RoleId], [MenuId], [CreatedBy], [CreatedAt])
SELECT NEWID(), @AdminRoleId, M.[Id], @SysUserId, @Now
FROM [Menus] M
WHERE M.[IsActive] = 1
  AND NOT EXISTS (SELECT 1 FROM [RoleMenus] RM WHERE RM.[RoleId] = @AdminRoleId AND RM.[MenuId] = M.[Id]);

DECLARE @Cnt int = (SELECT COUNT(*) FROM [RoleMenus] WHERE [RoleId] = @AdminRoleId);
PRINT N'Admin 角色当前共有 ' + CAST(@Cnt AS nvarchar) + N' 个菜单权限。';
GO

-- ===================================================================
-- 12. 角色菜单映射-审批角色
-- ===================================================================
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Now datetime2 = GETDATE();

DECLARE @OpsSup uniqueidentifier = (SELECT Id FROM Roles WHERE Code='OpsSupervisor');
DECLARE @DeptMgr uniqueidentifier = (SELECT Id FROM Roles WHERE Code='DeptManager');
DECLARE @GenMgr uniqueidentifier = (SELECT Id FROM Roles WHERE Code='GeneralManager');
DECLARE @FinSup uniqueidentifier = (SELECT Id FROM Roles WHERE Code='FinanceSupervisor');
DECLARE @FinDir uniqueidentifier = (SELECT Id FROM Roles WHERE Code='FinanceDirector');

DELETE FROM RoleMenus WHERE RoleId IN (@OpsSup, @DeptMgr, @GenMgr, @FinSup, @FinDir);

INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), @OpsSup, M.Id, @SysUserId, @Now
FROM [Menus] M
WHERE M.PermissionCode IN (
  'dashboard:view',
  'building:list',
  'building:detail',
  'building:view',
  'approval:view',
  'approval:pending',
  'approval:myrequests',
  'approval:history',
  'approval:approve',
  'approval:reject',
  'notification:view',
  'notification:markallread',
  'contract:view',
  'contract:list',
  'contract:create',
  'contract:detail',
  'tenant:view',
  'tenant:list',
  'bill:view',
  'bill:list',
  'collection:view',
  'collection:overview',
  'meter:view',
  'meter:list',
  'report:view',
  'report:collectionrate',
  'report:overduedetail',
  'report:dailyreceipt',
  'report:monthlyreceipt',
  'receipt:view',
  'receipt:list',
  'system:scheduler',
  'system:schedulerviewlog',
  'system:monitor:view'
)
  AND NOT EXISTS (SELECT 1 FROM RoleMenus RM WHERE RM.RoleId = @OpsSup AND RM.MenuId = M.Id);

INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), @DeptMgr, MenuId, @SysUserId, @Now
FROM RoleMenus WHERE RoleId = @OpsSup;

INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), @GenMgr, MenuId, @SysUserId, @Now
FROM RoleMenus WHERE RoleId = @OpsSup;

INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), @FinSup, M.Id, @SysUserId, @Now
FROM [Menus] M
WHERE M.PermissionCode IN (
  'dashboard:view',
  'building:list',
  'approval:view',
  'approval:pending',
  'approval:myrequests',
  'approval:history',
  'approval:approve',
  'approval:reject',
  'notification:view',
  'notification:markallread',
  'receipt:view',
  'receipt:list',
  'receipt:register',
  'receipt:confirm',
  'bill:view',
  'bill:list',
  'accounting:view',
  'accounting:subjects',
  --'accounting:vouchers',（已删除）
  'journal:view',
  'gl:view',
  'report:view',
  'report:collectionrate',
  'report:overduedetail',
  'report:dailyreceipt',
  'report:monthlyreceipt',
  'report:feerevenue',
  'bank:view',
  'bank:import',
  'bank:match',
  'bank:reconciliation'
)
  AND NOT EXISTS (SELECT 1 FROM RoleMenus RM WHERE RM.RoleId = @FinSup AND RM.MenuId = M.Id);

INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), @FinDir, MenuId, @SysUserId, @Now
FROM RoleMenus WHERE RoleId = @FinSup;

SELECT r.Name AS RoleName, r.Code, COUNT(rm.MenuId) AS MenuCount
FROM Roles r JOIN RoleMenus rm ON rm.RoleId = r.Id
WHERE r.Code IN ('OpsSupervisor','DeptManager','GeneralManager','FinanceSupervisor','FinanceDirector')
GROUP BY r.Name, r.Code ORDER BY r.Name;
-- Accountant（会计）
INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), (SELECT Id FROM Roles WHERE Code='Accountant'), M.Id, @SysUserId, @Now
FROM [Menus] M
WHERE M.PermissionCode IN (
  'dashboard:view',
  'notification:view', 'notification:markallread',
  'receipt:view', 'receipt:list',
  'bill:view', 'bill:list',
  'journal:view', 'gl:view', 'accounting:view', 'accounting:subjects',
  'report:view', 'report:collectionrate', 'report:overduedetail', 'report:dailyreceipt', 'report:monthlyreceipt', 'report:feerevenue',
  'system:scheduler', 'system:schedulerviewlog', 'system:monitor:view'
)
  AND NOT EXISTS (SELECT 1 FROM RoleMenus RM WHERE RM.RoleId = (SELECT Id FROM Roles WHERE Code='Accountant') AND RM.MenuId = M.Id);

-- Operator（运营人员）
INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), (SELECT Id FROM Roles WHERE Code='Operator'), M.Id, @SysUserId, @Now
FROM [Menus] M
WHERE M.PermissionCode IN (
  'dashboard:view',
  'building:list', 'building:detail', 'building:view',
  'contract:view', 'contract:list', 'contract:create', 'contract:detail',
  'tenant:view', 'tenant:list', 'tenant:create', 'tenant:detail', 'tenant:edit',
  'approval:view', 'approval:myrequests', 'approval:history',
  'notification:view', 'notification:markallread',
  'bill:view', 'bill:list',
  'meter:view', 'meter:list',
  'report:view', 'report:collectionrate',
  'system:scheduler', 'system:schedulerviewlog', 'system:monitor:view'
)
  AND NOT EXISTS (SELECT 1 FROM RoleMenus RM WHERE RM.RoleId = (SELECT Id FROM Roles WHERE Code='Operator') AND RM.MenuId = M.Id);

-- Legal（法务）
INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), (SELECT Id FROM Roles WHERE Code='Legal'), M.Id, @SysUserId, @Now
FROM [Menus] M
WHERE M.PermissionCode IN (
  'dashboard:view',
  'contract:view', 'contract:list', 'contract:detail',
  'approval:view', 'approval:history',
  'notification:view', 'notification:markallread',
  'report:view'
)
  AND NOT EXISTS (SELECT 1 FROM RoleMenus RM WHERE RM.RoleId = (SELECT Id FROM Roles WHERE Code='Legal') AND RM.MenuId = M.Id);

-- Landlord（公司账号-只读）
INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), (SELECT Id FROM Roles WHERE Code='Landlord'), M.Id, @SysUserId, @Now
FROM [Menus] M
WHERE M.PermissionCode IN (
  'dashboard:view',
  'building:list', 'building:detail', 'building:view',
  'contract:list', 'contract:detail', 'contract:view',
  'tenant:list', 'tenant:detail', 'tenant:view',
  'bill:list', 'bill:view',
  'receipt:list', 'receipt:view',
  'report:view', 'report:collectionrate', 'report:overduedetail', 'report:dailyreceipt', 'report:monthlyreceipt'
)
  AND NOT EXISTS (SELECT 1 FROM RoleMenus RM WHERE RM.RoleId = (SELECT Id FROM Roles WHERE Code='Landlord') AND RM.MenuId = M.Id);

SELECT r.Name AS RoleName, r.Code, COUNT(rm.MenuId) AS MenuCount
FROM Roles r JOIN RoleMenus rm ON rm.RoleId = r.Id
WHERE r.Code IN ('Accountant','Operator','Legal','Landlord')
GROUP BY r.Name, r.Code ORDER BY r.Name;
GO
-- ===================================================================
-- SeedBase.sql - 结束
-- ===================================================================
