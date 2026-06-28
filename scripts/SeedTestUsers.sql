-- ===================================================================
-- 用户管理测试数据
-- 密码以明文存储（AuthController 当前使用明文比较）
-- ===================================================================

DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

-- 先清除旧用户数据（谨慎：仅开发环境可执行）
-- DELETE FROM [UserRoles];
-- DELETE FROM [Users];

-- ==================== 管理员账号 ====================
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [Users] ([Id], [Username], [PasswordHash], [DisplayName], [Phone], [Email], [IsActive], [IsSuperAdmin], [CreatedBy], [CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111001', 'admin', '123456', N'系统管理员', '13800138000', 'admin@rental.com', 1, 1, @SysUserId, @Now);
    PRINT N'admin 用户已创建';
END
ELSE
    PRINT N'admin 用户已存在';

-- ==================== 运营人员 ====================
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'zhangsan')
BEGIN
    INSERT INTO [Users] ([Id], [Username], [PasswordHash], [DisplayName], [Phone], [Email], [IsActive], [IsSuperAdmin], [HomeLandlordId], [CreatedBy], [CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111002', 'zhangsan', '123456', N'张三（运营主管）', '13800138001', 'zhangsan@rental.com', 1, 0, 'A1111111-1111-1111-1111-111111111169', @SysUserId, @Now);
    PRINT N'zhangsan 用户已创建';
END

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'lisi')
BEGIN
    INSERT INTO [Users] ([Id], [Username], [PasswordHash], [DisplayName], [Phone], [Email], [IsActive], [IsSuperAdmin], [HomeLandlordId], [CreatedBy], [CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111003', 'lisi', '123456', N'李四（运营人员）', '13800138002', 'lisi@rental.com', 1, 0, 'A1111111-1111-1111-1111-111111111002', @SysUserId, @Now);
    PRINT N'lisi 用户已创建';
END

-- ==================== 财务人员 ====================
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'wangwu')
BEGIN
    INSERT INTO [Users] ([Id], [Username], [PasswordHash], [DisplayName], [Phone], [Email], [IsActive], [IsSuperAdmin], [HomeLandlordId], [CreatedBy], [CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111004', 'wangwu', '123456', N'王五（财务主管）', '13800138003', 'wangwu@rental.com', 1, 0, 'A1111111-1111-1111-1111-111111111002', @SysUserId, @Now);
    PRINT N'wangwu 用户已创建';
END

-- ==================== 房东账号 ====================
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'landlord_a')
BEGIN
    INSERT INTO [Users] ([Id], [Username], [PasswordHash], [DisplayName], [Phone], [Email], [IsActive], [IsSuperAdmin], [HomeLandlordId], [CreatedBy], [CreatedAt])
    VALUES ('C1111111-1111-1111-1111-111111111005', 'landlord_a', '123456', N'张建国（房东）', '13912345678', 'landlord_a@rental.com', 1, 0, 'A1111111-1111-1111-1111-111111111001', @SysUserId, @Now);
    PRINT N'landlord_a 用户已创建';
END

-- ==================== 分配角色 ====================
-- 获取角色 ID
DECLARE @RoleAdminId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'Admin');
DECLARE @RoleOpsSupervisorId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'OpsSupervisor');
DECLARE @RoleOperatorId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'Operator');
DECLARE @RoleFinanceSupervisorId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'FinanceSupervisor');
DECLARE @RoleLandlordId uniqueidentifier = (SELECT [Id] FROM [Roles] WHERE [Code] = 'Landlord');

-- admin → Admin 角色
DECLARE @AdminUserId uniqueidentifier = (SELECT [Id] FROM [Users] WHERE [Username] = 'admin');
IF @AdminUserId IS NOT NULL AND @RoleAdminId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = @AdminUserId AND [RoleId] = @RoleAdminId)
BEGIN
    INSERT INTO [UserRoles] ([Id], [UserId], [RoleId], [CreatedBy], [CreatedAt])
    VALUES (NEWID(), @AdminUserId, @RoleAdminId, @SysUserId, @Now);
END

-- zhangsan → OpsSupervisor
DECLARE @ZhangSanId uniqueidentifier = (SELECT [Id] FROM [Users] WHERE [Username] = 'zhangsan');
IF @ZhangSanId IS NOT NULL AND @RoleOpsSupervisorId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = @ZhangSanId AND [RoleId] = @RoleOpsSupervisorId)
BEGIN
    INSERT INTO [UserRoles] ([Id], [UserId], [RoleId], [CreatedBy], [CreatedAt])
    VALUES (NEWID(), @ZhangSanId, @RoleOpsSupervisorId, @SysUserId, @Now);
END

-- lisi → Operator
DECLARE @LiSiId uniqueidentifier = (SELECT [Id] FROM [Users] WHERE [Username] = 'lisi');
IF @LiSiId IS NOT NULL AND @RoleOperatorId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = @LiSiId AND [RoleId] = @RoleOperatorId)
BEGIN
    INSERT INTO [UserRoles] ([Id], [UserId], [RoleId], [CreatedBy], [CreatedAt])
    VALUES (NEWID(), @LiSiId, @RoleOperatorId, @SysUserId, @Now);
END

-- wangwu → FinanceSupervisor
DECLARE @WangWuId uniqueidentifier = (SELECT [Id] FROM [Users] WHERE [Username] = 'wangwu');
IF @WangWuId IS NOT NULL AND @RoleFinanceSupervisorId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = @WangWuId AND [RoleId] = @RoleFinanceSupervisorId)
BEGIN
    INSERT INTO [UserRoles] ([Id], [UserId], [RoleId], [CreatedBy], [CreatedAt])
    VALUES (NEWID(), @WangWuId, @RoleFinanceSupervisorId, @SysUserId, @Now);
END

-- landlord_a → Landlord
DECLARE @LandlordAId uniqueidentifier = (SELECT [Id] FROM [Users] WHERE [Username] = 'landlord_a');
IF @LandlordAId IS NOT NULL AND @RoleLandlordId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = @LandlordAId AND [RoleId] = @RoleLandlordId)
BEGIN
    INSERT INTO [UserRoles] ([Id], [UserId], [RoleId], [CreatedBy], [CreatedAt])
    VALUES (NEWID(), @LandlordAId, @RoleLandlordId, @SysUserId, @Now);
END

PRINT N'用户角色分配完成！';
GO
