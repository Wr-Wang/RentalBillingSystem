/**
 * =========================================================================
 *  API 接口声明
 *  集中管理所有后端接口的调用函数，按业务模块分组
 *
 *  调用规范：
 *    每个函数都返回 request(config) 的 Promise
 *    响应拦截器已剥壳 response.data，组件直接拿到业务 JSON
 *    错误由响应拦截器 + 组件 catch 中的 handleApiError 两级处理
 *
 *  模块索引：
 *    1. Auth                 行  4 — 登录/刷新/改密
 *    2. Companies            行 10 — 多公司管理
 *    3. Users                行 16 — 用户管理
 *    4. Roles                行 24 — 角色管理
 *    5. Menus                行 33 — 菜单管理
 *    6. HousingUnits         行 39 — 房源管理
 *    7. Imports              行 50 — 通用导入
 *    8. Tenants              行 54 — 租客管理
 *    9. ContractTenants      行 62 — 合同租客管理
 *   10. Contracts            行 68 — 合同管理（核心模块）
 *   11. FeeCodes             行 90 — 收费项目
 *   12. ContractFeeConfigs   行 96 — 合同费用配置
 *   13. MeterReadings        行 105 — 抄表
 *   14. Receivables          行 112 — 应收
 *   15. Receipts             行 118 — 收款
 *   16. Deposits             行 127 — 押金
 *   17. Collection           行 131 — 催缴
 *   18. Approvals            行 141 — 审批（核心模块）
 *   19. Accounting           行 162 — 会计（9 个子模块）
 *   20. Banking              行 185 — 银行对账
 *   21. Reports              行 195 — 报表
 *   22. DebitNotes           行 203 — 账单/催缴通知
 *   23. Holidays             行 210 — 节假日
 *   24. PaymentChannels      行 217 — 支付通道
 *   25. TaxRateConfigs       行 223 — 税率
 *   26. Scheduler            行 229 — 调度任务
 *   27. Audit                行 264 — 变更审计
 *   28. SystemLogs           行 270 — 系统日志
 *   29. Notifications        行 276 — 通知
 *   30. ApiLogs              行 282 — API 日志
 *   31. SystemConfig         行 288 — 系统配置
 * =========================================================================
 */
import request, { handleApiError } from './request'

// =========================================================================
// 1. Auth — 认证
// =========================================================================
/** 登录：提交凭证获取 token + user 信息 */
export function login(data) { return request({ url: '/auth/login', method: 'post', data }) }
/** 刷新 token：用 refreshToken 换取新 accessToken */
export function refreshToken(data) { return request({ url: '/auth/refresh', method: 'post', data }) }
/** 修改密码（需旧密码 + 新密码）*/
export function changePassword(data) { return request({ url: '/auth/changepassword', method: 'post', data }) }

// =========================================================================
// 2. Companies — 多公司管理
// =========================================================================
/** 公司列表（支持分页/搜索）*/
export function getCompanies(params) { return request({ url: '/companies', method: 'get', params }) }
/** 公司详情 */
export function getCompany(id) { return request({ url: `/companies/${id}`, method: 'get' }) }
/** 新建公司 */
export function createCompany(data) { return request({ url: '/companies', method: 'post', data }) }
/** 更新公司信息 */
export function updateCompany(id, data) { return request({ url: `/companies/${id}`, method: 'put', data }) }
/** 删除公司 */
export function deleteCompany(id) { return request({ url: `/companies/${id}`, method: 'delete' }) }
/** 公司统计（房间数/合同数/出租率等）*/
export function getCompanyStats(id) { return request({ url: `/companies/${id}/stats`, method: 'get' }) }
// =========================================================================
// 3. Users — 用户管理
// =========================================================================
/** 用户列表（支持分页/角色筛选）*/
/** 用户详情 */
export function getUsers(params) { return request({ url: '/users', method: 'get', params }) }
export function getUser(id) { return request({ url: `/users/${id}`, method: 'get' }) }
/** 创建用户 */
export function createUser(data) { return request({ url: '/users', method: 'post', data }) }
/** 更新用户信息 */
export function updateUser(id, data) { return request({ url: `/users/${id}`, method: 'put', data }) }
/** 删除用户 */
export function deleteUser(id) { return request({ url: `/users/${id}`, method: 'delete' }) }
/** 获取我的个人信息（含角色、权限、公司列表），页面刷新后加载 */
export function getMyProfile() { return request({ url: '/auth/me', method: 'get' }) }
/** 设置我的默认公司（超管切换视角时持久化到数据库）*/
export function setMyDefaultCompany(companyId) { return request({ url: '/users/me/defaultcompany', method: 'put', data: { companyId } }) }

// =========================================================================
// 4. Roles — 角色管理
// =========================================================================
/** 角色列表 */
/** 角色详情 */
export function getRoles(params) { return request({ url: '/roles', method: 'get', params }) }
export function getRole(id) { return request({ url: `/roles/${id}`, method: 'get' }) }
/** 创建角色 */
export function createRole(data) { return request({ url: '/roles', method: 'post', data }) }
/** 更新角色 */
export function updateRole(id, data) { return request({ url: `/roles/${id}`, method: 'put', data }) }
/** 删除角色 */
export function deleteRole(id) { return request({ url: `/roles/${id}`, method: 'delete' }) }
/** 获取角色的菜单权限 */
export function getRoleMenus(id) { return request({ url: `/roles/${id}/menus`, method: 'get' }) }
/** 更新角色的菜单权限（全量覆盖）*/
export function updateRoleMenus(id, data) { return request({ url: `/roles/${id}/menus`, method: 'post', data }) }

// =========================================================================
// 5. Menus — 菜单管理
// =========================================================================
/** 菜单树（按层级排列）*/
export function getMenus() { return request({ url: '/menus', method: 'get' }) }
/** 创建菜单项 */
export function createMenu(data) { return request({ url: '/menus', method: 'post', data }) }
/** 更新菜单 */
export function updateMenu(id, data) { return request({ url: `/menus/${id}`, method: 'put', data }) }
/** 删除菜单 */
export function deleteMenu(id) { return request({ url: `/menus/${id}`, method: 'delete' }) }

// =========================================================================
// 6. HousingUnits — 房源管理
// =========================================================================
/** 房源列表（支持分页/搜索/状态筛选）*/
/** 房源详情（含房间信息）*/
export function getHousingUnits(params) { return request({ url: '/housingunits', method: 'get', params }) }
export function getHousingUnit(id) { return request({ url: `/housingunits/${id}`, method: 'get' }) }
/** 新建房源 */
export function createHousingUnit(data) { return request({ url: '/housingunits', method: 'post', data }) }
/** 更新房源 */
export function updateHousingUnit(id, data) { return request({ url: `/housingunits/${id}`, method: 'put', data }) }
/** 删除房源 */
export function deleteHousingUnit(id) { return request({ url: `/housingunits/${id}`, method: 'delete' }) }
/** 房源树（座→层→房）*/
export function getHousingUnitTree() { return request({ url: '/housingunits/tree', method: 'get' }) }
/** 座楼列表（下拉选择器用）*/
export function getBuildingList() { return request({ url: '/housingunits/buildinglist', method: 'get' }) }
/** 房源统计仪表盘 */
export function getHousingUnitStats() { return request({ url: '/housingunits/stats', method: 'get' }) }
/** 批量导入房源（Excel）*/
export function importHousingUnits(data) { return request({ url: '/housingunits/import', method: 'post', data }) }

// =========================================================================
// 7. Imports — 通用导入（审批驱动）
// =========================================================================
/** 提交导入审批 */
export function submitImport(data) { return request({ url: '/imports/submit', method: 'post', data }) }
/** 获取导入批次详情（含行数据）*/
export function getImportBatch(id) { return request({ url: `/imports/${id}`, method: 'get' }) }
/** 导入批次列表 */
export function getImportBatches(params) { return request({ url: '/imports', method: 'get', params }) }

// =========================================================================
// 8. Tenants — 租客管理
// =========================================================================
/** 租客列表 */
/** 租客详情（含房屋和月租金）*/
export function getTenants(params) { return request({ url: '/tenants', method: 'get', params }) }
export function getTenant(id) { return request({ url: `/tenants/${id}`, method: 'get' }) }
/** 新建租客 */
export function createTenant(data) { return request({ url: '/tenants', method: 'post', data }) }
/** 更新租客 */
export function updateTenant(id, data) { return request({ url: `/tenants/${id}`, method: 'put', data }) }
/** 删除租客 */
export function deleteTenant(id) { return request({ url: `/tenants/${id}`, method: 'delete' }) }

// 合同租客管理
export function getContractTenants(contractId) { return request({ url: `/contracts/${contractId}/tenants`, method: 'get' }) }
export function addContractTenant(contractId, data) { return request({ url: `/contracts/${contractId}/tenants`, method: 'post', data }) }
export function removeContractTenant(contractId, tenantId, data) { return request({ url: `/contracts/${contractId}/tenants/${tenantId}`, method: 'delete', data }) }
export function setContractPrimaryTenant(contractId, tenantId) { return request({ url: `/contracts/${contractId}/tenants/${tenantId}/primary`, method: 'put' }) }

// Contracts
export function getContracts(params) { return request({ url: '/contracts', method: 'get', params }) }
export function getContract(id) { return request({ url: `/contracts/${id}`, method: 'get' }) }
export function createContract(data) { return request({ url: '/contracts', method: 'post', data }) }
export function submitContractCreateRequest(data) { return request({ url: '/contracts/createrequest', method: 'post', data }) }
export function updateContract(id, data) { return request({ url: `/contracts/${id}`, method: 'put', data }) }
export function terminateContract(id, data) { return request({ url: `/contracts/${id}/terminate`, method: 'post', data }) }
export function feeAdjust(id, data) { return request({ url: `/contracts/${id}/feeadjust`, method: 'post', data }) }
export function renewContract(id, data) { return request({ url: `/contracts/${id}/renew`, method: 'post', data }) }
export function previewRenewal(id) { return request({ url: `/contracts/${id}/renewal/preview`, method: 'get' }) }
export function getLastRejectedRenewal(id) { return request({ url: `/contracts/${id}/renewal/lastrejected`, method: 'get' }) }
export function getLastRejectedApproval(params) { return request({ url: '/approvals/lastrejected', method: 'get', params }) }
export function submitRenewal(id, data) { return request({ url: `/contracts/${id}/renewal/submit`, method: 'post', data }) }
export function getRenewalHistory(id) { return request({ url: `/contracts/${id}/renewal/history`, method: 'get' }) }
export function getRenewalChain(id) { return request({ url: `/contracts/${id}/renewal/chain`, method: 'get' }) }
export function getAllowedOperations(id) { return request({ url: `/contracts/${id}/allowedoperations`, method: 'get' }) }
export function getContractTimeline(id) { return request({ url: `/contracts/${id}/timeline`, method: 'get' }) }
export function getContractChanges(id) { return request({ url: `/contracts/${id}/changes`, method: 'get' }) }
export function submitContractModify(id, data) { return request({ url: `/contracts/${id}/modifysubmit`, method: 'post', data }) }
export function addSupplementaryFee(id, data) { return request({ url: `/contracts/${id}/supplementaryfee`, method: 'post', data }) }

// Fee Codes
export function getFeeCodes(params) { return request({ url: '/feecodes', method: 'get', params }) }
export function createFeeCode(data) { return request({ url: '/feecodes', method: 'post', data }) }
export function updateFeeCode(id, data) { return request({ url: `/feecodes/${id}`, method: 'put', data }) }
export function deleteFeeCode(id) { return request({ url: `/feecodes/${id}`, method: 'delete' }) }

// Contract Fee Configs
export function getContractFeeConfigs(contractId) { return request({ url: `/contractfeeconfigs?contractId=${contractId}`, method: 'get' }) }
export function createContractFeeConfig(data) { return request({ url: '/contractfeeconfigs', method: 'post', data }) }
export function updateContractFeeConfig(id, data) { return request({ url: `/contractfeeconfigs/${id}`, method: 'put', data }) }
export function deleteContractFeeConfig(id) { return request({ url: `/contractfeeconfigs/${id}`, method: 'delete' }) }
export function adjustContractFeeConfig(data) { return request({ url: '/contractfeeconfigs/adjust', method: 'post', data }) }
export function getContractFeeConfigHistory(contractId, feeCodeId) { return request({ url: `/contractfeeconfigs/history?contractId=${contractId}&feeCodeId=${feeCodeId}`, method: 'get' }) }
export function checkFeeConfigOverlap(data) { return request({ url: '/contractfeeconfigs/checkoverlap', method: 'post', data }) }

// Meter Readings
export function getMeterReadings(params) { return request({ url: '/meterreadings', method: 'get', params }) }
export function createMeterReading(data) { return request({ url: '/meterreadings', method: 'post', data }) }
export function updateMeterReading(id, data) { return request({ url: `/meterreadings/${id}`, method: 'put', data }) }
export function confirmMeterReading(id) { return request({ url: `/meterreadings/${id}/confirm`, method: 'post' }) }
export function importMeterReadings(data) { return request({ url: '/meterreadings/import', method: 'post', data }) }

// Journals（日记账）
export function getJournals(params) { return request({ url: '/journals', method: 'get', params }) }
export function getJournal(id) { return request({ url: `/journals/${id}`, method: 'get' }) }
export function getJournalsByContract(contractId) { return request({ url: `/journals/bycontract?contractId=${contractId}`, method: 'get' }) }
export function generateJournals(data) { return request({ url: '/journals/generate', method: 'post', data }) }
export function previewJournals(data) { return request({ url: '/journals/preview', method: 'post', data }) }
export function generateJournalRequest(data) { return request({ url: '/journals/generaterequest', method: 'post', data }) }
export function postJournals(ids) { return request({ url: '/journals/post', method: 'post', data: ids }) }

// GL（总账）
export function getGLBalance(params) { return request({ url: '/gl', method: 'get', params }) }
export function getTrialBalance(params) { return request({ url: '/gl/trialbalance', method: 'get', params }) }

// Prepaid（预收明细）
export function getPrepaids(params) { return request({ url: '/prepaids', method: 'get', params }) }
export function getGLDetail(params) { return request({ url: '/gl/detail', method: 'get', params }) }

// Receipts
export function getReceipts(params) { return request({ url: '/receipts', method: 'get', params }) }
export function createReceipt(data) { return request({ url: '/receipts', method: 'post', data }) }
export function confirmReceipt(id) { return request({ url: `/receipts/${id}/confirm`, method: 'put' }) }
export function rejectReceipt(id, data) { return request({ url: `/receipts/${id}/reject`, method: 'put', data }) }
export function reverseReceipt(id, data) { return request({ url: `/receipts/${id}/reverse`, method: 'post', data }) }
export function batchConfirmReceipts(data) { return request({ url: '/receipts/batchconfirm', method: 'post', data }) }

// Deposits
export function getDeposits(params) { return request({ url: '/deposits', method: 'get', params }) }
export function refundDeposit(data) { return request({ url: '/deposits/refund', method: 'post', data }) }
export function deductDeposit(data) { return request({ url: '/deposits/deduct', method: 'post', data }) }

// Collection
export function getCollectionOverview() { return request({ url: '/collection/overview', method: 'get' }) }
export function getCollectionStages() { return request({ url: '/collectionstages', method: 'get' }) }
export function createCollectionStage(data) { return request({ url: '/collectionstages', method: 'post', data }) }
export function updateCollectionStage(id, data) { return request({ url: `/collectionstages/${id}`, method: 'put', data }) }
export function deleteCollectionStage(id) { return request({ url: `/collectionstages/${id}`, method: 'delete' }) }
export function getCollectionRecords(params) { return request({ url: '/collectionrecords', method: 'get', params }) }
export function manualCollection(data) { return request({ url: '/collectionrecords/manual', method: 'post', data }) }

// Approvals
export function submitApproval(data) { return request({ url: '/approvals/submit', method: 'post', data }) }
export function getPendingApprovals() { return request({ url: '/approvals/pending', method: 'get' }) }
export function getMyApprovalRequests() { return request({ url: '/approvals/myrequests', method: 'get' }) }
export function approveApproval(id, data) { return request({ url: `/approvals/${id}/approve`, method: 'post', data }) }
export function rejectApproval(id, data) { return request({ url: `/approvals/${id}/reject`, method: 'post', data }) }
export function getApprovalDetail(id) { return request({ url: `/approvals/${id}`, method: 'get' }) }
export function getApprovalBizDetail(id) { return request({ url: `/approvals/${id}/bizdetail`, method: 'get', validateStatus: s => s < 500 }).catch(() => null) }
export function getApprovalHistoryList(params) { return request({ url: '/approvals/history', method: 'get', params }) }
export function getApprovalHistory(id) { return request({ url: `/approvals/${id}/history`, method: 'get' }) }
export function cancelApproval(id, data) { return request({ url: `/approvals/${id}/cancel`, method: 'post', data }) }
export function retryApprovalCallback(id) { return request({ url: `/approvals/${id}/retrycallback`, method: 'post' }) }
export function getApprovalTypes() { return request({ url: '/approvaltypes', method: 'get' }) }
export function createApprovalType(data) { return request({ url: '/approvaltypes', method: 'post', data }) }
export function updateApprovalType(id, data) { return request({ url: `/approvaltypes/${id}`, method: 'put', data }) }
export function deleteApprovalType(id) { return request({ url: `/approvaltypes/${id}`, method: 'delete' }) }
export function getApprovalLevels(typeId) { return request({ url: `/approvaltypes/${typeId}/levels`, method: 'get' }) }
export function createApprovalLevel(typeId, data) { return request({ url: `/approvaltypes/${typeId}/levels`, method: 'post', data }) }
export function updateApprovalLevel(id, data) { return request({ url: `/approvallevels/${id}`, method: 'put', data }) }
export function deleteApprovalLevel(id) { return request({ url: `/approvallevels/${id}`, method: 'delete' }) }

// Accounting
export function getAccountingSubjects(params) { return request({ url: '/accountingsubjects', method: 'get', params }) }
export function createAccountingSubject(data) { return request({ url: '/accountingsubjects', method: 'post', data }) }
export function updateAccountingSubject(id, data) { return request({ url: `/accountingsubjects/${id}`, method: 'put', data }) }
export function deleteAccountingSubject(id) { return request({ url: `/accountingsubjects/${id}`, method: 'delete' }) }
export function getLedger(params) { return request({ url: '/ledger', method: 'get', params }) }
export function getBalanceSheet(params) { return request({ url: '/financialstatements/balancesheet', method: 'get', params }) }
export function getIncomeStatement(params) { return request({ url: '/financialstatements/incomestatement', method: 'get', params }) }
export function getAccountingPeriods() { return request({ url: '/accountingperiods', method: 'get' }) }
export function openAccountingPeriod(data) { return request({ url: '/accountingperiods', method: 'post', data }) }
export function closeAccountingPeriod(id) { return request({ url: `/accountingperiods/${id}/close`, method: 'put' }) }
export function reopenAccountingPeriod(id) { return request({ url: `/accountingperiods/${id}/reopen`, method: 'put' }) }
export function lockAccountingPeriod(id) { return request({ url: `/accountingperiods/${id}/lock`, method: 'put' }) }

// Banking
export function getBankStatements(params) { return request({ url: '/banking/statements', method: 'get', params }) }
export function importBankStatements(data) { return request({ url: '/banking/statements/import', method: 'post', data }) }
export function getBankReconciliations(params) { return request({ url: '/banking/reconciliations', method: 'get', params }) }
export function createBankReconciliation(data) { return request({ url: '/banking/reconciliations', method: 'post', data }) }
export function autoMatchBank(id) { return request({ url: `/banking/reconciliations/${id}/automatch`, method: 'post' }) }
export function completeReconciliation(id) { return request({ url: `/banking/reconciliations/${id}/complete`, method: 'post' }) }
export function manualMatchBank(data) { return request({ url: '/banking/matches', method: 'post', data }) }
export function getBankMatches(params) { return request({ url: '/banking/matches', method: 'get', params }) }

// Reports
export function getCollectionRate(params) { return request({ url: '/reports/collectionrate', method: 'get', params }) }
export function getOverdueDetail(params) { return request({ url: '/reports/overduedetail', method: 'get', params }) }
export function getDailyReceipt(params) { return request({ url: '/reports/dailyreceipt', method: 'get', params }) }
export function getMonthlyReceipt(params) { return request({ url: '/reports/monthlyreceipt', method: 'get', params }) }
export function getFeeRevenue(params) { return request({ url: '/reports/feerevenue', method: 'get', params }) }
export function getOccupancyRate(params) { return request({ url: '/reports/occupancyrate', method: 'get', params }) }

// Debit Notes (Bills)
export function getDebitNotes(params) { return request({ url: '/debitnotes', method: 'get', params }) }
export function getDebitNote(id) { return request({ url: `/debitnotes/${id}`, method: 'get' }) }
export function generateDebitNotes(data) { return request({ url: '/debitnotes/generate', method: 'post', data }) }
export function exportDebitNotePdf(id) { return request({ url: `/debitnotes/${id}/pdf`, method: 'get', responseType: 'blob' }) }


// Holidays
export function getHolidayCalendars(params) { return request({ url: '/holidaycalendars', method: 'get', params }) }
export function createHolidayCalendar(data) { return request({ url: '/holidaycalendars', method: 'post', data }) }
export function updateHolidayCalendar(id, data) { return request({ url: `/holidaycalendars/${id}`, method: 'put', data }) }
export function deleteHolidayCalendar(id) { return request({ url: `/holidaycalendars/${id}`, method: 'delete' }) }
export function importHolidayYear(year) { return request({ url: `/holidaycalendars/import/${year}`, method: 'post' }) }

// Payment Channels
export function getPaymentChannels() { return request({ url: '/paymentchannels', method: 'get' }) }
export function createPaymentChannel(data) { return request({ url: '/paymentchannels', method: 'post', data }) }
export function updatePaymentChannel(id, data) { return request({ url: `/paymentchannels/${id}`, method: 'put', data }) }
export function deletePaymentChannel(id) { return request({ url: `/paymentchannels/${id}`, method: 'delete' }) }

// Tax Rate Configs
export function getTaxRateConfigs() { return request({ url: '/taxrateconfigs', method: 'get' }) }
export function createTaxRateConfig(data) { return request({ url: '/taxrateconfigs', method: 'post', data }) }
export function updateTaxRateConfig(id, data) { return request({ url: `/taxrateconfigs/${id}`, method: 'put', data }) }
export function deleteTaxRateConfig(id) { return request({ url: '/taxrateconfigs/' + id, method: 'delete' }) }

// Scheduler
export function getSchedulerJobs() { return request({ url: '/scheduler/jobs', method: 'get' }) }
export function createSchedulerJob(data) { return request({ url: '/scheduler/jobs', method: 'post', data }) }
export function updateSchedulerJob(id, data) { return request({ url: '/scheduler/jobs/' + id, method: 'put', data }) }
export function deleteSchedulerJob(id) { return request({ url: '/scheduler/jobs/' + id, method: 'delete' }) }

// Scheduler — Templates & Executions
export function getSchedulerTemplates() { return request({ url: '/scheduler/templates', method: 'get' }) }
export function getExecutions(jobId, params) { return request({ url: `/scheduler/jobs/${jobId}/executions`, method: 'get', params }) }
export function getExecution(jobId, id) { return request({ url: `/scheduler/jobs/${jobId}/executions/${id}`, method: 'get' }) }
export function createExecution(jobId, data) { return request({ url: `/scheduler/jobs/${jobId}/executions`, method: 'post', data }) }
export function updateExecution(jobId, id, data) { return request({ url: `/scheduler/jobs/${jobId}/executions/${id}`, method: 'put', data }) }
export function deleteExecution(jobId, id) { return request({ url: `/scheduler/jobs/${jobId}/executions/${id}`, method: 'delete' }) }
export function generateExecutions(jobId) { return request({ url: `/scheduler/jobs/${jobId}/executions/generate`, method: 'post' }) }
export function deleteFutureExecutions(jobId) { return request({ url: `/scheduler/jobs/${jobId}/executions/future`, method: 'delete' }) }
export function executeJob(jobName, data) { return request({ url: `/scheduler/execute/${jobName}`, method: 'post', data }) }
export function getTaskLogs(taskName, params) { return request({ url: `/scheduler/tasklogs`, method: 'get', params }) }
export function getTaskLog(id) { return request({ url: `/scheduler/tasklogs/${id}`, method: 'get' }) }
export function getTaskSteps(id) { return request({ url: `/scheduler/tasklogs/${id}/steps`, method: 'get' }) }
export function reverseTask(taskLogId, data) { return request({ url: `/scheduler/reverse/${taskLogId}`, method: 'post', data }) }
export function retryExecution(jobId, id) { return request({ url: `/scheduler/jobs/${jobId}/executions/${id}/retry`, method: 'post' }) }
export function skipExecution(jobId, id, data) { return request({ url: `/scheduler/jobs/${jobId}/executions/${id}/skip`, method: 'post', data }) }
export function pauseExecution(jobId, id, data) { return request({ url: `/scheduler/jobs/${jobId}/executions/${id}/pause`, method: 'post', data }) }
export function cancelExecution(jobId, id, data) { return request({ url: `/scheduler/jobs/${jobId}/executions/${id}/cancel`, method: 'post', data }) }
export function resumeExecution(jobId, id, data) { return request({ url: `/scheduler/jobs/${jobId}/executions/${id}/resume`, method: 'post', data }) }

// Scheduler — Monitor
export function getMonitorDashboard() { return request({ url: '/monitor/dashboard', method: 'get' }) }
export function getMonitorTrend(days) { return request({ url: '/monitor/dashboard/trend', method: 'get', params: { days } }) }
export function getMonitorDuration(days) { return request({ url: '/monitor/dashboard/duration', method: 'get', params: { days } }) }
export function getMonitorFailures(days) { return request({ url: '/monitor/dashboard/failures', method: 'get', params: { days } }) }
export function queryMonitorLogs(params) { return request({ url: '/monitor/logs', method: 'get', params }) }
export function getMonitorLogDetail(id) { return request({ url: `/monitor/logs/${id}`, method: 'get' }) }
export function previewReverse(id) { return request({ url: `/monitor/logs/${id}/previewreverse`, method: 'post' }) }

// Audit
export function getAuditHistory(tableName, params) { return request({ url: `/audit/${tableName}/history`, method: 'get', params }) }
export function compareAuditVersions(tableName, recordId, v1, v2) { return request({ url: `/audit/${tableName}/compare`, method: 'get', params: { recordId, v1, v2 } }) }
export function rollbackAudit(tableName, recordId, versionNo) { return request({ url: `/audit/${tableName}/rollback`, method: 'post', params: { recordId, versionNo } }) }
export function getAuditStats(params) { return request({ url: '/audit/stats', method: 'get', params }) }

// System Logs
export function getSystemLogs(params) { return request({ url: '/systemlogs', method: 'get', params }) }
export function getSystemLog(id) { return request({ url: `/systemlogs/${id}`, method: 'get' }) }
export function deleteSystemLog(id) { return request({ url: `/systemlogs/${id}`, method: 'delete' }) }
export function clearSystemLogs() { return request({ url: '/systemlogs', method: 'delete' }) }

// Notifications
export function getNotifications(params) { return request({ url: '/notifications', method: 'get', params }) }
export function getUnreadCounts() { return request({ url: '/notifications/unreadcounts', method: 'get' }) }
export function markNotificationRead(id) { return request({ url: `/notifications/${id}/read`, method: 'put' }) }
export function markAllNotificationsRead() { return request({ url: '/notifications/readall', method: 'put' }) }

// Api Logs
export function getApiLogs(params) { return request({ url: '/apilogs', method: 'get', params }) }
export function getApiLog(id) { return request({ url: `/apilogs/${id}`, method: 'get' }) }
export function deleteApiLog(id) { return request({ url: `/apilogs/${id}`, method: 'delete' }) }
export function clearApiLogs(params) { return request({ url: '/apilogs', method: 'delete', params }) }

// Room Types
export function getRoomTypes() { return request({ url: '/roomtypes', method: 'get' }) }
export function createRoomType(data) { return request({ url: '/roomtypes', method: 'post', data }) }
export function updateRoomType(id, data) { return request({ url: `/roomtypes/${id}`, method: 'put', data }) }
export function deleteRoomType(id) { return request({ url: `/roomtypes/${id}`, method: 'delete' }) }

// Pricing Standards
export function getPricingStandards(params) { return request({ url: '/roompricingstandards', method: 'get', params }) }
export function createPricingStandard(data) { return request({ url: '/roompricingstandards', method: 'post', data }) }
export function updatePricingStandard(id, data) { return request({ url: `/roompricingstandards/${id}`, method: 'put', data }) }
export function deletePricingStandard(id) { return request({ url: `/roompricingstandards/${id}`, method: 'delete' }) }

// Floor Level Bands
export function getFloorLevelBands() { return request({ url: '/floorlevelbands', method: 'get' }) }
export function createFloorLevelBand(data) { return request({ url: '/floorlevelbands', method: 'post', data }) }
export function updateFloorLevelBand(id, data) { return request({ url: '/floorlevelbands/' + id, method: 'put', data }) }
export function deleteFloorLevelBand(id) { return request({ url: '/floorlevelbands/' + id, method: 'delete' }) }

// Late Fee Config
export function getInterestConfigs() { return request({ url: '/interestconfig', method: 'get' }) }
export function getActiveInterestConfig() { return request({ url: '/interestconfig/active', method: 'get' }) }
export function saveInterestConfig(data) { return request({ url: '/interestconfig', method: 'post', data }) }
export function deleteInterestConfig(id) { return request({ url: `/interestconfig/${id}`, method: 'delete' }) }

// AutoRenew Config
export function getAutoRenewConfig(companyId) { return request({ url: '/autorenewconfig', method: 'get', params: { companyId } }) }
export function saveAutoRenewConfig(data) { return request({ url: '/autorenewconfig', method: 'post', data }) }

export { handleApiError }

// Change Requests
