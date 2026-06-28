-- ===================================================================
-- 数据库数据更新：将"房东"相关数据更新为"公司"
-- 适用于已有数据的数据库（开发/测试/生产环境）
-- ===================================================================

PRINT N'===== 开始更新数据库数据：房东 → 公司 =====';

-- ==================== 1. Menus 表菜单名称 ====================
PRINT N'1. 更新 Menus 表菜单数据...';

-- 14. 多房东总览 → 多公司总览
UPDATE [Menus] SET
    [Name] = N'多公司总览',
    [PermissionCode] = 'companyoverview:view',
    [Path] = '/reports/companyoverview'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111014'
  AND [Name] LIKE N'%房东%';

-- 16.4 房东管理 → 公司管理
UPDATE [Menus] SET
    [Name] = N'公司管理',
    [PermissionCode] = 'system:company',
    [Path] = '/system/companies'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111169'
  AND [Name] LIKE N'%房东%';

-- 新增房东 → 新增公司
UPDATE [Menus] SET
    [Name] = N'新增公司',
    [PermissionCode] = 'system:companycreate'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111170'
  AND [Name] LIKE N'%房东%';

-- 编辑房东 → 编辑公司
UPDATE [Menus] SET
    [Name] = N'编辑公司',
    [PermissionCode] = 'system:companyedit'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111171'
  AND [Name] LIKE N'%房东%';

-- 创建房东账号 → 创建公司账号
UPDATE [Menus] SET
    [Name] = N'创建公司账号',
    [PermissionCode] = 'system:companycreateuser'
WHERE [Id] = 'A1111111-1111-1111-1111-111111111172'
  AND [Name] LIKE N'%房东%';

PRINT N'   Menus 表更新完成。';

-- ==================== 2. Users 表显示名称 ====================
PRINT N'2. 更新 Users 表显示名称...';

UPDATE [Users] SET
    [DisplayName] = N'张建国（公司）'
WHERE [Username] = 'company_a'
  AND [DisplayName] LIKE N'%房东%';

-- 如有其他包含"房东"的用户显示名称，一并更新
UPDATE [Users] SET
    [DisplayName] = REPLACE([DisplayName], N'（房东）', N'（公司）')
WHERE [DisplayName] LIKE N'%（房东）%';

PRINT N'   Users 表更新完成。';

-- ==================== 3. 验证结果 ====================
PRINT N'';
PRINT N'===== 验证更新结果 =====';

-- 检查 Menus 表中是否还有"房东"
DECLARE @MenuCount int;
SELECT @MenuCount = COUNT(*) FROM [Menus] WHERE [Name] LIKE N'%房东%';
IF @MenuCount > 0
    PRINT N'⚠ Menus 表中仍有 ' + CAST(@MenuCount AS nvarchar) + N' 条含"房东"的记录';
ELSE
    PRINT N'✅ Menus 表已无"房东"残留';

-- 检查 Users 表中是否还有"（房东）"
DECLARE @UserCount int;
SELECT @UserCount = COUNT(*) FROM [Users] WHERE [DisplayName] LIKE N'%（房东）%';
IF @UserCount > 0
    PRINT N'⚠ Users 表中仍有 ' + CAST(@UserCount AS nvarchar) + N' 条含"（房东）"的记录';
ELSE
    PRINT N'✅ Users 表已无"（房东）"残留';

PRINT N'';
PRINT N'===== 更新完成 =====';
GO
