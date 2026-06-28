-- ===================================================================
-- 修复菜单层级 + 替换全局按钮为页面级按钮权限
-- ===================================================================

DECLARE @SystemViewId uniqueidentifier = 'A1111111-1111-1111-1111-111111111016';

-- 1. 将页面菜单直接挂到系统设置下（移除中间节点）
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 1  WHERE [PermissionCode] = 'system:user';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 2  WHERE [PermissionCode] = 'system:role';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 3  WHERE [PermissionCode] = 'system:userscope';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 4  WHERE [PermissionCode] = 'system:landlord';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 5  WHERE [PermissionCode] = 'system:menu';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 6  WHERE [PermissionCode] = 'system:approvaltype';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 7  WHERE [PermissionCode] = 'system:approvallevel';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 8  WHERE [PermissionCode] = 'system:feecode';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 9  WHERE [PermissionCode] = 'system:roomtype';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 10 WHERE [PermissionCode] = 'system:pricing';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 11 WHERE [PermissionCode] = 'system:paymentchannel';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 12 WHERE [PermissionCode] = 'system:taxrate';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 13 WHERE [PermissionCode] = 'system:accountingsubject';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 14 WHERE [PermissionCode] = 'system:scheduler';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 15 WHERE [PermissionCode] = 'system:holiday';
UPDATE [Menus] SET [ParentId] = @SystemViewId, [SortOrder] = 16 WHERE [PermissionCode] = 'system:latefee';

-- 2. 删除中间节点及全局按钮
DELETE FROM [RoleMenus] WHERE [MenuId] IN (
    SELECT [Id] FROM [Menus] WHERE [PermissionCode] IN (
        'system:org', 'system:approvalconfig', 'system:basicconfig',
        'system:configcreate', 'system:configdelete'
    )
);
DELETE FROM [Menus] WHERE [PermissionCode] IN (
    'system:org', 'system:approvalconfig', 'system:basicconfig',
    'system:configcreate', 'system:configdelete'
);

-- 3. 插入页面级按钮权限
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

-- 房型管理
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:roomtypecreate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111201', N'新增房型', 'system:roomtypecreate', 'A1111111-1111-1111-1111-111111111183', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:roomtypeedit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111202', N'编辑房型', 'system:roomtypeedit', 'A1111111-1111-1111-1111-111111111183', 11, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:roomtypedelete')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111203', N'删除房型', 'system:roomtypedelete', 'A1111111-1111-1111-1111-111111111183', 12, 1, @SysUserId, GETDATE());

-- 定价标准管理
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:pricingcreate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111204', N'新增定价', 'system:pricingcreate', 'A1111111-1111-1111-1111-111111111184', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:pricingedit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111205', N'编辑定价', 'system:pricingedit', 'A1111111-1111-1111-1111-111111111184', 11, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:pricingdelete')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111206', N'删除定价', 'system:pricingdelete', 'A1111111-1111-1111-1111-111111111184', 12, 1, @SysUserId, GETDATE());

-- 支付通道管理
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:paymentchannelcreate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111207', N'新增通道', 'system:paymentchannelcreate', 'A1111111-1111-1111-1111-111111111185', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:paymentchanneledit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111208', N'编辑通道', 'system:paymentchanneledit', 'A1111111-1111-1111-1111-111111111185', 11, 1, @SysUserId, GETDATE());

-- 税率配置
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:taxratecreate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111209', N'新增税率', 'system:taxratecreate', 'A1111111-1111-1111-1111-111111111186', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:taxrateedit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111210', N'编辑税率', 'system:taxrateedit', 'A1111111-1111-1111-1111-111111111186', 11, 1, @SysUserId, GETDATE());

-- 会计科目管理
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:accountingsubjectcreate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111211', N'新增科目', 'system:accountingsubjectcreate', 'A1111111-1111-1111-1111-111111111187', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:accountingsubjectedit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111212', N'编辑科目', 'system:accountingsubjectedit', 'A1111111-1111-1111-1111-111111111187', 11, 1, @SysUserId, GETDATE());

-- 节假日管理
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:holidaycreate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111213', N'新增节假日', 'system:holidaycreate', 'A1111111-1111-1111-1111-111111111188', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:holidayimport')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111214', N'导入节假日', 'system:holidayimport', 'A1111111-1111-1111-1111-111111111188', 11, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:holidayedit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111215', N'编辑节假日', 'system:holidayedit', 'A1111111-1111-1111-1111-111111111188', 12, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:holidaydelete')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111216', N'删除节假日', 'system:holidaydelete', 'A1111111-1111-1111-1111-111111111188', 13, 1, @SysUserId, GETDATE());

-- 调度任务管理
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:schedulerconfig')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111217', N'调度配置', 'system:schedulerconfig', 'A1111111-1111-1111-1111-111111111190', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:scheduleredit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111218', N'修改排期', 'system:scheduleredit', 'A1111111-1111-1111-1111-111111111190', 11, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:schedulergenerate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111219', N'批量生成排期', 'system:schedulergenerate', 'A1111111-1111-1111-1111-111111111190', 12, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:scheduleradd')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111220', N'添加自定义排期', 'system:scheduleradd', 'A1111111-1111-1111-1111-111111111190', 13, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:schedulerviewlog')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111221', N'查看日志', 'system:schedulerviewlog', 'A1111111-1111-1111-1111-111111111190', 14, 1, @SysUserId, GETDATE());

-- 滞纳金配置
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:latefeesave')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111301', N'保存配置', 'system:latefeesave', 'A1111111-1111-1111-1111-111111111189', 10, 1, @SysUserId, GETDATE());

-- 用户数据权限
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:userscopeconfig')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111302', N'配置权限', 'system:userscopeconfig', 'A1111111-1111-1111-1111-111111111164', 10, 1, @SysUserId, GETDATE());

-- 审批类型配置
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:approvaltypecreate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111303', N'新增审批类型', 'system:approvaltypecreate', 'A1111111-1111-1111-1111-111111111178', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:approvaltypeedit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111304', N'编辑审批类型', 'system:approvaltypeedit', 'A1111111-1111-1111-1111-111111111178', 11, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:approvaltypelevel')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111305', N'级别配置', 'system:approvaltypelevel', 'A1111111-1111-1111-1111-111111111178', 12, 1, @SysUserId, GETDATE());

-- 审批级别配置
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:approvallevelcreate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111306', N'新增级别', 'system:approvallevelcreate', 'A1111111-1111-1111-1111-111111111179', 10, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:approvalleveledit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111307', N'编辑级别', 'system:approvalleveledit', 'A1111111-1111-1111-1111-111111111179', 11, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:approvalleveldelete')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111308', N'删除级别', 'system:approvalleveldelete', 'A1111111-1111-1111-1111-111111111179', 12, 1, @SysUserId, GETDATE());

-- 收费项目管理（补充缺失按钮）
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:feecodeedit')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111309', N'编辑费用', 'system:feecodeedit', 'A1111111-1111-1111-1111-111111111180', 11, 1, @SysUserId, GETDATE());
IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [PermissionCode] = 'system:feecodetemplate')
INSERT INTO [Menus] ([Id], [Name], [PermissionCode], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111310', N'科目模板配置', 'system:feecodetemplate', 'A1111111-1111-1111-1111-111111111180', 12, 1, @SysUserId, GETDATE());

PRINT N'菜单层级修复 + 按钮权限替换完成!';
GO
