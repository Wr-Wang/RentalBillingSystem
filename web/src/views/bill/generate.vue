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
            <div class="summary-label">已生成合同（仅可导出）</div>
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
        stripe
        @selection-change="onSelectionChange"
      >
        <el-table-column type="selection" width="45" />
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="tenantName" label="租客" width="90" />
        <el-table-column prop="roomName" label="房屋" width="120" />
        <el-table-column label="应收金额" width="120">
          <template #default="{ row }">¥{{ row.estimatedAmount?.toLocaleString() }}</template>
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

      <div v-if="pendingContracts.length === 0" style="text-align: center; padding: 40px 0; color: #909399;">
        <el-icon :size="32"><CircleCheckFilled /></el-icon>
        <p style="margin-top: 8px;">当前筛选条件下所有合同均已生成账单</p>
      </div>
    </el-card>

    <!-- ====== Generated Contracts (read-only, can only export) ====== -->
    <el-card>
      <template #header>
        <span><el-icon style="vertical-align: middle;"><Finished /></el-icon> 已生成（仅可查看/导出）</span>
      </template>

      <el-alert
        title="以下合同在当前账期已生成过账单，不可重复生成。可查看账单详情或导出PDF（已生成账单的PDF将标注「历史账单」标记）。"
        type="warning"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />

      <el-table :data="generatedContracts" stripe>
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="tenantName" label="租客" width="90" />
        <el-table-column prop="roomName" label="房屋" width="120" />
        <el-table-column prop="billNo" label="账单编号" width="170" />
        <el-table-column label="应收金额" width="120">
          <template #default="{ row }">¥{{ row.estimatedAmount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="generatedAt" label="生成时间" width="160" />
        <el-table-column label="历史标记" width="90" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.isHistorical" type="warning" size="small">历史</el-tag>
            <el-tag v-else type="success" size="small">当前</el-tag>
          </template>
        </el-table-column>
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

      <div v-if="generatedContracts.length === 0" style="text-align: center; padding: 40px 0; color: #c0c4cc;">
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
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()
const pendingTableRef = ref(null)

// ==================== Filters ====================
const filters = reactive({
  period: new Date(),
  buildingId: '',
  roomId: '',
  contractStatus: ''
})

const buildings = ref([
  { id: 'bld1', name: 'A栋' },
  { id: 'bld2', name: 'B栋' },
  { id: 'bld3', name: 'C栋' }
])

const rooms = ref([
  { id: 'r1', name: '101', buildingId: 'bld1' },
  { id: 'r2', name: '102', buildingId: 'bld1' },
  { id: 'r3', name: '201', buildingId: 'bld2' },
  { id: 'r4', name: '202', buildingId: 'bld2' },
  { id: 'r5', name: '301', buildingId: 'bld3' }
])

const filteredRooms = computed(() => {
  if (!filters.buildingId) return rooms.value
  return rooms.value.filter(r => r.buildingId === filters.buildingId)
})

// ==================== Contract Data ====================
const allContracts = ref([
  {
    id: 'c1', contractNo: 'HT-2026-001', tenantName: '张三', roomName: 'A栋-101',
    buildingId: 'bld1', roomId: 'r1', estimatedAmount: 5460, contractStatus: 'Active',
    hasExistingBill: true, billNo: 'ZD-202606-00001', generatedAt: '2026-06-25 08:00:30',
    isHistorical: false, periodLabel: '2026-06'
  },
  {
    id: 'c2', contractNo: 'HT-2026-002', tenantName: '李四', roomName: 'A栋-102',
    buildingId: 'bld1', roomId: 'r2', estimatedAmount: 4030, contractStatus: 'Active',
    hasExistingBill: true, billNo: 'ZD-202606-00002', generatedAt: '2026-06-25 08:00:30',
    isHistorical: false, periodLabel: '2026-06'
  },
  {
    id: 'c3', contractNo: 'HT-2026-003', tenantName: '王五', roomName: 'B栋-201',
    buildingId: 'bld2', roomId: 'r3', estimatedAmount: 7030, contractStatus: 'Active',
    hasExistingBill: true, billNo: 'ZD-202606-00003', generatedAt: '2026-06-25 08:00:30',
    isHistorical: false, periodLabel: '2026-06'
  },
  {
    id: 'c4', contractNo: 'HT-2026-004', tenantName: '赵六', roomName: 'B栋-202',
    buildingId: 'bld2', roomId: 'r4', estimatedAmount: 6200, contractStatus: 'Active',
    hasExistingBill: true, billNo: 'ZD-202606-00004', generatedAt: '2026-06-27 09:15:00',
    isHistorical: true, periodLabel: '2026-06'
  },
  {
    id: 'c5', contractNo: 'HT-2026-005', tenantName: '孙七', roomName: 'C栋-301',
    buildingId: 'bld3', roomId: 'r5', estimatedAmount: 4500, contractStatus: 'Active',
    hasExistingBill: false, billNo: null, generatedAt: null,
    isHistorical: false, periodLabel: '2026-06'
  },
  {
    id: 'c6', contractNo: 'HT-2026-006', tenantName: '周八', roomName: 'A栋-103',
    buildingId: 'bld1', roomId: null, estimatedAmount: 5200, contractStatus: 'Active',
    hasExistingBill: false, billNo: null, generatedAt: null,
    isHistorical: false, periodLabel: '2026-06'
  },
  {
    id: 'c7', contractNo: 'HT-2026-007', tenantName: '吴九', roomName: 'B栋-203',
    buildingId: 'bld2', roomId: null, estimatedAmount: 3800, contractStatus: 'Suspended',
    hasExistingBill: false, billNo: null, generatedAt: null,
    isHistorical: false, periodLabel: '2026-06'
  }
])

function getPeriodLabel(d) {
  if (!d) { const n = new Date(); return `${n.getFullYear()}-${String(n.getMonth() + 1).padStart(2, '0')}` }
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
}

const matchedContracts = computed(() => {
  const periodLabel = getPeriodLabel(filters.period)
  let list = allContracts.value.filter(c => c.periodLabel === periodLabel)
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

// ==================== Selection (pending only) ====================
const selectedIds = ref([])
const batchLoading = ref(false)

function onSelectionChange(rows) {
  selectedIds.value = rows.map(r => r.id)
}

// ==================== Generate ====================
const showResult = ref(false)
const generateResults = ref([])

async function generateSingle(row) {
  row._generating = true
  const success = await doGenerate(row)
  row._generating = false
  if (success) {
    row.hasExistingBill = true
    row.isHistorical = false
    row.billNo = `ZD-${getPeriodLabel(filters.period).replace('-', '')}-${String(row.contractNo.slice(-5)).padStart(5, '0')}`
    row.generatedAt = new Date().toISOString().replace('T', ' ').slice(0, 19)
    ElMessage.success(`${row.contractNo} 账单已生成`)
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
    const success = await doGenerate(row)
    row._generating = false
    if (success) {
      row.hasExistingBill = true
      row.isHistorical = false
      row.billNo = `ZD-${getPeriodLabel(filters.period).replace('-', '')}-${String(row.contractNo.slice(-5)).padStart(5, '0')}`
      row.generatedAt = new Date().toISOString().replace('T', ' ').slice(0, 19)
    }
  }

  batchLoading.value = false
  showResult.value = true
}

async function doGenerate(row) {
  await new Promise(resolve => setTimeout(resolve, 600 + Math.random() * 400))
  const success = Math.random() > 0.05
  generateResults.value.push({
    contractNo: row.contractNo,
    tenantName: row.tenantName,
    success,
    billNo: success ? row.billNo || `ZD-${getPeriodLabel(filters.period).replace('-', '')}-${String(row.contractNo.slice(-5)).padStart(5, '0')}` : '-',
    message: success ? '首次生成' : '生成失败，请重试'
  })
  return success
}

// ==================== Actions ====================
function previewBill(row) {
  router.push(`/bills/preview/${row.id}`)
}

function exportPdf(row) {
  ElMessage.success(`账单 ${row.billNo} 的 PDF 已加入导出队列（${row.isHistorical ? '历史账单' : '当前账单'}）`)
}

function searchContracts() {
  // Filters are reactive, computed will re-evaluate
}

function resetFilters() {
  filters.period = new Date()
  filters.buildingId = ''
  filters.roomId = ''
  filters.contractStatus = ''
}
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
