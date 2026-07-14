<template>
  <div>
    <div class="page-header">
      <h2>收款管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="drawerVisible = true"><el-icon><Plus /></el-icon>收款登记</el-button>
        <el-button @click="$router.push('/receipts/confirm')">
          <el-icon><Select /></el-icon>待确认
          <el-tag v-if="pendingCount > 0" size="small" type="danger" style="margin-left:4px">{{ pendingCount }}</el-tag>
        </el-button>
      </div>
    </div>

    <!-- 收款登记抽屉 -->
    <el-drawer v-model="drawerVisible" title="收款登记" direction="rtl" size="420px">
      <el-form :model="form" label-position="top">
        <el-form-item label="收款金额（元）">
          <el-input-number v-model="form.amount" :min="0.01" :precision="2" style="width:100%;" />
        </el-form-item>
        <el-form-item label="收款日期">
          <el-date-picker v-model="form.receivedDate" type="date" style="width:100%;" />
        </el-form-item>
        <el-form-item label="关联合同">
          <el-select v-model="form.contractId" clearable filterable placeholder="选填，按合同号搜索" style="width:100%;">
            <el-option v-for="c in contractOptions" :key="c.id" :label="c.contractNo + ' - ' + c.tenantName" :value="c.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="支付渠道">
          <el-select v-model="form.paymentChannelId" clearable placeholder="选填" style="width:100%;">
            <el-option v-for="p in paymentChannels" :key="p.id" :label="p.name || p.channelName" :value="p.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="收款单号">
          <el-input v-model="form.receiptNo" placeholder="自动生成" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="drawerVisible = false">取消</el-button>
        <el-button type="primary" @click="submit" :loading="submitting">登记</el-button>
      </template>
    </el-drawer>

    <!-- 主内容 Tabs -->
    <el-tabs v-model="activeTab">
      <!-- Tab 1: 收款记录 -->
      <el-tab-pane label="收款记录" name="receipts">
        <el-card shadow="never" class="search-bar">
          <el-form :model="filter" inline>
            <el-input v-model="filter.keyword" placeholder="收据号/参考号" clearable style="width:160px;" @clear="fetchReceipts" @keyup.enter="fetchReceipts" />
            <el-select v-model="filter.status" placeholder="状态" clearable style="width:110px;" @change="fetchReceipts">
              <el-option label="待确认" value="Pending" />
              <el-option label="已确认" value="Confirmed" />
              <el-option label="已驳回" value="Rejected" />
              <el-option label="已取消" value="Cancelled" />
            </el-select>
            <el-button type="primary" @click="fetchReceipts">查询</el-button>
            <el-button @click="resetFilter">重置</el-button>
            <el-button
              v-if="selectedReceipts.length > 0"
              type="success"
              size="small"
              :loading="batchConfirmLoading"
              @click="batchConfirm"
            >
              批量确认（{{ selectedReceipts.length }}）
            </el-button>
          </el-form>
        </el-card>

        <el-table
          ref="receiptTableRef"
          :data="receiptList"
          v-loading="loading"
          stripe
          @selection-change="onReceiptSelectionChange"
        >
          <el-table-column type="selection" width="40" />
          <el-table-column prop="receiptNo" label="收据号" min-width="160" />
          <el-table-column label="金额" width="130" align="right">
            <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
          </el-table-column>
          <el-table-column label="关联合同" width="140">
            <template #default="{ row }">{{ row.contractNo || row.contractId || '-' }}</template>
          </el-table-column>
          <el-table-column prop="receivedDate" label="收款日期" width="110" />
          <el-table-column label="状态" width="100">
            <template #default="{ row }">
              <el-tag :type="statusType(row.status)" size="small" style="width:64px;text-align:center;">
                {{ statusLabel(row.status) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="220" fixed="right">
            <template #default="{ row }">
              <el-button text size="small" type="success" v-if="row.status === 'Pending'" @click="confirmSingle(row)">确认</el-button>
              <el-button text size="small" type="warning" v-if="row.status === 'Pending'" @click="rejectSingle(row)">驳回</el-button>
              <el-button text size="small" type="danger" v-if="row.status === 'Confirmed'" @click="reverseSingle(row)">冲销</el-button>
              <el-button text size="small" type="danger" v-if="row.status === 'Confirmed'" @click="refundSingle(row)">退款</el-button>
            </template>
          </el-table-column>
        </el-table>

        <div style="margin-top:16px;text-align:right;">
          <el-pagination
            v-model:page="pagination.page"
            v-model:page-size="pagination.pageSize"
            :total="pagination.total"
            :page-sizes="[10, 20, 50]"
            layout="total, sizes, prev, pager, next"
            @current-change="fetchReceipts"
            @size-change="fetchReceipts"
          />
        </div>
      </el-tab-pane>

      <!-- Tab 2: 押金管理 -->
      <el-tab-pane label="押金管理" name="deposits">
        <el-card shadow="never" class="search-bar">
          <el-form inline>
            <el-select v-model="depositContractId" clearable filterable placeholder="选择合同查看押金" style="width:280px;" @change="fetchDeposits">
              <el-option v-for="c in contractOptions" :key="c.id" :label="c.contractNo + ' - ' + c.tenantName" :value="c.id" />
            </el-select>
          </el-form>
        </el-card>
        <el-table :data="depositList" v-loading="depositLoading" stripe>
          <el-table-column prop="action" label="类型" width="90">
            <template #default="{ row }">{{ actionLabel(row.action) }}</template>
          </el-table-column>
          <el-table-column label="变动金额" width="120" align="right">
            <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
          </el-table-column>
          <el-table-column label="余额" width="120" align="right">
            <template #default="{ row }">¥{{ (row.balance || 0).toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="createdAt" label="时间" width="170" />
          <el-table-column prop="remark" label="备注" min-width="150" />
        </el-table>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useUserStore } from '@/store/user'
import {
  getReceipts, getDeposits, createReceipt,
  confirmReceipt as apiConfirm, rejectReceipt as apiReject,
  reverseReceipt as apiReverse, refundReceipt as apiRefund,
  batchConfirmReceipts, getContracts, getPaymentChannels
} from '@/api'
import { ElMessage, ElMessageBox } from 'element-plus'

const userStore = useUserStore()
const receiptTableRef = ref(null)
const activeTab = ref('receipts')
const loading = ref(false)
const depositLoading = ref(false)
const receiptList = ref([])
const depositList = ref([])
const batchConfirmLoading = ref(false)
const selectedReceipts = ref([])
const contractOptions = ref([])
const paymentChannels = ref([])

const pendingCount = computed(() =>
  receiptList.value.filter(r => r.status === 'Pending').length
)

// 筛选条件
const filter = reactive({ keyword: '', status: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })

// 押金查询
const depositContractId = ref('')

// 登记表单
const drawerVisible = ref(false)
const form = reactive({ receiptNo: '', amount: 0, receivedDate: '', contractId: '', paymentChannelId: '' })
const submitting = ref(false)

function statusType(s) {
  return { Confirmed: 'success', Pending: 'warning', Rejected: 'danger', Cancelled: 'info' }[s] || 'info'
}
function statusLabel(s) {
  return { Confirmed: '已确认', Pending: '待确认', Rejected: '已驳回', Cancelled: '已取消' }[s] || s
}
function actionLabel(a) {
  return { Create: '创建', Return: '退还', Deduct: '扣款' }[a] || a
}

// ========== 数据加载 ==========

function getEffectiveCompanyId() {
  return userStore.effectiveCompanyId || userStore.companyId
}

async function fetchPaymentChannels() {
  try { const res = await getPaymentChannels(); paymentChannels.value = Array.isArray(res) ? res : (res.items || res.data || []) }
  catch { /* 静默 */ }
}

async function fetchContracts() {
  try {
    const res = await getContracts({ pageSize: 200 })
    const items = res.items || res.data || []
    contractOptions.value = items.map(c => ({
      id: c.id,
      contractNo: c.contractNo,
      tenantName: c.tenants?.length > 0 ? c.tenants[0].tenantName : ''
    }))
  } catch { /* 静默 */ }
}

async function fetchReceipts() {
  const companyId = getEffectiveCompanyId()
  if (!companyId) {
    receiptList.value = []
    pagination.total = 0
    return
  }

  loading.value = true
  try {
    const params = {
      companyId,
      status: filter.status || undefined
    }
    // 默认获取全部，由后端根据 status 参数分发
    const res = await getReceipts(params)
    let items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    // 前端关键词过滤
    if (filter.keyword) {
      const kw = filter.keyword.toLowerCase()
      items = items.filter(r =>
        (r.receiptNo || '').toLowerCase().includes(kw) ||
        (r.referenceNo || '').toLowerCase().includes(kw)
      )
    }
    pagination.total = items.length
    const start = (pagination.page - 1) * pagination.pageSize
    receiptList.value = items.slice(start, start + pagination.pageSize)
  } catch { ElMessage.error('加载收款记录失败') }
  finally { loading.value = false }
}

async function fetchDeposits() {
  if (!depositContractId.value) {
    depositList.value = []
    return
  }
  depositLoading.value = true
  try {
    const res = await getDeposits({ contractId: depositContractId.value })
    depositList.value = Array.isArray(res) ? res : (res.items || res.data || [])
  } catch { ElMessage.error('加载押金记录失败') }
  finally { depositLoading.value = false }
}

function resetFilter() {
  filter.keyword = ''
  filter.status = ''
  pagination.page = 1
  fetchReceipts()
}

function onReceiptSelectionChange(rows) {
  selectedReceipts.value = rows.filter(r => r.status === 'Pending')
}

// ========== 收款登记 ==========

async function submit() {
  if (form.amount <= 0) { ElMessage.warning('请输入金额'); return }
  const companyId = getEffectiveCompanyId()
  if (!companyId) { ElMessage.warning('请先选择公司'); return }

  submitting.value = true
  try {
    await createReceipt({
      receiptNo: form.receiptNo || undefined,
      amount: form.amount,
      receivedDate: form.receivedDate || new Date().toISOString().slice(0, 10),
      companyId,
      contractId: form.contractId || undefined,
      paymentChannelId: form.paymentChannelId || undefined
    })
    ElMessage.success('登记成功')
    drawerVisible.value = false
    form.amount = 0; form.receiptNo = ''; form.receivedDate = ''; form.contractId = ''; form.paymentChannelId = ''
    await fetchReceipts()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '登记失败')
  } finally { submitting.value = false }
}

// ========== 单笔操作 ==========

async function confirmSingle(row) {
  try {
    await ElMessageBox.confirm(`确认收款 ${row.receiptNo} 金额 ¥${(row.amount || 0).toLocaleString()} 到账？`, '确认')
    await apiConfirm(row.id)
    ElMessage.success('收款已确认')
    await fetchReceipts()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '确认失败') }
}

async function rejectSingle(row) {
  try {
    const { value } = await ElMessageBox.prompt(`驳回原因（${row.receiptNo}）：`, '驳回', {
      confirmButtonText: '确定', cancelButtonText: '取消', inputPlaceholder: '请输入驳回原因'
    })
    if (!value) { ElMessage.warning('请输入驳回原因'); return }
    await apiReject(row.id, { reason: value })
    ElMessage.success('已驳回')
    await fetchReceipts()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '驳回失败') }
}

async function reverseSingle(row) {
  try {
    await ElMessageBox.confirm(`确定冲销收据 ${row.receiptNo} 吗？`, '提示', { type: 'warning' })
    await apiReverse(row.id, { reason: '手动冲销' })
    ElMessage.success('冲销成功')
    await fetchReceipts()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '冲销失败') }
}

async function refundSingle(row) {
  try {
    const { value } = await ElMessageBox.prompt(`退款金额（最多 ¥${(row.amount || 0).toLocaleString()}）：`, '退款', {
      confirmButtonText: '确定', cancelButtonText: '取消',
      inputValue: String(row.amount || 0)
    })
    const amt = parseFloat(value)
    if (!amt || amt <= 0 || amt > (row.amount || 0)) { ElMessage.warning('请输入有效金额'); return }
    await apiRefund(row.id, { amount: amt, reason: '退款' })
    ElMessage.success('退款成功')
    await fetchReceipts()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '退款失败') }
}

// ========== 批量操作 ==========

async function batchConfirm() {
  const ids = selectedReceipts.value.map(r => r.id)
  if (ids.length === 0) { ElMessage.warning('请选择待确认的收款'); return }
  try {
    await ElMessageBox.confirm(`批量确认 ${ids.length} 笔收款？`, '批量确认')
    batchConfirmLoading.value = true
    await batchConfirmReceipts(ids)
    ElMessage.success(`已确认 ${ids.length} 笔收款`)
    selectedReceipts.value = []
    await fetchReceipts()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('批量确认失败')
  } finally { batchConfirmLoading.value = false }
}

onMounted(() => {
  fetchContracts()
  if (getEffectiveCompanyId()) fetchReceipts()
  fetchPaymentChannels()
})
</script>

<style scoped>
.search-bar {
  margin-bottom: 16px;
}
</style>
