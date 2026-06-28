-- ===================================================================
-- 修复菜单 ParentId：按钮权限应挂在对应的功能菜单下，而非系统设置
-- ===================================================================

UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111162' WHERE [PermissionCode] = 'system:usercreate';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111162' WHERE [PermissionCode] = 'system:useredit';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111163' WHERE [PermissionCode] = 'system:rolecreate';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111163' WHERE [PermissionCode] = 'system:roleassignmenu';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111169' WHERE [PermissionCode] = 'system:landlordcreate';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111169' WHERE [PermissionCode] = 'system:landlordedit';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111169' WHERE [PermissionCode] = 'system:landlordcreateuser';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111173' WHERE [PermissionCode] = 'system:menucreate';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111173' WHERE [PermissionCode] = 'system:menuedit';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111173' WHERE [PermissionCode] = 'system:menudelete';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111180' WHERE [PermissionCode] = 'system:feecodecreate';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111182' WHERE [PermissionCode] = 'system:configcreate';
UPDATE [Menus] SET [ParentId] = 'A1111111-1111-1111-1111-111111111182' WHERE [PermissionCode] = 'system:configdelete';

PRINT N'菜单 ParentId 修复完成';
GO
