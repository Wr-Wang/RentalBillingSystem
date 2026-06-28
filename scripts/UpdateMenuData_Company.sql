-- ===================================================================
-- 将 Menus 表中"房东"相关菜单名称更新为"公司"
-- 对应 SeedMenus_AllInOne.sql 中的菜单种子数据修改
-- 注意：权限编码(PermissionCode)和路径(Path)也已一并更新
-- ===================================================================

PRINT N'开始更新菜单数据：房东 → 公司';

-- ==================== 14. 多公司总览 ====================
UPDATE [Menus] SET
    [Name] = N'多公司总览',
    [PermissionCode] = 'companyoverview:view',
    [Path] = '/reports/companyoverview'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111014'
  AND [Name] LIKE N'%房东%';

-- ==================== 16.4 公司管理及其子菜单 ====================
UPDATE [Menus] SET
    [Name] = N'公司管理',
    [PermissionCode] = 'system:company',
    [Path] = '/system/companies'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111169'
  AND [Name] LIKE N'%房东%';

UPDATE [Menus] SET
    [Name] = N'新增公司',
    [PermissionCode] = 'system:companycreate'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111170'
  AND [Name] LIKE N'%房东%';

UPDATE [Menus] SET
    [Name] = N'编辑公司',
    [PermissionCode] = 'system:companyedit'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111171'
  AND [Name] LIKE N'%房东%';

UPDATE [Menus] SET
    [Name] = N'创建公司账号',
    [PermissionCode] = 'system:companycreateuser'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111172'
  AND [Name] LIKE N'%房东%';

PRINT N'菜单数据更新完成！';

-- 验证更新结果
SELECT [Id], [Name], [PermissionCode], [Path] FROM [Menus]
WHERE [Id] IN (
    'A1111111-1111-1111-1111-111111111014',
    'A1111111-1111-1111-1111-111111111169',
    'A1111111-1111-1111-1111-111111111170',
    'A1111111-1111-1111-1111-111111111171',
    'A1111111-1111-1111-1111-111111111172'
);
GO
