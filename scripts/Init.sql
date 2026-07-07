-- ===================================================================
-- Init.sql - 数据库初始化脚本
-- 包含所有业务表的 CREATE TABLE 定义
-- 说明：本系统禁止使用外键约束（无 REFERENCES / CONSTRAINT FK_）
--       字段说明通过 sp_addextendedproperty 持久化到数据库
-- ===================================================================

-- ===================================================================
-- Organization（组织架构）
-- ===================================================================

-- ===================================================================
-- 1. Companies 表：公司根表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Companies]'))
CREATE TABLE [Companies] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Name] NVARCHAR(200) NOT NULL , -- 公司名称,
    [Code] VARCHAR(50) , -- 公司编号,
    [IdType] VARCHAR(50) , -- 证件类型,
    [IdNumber] VARCHAR(100) , -- 证件号码,
    [ContactPerson] NVARCHAR(100) , -- 联系人,
    [Phone] VARCHAR(20) , -- 联系电话,
    [Address] NVARCHAR(500) , -- 通讯地址,
    [BankName] NVARCHAR(200) , -- 开户行,
    [BankAccount] VARCHAR(50) , -- 银行账号,
    [BankAccountName] NVARCHAR(200) , -- 开户名,
    [SettlementCycle] VARCHAR(50) , -- 结算周期,
    [SettlementDay] INT , -- 结算日,
    [CommissionRate] DECIMAL(5,2) , -- 佣金比例,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [Remark] NVARCHAR(500) , -- 备注,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：公司根表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司根表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies';
GO

-- Companies 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'IdType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件号码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'IdNumber';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'联系人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'ContactPerson';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'联系电话', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Phone';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通讯地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Address';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开户行', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'BankName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'BankAccount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开户名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'BankAccountName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结算周期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'SettlementCycle';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结算日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'SettlementDay';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'佣金比例', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CommissionRate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Remark';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 公司编号唯一索引
CREATE UNIQUE INDEX [IX_Companies_Code] ON [Companies]([Code]) WHERE [Code] IS NOT NULL;

-- ===================================================================
-- 2. Users 表：用户表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Users]'))
CREATE TABLE [Users] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Username] VARCHAR(100) NOT NULL , -- 登录用户名,
    [PasswordHash] VARCHAR(500) NOT NULL , -- 密码哈希,
    [DisplayName] NVARCHAR(100) NOT NULL , -- 显示名称,
    [Phone] VARCHAR(20) , -- 手机号,
    [Email] VARCHAR(100) , -- 邮箱,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [HomeCompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [DefaultCompanyId] UNIQUEIDENTIFIER , -- 默认公司ID,
    [IsSuperAdmin] BIT NOT NULL DEFAULT (0) , -- 是否超级管理员,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：用户表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users';
GO

-- Users 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'登录用户名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'Username';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'密码哈希', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'PasswordHash';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'显示名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'DisplayName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'手机号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'Phone';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'邮箱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'Email';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'HomeCompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'DefaultCompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否超级管理员', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'IsSuperAdmin';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 用户名唯一索引
CREATE UNIQUE INDEX [IX_Users_Username] ON [Users]([Username])
-- 按公司查询索引
CREATE INDEX [IX_Users_HomeCompanyId] ON [Users]([HomeCompanyId])

-- ===================================================================
-- 3. Roles 表：角色表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Roles]'))
CREATE TABLE [Roles] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Name] NVARCHAR(100) NOT NULL , -- 角色名称,
    [Code] VARCHAR(50) NOT NULL , -- 角色代码,
    [Description] NVARCHAR(500) , -- 描述,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：角色表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles';
GO

-- Roles 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 角色代码唯一索引
CREATE UNIQUE INDEX [IX_Roles_Code] ON [Roles]([Code])

-- ===================================================================
-- 4. UserRoles 表：用户角色关联表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[UserRoles]'))
CREATE TABLE [UserRoles] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [UserId] UNIQUEIDENTIFIER NOT NULL , -- 用户ID,
    [RoleId] UNIQUEIDENTIFIER NOT NULL , -- 角色ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：用户角色关联表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户角色关联表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles';
GO

-- UserRoles 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'UserId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'RoleId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 用户角色联合唯一
CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles]([UserId],[RoleId])
-- 按角色查询
CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles]([RoleId])

-- ===================================================================
-- 5. UserCompanyScope 表：用户公司数据权限表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[UserCompanyScope]'))
CREATE TABLE [UserCompanyScope] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [UserId] UNIQUEIDENTIFIER NOT NULL , -- 用户ID,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：用户公司数据权限表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户公司数据权限表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserCompanyScope';
GO

-- UserCompanyScope 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserCompanyScope', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserCompanyScope', @level2type = N'COLUMN', @level2name = N'UserId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserCompanyScope', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserCompanyScope', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserCompanyScope', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 按用户查询
CREATE INDEX [IX_UserCompanyScope_UserId] ON [UserCompanyScope]([UserId])
-- 按公司查询
CREATE INDEX [IX_UserCompanyScope_CompanyId] ON [UserCompanyScope]([CompanyId])

-- ===================================================================
-- 6. Menus 表：菜单表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Menus]'))
CREATE TABLE [Menus] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ParentId] UNIQUEIDENTIFIER , -- 父菜单ID,
    [Name] NVARCHAR(100) NOT NULL , -- 菜单名称,
    [Path] VARCHAR(200) , -- 前端路由路径,
    [Icon] VARCHAR(50) , -- 图标,
    [PermissionCode] VARCHAR(100) , -- 权限代码,
    [SortOrder] INT NOT NULL DEFAULT (0) , -- 排序序号,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [Scope] VARCHAR(20) NOT NULL DEFAULT ('Company') , -- 可见范围,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：菜单表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'菜单表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus';
GO

-- Menus 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'父菜单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'ParentId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'菜单名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'前端路由路径', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Path';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'图标', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Icon';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'权限代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'PermissionCode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序序号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'SortOrder';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'可见范围', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Scope';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 权限代码唯一索引
CREATE UNIQUE INDEX [IX_Menus_PermissionCode] ON [Menus]([PermissionCode]) WHERE [PermissionCode] IS NOT NULL;

-- ===================================================================
-- 7. RoleMenus 表：角色菜单权限表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoleMenus]'))
CREATE TABLE [RoleMenus] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RoleId] UNIQUEIDENTIFIER NOT NULL , -- 角色ID,
    [MenuId] UNIQUEIDENTIFIER NOT NULL , -- 菜单ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：角色菜单权限表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色菜单权限表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus';
GO

-- RoleMenus 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'RoleId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'菜单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'MenuId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 角色菜单联合唯一
CREATE UNIQUE INDEX [IX_RoleMenus_RoleId_MenuId] ON [RoleMenus]([RoleId],[MenuId])

-- ===================================================================
-- 8. AuditLogs 表：审计日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AuditLogs]'))
CREATE TABLE [AuditLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [UserId] UNIQUEIDENTIFIER NOT NULL , -- 操作人ID,
    [Action] VARCHAR(50) NOT NULL , -- 操作类型,
    [EntityType] VARCHAR(50) NOT NULL , -- 对象类型,
    [EntityId] VARCHAR(100) NOT NULL , -- 对象ID,
    [OriginalValues] NVARCHAR(MAX) , -- 变更前JSON,
    [NewValues] NVARCHAR(MAX) , -- 变更后JSON,
    [IpAddress] VARCHAR(50) , -- 操作IP,
    [Remarks] NVARCHAR(500) , -- 备注,
    [OperatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 操作时间
)
GO

-- 表说明：审计日志表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs';
GO

-- AuditLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'UserId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'Action';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'对象类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'EntityType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'对象ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'EntityId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更前JSON', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'OriginalValues';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更后JSON', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'NewValues';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'IpAddress';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'Remarks';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AuditLogs', @level2type = N'COLUMN', @level2name = N'OperatedAt';
GO

-- 按操作时间查询
CREATE INDEX [IX_AuditLogs_OperatedAt] ON [AuditLogs]([OperatedAt])
-- 按实体查询
CREATE INDEX [IX_AuditLogs_Entity] ON [AuditLogs]([EntityType],[EntityId])

-- ===================================================================
-- Approval（审批工作流）
-- ===================================================================

-- ===================================================================
-- 9. ApprovalTypes 表：审批类型表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalTypes]'))
CREATE TABLE [ApprovalTypes] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Code] VARCHAR(50) NOT NULL , -- 审批类型编码,
    [Name] NVARCHAR(100) NOT NULL , -- 审批类型名称,
    [Description] NVARCHAR(500) , -- 描述,
    [RoutingStrategy] VARCHAR(20) NOT NULL DEFAULT ('Fixed') , -- 路由策略,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：审批类型表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes';
GO

-- ApprovalTypes 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型编码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'路由策略', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'RoutingStrategy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 审批类型编码唯一
CREATE UNIQUE INDEX [IX_ApprovalTypes_Code] ON [ApprovalTypes]([Code])

-- ===================================================================
-- 10. ApprovalLevelConfigs 表：审批级别配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs]'))
CREATE TABLE [ApprovalLevelConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ApprovalTypeId] UNIQUEIDENTIFIER NOT NULL , -- 审批类型ID,
    [LevelNo] INT NOT NULL , -- 审批级别序号,
    [ApproverRoleId] UNIQUEIDENTIFIER NOT NULL , -- 审批角色ID,
    [ApprovalMode] VARCHAR(20) NOT NULL DEFAULT ('AnyOne') , -- 审批模式,
    [MinAmount] DECIMAL(18,2) , -- 金额下限,
    [MaxAmount] DECIMAL(18,2) , -- 金额上限,
    [IsCumulativeCheck] BIT NOT NULL DEFAULT (0) , -- 累计金额检查,
    [CumulativeWindowDays] INT NOT NULL DEFAULT (19) , -- 累计检查窗口天数,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：审批级别配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs';
GO

-- ApprovalLevelConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'ApprovalTypeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别序号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'LevelNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批角色ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'ApproverRoleId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批模式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'ApprovalMode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额下限', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'MinAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额上限', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'MaxAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'累计金额检查', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'IsCumulativeCheck';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'累计检查窗口天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CumulativeWindowDays';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 审批类型+级别唯一
CREATE UNIQUE INDEX [IX_ApprovalLevelConfigs_Type_Level] ON [ApprovalLevelConfigs]([ApprovalTypeId],[LevelNo])

-- ===================================================================
-- 11. ApprovalRequests 表：审批申请表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRequests]'))
CREATE TABLE [ApprovalRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RequestNo] VARCHAR(100) NOT NULL , -- 申请编号,
    [ApprovalTypeId] UNIQUEIDENTIFIER NOT NULL , -- 审批类型ID,
    [BusinessId] UNIQUEIDENTIFIER , -- 业务ID,
    [BusinessData] NVARCHAR(MAX) , -- 业务数据JSON,
    [Amount] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 申请金额,
    [Reason] NVARCHAR(500) NOT NULL , -- 申请原因,
    [RequesterId] UNIQUEIDENTIFIER NOT NULL , -- 申请人ID,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 审批状态,
    [CurrentLevel] INT NOT NULL DEFAULT (0) , -- 当前审批级别,
    [CallbackStatus] VARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 回调状态,
    [CallbackError] NVARCHAR(MAX) , -- 回调错误,
    [CallbackRetryCount] INT NOT NULL DEFAULT (0) , -- 重试次数,
    [ContractId] UNIQUEIDENTIFIER , -- 合同ID,
    [RowVersion] TIMESTAMP NOT NULL , -- 乐观锁,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：审批申请表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批申请表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests';
GO

-- ApprovalRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'RequestNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'ApprovalTypeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'业务ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'BusinessId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'业务数据JSON', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'BusinessData';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请原因', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'Reason';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'RequesterId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'当前审批级别', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CurrentLevel';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'回调状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CallbackStatus';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'回调错误', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CallbackError';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'重试次数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CallbackRetryCount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'乐观锁', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'RowVersion';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 申请编号唯一
CREATE UNIQUE INDEX [IX_ApprovalRequests_RequestNo] ON [ApprovalRequests]([RequestNo])
-- 按状态查询
CREATE INDEX [IX_ApprovalRequests_Status] ON [ApprovalRequests]([Status])
-- 按合同查询
CREATE INDEX [IX_ApprovalRequests_ContractId] ON [ApprovalRequests]([ContractId])

-- ===================================================================
-- 12. ApprovalRecords 表：审批操作记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRecords]'))
CREATE TABLE [ApprovalRecords] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RequestId] UNIQUEIDENTIFIER NOT NULL , -- 审批申请ID,
    [LevelNo] INT NOT NULL , -- 审批级别,
    [ApproverId] UNIQUEIDENTIFIER NOT NULL , -- 审批人ID,
    [Action] VARCHAR(20) NOT NULL , -- 审批动作,
    [Comment] NVARCHAR(500) , -- 审批意见,
    [OperatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 操作时间,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：审批操作记录表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批操作记录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords';
GO

-- ApprovalRecords 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批申请ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'RequestId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'LevelNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'ApproverId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批动作', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'Action';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批意见', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'Comment';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'OperatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 按申请查询
CREATE INDEX [IX_ApprovalRecords_RequestId] ON [ApprovalRecords]([RequestId])

-- ===================================================================
-- 13. ApprovalBizData 表：审批业务数据表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalBizData]'))
CREATE TABLE [ApprovalBizData] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ApprovalRequestId] UNIQUEIDENTIFIER , -- 审批请求ID,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [ContractNo] NVARCHAR(100) , -- 合同号,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 公司ID,
    [ChangeType] VARCHAR(30) NOT NULL , -- 变更类型,
    [EffectiveDate] DATE , -- 生效日期,
    [OldAmount] DECIMAL(18,2) , -- 旧金额,
    [NewAmount] DECIMAL(18,2) , -- 新金额,
    [Reason] NVARCHAR(500) , -- 原因说明,
    [TerminateType] VARCHAR(20) , -- 终止类型,
    [ActualEndDate] DATE , -- 实际搬离日,
    [DepositReturn] VARCHAR(20) , -- 押金处理,
    [IsProcessed] BIT NOT NULL DEFAULT (0) , -- 是否已处理,
    [ProcessedAt] DATETIME2 , -- 处理时间,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 -- 更新时间
)
GO

-- 表说明：审批业务数据表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批业务数据表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData';
GO

-- ApprovalBizData 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ContractNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ChangeType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'EffectiveDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'旧金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'OldAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'NewAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原因说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'Reason';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'终止类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'TerminateType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'实际搬离日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ActualEndDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金处理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'DepositReturn';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已处理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'IsProcessed';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'处理时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ProcessedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
GO

-- 审批请求ID唯一
CREATE UNIQUE INDEX [IX_ApprovalBizData_ApprovalRequestId] ON [ApprovalBizData]([ApprovalRequestId]) WHERE [ApprovalRequestId] IS NOT NULL;
-- 按合同查询
CREATE INDEX [IX_ApprovalBizData_ContractId] ON [ApprovalBizData]([ContractId])

-- ===================================================================
-- 14. ApprovalFeeItems 表：审批调价明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalFeeItems]'))
CREATE TABLE [ApprovalFeeItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ApprovalRequestId] UNIQUEIDENTIFIER NOT NULL , -- 审批请求ID,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [FeeName] NVARCHAR(100) NOT NULL , -- 费用名称,
    [OldAmount] DECIMAL(18,2) NOT NULL , -- 原价,
    [NewAmount] DECIMAL(18,2) NOT NULL , -- 新价,
    [BillingMode] VARCHAR(20) NOT NULL , -- 计费方式,
    [Unit] NVARCHAR(20) , -- 计量单位,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：审批调价明细表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批调价明细表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems';
GO

-- ApprovalFeeItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'FeeCodeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'FeeName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'OldAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'NewAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'BillingMode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'Unit';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 按审批请求查询
CREATE INDEX [IX_ApprovalFeeItems_ApprovalRequestId] ON [ApprovalFeeItems]([ApprovalRequestId])

-- ===================================================================
-- Property（房屋管理）
-- ===================================================================

-- ===================================================================
-- 15. Buildings 表：楼栋表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Buildings]'))
CREATE TABLE [Buildings] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Code] VARCHAR(50) NOT NULL , -- 楼栋编号,
    [Name] NVARCHAR(200) NOT NULL , -- 楼栋名称,
    [Address] NVARCHAR(500) NOT NULL , -- 地址,
    [PropertyType] VARCHAR(20) , -- 产权类型,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：楼栋表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼栋表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings';
GO

-- Buildings 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼栋编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼栋名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'Address';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'产权类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'PropertyType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Buildings', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 楼栋编号唯一
CREATE UNIQUE INDEX [IX_Buildings_Code] ON [Buildings]([Code])

-- ===================================================================
-- 16. Floors 表：楼层表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Floors]'))
CREATE TABLE [Floors] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [BuildingId] UNIQUEIDENTIFIER NOT NULL , -- 所属楼栋ID,
    [FloorNo] INT NOT NULL , -- 楼层号,
    [FloorName] NVARCHAR(100) , -- 楼层名称,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：楼层表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors';
GO

-- Floors 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属楼栋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'BuildingId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'FloorNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'FloorName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Floors', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 楼栋+楼层号唯一
CREATE UNIQUE INDEX [IX_Floors_BuildingId_FloorNo] ON [Floors]([BuildingId],[FloorNo])

-- ===================================================================
-- 17. HousingUnits 表：房间表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HousingUnits]'))
CREATE TABLE [HousingUnits] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [FloorId] UNIQUEIDENTIFIER NOT NULL , -- 所属楼层ID,
    [UnitNo] VARCHAR(20) NOT NULL , -- 房间号,
    [FullCode] VARCHAR(100) NOT NULL , -- 完整编码,
    [Area] DECIMAL(10,2) , -- 面积,
    [RoomTypeId] UNIQUEIDENTIFIER , -- 房型ID,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Vacant') , -- 房间状态,
    [Orientation] VARCHAR(50) , -- 朝向,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：房间表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits';
GO

-- HousingUnits 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属楼层ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'FloorId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UnitNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'完整编码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'FullCode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'面积', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'Area';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'RoomTypeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'朝向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'Orientation';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 完整编码唯一
CREATE UNIQUE INDEX [IX_HousingUnits_FullCode] ON [HousingUnits]([FullCode])
-- 按状态查询
CREATE INDEX [IX_HousingUnits_Status] ON [HousingUnits]([Status])

-- ===================================================================
-- 18. RoomTypes 表：房型字典表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomTypes]'))
CREATE TABLE [RoomTypes] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Category] VARCHAR(20) NOT NULL , -- 分类,
    [Code] VARCHAR(50) NOT NULL , -- 代码,
    [Name] NVARCHAR(100) NOT NULL , -- 名称,
    [Description] NVARCHAR(500) , -- 描述,
    [SortOrder] INT NOT NULL DEFAULT (0) , -- 排序,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：房型字典表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型字典表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes';
GO

-- RoomTypes 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Category';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'SortOrder';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 房型代码唯一
CREATE UNIQUE INDEX [IX_RoomTypes_Code] ON [RoomTypes]([Code])

-- ===================================================================
-- 19. FloorLevelBands 表：楼层级别定义表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FloorLevelBands]'))
CREATE TABLE [FloorLevelBands] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Name] NVARCHAR(100) NOT NULL , -- 级别名称,
    [MinLevel] INT NOT NULL , -- 起始楼层,
    [MaxLevel] INT NOT NULL , -- 结束楼层,
    [Description] NVARCHAR(500) , -- 描述,
    [SortOrder] INT NOT NULL DEFAULT (0) , -- 排序,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：楼层级别定义表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层级别定义表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands';
GO

-- FloorLevelBands 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'级别名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'起始楼层', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'MinLevel';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结束楼层', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'MaxLevel';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'SortOrder';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- ===================================================================
-- 20. BuildingFloorLevelConfigs 表：楼栋楼层级别映射表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BuildingFloorLevelConfigs]'))
CREATE TABLE [BuildingFloorLevelConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [BuildingId] UNIQUEIDENTIFIER NOT NULL , -- 楼栋ID,
    [FloorLevelBandId] UNIQUEIDENTIFIER NOT NULL , -- 楼层级别ID,
    [FloorNoFrom] INT NOT NULL , -- 起始楼层号,
    [FloorNoTo] INT NOT NULL , -- 结束楼层号,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：楼栋楼层级别映射表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼栋楼层级别映射表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs';
GO

-- BuildingFloorLevelConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼栋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'BuildingId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层级别ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'FloorLevelBandId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'起始楼层号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'FloorNoFrom';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结束楼层号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'FloorNoTo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BuildingFloorLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 楼栋+级别唯一
CREATE UNIQUE INDEX [IX_BldgFloorLevel_Building_Band] ON [BuildingFloorLevelConfigs]([BuildingId],[FloorLevelBandId])

-- ===================================================================
-- 21. RoomPricingStandards 表：定价标准表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomPricingStandards]'))
CREATE TABLE [RoomPricingStandards] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RoomTypeId] UNIQUEIDENTIFIER NOT NULL , -- 房型ID,
    [FloorLevelBandId] UNIQUEIDENTIFIER , -- 楼层级别ID,
    [BuildingId] UNIQUEIDENTIFIER , -- 楼栋ID,
    [RentAmount] DECIMAL(18,2) NOT NULL , -- 标准租金,
    [EffectiveDate] DATE , -- 生效日期,
    [Remarks] NVARCHAR(500) , -- 备注,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：定价标准表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'定价标准表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards';
GO

-- RoomPricingStandards 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'RoomTypeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层级别ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'FloorLevelBandId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼栋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'BuildingId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'标准租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'RentAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'EffectiveDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'Remarks';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 按房型查询
CREATE INDEX [IX_RoomPricingStandards_RoomType] ON [RoomPricingStandards]([RoomTypeId])

-- ===================================================================
-- 22. RoomFeeDefaults 表：房间默认费用表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomFeeDefaults]'))
CREATE TABLE [RoomFeeDefaults] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RoomTypeId] UNIQUEIDENTIFIER , -- 房型ID,
    [FeeCodeId] UNIQUEIDENTIFIER , -- 费用项目ID,
    [DefaultAmount] DECIMAL(18,2) , -- 默认金额,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：房间默认费用表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间默认费用表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults';
GO

-- RoomFeeDefaults 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'RoomTypeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'FeeCodeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'DefaultAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomFeeDefaults', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- ===================================================================
-- Tenant & Contract（租客合同）
-- ===================================================================

-- ===================================================================
-- 23. Tenants 表：租客表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Tenants]'))
CREATE TABLE [Tenants] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Name] NVARCHAR(200) NOT NULL , -- 租客姓名,
    [IdentityType] VARCHAR(20) NOT NULL DEFAULT ('PRC_ID') , -- 证件类型,
    [IdentityNo] VARCHAR(50) NOT NULL , -- 证件号码,
    [Phone] VARCHAR(20) NOT NULL , -- 手机号,
    [Email] VARCHAR(200) , -- 邮箱,
    [Address] NVARCHAR(500) , -- 通讯地址,
    [Remarks] NVARCHAR(500) , -- 备注,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：租客表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants';
GO

-- Tenants 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客姓名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'IdentityType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件号码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'IdentityNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'手机号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Phone';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'邮箱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Email';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通讯地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Address';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Remarks';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 证件号查询
CREATE INDEX [IX_Tenants_IdentityNo] ON [Tenants]([IdentityNo])
-- 手机号查询
CREATE INDEX [IX_Tenants_Phone] ON [Tenants]([Phone])

-- ===================================================================
-- 24. Contracts 表：合同表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Contracts]'))
CREATE TABLE [Contracts] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractNo] VARCHAR(100) NOT NULL , -- 合同编号,
    [RoomId] UNIQUEIDENTIFIER NOT NULL , -- 房屋ID,
    [StartDate] DATE NOT NULL , -- 合同开始日期,
    [EndDate] DATE NOT NULL , -- 合同结束日期,
    [RentAmount] DECIMAL(18,2) NOT NULL , -- 月租金,
    [DepositAmount] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 押金金额,
    [PaymentCycle] VARCHAR(20) NOT NULL DEFAULT ('Monthly') , -- 支付周期,
    [PaymentDueDay] INT NOT NULL DEFAULT (5) , -- 每月到期日,
    [AllowDepositAsLastRent] BIT NOT NULL DEFAULT (0) , -- 押金抵扣最后租金,
    [AutoRenew] BIT NOT NULL DEFAULT (1) , -- 是否自动续签,
    [ActualEndDate] DATE , -- 实际搬离日,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Draft') , -- 合同状态,
    [PreviousContractId] UNIQUEIDENTIFIER , -- 上一份合同ID,
    [RenewalCount] INT NOT NULL DEFAULT (0) , -- 续签次数,
    [OriginalContractId] UNIQUEIDENTIFIER , -- 原始合同ID,
    [MarketPriceAtRenewal] DECIMAL(18,2) , -- 续签市场价,
    [TerminatedAt] DATETIME2 , -- 终止时间,
    [TerminationReason] NVARCHAR(500) , -- 终止原因,
    [SuspendedAt] DATETIME2 , -- 暂停时间,
    [ResumedAt] DATETIME2 , -- 恢复时间,
    [RowVersion] TIMESTAMP NOT NULL , -- 乐观锁,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：合同表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts';
GO

-- Contracts 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'ContractNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房屋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'RoomId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同开始日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'StartDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同结束日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'EndDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'月租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'RentAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'DepositAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付周期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'PaymentCycle';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'每月到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'PaymentDueDay';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金抵扣最后租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'AllowDepositAsLastRent';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动续签', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'AutoRenew';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'实际搬离日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'ActualEndDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上一份合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'PreviousContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签次数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'RenewalCount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原始合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'OriginalContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签市场价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'MarketPriceAtRenewal';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'终止时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'TerminatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'终止原因', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'TerminationReason';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'暂停时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'SuspendedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'恢复时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'ResumedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'乐观锁', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'RowVersion';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 合同编号唯一
CREATE UNIQUE INDEX [IX_Contracts_ContractNo] ON [Contracts]([ContractNo])
-- 按状态查询
CREATE INDEX [IX_Contracts_Status] ON [Contracts]([Status])
-- 按房屋查询
CREATE INDEX [IX_Contracts_RoomId] ON [Contracts]([RoomId])

-- ===================================================================
-- 25. ContractTenants 表：合同租客关联表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractTenants]'))
CREATE TABLE [ContractTenants] (
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [TenantId] UNIQUEIDENTIFIER NOT NULL , -- 租客ID,
    [IsPrimary] BIT NOT NULL DEFAULT (0) , -- 是否主租客,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    CONSTRAINT [PK_ContractTenants] PRIMARY KEY (ContractId, TenantId)
)
GO

-- 表说明：合同租客关联表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同租客关联表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants';
GO

-- ContractTenants 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'TenantId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否主租客', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'IsPrimary';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- ===================================================================
-- 26. ContractFeeConfigs 表：合同费用配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs]'))
CREATE TABLE [ContractFeeConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [Amount] DECIMAL(18,4) NOT NULL , -- 金额/单价,
    [BillingMode] VARCHAR(20) NOT NULL , -- 计费方式,
    [Unit] NVARCHAR(20) , -- 计量单位,
    [UnitPrice] DECIMAL(18,4) , -- 单价,
    [InitialReading] DECIMAL(18,4) , -- 初始读数,
    [InitialReadingDate] DATE , -- 初始读数日期,
    [EffectiveDate] DATE NOT NULL , -- 生效日期,
    [ExpiryDate] DATE , -- 失效日期,
    [ProrateOnMoveIn] BIT NOT NULL DEFAULT (1) , -- 入住当月分摊,
    [ProrateOnMoveOut] BIT NOT NULL DEFAULT (1) , -- 搬出当月分摊,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：合同费用配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同费用配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs';
GO

-- ContractFeeConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'FeeCodeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额/单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'BillingMode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'Unit';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UnitPrice';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'初始读数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'InitialReading';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'初始读数日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'InitialReadingDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'EffectiveDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'失效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'ExpiryDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'入住当月分摊', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'ProrateOnMoveIn';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'搬出当月分摊', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'ProrateOnMoveOut';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 按合同查询
CREATE INDEX [IX_ContractFeeConfigs_Contract] ON [ContractFeeConfigs]([ContractId])

-- ===================================================================
-- 27. ChangeHistory 表：合同变更历史表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ChangeHistory]'))
CREATE TABLE [ChangeHistory] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [ChangeType] VARCHAR(30) NOT NULL , -- 变更类型,
    [Title] NVARCHAR(200) NOT NULL , -- 标题,
    [Detail] NVARCHAR(500) , -- 详情,
    [OldValue] DECIMAL(18,2) , -- 旧值,
    [NewValue] DECIMAL(18,2) , -- 新值,
    [EffectiveDate] DATE , -- 生效日期,
    [OperatorId] UNIQUEIDENTIFIER , -- 操作人ID,
    [OperatorName] NVARCHAR(50) , -- 操作人姓名,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：合同变更历史表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同变更历史表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory';
GO

-- ChangeHistory 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'ChangeType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'标题', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'Title';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'详情', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'Detail';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'旧值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'OldValue';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'NewValue';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'EffectiveDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'OperatorId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人姓名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'OperatorName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 按合同查询
CREATE INDEX [IX_ChangeHistory_ContractId] ON [ChangeHistory]([ContractId])

-- ===================================================================
-- 28. RenewalRequests 表：续签申请表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RenewalRequests]'))
CREATE TABLE [RenewalRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [OldContractId] UNIQUEIDENTIFIER NOT NULL , -- 原合同ID,
    [NewContractId] UNIQUEIDENTIFIER , -- 新合同ID,
    [ContractNo] NVARCHAR(100) NOT NULL , -- 新合同号,
    [RenewalType] VARCHAR(20) NOT NULL DEFAULT ('Standard') , -- 续签类型,
    [PreviousRent] DECIMAL(18,2) NOT NULL , -- 原租金,
    [NewRent] DECIMAL(18,2) NOT NULL , -- 新租金,
    [NewEndDate] DATE NOT NULL , -- 新到期日,
    [DepositHandling] VARCHAR(20) NOT NULL , -- 押金处理,
    [OldDepositAmount] DECIMAL(18,2) NOT NULL , -- 原押金,
    [NewDepositAmount] DECIMAL(18,2) , -- 新押金,
    [MarketReferencePrice] DECIMAL(18,2) , -- 市场参考价,
    [PaymentStatusCheck] BIT NOT NULL DEFAULT (0) , -- 付款检查,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Draft') , -- 状态,
    [Remark] NVARCHAR(500) , -- 备注,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 -- 更新时间
)
GO

-- 表说明：续签申请表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签申请表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests';
GO

-- RenewalRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'OldContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'NewContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新合同号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'ContractNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'RenewalType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'PreviousRent';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'NewRent';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'NewEndDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金处理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'DepositHandling';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原押金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'OldDepositAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新押金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'NewDepositAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'市场参考价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'MarketReferencePrice';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款检查', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'PaymentStatusCheck';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'Remark';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
GO

-- 按原合同查询
CREATE INDEX [IX_RenewalRequests_OldContract] ON [RenewalRequests]([OldContractId])

-- ===================================================================
-- 29. ChangeRequests 表：变更请求表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ChangeRequests]'))
CREATE TABLE [ChangeRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [ChangeType] VARCHAR(30) NOT NULL , -- 变更类型,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Draft') , -- 状态,
    [EffectiveDate] DATE , -- 生效日期,
    [Reason] NVARCHAR(500) , -- 原因,
    [BatchId] UNIQUEIDENTIFIER , -- 批量分组ID,
    [ApprovalRequestId] UNIQUEIDENTIFIER , -- 审批请求ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：变更请求表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更请求表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests';
GO

-- ChangeRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'ChangeType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'EffectiveDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原因', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'Reason';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'批量分组ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'BatchId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequests', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 按合同查询
CREATE INDEX [IX_ChangeRequests_ContractId] ON [ChangeRequests]([ContractId])

-- ===================================================================
-- 30. ChangeRequestItems 表：变更请求明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ChangeRequestItems]'))
CREATE TABLE [ChangeRequestItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ChangeRequestId] UNIQUEIDENTIFIER NOT NULL , -- 变更请求ID,
    [TargetType] VARCHAR(20) NOT NULL , -- 目标类型,
    [TargetId] UNIQUEIDENTIFIER , -- 目标ID,
    [FieldName] VARCHAR(50) NOT NULL , -- 字段名,
    [OldValue] NVARCHAR(100) , -- 旧值,
    [NewValue] NVARCHAR(100) NOT NULL , -- 新值,
    [OldValueDecimal] DECIMAL(18,2) , -- 旧值-数值,
    [NewValueDecimal] DECIMAL(18,2) , -- 新值-数值,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：变更请求明细表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更请求明细表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems';
GO

-- ChangeRequestItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'ChangeRequestId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'目标类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'TargetType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'目标ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'TargetId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'字段名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'FieldName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'旧值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'OldValue';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'NewValue';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'旧值-数值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'OldValueDecimal';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新值-数值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'NewValueDecimal';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeRequestItems', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 按变更请求查询
CREATE INDEX [IX_ChangeRequestItems_ChangeRequestId] ON [ChangeRequestItems]([ChangeRequestId])

-- ===================================================================
-- Fee（费用配置）
-- ===================================================================

-- ===================================================================
-- 31. FeeCodes 表：费用项目表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodes]'))
CREATE TABLE [FeeCodes] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Code] VARCHAR(50) NOT NULL , -- 费用代码,
    [Name] NVARCHAR(200) NOT NULL , -- 费用名称,
    [BillingMode] VARCHAR(20) NOT NULL DEFAULT ('FixedAmount') , -- 计费方式,
    [Unit] NVARCHAR(20) , -- 计量单位,
    [SortOrder] INT NOT NULL DEFAULT (0) , -- 排序,
    [Category] VARCHAR(50) , -- 分类,
    [ChargeType] VARCHAR(20) NOT NULL DEFAULT ('Recurring') , -- 收费类型,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [IsRequired] BIT NOT NULL DEFAULT (0) , -- 是否必选,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：费用项目表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes';
GO

-- FeeCodes 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'BillingMode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Unit';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'SortOrder';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Category';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收费类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'ChargeType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否必选', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'IsRequired';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 公司内费用代码唯一
CREATE UNIQUE INDEX [IX_FeeCodes_CompanyId_Code] ON [FeeCodes]([CompanyId],[Code])

-- ===================================================================
-- 32. FeeCodeTemplates 表：费用科目模板表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates]'))
CREATE TABLE [FeeCodeTemplates] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [Direction] VARCHAR(10) NOT NULL , -- 借贷方向,
    [SubjectCode] VARCHAR(50) NOT NULL , -- 科目代码,
    [SubjectName] NVARCHAR(200) NOT NULL , -- 科目名称,
    [IsVatSeparate] BIT NOT NULL DEFAULT (0) , -- 增值税分离,
    [SortOrder] INT NOT NULL , -- 排序,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：费用科目模板表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用科目模板表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates';
GO

-- FeeCodeTemplates 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'FeeCodeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借贷方向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'Direction';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'SubjectCode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'SubjectName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'增值税分离', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'IsVatSeparate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'SortOrder';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 按费用项目查询
CREATE INDEX [IX_FeeCodeTemplates_FeeCodeId] ON [FeeCodeTemplates]([FeeCodeId])

-- ===================================================================
-- Meter（抄表）
-- ===================================================================

-- ===================================================================
-- 33. MeterReadings 表：抄表记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[MeterReadings]'))
CREATE TABLE [MeterReadings] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [RoomId] UNIQUEIDENTIFIER , -- 房间ID,
    [PeriodYear] INT NOT NULL , -- 账期年份,
    [PeriodMonth] INT NOT NULL , -- 账期月份,
    [PreviousReading] DECIMAL(18,4) NOT NULL , -- 上期读数,
    [CurrentReading] DECIMAL(18,4) NOT NULL DEFAULT (0) , -- 本期读数,
    [Consumption] DECIMAL(18,4) NOT NULL DEFAULT (0) , -- 用量,
    [UnitPrice] DECIMAL(18,4) NOT NULL , -- 单价,
    [Amount] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 金额,
    [ReadingSource] VARCHAR(20) NOT NULL DEFAULT ('Manual') , -- 来源,
    [ReadingStatus] VARCHAR(20) NOT NULL DEFAULT ('Draft') , -- 状态,
    [IsEstimated] BIT NOT NULL DEFAULT (0) , -- 是否估算,
    [EstimationMethod] VARCHAR(50) , -- 估算方法,
    [ReceivablePlanId] UNIQUEIDENTIFIER , -- 应收计划ID,
    [Remarks] NVARCHAR(500) , -- 备注,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：抄表记录表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'抄表记录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings';
GO

-- MeterReadings 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'FeeCodeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'RoomId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期年份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'PeriodYear';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'PeriodMonth';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上期读数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'PreviousReading';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'本期读数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'CurrentReading';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'Consumption';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'UnitPrice';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'来源', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'ReadingSource';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'ReadingStatus';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否估算', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'IsEstimated';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'估算方法', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'EstimationMethod';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收计划ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'ReceivablePlanId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'Remarks';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterReadings', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 合同+账期+费用唯一
CREATE UNIQUE INDEX [IX_MeterReadings_Contract_Period] ON [MeterReadings]([ContractId],[FeeCodeId],[PeriodYear],[PeriodMonth])

-- ===================================================================
-- 34. MeterEstimationConfigs 表：估读配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[MeterEstimationConfigs]'))
CREATE TABLE [MeterEstimationConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [FeeCodeId] UNIQUEIDENTIFIER , -- 费用项目ID,
    [Method] VARCHAR(50) NOT NULL , -- 估读方法,
    [OverdueDaysThreshold] INT NOT NULL DEFAULT (7) , -- 逾期天数,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：估读配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'估读配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs';
GO

-- MeterEstimationConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'FeeCodeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'估读方法', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'Method';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'逾期天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'OverdueDaysThreshold';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MeterEstimationConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- ===================================================================
-- Tax & Billing（税率与账单）
-- ===================================================================

-- ===================================================================
-- 35. TaxRateConfigs 表：税率配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaxRateConfigs]'))
CREATE TABLE [TaxRateConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Name] NVARCHAR(100) NOT NULL , -- 税率名称,
    [Rate] DECIMAL(5,2) NOT NULL , -- 税率(%),
    [EffectiveDate] DATE NOT NULL , -- 生效日期,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：税率配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs';
GO

-- TaxRateConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率(%)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'Rate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'EffectiveDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- ===================================================================
-- 36. ReceivablePlans 表：应收计划表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivablePlans]'))
CREATE TABLE [ReceivablePlans] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [PeriodYear] INT NOT NULL , -- 账期年份,
    [PeriodMonth] INT NOT NULL , -- 账期月份,
    [DueDate] DATE NOT NULL , -- 到期日,
    [Amount] DECIMAL(18,2) NOT NULL , -- 应收金额,
    [ReceivedAmount] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 已收金额,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 状态,
    [TaxRate] DECIMAL(5,4) NOT NULL DEFAULT (0) , -- 税率,
    [TaxAmount] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 税额,
    [Description] NVARCHAR(500) , -- 计算说明,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：应收计划表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收计划表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans';
GO

-- ReceivablePlans 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'FeeCodeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期年份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'PeriodYear';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'PeriodMonth';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'DueDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'已收金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'ReceivedAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'TaxRate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'TaxAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计算说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivablePlans', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 合同+账期+费用唯一
CREATE UNIQUE INDEX [IX_ReceivablePlans_Contract_Period] ON [ReceivablePlans]([ContractId],[PeriodYear],[PeriodMonth],[FeeCodeId])
-- 按状态查询
CREATE INDEX [IX_ReceivablePlans_Status] ON [ReceivablePlans]([Status])

-- ===================================================================
-- 37. DebitNotes 表：账单主表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNotes]'))
CREATE TABLE [DebitNotes] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [PeriodYear] INT NOT NULL , -- 账期年份,
    [PeriodMonth] INT NOT NULL , -- 账期月份,
    [BillNo] VARCHAR(50) NOT NULL , -- 账单编号,
    [DueDate] DATE NOT NULL , -- 到期日,
    [TotalAmount] DECIMAL(18,2) NOT NULL , -- 应收总金额,
    [TotalReceived] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 已收总金额,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 状态,
    [GeneratedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 生成时间,
    [GeneratedBy] UNIQUEIDENTIFIER NOT NULL , -- 生成人,
    [IsHistorical] BIT NOT NULL DEFAULT (0) , -- 是否历史账单,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：账单主表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单主表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes';
GO

-- DebitNotes 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期年份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'PeriodYear';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'PeriodMonth';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'BillNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'DueDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收总金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'TotalAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'已收总金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'TotalReceived';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生成时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'GeneratedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生成人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'GeneratedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否历史账单', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'IsHistorical';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 账单编号唯一
CREATE UNIQUE INDEX [IX_DebitNotes_BillNo] ON [DebitNotes]([BillNo])
-- 合同+账期唯一
CREATE UNIQUE INDEX [IX_DebitNotes_Contract_Period] ON [DebitNotes]([ContractId],[PeriodYear],[PeriodMonth])

-- ===================================================================
-- 38. DebitNoteItems 表：账单明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNoteItems]'))
CREATE TABLE [DebitNoteItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [DebitNoteId] UNIQUEIDENTIFIER NOT NULL , -- 账单ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [FeeName] NVARCHAR(100) NOT NULL , -- 费用名称,
    [Amount] DECIMAL(18,2) NOT NULL , -- 金额,
    [ReceivedAmount] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 已收金额,
    [Quantity] DECIMAL(18,4) , -- 数量,
    [UnitPrice] DECIMAL(18,4) , -- 单价,
    [Unit] NVARCHAR(20) , -- 单位,
    [Description] NVARCHAR(500) , -- 说明,
    [SortOrder] INT NOT NULL DEFAULT (0) , -- 排序,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedAt] DATETIME2 -- 更新时间
)
GO

-- 表说明：账单明细表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单明细表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems';
GO

-- DebitNoteItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'DebitNoteId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'FeeCodeId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'FeeName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'已收金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'ReceivedAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'数量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Quantity';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'UnitPrice';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Unit';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'SortOrder';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
GO

-- 按账单查询
CREATE INDEX [IX_DebitNoteItems_DebitNoteId] ON [DebitNoteItems]([DebitNoteId])

-- ===================================================================
-- 39. AutoRenewConfigs 表：自动续签配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AutoRenewConfigs]'))
CREATE TABLE [AutoRenewConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [IsAutoRenew] BIT NOT NULL DEFAULT (1) , -- 是否自动续签,
    [RenewalDaysBeforeExpiry] INT NOT NULL DEFAULT (30) , -- 提前续签天数,
    [RentAdjustmentPercent] DECIMAL(5,2) , -- 调价百分比,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Active') , -- 状态,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：自动续签配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'自动续签配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs';
GO

-- AutoRenewConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动续签', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'IsAutoRenew';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'提前续签天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'RenewalDaysBeforeExpiry';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'调价百分比', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'RentAdjustmentPercent';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- ===================================================================
-- Payment（支付）
-- ===================================================================

-- ===================================================================
-- 40. PaymentChannels 表：支付通道表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[PaymentChannels]'))
CREATE TABLE [PaymentChannels] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Code] VARCHAR(50) NOT NULL , -- 通道代码,
    [Name] NVARCHAR(200) NOT NULL , -- 通道名称,
    [ChannelType] VARCHAR(20) , -- 通道类型,
    [AccountNo] VARCHAR(100) , -- 收款账号,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：支付通道表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付通道表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels';
GO

-- PaymentChannels 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'ChannelType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'AccountNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 通道代码唯一
CREATE UNIQUE INDEX [IX_PaymentChannels_Code] ON [PaymentChannels]([Code])

-- ===================================================================
-- 41. Receipts 表：收据表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Receipts]'))
CREATE TABLE [Receipts] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ReceiptNo] VARCHAR(100) NOT NULL , -- 收据编号,
    [PaymentChannelId] UNIQUEIDENTIFIER NOT NULL , -- 支付通道ID,
    [Amount] DECIMAL(18,2) NOT NULL , -- 收款金额,
    [ReceivedDate] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 收款时间,
    [RemitterName] NVARCHAR(100) , -- 付款人,
    [RemitterAccount] VARCHAR(100) , -- 付款人账号,
    [TransactionRef] VARCHAR(200) , -- 交易参考号,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('PendingConfirm') , -- 状态,
    [RefundedReceiptId] UNIQUEIDENTIFIER , -- 被退款收据ID,
    [RowVersion] TIMESTAMP NOT NULL , -- 乐观锁,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：收据表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收据表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts';
GO

-- Receipts 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收据编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'ReceiptNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付通道ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'PaymentChannelId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'ReceivedDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'RemitterName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款人账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'RemitterAccount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'交易参考号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'TransactionRef';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'被退款收据ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'RefundedReceiptId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'乐观锁', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'RowVersion';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 收据编号唯一
CREATE UNIQUE INDEX [IX_Receipts_ReceiptNo] ON [Receipts]([ReceiptNo])
-- 交易参考号索引
CREATE INDEX [IX_Receipts_TransactionRef] ON [Receipts]([TransactionRef])

-- ===================================================================
-- 42. ReceiptAllocations 表：收据分配表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceiptAllocations]'))
CREATE TABLE [ReceiptAllocations] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ReceiptId] UNIQUEIDENTIFIER NOT NULL , -- 收据ID,
    [ReceivablePlanId] UNIQUEIDENTIFIER NOT NULL , -- 应收计划ID,
    [AllocatedAmount] DECIMAL(18,2) NOT NULL , -- 分配金额,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：收据分配表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收据分配表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceiptAllocations';
GO

-- ReceiptAllocations 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceiptAllocations', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收据ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceiptAllocations', @level2type = N'COLUMN', @level2name = N'ReceiptId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收计划ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceiptAllocations', @level2type = N'COLUMN', @level2name = N'ReceivablePlanId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分配金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceiptAllocations', @level2type = N'COLUMN', @level2name = N'AllocatedAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceiptAllocations', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceiptAllocations', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 收据+应收唯一
CREATE UNIQUE INDEX [IX_ReceiptAllocations_Receipt_Plan] ON [ReceiptAllocations]([ReceiptId],[ReceivablePlanId])

-- ===================================================================
-- Deposit（押金）
-- ===================================================================

-- ===================================================================
-- 43. DepositLogs 表：押金记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DepositLogs]'))
CREATE TABLE [DepositLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [ActionType] VARCHAR(20) NOT NULL , -- 操作类型,
    [Amount] DECIMAL(18,2) NOT NULL , -- 变动金额,
    [BalanceAfter] DECIMAL(18,2) NOT NULL , -- 操作后余额,
    [RelatedReceiptId] UNIQUEIDENTIFIER , -- 关联收据ID,
    [Remarks] NVARCHAR(500) , -- 备注,
    [OperatedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [OperatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 操作时间,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：押金记录表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金记录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs';
GO

-- DepositLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'ActionType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变动金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作后余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'BalanceAfter';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联收据ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'RelatedReceiptId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'Remarks';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'OperatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'OperatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 按合同查询
CREATE INDEX [IX_DepositLogs_ContractId] ON [DepositLogs]([ContractId])

-- ===================================================================
-- Collection（催缴）
-- ===================================================================

-- ===================================================================
-- 44. CollectionStages 表：催缴阶段配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionStages]'))
CREATE TABLE [CollectionStages] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [StageNo] INT NOT NULL , -- 阶段编号,
    [StageName] NVARCHAR(100) NOT NULL , -- 阶段名称,
    [OverdueDaysFrom] INT NOT NULL , -- 逾期起始天数,
    [OverdueDaysTo] INT NOT NULL , -- 逾期结束天数,
    [ActionType] VARCHAR(20) NOT NULL , -- 催缴动作,
    [IsAuto] BIT NOT NULL DEFAULT (1) , -- 是否自动执行,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：催缴阶段配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'催缴阶段配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages';
GO

-- CollectionStages 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'阶段编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'StageNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'阶段名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'StageName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'逾期起始天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'OverdueDaysFrom';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'逾期结束天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'OverdueDaysTo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'催缴动作', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'ActionType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动执行', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'IsAuto';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- ===================================================================
-- 45. CollectionRecords 表：催缴记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionRecords]'))
CREATE TABLE [CollectionRecords] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [StageNo] INT NOT NULL , -- 阶段编号,
    [Channel] VARCHAR(20) NOT NULL , -- 发送渠道,
    [Content] NVARCHAR(MAX) NOT NULL , -- 发送内容,
    [Status] VARCHAR(20) NOT NULL , -- 状态,
    [SentAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 发送时间,
    [OperatedBy] UNIQUEIDENTIFIER , -- 操作人,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：催缴记录表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'催缴记录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords';
GO

-- CollectionRecords 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'阶段编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'StageNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'发送渠道', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'Channel';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'发送内容', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'Content';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'发送时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'SentAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'OperatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 按合同查询
CREATE INDEX [IX_CollectionRecords_ContractId] ON [CollectionRecords]([ContractId])

-- ===================================================================
-- Accounting & Bank（会计核算与银行）
-- ===================================================================

-- ===================================================================
-- 46. AccountingSubjects 表：会计科目表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AccountingSubjects]'))
CREATE TABLE [AccountingSubjects] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Code] VARCHAR(50) NOT NULL , -- 科目代码,
    [Name] NVARCHAR(200) NOT NULL , -- 科目名称,
    [ParentCode] VARCHAR(50) , -- 父科目代码,
    [Level] INT NOT NULL DEFAULT (1) , -- 科目层级,
    [Direction] VARCHAR(10) NOT NULL DEFAULT ('Debit') , -- 借贷方向,
    [IsLeaf] BIT NOT NULL DEFAULT (1) , -- 是否末级科目,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：会计科目表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计科目表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects';
GO

-- AccountingSubjects 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Name';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'父科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'ParentCode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目层级', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Level';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借贷方向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Direction';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否末级科目', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'IsLeaf';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 科目代码唯一
CREATE UNIQUE INDEX [IX_AccountingSubjects_Code] ON [AccountingSubjects]([Code])

-- ===================================================================
-- 47. Vouchers 表：会计凭证表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Vouchers]'))
CREATE TABLE [Vouchers] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [VoucherNo] VARCHAR(100) NOT NULL , -- 凭证编号,
    [VoucherDate] DATE NOT NULL , -- 凭证日期,
    [TotalDebit] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 借方总额,
    [TotalCredit] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 贷方总额,
    [SourceType] VARCHAR(50) NOT NULL , -- 来源类型,
    [SourceId] UNIQUEIDENTIFIER NOT NULL , -- 来源ID,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Draft') , -- 状态,
    [ContractId] UNIQUEIDENTIFIER , -- 合同ID,
    [ApprovedBy] UNIQUEIDENTIFIER , -- 审核人,
    [ApprovedAt] DATETIME2 , -- 审核时间,
    [RowVersion] TIMESTAMP NOT NULL , -- 乐观锁,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：会计凭证表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计凭证表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers';
GO

-- Vouchers 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'凭证编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'VoucherNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'凭证日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'VoucherDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借方总额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'TotalDebit';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'贷方总额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'TotalCredit';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'来源类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'SourceType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'来源ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'SourceId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审核人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'ApprovedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审核时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'ApprovedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'乐观锁', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'RowVersion';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 凭证编号唯一
CREATE UNIQUE INDEX [IX_Vouchers_VoucherNo] ON [Vouchers]([VoucherNo])
-- 按状态查询
CREATE INDEX [IX_Vouchers_Status] ON [Vouchers]([Status])
-- 按合同查询
CREATE INDEX [IX_Vouchers_ContractId] ON [Vouchers]([ContractId])

-- ===================================================================
-- 48. JournalEntries 表：日记账分录明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JournalEntries]'))
CREATE TABLE [JournalEntries] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [VoucherId] UNIQUEIDENTIFIER NOT NULL , -- 凭证ID,
    [AccountingSubjectId] UNIQUEIDENTIFIER NOT NULL , -- 科目ID,
    [EntryNo] INT NOT NULL , -- 行号,
    [Direction] VARCHAR(10) NOT NULL , -- 借贷方向,
    [Amount] DECIMAL(18,2) NOT NULL , -- 金额,
    [Summary] NVARCHAR(500) , -- 摘要,
    [ContractId] UNIQUEIDENTIFIER , -- 合同ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：日记账分录明细表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日记账分录明细表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries';
GO

-- JournalEntries 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'凭证ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'VoucherId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'AccountingSubjectId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'行号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'EntryNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借贷方向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'Direction';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'摘要', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'Summary';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'ContractId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JournalEntries', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 凭证内行号唯一
CREATE UNIQUE INDEX [IX_JournalEntries_VoucherId_EntryNo] ON [JournalEntries]([VoucherId],[EntryNo])
-- 按合同查询
CREATE INDEX [IX_JournalEntries_ContractId] ON [JournalEntries]([ContractId])

-- ===================================================================
-- 49. BankMatches 表：银行匹配记录表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankMatches]'))
CREATE TABLE [BankMatches] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [BankStatementId] UNIQUEIDENTIFIER NOT NULL , -- 银行流水ID,
    [InternalDocumentId] UNIQUEIDENTIFIER NOT NULL , -- 内部单据ID,
    [DocumentType] VARCHAR(20) NOT NULL DEFAULT ('Receipt') , -- 单据类型,
    [MatchedAmount] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 匹配金额,
    [MatchMethod] VARCHAR(20) NOT NULL DEFAULT ('Manual') , -- 匹配方式,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：银行匹配记录表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行匹配记录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches';
GO

-- BankMatches 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行流水ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches', @level2type = N'COLUMN', @level2name = N'BankStatementId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'内部单据ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches', @level2type = N'COLUMN', @level2name = N'InternalDocumentId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单据类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches', @level2type = N'COLUMN', @level2name = N'DocumentType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'匹配金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches', @level2type = N'COLUMN', @level2name = N'MatchedAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'匹配方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches', @level2type = N'COLUMN', @level2name = N'MatchMethod';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankMatches', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 按流水查询
CREATE INDEX [IX_BankMatches_BankStatementId] ON [BankMatches]([BankStatementId])

-- ===================================================================
-- 50. BankReconciliations 表：银行余额调节表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankReconciliations]'))
CREATE TABLE [BankReconciliations] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [StartDate] DATE NOT NULL , -- 开始日期,
    [EndDate] DATE NOT NULL , -- 结束日期,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('InProgress') , -- 状态,
    [OpeningBalance] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 期初余额,
    [ClosingBalance] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 期末余额,
    [StatementTotal] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 银行总额,
    [SystemTotal] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 系统总额,
    [CompletedAt] DATETIME2 , -- 完成时间,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：银行余额调节表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行余额调节表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations';
GO

-- BankReconciliations 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开始日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'StartDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结束日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'EndDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期初余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'OpeningBalance';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期末余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'ClosingBalance';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行总额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'StatementTotal';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'系统总额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'SystemTotal';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'完成时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'CompletedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- ===================================================================
-- 51. BankStatements 表：银行流水表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankStatements]'))
CREATE TABLE [BankStatements] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [TransactionDate] DATE NOT NULL , -- 交易日期,
    [Amount] DECIMAL(18,2) NOT NULL , -- 金额,
    [Balance] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 余额,
    [Description] NVARCHAR(MAX) , -- 描述,
    [ReferenceNo] NVARCHAR(100) , -- 参考号,
    [Counterparty] NVARCHAR(200) , -- 对方账户,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Unmatched') , -- 状态,
    [ImportBatchId] UNIQUEIDENTIFIER , -- 导入批次ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：银行流水表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行流水表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements';
GO

-- BankStatements 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'交易日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'TransactionDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Amount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Balance';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'参考号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'ReferenceNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'对方账户', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Counterparty';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入批次ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'ImportBatchId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- ===================================================================
-- Scheduling（调度）
-- ===================================================================

-- ===================================================================
-- 52. JobSchedules 表：任务实例表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobSchedules]'))
CREATE TABLE [JobSchedules] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [JobName] NVARCHAR(200) NOT NULL , -- 任务名称,
    [CronExpression] NVARCHAR(100) NOT NULL , -- Cron表达式,
    [TemplateCode] NVARCHAR(50) , -- 模板代码,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [Description] NVARCHAR(500) , -- 描述,
    [LastRunAt] DATETIME2 , -- 上次执行时间,
    [LastRunStatus] NVARCHAR(20) , -- 上次执行结果,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：任务实例表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务实例表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules';
GO

-- JobSchedules 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'JobName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'Cron表达式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CronExpression';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'模板代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'TemplateCode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上次执行时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'LastRunAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上次执行结果', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'LastRunStatus';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- ===================================================================
-- 53. JobScheduleExecutions 表：执行排期表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobScheduleExecutions]'))
CREATE TABLE [JobScheduleExecutions] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [JobScheduleId] UNIQUEIDENTIFIER NOT NULL , -- 任务定义ID,
    [TargetDate] DATETIME2 NOT NULL , -- 排期执行时间,
    [OriginalDate] DATETIME2 , -- 原始Cron时间,
    [Month] NVARCHAR(7) NOT NULL , -- 所属月份,
    [Status] NVARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 状态,
    [Reason] NVARCHAR(500) , -- 备注,
    [IsAdjusted] BIT NOT NULL DEFAULT (0) , -- 是否手动调整,
    [IsCustom] BIT NOT NULL DEFAULT (0) , -- 是否自定义,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：执行排期表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'执行排期表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions';
GO

-- JobScheduleExecutions 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务定义ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'JobScheduleId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排期执行时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'TargetDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原始Cron时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'OriginalDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'Month';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'Reason';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否手动调整', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'IsAdjusted';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自定义', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'IsCustom';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 按任务查询
CREATE INDEX [IX_Executions_JobScheduleId] ON [JobScheduleExecutions]([JobScheduleId])
-- 按执行时间排序
CREATE INDEX [IX_Executions_TargetDate] ON [JobScheduleExecutions]([TargetDate])

-- ===================================================================
-- 54. JobTemplates 表：任务模板表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobTemplates]'))
CREATE TABLE [JobTemplates] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Code] NVARCHAR(50) NOT NULL , -- 模板代码,
    [DisplayName] NVARCHAR(100) NOT NULL , -- 显示名,
    [ShortName] NVARCHAR(50) NOT NULL , -- 短名,
    [DefaultCronExpression] NVARCHAR(100) NOT NULL , -- 默认Cron,
    [Description] NVARCHAR(500) , -- 说明,
    [Icon] NVARCHAR(50) , -- 图标,
    [Category] NVARCHAR(50) NOT NULL , -- 分类,
    [SortOrder] INT NOT NULL DEFAULT (0) , -- 排序,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：任务模板表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务模板表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates';
GO

-- JobTemplates 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'模板代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Code';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'显示名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'DisplayName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'短名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'ShortName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认Cron', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'DefaultCronExpression';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Description';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'图标', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Icon';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Category';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'SortOrder';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 模板代码唯一
CREATE UNIQUE INDEX [IX_JobTemplates_Code] ON [JobTemplates]([Code])

-- ===================================================================
-- Other（其他系统表）
-- ===================================================================

-- ===================================================================
-- 55. ApiLogs 表：API请求日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApiLogs]'))
CREATE TABLE [ApiLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ApiPath] NVARCHAR(500) , -- API路径,
    [HttpMethod] VARCHAR(10) , -- HTTP方法,
    [StatusCode] INT , -- 状态码,
    [RequestBody] NVARCHAR(MAX) , -- 请求体,
    [ResponseBody] NVARCHAR(MAX) , -- 响应体,
    [RequestHeaders] NVARCHAR(MAX) , -- 请求头,
    [ClientIp] VARCHAR(50) , -- 客户端IP,
    [UserId] UNIQUEIDENTIFIER , -- 用户ID,
    [UserDisplayName] NVARCHAR(100) , -- 用户显示名,
    [DurationMs] INT DEFAULT (0) , -- 耗时ms,
    [QueryString] NVARCHAR(2000) , -- 查询参数,
    [UserAgent] NVARCHAR(500) , -- 用户代理,
    [RequestAt] DATETIME2 DEFAULT (GETUTCDATE()) , -- 请求时间,
    [ResponseAt] DATETIME2 -- 响应时间
)
GO

-- 表说明：API请求日志表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'API请求日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs';
GO

-- ApiLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'API路径', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'ApiPath';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'HTTP方法', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'HttpMethod';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'StatusCode';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求体', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'RequestBody';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'响应体', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'ResponseBody';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求头', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'RequestHeaders';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'客户端IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'ClientIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'UserId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户显示名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'UserDisplayName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'耗时ms', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'DurationMs';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'查询参数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'QueryString';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户代理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'UserAgent';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'RequestAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'响应时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'ResponseAt';
GO

-- ===================================================================
-- 56. TaskLogs 表：任务执行日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaskLogs]'))
CREATE TABLE [TaskLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [TaskName] NVARCHAR(200) NOT NULL , -- 任务名称,
    [StartedAt] DATETIME2 , -- 开始时间,
    [CompletedAt] DATETIME2 , -- 完成时间,
    [Status] NVARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 状态,
    [ErrorMessage] NVARCHAR(2000) , -- 错误信息,
    [TargetMonth] NVARCHAR(7) , -- 目标月份,
    [HeartbeatAt] DATETIME2 , -- 心跳时间,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL -- 所属公司ID
)
GO

-- 表说明：任务执行日志表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务执行日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs';
GO

-- TaskLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'TaskName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开始时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'StartedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'完成时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'CompletedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'错误信息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'ErrorMessage';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'目标月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'TargetMonth';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'心跳时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'HeartbeatAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'CompanyId';
GO

-- 同公司同月同一任务执行锁
CREATE UNIQUE INDEX [IX_TaskLogs_Lock] ON [TaskLogs]([TaskName],[CompanyId],[TargetMonth]) WHERE [TargetMonth] IS NOT NULL;

-- ===================================================================
-- 57. SystemLogs 表：系统日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[SystemLogs]'))
CREATE TABLE [SystemLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Level] VARCHAR(20) , -- 日志级别,
    [Logger] NVARCHAR(200) , -- 日志记录器,
    [Message] NVARCHAR(MAX) , -- 消息,
    [Exception] NVARCHAR(MAX) , -- 异常,
    [LogDate] DATETIME2 DEFAULT (GETUTCDATE()) -- 日志时间
)
GO

-- 表说明：系统日志表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'系统日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs';
GO

-- SystemLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日志级别', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Level';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日志记录器', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Logger';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'消息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Message';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'异常', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Exception';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日志时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'LogDate';
GO

-- ===================================================================
-- 58. HolidayCalendars 表：节假日配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HolidayCalendars]'))
CREATE TABLE [HolidayCalendars] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Year] INT NOT NULL , -- 年份,
    [HolidayDate] DATE NOT NULL , -- 日期,
    [HolidayName] NVARCHAR(100) NOT NULL , -- 节假日名称,
    [HolidayType] VARCHAR(20) NOT NULL , -- 类型,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否生效,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：节假日配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'节假日配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars';
GO

-- HolidayCalendars 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'年份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'Year';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'HolidayDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'节假日名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'HolidayName';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'HolidayType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否生效', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- 同年同日期唯一
CREATE UNIQUE INDEX [IX_HolidayCalendars_Year_Date] ON [HolidayCalendars]([Year],[HolidayDate])

-- ===================================================================
-- 59. LateFeeConfigs 表：滞纳金配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[LateFeeConfigs]'))
CREATE TABLE [LateFeeConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [DailyRate] DECIMAL(5,4) NOT NULL DEFAULT (0) , -- 日利率,
    [GracePeriodDays] INT NOT NULL DEFAULT (3) , -- 宽限期天数,
    [MaxPercentOfPrincipal] DECIMAL(5,2) , -- 上限百分比,
    [MinLateFeeAmount] DECIMAL(18,2) , -- 最低金额,
    [EffectiveDate] DATE , -- 生效日期,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：滞纳金配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'滞纳金配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs';
GO

-- LateFeeConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日利率', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'DailyRate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'宽限期天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'GracePeriodDays';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上限百分比', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'MaxPercentOfPrincipal';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'最低金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'MinLateFeeAmount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'EffectiveDate';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'IsActive';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LateFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname';
GO

-- ===================================================================
-- 60. Notifications 表：站内通知表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Notifications]'))
CREATE TABLE [Notifications] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [UserId] UNIQUEIDENTIFIER NOT NULL , -- 用户ID,
    [CompanyId] UNIQUEIDENTIFIER , -- 公司ID,
    [Category] VARCHAR(50) NOT NULL , -- 通知分类,
    [Title] NVARCHAR(200) NOT NULL , -- 标题,
    [Content] NVARCHAR(MAX) , -- 内容,
    [ReferenceType] VARCHAR(50) , -- 关联类型,
    [ReferenceId] UNIQUEIDENTIFIER , -- 关联ID,
    [IsRead] BIT NOT NULL DEFAULT (0) , -- 是否已读,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：站内通知表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'站内通知表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications';
GO

-- Notifications 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'UserId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通知分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'Category';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'标题', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'Title';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'内容', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'Content';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'ReferenceType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'ReferenceId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已读', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'IsRead';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 按用户查询
CREATE INDEX [IX_Notifications_UserId] ON [Notifications]([UserId])
-- 按用户+分类查询
CREATE INDEX [IX_Notifications_UserId_Category] ON [Notifications]([UserId],[Category])
-- 未读通知查询
CREATE INDEX [IX_Notifications_Unread] ON [Notifications]([UserId],[IsRead]) WHERE [IsRead]=0;

-- ===================================================================
-- 61. ImportBatches 表：导入批次表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatches]'))
CREATE TABLE [ImportBatches] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [BatchNo] VARCHAR(50) NOT NULL , -- 批次号,
    [ImportType] VARCHAR(50) NOT NULL , -- 导入类型,
    [TotalCount] INT NOT NULL DEFAULT (0) , -- 总数,
    [SuccessCount] INT NOT NULL DEFAULT (0) , -- 成功数,
    [FailCount] INT NOT NULL DEFAULT (0) , -- 失败数,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Processing') , -- 状态,
    [ErrorMessage] NVARCHAR(MAX) , -- 错误信息,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：导入批次表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入批次表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches';
GO

-- ImportBatches 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'批次号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'BatchNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'ImportType';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'总数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'TotalCount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'成功数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'SuccessCount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'失败数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'FailCount';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'错误信息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'ErrorMessage';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'CompanyId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 批次号唯一
CREATE UNIQUE INDEX [IX_ImportBatches_BatchNo] ON [ImportBatches]([BatchNo])

-- ===================================================================
-- 62. ImportBatchItems 表：导入明细表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatchItems]'))
CREATE TABLE [ImportBatchItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [BatchId] UNIQUEIDENTIFIER NOT NULL , -- 批次ID,
    [RowNo] INT NOT NULL , -- 行号,
    [RawData] NVARCHAR(MAX) , -- 原始数据,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 状态,
    [ErrorMessage] NVARCHAR(2000) , -- 错误信息,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：导入明细表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入明细表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems';
GO

-- ImportBatchItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'Id';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'批次ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'BatchId';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'行号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'RowNo';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原始数据', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'RawData';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'Status';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'错误信息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'ErrorMessage';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'CreatedBy';
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'CreatedAt';
GO

-- 按批次查询
CREATE INDEX [IX_ImportBatchItems_BatchId] ON [ImportBatchItems]([BatchId])

-- ===================================================================
-- Init.sql - 结束
-- 共 62 张表
-- ===================================================================