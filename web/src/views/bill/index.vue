<template>
  <div>
    <div class="page-header">
      <h2>账单管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="$router.push('/bills/generate')">
          <el-icon><Plus /></el-icon>生成账单
        </el-button>
        <el-button @click="fetchList">
          <el-icon><Refresh /></el-icon>刷新
        </el-button>
      </div>
    </div>

    <!-- Search / Filter -->
    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="合同号/租客" clearable style="width: 180px;" @clear="fetchList" />
      <el-select v-model="search.contractId" placeholder="合同" clearable filterable style="width: 160px;">
        <el-option v-for="c in contracts" :key="c.id" :label="c.contractNo + ' - ' + c.tenantName" :value="c.id" />
      </el-select>
      <el-date-picker v-model="search.period" type="month" placeholder="账期" style="width: 140px;" />
      <el-select v-model="search.status" placeholder="状态" clearable style="width: 110px;">
        <el-option label="待收款" value="Pending" />
        <el-option label="部分已收" value="Partial" />
        <el-option label="已付清" value="Paid" />
        <el-option label="已取消" value="Cancelled" />
      </el-select>
      <el-button type="primary" @click="fetchList">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <!-- Bill Table -->
    <el-card>
      <div class="table-toolbar">
        <div class="toolbar-left">
          <el-checkbox
            v-model="selectAll"
            :indeterminate="isIndeterminate"
            @change="onSelectAllChange"
          >
            全选
          </el-checkbox>
          <span style="margin-left: 8px; color: #909399; font-size: 13px;">
            已选 {{ selectedIds.length }} 条
          </span>
          <el-tag v-if="selectedHistoricalCount > 0" type="warning" size="small" style="margin-left: 8px;">
            其中 {{ selectedHistoricalCount }} 条为历史账单
          </el-tag>
        </div>
        <div class="toolbar-right">
          <el-button
            :disabled="selectedIds.length === 0"
            :loading="batchExportLoading"
            @click="batchExportPdf"
          >
            <el-icon><Download /></el-icon>批量导出PDF
            <el-tag v-if="selectedHistoricalCount > 0" size="small" type="warning" style="margin-left: 4px;">
              含历史
            </el-tag>
          </el-button>
        </div>
      </div>

      <el-table
        ref="tableRef"
        :data="billList"
        stripe
        @sort-change="onSortChange"
        @selection-change="onSelectionChange"
      >
        <el-table-column type="selection" width="45" />
        <el-table-column prop="billNo" label="账单编号" width="170" sortable="custom" />
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="tenantName" label="租客" width="100" />
        <el-table-column prop="roomName" label="房屋" width="120" />
        <el-table-column prop="period" label="账期" width="80" sortable="custom" />
        <el-table-column prop="dueDate" label="到期日" width="100" sortable="custom" />
        <el-table-column prop="totalAmount" label="应收总额" width="120" sortable="custom">
          <template #default="{ row }">¥{{ row.totalAmount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="totalReceived" label="已收" width="110" sortable="custom">
          <template #default="{ row }">¥{{ row.totalReceived?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="欠费" width="110">
          <template #default="{ row }">
            <span v-if="row.totalAmount - row.totalReceived > 0" style="color: #f56c6c; font-weight: 600;">
              ¥{{ (row.totalAmount - row.totalReceived).toLocaleString() }}
            </span>
            <span v-else style="color: #67c23a;">已结清</span>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }">
            <el-tag
              :type="row.status === 'Paid' ? 'success' : row.status === 'Partial' ? 'warning' : row.status === 'Cancelled' ? 'info' : 'danger'"
              size="small"
            >
              {{ row.status === 'Paid' ? '已付清' : row.status === 'Partial' ? '部分' : row.status === 'Cancelled' ? '已取消' : '待收款' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="历史标记" width="80" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.isHistorical" type="warning" size="small">历史</el-tag>
            <span v-else style="color: #c0c4cc;">-</span>
          </template>
        </el-table-column>
        <el-table-column prop="generatedAt" label="生成时间" width="160" sortable="custom" />
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="previewBill(row)">
              <el-icon><View /></el-icon>预览
            </el-button>
            <el-button
              text
              size="small"
              :type="row.isHistorical ? 'warning' : 'success'"
              @click="exportPdf(row)"
            >
              <el-icon><Download /></el-icon>{{ row.isHistorical ? '历史PDF' : 'PDF' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- Pagination -->
      <div style="display: flex; justify-content: flex-end; margin-top: 16px;">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next"
          @size-change="fetchList"
          @current-change="fetchList"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const router = useRouter()
const tableRef = ref(null)

// Search state
const search = reactive({ keyword: '', contractId: '', period: null, status: '' })
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)

// Selection
const selectedIds = ref([])
const selectAll = ref(false)
const isIndeterminate = ref(false)
const batchExportLoading = ref(false)

// Contracts list for filter dropdown
const contracts = ref([
  { id: 'c1', contractNo: 'HT-2026-001', tenantName: '张三' },
  { id: 'c2', contractNo: 'HT-2026-002', tenantName: '李四' },
  { id: 'c3', contractNo: 'HT-2026-003', tenantName: '王五' },
  { id: 'c4', contractNo: 'HT-2026-004', tenantName: '赵六' },
  { id: 'c5', contractNo: 'HT-2026-005', tenantName: '孙七' }
])

// Bill list — with isHistorical flag
const billList = ref([
  {
    id: 'b1', billNo: 'ZD-202606-00001', contractId: 'c1', contractNo: 'HT-2026-001',
    tenantName: '张三', roomName: 'A栋-101', period: '2026-06', dueDate: '2026-06-05',
    totalAmount: 5460, totalReceived: 5460, status: 'Paid',
    generatedAt: '2026-06-25 08:00:30', isHistorical: false
  },
  {
    id: 'b2', billNo: 'ZD-202606-00002', contractId: 'c2', contractNo: 'HT-2026-002',
    tenantName: '李四', roomName: 'A栋-102', period: '2026-06', dueDate: '2026-06-05',
    totalAmount: 4030, totalReceived: 4000, status: 'Partial',
    generatedAt: '2026-06-25 08:00:30', isHistorical: false
  },
  {
    id: 'b3', billNo: 'ZD-202606-00003', contractId: 'c3', contractNo: 'HT-2026-003',
    tenantName: '王五', roomName: 'B栋-201', period: '2026-06', dueDate: '2026-06-05',
    totalAmount: 7030, totalReceived: 7030, status: 'Paid',
    generatedAt: '2026-06-25 08:00:30', isHistorical: false
  },
  {
    id: 'b4', billNo: 'ZD-202606-00004', contractId: 'c4', contractNo: 'HT-2026-004',
    tenantName: '赵六', roomName: 'B栋-202', period: '2026-06', dueDate: '2026-06-05',
    totalAmount: 6200, totalReceived: 0, status: 'Pending',
    generatedAt: '2026-06-27 09:15:00', isHistorical: true
  },
  {
    id: 'b5', billNo: 'ZD-202606-00005', contractId: 'c5', contractNo: 'HT-2026-005',
    tenantName: '孙七', roomName: 'C栋-301', period: '2026-06', dueDate: '2026-06-05',
    totalAmount: 4500, totalReceived: 0, status: 'Pending',
    generatedAt: '2026-06-25 08:00:30', isHistorical: false
  },
  {
    id: 'b6', billNo: 'ZD-202607-00001', contractId: 'c1', contractNo: 'HT-2026-001',
    tenantName: '张三', roomName: 'A栋-101', period: '2026-07', dueDate: '2026-07-05',
    totalAmount: 5460, totalReceived: 2000, status: 'Partial',
    generatedAt: '2026-06-25 08:00:30', isHistorical: false
  },
  {
    id: 'b7', billNo: 'ZD-202607-00002', contractId: 'c2', contractNo: 'HT-2026-002',
    tenantName: '李四', roomName: 'A栋-102', period: '2026-07', dueDate: '2026-07-05',
    totalAmount: 4030, totalReceived: 0, status: 'Pending',
    generatedAt: '2026-06-25 08:00:30', isHistorical: false
  }
])

const selectedHistoricalCount = computed(() =>
  billList.value.filter(b => selectedIds.value.includes(b.id) && b.isHistorical).length
)

function fetchList() {
  total.value = billList.value.length
}

function resetSearch() {
  search.keyword = ''
  search.contractId = ''
  search.period = null
  search.status = ''
  fetchList()
}

function onSortChange({ prop, order }) {
  if (!prop || !order) return
  billList.value.sort((a, b) => {
    const va = a[prop], vb = b[prop]
    if (typeof va === 'string') return order === 'ascending' ? va.localeCompare(vb) : vb.localeCompare(va)
    return order === 'ascending' ? va - vb : vb - va
  })
}

// Selection
function onSelectionChange(rows) {
  selectedIds.value = rows.map(r => r.id)
  selectAll.value = rows.length === billList.value.length
  isIndeterminate.value = rows.length > 0 && rows.length < billList.value.length
}

function onSelectAllChange(val) {
  tableRef.value?.toggleAllSelection()
}

// Actions
function previewBill(row) {
  router.push(`/bills/preview/${row.id}`)
}

function exportPdf(row) {
  const tag = row.isHistorical ? '历史账单' : '当前账单'
  ElMessage.success(`账单 ${row.billNo}（${tag}）的 PDF 正在生成，${row.isHistorical ? 'PDF已标注「历史账单」标记' : ''}`)
}

async function batchExportPdf() {
  const selected = billList.value.filter(b => selectedIds.value.includes(b.id))
  if (selected.length === 0) return

  const histCount = selected.filter(b => b.isHistorical).length
  batchExportLoading.value = true
  await new Promise(resolve => setTimeout(resolve, 1500))

  ElMessage.success(
    `批量导出完成，共 ${selected.length} 份账单 PDF` +
    (histCount > 0 ? `（其中 ${histCount} 份已标注「历史账单」标记）` : '')
  )
  batchExportLoading.value = false
}
</script>

<style scoped>
.table-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}
.toolbar-left {
  display: flex;
  align-items: center;
}
</style>
