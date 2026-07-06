<template>
  <div style="display:flex;flex-direction:column;height:calc(100vh - 120px);">
    <div class="page-header" style="flex-shrink:0;">
      <h2>合同管理</h2>
      <el-button type="primary" @click="$router.push('/contracts/create')">
        新建合同
      </el-button>
    </div>

    <div class="search-bar" style="flex-shrink:0;">
      <el-input v-model="search.keyword" placeholder="合同号/租客名/房间号" clearable style="width: 220px;" />
      <el-select v-model="search.status" placeholder="合同状态" clearable style="width: 140px;">
        <el-option label="草稿" value="Draft" />
        <el-option label="待审批" value="PendingApproval" />
        <el-option label="活跃" value="Active" />
        <el-option label="已暂停" value="Suspended" />
        <el-option label="已到期" value="Expired" />
        <el-option label="已终止" value="Terminated" />
        <el-option label="已续签" value="Renewed" />
      </el-select>
      <el-date-picker v-model="search.dateRange" type="daterange" range-separator="至" start-placeholder="到期开始" end-placeholder="到期结束" style="width: 240px;" />
      <el-button type="primary" @click="handleSearch">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-table :data="contractList" v-loading="loading" stripe style="flex:1;width:100%;" height="100%" max-height="100%">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="contractNo" label="合同号" width="150" />
      <el-table-column prop="roomName" label="房屋" width="100" />
      <el-table-column prop="tenantName" label="租客" width="100" />
      <el-table-column prop="rentAmount" label="月租金" width="100">
        <template #default="{ row }">¥{{ row.rentAmount?.toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="depositAmount" label="押金" width="100">
        <template #default="{ row }">¥{{ (row.depositAmount || 0).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="startDate" label="起租" width="95" />
      <el-table-column prop="endDate" label="到期" width="95" />
      <el-table-column prop="status" label="状态" width="95">
        <template #default="{ row }">
          <el-tag :type="statusTypeMap[row.status] || 'info'" size="small">{{ statusLabelMap[row.status] || row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="自动续签" width="80" align="center">
        <template #default="{ row }">
          <el-tag :type="row.autoRenew ? 'success' : 'info'" size="small" effect="plain">{{ row.autoRenew ? '开' : '关' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="240" fixed="right">
        <template #default="{ row }">
          <div style="display: flex; gap: 2px; flex-wrap: wrap;">
            <el-button text size="small" type="primary" @click="$router.push('/contracts/' + row.id)">详情</el-button>
            <el-button v-if="row.status === 'Active' || row.status === 'Suspended'" text size="small" type="warning" @click="showModifyRent(row)">调租</el-button>
            <el-button v-if="row.status === 'Active' || row.status === 'Suspended'" text size="small" type="warning" @click="showModifyFee(row)">调价</el-button>
            <el-button v-if="(row.status === 'Active' || row.status === 'Expired') && !row.hasRenewalContract" text size="small" type="primary" @click="handleRenew(row)">续签</el-button>
            <el-button v-if="row.status === 'Active'" text size="small" type="danger" @click="handleTerminate(row)">终止</el-button>
            <el-button v-if="row.status === 'Active'" text size="small" type="warning" @click="handleSuspend(row)">暂停</el-button>
            <el-button v-if="row.status === 'Suspended'" text size="small" type="success" @click="handleResume(row)">恢复</el-button>
          </div>
        </template>
      </el-table-column>
    </el-table>

    <div style="flex-shrink:0; text-align: right; padding: 12px 0 0;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" :page-sizes="[10, 20, 50]" layout="total, sizes, prev, pager, next" @current-change="onPageChange" @size-change="onPageChange" />
    </div>

    <!-- Terminate Dialog -->
    <el-dialog v-model="showTerminate" title="合同终止" width="500px">
      <el-form :model="terminateForm" label-width="100px">
        <el-form-item label="终止类型">
          <el-radio-group v-model="terminateForm.type">
            <el-radio label="EXPIRED">到期终止</el-radio>
            <el-radio label="EARLY">提前解约</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="实际终止日">
          <el-date-picker v-model="terminateForm.actualEndDate" type="date" />
        </el-form-item>
        <el-form-item label="终止原因">
          <el-input v-model="terminateForm.reason" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showTerminate = false">取消</el-button>
        <el-button type="primary" @click="submitTerminate">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Modify Rent Dialog -->
    <el-dialog v-model="showModifyRentDialog" :draggable="true" title="合同租金调整" width="550px">
      <el-alert title="租金调整需要经过审批，金额越大审批级别越高。" type="info" show-icon :closable="false" style="margin-bottom: 16px;" />
      <el-form :model="modifyRentForm" label-width="120px">
        <el-descriptions :column="2" border style="margin-bottom: 16px;">
          <el-descriptions-item label="合同号">{{ modifyRentTarget?.contractNo }}</el-descriptions-item>
          <el-descriptions-item label="当前月租">¥{{ modifyRentTarget?.rentAmount?.toLocaleString() }}</el-descriptions-item>
        <el-descriptions-item label="当前生效日">{{ modifyRentTarget?.rentEffDate || '-' }}</el-descriptions-item>
        </el-descriptions>
        <el-form-item label="新租金 (元/月)">
          <el-input-number v-model="modifyRentForm.newRentAmount" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="调整差额">
          <span :style="{ color: rentDiff >= 0 ? '#f56c6c' : '#67c23a', fontWeight: 'bold', fontSize: '16px' }">
            {{ rentDiff >= 0 ? '+' : '' }}¥{{ rentDiff.toLocaleString() }}
          </span>
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="modifyRentForm.effectiveDate" type="date" value-format="YYYY-MM-DD" />
        </el-form-item>
        <el-form-item label="调整原因">
          <el-input v-model="modifyRentForm.reason" type="textarea" :rows="3" placeholder="请说明调价原因，如：市场行情变化、合同约定涨幅等" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showModifyRentDialog = false">取消</el-button>
        <el-button type="primary" :loading="submittingRent" @click="submitModifyRent">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Modify Fee Dialog -->
    <el-dialog v-model="showModifyFeeDialog" :draggable="true" title="合同费用调价" width="820px">
      <el-alert title="费用中途调价需要运营主管审批。" type="info" show-icon :closable="false" style="margin-bottom: 16px;" />
      <el-descriptions :column="2" border style="margin-bottom: 16px;">
        <el-descriptions-item label="合同号">{{ modifyFeeTarget?.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="租客">{{ modifyFeeTarget?.tenantName }}</el-descriptions-item>
      </el-descriptions>
      <el-table :data="modifyFeeForm.items" stripe v-loading="feeConfigLoading">
        <el-table-column prop="feeName" label="收费项目" width="130" />
        <el-table-column prop="chargeMethod" label="计费方式" width="100" />
        <el-table-column label="当前价格" width="130">
          <template #default="{ row }">{{ row.oldPrice }}</template>
        </el-table-column>
        <el-table-column label="生效日期" width="160">
          <template #default="{ row }">
            <el-date-picker v-model="row.effectiveDate" type="date" value-format="YYYY-MM-DD" size="small" style="width:115px" :disabled-date="d => row._minDate && d.getTime() < row._minDate.getTime()" />
          </template>
        </el-table-column>
        <el-table-column label="新价格" width="140">
          <template #default="{ row }">
            <el-input-number v-model="row.newPrice" :min="0" :precision="row.chargeMethod === '按表计量' ? 4 : 2" size="small" style="width: 100px;" />
          </template>
        </el-table-column>
        <el-table-column label="涨幅" width="80">
          <template #default="{ row }">
            <span :style="{ color: row.newPrice > row.oldPriceVal ? '#f56c6c' : row.newPrice < row.oldPriceVal ? '#67c23a' : '#909399' }">
              {{ row.newPrice > row.oldPriceVal ? '↑' : row.newPrice < row.oldPriceVal ? '↓' : '-' }}
              {{ row.oldPriceVal ? Math.abs((row.newPrice - row.oldPriceVal) / row.oldPriceVal * 100).toFixed(1) + '%' : '' }}
            </span>
          </template>
        </el-table-column>
      </el-table>
      <el-form style="margin-top: 12px;">
        <el-form-item label="调价原因">
          <el-input v-model="modifyFeeForm.reason" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showModifyFeeDialog = false">取消</el-button>
        <el-button type="primary" :loading="submittingFee" @click="submitModifyFee">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Renew Dialog -->
    <el-dialog v-model="showRenewDialog" title="合同续签" width="550px">
      <el-alert v-if="renewFromRejected" title="上次续签被驳回，请修改后重新提交" type="warning" show-icon :closable="false" style="margin-bottom: 16px;" />
      <el-alert v-else title="续签将创建新合同，原合同标记为已续签。审批通过后自动执行。" type="success" show-icon :closable="false" style="margin-bottom: 16px;" />
      <el-descriptions :column="2" border style="margin-bottom: 16px;">
        <el-descriptions-item label="原合同号">{{ renewTarget?.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="当前月租">¥{{ renewTarget?.rentAmount?.toLocaleString() }}</el-descriptions-item>
        <el-descriptions-item label="原到期日">{{ renewTarget?.endDate }}</el-descriptions-item>
        <el-descriptions-item label="当前押金">¥{{ renewTarget?.depositAmount?.toLocaleString() || 0 }}</el-descriptions-item>
      </el-descriptions>
      <el-form :model="renewForm" label-width="120px">
        <el-form-item label="新合同月租">
          <el-input-number v-model="renewForm.rentAmount" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="新到期日期">
          <el-date-picker v-model="renewForm.endDate" type="date" value-format="YYYY-MM-DD" />
          <span style="margin-left: 8px; color: #909399; font-size: 12px;">起租日自动续接</span>
        </el-form-item>
        <el-form-item label="押金处理">
          <el-radio-group v-model="renewForm.depositHandling">
            <el-radio label="TRANSFER">原押金延续（¥{{ renewTarget?.depositAmount?.toLocaleString() || 0 }}）</el-radio>
            <el-radio label="NEW">重新收取</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item v-if="renewForm.depositHandling === 'NEW'" label="新押金金额">
          <el-input-number v-model="renewForm.newDeposit" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <!-- 押金处理摘要 -->
        <el-form-item label=" ">
          <div v-if="renewForm.depositHandling === 'NEW'" style="padding:6px 10px;background:#f5f7fa;border-radius:4px;font-size:13px;line-height:1.8;">
            <div>押金：¥{{ (renewTarget?.depositAmount || 0).toFixed(2) }} → ¥{{ (renewForm.newDeposit || 0).toFixed(2) }}
              <span v-if="renewForm.newDeposit > (renewTarget?.depositAmount || 0)" style="color:#e6a23c;">（上调 ¥{{ (renewForm.newDeposit - (renewTarget?.depositAmount || 0)).toFixed(2) }}）</span>
              <span v-else-if="renewForm.newDeposit < (renewTarget?.depositAmount || 0)" style="color:#67c23a;">（下调 ¥{{ ((renewTarget?.depositAmount || 0) - renewForm.newDeposit).toFixed(2) }}）</span>
            </div>
            <div style="color:#909399;">操作：退还旧押金 ¥{{ (renewTarget?.depositAmount || 0).toFixed(2) }} + 收取新押金 ¥{{ (renewForm.newDeposit || 0).toFixed(2) }}</div>
          </div>
          <div v-else style="padding:6px 10px;background:#f5f7fa;border-radius:4px;font-size:13px;line-height:1.8;">
            <div>押金：原押金 ¥{{ (renewTarget?.depositAmount || 0).toFixed(2) }} 延续至新合同</div>
            <div style="color:#909399;">说明：旧合同押金转出 → 新合同押金转入</div>
          </div>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="renewForm.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showRenewDialog = false; renewFromRejected = false">取消</el-button>
        <el-button type="primary" @click="submitRenew">提交续签审批</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getApprovalTypes, getRoles, createApprovalType, createApprovalLevel, getContracts, renewContract, terminateContract, suspendContract, resumeContract, previewRenewal, submitRenewal, getLastRejectedRenewal, rentAdjust, feeAdjust, getContractFeeConfigs, getContractFeeConfigHistory, handleApiError } from '@/api/index.js'
import { useUserStore } from '@/store/user'

const router = useRouter()
const userStore = useUserStore()

const search = reactive({ keyword: '', status: '', dateRange: null })
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })

const statusTypeMap = {
  'Draft': 'info', 'PendingApproval': 'warning', 'Active': 'success',
  'Suspended': '', 'Expired': 'danger', 'Terminated': 'danger', 'Renewed': 'primary'
}
const statusLabelMap = {
  'Draft': '草稿', 'PendingApproval': '待审批', 'Active': '活跃',
  'Suspended': '已暂停', 'Expired': '已到期', 'Terminated': '已终止', 'Renewed': '已续签'
}

const contractList = ref([])
const loading = ref(false)

async function fetchContracts() {
  loading.value = true
  try {
    const params = {
      page: pagination.page,
      pageSize: pagination.pageSize,
      keyword: search.keyword || undefined,
      status: search.status || undefined
    }
    const res = await getContracts(params)
    const items = res.items || res.data || []
    // API 返回字段映射：roomFullCode → roomName, tenants[0].tenantName → tenantName
    contractList.value = items.map(c => ({
      id: c.id,
      contractNo: c.contractNo,
      roomName: c.roomFullCode || '',
      tenantName: c.tenants?.length > 0 ? c.tenants[0].tenantName : '',
      tenantPhone: c.tenants?.length > 0 ? c.tenants[0].tenantPhone : '',
      rentAmount: c.rentAmount,
      depositAmount: c.depositAmount,
      startDate: c.startDate,
      endDate: c.endDate,
      status: c.status,
      roomId: c.roomId,
      companyId: c.companyId,
      previousContractId: c.previousContractId,
      renewalCount: c.renewalCount || 0,
      originalContractId: c.originalContractId,
      hasRenewalContract: c.hasRenewalContract || false,
      hasPendingRenewal: c.hasPendingRenewal || false,
      hasRejectedRenewal: c.hasRejectedRenewal || false,
      autoRenew: c.autoRenew !== false
    }))
    pagination.total = res.total || 0
  } catch (e) {
    ElMessage.error('加载合同列表失败')
  } finally {
    loading.value = false
  }
}

function onPageChange() {
  fetchContracts()
}

onMounted(fetchContracts)

// === Terminate ===
const showTerminate = ref(false)
const terminateForm = reactive({ type: 'EARLY', actualEndDate: '', reason: '' })
const currentContract = ref(null)

// === Modify Rent ===
const showModifyRentDialog = ref(false)
const modifyRentTarget = ref(null)
const modifyRentForm = reactive({ newRentAmount: 0, effectiveDate: '', reason: '' })
const contractModifyTypeId = ref(null)
const submittingRent = ref(false)

// 将字符串 ID 转为 GUID 格式（模拟数据使用，已有 GUID 则直接返回）
function toGuidId(id) {
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id)) return id
  const hex = Array.from(String(id)).reduce((h, c) => { const n = c.charCodeAt(0).toString(16); return h + (n.length < 2 ? '0' + n : n) }, '').padEnd(32, '0').slice(0, 32)
  return `${hex.slice(0,8)}-${hex.slice(8,12)}-${hex.slice(12,16)}-${hex.slice(16,20)}-${hex.slice(20,32)}`
}

// 获取 CONTRACT_MODIFY 审批类型 ID（带缓存，不存在则自动创建）
async function ensureContractModifyTypeId() {
  if (contractModifyTypeId.value) return contractModifyTypeId.value
  try {
    const types = await getApprovalTypes()
    let found = types.find(t => t.code === 'CONTRACT_MODIFY')

    if (!found) {
      // 自动创建 CONTRACT_MODIFY 审批类型
      found = await createApprovalType({
        name: '修改合同租金',
        code: 'CONTRACT_MODIFY',
        description: '修改合同租金需要审批，金额越大审批级别越高。'
      })

      // 查找角色 ID
      const roles = await getRoles()
      const opsSup = roles.find(r => r.code === 'OpsSupervisor')
      const deptMgr = roles.find(r => r.code === 'DeptManager')

      // 创建 2 级审批配置
      if (opsSup) {
        await createApprovalLevel(found.id, { level: 1, roleId: opsSup.id, minAmount: 0, maxAmount: 5000 })
      }
      if (deptMgr) {
        await createApprovalLevel(found.id, { level: 2, roleId: deptMgr.id, minAmount: 5000, maxAmount: 99999999 })
      }
    }

    contractModifyTypeId.value = found?.id || null
    return contractModifyTypeId.value
  } catch {
    return null
  }
}

const rentDiff = computed(() => {
  const old = modifyRentTarget.value?.rentAmount || 0
  return modifyRentForm.newRentAmount - old
})

// === Modify Fee ===
const showModifyFeeDialog = ref(false)
const modifyFeeTarget = ref(null)
const modifyFeeForm = reactive({ items: [], effectiveDate: '', reason: '' })

// === Renew ===
const showRenewDialog = ref(false)
const renewFromRejected = ref(false)
const renewTarget = ref(null)
const renewForm = reactive({ rentAmount: 0, endDate: '', depositHandling: 'TRANSFER', newDeposit: 0, remark: '' })
const renewChecks = ref({})

// === Event Handlers ===
function handleSearch() { pagination.page = 1; fetchContracts() }
function resetSearch() { search.keyword = ''; search.status = ''; search.dateRange = null; pagination.page = 1; fetchContracts() }

function handleTerminate(row) {
  currentContract.value = row
  showTerminate.value = true
}
async function submitTerminate() {
  if (!currentContract.value) return
  try {
    await terminateContract(toGuidId(currentContract.value.id), { reason: terminateForm.reason || '合同终止' })
    currentContract.value.status = 'Terminated'
    ElMessage.success('合同已终止')
    showTerminate.value = false
    fetchContracts()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '终止失败')
  }
}

function handleSuspend(row) {
  ElMessageBox.confirm(`确定暂停合同 ${row.contractNo} 吗？`, '提示').then(async () => {
    try {
      await suspendContract(toGuidId(row.id))
      row.status = 'Suspended'
      ElMessage.success('合同已暂停')
    } catch (e) {
      ElMessage.error(e?.response?.data?.message || '暂停失败')
    }
  }).catch(() => {})
}
function handleResume(row) {
  ElMessageBox.confirm(`确定恢复合同 ${row.contractNo} 吗？`, '提示').then(async () => {
    try {
      await resumeContract(toGuidId(row.id))
      row.status = 'Active'
      ElMessage.success('合同已恢复')
    } catch (e) {
      ElMessage.error(e?.response?.data?.message || '恢复失败')
    }
  }).catch(() => {})
}

// === Modify Rent ===
function showModifyRent(row) {
  modifyRentTarget.value = row
  modifyRentForm.newRentAmount = row.rentAmount
  modifyRentForm.effectiveDate = ''
  modifyRentForm.reason = ''
  showModifyRentDialog.value = true
}
async function submitModifyRent() {
  if (!modifyRentForm.newRentAmount || modifyRentForm.newRentAmount <= 0) {
    ElMessage.warning('请输入有效的租金金额')
    return
  }
  if (!modifyRentForm.reason) {
    ElMessage.warning('请填写调整原因')
    return
  }

  submittingRent.value = true
  try {
    const diff = modifyRentForm.newRentAmount - (modifyRentTarget.value?.rentAmount || 0)
    const approvalLevel = Math.abs(diff) > 5000 ? '2级(部门经理)' : '1级(运营主管)'

    await rentAdjust(toGuidId(modifyRentTarget.value?.id), {
      newAmount: modifyRentForm.newRentAmount,
      effectiveDate: modifyRentForm.effectiveDate || null,
      reason: modifyRentForm.reason
    })

    ElMessage.success(`租金调整申请已提交${approvalLevel}审批，等待审批人处理`)
    showModifyRentDialog.value = false
  } catch (e) {
    handleApiError(e, '提交审批失败')
  } finally {
    submittingRent.value = false
  }
}
// === Modify Fee ===
const feeConfigLoading = ref(false)
const submittingFee = ref(false)
const feeMinDate = ref(null)
function disabledFeeDate(time) {
  if (!feeMinDate.value) return false
  return time.getTime() < feeMinDate.value.getTime()
}
async function showModifyFee(row) {
  modifyFeeTarget.value = row
  modifyFeeForm.effectiveDate = ''
  modifyFeeForm.reason = ''
  modifyFeeForm.items = []
  showModifyFeeDialog.value = true

  feeConfigLoading.value = true
  try {
    const configs = await getContractFeeConfigs(toGuidId(row.id))
    if (Array.isArray(configs) && configs.length > 0) {
      // 计算 DatePicker 最小可选日期（最早生效日 + 2天）
      const dates = configs
        .filter(f => f.isActive && f.effectiveDate)
        .map(f => new Date(f.effectiveDate))
        .filter(d => !isNaN(d.getTime()))
      if (dates.length > 0) {
        const min = new Date(Math.min(...dates))
        min.setDate(min.getDate() + 2)
        feeMinDate.value = min
        modifyFeeForm.effectiveDate = min.toISOString().split('T')[0]
      }
      const activeConfigs = configs.filter(f => f.isActive)
      modifyFeeForm.items = activeConfigs.map(f => {
        const amount = typeof f.amount === 'number' ? f.amount : parseFloat(f.amount) || 0
        const isMeter = f.billingMode === 'MeterBased'
        const priceLabel = isMeter
          ? `${amount.toFixed(2)} 元/${f.unit || '吨'}`
          : `¥${amount.toLocaleString()}`
        const effDate = f.effectiveDate ? new Date(f.effectiveDate) : null
        const minDate = effDate ? new Date(effDate.getTime() + 86400000) : null
        return {
          id: f.id || '',
          feeCodeId: f.feeCodeId || '',
          feeName: f.feeCodeName || f.feeName || '',
          chargeMethod: isMeter ? '按表计量' : '固定金额',
          oldPrice: priceLabel,
          oldPriceVal: amount,
          newPrice: amount,
          unit: f.unit || '',
          _isActive: f.isActive,
          _expiryDate: f.expiryDate,
          effectiveDate: minDate ? minDate.toISOString().split('T')[0] : '',
          _minDate: minDate
        }
      })
    } else {
      ElMessage.warning('该合同暂无费用配置，请先在合同详情页添加费用项目')
    }
  } catch {
    handleApiError(new Error(), '获取费用配置失败')
  }
  feeConfigLoading.value = false
}
async function submitModifyFee() {
  if (!modifyFeeForm.reason) {
    ElMessage.warning('请填写调价原因')
    return
  }


  const changedItems = modifyFeeForm.items.filter(item => item.newPrice !== item.oldPriceVal)
  if (changedItems.length === 0) {
    ElMessage.warning('没有费用项目价格发生变化')
    return
  }

  // ★ 前端预校验：日期区间冲突检测
  for (const item of changedItems) {
    try {
      const history = await getContractFeeConfigHistory(toGuidId(modifyFeeTarget.value?.id), item.feeCodeId)
      const currentActive = modifyFeeForm.items.find(c => c.feeCodeId === item.feeCodeId && c._isActive && !c._expiryDate)
      if (!item.effectiveDate) { ElMessage.error(`请选择「${item.feeName}」的生效日期`); return }
      const newEff = new Date(item.effectiveDate)
      const hasConflict = history.some(cfg => {
        if (cfg.id === currentActive?.id) return false
        if (!cfg.effectiveDate) return false
        const cfgExp = cfg.expiryDate ? new Date(cfg.expiryDate) : new Date('9999-12-31')
        return newEff <= cfgExp
      })
      if (hasConflict) {
        ElMessage.error(`「${item.feeName}」的生效日期与已有记录冲突，请调整生效日期`)
        return
      }
      // 生效日期必须晚于当前配置的生效日期
      if (currentActive?.effectiveDate && newEff <= new Date(currentActive.effectiveDate)) {
        ElMessage.error(`「${item.feeName}」的生效日期必须晚于当前配置的生效日期 ${currentActive.effectiveDate}`)
        return
      }
    } catch (e) {
      ElMessage.warning(`「${item.feeName}」校验日期冲突失败，请稍后重试`)
      return
    }
  }

  submittingFee.value = true
  try {
    const items = changedItems.map(i => ({
      feeCodeId: i.feeCodeId || '00000000-0000-0000-0000-000000000000',
      feeName: i.feeName,
      oldAmount: i.oldPriceVal || 0,
      newAmount: i.newPrice,
      billingMode: i.chargeMethod === '按表计量' ? 'MeterBased' : 'FixedAmount',
      unit: i.unit || '',
      effectiveDate: i.effectiveDate || ''
    }))

    const res = await feeAdjust(toGuidId(modifyFeeTarget.value?.id), {
      reason: modifyFeeForm.reason,
      items
    })

    ElMessage.success(res?.message || '费用调价申请已提交审批')
    showModifyFeeDialog.value = false
  } catch (e) {
    handleApiError(e, '提交审批失败')
  } finally {
    submittingFee.value = false
  }
}

// === Renew ===
async function handleRenew(row) {
  if (row.hasPendingRenewal) {
    ElMessage.warning('该合同已有待审批的续签申请，请处理完成后再提交')
    return
  }
  renewTarget.value = row
  renewForm.rentAmount = row.rentAmount
  renewForm.depositHandling = 'TRANSFER'
  renewForm.newDeposit = (row.depositAmount || 0) + 20
  renewForm.remark = ''
  // 有被驳回的续签 → 预填上次数据
  renewFromRejected.value = false
  if (row.hasRejectedRenewal) {
    try {
      const rejected = await getLastRejectedRenewal(row.id)
      if (rejected) {
        renewForm.rentAmount = rejected.newRentAmount || row.rentAmount
        if (rejected.depositHandling) renewForm.depositHandling = rejected.depositHandling
        if (rejected.newEndDate) renewForm.endDate = rejected.newEndDate
        renewFromRejected.value = true
        showRenewDialog.value = true
        return
      }
    } catch { /* 预填失败走默认值 */ }
  }
  // Load preview to check constraints
  try {
    const preview = await previewRenewal(row.id)
    renewChecks.value = preview.checks || {}
    if (!renewChecks.value.paymentStatus?.passed) {
      ElMessage.warning(`该合同有未结清欠费 ¥${renewChecks.value.paymentStatus.outstandingAmount?.toLocaleString()}，请先处理`)
      return
    }
    if (renewChecks.value.concurrentApprovals?.hasPending) {
      ElMessage.warning(renewChecks.value.concurrentApprovals.blockedMessage || '该合同存在待审批的申请')
      return
    }
    if (renewChecks.value.concurrentApprovals?.alreadyRenewed) {
      ElMessage.warning(`该合同已被续签（新合同号：${renewChecks.value.concurrentApprovals.renewedContractNo}），不可再次续签`)
      return
    }
    // 使用后端建议的默认日期（消除 JS 时区计算偏差）
    const suggested = preview.defaultRenewalInfo
    if (suggested?.suggestedEndDate) {
      renewForm.endDate = suggested.suggestedEndDate
    }
    showRenewDialog.value = true
  } catch (e) {
    // Fallback: allow dialog open even if preview fails
    renewChecks.value = {}
    showRenewDialog.value = true
  }
}
async function submitRenew() {
  if (!renewForm.rentAmount || !renewForm.endDate) {
    ElMessage.warning('请填写完整的续签信息')
    return
  }
  try {
    const result = await submitRenewal(toGuidId(renewTarget.value?.id), {
      newRentAmount: renewForm.rentAmount,
      newEndDate: renewForm.endDate,
      depositHandling: renewForm.depositHandling,
      newDepositAmount: renewForm.depositHandling === 'NEW' ? renewForm.newDeposit : null,
      remark: renewForm.remark
    })
    ElMessage.success(`续签申请已提交${result.status === 'Pending' ? '，等待审批' : ''}`)
    showRenewDialog.value = false
    fetchContracts()
  } catch (e) {
    ElMessage.error(e?.response?.data?.error || e?.response?.data?.message || '续签提交失败')
  }
}
</script>
