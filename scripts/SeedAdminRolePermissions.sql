-- ===================================================================
-- 创建 Admin 角色并分配所有菜单权限
-- ===================================================================

DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @AdminRoleId uniqueidentifier = 'B1111111-1111-1111-1111-111111111001';

-- 1. 创建 Admin 角色（如不存在则插入）
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Admin')
BEGIN
    INSERT INTO [Roles] ([Id], [Name], [Code], [Description], [IsActive], [CreatedBy], [CreatedAt])
    VALUES (@AdminRoleId, N'系统管理员', 'Admin', N'系统配置、用户管理、审批流程', 1, @SysUserId, @Now);
    PRINT N'Admin 角色已创建';
END
ELSE
BEGIN
    PRINT N'Admin 角色已存在，跳过创建';
    SELECT @AdminRoleId = [Id] FROM [Roles] WHERE [Code] = 'Admin';
END

-- 2. 分配所有已启用菜单给 Admin 角色（跳过已存在的关联）
INSERT INTO [RoleMenus] ([Id], [RoleId], [MenuId], [CreatedBy], [CreatedAt])
SELECT NEWID(), @AdminRoleId, M.[Id], @SysUserId, @Now
FROM [Menus] M
WHERE M.[IsActive] = 1
  AND NOT EXISTS (SELECT 1 FROM [RoleMenus] RM WHERE RM.[RoleId] = @AdminRoleId AND RM.[MenuId] = M.[Id]);

DECLARE @Count int = (SELECT COUNT(*) FROM [RoleMenus] WHERE [RoleId] = @AdminRoleId);
PRINT N'Admin 角色当前共有 ' + CAST(@Count AS nvarchar) + N' 个菜单权限。';
GO
