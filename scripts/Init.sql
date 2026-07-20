-- ===================================================================
-- Init.sql - 数据库初始化脚本
-- 包含所有业务表 + _Audit 镜像审计表的 CREATE TABLE 定义
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司根表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies'
GO

-- Companies 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'IdType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件号码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'IdNumber'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'联系人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'ContactPerson'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'联系电话', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Phone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通讯地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Address'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开户行', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'BankName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'BankAccount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开户名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'BankAccountName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结算周期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'SettlementCycle'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结算日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'SettlementDay'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'佣金比例', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CommissionRate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'Remark'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 公司编号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Companies]') AND name=N'IX_Companies_Code')
CREATE UNIQUE INDEX [IX_Companies_Code] ON [Companies]([Code]) WHERE [Code] IS NOT NULL

-- ===================================================================
-- Companies_Audit 表：公司根表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Companies_Audit]'))
CREATE TABLE [Companies_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Name] NVARCHAR(200) , -- 公司名称,
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
    [IsActive] BIT , -- 是否启用,
    [Remark] NVARCHAR(500) , -- 备注,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- Companies_Audit 表说明：公司根表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司根表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit'
GO

-- Companies_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'IdType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件号码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'IdNumber'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'联系人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'ContactPerson'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'联系电话', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'Phone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通讯地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'Address'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开户行', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'BankName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'BankAccount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开户名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'BankAccountName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结算周期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'SettlementCycle'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结算日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'SettlementDay'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'佣金比例', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'CommissionRate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'Remark'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Companies_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Companies_Audit]') AND name=N'IX_Companies_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_Companies_Audit_Id_Version] ON [Companies_Audit]([Id], [AuditVersionNo])


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
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users'
GO

-- Users 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'登录用户名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'Username'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'密码哈希', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'PasswordHash'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'显示名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'DisplayName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'手机号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'Phone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'邮箱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'Email'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'DefaultCompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否超级管理员', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'IsSuperAdmin'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 用户名唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Users]') AND name=N'IX_Users_Username')
CREATE UNIQUE INDEX [IX_Users_Username] ON [Users]([Username])
-- 按公司查询索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Users]') AND name=N'IX_Users_CompanyId')
CREATE INDEX [IX_Users_CompanyId] ON [Users]([CompanyId])

-- ===================================================================
-- Users_Audit 表：用户表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Users_Audit]'))
CREATE TABLE [Users_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Username] VARCHAR(100) , -- 登录用户名,
    [PasswordHash] VARCHAR(500) , -- 密码哈希,
    [DisplayName] NVARCHAR(100) , -- 显示名称,
    [Phone] VARCHAR(20) , -- 手机号,
    [Email] VARCHAR(100) , -- 邮箱,
    [IsActive] BIT , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [DefaultCompanyId] UNIQUEIDENTIFIER , -- 默认公司ID,
    [IsSuperAdmin] BIT , -- 是否超级管理员,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- Users_Audit 表说明：用户表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit'
GO

-- Users_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'登录用户名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'Username'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'密码哈希', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'PasswordHash'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'显示名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'DisplayName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'手机号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'Phone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'邮箱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'Email'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'DefaultCompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否超级管理员', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'IsSuperAdmin'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Users_Audit]') AND name=N'IX_Users_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_Users_Audit_Id_Version] ON [Users_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles'
GO

-- Roles 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 角色代码唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Roles]') AND name=N'IX_Roles_Code')
CREATE UNIQUE INDEX [IX_Roles_Code] ON [Roles]([Code])

-- ===================================================================
-- Roles_Audit 表：角色表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Roles_Audit]'))
CREATE TABLE [Roles_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Name] NVARCHAR(100) , -- 角色名称,
    [Code] VARCHAR(50) , -- 角色代码,
    [Description] NVARCHAR(500) , -- 描述,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [IsActive] BIT , -- 是否启用,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- Roles_Audit 表说明：角色表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit'
GO

-- Roles_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Roles_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Roles_Audit]') AND name=N'IX_Roles_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_Roles_Audit_Id_Version] ON [Roles_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户角色关联表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles'
GO

-- UserRoles 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'UserId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'RoleId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'UserRoles', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 用户角色联合唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[UserRoles]') AND name=N'IX_UserRoles_UserId_RoleId')
CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles]([UserId],[RoleId])
-- 按角色查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[UserRoles]') AND name=N'IX_UserRoles_RoleId')
CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles]([RoleId])



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
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'菜单表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus'
GO

-- Menus 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'父菜单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'ParentId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'菜单名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'前端路由路径', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Path'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'图标', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Icon'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'权限代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'PermissionCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序序号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'可见范围', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'Scope'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 权限代码唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Menus]') AND name=N'IX_Menus_PermissionCode')
CREATE UNIQUE INDEX [IX_Menus_PermissionCode] ON [Menus]([PermissionCode]) WHERE [PermissionCode] IS NOT NULL

-- ===================================================================
-- Menus_Audit 表：菜单表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Menus_Audit]'))
CREATE TABLE [Menus_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [ParentId] UNIQUEIDENTIFIER , -- 父菜单ID,
    [Name] NVARCHAR(100) , -- 菜单名称,
    [Path] VARCHAR(200) , -- 前端路由路径,
    [Icon] VARCHAR(50) , -- 图标,
    [PermissionCode] VARCHAR(100) , -- 权限代码,
    [SortOrder] INT , -- 排序序号,
    [IsActive] BIT , -- 是否启用,
    [Scope] VARCHAR(20) , -- 可见范围,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- Menus_Audit 表说明：菜单表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'菜单表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit'
GO

-- Menus_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'父菜单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'ParentId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'菜单名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'前端路由路径', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'Path'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'图标', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'Icon'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'权限代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'PermissionCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序序号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'可见范围', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'Scope'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Menus_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Menus_Audit]') AND name=N'IX_Menus_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_Menus_Audit_Id_Version] ON [Menus_Audit]([Id], [AuditVersionNo])


-- ===================================================================
-- RoleMenus 表：角色菜单权限关联表
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

-- 表说明：角色菜单权限关联表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色菜单权限关联表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus'
GO

-- RoleMenus 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'角色ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'RoleId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'菜单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'MenuId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoleMenus', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO

-- 角色+菜单唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[RoleMenus]') AND name=N'IX_RoleMenus_Role_Menu')
CREATE UNIQUE INDEX [IX_RoleMenus_Role_Menu] ON [RoleMenus]([RoleId],[MenuId])


-- ===================================================================
-- ApprovalTypes 表：审批类型定义表
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes'
GO

-- ApprovalTypes 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型编码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'路由策略', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'RoutingStrategy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 审批类型编码唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalTypes]') AND name=N'IX_ApprovalTypes_Code')
CREATE UNIQUE INDEX [IX_ApprovalTypes_Code] ON [ApprovalTypes]([Code])

-- ===================================================================
-- ApprovalTypes_Audit 表：审批类型表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalTypes_Audit]'))
CREATE TABLE [ApprovalTypes_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Code] VARCHAR(50) , -- 审批类型编码,
    [Name] NVARCHAR(100) , -- 审批类型名称,
    [Description] NVARCHAR(500) , -- 描述,
    [RoutingStrategy] VARCHAR(20) , -- 路由策略,
    [IsActive] BIT , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- ApprovalTypes_Audit 表说明：审批类型表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit'
GO

-- ApprovalTypes_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型编码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'路由策略', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'RoutingStrategy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalTypes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalTypes_Audit]') AND name=N'IX_ApprovalTypes_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_ApprovalTypes_Audit_Id_Version] ON [ApprovalTypes_Audit]([Id], [AuditVersionNo])


-- ===================================================================
-- 10. ApprovalLevelConfigs 表：审批级别配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs]'))
CREATE TABLE [ApprovalLevelConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ApprovalTypeId] UNIQUEIDENTIFIER NOT NULL , -- 审批类型ID,
    [LevelNo] INT NOT NULL , -- 审批级别序号(旧),
    [ApproverRoleId] UNIQUEIDENTIFIER NOT NULL , -- 审批角色ID(旧),
    [Level] INT NOT NULL DEFAULT (0) , -- 审批级别序号,
    [RoleId] UNIQUEIDENTIFIER NOT NULL , -- 审批角色ID,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs'
GO

-- ApprovalLevelConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'ApprovalTypeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别序号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'LevelNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批角色ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'ApproverRoleId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批模式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'ApprovalMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额下限', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'MinAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额上限', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'MaxAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'累计金额检查', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'IsCumulativeCheck'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'累计检查窗口天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CumulativeWindowDays'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 审批类型+级别唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs]') AND name=N'IX_ApprovalLevelConfigs_Type_Level')
CREATE UNIQUE INDEX [IX_ApprovalLevelConfigs_Type_Level] ON [ApprovalLevelConfigs]([ApprovalTypeId],[LevelNo])

-- ===================================================================
-- ApprovalLevelConfigs_Audit 表：审批级别配置表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs_Audit]'))
CREATE TABLE [ApprovalLevelConfigs_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [ApprovalTypeId] UNIQUEIDENTIFIER , -- 审批类型ID,
    [LevelNo] INT , -- 审批级别序号,
    [ApproverRoleId] UNIQUEIDENTIFIER , -- 审批角色ID,
    [ApprovalMode] VARCHAR(20) , -- 审批模式,
    [MinAmount] DECIMAL(18,2) , -- 金额下限,
    [MaxAmount] DECIMAL(18,2) , -- 金额上限,
    [IsCumulativeCheck] BIT , -- 累计金额检查,
    [CumulativeWindowDays] INT , -- 累计检查窗口天数,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- ApprovalLevelConfigs_Audit 表说明：审批级别配置表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别配置表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit'
GO

-- ApprovalLevelConfigs_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'ApprovalTypeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别序号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'LevelNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批角色ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'ApproverRoleId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批模式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'ApprovalMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额下限', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'MinAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额上限', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'MaxAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'累计金额检查', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'IsCumulativeCheck'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'累计检查窗口天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CumulativeWindowDays'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalLevelConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalLevelConfigs_Audit]') AND name=N'IX_ApprovalLevelConfigs_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_ApprovalLevelConfigs_Audit_Id_Version] ON [ApprovalLevelConfigs_Audit]([Id], [AuditVersionNo])


-- ===================================================================
-- 11. ApprovalRequests 表：审批申请表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRequests]'))
CREATE TABLE [ApprovalRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RequestNo] VARCHAR(100) NULL , -- 申请编号,
    [ApprovalTypeId] UNIQUEIDENTIFIER NOT NULL , -- 审批类型ID,
    [Title] NVARCHAR(200) , -- 审批标题,
    [Description] NVARCHAR(500) , -- 审批描述,
    [TargetEntityId] UNIQUEIDENTIFIER , -- 目标实体ID,
    [TargetEntityType] NVARCHAR(64) , -- 目标实体类型,
    [MaxLevel] INT NOT NULL DEFAULT (0) , -- 最大审批级别,
    [BusinessId] UNIQUEIDENTIFIER , -- 业务ID,
    [BusinessData] NVARCHAR(MAX) , -- 业务数据JSON,
    [Amount] DECIMAL(18,2) NULL , -- 申请金额,
    [Reason] NVARCHAR(500) NULL , -- 申请原因,
    [RequesterId] UNIQUEIDENTIFIER NULL , -- 申请人ID,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 审批状态,
    [CurrentLevel] INT NOT NULL DEFAULT (0) , -- 当前审批级别,
    [CallbackStatus] VARCHAR(20) NULL , -- 回调状态,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批申请表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests'
GO

-- ApprovalRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'RequestNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'ApprovalTypeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'业务ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'BusinessId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'业务数据JSON', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'BusinessData'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请原因', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'Reason'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'RequesterId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'当前审批级别', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CurrentLevel'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'回调状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CallbackStatus'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'回调错误', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CallbackError'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'重试次数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CallbackRetryCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'乐观锁', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'RowVersion'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 申请编号唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalRequests]') AND name=N'IX_ApprovalRequests_RequestNo')
-- CREATE UNIQUE INDEX [IX_ApprovalRequests_RequestNo] -- Removed: RequestNo is now nullable ON [ApprovalRequests]([RequestNo])
-- 按状态查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalRequests]') AND name=N'IX_ApprovalRequests_Status')
CREATE INDEX [IX_ApprovalRequests_Status] ON [ApprovalRequests]([Status])
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalRequests]') AND name=N'IX_ApprovalRequests_ContractId')
CREATE INDEX [IX_ApprovalRequests_ContractId] ON [ApprovalRequests]([ContractId])

-- ===================================================================
-- ApprovalRequests_Audit 表：审批申请表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRequests_Audit]'))
CREATE TABLE [ApprovalRequests_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [RequestNo] VARCHAR(100) , -- 申请编号,
    [ApprovalTypeId] UNIQUEIDENTIFIER , -- 审批类型ID,
    [BusinessId] UNIQUEIDENTIFIER , -- 业务ID,
    [BusinessData] NVARCHAR(MAX) , -- 业务数据JSON,
    [Amount] DECIMAL(18,2) , -- 申请金额,
    [Reason] NVARCHAR(500) , -- 申请原因,
    [RequesterId] UNIQUEIDENTIFIER , -- 申请人ID,
    [Status] VARCHAR(20) , -- 审批状态,
    [CurrentLevel] INT , -- 当前审批级别,
    [CallbackStatus] VARCHAR(20) , -- 回调状态,
    [CallbackError] NVARCHAR(MAX) , -- 回调错误,
    [CallbackRetryCount] INT , -- 重试次数,
    [ContractId] UNIQUEIDENTIFIER , -- 合同ID,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- ApprovalRequests_Audit 表说明：审批申请表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批申请表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit'
GO

-- ApprovalRequests_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'RequestNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批类型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'ApprovalTypeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'业务ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'BusinessId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'业务数据JSON', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'BusinessData'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请原因', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'Reason'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'申请人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'RequesterId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'当前审批级别', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CurrentLevel'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'回调状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CallbackStatus'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'回调错误', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CallbackError'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'重试次数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CallbackRetryCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRequests_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalRequests_Audit]') AND name=N'IX_ApprovalRequests_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_ApprovalRequests_Audit_Id_Version] ON [ApprovalRequests_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批操作记录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords'
GO

-- ApprovalRecords 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批申请ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'RequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'LevelNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'ApproverId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批动作', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'Action'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批意见', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'Comment'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'OperatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 按申请查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalRecords]') AND name=N'IX_ApprovalRecords_RequestId')
CREATE INDEX [IX_ApprovalRecords_RequestId] ON [ApprovalRecords]([RequestId])

-- ===================================================================
-- ApprovalRecords_Audit 表：审批操作记录表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ApprovalRecords_Audit]'))
CREATE TABLE [ApprovalRecords_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [RequestId] UNIQUEIDENTIFIER , -- 审批申请ID,
    [LevelNo] INT , -- 审批级别,
    [ApproverId] UNIQUEIDENTIFIER , -- 审批人ID,
    [Action] VARCHAR(20) , -- 审批动作,
    [Comment] NVARCHAR(500) , -- 审批意见,
    [OperatedAt] DATETIME2 , -- 操作时间,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- ApprovalRecords_Audit 表说明：审批操作记录表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批操作记录表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit'
GO

-- ApprovalRecords_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批申请ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'RequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批级别', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'LevelNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'ApproverId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批动作', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'Action'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批意见', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'Comment'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'OperatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalRecords_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalRecords_Audit]') AND name=N'IX_ApprovalRecords_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_ApprovalRecords_Audit_Id_Version] ON [ApprovalRecords_Audit]([Id], [AuditVersionNo])


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
    [EffectiveDate] DATETIME , -- 生效日期,
    [OldAmount] DECIMAL(18,2) , -- 旧金额,
    [NewAmount] DECIMAL(18,2) , -- 新金额,
    [Reason] NVARCHAR(500) , -- 原因说明,
    [TerminateType] VARCHAR(20) , -- 终止类型,
    [ActualEndDate] DATETIME , -- 实际搬离日,
    [DepositReturn] VARCHAR(20) , -- 押金处理,
    [IsProcessed] BIT NOT NULL DEFAULT (0) , -- 是否已处理,
    [ProcessedAt] DATETIME2 , -- 处理时间,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：审批业务数据表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批业务数据表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData'
GO

-- ApprovalBizData 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ContractNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ChangeType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'旧金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'OldAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'NewAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原因说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'Reason'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'终止类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'TerminateType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'实际搬离日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ActualEndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金处理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'DepositReturn'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已处理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'IsProcessed'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'处理时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'ProcessedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalBizData', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 审批请求ID唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalBizData]') AND name=N'IX_ApprovalBizData_ApprovalRequestId')
CREATE UNIQUE INDEX [IX_ApprovalBizData_ApprovalRequestId] ON [ApprovalBizData]([ApprovalRequestId]) WHERE [ApprovalRequestId] IS NOT NULL
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalBizData]') AND name=N'IX_ApprovalBizData_ContractId')
CREATE INDEX [IX_ApprovalBizData_ContractId] ON [ApprovalBizData]([ContractId])

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
    [EffectiveDate] VARCHAR(10) , -- 生效日期（每条费用独立，yyyy-MM-dd）,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：审批调价明细表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批调价明细表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems'
GO

-- ApprovalFeeItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'FeeName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'OldAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'NewAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'BillingMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'Unit'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期（每条费用独立，yyyy-MM-dd）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApprovalFeeItems', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 按审批请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ApprovalFeeItems]') AND name=N'IX_ApprovalFeeItems_ApprovalRequestId')
CREATE INDEX [IX_ApprovalFeeItems_ApprovalRequestId] ON [ApprovalFeeItems]([ApprovalRequestId])


-- 17. HousingUnits 表：房间表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HousingUnits]'))
CREATE TABLE [HousingUnits] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [BuildingName] NVARCHAR(200) NOT NULL , -- 座楼名称,
    [BuildingCode] VARCHAR(50) , -- 座楼编号,
    [BuildingAddress] NVARCHAR(500) , -- 座楼地址,
    [FloorName] NVARCHAR(100) NOT NULL , -- 楼层名称,
    [FloorSortOrder] INT NOT NULL DEFAULT (0) , -- 楼层排序,
    [UnitNo] VARCHAR(20) NOT NULL , -- 房间号,
    [FullCode] VARCHAR(100) , -- 完整编码,
    [Area] DECIMAL(10,2) , -- 面积,
    [RoomTypeId] UNIQUEIDENTIFIER , -- 房型ID,
    [BaseRentAmount] DECIMAL(18,2) , -- 基础租金,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits'
GO

-- HousingUnits 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'座楼名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'BuildingName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'座楼编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'BuildingCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'座楼地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'BuildingAddress'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'FloorName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'FloorSortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UnitNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'完整编码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'FullCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'面积', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'Area'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'RoomTypeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'基础租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'BaseRentAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'朝向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'Orientation'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 完整编码唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[HousingUnits]') AND name=N'IX_HousingUnits_FullCode')
CREATE UNIQUE INDEX [IX_HousingUnits_FullCode] ON [HousingUnits]([FullCode])
-- 按状态查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[HousingUnits]') AND name=N'IX_HousingUnits_Status')
CREATE INDEX [IX_HousingUnits_Status] ON [HousingUnits]([Status])

-- ===================================================================
-- HousingUnits_Audit 表：房间表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HousingUnits_Audit]'))
CREATE TABLE [HousingUnits_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [BuildingName] NVARCHAR(200) , -- 座楼名称,
    [BuildingCode] VARCHAR(50) , -- 座楼编号,
    [BuildingAddress] NVARCHAR(500) , -- 座楼地址,
    [FloorName] NVARCHAR(100) , -- 楼层名称,
    [FloorSortOrder] INT , -- 楼层排序,
    [UnitNo] VARCHAR(20) , -- 房间号,
    [FullCode] VARCHAR(100) , -- 完整编码,
    [Area] DECIMAL(10,2) , -- 面积,
    [RoomTypeId] UNIQUEIDENTIFIER , -- 房型ID,
    [BaseRentAmount] DECIMAL(18,2) , -- 基础租金,
    [Status] VARCHAR(20) , -- 房间状态,
    [Orientation] VARCHAR(50) , -- 朝向,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- HousingUnits_Audit 表说明：房间表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit'
GO

-- HousingUnits_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'座楼名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'BuildingName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'座楼编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'BuildingCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'座楼地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'BuildingAddress'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'FloorName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'FloorSortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'UnitNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'完整编码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'FullCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'面积', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'Area'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'RoomTypeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'基础租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'BaseRentAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房间状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'朝向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'Orientation'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HousingUnits_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[HousingUnits_Audit]') AND name=N'IX_HousingUnits_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_HousingUnits_Audit_Id_Version] ON [HousingUnits_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型字典表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes'
GO

-- RoomTypes 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Category'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 房型代码唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[RoomTypes]') AND name=N'IX_RoomTypes_Code')
CREATE UNIQUE INDEX [IX_RoomTypes_Code] ON [RoomTypes]([Code])

-- ===================================================================
-- RoomTypes_Audit 表：房型字典表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomTypes_Audit]'))
CREATE TABLE [RoomTypes_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Category] VARCHAR(20) , -- 分类,
    [Code] VARCHAR(50) , -- 代码,
    [Name] NVARCHAR(100) , -- 名称,
    [Description] NVARCHAR(500) , -- 描述,
    [SortOrder] INT , -- 排序,
    [IsActive] BIT , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- RoomTypes_Audit 表说明：房型字典表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型字典表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit'
GO

-- RoomTypes_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'Category'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomTypes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[RoomTypes_Audit]') AND name=N'IX_RoomTypes_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_RoomTypes_Audit_Id_Version] ON [RoomTypes_Audit]([Id], [AuditVersionNo])


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

-- 表说明：楼层级别定义表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层级别定义表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands'
GO

-- FloorLevelBands 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'级别名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'起始楼层', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'MinLevel'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结束楼层', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'MaxLevel'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- ===================================================================
-- FloorLevelBands_Audit 表：楼层级别定义表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FloorLevelBands_Audit]'))
CREATE TABLE [FloorLevelBands_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Name] NVARCHAR(100) , -- 级别名称,
    [MinLevel] INT , -- 起始楼层,
    [MaxLevel] INT , -- 结束楼层,
    [Description] NVARCHAR(500) , -- 描述,
    [SortOrder] INT , -- 排序,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- FloorLevelBands_Audit 表说明：楼层级别定义表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层级别定义表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit'
GO

-- FloorLevelBands_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'级别名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'起始楼层', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'MinLevel'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结束楼层', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'MaxLevel'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FloorLevelBands_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[FloorLevelBands_Audit]') AND name=N'IX_FloorLevelBands_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_FloorLevelBands_Audit_Id_Version] ON [FloorLevelBands_Audit]([Id], [AuditVersionNo])


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
    [EffectiveDate] DATETIME , -- 生效日期,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'定价标准表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards'
GO

-- RoomPricingStandards 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'RoomTypeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层级别ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'FloorLevelBandId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼栋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'BuildingId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'标准租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'RentAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'Remarks'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 按房型查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[RoomPricingStandards]') AND name=N'IX_RoomPricingStandards_RoomType')
CREATE INDEX [IX_RoomPricingStandards_RoomType] ON [RoomPricingStandards]([RoomTypeId])

-- ===================================================================
-- RoomPricingStandards_Audit 表：定价标准表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RoomPricingStandards_Audit]'))
CREATE TABLE [RoomPricingStandards_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [RoomTypeId] UNIQUEIDENTIFIER , -- 房型ID,
    [FloorLevelBandId] UNIQUEIDENTIFIER , -- 楼层级别ID,
    [BuildingId] UNIQUEIDENTIFIER , -- 楼栋ID,
    [RentAmount] DECIMAL(18,2) , -- 标准租金,
    [EffectiveDate] DATETIME , -- 生效日期,
    [Remarks] NVARCHAR(500) , -- 备注,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- RoomPricingStandards_Audit 表说明：定价标准表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'定价标准表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit'
GO

-- RoomPricingStandards_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房型ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'RoomTypeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼层级别ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'FloorLevelBandId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'楼栋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'BuildingId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'标准租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'RentAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'Remarks'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RoomPricingStandards_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[RoomPricingStandards_Audit]') AND name=N'IX_RoomPricingStandards_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_RoomPricingStandards_Audit_Id_Version] ON [RoomPricingStandards_Audit]([Id], [AuditVersionNo])


-- ===================================================================

-- 23. Tenants 表：租客表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Tenants]'))
CREATE TABLE [Tenants] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Name] NVARCHAR(200) NOT NULL , -- 租客姓名,
    [IdentityType] VARCHAR(20) NOT NULL DEFAULT ('PRC_ID') , -- 证件类型,
    [IdCard] VARCHAR(50) NOT NULL , -- 证件号码,
    [Phone] VARCHAR(20) NOT NULL , -- 手机号,
    [Email] VARCHAR(200) , -- 邮箱,
    [Address] NVARCHAR(500) , -- 通讯地址,
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

-- 表说明：租客表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants'
GO

-- Tenants 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客姓名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'IdentityType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件号码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'IdCard'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'手机号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Phone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'邮箱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Email'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通讯地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Address'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'Remarks'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 证件号查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Tenants]') AND name=N'IX_Tenants_IdCard')
CREATE INDEX [IX_Tenants_IdCard] ON [Tenants]([IdCard])
-- 手机号查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Tenants]') AND name=N'IX_Tenants_Phone')
CREATE INDEX [IX_Tenants_Phone] ON [Tenants]([Phone])

-- ===================================================================
-- Tenants_Audit 表：租客表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Tenants_Audit]'))
CREATE TABLE [Tenants_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Name] NVARCHAR(200) , -- 租客姓名,
    [IdentityType] VARCHAR(20) , -- 证件类型,
    [IdCard] VARCHAR(50) , -- 证件号码,
    [Phone] VARCHAR(20) , -- 手机号,
    [Email] VARCHAR(200) , -- 邮箱,
    [Address] NVARCHAR(500) , -- 通讯地址,
    [Remarks] NVARCHAR(500) , -- 备注,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- Tenants_Audit 表说明：租客表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit'
GO

-- Tenants_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客姓名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'IdentityType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'证件号码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'IdCard'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'手机号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'Phone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'邮箱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'Email'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通讯地址', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'Address'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'Remarks'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Tenants_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Tenants_Audit]') AND name=N'IX_Tenants_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_Tenants_Audit_Id_Version] ON [Tenants_Audit]([Id], [AuditVersionNo])


-- ===================================================================
-- 23b. Tenant 表扩充字段
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Tenants') AND name='Wechat')
    ALTER TABLE Tenants ADD Wechat NVARCHAR(64) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Tenants') AND name='EmergencyContact')
    ALTER TABLE Tenants ADD EmergencyContact NVARCHAR(64) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Tenants') AND name='EmergencyPhone')
    ALTER TABLE Tenants ADD EmergencyPhone NVARCHAR(32) NULL;
GO

-- ===================================================================
-- 23c. TenantCreateRequests 表：新建租客审批暂存
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TenantCreateRequests]'))
CREATE TABLE [TenantCreateRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()),
    [Name] NVARCHAR(64) NOT NULL,
    [Phone] NVARCHAR(32) NULL,
    [IdCard] NVARCHAR(32) NULL,
    [Email] NVARCHAR(64) NULL,
    [Wechat] NVARCHAR(64) NULL,
    [EmergencyContact] NVARCHAR(64) NULL,
    [EmergencyPhone] NVARCHAR(32) NULL,
    [Address] NVARCHAR(256) NULL,
    [Remark] NVARCHAR(500) NULL,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL,
    [ContractId] UNIQUEIDENTIFIER NOT NULL,
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [Status] NVARCHAR(32) NOT NULL DEFAULT 'Draft',
    [ApprovalRequestId] UNIQUEIDENTIFIER NULL,
    [NewTenantId] UNIQUEIDENTIFIER NULL,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] DATETIME2 NULL
)
GO

-- ===================================================================
-- 24. Contracts 表：合同表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Contracts]'))
CREATE TABLE [Contracts] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractNo] VARCHAR(100) NOT NULL , -- 合同编号,
    [RoomId] UNIQUEIDENTIFIER NOT NULL , -- 房屋ID,
    [StartDate] DATETIME NOT NULL , -- 合同开始日期,
    [EndDate] DATETIME NULL , -- 合同结束日期（null 表示无固定到期日）,
    [PaymentCycle] VARCHAR(20) NOT NULL DEFAULT ('Monthly') , -- 支付周期,
    [PaymentDueDay] INT NOT NULL DEFAULT (5) , -- 每月到期日,
    [AllowDepositAsLastRent] BIT NOT NULL DEFAULT (0) , -- 押金抵扣最后租金,
    [AutoRenew] BIT NOT NULL DEFAULT (1) , -- 是否自动续签,
    [TenantPhone] NVARCHAR(32) NULL , -- 租客电话,
    [Remark] NVARCHAR(MAX) NULL , -- 备注,
    [ActualEndDate] DATETIME , -- 实际搬离日,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Draft') , -- 合同状态,
    [PreviousContractId] UNIQUEIDENTIFIER , -- 上一份合同ID,
    [RenewalCount] INT NOT NULL DEFAULT (0) , -- 续签次数,
    [OutstandingBalance] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 欠款余额（应收未收，出账+收款后实时更新）,
    [PrepaidBalance] DECIMAL(18,2) NOT NULL DEFAULT (0) , -- 预存金额（溢收未抵）,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts'
GO

-- Contracts 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'ContractNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房屋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'RoomId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同开始日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'StartDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同结束日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'EndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付周期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'PaymentCycle'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'每月到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'PaymentDueDay'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金抵扣最后租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'AllowDepositAsLastRent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动续签', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'AutoRenew'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客电话', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'TenantPhone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'Remark'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'实际搬离日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'ActualEndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上一份合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'PreviousContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签次数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'RenewalCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'预存金额（独立于日记账）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'PrepaidBalance'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原始合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'OriginalContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签市场价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'MarketPriceAtRenewal'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'终止时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'TerminatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'终止原因', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'TerminationReason'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'暂停时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'SuspendedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'恢复时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'ResumedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'乐观锁', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'RowVersion'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 合同编号唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Contracts]') AND name=N'IX_Contracts_ContractNo')
CREATE UNIQUE INDEX [IX_Contracts_ContractNo] ON [Contracts]([ContractNo])
-- 按状态查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Contracts]') AND name=N'IX_Contracts_Status')
CREATE INDEX [IX_Contracts_Status] ON [Contracts]([Status])
-- 按房屋查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Contracts]') AND name=N'IX_Contracts_RoomId')
CREATE INDEX [IX_Contracts_RoomId] ON [Contracts]([RoomId])

-- ===================================================================
-- Contracts_Audit 表：合同表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Contracts_Audit]'))
CREATE TABLE [Contracts_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [ContractNo] VARCHAR(100) , -- 合同编号,
    [RoomId] UNIQUEIDENTIFIER , -- 房屋ID,
    [StartDate] DATETIME , -- 合同开始日期,
    [EndDate] DATETIME , -- 合同结束日期,
    [PaymentCycle] VARCHAR(20) , -- 支付周期,
    [PaymentDueDay] INT , -- 每月到期日,
    [AllowDepositAsLastRent] BIT , -- 押金抵扣最后租金,
    [AutoRenew] BIT , -- 是否自动续签,
    [TenantPhone] NVARCHAR(32) , -- 租客电话,
    [Remark] NVARCHAR(MAX) , -- 备注,
    [ActualEndDate] DATETIME , -- 实际搬离日,
    [Status] VARCHAR(20) , -- 合同状态,
    [PreviousContractId] UNIQUEIDENTIFIER , -- 上一份合同ID,
    [RenewalCount] INT , -- 续签次数,
    [OriginalContractId] UNIQUEIDENTIFIER , -- 原始合同ID,
    [MarketPriceAtRenewal] DECIMAL(18,2) , -- 续签市场价,
    [TerminatedAt] DATETIME2 , -- 终止时间,
    [TerminationReason] NVARCHAR(500) , -- 终止原因,
    [SuspendedAt] DATETIME2 , -- 暂停时间,
    [ResumedAt] DATETIME2 , -- 恢复时间,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- Contracts_Audit 表说明：合同表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit'
GO

-- Contracts_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'ContractNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房屋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'RoomId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同开始日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'StartDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同结束日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'EndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付周期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'PaymentCycle'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'每月到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'PaymentDueDay'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金抵扣最后租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'AllowDepositAsLastRent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动续签', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'AutoRenew'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客电话', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'TenantPhone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'Remark'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'实际搬离日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'ActualEndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上一份合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'PreviousContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签次数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'RenewalCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原始合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'OriginalContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签市场价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'MarketPriceAtRenewal'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'终止时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'TerminatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'终止原因', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'TerminationReason'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'暂停时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'SuspendedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'恢复时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'ResumedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Contracts_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Contracts_Audit]') AND name=N'IX_Contracts_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_Contracts_Audit_Id_Version] ON [Contracts_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同租客关联表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants'
GO

-- ContractTenants 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'TenantId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否主租客', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'IsPrimary'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractTenants', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO


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
    [InitialReadingDate] DATETIME , -- 初始读数日期,
    [EffectiveDate] DATETIME NOT NULL , -- 生效日期,
    [ExpiryDate] DATETIME , -- 失效日期,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同费用配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs'
GO

-- ContractFeeConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额/单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'BillingMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'Unit'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UnitPrice'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'初始读数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'InitialReading'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'初始读数日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'InitialReadingDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'失效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'ExpiryDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'入住当月分摊', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'ProrateOnMoveIn'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'搬出当月分摊', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'ProrateOnMoveOut'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs]') AND name=N'IX_ContractFeeConfigs_Contract')
CREATE INDEX [IX_ContractFeeConfigs_Contract] ON [ContractFeeConfigs]([ContractId])

-- ===================================================================
-- ContractFeeConfigs_Audit 表：合同费用配置表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs_Audit]'))
CREATE TABLE [ContractFeeConfigs_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [ContractId] UNIQUEIDENTIFIER , -- 合同ID,
    [FeeCodeId] UNIQUEIDENTIFIER , -- 费用项目ID,
    [Amount] DECIMAL(18,4) , -- 金额/单价,
    [BillingMode] VARCHAR(20) , -- 计费方式,
    [Unit] NVARCHAR(20) , -- 计量单位,
    [UnitPrice] DECIMAL(18,4) , -- 单价,
    [InitialReading] DECIMAL(18,4) , -- 初始读数,
    [InitialReadingDate] DATETIME , -- 初始读数日期,
    [EffectiveDate] DATETIME , -- 生效日期,
    [ExpiryDate] DATETIME , -- 失效日期,
    [ProrateOnMoveIn] BIT , -- 入住当月分摊,
    [ProrateOnMoveOut] BIT , -- 搬出当月分摊,
    [IsActive] BIT , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- ContractFeeConfigs_Audit 表说明：合同费用配置表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同费用配置表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit'
GO

-- ContractFeeConfigs_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额/单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'BillingMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'Unit'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UnitPrice'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'初始读数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'InitialReading'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'初始读数日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'InitialReadingDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'失效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'ExpiryDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'入住当月分摊', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'ProrateOnMoveIn'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'搬出当月分摊', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'ProrateOnMoveOut'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractFeeConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ContractFeeConfigs_Audit]') AND name=N'IX_ContractFeeConfigs_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_ContractFeeConfigs_Audit_Id_Version] ON [ContractFeeConfigs_Audit]([Id], [AuditVersionNo])


-- ===================================================================
-- 26a. ContractCreateRequests 表：新建合同审批暂存表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]'))
CREATE TABLE [ContractCreateRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractNo] NVARCHAR(64) NOT NULL , -- 合同编号,
    [RoomId] UNIQUEIDENTIFIER NOT NULL , -- 房屋ID,
    [StartDate] DATETIME NOT NULL , -- 合同开始日期,
    [EndDate] DATETIME NULL , -- 合同结束日期,
    [PaymentCycle] NVARCHAR(32) NOT NULL DEFAULT ('Monthly') , -- 支付周期,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [Status] NVARCHAR(32) NOT NULL DEFAULT ('Draft') , -- 状态(Draft/PendingApproval/Executing/Completed/Rejected),
    [Remark] NVARCHAR(500) NULL , -- 备注,
    [ApprovalRequestId] UNIQUEIDENTIFIER NULL , -- 审批请求ID,
    [NewContractId] UNIQUEIDENTIFIER NULL , -- 新合同ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] NVARCHAR(64) NULL , -- 创建IP,
    [CreatedHostname] NVARCHAR(128) NULL , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER NULL , -- 更新人,
    [UpdatedAt] DATETIME2 NULL , -- 更新时间,
    [UpdatedIp] NVARCHAR(64) NULL , -- 更新IP,
    [UpdatedHostname] NVARCHAR(128) NULL -- 更新主机名
)
GO

-- 表说明：新建合同审批暂存表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新建合同审批暂存表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests'
GO

-- ContractCreateRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'ContractNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'房屋ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'RoomId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同开始日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'StartDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同结束日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'EndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付周期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'PaymentCycle'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'Remark'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'NewContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按审批请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]') AND name=N'IX_ContractCreateRequests_ApprovalRequestId')
CREATE INDEX [IX_ContractCreateRequests_ApprovalRequestId] ON [ContractCreateRequests]([ApprovalRequestId])
-- 按状态查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]') AND name=N'IX_ContractCreateRequests_Status')
CREATE INDEX [IX_ContractCreateRequests_Status] ON [ContractCreateRequests]([Status])


-- ===================================================================
-- 26b. ContractCreateRequestTenants 表：新建合同租客暂存表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractCreateRequestTenants]'))
CREATE TABLE [ContractCreateRequestTenants] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RequestId] UNIQUEIDENTIFIER NOT NULL , -- 请求ID,
    [TenantId] UNIQUEIDENTIFIER NOT NULL , -- 租客ID,
    [IsPrimary] BIT NOT NULL DEFAULT (0) , -- 是否主租客,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] NVARCHAR(64) NULL , -- 创建IP,
    [CreatedHostname] NVARCHAR(128) NULL , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER NULL , -- 更新人,
    [UpdatedAt] DATETIME2 NULL , -- 更新时间,
    [UpdatedIp] NVARCHAR(64) NULL , -- 更新IP,
    [UpdatedHostname] NVARCHAR(128) NULL -- 更新主机名
)
GO

-- 表说明：新建合同租客暂存表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新建合同租客暂存表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestTenants'
GO

-- ContractCreateRequestTenants 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestTenants', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestTenants', @level2type = N'COLUMN', @level2name = N'RequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestTenants', @level2type = N'COLUMN', @level2name = N'TenantId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否主租客', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestTenants', @level2type = N'COLUMN', @level2name = N'IsPrimary'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestTenants', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestTenants', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestTenants', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ContractCreateRequestTenants]') AND name=N'IX_ContractCreateRequestTenants_RequestId')
CREATE INDEX [IX_ContractCreateRequestTenants_RequestId] ON [ContractCreateRequestTenants]([RequestId])


-- ===================================================================
-- 26c. ContractCreateRequestFees 表：新建合同费用暂存表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractCreateRequestFees]'))
CREATE TABLE [ContractCreateRequestFees] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RequestId] UNIQUEIDENTIFIER NOT NULL , -- 请求ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [Amount] DECIMAL(18,2) NOT NULL , -- 金额,
    [BillingMode] NVARCHAR(32) NOT NULL DEFAULT ('FixedAmount') , -- 计费方式,
    [ChargeType] NVARCHAR(32) NOT NULL DEFAULT ('Recurring') , -- 费用类型,
    [Unit] NVARCHAR(32) NULL , -- 计量单位,
    [UnitPrice] DECIMAL(18,4) NULL , -- 单价,
    [EffectiveDate] NVARCHAR(10) NULL , -- 生效日期,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] NVARCHAR(64) NULL , -- 创建IP,
    [CreatedHostname] NVARCHAR(128) NULL , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER NULL , -- 更新人,
    [UpdatedAt] DATETIME2 NULL , -- 更新时间,
    [UpdatedIp] NVARCHAR(64) NULL , -- 更新IP,
    [UpdatedHostname] NVARCHAR(128) NULL -- 更新主机名
)
GO

-- 表说明：新建合同费用暂存表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新建合同费用暂存表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees'
GO

-- ContractCreateRequestFees 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'RequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'BillingMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'ChargeType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'Unit'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'UnitPrice'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractCreateRequestFees', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ContractCreateRequestFees]') AND name=N'IX_ContractCreateRequestFees_RequestId')
CREATE INDEX [IX_ContractCreateRequestFees_RequestId] ON [ContractCreateRequestFees]([RequestId])


-- ===================================================================
-- 26d. ContractModifyRequests 表：合同变更审批暂存表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ContractModifyRequests]'))
CREATE TABLE [ContractModifyRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [StartDate] DATETIME NULL , -- 合同开始日期,
    [EndDate] DATETIME NULL , -- 合同结束日期,
    [PaymentCycle] NVARCHAR(32) NULL , -- 支付周期,
    [AutoRenew] BIT NULL , -- 是否自动续签,
    [AllowDepositAsLastRent] BIT NULL , -- 押金抵扣最后租金,
    [PaymentDueDay] INT NULL , -- 每月到期日,
    [TenantPhone] NVARCHAR(32) NULL , -- 租客电话,
    [Remark] NVARCHAR(500) NULL , -- 备注,
    [Status] NVARCHAR(32) NOT NULL DEFAULT ('Draft') , -- 状态,
    [ApprovalRequestId] UNIQUEIDENTIFIER NULL , -- 审批请求ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedAt] DATETIME2 NULL -- 更新时间
)
GO

-- 表说明：合同变更审批暂存表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同变更审批暂存表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests'
GO

-- ContractModifyRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同开始日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'StartDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同结束日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'EndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付周期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'PaymentCycle'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动续签', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'AutoRenew'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金抵扣最后租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'AllowDepositAsLastRent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'每月到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'PaymentDueDay'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'租客电话', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'TenantPhone'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'Remark'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContractModifyRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ContractModifyRequests]') AND name=N'IX_ContractModifyRequests_ContractId')
CREATE INDEX [IX_ContractModifyRequests_ContractId] ON [ContractModifyRequests]([ContractId])
-- 按审批请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ContractModifyRequests]') AND name=N'IX_ContractModifyRequests_ApprovalRequestId')
CREATE INDEX [IX_ContractModifyRequests_ApprovalRequestId] ON [ContractModifyRequests]([ApprovalRequestId])


-- ===================================================================
-- 26e. SupplementaryFeeRequests 表：补充费用审批暂存表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[SupplementaryFeeRequests]'))
CREATE TABLE [SupplementaryFeeRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [Amount] DECIMAL(18,2) NOT NULL , -- 金额,
    [BillingMode] NVARCHAR(32) NOT NULL DEFAULT ('FixedAmount') , -- 计费方式,
    [EffectiveDate] NVARCHAR(10) NOT NULL , -- 生效日期,
    [PeriodFrom] NVARCHAR(10) NOT NULL , -- 期间开始,
    [PeriodTo] NVARCHAR(10) NOT NULL , -- 期间结束,
    [Status] NVARCHAR(32) NOT NULL DEFAULT ('Draft') , -- 状态,
    [ApprovalRequestId] UNIQUEIDENTIFIER NULL , -- 审批请求ID,
    [FeeConfigId] UNIQUEIDENTIFIER NULL , -- 费用配置ID,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedAt] DATETIME2 NULL -- 更新时间
)
GO

-- 表说明：补充费用审批暂存表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'补充费用审批暂存表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests'
GO

-- SupplementaryFeeRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'BillingMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期间开始', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'PeriodFrom'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期间结束', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'PeriodTo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用配置ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'FeeConfigId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[SupplementaryFeeRequests]') AND name=N'IX_SupplementaryFeeRequests_ContractId')
CREATE INDEX [IX_SupplementaryFeeRequests_ContractId] ON [SupplementaryFeeRequests]([ContractId])
-- 按审批请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[SupplementaryFeeRequests]') AND name=N'IX_SupplementaryFeeRequests_ApprovalRequestId')
CREATE INDEX [IX_SupplementaryFeeRequests_ApprovalRequestId] ON [SupplementaryFeeRequests]([ApprovalRequestId])


-- ===================================================================
-- 26f. SupplementaryFeeRequestItems 表：补充费用期间明细暂存表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[SupplementaryFeeRequestItems]'))
CREATE TABLE [SupplementaryFeeRequestItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RequestId] UNIQUEIDENTIFIER NOT NULL , -- 请求ID,
    [Period] NVARCHAR(10) NOT NULL , -- 期间,
    [ProratedAmount] DECIMAL(18,2) NOT NULL , -- 分摊金额,
    [DaysInMonth] INT NOT NULL , -- 当月天数,
    [CoveredDays] INT NOT NULL , -- 覆盖天数,
    [ReceivablePlanId] UNIQUEIDENTIFIER NULL , -- 应收计划ID,
    [VoucherId] UNIQUEIDENTIFIER NULL , -- 凭证ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedAt] DATETIME2 NULL -- 更新时间
)
GO

-- 表说明：补充费用期间明细暂存表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'补充费用期间明细暂存表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems'
GO

-- SupplementaryFeeRequestItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'RequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'Period'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分摊金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'ProratedAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'当月天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'DaysInMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'覆盖天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'CoveredDays'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收计划ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'ReceivablePlanId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'凭证ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'VoucherId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SupplementaryFeeRequestItems', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[SupplementaryFeeRequestItems]') AND name=N'IX_SupplementaryFeeRequestItems_RequestId')
CREATE INDEX [IX_SupplementaryFeeRequestItems_RequestId] ON [SupplementaryFeeRequestItems]([RequestId])


-- ===================================================================
-- 26g. ReceivableGenerateRequests 表：应收生成审批暂存表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivableGenerateRequests]'))
CREATE TABLE [ReceivableGenerateRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [PeriodFrom] NVARCHAR(10) NOT NULL , -- 期间开始,
    [PeriodTo] NVARCHAR(10) NOT NULL , -- 期间结束,
    [Status] NVARCHAR(32) NOT NULL DEFAULT ('Draft') , -- 状态,
    [ApprovalRequestId] UNIQUEIDENTIFIER NULL , -- 审批请求ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedAt] DATETIME2 NULL -- 更新时间
)
GO

-- 表说明：应收生成审批暂存表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收生成审批暂存表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests'
GO

-- ReceivableGenerateRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期间开始', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'PeriodFrom'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期间结束', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'PeriodTo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审批请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'ApprovalRequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ReceivableGenerateRequests]') AND name=N'IX_ReceivableGenerateRequests_ContractId')
CREATE INDEX [IX_ReceivableGenerateRequests_ContractId] ON [ReceivableGenerateRequests]([ContractId])
-- 按审批请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ReceivableGenerateRequests]') AND name=N'IX_ReceivableGenerateRequests_ApprovalRequestId')
CREATE INDEX [IX_ReceivableGenerateRequests_ApprovalRequestId] ON [ReceivableGenerateRequests]([ApprovalRequestId])


-- ===================================================================
-- 26h. ReceivableGenerateRequestItems 表：应收生成明细暂存表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivableGenerateRequestItems]'))
CREATE TABLE [ReceivableGenerateRequestItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [RequestId] UNIQUEIDENTIFIER NOT NULL , -- 请求ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID,
    [FeeName] NVARCHAR(64) NOT NULL , -- 费用名称,
    [Period] NVARCHAR(10) NOT NULL , -- 期间,
    [Amount] DECIMAL(18,2) NOT NULL , -- 金额,
    [DueDate] DATETIME NOT NULL , -- 到期日,
    [EntryType] NVARCHAR(32) NOT NULL DEFAULT ('Normal') , -- 分录类型,
    [ReceivablePlanId] UNIQUEIDENTIFIER NULL , -- 应收计划ID,
    [VoucherId] UNIQUEIDENTIFIER NULL , -- 凭证ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedAt] DATETIME2 NULL -- 更新时间
)
GO

-- 表说明：应收生成明细暂存表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收生成明细暂存表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems'
GO

-- ReceivableGenerateRequestItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'RequestId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'FeeName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'Period'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'DueDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分录类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'EntryType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收计划ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'ReceivablePlanId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'凭证ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'VoucherId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ReceivableGenerateRequestItems', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按请求查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ReceivableGenerateRequestItems]') AND name=N'IX_ReceivableGenerateRequestItems_RequestId')
CREATE INDEX [IX_ReceivableGenerateRequestItems_RequestId] ON [ReceivableGenerateRequestItems]([RequestId])


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
    [EffectiveDate] DATETIME , -- 生效日期,
    [OperatorId] UNIQUEIDENTIFIER , -- 操作人ID,
    [OperatorName] NVARCHAR(50) , -- 操作人姓名,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：合同变更历史表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同变更历史表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory'
GO

-- ChangeHistory 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变更类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'ChangeType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'标题', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'Title'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'详情', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'Detail'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'旧值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'OldValue'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'NewValue'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'OperatorId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人姓名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'OperatorName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeHistory', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ChangeHistory]') AND name=N'IX_ChangeHistory_ContractId')
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
    [NewEndDate] DATETIME NOT NULL , -- 新到期日,
    [DepositHandling] VARCHAR(20) NOT NULL , -- 押金处理,
    [OldDepositAmount] DECIMAL(18,2) NOT NULL , -- 原押金,
    [NewDepositAmount] DECIMAL(18,2) , -- 新押金,
    [MarketReferencePrice] DECIMAL(18,2) , -- 市场参考价,
    [PaymentStatusCheck] BIT NOT NULL DEFAULT (0) , -- 付款检查,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Draft') , -- 状态,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [Remark] NVARCHAR(500) , -- 备注,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 -- 更新时间
)
GO

-- 表说明：续签申请表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签申请表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests'
GO

-- RenewalRequests 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'OldContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'NewContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新合同号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'ContractNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'RenewalType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'PreviousRent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'NewRent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'NewEndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金处理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'DepositHandling'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原押金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'OldDepositAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新押金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'NewDepositAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'市场参考价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'MarketReferencePrice'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款检查', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'PaymentStatusCheck'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'Remark'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按原合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[RenewalRequests]') AND name=N'IX_RenewalRequests_OldContract')
CREATE INDEX [IX_RenewalRequests_OldContract] ON [RenewalRequests]([OldContractId])

-- ===================================================================
-- RenewalRequests_Audit 表：续签申请表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[RenewalRequests_Audit]'))
CREATE TABLE [RenewalRequests_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [OldContractId] UNIQUEIDENTIFIER , -- 原合同ID,
    [NewContractId] UNIQUEIDENTIFIER , -- 新合同ID,
    [ContractNo] NVARCHAR(100) , -- 新合同号,
    [RenewalType] VARCHAR(20) , -- 续签类型,
    [PreviousRent] DECIMAL(18,2) , -- 原租金,
    [NewRent] DECIMAL(18,2) , -- 新租金,
    [NewEndDate] DATETIME , -- 新到期日,
    [DepositHandling] VARCHAR(20) , -- 押金处理,
    [OldDepositAmount] DECIMAL(18,2) , -- 原押金,
    [NewDepositAmount] DECIMAL(18,2) , -- 新押金,
    [MarketReferencePrice] DECIMAL(18,2) , -- 市场参考价,
    [PaymentStatusCheck] BIT , -- 付款检查,
    [Status] VARCHAR(20) , -- 状态,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [Remark] NVARCHAR(500) , -- 备注,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 -- 更新时间
)
GO

-- RenewalRequests_Audit 表说明：续签申请表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签申请表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit'
GO

-- RenewalRequests_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'OldContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'NewContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新合同号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'ContractNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'续签类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'RenewalType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'PreviousRent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新租金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'NewRent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'NewEndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金处理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'DepositHandling'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原押金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'OldDepositAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'新押金', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'NewDepositAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'市场参考价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'MarketReferencePrice'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款检查', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'PaymentStatusCheck'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'Remark'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RenewalRequests_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[RenewalRequests_Audit]') AND name=N'IX_RenewalRequests_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_RenewalRequests_Audit_Id_Version] ON [RenewalRequests_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes'
GO

-- FeeCodes 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'BillingMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Unit'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'Category'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收费类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'ChargeType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否必选', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'IsRequired'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 公司内费用代码唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[FeeCodes]') AND name=N'IX_FeeCodes_CompanyId_Code')
CREATE UNIQUE INDEX [IX_FeeCodes_CompanyId_Code] ON [FeeCodes]([CompanyId],[Code])

-- ===================================================================
-- FeeCodes_Audit 表：费用项目表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodes_Audit]'))
CREATE TABLE [FeeCodes_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Code] VARCHAR(50) , -- 费用代码,
    [Name] NVARCHAR(200) , -- 费用名称,
    [BillingMode] VARCHAR(20) , -- 计费方式,
    [Unit] NVARCHAR(20) , -- 计量单位,
    [SortOrder] INT , -- 排序,
    [Category] VARCHAR(50) , -- 分类,
    [ChargeType] VARCHAR(20) , -- 收费类型,
    [IsActive] BIT , -- 是否启用,
    [IsRequired] BIT , -- 是否必选,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- FeeCodes_Audit 表说明：费用项目表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit'
GO

-- FeeCodes_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计费方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'BillingMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'计量单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'Unit'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'Category'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收费类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'ChargeType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否必选', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'IsRequired'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[FeeCodes_Audit]') AND name=N'IX_FeeCodes_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_FeeCodes_Audit_Id_Version] ON [FeeCodes_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用科目模板表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates'
GO

-- FeeCodeTemplates 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借贷方向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'Direction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'SubjectCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'SubjectName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'增值税分离', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'IsVatSeparate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 按费用项目查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates]') AND name=N'IX_FeeCodeTemplates_FeeCodeId')
CREATE INDEX [IX_FeeCodeTemplates_FeeCodeId] ON [FeeCodeTemplates]([FeeCodeId])

-- ===================================================================
-- FeeCodeTemplates_Audit 表：费用科目模板表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates_Audit]'))
CREATE TABLE [FeeCodeTemplates_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [FeeCodeId] UNIQUEIDENTIFIER , -- 费用项目ID,
    [Direction] VARCHAR(10) , -- 借贷方向,
    [SubjectCode] VARCHAR(50) , -- 科目代码,
    [SubjectName] NVARCHAR(200) , -- 科目名称,
    [IsVatSeparate] BIT , -- 增值税分离,
    [SortOrder] INT , -- 排序,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- FeeCodeTemplates_Audit 表说明：费用科目模板表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用科目模板表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit'
GO

-- FeeCodeTemplates_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借贷方向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'Direction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'SubjectCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'SubjectName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'增值税分离', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'IsVatSeparate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'FeeCodeTemplates_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[FeeCodeTemplates_Audit]') AND name=N'IX_FeeCodeTemplates_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_FeeCodeTemplates_Audit_Id_Version] ON [FeeCodeTemplates_Audit]([Id], [AuditVersionNo])

-- ===================================================================
-- TaxRateConfigs 表：税率配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaxRateConfigs]'))
CREATE TABLE [TaxRateConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Name] NVARCHAR(100) NOT NULL , -- 税率名称,
    [Rate] DECIMAL(5,2) NOT NULL , -- 税率(%),
    [EffectiveDate] DATETIME NOT NULL , -- 生效日期,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs'
GO

-- TaxRateConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率(%)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'Rate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- ===================================================================
-- TaxRateConfigs_Audit 表：税率配置表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaxRateConfigs_Audit]'))
CREATE TABLE [TaxRateConfigs_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Name] NVARCHAR(100) , -- 税率名称,
    [Rate] DECIMAL(5,2) , -- 税率(%),
    [EffectiveDate] DATETIME , -- 生效日期,
    [IsActive] BIT , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- TaxRateConfigs_Audit 表说明：税率配置表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率配置表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit'
GO

-- TaxRateConfigs_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'税率(%)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'Rate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaxRateConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[TaxRateConfigs_Audit]') AND name=N'IX_TaxRateConfigs_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_TaxRateConfigs_Audit_Id_Version] ON [TaxRateConfigs_Audit]([Id], [AuditVersionNo])


-- ===================================================================
-- 36. Journals 表：日记账表（不可变的出账记录，替代 ReceivablePlans）
-- ===================================================================
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivablePlans_Audit]')) DROP TABLE [ReceivablePlans_Audit]
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ReceivablePlans]')) DROP TABLE [ReceivablePlans]
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Journals]'))
CREATE TABLE [Journals] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [ContractId] UNIQUEIDENTIFIER NOT NULL , -- 合同ID,
    [FeeCodeId] UNIQUEIDENTIFIER NOT NULL , -- 费用项目ID（租金/物业费/利息等）,
    [FeeConfigId] UNIQUEIDENTIFIER NULL , -- 费用配置实例ID（一次性费用幂等去重用，NULL为周期性费用）,
    [AccountingSubjectId] UNIQUEIDENTIFIER NOT NULL , -- 会计科目ID（默认1122应收账款）,
    [Period] VARCHAR(7) NOT NULL , -- 归属账期(格式: yyyy-MM),
    [Amount] DECIMAL(18,2) NOT NULL , -- 金额（允许负数，用于冲销错误记录）,
    [DueDate] DATETIME NOT NULL , -- 到期日,
    [EntryType] VARCHAR(20) NOT NULL DEFAULT ('Normal') , -- 条目类型(Normal/Deposit/Supplementary/Interest/Adjustment),
    [GLPosted] BIT NOT NULL DEFAULT (0) , -- 是否已写入总账(0=未入账,1=已入账),
    [PostedAt] DATETIME2 NULL , -- 总账写入时间,
    [IsBilled] BIT NOT NULL DEFAULT (0) , -- 是否已写入DebitNote(0=未入账单,1=已入账),
    [BilledAt] DATETIME2 NOT NULL , -- 出账时间,
    [BillMonth] VARCHAR(7) NOT NULL DEFAULT (''), -- 账单月(格式: yyyy-MM),
    [DebitNoteId] UNIQUEIDENTIFIER NULL , -- 关联账单ID,
    [ParentJournalId] UNIQUEIDENTIFIER NULL , -- 关联源日记账ID（利息/调整/冲销指向被操作的源记录）,
    [Summary] NVARCHAR(500) NULL , -- 摘要说明,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) NULL , -- 创建IP,
    [CreatedHostname] VARCHAR(100) NULL , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER NULL , -- 更新人,
    [UpdatedAt] DATETIME2 NULL , -- 更新时间,
    [UpdatedIp] VARCHAR(45) NULL , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) NULL -- 更新主机名
)
GO

-- 表说明：日记账表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日记账表（不可变的出账记录）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals'
GO

-- Journals 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID（租金/物业费/利息等）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用配置实例ID（一次性费用幂等去重用，NULL为周期性费用）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'FeeConfigId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计科目ID（默认1122应收账款）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'AccountingSubjectId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'归属账期(格式: yyyy-MM)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'Period'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额（允许负数，用于冲销错误记录）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'DueDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'条目类型(Normal/Deposit/Supplementary/Interest/Adjustment)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'EntryType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已写入总账(0=未入账,1=已入账)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'GLPosted'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'总账写入时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'PostedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'出账时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'BilledAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联账单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'DebitNoteId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联源日记账ID（利息/调整/冲销指向被操作的源记录）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'ParentJournalId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'摘要说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'Summary'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 公司+账期查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Journals]') AND name=N'IX_Journals_Company_Period')
CREATE INDEX [IX_Journals_Company_Period] ON [Journals]([CompanyId], [Period] DESC)
-- 未入总账记录（结账校验用）
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Journals]') AND name=N'IX_Journals_GLPosted')
CREATE INDEX [IX_Journals_GLPosted] ON [Journals]([GLPosted]) WHERE [GLPosted] = 0
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Journals]') AND name=N'IX_Journals_ContractId')
CREATE INDEX [IX_Journals_ContractId] ON [Journals]([ContractId])
-- 一次性费用幂等去重索引（非唯一，允许多次）
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Journals]') AND name=N'IX_Journals_FeeConfigId')
CREATE INDEX [IX_Journals_FeeConfigId] ON [Journals]([FeeConfigId]) WHERE [FeeConfigId] IS NOT NULL

-- ===================================================================
-- Journals_Audit 表：日记账表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Journals_Audit]'))
CREATE TABLE [Journals_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) NULL , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [CompanyId] UNIQUEIDENTIFIER NULL , -- 所属公司ID,
    [ContractId] UNIQUEIDENTIFIER NULL , -- 合同ID,
    [FeeCodeId] UNIQUEIDENTIFIER NULL , -- 费用项目ID,
    [FeeConfigId] UNIQUEIDENTIFIER NULL , -- 费用配置实例ID,
    [AccountingSubjectId] UNIQUEIDENTIFIER NULL , -- 会计科目ID,
    [Period] VARCHAR(7) NULL , -- 归属账期,
    [Amount] DECIMAL(18,2) NULL , -- 金额,
    [DueDate] DATETIME NULL , -- 到期日,
    [EntryType] VARCHAR(20) NULL , -- 条目类型,
    [GLPosted] BIT NULL , -- 是否已写入总账,
    [PostedAt] DATETIME2 NULL , -- 总账写入时间,
    [IsBilled] BIT NULL , -- 是否已写入DebitNote,
    [BilledAt] DATETIME2 NULL , -- 出账时间,
    [DebitNoteId] UNIQUEIDENTIFIER NULL , -- 关联账单ID,
    [ParentJournalId] UNIQUEIDENTIFIER NULL , -- 关联源日记账ID,
    [Summary] NVARCHAR(500) NULL , -- 摘要说明,
    [CreatedBy] UNIQUEIDENTIFIER NULL , -- 创建人,
    [CreatedAt] DATETIME2 NULL , -- 创建时间,
    [CreatedIp] VARCHAR(45) NULL , -- 创建IP,
    [CreatedHostname] VARCHAR(100) NULL , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER NULL , -- 更新人,
    [UpdatedAt] DATETIME2 NULL , -- 更新时间,
    [UpdatedIp] VARCHAR(45) NULL , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) NULL -- 更新主机名
)
GO

-- Journals_Audit 表说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日记账表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit'
GO

-- Journals_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用配置实例ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'FeeConfigId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计科目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'AccountingSubjectId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'归属账期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'Period'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'DueDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'条目类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'EntryType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已写入总账', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'GLPosted'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'总账写入时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'PostedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'出账时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'BilledAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联账单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'DebitNoteId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联源日记账ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'ParentJournalId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'摘要说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'Summary'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Journals_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Journals_Audit]') AND name=N'IX_Journals_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_Journals_Audit_Id_Version] ON [Journals_Audit]([Id], [AuditVersionNo])


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
    [DueDate] DATETIME NOT NULL , -- 到期日,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单主表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes'
GO

-- DebitNotes 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期年份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'PeriodYear'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'PeriodMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'BillNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'DueDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收总金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'TotalAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'已收总金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'TotalReceived'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生成时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'GeneratedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生成人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'GeneratedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否历史账单', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'IsHistorical'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 账单编号唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[DebitNotes]') AND name=N'IX_DebitNotes_BillNo')
CREATE UNIQUE INDEX [IX_DebitNotes_BillNo] ON [DebitNotes]([BillNo])
-- 合同+账期唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[DebitNotes]') AND name=N'IX_DebitNotes_Contract_Period')
CREATE UNIQUE INDEX [IX_DebitNotes_Contract_Period] ON [DebitNotes]([ContractId],[PeriodYear],[PeriodMonth])

-- ===================================================================
-- DebitNotes_Audit 表：账单主表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DebitNotes_Audit]'))
CREATE TABLE [DebitNotes_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [ContractId] UNIQUEIDENTIFIER , -- 合同ID,
    [PeriodYear] INT , -- 账期年份,
    [PeriodMonth] INT , -- 账期月份,
    [BillNo] VARCHAR(50) , -- 账单编号,
    [DueDate] DATETIME , -- 到期日,
    [TotalAmount] DECIMAL(18,2) , -- 应收总金额,
    [TotalReceived] DECIMAL(18,2) , -- 已收总金额,
    [Status] VARCHAR(20) , -- 状态,
    [GeneratedAt] DATETIME2 , -- 生成时间,
    [GeneratedBy] UNIQUEIDENTIFIER , -- 生成人,
    [IsHistorical] BIT , -- 是否历史账单,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- DebitNotes_Audit 表说明：账单主表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单主表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit'
GO

-- DebitNotes_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期年份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'PeriodYear'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账期月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'PeriodMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'BillNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'到期日', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'DueDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'应收总金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'TotalAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'已收总金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'TotalReceived'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生成时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'GeneratedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生成人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'GeneratedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否历史账单', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'IsHistorical'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNotes_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[DebitNotes_Audit]') AND name=N'IX_DebitNotes_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_DebitNotes_Audit_Id_Version] ON [DebitNotes_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单明细表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems'
GO

-- DebitNoteItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'账单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'DebitNoteId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用项目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'FeeCodeId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'费用名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'FeeName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'已收金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'ReceivedAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'数量', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Quantity'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单价', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'UnitPrice'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'单位', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Unit'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DebitNoteItems', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO
-- 按账单查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[DebitNoteItems]') AND name=N'IX_DebitNoteItems_DebitNoteId')
CREATE INDEX [IX_DebitNoteItems_DebitNoteId] ON [DebitNoteItems]([DebitNoteId])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'自动续签配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs'
GO

-- AutoRenewConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动续签', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'IsAutoRenew'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'提前续签天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'RenewalDaysBeforeExpiry'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'调价百分比', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'RentAdjustmentPercent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AutoRenewConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付通道表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels'
GO

-- PaymentChannels 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'ChannelType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'AccountNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 通道代码唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[PaymentChannels]') AND name=N'IX_PaymentChannels_Code')
CREATE UNIQUE INDEX [IX_PaymentChannels_Code] ON [PaymentChannels]([Code])

-- ===================================================================
-- PaymentChannels_Audit 表：支付通道表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[PaymentChannels_Audit]'))
CREATE TABLE [PaymentChannels_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Code] VARCHAR(50) , -- 通道代码,
    [Name] NVARCHAR(200) , -- 通道名称,
    [ChannelType] VARCHAR(20) , -- 通道类型,
    [AccountNo] VARCHAR(100) , -- 收款账号,
    [IsActive] BIT , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- PaymentChannels_Audit 表说明：支付通道表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付通道表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit'
GO

-- PaymentChannels_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通道类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'ChannelType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'AccountNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PaymentChannels_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[PaymentChannels_Audit]') AND name=N'IX_PaymentChannels_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_PaymentChannels_Audit_Id_Version] ON [PaymentChannels_Audit]([Id], [AuditVersionNo])


-- ===================================================================
-- 41. Receipts 表：收据表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Receipts]'))
CREATE TABLE [Receipts] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [ReceiptNo] VARCHAR(100) NOT NULL , -- 收据编号,
    [ContractId] UNIQUEIDENTIFIER NULL , -- 关联合同ID,
    [PaymentChannelId] UNIQUEIDENTIFIER NULL , -- 支付通道ID,
    [Amount] DECIMAL(18,2) NOT NULL , -- 收款金额,
    [ReceivedDate] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 收款日期,
    [ReferenceNo] NVARCHAR(128) NULL , -- 参考号,
    [Status] VARCHAR(20) NOT NULL DEFAULT ('Pending') , -- 状态: Pending | Confirmed | Rejected | Cancelled,
    [RejectReason] NVARCHAR(256) NULL , -- 驳回原因,
    [ConfirmedAt] DATETIME2 NULL , -- 确认时间,
    [ConfirmedBy] UNIQUEIDENTIFIER NULL , -- 确认人,
    [RowVersion] TIMESTAMP NOT NULL , -- 乐观锁,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] VARCHAR(45) NULL , -- 创建IP,
    [CreatedHostname] VARCHAR(100) NULL , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER NULL , -- 更新人,
    [UpdatedAt] DATETIME2 NULL , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- 表说明：收据表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收据表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts'
GO

-- Receipts 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收据编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'ReceiptNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付通道ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'PaymentChannelId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'ReceivedDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'RemitterName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款人账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'RemitterAccount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'交易参考号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'TransactionRef'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'被退款收据ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'RefundedReceiptId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'乐观锁', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'RowVersion'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 收据编号唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Receipts]') AND name=N'IX_Receipts_ReceiptNo')
CREATE UNIQUE INDEX [IX_Receipts_ReceiptNo] ON [Receipts]([ReceiptNo])
-- 交易参考号索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Receipts]') AND name=N'IX_Receipts_TransactionRef')
CREATE INDEX [IX_Receipts_TransactionRef] ON [Receipts]([TransactionRef])

-- ===================================================================
-- Receipts_Audit 表：收据表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Receipts_Audit]'))
CREATE TABLE [Receipts_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [ReceiptNo] VARCHAR(100) , -- 收据编号,
    [PaymentChannelId] UNIQUEIDENTIFIER , -- 支付通道ID,
    [Amount] DECIMAL(18,2) , -- 收款金额,
    [ReceivedDate] DATETIME2 , -- 收款时间,
    [RemitterName] NVARCHAR(100) , -- 付款人,
    [RemitterAccount] VARCHAR(100) , -- 付款人账号,
    [TransactionRef] VARCHAR(200) , -- 交易参考号,
    [Status] VARCHAR(20) , -- 状态,
    [RefundedReceiptId] UNIQUEIDENTIFIER , -- 被退款收据ID,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- Receipts_Audit 表说明：收据表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收据表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit'
GO

-- Receipts_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收据编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'ReceiptNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'支付通道ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'PaymentChannelId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'收款时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'ReceivedDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'RemitterName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'付款人账号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'RemitterAccount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'交易参考号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'TransactionRef'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'被退款收据ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'RefundedReceiptId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Receipts_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Receipts_Audit]') AND name=N'IX_Receipts_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_Receipts_Audit_Id_Version] ON [Receipts_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金记录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs'
GO

-- DepositLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'ActionType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变动金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作后余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'BalanceAfter'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联收据ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'RelatedReceiptId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'Remarks'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'OperatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'OperatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[DepositLogs]') AND name=N'IX_DepositLogs_ContractId')
CREATE INDEX [IX_DepositLogs_ContractId] ON [DepositLogs]([ContractId])

-- ===================================================================
-- DepositLogs_Audit 表：押金记录表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[DepositLogs_Audit]'))
CREATE TABLE [DepositLogs_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [ContractId] UNIQUEIDENTIFIER , -- 合同ID,
    [ActionType] VARCHAR(20) , -- 操作类型,
    [Amount] DECIMAL(18,2) , -- 变动金额,
    [BalanceAfter] DECIMAL(18,2) , -- 操作后余额,
    [RelatedReceiptId] UNIQUEIDENTIFIER , -- 关联收据ID,
    [Remarks] NVARCHAR(500) , -- 备注,
    [OperatedBy] UNIQUEIDENTIFIER , -- 操作人,
    [OperatedAt] DATETIME2 , -- 操作时间,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- DepositLogs_Audit 表说明：押金记录表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'押金记录表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit'
GO

-- DepositLogs_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'ActionType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'变动金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作后余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'BalanceAfter'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联收据ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'RelatedReceiptId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'Remarks'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'OperatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'OperatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DepositLogs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[DepositLogs_Audit]') AND name=N'IX_DepositLogs_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_DepositLogs_Audit_Id_Version] ON [DepositLogs_Audit]([Id], [AuditVersionNo])

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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'催缴阶段配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages'
GO

-- CollectionStages 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'阶段编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'StageNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'阶段名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'StageName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'逾期起始天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'OverdueDaysFrom'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'逾期结束天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'OverdueDaysTo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'催缴动作', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'ActionType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动执行', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'IsAuto'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- ===================================================================
-- CollectionStages_Audit 表：催缴阶段配置表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[CollectionStages_Audit]'))
CREATE TABLE [CollectionStages_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [StageNo] INT , -- 阶段编号,
    [StageName] NVARCHAR(100) , -- 阶段名称,
    [OverdueDaysFrom] INT , -- 逾期起始天数,
    [OverdueDaysTo] INT , -- 逾期结束天数,
    [ActionType] VARCHAR(20) , -- 催缴动作,
    [IsAuto] BIT , -- 是否自动执行,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- CollectionStages_Audit 表说明：催缴阶段配置表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'催缴阶段配置表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit'
GO

-- CollectionStages_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'阶段编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'StageNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'阶段名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'StageName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'逾期起始天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'OverdueDaysFrom'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'逾期结束天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'OverdueDaysTo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'催缴动作', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'ActionType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自动执行', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'IsAuto'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionStages_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[CollectionStages_Audit]') AND name=N'IX_CollectionStages_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_CollectionStages_Audit_Id_Version] ON [CollectionStages_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'催缴记录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords'
GO

-- CollectionRecords 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'阶段编号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'StageNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'发送渠道', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'Channel'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'发送内容', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'Content'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'发送时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'SentAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'OperatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CollectionRecords', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 按合同查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[CollectionRecords]') AND name=N'IX_CollectionRecords_ContractId')
CREATE INDEX [IX_CollectionRecords_ContractId] ON [CollectionRecords]([ContractId])

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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计科目表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects'
GO

-- AccountingSubjects 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'父科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'ParentCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目层级', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Level'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借贷方向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'Direction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否末级科目', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'IsLeaf'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 科目代码唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[AccountingSubjects]') AND name=N'IX_AccountingSubjects_Code')
CREATE UNIQUE INDEX [IX_AccountingSubjects_Code] ON [AccountingSubjects]([Code])

-- ===================================================================
-- AccountingSubjects_Audit 表：会计科目表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[AccountingSubjects_Audit]'))
CREATE TABLE [AccountingSubjects_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Code] VARCHAR(50) , -- 科目代码,
    [Name] NVARCHAR(200) , -- 科目名称,
    [ParentCode] VARCHAR(50) , -- 父科目代码,
    [Level] INT , -- 科目层级,
    [Direction] VARCHAR(10) , -- 借贷方向,
    [IsLeaf] BIT , -- 是否末级科目,
    [IsActive] BIT , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- AccountingSubjects_Audit 表说明：会计科目表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计科目表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit'
GO

-- AccountingSubjects_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'父科目代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'ParentCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目层级', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'Level'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借贷方向', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'Direction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否末级科目', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'IsLeaf'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AccountingSubjects_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[AccountingSubjects_Audit]') AND name=N'IX_AccountingSubjects_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_AccountingSubjects_Audit_Id_Version] ON [AccountingSubjects_Audit]([Id], [AuditVersionNo])


-- 清理旧会计表（Vouchers + 旧 JournalEntries 已废弃）
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JournalEntries_Audit]')) DROP TABLE [JournalEntries_Audit]
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JournalEntries]')) DROP TABLE [JournalEntries]
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Vouchers_Audit]')) DROP TABLE [Vouchers_Audit]
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[Vouchers]')) DROP TABLE [Vouchers]

-- ===================================================================

-- ===================================================================
-- 48. GeneralLedgerEntries 表：总账分录表
-- 记录每笔业务事件的科目级借贷变动
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[GeneralLedgerEntries]'))
CREATE TABLE [GeneralLedgerEntries] (
    [Id]          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [CompanyId]   UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [ContractId]  UNIQUEIDENTIFIER NULL     , -- 关联合同ID,
    [ContractNo]  NVARCHAR(64)     NULL     , -- 合同号（冗余）,
    [Period]      NVARCHAR(10)     NOT NULL , -- 会计期间 yyyy-MM,
    [SubjectId]   UNIQUEIDENTIFIER NOT NULL , -- 会计科目ID,
    [SubjectCode] NVARCHAR(32)     NOT NULL , -- 科目编码,
    [Direction]   NVARCHAR(8)      NOT NULL , -- Debit | Credit,
    [Amount]      DECIMAL(18,2)    NOT NULL , -- 金额,
    [SourceType]  NVARCHAR(32)     NOT NULL , -- BillJob | JournalPost | Receipt | SettleOffset,
    [SourceId]    UNIQUEIDENTIFIER NULL     , -- 来源单据ID,
    [Description] NVARCHAR(128)    NULL     , -- 摘要,
    [CreatedBy]   UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt]   DATETIME2        NOT NULL DEFAULT (DATEADD(HOUR, 8, GETUTCDATE())) , -- 创建时间（东八区）,
    [UpdatedAt]   DATETIME2        NULL      -- 更新时间
)
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'总账分录表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries'
GO

-- GeneralLedgerEntries 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同号（冗余，便于查询）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'ContractNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计期间 yyyy-MM', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'Period'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计科目ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'SubjectId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'科目编码（冗余，便于查询）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'SubjectCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'借贷方向: Debit | Credit', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'Direction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'来源类型: BillJob | JournalPost | Receipt | SettleOffset', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'SourceType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'来源单据ID（JournalId / ReceiptId）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'SourceId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'摘要', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间（东八区）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'GeneralLedgerEntries', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
GO

-- 索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[GeneralLedgerEntries]') AND name=N'IX_GLE_Company_Period')
CREATE INDEX [IX_GLE_Company_Period] ON [GeneralLedgerEntries]([CompanyId], [Period] DESC)
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[GeneralLedgerEntries]') AND name=N'IX_GLE_Source')
CREATE INDEX [IX_GLE_Source] ON [GeneralLedgerEntries]([SourceType], [SourceId])
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[GeneralLedgerEntries]') AND name=N'IX_GLE_Contract')
CREATE INDEX [IX_GLE_Contract] ON [GeneralLedgerEntries]([ContractId])
GO

-- ===================================================================-- 50. BankReconciliations 表：银行余额调节表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankReconciliations]'))
CREATE TABLE [BankReconciliations] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [StartDate] DATETIME NOT NULL , -- 开始日期,
    [EndDate] DATETIME NOT NULL , -- 结束日期,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行余额调节表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations'
GO

-- BankReconciliations 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开始日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'StartDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'结束日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'EndDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期初余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'OpeningBalance'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'期末余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'ClosingBalance'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行总额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'StatementTotal'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'系统总额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'SystemTotal'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'完成时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'CompletedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankReconciliations', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO

-- 51. BankStatements 表：银行流水表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankStatements]'))
CREATE TABLE [BankStatements] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [TransactionDate] DATETIME NOT NULL , -- 交易日期,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行流水表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements'
GO

-- BankStatements 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'交易日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'TransactionDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Balance'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'参考号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'ReferenceNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'对方账户', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Counterparty'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入批次ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'ImportBatchId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO

-- ===================================================================
-- BankStatements_Audit 表：银行流水表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[BankStatements_Audit]'))
CREATE TABLE [BankStatements_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [TransactionDate] DATETIME , -- 交易日期,
    [Amount] DECIMAL(18,2) , -- 金额,
    [Balance] DECIMAL(18,2) , -- 余额,
    [Description] NVARCHAR(MAX) , -- 描述,
    [ReferenceNo] NVARCHAR(100) , -- 参考号,
    [Counterparty] NVARCHAR(200) , -- 对方账户,
    [Status] VARCHAR(20) , -- 状态,
    [ImportBatchId] UNIQUEIDENTIFIER , -- 导入批次ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 -- 创建时间
)
GO

-- BankStatements_Audit 表说明：银行流水表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'银行流水表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit'
GO

-- BankStatements_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'交易日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'TransactionDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'Amount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'余额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'Balance'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'参考号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'ReferenceNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'对方账户', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'Counterparty'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入批次ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'ImportBatchId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'BankStatements_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[BankStatements_Audit]') AND name=N'IX_BankStatements_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_BankStatements_Audit_Id_Version] ON [BankStatements_Audit]([Id], [AuditVersionNo])

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
    [ScheduleType] VARCHAR(20) NOT NULL DEFAULT ('Daily') , -- 调度类型,
    [Hour] INT NOT NULL DEFAULT (0) , -- 小时,
    [Minute] INT NOT NULL DEFAULT (0) , -- 分钟,
    [DayOfMonth] INT , -- 月中的天,
    [TemplateCode] NVARCHAR(50) , -- 模板代码,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [Description] NVARCHAR(500) , -- 描述,
    [LastRunAt] DATETIME2 , -- 上次执行时间,
    [LastRunStatus] NVARCHAR(20) , -- 上次执行结果,
    [TargetDate] DATETIME2 , -- 最近一次执行成功的时间（来自 JobScheduleExecutions.TargetDate）,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务实例表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules'
GO

-- JobSchedules 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'JobName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'调度类型（Daily/Monthly）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'ScheduleType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'小时', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'Hour'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分钟', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'Minute'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'月中的天（ScheduleType=Monthly时有效）', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'DayOfMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'模板代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'TemplateCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上次执行时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'LastRunAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上次执行结果', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'LastRunStatus'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- ===================================================================
-- JobSchedules_Audit 表：任务实例表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobSchedules_Audit]'))
CREATE TABLE [JobSchedules_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [JobName] NVARCHAR(200) , -- 任务名称,
    [ScheduleType] VARCHAR(20) , -- 调度类型,
    [Hour] INT , -- 小时,
    [Minute] INT , -- 分钟,
    [DayOfMonth] INT , -- 月中的天,
    [TemplateCode] NVARCHAR(50) , -- 模板代码,
    [IsActive] BIT , -- 是否启用,
    [Description] NVARCHAR(500) , -- 描述,
    [LastRunAt] DATETIME2 , -- 上次执行时间,
    [LastRunStatus] NVARCHAR(20) , -- 上次执行结果,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- JobSchedules_Audit 表说明：任务实例表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务实例表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit'
GO

-- JobSchedules_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'JobName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'调度类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'ScheduleType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'小时', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'Hour'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分钟', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'Minute'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'月中的天', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'DayOfMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'模板代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'TemplateCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'描述', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上次执行时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'LastRunAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上次执行结果', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'LastRunStatus'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobSchedules_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[JobSchedules_Audit]') AND name=N'IX_JobSchedules_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_JobSchedules_Audit_Id_Version] ON [JobSchedules_Audit]([Id], [AuditVersionNo])


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
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) , -- 创建时间,
    [CreatedIp] NVARCHAR(50) , -- 创建IP,
    [CreatedHostname] NVARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] NVARCHAR(50) , -- 更新IP,
    [UpdatedHostname] NVARCHAR(100) -- 更新主机名
)
GO

-- 表说明：执行排期表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'执行排期表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions'
GO

-- JobScheduleExecutions 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务定义ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'JobScheduleId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排期执行时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'TargetDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原始Cron时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'OriginalDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'Month'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'Reason'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否手动调整', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'IsAdjusted'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否自定义', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'IsCustom'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobScheduleExecutions', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 按任务查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[JobScheduleExecutions]') AND name=N'IX_Executions_JobScheduleId')
CREATE INDEX [IX_Executions_JobScheduleId] ON [JobScheduleExecutions]([JobScheduleId])
-- 按执行时间排序
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[JobScheduleExecutions]') AND name=N'IX_Executions_TargetDate')
CREATE INDEX [IX_Executions_TargetDate] ON [JobScheduleExecutions]([TargetDate])

-- ===================================================================
-- 53a. ExecutionHeartbeats 表：排期心跳日志表（独立于 TaskLog，仅用于进程探活）
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ExecutionHeartbeats]'))
CREATE TABLE [ExecutionHeartbeats] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY ,
    [ExecutionId] UNIQUEIDENTIFIER NOT NULL , -- 排期ID,
    [JobScheduleId] UNIQUEIDENTIFIER NOT NULL , -- 任务定义ID,
    [JobName] NVARCHAR(200) NOT NULL , -- 任务名称,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 公司ID,
    [TargetMonth] NVARCHAR(7) NOT NULL , -- 目标月份,
    [HeartbeatAt] DATETIME2 NOT NULL , -- 心跳时间,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 记录时间
)
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排期心跳日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExecutionHeartbeats'
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排期ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExecutionHeartbeats', @level2type = N'COLUMN', @level2name = N'ExecutionId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务定义ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExecutionHeartbeats', @level2type = N'COLUMN', @level2name = N'JobScheduleId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExecutionHeartbeats', @level2type = N'COLUMN', @level2name = N'JobName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExecutionHeartbeats', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'目标月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExecutionHeartbeats', @level2type = N'COLUMN', @level2name = N'TargetMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'心跳时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExecutionHeartbeats', @level2type = N'COLUMN', @level2name = N'HeartbeatAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'记录时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ExecutionHeartbeats', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO

-- 按排期+时间查询心跳
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ExecutionHeartbeats]') AND name=N'IX_ExecutionHeartbeats_ExecutionId')
CREATE INDEX [IX_ExecutionHeartbeats_ExecutionId] ON [ExecutionHeartbeats]([ExecutionId], [HeartbeatAt] DESC)

-- 54. JobTemplates 表：任务模板表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobTemplates]'))
CREATE TABLE [JobTemplates] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Code] NVARCHAR(50) NOT NULL , -- 模板代码,
    [DisplayName] NVARCHAR(100) NOT NULL , -- 显示名,
    [ShortName] NVARCHAR(50) NOT NULL , -- 短名,
    [Description] NVARCHAR(500) , -- 说明,
    [Icon] NVARCHAR(50) , -- 图标,
    [Category] NVARCHAR(50) NOT NULL , -- 分类,
    [SortOrder] INT NOT NULL DEFAULT (0) , -- 排序,
    [IsActive] BIT NOT NULL DEFAULT (1) , -- 是否启用,
    [DefaultScheduleType] VARCHAR(20) , -- 默认调度类型,
    [DefaultHour] INT , -- 默认小时,
    [DefaultMinute] INT , -- 默认分钟,
    [DefaultDayOfMonth] INT , -- 默认月中的天,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：任务模板表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务模板表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates'
GO

-- JobTemplates 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'模板代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'显示名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'DisplayName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'短名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'ShortName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'图标', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Icon'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'Category'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认调度类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'DefaultScheduleType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认小时', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'DefaultHour'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认分钟', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'DefaultMinute'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认月中的天', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'DefaultDayOfMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 模板代码唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[JobTemplates]') AND name=N'IX_JobTemplates_Code')
CREATE UNIQUE INDEX [IX_JobTemplates_Code] ON [JobTemplates]([Code])

-- ===================================================================
-- JobTemplates_Audit 表：任务模板表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[JobTemplates_Audit]'))
CREATE TABLE [JobTemplates_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [Code] NVARCHAR(50) , -- 模板代码,
    [DisplayName] NVARCHAR(100) , -- 显示名,
    [ShortName] NVARCHAR(50) , -- 短名,
    [Description] NVARCHAR(500) , -- 说明,
    [Icon] NVARCHAR(50) , -- 图标,
    [Category] NVARCHAR(50) , -- 分类,
    [SortOrder] INT , -- 排序,
    [IsActive] BIT , -- 是否启用,
    [DefaultScheduleType] VARCHAR(20) , -- 默认调度类型,
    [DefaultHour] INT , -- 默认小时,
    [DefaultMinute] INT , -- 默认分钟,
    [DefaultDayOfMonth] INT , -- 默认月中的天,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 -- 创建时间
)
GO

-- JobTemplates_Audit 表说明：任务模板表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务模板表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit'
GO

-- JobTemplates_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'模板代码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'Code'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'显示名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'DisplayName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'短名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'ShortName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'说明', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'Description'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'图标', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'Icon'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'Category'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认调度类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'DefaultScheduleType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认小时', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'DefaultHour'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认分钟', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'DefaultMinute'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'默认月中的天', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'DefaultDayOfMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'JobTemplates_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[JobTemplates_Audit]') AND name=N'IX_JobTemplates_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_JobTemplates_Audit_Id_Version] ON [JobTemplates_Audit]([Id], [AuditVersionNo])

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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'API请求日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs'
GO

-- ApiLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'API路径', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'ApiPath'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'HTTP方法', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'HttpMethod'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'StatusCode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求体', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'RequestBody'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'响应体', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'ResponseBody'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求头', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'RequestHeaders'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'客户端IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'ClientIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'UserId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户显示名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'UserDisplayName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'耗时ms', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'DurationMs'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'查询参数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'QueryString'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户代理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'UserAgent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'RequestAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'响应时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ApiLogs', @level2type = N'COLUMN', @level2name = N'ResponseAt'
GO

-- ===================================================================
-- ApiLogs 索引：提升列表查询性能
-- ===================================================================
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ApiLogs]'))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiLogs_RequestAt' AND object_id = OBJECT_ID('ApiLogs'))
        CREATE INDEX [IX_ApiLogs_RequestAt] ON [ApiLogs] ([RequestAt] DESC);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiLogs_Method_StatusCode' AND object_id = OBJECT_ID('ApiLogs'))
        CREATE INDEX [IX_ApiLogs_Method_StatusCode] ON [ApiLogs] ([HttpMethod], [StatusCode])
        INCLUDE ([ApiPath], [DurationMs], [ClientIp], [UserId], [RequestAt]);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiLogs_UserId' AND object_id = OBJECT_ID('ApiLogs'))
        CREATE INDEX [IX_ApiLogs_UserId] ON [ApiLogs] ([UserId])
        INCLUDE ([RequestAt]);
END
GO

-- ===================================================================
-- 56. TaskLogs 表：任务执行日志表
-- ===================================================================
-- 56. TaskLogs 表：任务执行日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaskLogs]'))
CREATE TABLE [TaskLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [TaskName] NVARCHAR(200) NOT NULL , -- 任务名称,
    [CompanyId] UNIQUEIDENTIFIER NOT NULL , -- 所属公司ID,
    [ContractId] UNIQUEIDENTIFIER , -- 合同ID,
    [TargetMonth] NVARCHAR(7) , -- 目标月份,
    [TriggerType] NVARCHAR(50) NOT NULL DEFAULT ('Scheduled') , -- 触发方式,
    [RunMode] NVARCHAR(20) NOT NULL DEFAULT ('Execute') , -- 运行模式,
    [Status] NVARCHAR(20) NOT NULL DEFAULT ('Running') , -- 状态,
    [StartedAt] DATETIME2 NOT NULL , -- 开始时间,
    [CompletedAt] DATETIME2 , -- 完成时间,
    [TotalDurationMs] INT , -- 总耗时(毫秒),
    [TotalCount] INT , -- 总处理数,
    [SuccessCount] INT , -- 成功数,
    [FailCount] INT , -- 失败数,
    [WarningCount] INT , -- 告警数,
    [Summary] NVARCHAR(1000) , -- 摘要,
    [ErrorMessage] NVARCHAR(2000) , -- 错误信息,
    [HeartbeatAt] DATETIME2 , -- 心跳时间,
    [Params] NVARCHAR(MAX) , -- 执行参数,
    [ResultData] NVARCHAR(MAX) , -- 执行结果数据,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL , -- 创建人,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：任务执行日志表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务执行日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs'
GO

-- TaskLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'TaskName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'合同ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'ContractId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'目标月份', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'TargetMonth'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'触发方式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'TriggerType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'运行模式', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'RunMode'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开始时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'StartedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'完成时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'CompletedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'总耗时(毫秒)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'TotalDurationMs'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'总处理数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'TotalCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'成功数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'SuccessCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'失败数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'FailCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'告警数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'WarningCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'摘要', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'Summary'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'错误信息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'ErrorMessage'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'心跳时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'HeartbeatAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'执行参数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'Params'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'执行结果数据', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'ResultData'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskLogs', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 按开始时间降序查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[TaskLogs]') AND name=N'IX_TaskLogs_StartedAt')
CREATE INDEX [IX_TaskLogs_StartedAt] ON [TaskLogs]([StartedAt] DESC)

-- ===================================================================
-- 57a. TaskStepLogs 表：任务步骤执行日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[TaskStepLogs]'))
CREATE TABLE [TaskStepLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY ,
    [TaskLogId] UNIQUEIDENTIFIER NOT NULL , -- 归属任务ID,
    [StepName] NVARCHAR(50) NOT NULL , -- 步骤名称,
    [StepDisplayName] NVARCHAR(100) , -- 步骤显示名,
    [ParentId] UNIQUEIDENTIFIER , -- 父步骤ID,
    [SortOrder] INT NOT NULL DEFAULT (0) , -- 排序,
    [Status] NVARCHAR(20) NOT NULL DEFAULT ('Running') , -- 状态,
    [StartedAt] DATETIME2 NOT NULL , -- 开始时间,
    [CompletedAt] DATETIME2 , -- 完成时间,
    [DurationMs] INT , -- 耗时(毫秒),
    [AffectedCount] INT , -- 影响数,
    [Message] NVARCHAR(500) , -- 消息,
    [ErrorMessage] NVARCHAR(2000) -- 错误信息
)
GO

-- 表说明：任务步骤执行日志表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'任务步骤执行日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs'
GO

-- TaskStepLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'归属任务ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'TaskLogId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'步骤名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'StepName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'步骤显示名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'StepDisplayName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'父步骤ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'ParentId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'SortOrder'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'开始时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'StartedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'完成时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'CompletedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'耗时(毫秒)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'DurationMs'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'影响数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'AffectedCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'消息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'Message'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'错误信息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'TaskStepLogs', @level2type = N'COLUMN', @level2name = N'ErrorMessage'
GO

-- 按 TaskLogId 查询步骤
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[TaskStepLogs]') AND name=N'IX_TaskStepLogs_TaskLogId')
CREATE INDEX [IX_TaskStepLogs_TaskLogId] ON [TaskStepLogs]([TaskLogId])

-- ===================================================================
-- 57. SystemLogs 表：系统日志表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[SystemLogs]'))
CREATE TABLE [SystemLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [Level] VARCHAR(20) , -- 日志级别,
    [Source] NVARCHAR(200) , -- 日志来源,
    [Message] NVARCHAR(MAX) , -- 消息,
    [Exception] NVARCHAR(MAX) , -- 异常堆栈,
    [Path] NVARCHAR(500) , -- 请求路径,
    [Method] VARCHAR(20) , -- 请求方法,
    [IpAddress] VARCHAR(45) , -- 客户端IP,
    [UserAgent] NVARCHAR(500) , -- 用户代理,
    [UserId] UNIQUEIDENTIFIER , -- 操作用户ID,
    [UserDisplayName] NVARCHAR(100) , -- 操作用户名,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()) -- 创建时间
)
GO

-- 表说明：系统日志表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'系统日志表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs'
GO

-- SystemLogs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日志级别', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Level'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日志来源', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Source'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'消息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Message'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'异常堆栈', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Exception'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求路径', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Path'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'请求方法', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'Method'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'客户端IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'IpAddress'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户代理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'UserAgent'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'UserId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作用户名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'UserDisplayName'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'SystemLogs', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 按创建时间降序索引
CREATE INDEX [IX_SystemLogs_CreatedAt] ON [SystemLogs]([CreatedAt] DESC)

-- ===================================================================
-- 58. HolidayCalendars 表：节假日配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HolidayCalendars]'))
CREATE TABLE [HolidayCalendars] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [HolidayDate] DATETIME NOT NULL , -- 日期,
    [Name] NVARCHAR(100) , -- 节假日名称,
    [IsWorkingDay] BIT NOT NULL DEFAULT (0) , -- false=放假, true=调休上班,
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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'节假日配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars'
GO

-- HolidayCalendars 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'HolidayDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'节假日名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否调休上班', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'IsWorkingDay'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO
-- 同年同日期唯一（按公司隔离）
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[HolidayCalendars]') AND name=N'IX_HolidayCalendars_Date_Company')
CREATE UNIQUE INDEX [IX_HolidayCalendars_Date_Company] ON [HolidayCalendars]([HolidayDate],[CompanyId])

-- ===================================================================
-- HolidayCalendars_Audit 表：节假日配置表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[HolidayCalendars_Audit]'))
CREATE TABLE [HolidayCalendars_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [HolidayDate] DATETIME , -- 日期,
    [Name] NVARCHAR(100) , -- 节假日名称,
    [IsWorkingDay] BIT , -- 是否调休上班,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- HolidayCalendars_Audit 表说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'节假日配置表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit'
GO

-- HolidayCalendars_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'HolidayDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'节假日名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'Name'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否调休上班', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'IsWorkingDay'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'HolidayCalendars_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[HolidayCalendars_Audit]') AND name=N'IX_HolidayCalendars_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_HolidayCalendars_Audit_Id_Version] ON [HolidayCalendars_Audit]([Id], [AuditVersionNo])


-- ===================================================================
-- 59. InterestConfigs 表：利息配置表
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[InterestConfigs]'))
CREATE TABLE [InterestConfigs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWSEQUENTIALID()) , -- 主键,
    [DailyRate] DECIMAL(5,4) NOT NULL DEFAULT (0) , -- 日利率,
    [GracePeriodDays] INT NOT NULL DEFAULT (3) , -- 宽限期天数,
    [MaxPercentOfPrincipal] DECIMAL(5,2) , -- 上限百分比,
    [MinInterestAmount] DECIMAL(18,2) , -- 最低金额,
    [EffectiveDate] DATETIME , -- 生效日期,
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

-- 表说明：利息配置表
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'利息配置表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs'
GO

-- InterestConfigs 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日利率', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'DailyRate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'宽限期天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'GracePeriodDays'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上限百分比', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'MaxPercentOfPrincipal'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'最低金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'MinInterestAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- ===================================================================
-- InterestConfigs_Audit 表：利息配置表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[InterestConfigs_Audit]'))
CREATE TABLE [InterestConfigs_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [DailyRate] DECIMAL(5,4) , -- 日利率,
    [GracePeriodDays] INT , -- 宽限期天数,
    [MaxPercentOfPrincipal] DECIMAL(5,2) , -- 上限百分比,
    [MinInterestAmount] DECIMAL(18,2) , -- 最低金额,
    [EffectiveDate] DATETIME , -- 生效日期,
    [IsActive] BIT , -- 是否启用,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 , -- 创建时间,
    [CreatedIp] VARCHAR(45) , -- 创建IP,
    [CreatedHostname] VARCHAR(100) , -- 创建主机名,
    [UpdatedBy] UNIQUEIDENTIFIER , -- 更新人,
    [UpdatedAt] DATETIME2 , -- 更新时间,
    [UpdatedIp] VARCHAR(45) , -- 更新IP,
    [UpdatedHostname] VARCHAR(100) -- 更新主机名
)
GO

-- InterestConfigs_Audit 表说明：利息配置表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'利息配置表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit'
GO

-- InterestConfigs_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'日利率', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'DailyRate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'宽限期天数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'GracePeriodDays'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'上限百分比', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'MaxPercentOfPrincipal'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'最低金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'MinInterestAmount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'生效日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'EffectiveDate'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否启用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'IsActive'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'CreatedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedIp'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'更新主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'InterestConfigs_Audit', @level2type = N'COLUMN', @level2name = N'UpdatedHostname'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[InterestConfigs_Audit]') AND name=N'IX_InterestConfigs_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_InterestConfigs_Audit_Id_Version] ON [InterestConfigs_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'站内通知表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications'
GO

-- Notifications 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'用户ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'UserId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'通知分类', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'Category'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'标题', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'Title'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'内容', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'Content'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'ReferenceType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'关联ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'ReferenceId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'是否已读', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'IsRead'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Notifications', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 按用户查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Notifications]') AND name=N'IX_Notifications_UserId')
CREATE INDEX [IX_Notifications_UserId] ON [Notifications]([UserId])
-- 按用户+分类查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Notifications]') AND name=N'IX_Notifications_UserId_Category')
CREATE INDEX [IX_Notifications_UserId_Category] ON [Notifications]([UserId],[Category])
-- 未读通知查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[Notifications]') AND name=N'IX_Notifications_Unread')
CREATE INDEX [IX_Notifications_Unread] ON [Notifications]([UserId],[IsRead]) WHERE [IsRead]=0

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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入批次表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches'
GO

-- ImportBatches 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'批次号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'BatchNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'ImportType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'总数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'TotalCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'成功数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'SuccessCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'失败数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'FailCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'错误信息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'ErrorMessage'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 批次号唯一
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ImportBatches]') AND name=N'IX_ImportBatches_BatchNo')
CREATE UNIQUE INDEX [IX_ImportBatches_BatchNo] ON [ImportBatches]([BatchNo])

-- ===================================================================
-- ImportBatches_Audit 表：导入批次表审计
-- ===================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'[ImportBatches_Audit]'))
CREATE TABLE [ImportBatches_Audit] (
    [AuditId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY , -- 审计自增主键,
    [AuditAction] VARCHAR(20) NOT NULL , -- 审计操作类型,
    [AuditVersionNo] INT NOT NULL , -- 版本号,
    [AuditChangedAt] DATETIME2 NOT NULL , -- 操作时间,
    [AuditChangedBy] UNIQUEIDENTIFIER NOT NULL , -- 操作人,
    [AuditChangedHostname] VARCHAR(100) , -- 操作人主机名,
    [Id] UNIQUEIDENTIFIER NOT NULL , -- 主键,
    [BatchNo] VARCHAR(50) , -- 批次号,
    [ImportType] VARCHAR(50) , -- 导入类型,
    [TotalCount] INT , -- 总数,
    [SuccessCount] INT , -- 成功数,
    [FailCount] INT , -- 失败数,
    [Status] VARCHAR(20) , -- 状态,
    [ErrorMessage] NVARCHAR(MAX) , -- 错误信息,
    [CompanyId] UNIQUEIDENTIFIER , -- 所属公司ID,
    [CreatedBy] UNIQUEIDENTIFIER , -- 创建人,
    [CreatedAt] DATETIME2 -- 创建时间
)
GO

-- ImportBatches_Audit 表说明：导入批次表审计
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入批次表审计', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit'
GO

-- ImportBatches_Audit 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计自增主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'AuditId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'审计操作类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'AuditAction'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'版本号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'AuditVersionNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedAt'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'操作人主机名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'AuditChangedHostname'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'批次号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'BatchNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入类型', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'ImportType'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'总数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'TotalCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'成功数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'SuccessCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'失败数', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'FailCount'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'错误信息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'ErrorMessage'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'所属公司ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'CompanyId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatches_Audit', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO

-- 按记录ID+版本号唯一索引
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ImportBatches_Audit]') AND name=N'IX_ImportBatches_Audit_Id_Version')
CREATE UNIQUE INDEX [IX_ImportBatches_Audit_Id_Version] ON [ImportBatches_Audit]([Id], [AuditVersionNo])


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
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'导入明细表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems'
GO

-- ImportBatchItems 字段说明
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'主键', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'Id'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'批次ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'BatchId'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'行号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'RowNo'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'原始数据', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'RawData'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'状态', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'Status'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'错误信息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'ErrorMessage'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'CreatedBy'
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ImportBatchItems', @level2type = N'COLUMN', @level2name = N'CreatedAt'
GO
-- 按批次查询
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[ImportBatchItems]') AND name=N'IX_ImportBatchItems_BatchId')
CREATE INDEX [IX_ImportBatchItems_BatchId] ON [ImportBatchItems]([BatchId])

-- =================================================================
-- v2026.07: Vouchers 表增加 Period 列（日记账方案新增）
-- =================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Vouchers]') AND name=N'Period')
ALTER TABLE [Vouchers] ADD [Period] VARCHAR(7) NULL
GO
EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'会计期间(yyyy-MM)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vouchers', @level2type = N'COLUMN', @level2name = N'Period'

-- =================================================================
-- v2026.07.13: ContractCreateRequests.EndDate 改为可选
-- =================================================================
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]') AND name=N'EndDate' AND is_nullable=0)
ALTER TABLE [ContractCreateRequests] ALTER COLUMN [EndDate] DATETIME NULL
GO

-- =================================================================
-- v2026.07.13: Contracts.EndDate 改为可选（null 表示无固定到期日）
-- =================================================================
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[Contracts]') AND name=N'EndDate' AND is_nullable=0)
ALTER TABLE [Contracts] ALTER COLUMN [EndDate] DATETIME NULL
GO

-- =================================================================
-- v2026.07.13: ContractCreateRequests 补充审计字段
-- =================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]') AND name=N'CreatedIp')
ALTER TABLE [ContractCreateRequests] ADD [CreatedIp] NVARCHAR(64) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]') AND name=N'CreatedHostname')
ALTER TABLE [ContractCreateRequests] ADD [CreatedHostname] NVARCHAR(128) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]') AND name=N'UpdatedBy')
ALTER TABLE [ContractCreateRequests] ADD [UpdatedBy] UNIQUEIDENTIFIER NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]') AND name=N'UpdatedIp')
ALTER TABLE [ContractCreateRequests] ADD [UpdatedIp] NVARCHAR(64) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequests]') AND name=N'UpdatedHostname')
ALTER TABLE [ContractCreateRequests] ADD [UpdatedHostname] NVARCHAR(128) NULL
GO

-- =================================================================
-- v2026.07.13: ContractCreateRequestTenants 补充审计字段
-- =================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestTenants]') AND name=N'CreatedIp')
ALTER TABLE [ContractCreateRequestTenants] ADD [CreatedIp] NVARCHAR(64) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestTenants]') AND name=N'CreatedHostname')
ALTER TABLE [ContractCreateRequestTenants] ADD [CreatedHostname] NVARCHAR(128) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestTenants]') AND name=N'UpdatedBy')
ALTER TABLE [ContractCreateRequestTenants] ADD [UpdatedBy] UNIQUEIDENTIFIER NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestTenants]') AND name=N'UpdatedIp')
ALTER TABLE [ContractCreateRequestTenants] ADD [UpdatedIp] NVARCHAR(64) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestTenants]') AND name=N'UpdatedHostname')
ALTER TABLE [ContractCreateRequestTenants] ADD [UpdatedHostname] NVARCHAR(128) NULL
GO

-- =================================================================
-- v2026.07.13: ContractCreateRequestFees 补充审计字段
-- =================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestFees]') AND name=N'CreatedIp')
ALTER TABLE [ContractCreateRequestFees] ADD [CreatedIp] NVARCHAR(64) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestFees]') AND name=N'CreatedHostname')
ALTER TABLE [ContractCreateRequestFees] ADD [CreatedHostname] NVARCHAR(128) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestFees]') AND name=N'UpdatedBy')
ALTER TABLE [ContractCreateRequestFees] ADD [UpdatedBy] UNIQUEIDENTIFIER NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestFees]') AND name=N'UpdatedIp')
ALTER TABLE [ContractCreateRequestFees] ADD [UpdatedIp] NVARCHAR(64) NULL
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'[ContractCreateRequestFees]') AND name=N'UpdatedHostname')
ALTER TABLE [ContractCreateRequestFees] ADD [UpdatedHostname] NVARCHAR(128) NULL
GO

-- =================================================================
-- v2026.07.16: RECEIVABLE_GENERATE 审批类型 + 三级审批配置
-- =================================================================
DECLARE @CompanyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Companies)
DECLARE @SysUserId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'
DECLARE @Now DATETIME2 = GETUTCDATE()

-- 审批类型
IF NOT EXISTS (SELECT 1 FROM ApprovalTypes WHERE Code = 'RECEIVABLE_GENERATE')
BEGIN
  INSERT INTO ApprovalTypes (Id, Code, Name, Description, RoutingStrategy, IsActive, CompanyId, CreatedBy, CreatedAt)
  VALUES (NEWID(), 'RECEIVABLE_GENERATE', '应收生成', '手动触发生成应收，按金额路由审批级别', 'Fixed', 1, @CompanyId, @SysUserId, @Now)
END
GO

-- 三级审批级别配置
DECLARE @TypeId UNIQUEIDENTIFIER = (SELECT Id FROM ApprovalTypes WHERE Code = 'RECEIVABLE_GENERATE')
DECLARE @CompanyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Companies)
DECLARE @SysUserId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'
DECLARE @Now DATETIME2 = GETUTCDATE()

-- Level 1: 运营主管（≤10万）
IF NOT EXISTS (SELECT 1 FROM ApprovalLevelConfigs WHERE ApprovalTypeId = @TypeId AND LevelNo = 1)
BEGIN
  INSERT INTO ApprovalLevelConfigs (Id, ApprovalTypeId, LevelNo, ApproverRoleId, RoleId, Level, ApprovalMode,
    MinAmount, MaxAmount, IsCumulativeCheck, CumulativeWindowDays, CompanyId, CreatedBy, CreatedAt)
  SELECT NEWID(), @TypeId, 1, r.Id, r.Id, 1, 'AnyOne', 0, 100000, 0, 0, @CompanyId, @SysUserId, @Now
  FROM Roles r WHERE r.Code = 'OpsSupervisor'
END

-- Level 2: 部门经理（10万~50万）
IF NOT EXISTS (SELECT 1 FROM ApprovalLevelConfigs WHERE ApprovalTypeId = @TypeId AND LevelNo = 2)
BEGIN
  INSERT INTO ApprovalLevelConfigs (Id, ApprovalTypeId, LevelNo, ApproverRoleId, RoleId, Level, ApprovalMode,
    MinAmount, MaxAmount, IsCumulativeCheck, CumulativeWindowDays, CompanyId, CreatedBy, CreatedAt)
  SELECT NEWID(), @TypeId, 2, r.Id, r.Id, 2, 'AnyOne', 100000.01, 500000, 0, 0, @CompanyId, @SysUserId, @Now
  FROM Roles r WHERE r.Code = 'DeptManager'
END

-- Level 3: 总经理（>50万）
IF NOT EXISTS (SELECT 1 FROM ApprovalLevelConfigs WHERE ApprovalTypeId = @TypeId AND LevelNo = 3)
BEGIN
  INSERT INTO ApprovalLevelConfigs (Id, ApprovalTypeId, LevelNo, ApproverRoleId, RoleId, Level, ApprovalMode,
    MinAmount, MaxAmount, IsCumulativeCheck, CumulativeWindowDays, CompanyId, CreatedBy, CreatedAt)
  SELECT NEWID(), @TypeId, 3, r.Id, r.Id, 3, 'AnyOne', 500000.01, 999999999, 0, 0, @CompanyId, @SysUserId, @Now
  FROM Roles r WHERE r.Code = 'GeneralManager'
END
GO

-- ===================================================================
-- 会计科目种子数据（幂等插入）
-- ===================================================================
DECLARE @AcctCompanyId uniqueidentifier = (SELECT TOP 1 Id FROM Companies ORDER BY CreatedAt);
DECLARE @AcctUserId uniqueidentifier = (SELECT TOP 1 Id FROM Users WHERE Username = 'admin');
IF @AcctUserId IS NULL SELECT TOP 1 @AcctUserId = Id FROM Users ORDER BY CreatedAt;
DECLARE @AcctNow datetime2 = DATEADD(HOUR, 8, GETUTCDATE());

-- 资产类（1xxx）借方科目
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '1001')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'1001',N'库存现金',NULL,1,'Debit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '1002')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'1002',N'银行存款',NULL,1,'Debit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '1122')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'1122',N'应收账款',NULL,1,'Debit',0,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '112201')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'112201',N'应收房租','1122',2,'Debit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '112202')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'112202',N'应收押金','1122',2,'Debit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '1131')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'1131',N'其他应收款',NULL,1,'Debit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);

-- 负债类（2xxx）贷方科目
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '2202')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'2202',N'应付款',NULL,1,'Credit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '2203')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'2203',N'预收账款',NULL,1,'Credit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '2221')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'2221',N'应交税费',NULL,1,'Credit',0,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '222101')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'222101',N'应交增值税','2221',2,'Credit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '2241')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'2241',N'其他应付款',NULL,1,'Credit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);

-- 损益类（6xxx）收入贷方/成本借方
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '6001')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'6001',N'主营业务收入',NULL,1,'Credit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '6051')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'6051',N'其他业务收入',NULL,1,'Credit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '6401')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'6401',N'主营业务成本',NULL,1,'Debit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '6601')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'6601',N'管理费用',NULL,1,'Debit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);
IF NOT EXISTS (SELECT 1 FROM AccountingSubjects WHERE Code = '6602')
    INSERT INTO AccountingSubjects (Id,Code,Name,ParentCode,Level,Direction,IsLeaf,IsActive,CompanyId,CreatedBy,CreatedAt)
    VALUES (NEWID(),'6602',N'财务费用',NULL,1,'Debit',1,1,@AcctCompanyId,@AcctUserId,@AcctNow);

PRINT N'会计科目种子数据已初始化。';
GO

