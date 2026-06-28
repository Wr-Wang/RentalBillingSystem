-- ===================================================================
-- 菜单权限种子数据 — 全量重置（清除旧数据重新插入）
-- 层级严格按前端路由，所有按钮权限挂在对应页面下
-- ===================================================================

DECLARE @Now datetime2 = GETDATE();
DECLARE @SysUserId uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @AdminRoleId uniqueidentifier = 'B1111111-1111-1111-1111-111111111001';

-- ==================== 清除旧数据 ====================
DELETE FROM [RoleMenus];
DELETE FROM [Menus];
PRINT N'已清除旧菜单数据';

-- ==================== 1. 仪表盘 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111001','仪表盘','dashboard:view','/dashboard','DataAnalysis',NULL,1,1,@SysUserId,@Now);

-- ==================== 2. 房屋管理 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111002','房屋管理','building:view','/buildings','HomeFilled',NULL,2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111021','房间列表','building:list','/buildings','A1111111-1111-1111-1111-111111111002',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111022','房间详情','building:detail','/buildings/room/:id','A1111111-1111-1111-1111-111111111002',2,0,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111023','批量导入','building:import','/buildings/import','A1111111-1111-1111-1111-111111111002',3,1,@SysUserId,@Now);
-- 按钮权限
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111024','新增楼宇','building:create','A1111111-1111-1111-1111-111111111002',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111025','编辑楼宇','building:edit','A1111111-1111-1111-1111-111111111002',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111026','删除楼宇','building:delete','A1111111-1111-1111-1111-111111111002',12,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111027','房间状态变更','building:changestatus','A1111111-1111-1111-1111-111111111002',13,1,@SysUserId,@Now);

-- ==================== 3. 合同管理 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111003','合同管理','contract:view','/contracts','Document',NULL,3,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111031','合同列表','contract:list','/contracts','A1111111-1111-1111-1111-111111111003',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111032','新建合同','contract:create','/contracts/create','A1111111-1111-1111-1111-111111111003',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111033','合同详情','contract:detail','/contracts/:id','A1111111-1111-1111-1111-111111111003',3,0,@SysUserId,@Now);
-- 按钮权限
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111034','编辑合同','contract:edit','A1111111-1111-1111-1111-111111111003',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111035','终止合同','contract:terminate','A1111111-1111-1111-1111-111111111003',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111036','续签合同','contract:renew','A1111111-1111-1111-1111-111111111003',12,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111037','暂停/恢复合同','contract:togglestatus','A1111111-1111-1111-1111-111111111003',13,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111038','租金调整','contract:adjustrent','A1111111-1111-1111-1111-111111111003',14,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111039','费用调价','contract:adjustfee','A1111111-1111-1111-1111-111111111003',15,1,@SysUserId,@Now);

-- ==================== 4. 收款管理 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111004','收款管理','receipt:view','/receipts','Money',NULL,4,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111041','收款列表','receipt:list','/receipts','A1111111-1111-1111-1111-111111111004',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111042','收款登记','receipt:register','/receipts/register','A1111111-1111-1111-1111-111111111004',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111043','收款确认','receipt:confirm','/receipts/confirm','A1111111-1111-1111-1111-111111111004',3,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111044','确认到账','receipt:confirmamount','A1111111-1111-1111-1111-111111111004',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111045','驳回收款','receipt:reject','A1111111-1111-1111-1111-111111111004',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111046','收款冲销','receipt:reverse','A1111111-1111-1111-1111-111111111004',12,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111047','押金退还/扣款','receipt:deposit','A1111111-1111-1111-1111-111111111004',13,1,@SysUserId,@Now);

-- ==================== 5. 账单管理 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111005','账单管理','bill:view','/bills','DocumentCopy',NULL,5,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111051','账单列表','bill:list','/bills','A1111111-1111-1111-1111-111111111005',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111052','生成账单','bill:generate','/bills/generate','A1111111-1111-1111-1111-111111111005',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111053','账单预览','bill:preview','/bills/preview/:id','A1111111-1111-1111-1111-111111111005',3,0,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111054','批量导出PDF','bill:exportpdf','A1111111-1111-1111-1111-111111111005',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111055','打印账单','bill:print','A1111111-1111-1111-1111-111111111005',11,1,@SysUserId,@Now);

-- ==================== 6. 租客管理 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111006','租客管理','tenant:view','/tenants','UserFilled',NULL,6,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111061','租客列表','tenant:list','/tenants','A1111111-1111-1111-1111-111111111006',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111062','租客详情','tenant:detail','/tenants/:id','A1111111-1111-1111-1111-111111111006',2,0,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111063','新增租客','tenant:create','A1111111-1111-1111-1111-111111111006',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111064','编辑租客','tenant:edit','A1111111-1111-1111-1111-111111111006',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111065','删除租客','tenant:delete','A1111111-1111-1111-1111-111111111006',12,1,@SysUserId,@Now);

-- ==================== 7. 催缴管理 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111007','催缴管理','collection:view','/collection','BellFilled',NULL,7,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111071','催缴概览','collection:overview','/collection','A1111111-1111-1111-1111-111111111007',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111072','催缴配置','collection:config','/collection/config','A1111111-1111-1111-1111-111111111007',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111073','催缴记录','collection:records','/collection/records','A1111111-1111-1111-1111-111111111007',3,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111074','发送催缴','collection:send','A1111111-1111-1111-1111-111111111007',10,1,@SysUserId,@Now);

-- ==================== 8. 抄表管理 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111008','抄表管理','meter:view','/meter','Reading',NULL,8,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111081','抄表记录','meter:list','/meter','A1111111-1111-1111-1111-111111111008',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111082','Excel批量导入','meter:import','A1111111-1111-1111-1111-111111111008',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111083','逾期估读','meter:estimate','A1111111-1111-1111-1111-111111111008',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111084','保存/确认抄表','meter:savereadings','A1111111-1111-1111-1111-111111111008',12,1,@SysUserId,@Now);

-- ==================== 9. 审批中心 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111009','审批中心','approval:view','/approvals','CircleCheck',NULL,9,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111091','待审批','approval:pending','/approvals','A1111111-1111-1111-1111-111111111009',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111092','我的提交','approval:myrequests','/approvals/myrequests','A1111111-1111-1111-1111-111111111009',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111093','审批历史','approval:history','/approvals/history','A1111111-1111-1111-1111-111111111009',3,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111094','通过审批','approval:approve','A1111111-1111-1111-1111-111111111009',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111095','驳回审批','approval:reject','A1111111-1111-1111-1111-111111111009',11,1,@SysUserId,@Now);

-- ==================== 10. 通知中心 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111010','通知中心','notification:view','/notifications','Bell',NULL,10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111101','全部标记已读','notification:markallread','A1111111-1111-1111-1111-111111111010',10,1,@SysUserId,@Now);

-- ==================== 11. 财务报表 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111011','财务报表','report:view','/reports','TrendCharts',NULL,11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111111','收租率统计','report:collectionrate','/reports/collectionrate','A1111111-1111-1111-1111-111111111011',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111112','欠费明细表','report:overduedetail','/reports/overduedetail','A1111111-1111-1111-1111-111111111011',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111113','收款日报','report:dailyreceipt','/reports/dailyreceipt','A1111111-1111-1111-1111-111111111011',3,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111114','收款月报','report:monthlyreceipt','/reports/monthlyreceipt','A1111111-1111-1111-1111-111111111011',4,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111115','费用收入统计','report:feerevenue','/reports/feerevenue','A1111111-1111-1111-1111-111111111011',5,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111116','出租率统计','report:occupancyrate','/reports/occupancyrate','A1111111-1111-1111-1111-111111111011',6,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111117','导出报表Excel','report:export','A1111111-1111-1111-1111-111111111011',10,1,@SysUserId,@Now);

-- ==================== 12. 会计核算 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111012','会计核算','accounting:view','/accounting','Files',NULL,12,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111121','科目表','accounting:subjects','/accounting/subjects','A1111111-1111-1111-1111-111111111012',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111122','日记账','accounting:journal','/accounting/journal','A1111111-1111-1111-1111-111111111012',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111123','凭证管理','accounting:vouchers','/accounting/vouchers','A1111111-1111-1111-1111-111111111012',3,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111124','试算平衡表','accounting:trialbalance','/accounting/trialbalance','A1111111-1111-1111-1111-111111111012',4,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111125','新增科目','accounting:subjectcreate','A1111111-1111-1111-1111-111111111012',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111126','过账','accounting:post','A1111111-1111-1111-1111-111111111012',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111127','冲销凭证','accounting:reverse','A1111111-1111-1111-1111-111111111012',12,1,@SysUserId,@Now);

-- ==================== 13. 银企直连 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111013','银企直连','bank:view','/bank','Link',NULL,13,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111131','流水导入','bank:import','/bank/import','A1111111-1111-1111-1111-111111111013',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111132','自动匹配','bank:match','/bank/match','A1111111-1111-1111-1111-111111111013',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111133','余额调节表','bank:reconciliation','/bank/reconciliation','A1111111-1111-1111-1111-111111111013',3,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111134','确认导入','bank:confirmimport','A1111111-1111-1111-1111-111111111013',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111135','手动匹配','bank:manualmatch','A1111111-1111-1111-1111-111111111013',11,1,@SysUserId,@Now);

-- ==================== 14. 多公司总览 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111014','多公司总览','companyoverview:view','/reports/companyoverview','DataAnalysis',NULL,14,1,@SysUserId,@Now);

-- ==================== 15. 变更审计 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111015','变更审计','audit:view','/audit','Search',NULL,15,1,@SysUserId,@Now);

-- ==================== 16. 系统设置 ====================
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111016','系统设置','system:view','/system','Setting',NULL,99,1,@SysUserId,@Now);

-- 16.1 用户管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111162','用户管理','system:user','/system/organization/users','User','A1111111-1111-1111-1111-111111111016',1,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111165','新增用户','system:usercreate','A1111111-1111-1111-1111-111111111162',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111166','编辑用户','system:useredit','A1111111-1111-1111-1111-111111111162',11,1,@SysUserId,@Now);

-- 16.2 角色管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111163','角色管理','system:role','/system/organization/roles','Avatar','A1111111-1111-1111-1111-111111111016',2,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111167','新增角色','system:rolecreate','A1111111-1111-1111-1111-111111111163',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111168','分配菜单权限','system:roleassignmenu','A1111111-1111-1111-1111-111111111163',11,1,@SysUserId,@Now);

-- 16.3 用户数据权限
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111164','用户数据权限','system:userscope','/system/organization/userscope','Unlock','A1111111-1111-1111-1111-111111111016',3,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111302','配置权限','system:userscopeconfig','A1111111-1111-1111-1111-111111111164',10,1,@SysUserId,@Now);

-- 16.4 公司管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111169','公司管理','system:company','/system/companies','OfficeBuilding','A1111111-1111-1111-1111-111111111016',4,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111170','新增公司','system:companycreate','A1111111-1111-1111-1111-111111111169',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111171','编辑公司','system:companyedit','A1111111-1111-1111-1111-111111111169',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111172','创建公司账号','system:companycreateuser','A1111111-1111-1111-1111-111111111169',12,1,@SysUserId,@Now);

-- 16.5 菜单权限配置
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111173','菜单权限配置','system:menu','/system/menus','Menu','A1111111-1111-1111-1111-111111111016',5,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111174','新增菜单','system:menucreate','A1111111-1111-1111-1111-111111111173',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111175','编辑菜单','system:menuedit','A1111111-1111-1111-1111-111111111173',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111176','删除菜单','system:menudelete','A1111111-1111-1111-1111-111111111173',12,1,@SysUserId,@Now);

-- 16.6 审批类型配置
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111178','审批类型配置','system:approvaltype','/system/approvaltypes','CircleCheck','A1111111-1111-1111-1111-111111111016',6,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111303','新增审批类型','system:approvaltypecreate','A1111111-1111-1111-1111-111111111178',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111304','编辑审批类型','system:approvaltypeedit','A1111111-1111-1111-1111-111111111178',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111305','级别配置','system:approvaltypelevel','A1111111-1111-1111-1111-111111111178',12,1,@SysUserId,@Now);

-- 级别按钮权限
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111306',N'新增级别','system:approvallevelcreate','A1111111-1111-1111-1111-111111111178',15,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111307',N'编辑级别','system:approvalleveledit','A1111111-1111-1111-1111-111111111178',16,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111308',N'删除级别','system:approvalleveldelete','A1111111-1111-1111-1111-111111111178',17,1,@SysUserId,@Now);


-- 16.8 收费项目管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111180','收费项目管理','system:feecode','/system/feecodes','Coin','A1111111-1111-1111-1111-111111111016',8,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111181','新增费用','system:feecodecreate','A1111111-1111-1111-1111-111111111180',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111309','编辑费用','system:feecodeedit','A1111111-1111-1111-1111-111111111180',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111310','科目模板配置','system:feecodetemplate','A1111111-1111-1111-1111-111111111180',12,1,@SysUserId,@Now);

-- 16.9 房型管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111183','房型管理','system:roomtype','/system/roomtypes','Grid','A1111111-1111-1111-1111-111111111016',9,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111201','新增房型','system:roomtypecreate','A1111111-1111-1111-1111-111111111183',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111202','编辑房型','system:roomtypeedit','A1111111-1111-1111-1111-111111111183',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111203','删除房型','system:roomtypedelete','A1111111-1111-1111-1111-111111111183',12,1,@SysUserId,@Now);

-- 16.10 定价标准管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111184','定价标准管理','system:pricing','/system/pricing','PriceTag','A1111111-1111-1111-1111-111111111016',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111204','新增定价','system:pricingcreate','A1111111-1111-1111-1111-111111111184',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111205','编辑定价','system:pricingedit','A1111111-1111-1111-1111-111111111184',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111206','删除定价','system:pricingdelete','A1111111-1111-1111-1111-111111111184',12,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111401','新增楼层级别','system:floorlevelcreate','A1111111-1111-1111-1111-111111111184',15,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111402','编辑楼层级别','system:floorleveledit','A1111111-1111-1111-1111-111111111184',16,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111403','删除楼层级别','system:floorleveldelete','A1111111-1111-1111-1111-111111111184',17,1,@SysUserId,@Now);

-- 16.11 支付通道管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111185','支付通道管理','system:paymentchannel','/system/paymentchannels','CreditCard','A1111111-1111-1111-1111-111111111016',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111207','新增通道','system:paymentchannelcreate','A1111111-1111-1111-1111-111111111185',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111208','编辑通道','system:paymentchanneledit','A1111111-1111-1111-1111-111111111185',11,1,@SysUserId,@Now);

-- 16.12 税率配置
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111186','税率配置','system:taxrate','/system/taxrates','CollectionTag','A1111111-1111-1111-1111-111111111016',12,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111209','新增税率','system:taxratecreate','A1111111-1111-1111-1111-111111111186',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111210','编辑税率','system:taxrateedit','A1111111-1111-1111-1111-111111111186',11,1,@SysUserId,@Now);

-- 16.13 会计科目管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111187','会计科目管理','system:accountingsubject','/system/accountingsubjects','DataBoard','A1111111-1111-1111-1111-111111111016',13,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111211','新增科目','system:accountingsubjectcreate','A1111111-1111-1111-1111-111111111187',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111212','编辑科目','system:accountingsubjectedit','A1111111-1111-1111-1111-111111111187',11,1,@SysUserId,@Now);

-- 16.14 节假日管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111188','节假日管理','system:holiday','/system/holidays','Calendar','A1111111-1111-1111-1111-111111111016',14,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111213','新增节假日','system:holidaycreate','A1111111-1111-1111-1111-111111111188',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111214','导入节假日','system:holidayimport','A1111111-1111-1111-1111-111111111188',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111215','编辑节假日','system:holidayedit','A1111111-1111-1111-1111-111111111188',12,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111216','删除节假日','system:holidaydelete','A1111111-1111-1111-1111-111111111188',13,1,@SysUserId,@Now);

-- 16.15 滞纳金配置
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111189','滞纳金配置','system:latefee','/system/latefee','WarningFilled','A1111111-1111-1111-1111-111111111016',15,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111301','保存配置','system:latefeesave','A1111111-1111-1111-1111-111111111189',10,1,@SysUserId,@Now);

-- 16.16 调度任务管理
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111190','调度任务管理','system:scheduler','/system/scheduler','Timer','A1111111-1111-1111-1111-111111111016',16,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111217','调度配置','system:schedulerconfig','A1111111-1111-1111-1111-111111111190',10,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111218','修改排期','system:scheduleredit','A1111111-1111-1111-1111-111111111190',11,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111219','批量生成排期','system:schedulergenerate','A1111111-1111-1111-1111-111111111190',12,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111220','添加自定义排期','system:scheduleradd','A1111111-1111-1111-1111-111111111190',13,1,@SysUserId,@Now);
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111221','查看日志','system:schedulerviewlog','A1111111-1111-1111-1111-111111111190',14,1,@SysUserId,@Now);

-- 16.17 系统日志
INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt])
VALUES ('A1111111-1111-1111-1111-111111111222','系统日志','system:logs','/system/logs','Document','A1111111-1111-1111-1111-111111111016',17,1,@SysUserId,@Now);

PRINT N'菜单种子数据插入完成！';

-- ==================== 创建 Admin 角色并分配权限 ====================
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Code] = 'Admin')
BEGIN
    INSERT INTO [Roles] ([Id], [Name], [Code], [Description], [IsActive], [CreatedBy], [CreatedAt])
    VALUES (@AdminRoleId, N'系统管理员', 'Admin', N'系统配置、用户管理、审批流程', 1, @SysUserId, @Now);
    PRINT N'Admin 角色已创建';
END
ELSE
    SELECT @AdminRoleId = [Id] FROM [Roles] WHERE [Code] = 'Admin';

INSERT INTO [RoleMenus] ([Id], [RoleId], [MenuId], [CreatedBy], [CreatedAt])
SELECT NEWID(), @AdminRoleId, M.[Id], @SysUserId, @Now
FROM [Menus] M
WHERE M.[IsActive] = 1
  AND NOT EXISTS (SELECT 1 FROM [RoleMenus] RM WHERE RM.[RoleId] = @AdminRoleId AND RM.[MenuId] = M.[Id]);

DECLARE @Count int = (SELECT COUNT(*) FROM [RoleMenus] WHERE [RoleId] = @AdminRoleId);
PRINT N'Admin 角色当前共有 ' + CAST(@Count AS nvarchar) + N' 个菜单权限。';
GO
