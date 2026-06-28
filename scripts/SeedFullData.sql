-- ===================================================================
-- 全量测试数据初始化：公司 + 角色 + 用户
-- 执行前请确认已运行 SeedMenus_AllInOne.sql 初始化菜单
-- ===================================================================

DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

-- ==================== 1. 公司数据 ====================
-- 先清除（谨慎使用）
-- DELETE FROM [Companies];

IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS001')
INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111001',N'上海茂源置业有限公司','GS001',N'张建国','13912345678',N'上海市浦东新区陆家嘴金融中心A座',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS002')
INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111002',N'南京恒达物业管理有限公司','GS002',N'李春华','13898765432',N'南京市鼓楼区新街口广场B座',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS003')
INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111003',N'深圳万方投资发展有限公司','GS003',N'王芳','13655556666',N'深圳市南山区科技园C栋',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Companies] WHERE [Code] = 'GS004')
INSERT INTO [Companies] ([Id],[Name],[Code],[ContactPerson],[Phone],[Address],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111004',N'广州天恒物业管理有限公司','GS004',N'赵德明','13777778888',N'广州市天河区珠江新城D栋',0,@SysUserId,@Now);

PRINT N'公司数据初始化完成';

-- ==================== 2. 角色数据（如不存在则创建） ====================
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Admin')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111001',N'系统管理员','Admin',N'系统配置、用户管理、审批流程',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'OpsSupervisor')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111002',N'运营主管','OpsSupervisor',N'审核合同、费用、抄表等日常运营事务',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Operator')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111003',N'运营人员','Operator',N'日常房屋、合同、租客等操作',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'FinanceSupervisor')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111004',N'财务主管','FinanceSupervisor',N'审核收款、会计、对账等财务事务',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'FinanceDirector')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111005',N'财务总监','FinanceDirector',N'财务报表审核、资金调度审批',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Accountant')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111006',N'会计','Accountant',N'日常记账、凭证处理',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'DeptManager')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111007',N'部门经理','DeptManager',N'部门业务审批',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'GeneralManager')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111008',N'总经理','GeneralManager',N'公司级业务审批、决策',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Legal')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111009',N'法务','Legal',N'合同法务审核、纠纷处理',1,@SysUserId,@Now);

IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Landlord')
    INSERT INTO [Roles] ([Id],[Name],[Code],[Description],[IsActive],[CreatedBy],[CreatedAt])
    VALUES ('B1111111-1111-1111-1111-111111111010',N'公司账号（只读）','Landlord',N'归属公司账号，仅可查看本公司数据',1,@SysUserId,@Now);

PRINT N'角色数据初始化完成';

-- ==================== 3. 获取角色 ID ====================
DECLARE @R_AdminId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'Admin');
DECLARE @R_OpsSupId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'OpsSupervisor');
DECLARE @R_OperId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'Operator');
DECLARE @R_FinSupId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'FinanceSupervisor');
DECLARE @R_FinDirId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'FinanceDirector');
DECLARE @R_AccId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'Accountant');
DECLARE @R_DeptMgrId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'DeptManager');
DECLARE @R_GenMgrId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'GeneralManager');
DECLARE @R_LegalId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'Legal');
DECLARE @R_LandlordId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'Landlord');

-- ==================== 4. 用户数据 ====================
-- 清除旧数据（谨慎使用）
-- DELETE FROM [UserRoles];
-- DELETE FROM [Users];

-- admin / 123456（超级管理员）
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[CreatedBy],[CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111001','admin','123456',N'系统管理员','13800138000','admin@rental.com',1,1,@SysUserId,@Now);
    IF @R_AdminId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),'C1111111-1111-1111-1111-111111111001',@R_AdminId,@SysUserId,@Now);
END

-- zhangsan / 123456（运营主管 → 归属上海茂源）
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'zhangsan')
BEGIN
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111002','zhangsan','123456',N'张山','13800138001','zhangsan@rental.com',1,0,'A1111111-1111-1111-1111-111111111001',@SysUserId,@Now);
    IF @R_OpsSupId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),'C1111111-1111-1111-1111-111111111002',@R_OpsSupId,@SysUserId,@Now);
END

-- lisi / 123456（运营人员 → 归属南京恒达）
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'lisi')
BEGIN
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111003','lisi','123456',N'李思','13800138002','lisi@rental.com',1,0,'A1111111-1111-1111-1111-111111111002',@SysUserId,@Now);
    IF @R_OperId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),'C1111111-1111-1111-1111-111111111003',@R_OperId,@SysUserId,@Now);
END

-- wangwu / 123456（财务主管 → 归属上海茂源）
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'wangwu')
BEGIN
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111004','wangwu','123456',N'王武','13800138003','wangwu@rental.com',1,0,'A1111111-1111-1111-1111-111111111001',@SysUserId,@Now);
    IF @R_FinSupId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),'C1111111-1111-1111-1111-111111111004',@R_FinSupId,@SysUserId,@Now);
END

-- zhaoliu / 123456（会计 → 归属深圳万方）
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'zhaoliu')
BEGIN
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111005','zhaoliu','123456',N'赵柳','13800138004','zhaoliu@rental.com',1,0,'A1111111-1111-1111-1111-111111111003',@SysUserId,@Now);
    IF @R_AccId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),'C1111111-1111-1111-1111-111111111005',@R_AccId,@SysUserId,@Now);
END

-- company_a / 123456（公司只读账号 → 归属上海茂源）
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'company_a')
BEGIN
    INSERT INTO [Users] ([Id],[Username],[PasswordHash],[DisplayName],[Phone],[Email],[IsActive],[IsSuperAdmin],[HomeCompanyId],[CreatedBy],[CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111006','company_a','123456',N'张建国（茂源）','13912345678','company_a@rental.com',1,0,'A1111111-1111-1111-1111-111111111001',@SysUserId,@Now);
    IF @R_LandlordId IS NOT NULL
        INSERT INTO [UserRoles] ([Id],[UserId],[RoleId],[CreatedBy],[CreatedAt])
        VALUES (NEWID(),'C1111111-1111-1111-1111-111111111006',@R_LandlordId,@SysUserId,@Now);
END

PRINT N'用户数据初始化完成';
PRINT N'';

-- 汇总
SELECT 'Companies' AS [Table], COUNT(*) AS [Count] FROM [Companies]
UNION ALL
SELECT 'Roles', COUNT(*) FROM [Roles]
UNION ALL
SELECT 'Users', COUNT(*) FROM [Users]
UNION ALL
SELECT 'UserRoles', COUNT(*) FROM [UserRoles];
GO
