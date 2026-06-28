-- ===================================================================
-- 菜单权限种子数据（无连字符版）
-- 使用 MERGE 实现幂等插入/更新
-- ===================================================================

-- ==================== 辅助存储过程 ====================
-- 使用 MERGE 避免重复
DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';

-- ==================== 1. 仪表盘 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111001' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET
    [Name] = N'仪表盘', [PermissionCode] = 'dashboard:view', [Path] = '/dashboard', [Icon] = 'DataAnalysis', [ParentId] = NULL, [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111001', N'仪表盘', 'dashboard:view', '/dashboard', 'DataAnalysis', NULL, 1, 1, @SysUserId, @Now);

-- ==================== 2. 房屋管理 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111002' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'房屋管理', [PermissionCode] = 'building:view', [Path] = '/buildings', [Icon] = 'HomeFilled', [ParentId] = NULL, [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111002', N'房屋管理', 'building:view', '/buildings', 'HomeFilled', NULL, 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111021' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'房间列表', [PermissionCode] = 'building:list', [Path] = '/buildings', [ParentId] = 'A1111111-1111-1111-1111-111111111002', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111021', N'房间列表', 'building:list', '/buildings', '', 'A1111111-1111-1111-1111-111111111002', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111022' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'房间详情', [PermissionCode] = 'building:detail', [Path] = '/buildings/room/:id', [ParentId] = 'A1111111-1111-1111-1111-111111111002', [SortOrder] = 2, [IsActive] = 0, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111022', N'房间详情', 'building:detail', '/buildings/room/:id', '', 'A1111111-1111-1111-1111-111111111002', 2, 0, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111023' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'批量导入', [PermissionCode] = 'building:import', [Path] = '/buildings/import', [ParentId] = 'A1111111-1111-1111-1111-111111111002', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111023', N'批量导入', 'building:import', '/buildings/import', '', 'A1111111-1111-1111-1111-111111111002', 3, 1, @SysUserId, @Now);

-- 按钮权限
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111024' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新增楼宇', [PermissionCode] = 'building:create', [ParentId] = 'A1111111-1111-1111-1111-111111111002', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111024', N'新增楼宇', 'building:create', NULL, NULL, 'A1111111-1111-1111-1111-111111111002', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111025' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'编辑楼宇', [PermissionCode] = 'building:edit', [ParentId] = 'A1111111-1111-1111-1111-111111111002', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111025', N'编辑楼宇', 'building:edit', NULL, NULL, 'A1111111-1111-1111-1111-111111111002', 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111026' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'删除楼宇', [PermissionCode] = 'building:delete', [ParentId] = 'A1111111-1111-1111-1111-111111111002', [SortOrder] = 12, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111026', N'删除楼宇', 'building:delete', NULL, NULL, 'A1111111-1111-1111-1111-111111111002', 12, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111027' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'房间状态变更', [PermissionCode] = 'building:changestatus', [ParentId] = 'A1111111-1111-1111-1111-111111111002', [SortOrder] = 13, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111027', N'房间状态变更', 'building:changestatus', NULL, NULL, 'A1111111-1111-1111-1111-111111111002', 13, 1, @SysUserId, @Now);

-- ==================== 3. 合同管理 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111003' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'合同管理', [PermissionCode] = 'contract:view', [Path] = '/contracts', [Icon] = 'Document', [ParentId] = NULL, [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111003', N'合同管理', 'contract:view', '/contracts', 'Document', NULL, 3, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111031' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'合同列表', [PermissionCode] = 'contract:list', [Path] = '/contracts', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111031', N'合同列表', 'contract:list', '/contracts', '', 'A1111111-1111-1111-1111-111111111003', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111032' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新建合同', [PermissionCode] = 'contract:create', [Path] = '/contracts/create', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111032', N'新建合同', 'contract:create', '/contracts/create', '', 'A1111111-1111-1111-1111-111111111003', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111033' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'合同详情', [PermissionCode] = 'contract:detail', [Path] = '/contracts/:id', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 3, [IsActive] = 0, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111033', N'合同详情', 'contract:detail', '/contracts/:id', '', 'A1111111-1111-1111-1111-111111111003', 3, 0, @SysUserId, @Now);

-- 按钮权限
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111034' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'编辑合同', [PermissionCode] = 'contract:edit', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111034', N'编辑合同', 'contract:edit', NULL, NULL, 'A1111111-1111-1111-1111-111111111003', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111035' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'终止合同', [PermissionCode] = 'contract:terminate', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111035', N'终止合同', 'contract:terminate', NULL, NULL, 'A1111111-1111-1111-1111-111111111003', 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111036' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'续签合同', [PermissionCode] = 'contract:renew', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 12, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111036', N'续签合同', 'contract:renew', NULL, NULL, 'A1111111-1111-1111-1111-111111111003', 12, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111037' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'暂停/恢复合同', [PermissionCode] = 'contract:togglestatus', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 13, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111037', N'暂停/恢复合同', 'contract:togglestatus', NULL, NULL, 'A1111111-1111-1111-1111-111111111003', 13, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111038' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'租金调整', [PermissionCode] = 'contract:adjustrent', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 14, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111038', N'租金调整', 'contract:adjustrent', NULL, NULL, 'A1111111-1111-1111-1111-111111111003', 14, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111039' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'费用调价', [PermissionCode] = 'contract:adjustfee', [ParentId] = 'A1111111-1111-1111-1111-111111111003', [SortOrder] = 15, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111039', N'费用调价', 'contract:adjustfee', NULL, NULL, 'A1111111-1111-1111-1111-111111111003', 15, 1, @SysUserId, @Now);

-- ==================== 4. 收款管理 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111004' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收款管理', [PermissionCode] = 'receipt:view', [Path] = '/receipts', [Icon] = 'Money', [ParentId] = NULL, [SortOrder] = 4, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111004', N'收款管理', 'receipt:view', '/receipts', 'Money', NULL, 4, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111041' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收款列表', [PermissionCode] = 'receipt:list', [Path] = '/receipts', [ParentId] = 'A1111111-1111-1111-1111-111111111004', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111041', N'收款列表', 'receipt:list', '/receipts', '', 'A1111111-1111-1111-1111-111111111004', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111042' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收款登记', [PermissionCode] = 'receipt:register', [Path] = '/receipts/register', [ParentId] = 'A1111111-1111-1111-1111-111111111004', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111042', N'收款登记', 'receipt:register', '/receipts/register', '', 'A1111111-1111-1111-1111-111111111004', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111043' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收款确认', [PermissionCode] = 'receipt:confirm', [Path] = '/receipts/confirm', [ParentId] = 'A1111111-1111-1111-1111-111111111004', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111043', N'收款确认', 'receipt:confirm', '/receipts/confirm', '', 'A1111111-1111-1111-1111-111111111004', 3, 1, @SysUserId, @Now);

-- 按钮权限
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111044' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'确认到账', [PermissionCode] = 'receipt:confirmamount', [ParentId] = 'A1111111-1111-1111-1111-111111111004', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111044', N'确认到账', 'receipt:confirmamount', NULL, NULL, 'A1111111-1111-1111-1111-111111111004', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111045' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'驳回收款', [PermissionCode] = 'receipt:reject', [ParentId] = 'A1111111-1111-1111-1111-111111111004', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111045', N'驳回收款', 'receipt:reject', NULL, NULL, 'A1111111-1111-1111-1111-111111111004', 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111046' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收款冲销', [PermissionCode] = 'receipt:reverse', [ParentId] = 'A1111111-1111-1111-1111-111111111004', [SortOrder] = 12, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111046', N'收款冲销', 'receipt:reverse', NULL, NULL, 'A1111111-1111-1111-1111-111111111004', 12, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111047' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'押金退还/扣款', [PermissionCode] = 'receipt:deposit', [ParentId] = 'A1111111-1111-1111-1111-111111111004', [SortOrder] = 13, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111047', N'押金退还/扣款', 'receipt:deposit', NULL, NULL, 'A1111111-1111-1111-1111-111111111004', 13, 1, @SysUserId, @Now);

-- ==================== 5. 账单管理 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111005' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'账单管理', [PermissionCode] = 'bill:view', [Path] = '/bills', [Icon] = 'DocumentCopy', [ParentId] = NULL, [SortOrder] = 5, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111005', N'账单管理', 'bill:view', '/bills', 'DocumentCopy', NULL, 5, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111051' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'账单列表', [PermissionCode] = 'bill:list', [Path] = '/bills', [ParentId] = 'A1111111-1111-1111-1111-111111111005', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111051', N'账单列表', 'bill:list', '/bills', '', 'A1111111-1111-1111-1111-111111111005', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111052' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'生成账单', [PermissionCode] = 'bill:generate', [Path] = '/bills/generate', [ParentId] = 'A1111111-1111-1111-1111-111111111005', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111052', N'生成账单', 'bill:generate', '/bills/generate', '', 'A1111111-1111-1111-1111-111111111005', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111053' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'账单预览', [PermissionCode] = 'bill:preview', [Path] = '/bills/preview/:id', [ParentId] = 'A1111111-1111-1111-1111-111111111005', [SortOrder] = 3, [IsActive] = 0, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111053', N'账单预览', 'bill:preview', '/bills/preview/:id', '', 'A1111111-1111-1111-1111-111111111005', 3, 0, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111054' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'批量导出PDF', [PermissionCode] = 'bill:exportpdf', [ParentId] = 'A1111111-1111-1111-1111-111111111005', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111054', N'批量导出PDF', 'bill:exportpdf', NULL, NULL, 'A1111111-1111-1111-1111-111111111005', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111055' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'打印账单', [PermissionCode] = 'bill:print', [ParentId] = 'A1111111-1111-1111-1111-111111111005', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111055', N'打印账单', 'bill:print', NULL, NULL, 'A1111111-1111-1111-1111-111111111005', 11, 1, @SysUserId, @Now);

-- ==================== 6. 租客管理 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111006' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'租客管理', [PermissionCode] = 'tenant:view', [Path] = '/tenants', [Icon] = 'UserFilled', [ParentId] = NULL, [SortOrder] = 6, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111006', N'租客管理', 'tenant:view', '/tenants', 'UserFilled', NULL, 6, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111061' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'租客列表', [PermissionCode] = 'tenant:list', [Path] = '/tenants', [ParentId] = 'A1111111-1111-1111-1111-111111111006', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111061', N'租客列表', 'tenant:list', '/tenants', '', 'A1111111-1111-1111-1111-111111111006', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111062' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'租客详情', [PermissionCode] = 'tenant:detail', [Path] = '/tenants/:id', [ParentId] = 'A1111111-1111-1111-1111-111111111006', [SortOrder] = 2, [IsActive] = 0, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111062', N'租客详情', 'tenant:detail', '/tenants/:id', '', 'A1111111-1111-1111-1111-111111111006', 2, 0, @SysUserId, @Now);

-- 按钮
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111063' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新增租客', [PermissionCode] = 'tenant:create', [ParentId] = 'A1111111-1111-1111-1111-111111111006', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111063', N'新增租客', 'tenant:create', NULL, NULL, 'A1111111-1111-1111-1111-111111111006', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111064' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'编辑租客', [PermissionCode] = 'tenant:edit', [ParentId] = 'A1111111-1111-1111-1111-111111111006', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111064', N'编辑租客', 'tenant:edit', NULL, NULL, 'A1111111-1111-1111-1111-111111111006', 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111065' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'删除租客', [PermissionCode] = 'tenant:delete', [ParentId] = 'A1111111-1111-1111-1111-111111111006', [SortOrder] = 12, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111065', N'删除租客', 'tenant:delete', NULL, NULL, 'A1111111-1111-1111-1111-111111111006', 12, 1, @SysUserId, @Now);

-- ==================== 7. 催缴管理 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111007' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'催缴管理', [PermissionCode] = 'collection:view', [Path] = '/collection', [Icon] = 'BellFilled', [ParentId] = NULL, [SortOrder] = 7, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111007', N'催缴管理', 'collection:view', '/collection', 'BellFilled', NULL, 7, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111071' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'催缴概览', [PermissionCode] = 'collection:overview', [Path] = '/collection', [ParentId] = 'A1111111-1111-1111-1111-111111111007', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111071', N'催缴概览', 'collection:overview', '/collection', '', 'A1111111-1111-1111-1111-111111111007', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111072' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'催缴配置', [PermissionCode] = 'collection:config', [Path] = '/collection/config', [ParentId] = 'A1111111-1111-1111-1111-111111111007', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111072', N'催缴配置', 'collection:config', '/collection/config', '', 'A1111111-1111-1111-1111-111111111007', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111073' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'催缴记录', [PermissionCode] = 'collection:records', [Path] = '/collection/records', [ParentId] = 'A1111111-1111-1111-1111-111111111007', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111073', N'催缴记录', 'collection:records', '/collection/records', '', 'A1111111-1111-1111-1111-111111111007', 3, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111074' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'发送催缴', [PermissionCode] = 'collection:send', [ParentId] = 'A1111111-1111-1111-1111-111111111007', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111074', N'发送催缴', 'collection:send', NULL, NULL, 'A1111111-1111-1111-1111-111111111007', 10, 1, @SysUserId, @Now);

-- ==================== 8. 抄表管理 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111008' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'抄表管理', [PermissionCode] = 'meter:view', [Path] = '/meter', [Icon] = 'Reading', [ParentId] = NULL, [SortOrder] = 8, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111008', N'抄表管理', 'meter:view', '/meter', 'Reading', NULL, 8, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111081' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'抄表记录', [PermissionCode] = 'meter:list', [Path] = '/meter', [ParentId] = 'A1111111-1111-1111-1111-111111111008', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111081', N'抄表记录', 'meter:list', '/meter', '', 'A1111111-1111-1111-1111-111111111008', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111082' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'Excel批量导入', [PermissionCode] = 'meter:import', [ParentId] = 'A1111111-1111-1111-1111-111111111008', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111082', N'Excel批量导入', 'meter:import', NULL, NULL, 'A1111111-1111-1111-1111-111111111008', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111083' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'逾期估读', [PermissionCode] = 'meter:estimate', [ParentId] = 'A1111111-1111-1111-1111-111111111008', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111083', N'逾期估读', 'meter:estimate', NULL, NULL, 'A1111111-1111-1111-1111-111111111008', 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111084' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'保存/确认抄表', [PermissionCode] = 'meter:savereadings', [ParentId] = 'A1111111-1111-1111-1111-111111111008', [SortOrder] = 12, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111084', N'保存/确认抄表', 'meter:savereadings', NULL, NULL, 'A1111111-1111-1111-1111-111111111008', 12, 1, @SysUserId, @Now);

-- ==================== 9. 审批中心 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111009' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'审批中心', [PermissionCode] = 'approval:view', [Path] = '/approvals', [Icon] = 'CircleCheck', [ParentId] = NULL, [SortOrder] = 9, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111009', N'审批中心', 'approval:view', '/approvals', 'CircleCheck', NULL, 9, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111091' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'待审批', [PermissionCode] = 'approval:pending', [Path] = '/approvals', [ParentId] = 'A1111111-1111-1111-1111-111111111009', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111091', N'待审批', 'approval:pending', '/approvals', '', 'A1111111-1111-1111-1111-111111111009', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111092' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'我的提交', [PermissionCode] = 'approval:myrequests', [Path] = '/approvals/myrequests', [ParentId] = 'A1111111-1111-1111-1111-111111111009', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111092', N'我的提交', 'approval:myrequests', '/approvals/myrequests', '', 'A1111111-1111-1111-1111-111111111009', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111093' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'审批历史', [PermissionCode] = 'approval:history', [Path] = '/approvals/history', [ParentId] = 'A1111111-1111-1111-1111-111111111009', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111093', N'审批历史', 'approval:history', '/approvals/history', '', 'A1111111-1111-1111-1111-111111111009', 3, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111094' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'通过审批', [PermissionCode] = 'approval:approve', [ParentId] = 'A1111111-1111-1111-1111-111111111009', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111094', N'通过审批', 'approval:approve', NULL, NULL, 'A1111111-1111-1111-1111-111111111009', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111095' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'驳回审批', [PermissionCode] = 'approval:reject', [ParentId] = 'A1111111-1111-1111-1111-111111111009', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111095', N'驳回审批', 'approval:reject', NULL, NULL, 'A1111111-1111-1111-1111-111111111009', 11, 1, @SysUserId, @Now);

-- ==================== 10. 通知中心 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111010' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'通知中心', [PermissionCode] = 'notification:view', [Path] = '/notifications', [Icon] = 'Bell', [ParentId] = NULL, [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111010', N'通知中心', 'notification:view', '/notifications', 'Bell', NULL, 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111101' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'全部标记已读', [PermissionCode] = 'notification:markallread', [ParentId] = 'A1111111-1111-1111-1111-111111111010', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111101', N'全部标记已读', 'notification:markallread', NULL, NULL, 'A1111111-1111-1111-1111-111111111010', 10, 1, @SysUserId, @Now);

-- ==================== 11. 财务报表 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111011' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'财务报表', [PermissionCode] = 'report:view', [Path] = '/reports', [Icon] = 'TrendCharts', [ParentId] = NULL, [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111011', N'财务报表', 'report:view', '/reports', 'TrendCharts', NULL, 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111111' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收租率统计', [PermissionCode] = 'report:collectionrate', [Path] = '/reports/collectionrate', [ParentId] = 'A1111111-1111-1111-1111-111111111011', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111111', N'收租率统计', 'report:collectionrate', '/reports/collectionrate', '', 'A1111111-1111-1111-1111-111111111011', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111112' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'欠费明细表', [PermissionCode] = 'report:overduedetail', [Path] = '/reports/overduedetail', [ParentId] = 'A1111111-1111-1111-1111-111111111011', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111112', N'欠费明细表', 'report:overduedetail', '/reports/overduedetail', '', 'A1111111-1111-1111-1111-111111111011', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111113' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收款日报', [PermissionCode] = 'report:dailyreceipt', [Path] = '/reports/dailyreceipt', [ParentId] = 'A1111111-1111-1111-1111-111111111011', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111113', N'收款日报', 'report:dailyreceipt', '/reports/dailyreceipt', '', 'A1111111-1111-1111-1111-111111111011', 3, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111114' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收款月报', [PermissionCode] = 'report:monthlyreceipt', [Path] = '/reports/monthlyreceipt', [ParentId] = 'A1111111-1111-1111-1111-111111111011', [SortOrder] = 4, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111114', N'收款月报', 'report:monthlyreceipt', '/reports/monthlyreceipt', '', 'A1111111-1111-1111-1111-111111111011', 4, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111115' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'费用收入统计', [PermissionCode] = 'report:feerevenue', [Path] = '/reports/feerevenue', [ParentId] = 'A1111111-1111-1111-1111-111111111011', [SortOrder] = 5, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111115', N'费用收入统计', 'report:feerevenue', '/reports/feerevenue', '', 'A1111111-1111-1111-1111-111111111011', 5, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111116' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'出租率统计', [PermissionCode] = 'report:occupancyrate', [Path] = '/reports/occupancyrate', [ParentId] = 'A1111111-1111-1111-1111-111111111011', [SortOrder] = 6, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111116', N'出租率统计', 'report:occupancyrate', '/reports/occupancyrate', '', 'A1111111-1111-1111-1111-111111111011', 6, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111117' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'导出报表Excel', [PermissionCode] = 'report:export', [ParentId] = 'A1111111-1111-1111-1111-111111111011', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111117', N'导出报表Excel', 'report:export', NULL, NULL, 'A1111111-1111-1111-1111-111111111011', 10, 1, @SysUserId, @Now);

-- ==================== 12. 会计核算 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111012' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'会计核算', [PermissionCode] = 'accounting:view', [Path] = '/accounting', [Icon] = 'Files', [ParentId] = NULL, [SortOrder] = 12, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111012', N'会计核算', 'accounting:view', '/accounting', 'Files', NULL, 12, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111121' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'科目表', [PermissionCode] = 'accounting:subjects', [Path] = '/accounting/subjects', [ParentId] = 'A1111111-1111-1111-1111-111111111012', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111121', N'科目表', 'accounting:subjects', '/accounting/subjects', '', 'A1111111-1111-1111-1111-111111111012', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111122' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'日记账', [PermissionCode] = 'accounting:journal', [Path] = '/accounting/journal', [ParentId] = 'A1111111-1111-1111-1111-111111111012', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111122', N'日记账', 'accounting:journal', '/accounting/journal', '', 'A1111111-1111-1111-1111-111111111012', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111123' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'凭证管理', [PermissionCode] = 'accounting:vouchers', [Path] = '/accounting/vouchers', [ParentId] = 'A1111111-1111-1111-1111-111111111012', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111123', N'凭证管理', 'accounting:vouchers', '/accounting/vouchers', '', 'A1111111-1111-1111-1111-111111111012', 3, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111124' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'试算平衡表', [PermissionCode] = 'accounting:trialbalance', [Path] = '/accounting/trialbalance', [ParentId] = 'A1111111-1111-1111-1111-111111111012', [SortOrder] = 4, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111124', N'试算平衡表', 'accounting:trialbalance', '/accounting/trialbalance', '', 'A1111111-1111-1111-1111-111111111012', 4, 1, @SysUserId, @Now);

-- 按钮
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111125' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新增科目', [PermissionCode] = 'accounting:subjectcreate', [ParentId] = 'A1111111-1111-1111-1111-111111111012', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111125', N'新增科目', 'accounting:subjectcreate', NULL, NULL, 'A1111111-1111-1111-1111-111111111012', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111126' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'过账', [PermissionCode] = 'accounting:post', [ParentId] = 'A1111111-1111-1111-1111-111111111012', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111126', N'过账', 'accounting:post', NULL, NULL, 'A1111111-1111-1111-1111-111111111012', 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111127' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'冲销凭证', [PermissionCode] = 'accounting:reverse', [ParentId] = 'A1111111-1111-1111-1111-111111111012', [SortOrder] = 12, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111127', N'冲销凭证', 'accounting:reverse', NULL, NULL, 'A1111111-1111-1111-1111-111111111012', 12, 1, @SysUserId, @Now);

-- ==================== 13. 银企直连 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111013' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'银企直连', [PermissionCode] = 'bank:view', [Path] = '/bank', [Icon] = 'Link', [ParentId] = NULL, [SortOrder] = 13, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111013', N'银企直连', 'bank:view', '/bank', 'Link', NULL, 13, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111131' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'流水导入', [PermissionCode] = 'bank:import', [Path] = '/bank/import', [ParentId] = 'A1111111-1111-1111-1111-111111111013', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111131', N'流水导入', 'bank:import', '/bank/import', '', 'A1111111-1111-1111-1111-111111111013', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111132' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'自动匹配', [PermissionCode] = 'bank:match', [Path] = '/bank/match', [ParentId] = 'A1111111-1111-1111-1111-111111111013', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111132', N'自动匹配', 'bank:match', '/bank/match', '', 'A1111111-1111-1111-1111-111111111013', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111133' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'余额调节表', [PermissionCode] = 'bank:reconciliation', [Path] = '/bank/reconciliation', [ParentId] = 'A1111111-1111-1111-1111-111111111013', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111133', N'余额调节表', 'bank:reconciliation', '/bank/reconciliation', '', 'A1111111-1111-1111-1111-111111111013', 3, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111134' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'确认导入', [PermissionCode] = 'bank:confirmimport', [ParentId] = 'A1111111-1111-1111-1111-111111111013', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111134', N'确认导入', 'bank:confirmimport', NULL, NULL, 'A1111111-1111-1111-1111-111111111013', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111135' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'手动匹配', [PermissionCode] = 'bank:manualmatch', [ParentId] = 'A1111111-1111-1111-1111-111111111013', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111135', N'手动匹配', 'bank:manualmatch', NULL, NULL, 'A1111111-1111-1111-1111-111111111013', 11, 1, @SysUserId, @Now);

-- ==================== 14. 多房东总览 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111014' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'多房东总览', [PermissionCode] = 'landlordoverview:view', [Path] = '/reports/landlordoverview', [Icon] = 'DataAnalysis', [ParentId] = NULL, [SortOrder] = 14, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111014', N'多房东总览', 'landlordoverview:view', '/reports/landlordoverview', 'DataAnalysis', NULL, 14, 1, @SysUserId, @Now);

-- ==================== 15. 变更审计 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111015' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'变更审计', [PermissionCode] = 'audit:view', [Path] = '/audit', [Icon] = 'Search', [ParentId] = NULL, [SortOrder] = 15, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111015', N'变更审计', 'audit:view', '/audit', 'Search', NULL, 15, 1, @SysUserId, @Now);

-- ==================== 16. 系统设置 ====================
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111016' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'系统设置', [PermissionCode] = 'system:view', [Path] = '/system', [Icon] = 'Setting', [ParentId] = NULL, [SortOrder] = 99, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111016', N'系统设置', 'system:view', '/system', 'Setting', NULL, 99, 1, @SysUserId, @Now);


MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111162' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'用户管理', [PermissionCode] = 'system:user', [Path] = '/system/organization/users', [ParentId] = 'A1111111-1111-1111-1111-111111111016', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111162', N'用户管理', 'system:user', '/system/organization/users', 'User', 'A1111111-1111-1111-1111-111111111016', 1, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111163' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'角色管理', [PermissionCode] = 'system:role', [Path] = '/system/organization/roles', [ParentId] = 'A1111111-1111-1111-1111-111111111016', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111163', N'角色管理', 'system:role', '/system/organization/roles', 'Avatar', 'A1111111-1111-1111-1111-111111111016', 2, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111164' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'用户数据权限', [PermissionCode] = 'system:userscope', [Path] = '/system/organization/userscope', [ParentId] = 'A1111111-1111-1111-1111-111111111016', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111164', N'用户数据权限', 'system:userscope', '/system/organization/userscope', 'Unlock', 'A1111111-1111-1111-1111-111111111016', 3, 1, @SysUserId, @Now);

-- 按钮权限
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111165' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新增用户', [PermissionCode] = 'system:usercreate', [ParentId] = 'A1111111-1111-1111-1111-111111111162', [SortOrder] = 10, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111165', N'新增用户', 'system:usercreate', NULL, NULL, 'A1111111-1111-1111-1111-111111111162', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111166' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'编辑用户', [PermissionCode] = 'system:useredit', [ParentId] = 'A1111111-1111-1111-1111-111111111162', [SortOrder] = 11, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111166', N'编辑用户', 'system:useredit', NULL, NULL, 'A1111111-1111-1111-1111-111111111162', 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111167' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新增角色', [PermissionCode] = 'system:rolecreate', [ParentId] = 'A1111111-1111-1111-1111-111111111163', [SortOrder] = 12, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111167', N'新增角色', 'system:rolecreate', NULL, NULL, 'A1111111-1111-1111-1111-111111111163', 12, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111168' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'分配菜单权限', [PermissionCode] = 'system:roleassignmenu', [ParentId] = 'A1111111-1111-1111-1111-111111111163', [SortOrder] = 13, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111168', N'分配菜单权限', 'system:roleassignmenu', NULL, NULL, 'A1111111-1111-1111-1111-111111111163', 13, 1, @SysUserId, @Now);

-- 16.2 房东管理（含按钮权限）
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111169' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'房东管理', [PermissionCode] = 'system:landlord', [Path] = '/system/landlords', [Icon] = 'OfficeBuilding', [ParentId] = 'A1111111-1111-1111-1111-111111111016', [SortOrder] = 4, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111169', N'房东管理', 'system:landlord', '/system/landlords', 'OfficeBuilding', 'A1111111-1111-1111-1111-111111111016', 4, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111170' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新增房东', [PermissionCode] = 'system:landlordcreate', [ParentId] = 'A1111111-1111-1111-1111-111111111169', [SortOrder] = 20, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111170', N'新增房东', 'system:landlordcreate', NULL, NULL, 'A1111111-1111-1111-1111-111111111169', 20, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111171' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'编辑房东', [PermissionCode] = 'system:landlordedit', [ParentId] = 'A1111111-1111-1111-1111-111111111169', [SortOrder] = 21, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111171', N'编辑房东', 'system:landlordedit', NULL, NULL, 'A1111111-1111-1111-1111-111111111169', 21, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111172' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'创建房东账号', [PermissionCode] = 'system:landlordcreateuser', [ParentId] = 'A1111111-1111-1111-1111-111111111169', [SortOrder] = 22, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111172', N'创建房东账号', 'system:landlordcreateuser', NULL, NULL, 'A1111111-1111-1111-1111-111111111169', 22, 1, @SysUserId, @Now);

-- 16.3 菜单权限配置（含按钮权限）
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111173' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'菜单权限配置', [PermissionCode] = 'system:menu', [Path] = '/system/menus', [Icon] = 'Menu', [ParentId] = 'A1111111-1111-1111-1111-111111111016', [SortOrder] = 5, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111173', N'菜单权限配置', 'system:menu', '/system/menus', 'Menu', 'A1111111-1111-1111-1111-111111111016', 5, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111174' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新增菜单', [PermissionCode] = 'system:menucreate', [ParentId] = 'A1111111-1111-1111-1111-111111111173', [SortOrder] = 30, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111174', N'新增菜单', 'system:menucreate', NULL, NULL, 'A1111111-1111-1111-1111-111111111173', 30, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111175' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'编辑菜单', [PermissionCode] = 'system:menuedit', [ParentId] = 'A1111111-1111-1111-1111-111111111173', [SortOrder] = 31, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111175', N'编辑菜单', 'system:menuedit', NULL, NULL, 'A1111111-1111-1111-1111-111111111173', 31, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111176' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'删除菜单', [PermissionCode] = 'system:menudelete', [ParentId] = 'A1111111-1111-1111-1111-111111111173', [SortOrder] = 32, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111176', N'删除菜单', 'system:menudelete', NULL, NULL, 'A1111111-1111-1111-1111-111111111173', 32, 1, @SysUserId, @Now);


MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111178' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'审批类型配置', [PermissionCode] = 'system:approvaltype', [Path] = '/system/approvaltypes', [ParentId] = 'A1111111-1111-1111-1111-111111111177', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111178', N'审批类型配置', 'system:approvaltype', '/system/approvaltypes', 'CircleCheck', 'A1111111-1111-1111-1111-111111111016', 6, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111179' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'审批级别配置', [PermissionCode] = 'system:approvallevel', [Path] = '/system/approvallevels', [ParentId] = 'A1111111-1111-1111-1111-111111111177', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111179', N'审批级别配置', 'system:approvallevel', '/system/approvallevels', 'Sort', 'A1111111-1111-1111-1111-111111111016', 7, 1, @SysUserId, @Now);

-- 16.5 收费项目管理
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111180' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'收费项目管理', [PermissionCode] = 'system:feecode', [Path] = '/system/feecodes', [Icon] = 'Coin', [ParentId] = 'A1111111-1111-1111-1111-111111111016', [SortOrder] = 8, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111180', N'收费项目管理', 'system:feecode', '/system/feecodes', 'Coin', 'A1111111-1111-1111-1111-111111111016', 8, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111181' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'新增费用', [PermissionCode] = 'system:feecodecreate', [ParentId] = 'A1111111-1111-1111-1111-111111111180', [SortOrder] = 40, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111181', N'新增费用', 'system:feecodecreate', NULL, NULL, 'A1111111-1111-1111-1111-111111111180', 40, 1, @SysUserId, @Now);


MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111183' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'房型管理', [PermissionCode] = 'system:roomtype', [Path] = '/system/roomtypes', [ParentId] = 'A1111111-1111-1111-1111-111111111182', [SortOrder] = 1, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111183', N'房型管理', 'system:roomtype', '/system/roomtypes', 'Grid', 'A1111111-1111-1111-1111-111111111016', 9, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111184' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'定价标准管理', [PermissionCode] = 'system:pricing', [Path] = '/system/pricing', [ParentId] = 'A1111111-1111-1111-1111-111111111182', [SortOrder] = 2, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111184', N'定价标准管理', 'system:pricing', '/system/pricing', 'PriceTag', 'A1111111-1111-1111-1111-111111111016', 10, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111185' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'支付通道管理', [PermissionCode] = 'system:paymentchannel', [Path] = '/system/paymentchannels', [ParentId] = 'A1111111-1111-1111-1111-111111111182', [SortOrder] = 3, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111185', N'支付通道管理', 'system:paymentchannel', '/system/paymentchannels', 'CreditCard', 'A1111111-1111-1111-1111-111111111016', 11, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111186' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'税率配置', [PermissionCode] = 'system:taxrate', [Path] = '/system/taxrates', [ParentId] = 'A1111111-1111-1111-1111-111111111182', [SortOrder] = 4, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111186', N'税率配置', 'system:taxrate', '/system/taxrates', 'CollectionTag', 'A1111111-1111-1111-1111-111111111016', 12, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111187' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'会计科目管理', [PermissionCode] = 'system:accountingsubject', [Path] = '/system/accountingsubjects', [ParentId] = 'A1111111-1111-1111-1111-111111111182', [SortOrder] = 5, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111187', N'会计科目管理', 'system:accountingsubject', '/system/accountingsubjects', 'DataBoard', 'A1111111-1111-1111-1111-111111111016', 13, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111188' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'节假日管理', [PermissionCode] = 'system:holiday', [Path] = '/system/holidays', [ParentId] = 'A1111111-1111-1111-1111-111111111182', [SortOrder] = 6, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111188', N'节假日管理', 'system:holiday', '/system/holidays', 'Calendar', 'A1111111-1111-1111-1111-111111111016', 14, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111189' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'滞纳金配置', [PermissionCode] = 'system:latefee', [Path] = '/system/latefee', [ParentId] = 'A1111111-1111-1111-1111-111111111182', [SortOrder] = 7, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111189', N'滞纳金配置', 'system:latefee', '/system/latefee', 'WarningFilled', 'A1111111-1111-1111-1111-111111111016', 15, 1, @SysUserId, @Now);

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111190' AS Id) AS S ON T.Id = S.Id
WHEN MATCHED THEN UPDATE SET [Name] = N'调度任务管理', [PermissionCode] = 'system:scheduler', [Path] = '/system/scheduler', [ParentId] = 'A1111111-1111-1111-1111-111111111182', [SortOrder] = 8, [IsActive] = 1, [UpdatedAt] = @Now
WHEN NOT MATCHED THEN INSERT ([Id], [Name], [PermissionCode], [Path], [Icon], [ParentId], [SortOrder], [IsActive], [CreatedBy], [CreatedAt])
    VALUES ('A1111111-1111-1111-1111-111111111190', N'调度任务管理', 'system:scheduler', '/system/scheduler', 'Timer', 'A1111111-1111-1111-1111-111111111016', 16, 1, @SysUserId, @Now);

-- 按钮权限
MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111191' AS Id) AS S ON T.Id = S.Id

MERGE [Menus] AS T
USING (SELECT 'A1111111-1111-1111-1111-111111111192' AS Id) AS S ON T.Id = S.Id

PRINT N'菜单种子数据插入/更新完成！';
GO
