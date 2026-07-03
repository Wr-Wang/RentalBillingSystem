<template>
  <div>
    <div class="page-header">
      <h2>生成账单</h2>
      <div class="table-actions">
        <el-button @click="$router.push('/bills')">
          <el-icon><ArrowLeft /></el-icon>返回列表
        </el-button>
      </div>
    </div>

    <!-- ====== Filter Section ====== -->
    <el-card style="margin-bottom: 16px;">
      <div class="filter-bar">
        <el-form :inline="true" :model="filters" label-width="80px">
          <el-form-item label="账期">
            <el-date-picker v-model="filters.period" type="month" placeholder="选择月份" style="width: 140px;" />
          </el-form-item>
          <el-form-item label="楼栋">
            <el-select v-model="filters.buildingId" placeholder="全部楼栋" clearable style="width: 140px;">
              <el-option v-for="b in buildings" :key="b.id" :label="b.name" :value="b.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="房屋">
            <el-select v-model="filters.roomId" placeholder="全部房间" clearable filterable style="width: 160px;">
              <el-option v-for="r in filteredRooms" :key="r.id" :label="r.name" :value="r.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="合同状态">
            <el-select v-model="filters.contractStatus" placeholder="全部" clearable style="width: 120px;">
              <el-option label="活跃" value="Active" />
              <el-option label="已暂停" value="Suspended" />
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="searchContracts">
              <el-icon><Search /></el-icon>筛选
            </el-button>
            <el-button @click="resetFilters">重置</el-button>
          </el-form-item>
        </el-form>
      </div>
    </el-card>

    <!-- ====== Summary Bar ====== -->
    <el-card style="margin-bottom: 16px;">
      <el-row :gutter="16">
        <el-col :span="8">
          <div class="summary-item">
            <div class="summary-label">待生成合同</div>
            <div class="summary-value" style="color: #409eff;">{{ pendingContracts.length }}</div>
          </div>
        </el-col>
        <el-col :span="8">
          <div class="summary-item">
            <div class="summary-label">已生成合同</div>
            <div class="summary-value" style="color: #909399;">{{ generatedContracts.length }}</div>
          </div>
        </el-col>
        <el-col :span="8">
          <div class="summary-item">
            <div class="summary-label">已选待生成</div>
            <div class="summary-value" style="color: #67c23a;">{{ selectedIds.length }} 份</div>
          </div>
        </el-col>
      </el-row>
    </el-card>

    <!-- ====== Pending Contracts (can generate) ====== -->
    <el-card style="margin-bottom: 16px;">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center;">
          <span><el-icon style="vertical-align: middle;"><CirclePlus /></el-icon> 待生成</span>
          <el-button
            type="primary"
            size="small"
            :disabled="selectedIds.length === 0"
            :loading="batchLoading"
            @click="batchGenerate"
          >
            批量生成（{{ selectedIds.length }}）
          </el-button>
        </div>
      </template>

      <el-alert
        title="仅显示当前账期尚未生成过账单的合同。勾选后点击「批量生成」或逐条点击「生成」按钮。"
        type="info"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />

      <el-table
        ref="pendingTableRef"
        :data="pendingContracts"
        v-loading="loading"
        stripe
        @selection-change="onSelectionChange"
      >
        <el-table-column type="selection" width="45" />
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="tenantName" label="租客" width="90" />
        <el-table-column prop="roomName" label="房屋" width="120" />
        <el-table-column label="应收金额" width="120">
          <template #default="{ row }">¥{{ (row.estimatedAmount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="contractStatus" label="合同状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.contractStatus === 'Active' ? 'success' : 'info'" size="small">
              {{ row.contractStatus === 'Active' ? '活跃' : '暂停' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button
              text
              size="small"
              type="primary"
              :loading="row._generating"
              :disabled="batchLoading"
              @click="generateSingle(row)"
            >
              生成
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div v-if="!loading && pendingContracts.length === 0" style="text-align: center; padding: 40px 0; color: #909399;">
        <el-icon :size="32"><CircleCheckFilled /></el-icon>
        <p style="margin-top: 8px;">当前筛选条件下所有合同均已生成账单</p>
      </div>
    </el-card>

    <!-- ====== Generated Contracts (read-only) ====== -->
    <el-card>
      <template #header>
        <span><el-icon style="vertical-align: middle;"><Finished /></el-icon> 已生成（仅可查看/导出）</span>
      </template>

      <el-alert
        title="以下合同在当前账期已生成过账单，不可重复生成。"
        type="warning"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />

      <el-table :data="generatedContracts" v-loading="loading" stripe>
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="tenantName" label="租客" width="90" />
        <el-table-column prop="roomName" label="房屋" width="120" />
        <el-table-column prop="billNo" label="账单编号" width="170" />
        <el-table-column label="应收金额" width="120">
          <template #default="{ row }">¥{{ (row.estimatedAmount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="generatedAt" label="生成时间" width="160" />
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="previewBill(row)">
              <el-icon><View /></el-icon>预览
            </el-button>
            <el-button text size="small" type="success" @click="exportPdf(row)">
              <el-icon><Download /></el-icon>PDF
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div v-if="!loading && generatedContracts.length === 0" style="text-align: center; padding: 40px 0; color: #c0c4cc;">
        当前筛选条件下没有已生成账单的合同
      </div>
    </el-card>

    <!-- ====== Generate Result Dialog ====== -->
    <el-dialog v-model="showResult" title="生成结果" width="600px">
      <el-table :data="generateResults" stripe size="small">
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="tenantName" label="租客" width="90" />
        <el-table-column label="结果" width="100">
          <template #default="{ row }">
            <el-tag :type="row.success ? 'success' : 'danger'" size="small">
              {{ row.success ? '✅ 成功' : '❌ 失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="billNo" label="账单编号" width="170" />
        <el-table-column prop="message" label="说明" min-width="150" />
      </el-table>
      <template #footer>
        <el-button @click="showResult = false">关闭</el-button>
        <el-button type="primary" @click="$router.push('/bills')">查看账单列表</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '@/store/user'
import { getContracts, getBuildingList, getHousingUnits, generateDebitNotes, getDebitNotes, exportDebitNotePdf } from '@/api'

const router = useRouter()
const userStore = useUserStore()
const pendingTableRef = ref(null)
const loading = ref(false)

// ==================== Filters ====================
const filters = reactive({
  period: new Date(),
  buildingId: '',
  roomId: '',
  contractStatus: ''
})

const buildings = ref([])
const rooms = ref([])

const filteredRooms = computed(() => {
  if (!filters.buildingId) return rooms.value
  return rooms.value.filter(r => r.buildingId === filters.buildingId)
})

// ==================== Contract Data ====================
const allContracts = ref([])

function getPeriodLabel(d) {
  if (!d) { const n = new Date(); return `${n.getFullYear()}-${String(n.getMonth() + 1).padStart(2, '0')}` }
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
}

const matchedContracts = computed(() => {
  const periodLabel = getPeriodLabel(filters.period)
  let list = allContracts.value.filter(c => c.periodLabel === periodLabel || c.periodLabel === undefined)
  if (filters.buildingId) list = list.filter(c => c.buildingId === filters.buildingId)
  if (filters.roomId) list = list.filter(c => c.roomId === filters.roomId)
  if (filters.contractStatus) list = list.filter(c => c.contractStatus === filters.contractStatus)
  return list
})

const pendingContracts = computed(() =>
  matchedContracts.value.filter(c => !c.hasExistingBill)
)
const generatedContracts = computed(() =>
  matchedContracts.value.filter(c => c.hasExistingBill)
)

// ==================== Selection ====================
const selectedIds = ref([])
const batchLoading = ref(false)

function onSelectionChange(rows) {
  selectedIds.value = rows.map(r => r.id)
}

// ==================== Data Loading ====================

function getEffectiveCompanyId() {
  return userStore.effectiveCompanyId || userStore.homeCompanyId
}

async function loadBuildings() {
  try {
    const res = await getBuildingList()
    const items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    buildings.value = items.map(b => ({ id: b.id, name: b.name || b.buildingName || '' }))
  } catch { /* 静默 */ }
}

async function loadRooms() {
  try {
    const res = await getHousingUnits({ pageSize: 500 })
    const items = res.items || res.data || res || []
    rooms.value = items
      .filter(u => u.unitType === 'Room')
      .map(u => ({ id: u.id, name: u.fullCode || u.name || '', buildingId: u.buildingId || u.parentId || '' }))
  } catch { /* 静默 */ }
}

async function loadContracts() {
  loading.value = true
  try {
    const periodLabel = getPeriodLabel(filters.period)
    const res = await getContracts({ pageSize: 200, status: filters.contractStatus || undefined })
    const items = res.items || res.data || []

    // 获取已生成账单的合同 ID 集合
    let generatedContractIds = new Set()
    try {
      const debitNotes = await getDebitNotes({})
      // 后端返回所有 debit notes，过滤当前账期
      const dnItems = Array.isArray(debitNotes) ? debitNotes : (debitNotes.items || debitNotes.data || [])
      dnItems.filter(d => d.period === periodLabel).forEach(d => generatedContractIds.add(d.contractId))
    } catch { /* 静默 */ }

    allContracts.value = items.map(c => ({
      id: c.id,
      contractNo: c.contractNo,
      tenantName: c.tenants?.length > 0 ? c.tenants[0].tenantName : '',
      roomName: c.roomFullCode || '',
      buildingId: c.buildingId || '',
      roomId: c.roomId || '',
      estimatedAmount: c.rentAmount || 0,
      contractStatus: c.status || 'Active',
      hasExistingBill: generatedContractIds.has(c.id),
      billNo: '',
      generatedAt: '',
      isHistorical: false,
      periodLabel,
      _generating: false
    }))
  } catch {
    ElMessage.error('加载合同数据失败')
  }
  loading.value = false
}

// ==================== Generate ====================
const showResult = ref(false)
const generateResults = ref([])

async function generateSingle(row) {
  row._generating = true
  const result = await doGenerate(row)
  row._generating = false
  if (result.success) {
    row.hasExistingBill = true
    row.billNo = result.billNo
    row.generatedAt = result.generatedAt
  }
}

async function batchGenerate() {
  const selected = pendingContracts.value.filter(c => selectedIds.value.includes(c.id))
  if (selected.length === 0) return

  await ElMessageBox.confirm(
    `确定要为 ${selected.length} 份合同生成 ${getPeriodLabel(filters.period)} 账单吗？`,
    '确认批量生成',
    { confirmButtonText: '确认生成', cancelButtonText: '取消', type: 'info' }
  )

  batchLoading.value = true
  generateResults.value = []

  for (const row of selected) {
    row._generating = true
    const result = await doGenerate(row)
    row._generating = false
    if (result.success) {
      row.hasExistingBill = true
      row.billNo = result.billNo
      row.generatedAt = result.generatedAt
    }
  }

  batchLoading.value = false
  showResult.value = true
}

async function doGenerate(row) {
  const period = getPeriodLabel(filters.period)
  try {
    const res = await generateDebitNotes({ contractId: row.id, period })
    const result = {
      contractNo: row.contractNo,
      tenantName: row.tenantName,
      success: true,
      billNo: res.noteNo || res.id || '-',
      message: res.message || '生成成功'
    }
    generateResults.value.push(result)
    return { ...result, generatedAt: new Date().toISOString().replace('T', ' ').slice(0, 19) }
  } catch (e) {
    const msg = e?.response?.data?.message || e.message || '生成失败'
    const result = {
      contractNo: row.contractNo,
      tenantName: row.tenantName,
      success: false,
      billNo: '-',
      message: msg
    }
    generateResults.value.push(result)
    return result
  }
}

// ==================== Actions ====================
function previewBill(row) {
  router.push(`/bills/preview/${row.id}`)
}

async function exportPdf(row) {
  try {
    const blob = await exportDebitNotePdf(row.id)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${row.billNo || 'bill'}.pdf`
    a.click()
    window.URL.revokeObjectURL(url)
    ElMessage.success(`账单 ${row.billNo} PDF 已下载`)
  } catch {
    ElMessage.error('导出PDF失败')
  }
}

function searchContracts() {
  loadContracts()
}

function resetFilters() {
  filters.period = new Date()
  filters.buildingId = ''
  filters.roomId = ''
  filters.contractStatus = ''
  loadContracts()
}

onMounted(() => {
  loadBuildings()
  loadRooms()
  loadContracts()
})
</script>

<style scoped>
.filter-bar {
  padding: 4px 0;
}
.summary-item {
  text-align: center;
  padding: 12px 0;
}
.summary-label {
  font-size: 13px;
  color: #909399;
  margin-bottom: 6px;
}
.summary-value {
  font-size: 24px;
  font-weight: 700;
  color: #303133;
}
</style>
