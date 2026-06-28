import request from './request'

// Auth
export function login(data) { return request({ url: '/auth/login', method: 'post', data }) }
export function refreshToken(data) { return request({ url: '/auth/refresh', method: 'post', data }) }
export function changePassword(data) { return request({ url: '/auth/change-password', method: 'post', data }) }

// Companies (多公司管理)
export function getCompanies(params) { return request({ url: '/companies', method: 'get', params }) }
export function getCompany(id) { return request({ url: `/companies/${id}`, method: 'get' }) }
export function createCompany(data) { return request({ url: '/companies', method: 'post', data }) }
export function updateCompany(id, data) { return request({ url: `/companies/${id}`, method: 'put', data }) }
export function deleteCompany(id) { return request({ url: `/companies/${id}`, method: 'delete' }) }
export function getCompanyStats(id) { return request({ url: `/companies/${id}/stats`, method: 'get' }) }
export function getUserCompanyScope(userId) { return request({ url: `/users/${userId}/company-scope`, method: 'get' }) }
export function updateUserCompanyScope(userId, data) { return request({ url: `/users/${userId}/company-scope`, method: 'put', data }) }

// Users
export function getUsers(params) { return request({ url: '/users', method: 'get', params }) }
export function getUser(id) { return request({ url: `/users/${id}`, method: 'get' }) }
export function createUser(data) { return request({ url: '/users', method: 'post', data }) }
export function updateUser(id, data) { return request({ url: `/users/${id}`, method: 'put', data }) }
export function deleteUser(id) { return request({ url: `/users/${id}`, method: 'delete' }) }

// Roles
export function getRoles(params) { return request({ url: '/roles', method: 'get', params }) }
export function getRole(id) { return request({ url: `/roles/${id}`, method: 'get' }) }
export function createRole(data) { return request({ url: '/roles', method: 'post', data }) }
export function updateRole(id, data) { return request({ url: `/roles/${id}`, method: 'put', data }) }
export function deleteRole(id) { return request({ url: `/roles/${id}`, method: 'delete' }) }
export function getRoleMenus(id) { return request({ url: `/roles/${id}/menus`, method: 'get' }) }
export function updateRoleMenus(id, data) { return request({ url: `/roles/${id}/menus`, method: 'post', data }) }

// Menus
export function getMenus() { return request({ url: '/menus', method: 'get' }) }
export function createMenu(data) { return request({ url: '/menus', method: 'post', data }) }
export function updateMenu(id, data) { return request({ url: `/menus/${id}`, method: 'put', data }) }
export function deleteMenu(id) { return request({ url: `/menus/${id}`, method: 'delete' }) }

// Buildings
export function getBuildings() { return request({ url: '/buildings', method: 'get' }) }
export function getBuilding(id) { return request({ url: `/buildings/${id}`, method: 'get' }) }
export function createBuilding(data) { return request({ url: '/buildings', method: 'post', data }) }
export function updateBuilding(id, data) { return request({ url: `/buildings/${id}`, method: 'put', data }) }
export function deleteBuilding(id) { return request({ url: `/buildings/${id}`, method: 'delete' }) }

// Floors
export function getFloors(buildingId) { return request({ url: `/buildings/${buildingId}/floors`, method: 'get' }) }
export function createFloor(buildingId, data) { return request({ url: `/buildings/${buildingId}/floors`, method: 'post', data }) }
export function updateFloor(id, data) { return request({ url: `/floors/${id}`, method: 'put', data }) }
export function deleteFloor(id) { return request({ url: `/floors/${id}`, method: 'delete' }) }

// Rooms
export function getRooms(params) { return request({ url: '/rooms', method: 'get', params }) }
export function getRoom(id) { return request({ url: `/rooms/${id}`, method: 'get' }) }
export function createRoom(data) { return request({ url: '/rooms', method: 'post', data }) }
export function updateRoom(id, data) { return request({ url: `/rooms/${id}`, method: 'put', data }) }
export function deleteRoom(id) { return request({ url: `/rooms/${id}`, method: 'delete' }) }
export function getRoomTree() { return request({ url: '/rooms/tree', method: 'get' }) }
export function importRooms(data) { return request({ url: '/rooms/import', method: 'post', data }) }

// Tenants
export function getTenants(params) { return request({ url: '/tenants', method: 'get', params }) }
export function getTenant(id) { return request({ url: `/tenants/${id}`, method: 'get' }) }
export function createTenant(data) { return request({ url: '/tenants', method: 'post', data }) }
export function updateTenant(id, data) { return request({ url: `/tenants/${id}`, method: 'put', data }) }
export function deleteTenant(id) { return request({ url: `/tenants/${id}`, method: 'delete' }) }

// Contracts
export function getContracts(params) { return request({ url: '/contracts', method: 'get', params }) }
export function getContract(id) { return request({ url: `/contracts/${id}`, method: 'get' }) }
export function createContract(data) { return request({ url: '/contracts', method: 'post', data }) }
export function updateContract(id, data) { return request({ url: `/contracts/${id}`, method: 'put', data }) }
export function terminateContract(id, data) { return request({ url: `/contracts/${id}/terminate`, method: 'post', data }) }
export function renewContract(id, data) { return request({ url: `/contracts/${id}/renew`, method: 'post', data }) }
export function suspendContract(id) { return request({ url: `/contracts/${id}/suspend`, method: 'post' }) }
export function resumeContract(id) { return request({ url: `/contracts/${id}/resume`, method: 'post' }) }
export function getContractTimeline(id) { return request({ url: `/contracts/${id}/timeline`, method: 'get' }) }

// Fee Codes
export function getFeeCodes(params) { return request({ url: '/fee-codes', method: 'get', params }) }
export function createFeeCode(data) { return request({ url: '/fee-codes', method: 'post', data }) }
export function updateFeeCode(id, data) { return request({ url: `/fee-codes/${id}`, method: 'put', data }) }
export function deleteFeeCode(id) { return request({ url: `/fee-codes/${id}`, method: 'delete' }) }

// Contract Fee Configs
export function getContractFeeConfigs(contractId) { return request({ url: `/contract-fee-configs?contractId=${contractId}`, method: 'get' }) }
export function updateContractFeeConfig(id, data) { return request({ url: `/contract-fee-configs/${id}`, method: 'put', data }) }

// Meter Readings
export function getMeterReadings(params) { return request({ url: '/meter-readings', method: 'get', params }) }
export function createMeterReading(data) { return request({ url: '/meter-readings', method: 'post', data }) }
export function updateMeterReading(id, data) { return request({ url: `/meter-readings/${id}`, method: 'put', data }) }
export function confirmMeterReading(id) { return request({ url: `/meter-readings/${id}/confirm`, method: 'post' }) }
export function importMeterReadings(data) { return request({ url: '/meter-readings/import', method: 'post', data }) }

// Receivables
export function getReceivables(params) { return request({ url: '/receivables', method: 'get', params }) }
export function getReceivable(id) { return request({ url: `/receivables/${id}`, method: 'get' }) }
export function generateReceivables(data) { return request({ url: '/receivables/generate', method: 'post', data }) }

// Receipts
export function getReceipts(params) { return request({ url: '/receipts', method: 'get', params }) }
export function createReceipt(data) { return request({ url: '/receipts', method: 'post', data }) }
export function confirmReceipt(id) { return request({ url: `/receipts/${id}/confirm`, method: 'put' }) }
export function rejectReceipt(id, data) { return request({ url: `/receipts/${id}/reject`, method: 'put', data }) }
export function reverseReceipt(id, data) { return request({ url: `/receipts/${id}/reverse`, method: 'post', data }) }
export function refundReceipt(id, data) { return request({ url: `/receipts/${id}/refund`, method: 'post', data }) }
export function batchConfirmReceipts(data) { return request({ url: '/receipts/batch-confirm', method: 'post', data }) }

// Deposits
export function getDeposits(params) { return request({ url: '/deposits', method: 'get', params }) }
export function refundDeposit(data) { return request({ url: '/deposits/refund', method: 'post', data }) }
export function deductDeposit(data) { return request({ url: '/deposits/deduct', method: 'post', data }) }

// Collection
export function getCollectionOverview() { return request({ url: '/collection/overview', method: 'get' }) }
export function getCollectionStages() { return request({ url: '/collection-stages', method: 'get' }) }
export function updateCollectionStage(id, data) { return request({ url: `/collection-stages/${id}`, method: 'put', data }) }
export function getCollectionRecords(params) { return request({ url: '/collection-records', method: 'get', params }) }
export function manualCollection(data) { return request({ url: '/collection-records/manual', method: 'post', data }) }

// Approvals
export function submitApproval(data) { return request({ url: '/approvals/submit', method: 'post', data }) }
export function getPendingApprovals() { return request({ url: '/approvals/pending', method: 'get' }) }
export function getMyApprovalRequests() { return request({ url: '/approvals/my-requests', method: 'get' }) }
export function approveApproval(id, data) { return request({ url: `/approvals/${id}/approve`, method: 'post', data }) }
export function rejectApproval(id, data) { return request({ url: `/approvals/${id}/reject`, method: 'post', data }) }
export function getApprovalHistory(id) { return request({ url: `/approvals/${id}/history`, method: 'get' }) }
export function retryApprovalCallback(id) { return request({ url: `/approvals/${id}/retry-callback`, method: 'post' }) }
export function getApprovalTypes() { return request({ url: '/approval-types', method: 'get' }) }
export function createApprovalType(data) { return request({ url: '/approval-types', method: 'post', data }) }
export function updateApprovalType(id, data) { return request({ url: `/approval-types/${id}`, method: 'put', data }) }
export function deleteApprovalType(id) { return request({ url: `/approval-types/${id}`, method: 'delete' }) }
export function getApprovalLevels(typeId) { return request({ url: `/approval-types/${typeId}/levels`, method: 'get' }) }
export function createApprovalLevel(typeId, data) { return request({ url: `/approval-types/${typeId}/levels`, method: 'post', data }) }
export function updateApprovalLevel(id, data) { return request({ url: `/approval-levels/${id}`, method: 'put', data }) }
export function deleteApprovalLevel(id) { return request({ url: `/approval-levels/${id}`, method: 'delete' }) }

// Accounting
export function getAccountingSubjects(params) { return request({ url: '/accounting-subjects', method: 'get', params }) }
export function createAccountingSubject(data) { return request({ url: '/accounting-subjects', method: 'post', data }) }
export function updateAccountingSubject(id, data) { return request({ url: `/accounting-subjects/${id}`, method: 'put', data }) }
export function deleteAccountingSubject(id) { return request({ url: `/accounting-subjects/${id}`, method: 'delete' }) }
export function getJournalEntries(params) { return request({ url: '/journal-entries', method: 'get', params }) }
export function getVouchers(params) { return request({ url: '/vouchers', method: 'get', params }) }
export function getVoucher(id) { return request({ url: `/vouchers/${id}`, method: 'get' }) }
export function postVoucher(id) { return request({ url: `/vouchers/${id}/post`, method: 'put' }) }
export function reverseVoucher(id, data) { return request({ url: `/vouchers/${id}/reverse`, method: 'post', data }) }
export function getTrialBalance(params) { return request({ url: '/trial-balance', method: 'get', params }) }

// Reports
export function getCollectionRate(params) { return request({ url: '/reports/collection-rate', method: 'get', params }) }
export function getOverdueDetail(params) { return request({ url: '/reports/overdue-detail', method: 'get', params }) }
export function getDailyReceipt(params) { return request({ url: '/reports/daily-receipt', method: 'get', params }) }
export function getMonthlyReceipt(params) { return request({ url: '/reports/monthly-receipt', method: 'get', params }) }
export function getFeeRevenue(params) { return request({ url: '/reports/fee-revenue', method: 'get', params }) }
export function getOccupancyRate(params) { return request({ url: '/reports/occupancy-rate', method: 'get', params }) }

// Debit Notes (Bills)
export function getDebitNotes(params) { return request({ url: '/debit-notes', method: 'get', params }) }
export function getDebitNote(id) { return request({ url: `/debit-notes/${id}`, method: 'get' }) }
export function generateDebitNotes(data) { return request({ url: '/debit-notes/generate', method: 'post', data }) }
export function exportDebitNotePdf(id) { return request({ url: `/debit-notes/${id}/pdf`, method: 'get', responseType: 'blob' }) }

// Scheduler
export function getSchedulerJobs() { return request({ url: '/scheduler/jobs', method: 'get' }) }
export function triggerSchedulerJob(name) { return request({ url: `/scheduler/jobs/${name}/trigger`, method: 'post' }) }

// Holidays
export function getHolidayCalendars(params) { return request({ url: '/holiday-calendars', method: 'get', params }) }
export function createHolidayCalendar(data) { return request({ url: '/holiday-calendars', method: 'post', data }) }
export function updateHolidayCalendar(id, data) { return request({ url: `/holiday-calendars/${id}`, method: 'put', data }) }
export function deleteHolidayCalendar(id) { return request({ url: `/holiday-calendars/${id}`, method: 'delete' }) }

// Payment Channels
export function getPaymentChannels() { return request({ url: '/payment-channels', method: 'get' }) }
export function createPaymentChannel(data) { return request({ url: '/payment-channels', method: 'post', data }) }
export function updatePaymentChannel(id, data) { return request({ url: `/payment-channels/${id}`, method: 'put', data }) }
export function deletePaymentChannel(id) { return request({ url: `/payment-channels/${id}`, method: 'delete' }) }

// Tax Rate Configs
export function getTaxRateConfigs() { return request({ url: '/tax-rate-configs', method: 'get' }) }
export function createTaxRateConfig(data) { return request({ url: '/tax-rate-configs', method: 'post', data }) }
export function updateTaxRateConfig(id, data) { return request({ url: `/tax-rate-configs/${id}`, method: 'put', data }) }

// Audit
export function getAuditHistory(tableName, params) { return request({ url: `/audit/${tableName}/history`, method: 'get', params }) }
export function compareAuditVersions(tableName, recordId, v1, v2) { return request({ url: `/audit/${tableName}/compare`, method: 'get', params: { recordId, v1, v2 } }) }
export function rollbackAudit(tableName, recordId, versionNo) { return request({ url: `/audit/${tableName}/rollback`, method: 'post', params: { recordId, versionNo } }) }
export function getAuditStats(params) { return request({ url: '/audit/stats', method: 'get', params }) }

// Room Types
export function getRoomTypes() { return request({ url: '/room-types', method: 'get' }) }
export function createRoomType(data) { return request({ url: '/room-types', method: 'post', data }) }
export function updateRoomType(id, data) { return request({ url: `/room-types/${id}`, method: 'put', data }) }
export function deleteRoomType(id) { return request({ url: `/room-types/${id}`, method: 'delete' }) }

// Pricing Standards
export function getPricingStandards(params) { return request({ url: '/room-pricing-standards', method: 'get', params }) }
export function createPricingStandard(data) { return request({ url: '/room-pricing-standards', method: 'post', data }) }
export function updatePricingStandard(id, data) { return request({ url: `/room-pricing-standards/${id}`, method: 'put', data }) }
export function deletePricingStandard(id) { return request({ url: `/room-pricing-standards/${id}`, method: 'delete' }) }
