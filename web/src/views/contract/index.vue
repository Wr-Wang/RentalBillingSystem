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
      <el-select v-model="search.status" placeholder="合同状态" clearable filterable style="width: 140px;">
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

    <el-table :data="contractList" v-loading="loading" stripe style="flex:1;width:100%;" height="100%" max-height="100%" border>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="contractNo" label="合同号" min-width="220" />
      <el-table-column prop="roomName" label="房屋" min-width="110" />
      <el-table-column prop="tenantName" label="租客" min-width="90" />
      <el-table-column prop="startDate" label="起租" min-width="110" />
      <el-table-column prop="endDate" label="到期" min-width="110" />
      <el-table-column prop="status" label="状态" min-width="80">
        <template #default="{ row }">
          <el-tag :type="statusTypeMap[row.status] || 'info'" size="small">{{ statusLabelMap[row.status] || row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="自动续签" min-width="90" align="center">
        <template #default="{ row }">
          {{ row.autoRenew ? '开' : '关' }}
        </template>
      </el-table-column>
      <el-table-column label="操作" width="270" fixed="right">
        <template #default="{ row }">
          <div style="display: flex; gap: 0; flex-wrap: nowrap; white-space: nowrap;">
            <el-button text size="small" type="primary" @click="$router.push('/contracts/' + row.id)" style="padding:4px 6px;">详情</el-button>
            <el-button v-if="row.status === 'Active' || row.status === 'Suspended'" text size="small" type="warning" @click="showModifyFee(row)" style="padding:4px 6px;">调价</el-button>
            <el-button v-if="(row.status === 'Active' || row.status === 'Expired') && !row.hasRenewalContract" text size="small" type="primary" @click="handleRenew(row)" style="padding:4px 6px;">续签</el-button>
            <el-button v-if="row.status === 'Active'" text size="small" type="danger" @click="handleTerminate(row)" style="padding:4px 6px;">终止</el-button>
            <el-button v-if="row.status === 'Active'" text size="small" type="warning"  style="padding:4px 6px;">暂停</el-button>
            <el-button v-if="row.status === 'Suspended'" text size="small" type="success"  style="padding:4px 6px;">恢复</el-button>
          </div>
        </template>
      </el-table-column>
    </el-table>

    <div style="flex-shrink:0; text-align: right; padding: 12px 0 0;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" :page-sizes="[10, 20, 50]" layout="total, sizes, prev, pager, next" @current-change="onPageChange" @size-change="onPageChange" />
    </div>

    <!-- Terminate Dialog -->
    <el-dialog :draggable="true" v-model="showTerminate" title="合同终止" width="500px">
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
    <el-dialog :draggable="true" v-model="showRenewDialog" title="合同续签" width="620px">
      <el-alert v-if="renewFromRejected" title="上次续签被驳回，请修改后重新提交" type="warning" show-icon :closable="false" style="margin-bottom: 16px;" />
      <el-alert v-else title="续签将创建新合同，原合同标记为已续签。审批通过后自动执行。" type="success" show-icon :closable="false" style="margin-bottom: 16px;" />
      <!-- 检查结果提示 -->
      <div v-if="renewChecks.paymentStatus && !renewChecks.paymentStatus.passed" style="margin-bottom: 12px;">
        <el-alert :title="`该合同有未结清欠费 ¥${renewChecks.paymentStatus.outstandingAmount?.toLocaleString()}，请先处理`" type="error" show-icon :closable="false" />
      </div>
      <div v-if="renewChecks.concurrentApprovals?.hasPending" style="margin-bottom: 12px;">
        <el-alert :title="renewChecks.concurrentApprovals.blockedMessage || '该合同存在待审批的申请，请处理完成后再提交续签'" type="warning" show-icon :closable="false" />
      </div>
      <div v-if="renewChecks.concurrentApprovals?.alreadyRenewed" style="margin-bottom: 12px;">
        <el-alert title="该合同已被续签，不可再次续签" type="warning" show-icon :closable="false" />
      </div>
      <!-- 市场参考价 -->
      <div v-if="renewChecks.marketPrice" style="margin-bottom: 12px; padding: 8px 12px; background: #f5f7fa; border-radius: 4px; font-size: 13px; color: #606266;">
        同户型市场参考价：¥{{ renewChecks.marketPrice.minPrice?.toLocaleString() }} ~ ¥{{ renewChecks.marketPrice.maxPrice?.toLocaleString() }}
        （均价 ¥{{ renewChecks.marketPrice.averagePrice?.toLocaleString() }}）
      </div>
      <el-descriptions :column="2" border style="margin-bottom: 16px;">
        <el-descriptions-item label="原合同号">{{ renewTarget?.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="原到期日">{{ renewTarget?.endDate }}</el-descriptions-item>
      </el-descriptions>
      <el-form :model="renewForm" label-width="120px">
        <el-form-item label="新合同月租">
          <el-input-number v-model="renewForm.rentAmount" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="新到期日期" :required="true">
          <el-date-picker v-model="renewForm.endDate" type="date" value-format="YYYY-MM-DD" placeholder="请选择到期日期" />
          <span style="margin-left: 8px; color: #909399; font-size: 12px;">起租日自动续接，到期日须晚于起租日</span>
        </el-form-item>
        <el-form-item label="押金处理">
          <el-radio-group v-model="renewForm.depositHandling">
            <el-radio label="TRANSFER">原押金延续</el-radio>
            <el-radio label="NEW">重新收取</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item v-if="renewForm.depositHandling === 'NEW'" label="新押金金额">
          <el-input-number v-model="renewForm.newDeposit" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <!-- 押金处理摘要 -->
        <el-form-item label=" ">
          <div v-if="renewForm.depositHandling === 'NEW'" style="padding:6px 10px;background:#f5f7fa;border-radius:4px;font-size:13px;line-height:1.8;">


          </div>
          <div v-else style="padding:6px 10px;background:#f5f7fa;border-radius:4px;font-size:13px;line-height:1.8;">
            <div>押金：原押金延续至新合同</div>
            <div style="color:#909399;">说明：旧合同押金转出 → 新合同押金转入</div>
          </div>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="renewForm.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showRenewDialog = false; renewFromRejected = false">取消</el-button>
        <el-button type="primary" :disabled="!canSubmitRenewal" @click="submitRenew">提交续签审批</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getApprovalTypes, getRoles, createApprovalType, createApprovalLevel, getContracts, renewContract, terminateContract, previewRenewal, submitRenewal, getLastRejectedRenewal, feeAdjust, getContractFeeConfigs, getContractFeeConfigHistory, handleApiError } from '@/api/index.js'
import { useUserStore } from '@/store/user'
import { toGuidId } from '@/utils'

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
const canSubmitRenewal = computed(() => {
  const checks = renewChecks.value
  return checks.paymentStatus?.passed !== false
    && !checks.concurrentApprovals?.hasPending
    && !checks.concurrentApprovals?.alreadyRenewed
})


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
      const activeConfigs = (configs || []).filter(f => f.isActive && f.chargeType === 'Recurring')
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
          _originalEffectiveDate: f.effectiveDate, // 保留原始生效日期，供校验用
          _minDate: minDate
        }
      })
    } else {
      ElMessage.warning('该合同暂无费用配置，请先在合同详情页添加费用项目')
    }
  } catch (e) {
    console.error('[showModifyFee] 获取费用配置失败:', e)
    handleApiError(e, '获取费用配置失败')
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
      if (currentActive?._originalEffectiveDate && newEff <= new Date(currentActive._originalEffectiveDate)) {
        ElMessage.error(`「${item.feeName}」的生效日期必须晚于当前配置的生效日期 ${currentActive._originalEffectiveDate}`)
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
  renewForm.rentAmount = 0
  renewForm.depositHandling = 'TRANSFER'
  renewForm.newDeposit = 20
  renewForm.remark = ''
  // 有被驳回的续签 → 预填上次数据
  renewFromRejected.value = false
  if (row.hasRejectedRenewal) {
    try {
      const rejected = await getLastRejectedRenewal(row.id)
      if (rejected) {
        renewForm.rentAmount = rejected.newRentAmount || 0
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
    // 预填租金
    renewForm.rentAmount = preview.defaultRenewalInfo?.currentRentAmount || 0
    // 到期日期由用户手动填写，不设默认值
    renewForm.endDate = ''
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
  // 校验到期日必须晚于起租日
  const startDate = renewTarget.value?.endDate ? new Date(renewTarget.value.endDate).getTime() + 86400000 : 0
  if (startDate && new Date(renewForm.endDate).getTime() <= startDate) {
    ElMessage.warning('到期日期必须晚于起租日期（' + (renewTarget.value?.endDate ? new Date(renewTarget.value.endDate).toLocaleDateString('zh-CN') + ' 次日' : '') + '）')
    return
  }
  if (!canSubmitRenewal.value) {
    ElMessage.warning('存在待审批流或欠费，无法提交')
    return
  }
  const contractId = renewTarget.value?.id
  console.log('[submitRenew] contractId:', contractId)
  if (!contractId) {
    ElMessage.warning('合同 ID 无效，请重新选择')
    return
  }
  try {
    const result = await submitRenewal(contractId, {
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
    //console.error('[submitRenew] error:', e?.response?.status, e?.response?.data, e)
    handleApiError(e, '续签提交失败')
  }
}
</script>
