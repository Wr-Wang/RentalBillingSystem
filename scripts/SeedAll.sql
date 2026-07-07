-- ===================================================================
-- SeedAll.sql - 全量种子数据（动态 GUID 版本）
-- 合并自所有独立种子文件，按数据依赖关系排列
-- 所有 GUID 均通过 NEWID() 动态生成，无任何硬编码 GUID 字符串
-- ===================================================================

-- ===================================================================
-- 1. 公司 + 2. 角色 + 3. 用户
-- ===================================================================
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

-- ======== 1. 公司 ========
DECLARE @GS001Id uniqueidentifier;
DECLARE @GS002Id uniqueidentifier;
DECLARE @GS003Id uniqueidentifier;
DECLARE @GS004Id uniqueidentifier;

IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS001')
    INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'上海茂源置业有限公司','GS001',N'张建国','13912345678',N'上海市浦东新区陆家嘴金融中心A座',1,@SysUserId,@Now);
SELECT @GS001Id = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS002')
    INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'南京恒达物业管理有限公司','GS002',N'李春华','13898765432',N'南京市鼓楼区新街口广场B座',1,@SysUserId,@Now);
SELECT @GS002Id = [Id] FROM [Companies] WHERE [Code] = 'GS002';

IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS003')
    INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'深圳万方投资发展有限公司','GS003',N'王芳','13655556666',N'深圳市南山区科技园C栋',1,@SysUserId,@Now);
SELECT @GS003Id = [Id] FROM [Companies] WHERE [Code] = 'GS003';

IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS004')
    INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'广州天恒物业管理有限公司','GS004',N'赵德明','13777778888',N'广州市天河区珠江新城D栋',0,@SysUserId,@Now);
SELECT @GS004Id = [Id] FROM [Companies] WHERE [Code] = 'GS004';

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
    VALUES (@U_AdminId,'admin','123456',N'系统管理员','13800138000','admin@rental.com',1,1,@SysUserId,@Now);
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
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_ZhangsanId,'zhangsan','123456',N'张山','13800138001','zhangsan@rental.com',1,0,@GS001Id,@SysUserId,@Now);
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
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_LisiId,'lisi','123456',N'李思','13800138002','lisi@rental.com',1,0,@GS002Id,@SysUserId,@Now);
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
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_WangwuId,'wangwu','123456',N'王武','13800138003','wangwu@rental.com',1,0,@GS001Id,@SysUserId,@Now);
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
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_ZhaoliuId,'zhaoliu','123456',N'赵柳','13800138004','zhaoliu@rental.com',1,0,@GS003Id,@SysUserId,@Now);
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
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES (@U_CompanyAId,'company_a','123456',N'张建国（茂源）','13912345678','company_a@rental.com',1,0,@GS001Id,@SysUserId,@Now);
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
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'开间/单间',N'开放式一体的居住空间',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'一室一厅')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'一室一厅',N'一间卧室加独立客厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'两室一厅')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'两室一厅',N'两间卧室加独立客厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'两室两厅')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'两室两厅',N'两间卧室加独立客厅和餐厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'三室一厅')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'三室一厅',N'三间卧室加独立客厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'三室两厅')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'三室两厅',N'三间卧室加独立客厅和餐厅',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'四室及以上')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'四室及以上',N'四间及以上卧室',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'主卧')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'主卧',N'合租主卧（带独立卫生间）',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'次卧')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'次卧',N'合租次卧（共用卫生间）',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [RoomTypes] WHERE [Name] = N'公寓')
    INSERT INTO [RoomTypes] ([Id],[Name],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'公寓',N'酒店式公寓/服务式公寓',1,@SysUserId,@Now);

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

IF NOT EXISTS (SELECT 1 FROM [FeeCodes] WHERE [Code] = 'LATE_FEE')
    INSERT INTO [FeeCodes] ([Id],[Code],[Name],[BillingMode],[SortOrder],[Category],[ChargeType],[IsActive],[IsRequired],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'LATE_FEE',N'滞纳金','FixedAmount',99,'Other','Recurring',1,0,@Cid,@SysUserId,@Now);

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
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'低层',1,5,N'低层',@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'中层',6,12,N'中层',@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'高层',13,17,N'高层',@SysUserId,@Now);
    INSERT INTO [FloorLevelBands] ([Id],[Name],[MinLevel],[MaxLevel],[Description],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'顶层',18,99,N'顶层',@SysUserId,@Now);
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

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_RENEW')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'合同续签','CONTRACT_RENEW',N'合同续签需要审批，根据月租金额自动路由审批级别',1,@Cid,@SysUserId,@Now);
SELECT @AT_ContractRenew = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CONTRACT_RENEW';

IF NOT EXISTS (SELECT 1 FROM [ApprovalTypes] WHERE [Code] = 'CHANGE_REQUEST')
    INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'合同变更','CHANGE_REQUEST',N'合同信息变更请求需要审批',1,@Cid,@SysUserId,@Now);
SELECT @AT_ChangeRequest = [Id] FROM [ApprovalTypes] WHERE [Code] = 'CHANGE_REQUEST';

DECLARE @R_OpsSup uniqueidentifier; SELECT @R_OpsSup = [Id] FROM [Roles] WHERE [Code] = 'OpsSupervisor';
DECLARE @R_DeptMgr uniqueidentifier; SELECT @R_DeptMgr = [Id] FROM [Roles] WHERE [Code] = 'DeptManager';
DECLARE @R_FinSup uniqueidentifier; SELECT @R_FinSup = [Id] FROM [Roles] WHERE [Code] = 'FinanceSupervisor';
DECLARE @R_FinDir uniqueidentifier; SELECT @R_FinDir = [Id] FROM [Roles] WHERE [Code] = 'FinanceDirector';
DECLARE @R_GenMgr uniqueidentifier; SELECT @R_GenMgr = [Id] FROM [Roles] WHERE [Code] = 'GeneralManager';

IF @AT_BatchImport IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_BatchImport AND [Level] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_BatchImport,1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_BatchImport IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_BatchImport AND [Level] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_BatchImport,2,@R_DeptMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_BatchImport IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_BatchImport AND [Level] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_BatchImport,3,@R_GenMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractCreate IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractCreate AND [Level] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractCreate,1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractCreate IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractCreate AND [Level] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractCreate,2,@R_DeptMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractCreate IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractCreate AND [Level] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractCreate,3,@R_GenMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractTerminate IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractTerminate AND [Level] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractTerminate,1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);

IF @AT_ContractTerminate IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractTerminate AND [Level] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractTerminate,2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);

IF @AT_ContractTerminate IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractTerminate AND [Level] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractTerminate,3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

IF @AT_ReceiptReverse IS NOT NULL AND @R_FinSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ReceiptReverse AND [Level] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ReceiptReverse,1,@R_FinSup,0,50000,@Cid,@SysUserId,@Now);

IF @AT_ReceiptReverse IS NOT NULL AND @R_FinDir IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ReceiptReverse AND [Level] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ReceiptReverse,2,@R_FinDir,50000,99999999,@Cid,@SysUserId,@Now);

IF @AT_ReceiptReverse IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ReceiptReverse AND [Level] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ReceiptReverse,3,@R_GenMgr,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_Discount IS NOT NULL AND @R_FinSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_Discount AND [Level] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_Discount,1,@R_FinSup,0,5000,@Cid,@SysUserId,@Now);

IF @AT_Discount IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_Discount AND [Level] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_Discount,2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);

IF @AT_Discount IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_Discount AND [Level] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_Discount,3,@R_GenMgr,50000,99999999,@Cid,@SysUserId,@Now);

IF @AT_ContractModify IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractModify AND [Level] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractModify,1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);

IF @AT_ContractModify IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractModify AND [Level] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractModify,2,@R_DeptMgr,5000,99999999,@Cid,@SysUserId,@Now);

IF @AT_ChangeRequest IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ChangeRequest AND [Level] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ChangeRequest,1,@R_OpsSup,NULL,NULL,@Cid,@SysUserId,@Now);

IF @AT_ContractRenew IS NOT NULL AND @R_OpsSup IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractRenew AND [Level] = 1)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractRenew,1,@R_OpsSup,0,5000,@Cid,@SysUserId,@Now);

IF @AT_ContractRenew IS NOT NULL AND @R_DeptMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractRenew AND [Level] = 2)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AT_ContractRenew,2,@R_DeptMgr,5000,50000,@Cid,@SysUserId,@Now);

IF @AT_ContractRenew IS NOT NULL AND @R_GenMgr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [ApprovalLevelConfigs] WHERE [ApprovalTypeId] = @AT_ContractRenew AND [Level] = 3)
    INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
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
VALUES (@M_Accounting,N'会计核算','accounting:view','/accounting','Files',NULL,12,1,@SysUserId,@Now);

DECLARE @M_Bank uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Bank,N'银企直连','bank:view','/bank','Link',NULL,13,1,@SysUserId,@Now);

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

DECLARE @M_Accounting_Journal uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Accounting_Journal,N'日记账','accounting:journal','/accounting/journal',@M_Accounting,2,1,@SysUserId,@Now);

DECLARE @M_Accounting_Vouchers uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Accounting_Vouchers,N'凭证管理','accounting:vouchers','/accounting/vouchers',@M_Accounting,3,1,@SysUserId,@Now);

DECLARE @M_Accounting_TrialBalance uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_Accounting_TrialBalance,N'试算平衡表','accounting:trialbalance','/accounting/trialbalance',@M_Accounting,4,1,@SysUserId,@Now);

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

DECLARE @M_System_LateFee uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_LateFee,N'滞纳金配置','system:latefee','/system/latefee','WarningFilled',@M_System,15,1,@SysUserId,@Now);

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

DECLARE @M_System_LateFeeSave uniqueidentifier = NEWID();
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES (@M_System_LateFeeSave,N'保存配置','system:latefeesave',@M_System_LateFee,10,1,@SysUserId,@Now);

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
  'accounting:journal',
  'accounting:vouchers',
  'accounting:trialbalance',
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
-- Accountant（会计）
INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedBy, CreatedAt)
SELECT NEWID(), (SELECT Id FROM Roles WHERE Code='Accountant'), M.Id, @SysUserId, @Now
FROM [Menus] M
WHERE M.PermissionCode IN (
  'dashboard:view',
  'notification:view', 'notification:markallread',
  'receipt:view', 'receipt:list',
  'bill:view', 'bill:list',
  'accounting:view', 'accounting:subjects', 'accounting:journal', 'accounting:vouchers', 'accounting:trialbalance',
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

GROUP BY r.Name, r.Code ORDER BY r.Name;
GO

-- ===================================================================
-- 13. 任务模板（Cron 表达式版本）
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'MonthlyFeeBill')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'MonthlyFeeBill',N'月度应收生成',N'月度应收','0 0 8 25 * ?',N'每月25日 08:00 生成月度应收账单','Calendar','Billing',1,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'LateFeeCalc')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'LateFeeCalc',N'滞纳金计算',N'滞纳金','0 0 2 * * ?',N'每天 02:00 计算滞纳金','Money','Billing',2,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'AutoRenew')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'AutoRenew',N'自动续签',N'续签','0 0 0 * * ?',N'每天 00:00 自动续签到期的合同','RefreshRight','Contract',3,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'Collection')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'Collection',N'催缴任务',N'催缴','0 0 9 * * ?',N'每天 09:00 执行催缴任务','Bell','Collection',4,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'RenewalReminder')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultCronExpression],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'RenewalReminder',N'续签提醒',N'续签提醒','0 0 8 * * ?',N'每天 08:00 提醒运营人员合同即将到期','Notifications','Renewal',5,1,@SysUserId,@Now);

PRINT N'任务模板种子数据初始化完成！';
GO

-- ===================================================================
-- 14. 调度任务 + 公司实例
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'MonthlyFeeBill')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[DefaultDayOfMonth],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'MonthlyFeeBill',N'月度应收生成',N'月度应收','Monthly',8,0,25,N'每月25日 08:00 生成月度应收账单','Calendar','Billing',1,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'LateFeeCalc')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'LateFeeCalc',N'滞纳金计算',N'滞纳金','Daily',2,0,N'每天 02:00 计算滞纳金','Money','Billing',2,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'AutoRenew')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'AutoRenew',N'自动续签',N'续签','Daily',0,0,N'每天 00:00 自动续签到期的合同','RefreshRight','Contract',3,1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobTemplates] WHERE [Code] = 'Collection')
    INSERT INTO [JobTemplates] ([Id],[Code],[DisplayName],[ShortName],[DefaultScheduleType],[DefaultHour],[DefaultMinute],[Description],[Icon],[Category],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),'Collection',N'催缴任务',N'催缴','Daily',9,0,N'每天 09:00 执行催缴任务','Bell','Collection',4,1,@SysUserId,@Now);

DELETE FROM [JobSchedules] WHERE [CompanyId] = @Cid AND [JobName] IN (
  N'📅 月度应收生成', N'💰 滞纳金计算', N'🔄 自动续签', N'📢 催缴任务', N'🔔 续签提醒');

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

IF NOT EXISTS (SELECT 1 FROM [JobSchedules] WHERE [JobName] = N'🔔 续签提醒' AND [CompanyId] = @Cid)
    INSERT INTO [JobSchedules] ([Id],[JobName],[ScheduleType],[Hour],[Minute],[Description],[IsActive],[CompanyId],[TemplateCode],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'🔔 续签提醒','Daily',8,0,N'每天 08:00 提醒运营',1,@Cid,'RenewalReminder',@SysUserId,@Now);

PRINT N'调度任务初始化完成！';
GO

-- ===================================================================
-- 15. 排期执行实例
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

DECLARE @BillJobId  uniqueidentifier; SELECT @BillJobId  = [Id] FROM [JobSchedules] WHERE [JobName]=N'BillJob' AND [CompanyId]=@Cid;
DECLARE @SettleJobId     uniqueidentifier; SELECT @SettleJobId     = [Id] FROM [JobSchedules] WHERE [JobName]=N'SettleJob' AND [CompanyId]=@Cid;
DECLARE @AutoRenewJobId       uniqueidentifier; SELECT @AutoRenewJobId       = [Id] FROM [JobSchedules] WHERE [JobName]=N'AutoRenewJob' AND [CompanyId]=@Cid;
DECLARE @CollectionJobId      uniqueidentifier; SELECT @CollectionJobId      = [Id] FROM [JobSchedules] WHERE [JobName]=N'CollectionJob' AND [CompanyId]=@Cid;
DECLARE @RenewalReminderJobId uniqueidentifier; SELECT @RenewalReminderJobId = [Id] FROM [JobSchedules] WHERE [JobName]=N'RenewalReminderJob' AND [CompanyId]=@Cid;

IF @BillJobId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@BillJobId AND [Month]='2026-07' AND [IsCustom]=0)
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@BillJobId,@Cid,'2026-07-24T08:00:00','2026-07-25T08:00:00','2026-07','Pending',N'25日逢周六，提前至24日',1,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@BillJobId AND [Month]='2026-08' AND [IsCustom]=0)
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@BillJobId,@Cid,'2026-08-25T08:00:00','2026-08-25T08:00:00','2026-08','Pending',N'默认',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@BillJobId AND [Month]='2026-09' AND [IsCustom]=0)
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@BillJobId,@Cid,'2026-09-25T08:00:00','2026-09-25T08:00:00','2026-09','Pending',N'默认',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@BillJobId AND [Month]='2026-07' AND [IsCustom]=1)
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@BillJobId,@Cid,'2026-07-15T14:30:00',NULL,'2026-07','Pending',N'月中临时加跑一次核对',1,1,@SysUserId,@Now);

END

IF @SettleJobId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@SettleJobId AND [Month]='2026-07')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@SettleJobId,@Cid,'2026-07-01T02:00:00','2026-07-01T02:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@SettleJobId AND [Month]='2026-08')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@SettleJobId,@Cid,'2026-08-01T02:00:00','2026-08-01T02:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@SettleJobId AND [Month]='2026-09')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@SettleJobId,@Cid,'2026-09-01T02:00:00','2026-09-01T02:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now);

END

IF @AutoRenewJobId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@AutoRenewJobId AND [Month]='2026-07')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AutoRenewJobId,@Cid,'2026-07-01T00:00:00','2026-07-01T00:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@AutoRenewJobId AND [Month]='2026-08')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AutoRenewJobId,@Cid,'2026-08-01T00:00:00','2026-08-01T00:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@AutoRenewJobId AND [Month]='2026-09')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@AutoRenewJobId,@Cid,'2026-09-01T00:00:00','2026-09-01T00:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now);

END

IF @CollectionJobId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@CollectionJobId AND [Month]='2026-06')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@CollectionJobId,@Cid,'2026-06-01T09:00:00','2026-06-01T09:00:00','2026-06','Success',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@CollectionJobId AND [Month]='2026-07')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@CollectionJobId,@Cid,'2026-07-01T09:00:00','2026-07-01T09:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@CollectionJobId AND [Month]='2026-08')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@CollectionJobId,@Cid,'2026-08-01T09:00:00','2026-08-01T09:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@CollectionJobId AND [Month]='2026-09')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@CollectionJobId,@Cid,'2026-09-01T09:00:00','2026-09-01T09:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now);

END

IF @RenewalReminderJobId IS NOT NULL BEGIN
IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@RenewalReminderJobId AND [Month]='2026-07')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@RenewalReminderJobId,@Cid,'2026-07-01T08:00:00','2026-07-01T08:00:00','2026-07','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@RenewalReminderJobId AND [Month]='2026-08')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@RenewalReminderJobId,@Cid,'2026-08-01T08:00:00','2026-08-01T08:00:00','2026-08','Pending',N'每日执行',0,0,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [JobScheduleExecutions] WHERE [JobScheduleId]=@RenewalReminderJobId AND [Month]='2026-09')
    INSERT INTO [JobScheduleExecutions] ([Id],[JobScheduleId],[CompanyId],[TargetDate],[OriginalDate],[Month],[Status],[Reason],[IsAdjusted],[IsCustom],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@RenewalReminderJobId,@Cid,'2026-09-01T08:00:00','2026-09-01T08:00:00','2026-09','Pending',N'每日执行',0,0,@SysUserId,@Now);

END

PRINT N'排期种子数据初始化完成！';
GO

-- ===================================================================
-- 16. 催缴阶段
-- ===================================================================
DECLARE @Now datetime2 = GETUTCDATE(); DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Cid uniqueidentifier; SELECT @Cid = [Id] FROM [Companies] WHERE [Code] = 'GS001';

IF NOT EXISTS (SELECT 1 FROM [CollectionStages] WHERE [Name]=N'逾期提醒' AND [CompanyId]=@Cid)
    INSERT INTO [CollectionStages] ([Id],[Name],[DaysOverdue],[Action],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'逾期提醒',7,N'SMS',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [CollectionStages] WHERE [Name]=N'电话催缴' AND [CompanyId]=@Cid)
    INSERT INTO [CollectionStages] ([Id],[Name],[DaysOverdue],[Action],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'电话催缴',15,N'CALL',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [CollectionStages] WHERE [Name]=N'上门催缴' AND [CompanyId]=@Cid)
    INSERT INTO [CollectionStages] ([Id],[Name],[DaysOverdue],[Action],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'上门催缴',30,N'VISIT',1,@Cid,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [CollectionStages] WHERE [Name]=N'律师函' AND [CompanyId]=@Cid)
    INSERT INTO [CollectionStages] ([Id],[Name],[DaysOverdue],[Action],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'律师函',60,N'LEGAL',1,@Cid,@SysUserId,@Now);

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
INSERT INTO [LateFeeConfigs] ([Id],[DailyRate],[GraceDays],[MaxRate],[MinAmount],[EffectiveDate],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES (NEWID(),0.0005,3,100.00,1.00,'2026-01-01',1,@Cid,@SysUserId,@Now);
GO

-- ===================================================================
-- 18. 房源（含完整属性：面积、朝向、基础租金、房型）
-- ===================================================================
DECLARE @Sys uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Now datetime2 = GETDATE();
DECLARE @GS001Id uniqueidentifier; SELECT @GS001Id = [Id] FROM [Companies] WHERE [Code] = 'GS001';
DECLARE @GS002Id uniqueidentifier; SELECT @GS002Id = [Id] FROM [Companies] WHERE [Code] = 'GS002';
DECLARE @GS003Id uniqueidentifier; SELECT @GS003Id = [Id] FROM [Companies] WHERE [Code] = 'GS003';
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

-- D栋（8套）- 南京鼓楼 GS002
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'D栋',N'D',N'南京市鼓楼区新街口广场D座',@GS002Id,N'1层',1,N'101',N'D栋-1层-101',@Studio,60,N'南',3200,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'D栋',N'D',N'南京市鼓楼区新街口广场D座',@GS002Id,N'1层',1,N'102',N'D栋-1层-102',@TwoBr,80,N'北',4200,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'D栋',N'D',N'南京市鼓楼区新街口广场D座',@GS002Id,N'2层',2,N'201',N'D栋-2层-201',@TwoBr,88,N'南',4800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'D栋',N'D',N'南京市鼓楼区新街口广场D座',@GS002Id,N'2层',2,N'202',N'D栋-2层-202',@Studio,62,N'东',3500,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'D栋',N'D',N'南京市鼓楼区新街口广场D座',@GS002Id,N'3层',3,N'301',N'D栋-3层-301',@ThreeBr,118,N'南',6200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'D栋',N'D',N'南京市鼓楼区新街口广场D座',@GS002Id,N'3层',3,N'302',N'D栋-3层-302',@TwoBr,85,N'西',4600,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'D栋',N'D',N'南京市鼓楼区新街口广场D座',@GS002Id,N'4层',4,N'401',N'D栋-4层-401',@ThreeBrTwo,135,N'南',7200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'D栋',N'D',N'南京市鼓楼区新街口广场D座',@GS002Id,N'4层',4,N'402',N'D栋-4层-402',@TwoBr,90,N'北',5000,'Vacant',@Sys,@Now);

-- E栋（8套）- 深圳南山 GS003
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'E栋',N'E',N'深圳市南山区科技园E栋',@GS003Id,N'1层',1,N'101',N'E栋-1层-101',@Studio,65,N'南',3800,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'E栋',N'E',N'深圳市南山区科技园E栋',@GS003Id,N'1层',1,N'102',N'E栋-1层-102',@TwoBr,82,N'北',4800,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'E栋',N'E',N'深圳市南山区科技园E栋',@GS003Id,N'2层',2,N'201',N'E栋-2层-201',@TwoBr,90,N'南',5400,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'E栋',N'E',N'深圳市南山区科技园E栋',@GS003Id,N'2层',2,N'202',N'E栋-2层-202',@Studio,68,N'东',4000,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'E栋',N'E',N'深圳市南山区科技园E栋',@GS003Id,N'3层',3,N'301',N'E栋-3层-301',@ThreeBr,120,N'南',6800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'E栋',N'E',N'深圳市南山区科技园E栋',@GS003Id,N'3层',3,N'302',N'E栋-3层-302',@TwoBr,88,N'西',5200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'E栋',N'E',N'深圳市南山区科技园E栋',@GS003Id,N'4层',4,N'401',N'E栋-4层-401',@ThreeBrTwo,132,N'南',7600,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'E栋',N'E',N'深圳市南山区科技园E栋',@GS003Id,N'4层',4,N'402',N'E栋-4层-402',@TwoBr,92,N'北',5600,'Vacant',@Sys,@Now);

-- F栋（8套）- 深圳南山 GS003
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'F栋',N'F',N'深圳市南山区科技园F栋',@GS003Id,N'1层',1,N'101',N'F栋-1层-101',@Studio,60,N'南',3600,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'F栋',N'F',N'深圳市南山区科技园F栋',@GS003Id,N'1层',1,N'102',N'F栋-1层-102',@TwoBr,78,N'北',4600,'Rented',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'F栋',N'F',N'深圳市南山区科技园F栋',@GS003Id,N'2层',2,N'201',N'F栋-2层-201',@TwoBr,88,N'南',5200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'F栋',N'F',N'深圳市南山区科技园F栋',@GS003Id,N'2层',2,N'202',N'F栋-2层-202',@Studio,65,N'东',3800,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'F栋',N'F',N'深圳市南山区科技园F栋',@GS003Id,N'3层',3,N'301',N'F栋-3层-301',@ThreeBr,118,N'南',6500,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'F栋',N'F',N'深圳市南山区科技园F栋',@GS003Id,N'3层',3,N'302',N'F栋-3层-302',@TwoBr,85,N'西',5000,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'F栋',N'F',N'深圳市南山区科技园F栋',@GS003Id,N'4层',4,N'401',N'F栋-4层-401',@ThreeBrTwo,130,N'南',7200,'Vacant',@Sys,@Now);
INSERT INTO HousingUnits (Id,BuildingName,BuildingCode,BuildingAddress,CompanyId,FloorName,FloorSortOrder,UnitNo,FullCode,RoomTypeId,Area,Orientation,BaseRentAmount,Status,CreatedBy,CreatedAt) VALUES (NEWID(),N'F栋',N'F',N'深圳市南山区科技园F栋',@GS003Id,N'4层',4,N'402',N'F栋-4层-402',@TwoBr,90,N'北',5400,'Vacant',@Sys,@Now);

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

DECLARE @TenantZS uniqueidentifier;
DECLARE @TenantLS uniqueidentifier;
DECLARE @TenantWW uniqueidentifier;
DECLARE @TenantZL uniqueidentifier;

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [Name]=N'张三')
    INSERT INTO [Tenants] ([Id],[Name],[Phone],[IdCard],[CompanyId],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'张三','13800138001',N'310101199001011234',@Cid,1,@SysUserId,@Now);
SELECT @TenantZS = [Id] FROM [Tenants] WHERE [Name]=N'张三';

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [Name]=N'李四')
    INSERT INTO [Tenants] ([Id],[Name],[Phone],[IdCard],[CompanyId],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'李四','13900139002',N'310101199002022345',@Cid,1,@SysUserId,@Now);
SELECT @TenantLS = [Id] FROM [Tenants] WHERE [Name]=N'李四';

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [Name]=N'王五')
    INSERT INTO [Tenants] ([Id],[Name],[Phone],[IdCard],[CompanyId],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'王五','13700137003',N'310101199003033456',@Cid,1,@SysUserId,@Now);
SELECT @TenantWW = [Id] FROM [Tenants] WHERE [Name]=N'王五';

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [Name]=N'赵六')
    INSERT INTO [Tenants] ([Id],[Name],[Phone],[IdCard],[CompanyId],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),N'赵六','13600136004',N'310101199004044567',@Cid,1,@SysUserId,@Now);
SELECT @TenantZL = [Id] FROM [Tenants] WHERE [Name]=N'赵六';

DECLARE @Contract1 uniqueidentifier;
DECLARE @Contract2 uniqueidentifier;
DECLARE @Contract3 uniqueidentifier;
DECLARE @Contract4 uniqueidentifier;

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

SET @Contract4 = NEWID();
INSERT INTO [Contracts] ([Id],[ContractNo],[RoomId],[StartDate],[EndDate],[PaymentCycle],[Status],[CompanyId],[CreatedBy],[CreatedAt])
SELECT @Contract4,'HT-2026-004',Id,'2026-01-01','2026-06-30','Monthly','Expired',@Cid,@ZhangsanUserId,@Now
FROM HousingUnits WHERE UnitNo='101' AND CompanyId=@Cid ORDER BY FullCode OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;

IF @Contract1 IS NOT NULL AND @TenantZS IS NOT NULL
    INSERT INTO [ContractTenants] ([Id],[ContractId],[TenantId],[IsPrimary],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract1,@TenantZS,1,@SysUserId,@Now);

IF @Contract2 IS NOT NULL AND @TenantLS IS NOT NULL
    INSERT INTO [ContractTenants] ([Id],[ContractId],[TenantId],[IsPrimary],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract2,@TenantLS,1,@SysUserId,@Now);

IF @Contract3 IS NOT NULL AND @TenantWW IS NOT NULL
    INSERT INTO [ContractTenants] ([Id],[ContractId],[TenantId],[IsPrimary],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract3,@TenantWW,1,@SysUserId,@Now);

IF @Contract4 IS NOT NULL AND @TenantZL IS NOT NULL
    INSERT INTO [ContractTenants] ([Id],[ContractId],[TenantId],[IsPrimary],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract4,@TenantZL,1,@SysUserId,@Now);

DECLARE @RentFeeCodeId uniqueidentifier; SELECT @RentFeeCodeId=[Id] FROM [FeeCodes] WHERE [Code]='RENT' AND [CompanyId]=@Cid;

IF @Contract1 IS NOT NULL AND @RentFeeCodeId IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract1,@RentFeeCodeId,'FixedAmount',5200,1,@SysUserId,@Now);

IF @Contract2 IS NOT NULL AND @RentFeeCodeId IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract2,@RentFeeCodeId,'FixedAmount',3800,1,@SysUserId,@Now);

IF @Contract3 IS NOT NULL AND @RentFeeCodeId IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract3,@RentFeeCodeId,'FixedAmount',6800,1,@SysUserId,@Now);

IF @Contract4 IS NOT NULL AND @RentFeeCodeId IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract4,@RentFeeCodeId,'FixedAmount',5000,1,@SysUserId,@Now);

-- ===== 押金配置（一次性收费）=====
DECLARE @DepositFeeCodeId uniqueidentifier; SELECT @DepositFeeCodeId=[Id] FROM [FeeCodes] WHERE [Code]='DEPOSIT' AND [CompanyId]=@Cid;

IF @Contract1 IS NOT NULL AND @DepositFeeCodeId IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract1,@DepositFeeCodeId,'FixedAmount',10400,1,@SysUserId,@Now);

IF @Contract2 IS NOT NULL AND @DepositFeeCodeId IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract2,@DepositFeeCodeId,'FixedAmount',7600,1,@SysUserId,@Now);

IF @Contract3 IS NOT NULL AND @DepositFeeCodeId IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract3,@DepositFeeCodeId,'FixedAmount',13600,1,@SysUserId,@Now);

IF @Contract4 IS NOT NULL AND @DepositFeeCodeId IS NOT NULL
    INSERT INTO [ContractFeeConfigs] ([Id],[ContractId],[FeeCodeId],[BillingMode],[Amount],[IsActive],[CreatedBy],[CreatedAt])
    VALUES (NEWID(),@Contract4,@DepositFeeCodeId,'FixedAmount',10000,1,@SysUserId,@Now);

PRINT 'SeedAll.sql 全量种子数据执行完成！';
GO
