-- ===================================================================
-- 审批数据统一初始化（三级审批）
-- 执行方式: sqlcmd -S localhost -d RBS -i SeedApprovalTypes_3Level.sql -C
-- ===================================================================
DELETE FROM [ApprovalLevelConfigs];
DELETE FROM [ApprovalRequests];
DELETE FROM [ApprovalTypes];

DECLARE @Cid uniqueidentifier = 'A1111111-1111-1111-1111-111111111001';
DECLARE @Sys uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Now datetime2 = GETDATE();

-- 审批类型（5 种）
INSERT INTO [ApprovalTypes] ([Id],[Name],[Code],[Description],[IsActive],[CompanyId],[CreatedBy],[CreatedAt])
VALUES
('F1111111-1111-1111-1111-111111111001',N'批量导入房屋','BATCH_IMPORT_ROOMS',N'批量导入房屋数据需要审批',1,@Cid,@Sys,@Now),
('F1111111-1111-1111-1111-111111111002',N'新建合同','CONTRACT_CREATE',N'新建租赁合同需要审批',1,@Cid,@Sys,@Now),
('F1111111-1111-1111-1111-111111111003',N'提前解约','CONTRACT_TERMINATE',N'合同提前终止需要审批',1,@Cid,@Sys,@Now),
('F1111111-1111-1111-1111-111111111004',N'收款冲销','RECEIPT_REVERSE',N'收款冲销操作需要审批',1,@Cid,@Sys,@Now),
('F1111111-1111-1111-1111-111111111005',N'应收减免','DISCOUNT',N'应收费用减免需要审批',1,@Cid,@Sys,@Now);

-- 获取角色实际 ID（动态查询，兼容不同环境）
DECLARE @OpsSup uniqueidentifier = (SELECT Id FROM Roles WHERE Code='OpsSupervisor');
DECLARE @DeptMgr uniqueidentifier = (SELECT Id FROM Roles WHERE Code='DeptManager');
DECLARE @GenMgr uniqueidentifier = (SELECT Id FROM Roles WHERE Code='GeneralManager');
DECLARE @FinSup uniqueidentifier = (SELECT Id FROM Roles WHERE Code='FinanceSupervisor');
DECLARE @FinDir uniqueidentifier = (SELECT Id FROM Roles WHERE Code='FinanceDirector');

-- 三级审批级别（15 条）
-- 批量导入房屋: 运营主管 → 部门经理 → 总经理
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES
('F2111111-1111-1111-1111-111111111001','F1111111-1111-1111-1111-111111111001',1,@OpsSup,NULL,NULL,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111002','F1111111-1111-1111-1111-111111111001',2,@DeptMgr,NULL,NULL,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111012','F1111111-1111-1111-1111-111111111001',3,@GenMgr,NULL,NULL,@Cid,@Sys,@Now);

-- 新建合同: 运营主管 → 部门经理 → 总经理
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES
('F2111111-1111-1111-1111-111111111003','F1111111-1111-1111-1111-111111111002',1,@OpsSup,NULL,NULL,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111013','F1111111-1111-1111-1111-111111111002',2,@DeptMgr,NULL,NULL,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111014','F1111111-1111-1111-1111-111111111002',3,@GenMgr,NULL,NULL,@Cid,@Sys,@Now);

-- 提前解约（金额分级）: 运营主管≤5k → 部门经理5k-50k → 总经理≥50k
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES
('F2111111-1111-1111-1111-111111111004','F1111111-1111-1111-1111-111111111003',1,@OpsSup,0,5000,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111005','F1111111-1111-1111-1111-111111111003',2,@DeptMgr,5000,50000,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111006','F1111111-1111-1111-1111-111111111003',3,@GenMgr,50000,99999999,@Cid,@Sys,@Now);

-- 收款冲销: 财务主管 → 财务总监 → 总经理
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES
('F2111111-1111-1111-1111-111111111007','F1111111-1111-1111-1111-111111111004',1,@FinSup,NULL,NULL,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111008','F1111111-1111-1111-1111-111111111004',2,@FinDir,NULL,NULL,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111015','F1111111-1111-1111-1111-111111111004',3,@GenMgr,NULL,NULL,@Cid,@Sys,@Now);

-- 应收减免: 财务主管 → 部门经理 → 总经理
INSERT INTO [ApprovalLevelConfigs] ([Id],[ApprovalTypeId],[Level],[RoleId],[MinAmount],[MaxAmount],[CompanyId],[CreatedBy],[CreatedAt])
VALUES
('F2111111-1111-1111-1111-111111111009','F1111111-1111-1111-1111-111111111005',1,@FinSup,NULL,NULL,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111010','F1111111-1111-1111-1111-111111111005',2,@DeptMgr,NULL,NULL,@Cid,@Sys,@Now),
('F2111111-1111-1111-1111-111111111011','F1111111-1111-1111-1111-111111111005',3,@GenMgr,NULL,NULL,@Cid,@Sys,@Now);

-- 验证
SELECT t.Name AS 审批类型, t.Code, COUNT(l.Id) AS 级别数
FROM ApprovalTypes t LEFT JOIN ApprovalLevelConfigs l ON l.ApprovalTypeId = t.Id
GROUP BY t.Name, t.Code ORDER BY t.Code;
GO
