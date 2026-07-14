<template>
  <div>
    <div class="page-header">
      <h2>自动匹配</h2>
      <div class="table-actions">
        <el-select v-model="selectedReconId" clearable filterable placeholder="选择对账会话" style="width:280px;" @change="onReconChange">
          <el-option v-for="r in reconciliations" :key="r.id" :label="r.startDate + ' ~ ' + r.endDate + ' (' + r.status + ')'" :value="r.id" />
        </el-select>
        <el-button type="primary" @click="autoMatch" :loading="autoMatching" :disabled="!selectedReconId">自动匹配</el-button>
        <el-button type="success" @click="completeRecon" :loading="completing" :disabled="!canComplete">完成对账</el-button>
        <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
      </div>
    </div>

    <div class="search-bar">
      <el-radio-group v-model="filterStatus" @change="loadData">
        <el-radio label="Unmatched">未匹配</el-radio>
        <el-radio label="Matched">已匹配</el-radio>
        <el-radio label="">全部</el-radio>
      </el-radio-group>
    </div>

    <el-table :data="statements" v-loading="loading" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="referenceNo" label="交易号" width="180" />
      <el-table-column prop="counterparty" label="付款人" width="120" />
      <el-table-column label="金额" width="110">
        <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="description" label="摘要" min-width="200" />
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Matched' ? 'success' : row.status === 'Reconciled' ? 'primary' : 'info'" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="120" fixed="right">
        <template #default="{ row }">
          <el-button v-if="row.status === 'Unmatched'" text size="small" type="primary" @click="openManualMatch(row)">手动匹配</el-button>
          <el-tag v-else-if="row.status === 'Matched'" type="success" size="small">已匹配</el-tag>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top:16px;text-align:right;">
      <el-pagination
        v-model:page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @current-change="loadData"
        @size-change="loadData"
      />
    </div>

    <!-- Manual Match Dialog -->
    <el-dialog v-model="showManualMatch" title="手动匹配" width="500px">
      <el-form label-width="100px">
        <el-form-item label="银行流水">
          <el-tag>{{ manualTarget?.referenceNo || manualTarget?.id }}</el-tag>
          <span style="margin-left:8px;">¥{{ (manualTarget?.amount || 0).toLocaleString() }}</span>
        </el-form-item>
        <el-form-item label="关联收款">
          <el-select v-model="selectedReceiptId" filterable placeholder="选择收款记录" style="width:100%;">
            <el-option v-for="r in receipts" :key="r.id" :label="r.receiptNo + ' - ¥' + (r.amount || 0).toLocaleString()" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="匹配金额">
          <el-input-number v-model="matchAmount" :min="0.01" :precision="2" style="width:200px;" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showManualMatch = false">取消</el-button>
        <el-button type="primary" :loading="matching" @click="submitManualMatch">确认匹配</el-button>
      </template>
    </el-dialog>
  </div>
</template>
<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  getBankStatements, getBankReconciliations, getReceipts,
  autoMatchBank, manualMatchBank, completeReconciliation
} from '@/api'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/user'

const userStore = useUserStore()
const loading = ref(false)
const autoMatching = ref(false)
const completing = ref(false)
const matching = ref(false)
const statements = ref([])
const receipts = ref([])
const reconciliations = ref([])
const selectedReconId = ref('')
const filterStatus = ref('Unmatched')
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const showManualMatch = ref(false)
const manualTarget = ref(null)
const selectedReceiptId = ref('')
const matchAmount = ref(0)

const canComplete = computed(() =>
  selectedReconId.value && reconciliations.value.find(r => r.id === selectedReconId.value)?.status !== 'Completed'
)

async function loadRecons() {
  try {
    const res = await getBankReconciliations({})
    reconciliations.value = Array.isArray(res) ? res : (res.items || res.data || [])
    if (reconciliations.value.length > 0 && !selectedReconId.value) {
      selectedReconId.value = reconciliations.value[0].id
    }
  } catch { /* 静默 */ }
}

async function loadData() {
  loading.value = true
  try {
    const companyId = userStore.effectiveCompanyId
    const res = await getBankStatements({
      companyId: companyId || undefined,
      status: filterStatus.value || undefined
    })
    let items = Array.isArray(res) ? res : (res.items || res.data || [])
    total.value = items.length
    const start = (page.value - 1) * pageSize.value
    statements.value = items.slice(start, start + pageSize.value)
  } catch { /* 静默 */ }
  finally { loading.value = false }
}

async function loadReceipts() {
  try {
    const companyId = userStore.effectiveCompanyId
    const res = await getReceipts({ companyId: companyId || undefined })
    receipts.value = Array.isArray(res) ? res : (res.items || res.data || res || [])
  } catch { /* 静默 */ }
}

function onReconChange() { /* 对账会话切换，后续可用作过滤 */ }

async function autoMatch() {
  if (!selectedReconId.value) { ElMessage.warning('请选择对账会话'); return }
  autoMatching.value = true
  try {
    const res = await autoMatchBank(selectedReconId.value)
    ElMessage.success(`自动匹配完成，共匹配 ${res.matched || 0} 条`)
    await loadData()
  } catch (e) { ElMessage.error(e?.response?.data?.message || '匹配失败') }
  autoMatching.value = false
}

async function completeRecon() {
  if (!selectedReconId.value) { ElMessage.warning('请选择对账会话'); return }
  completing.value = true
  try {
    await completeReconciliation(selectedReconId.value)
    ElMessage.success('对账已完成')
    await loadRecons()
  } catch (e) { ElMessage.error(e?.response?.data?.message || '完成对账失败') }
  completing.value = false
}

function openManualMatch(row) {
  manualTarget.value = row
  selectedReceiptId.value = ''
  matchAmount.value = Math.abs(row.amount || 0)
  loadReceipts()
  showManualMatch.value = true
}

async function submitManualMatch() {
  if (!selectedReceiptId.value) { ElMessage.warning('请选择关联收款'); return }
  if (!matchAmount.value || matchAmount.value <= 0) { ElMessage.warning('请输入匹配金额'); return }
  matching.value = true
  try {
    await manualMatchBank({
      statementId: manualTarget.value.id,
      receiptId: selectedReceiptId.value,
      amount: matchAmount.value
    })
    ElMessage.success('匹配成功')
    showManualMatch.value = false
    await loadData()
  } catch (e) { ElMessage.error(e?.response?.data?.message || '匹配失败') }
  matching.value = false
}

onMounted(async () => {
  await loadRecons()
  await loadData()
})
</script>
