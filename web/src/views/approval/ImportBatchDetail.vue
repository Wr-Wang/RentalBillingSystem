<template>
  <div v-loading="loading">
    <div class="page-header">
      <h2>导入批次详情</h2>
      <div class="table-actions">
        <el-button @click="$router.back()">返回</el-button>
      </div>
    </div>

    <template v-if="batch">
      <!-- 批次信息 -->
      <el-card style="margin-bottom: 16px;">
        <template #header>批次信息</template>
        <el-descriptions :column="3" border>
          <el-descriptions-item label="文件名称">{{ batch.fileName || '-' }}</el-descriptions-item>
          <el-descriptions-item label="导入类型">{{ batch.importType || '-' }}</el-descriptions-item>
          <el-descriptions-item label="批次状态">
            <el-tag :type="batchStatusType(batch.status)" size="small">{{ batch.statusLabel || batch.status }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="总行数">{{ batch.totalRows ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="有效行">{{ batch.validRows ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="失败行">{{ batch.failedRows ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatTime(batch.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="创建人">{{ batch.createdBy || '-' }}</el-descriptions-item>
          <el-descriptions-item label="审批请求ID">{{ batch.approvalRequestId?.slice(0, 8) || '-' }}</el-descriptions-item>
        </el-descriptions>
      </el-card>

      <!-- 导入行明细 -->
      <el-card>
        <template #header>
          <span>导入行明细（共 {{ batch.items?.length || 0 }} 行）</span>
          <div style="float: right;">
            <el-radio-group v-model="filterStatus" size="small" @change="applyFilter">
              <el-radio-button label="all">全部</el-radio-button>
              <el-radio-button label="valid">有效</el-radio-button>
              <el-radio-button label="fail">失败</el-radio-button>
            </el-radio-group>
          </div>
        </template>
        <el-table :data="filteredItems" stripe max-height="500">
          <el-table-column prop="rowIndex" label="行号" width="70" />
          <el-table-column label="结果" width="70">
            <template #default="{ row }">
              <el-tag :type="row.isValid ? 'success' : 'danger'" size="small">
                {{ row.isValid ? '通过' : '失败' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="buildingName" label="座楼" width="100" />
          <el-table-column prop="floorName" label="楼层" width="80" />
          <el-table-column prop="unitNo" label="房号" width="80" />
          <el-table-column prop="fullCode" label="完整编码" width="180" />
          <el-table-column prop="roomTypeName" label="房型" width="120" />
          <el-table-column prop="area" label="面积" width="80">
            <template #default="{ row }">{{ row.area ?? '-' }}</template>
          </el-table-column>
          <el-table-column prop="orientation" label="朝向" width="80" />
          <el-table-column prop="baseRentAmount" label="标准租金" width="100">
            <template #default="{ row }">{{ row.baseRentAmount ? '¥' + row.baseRentAmount.toLocaleString() : '-' }}</template>
          </el-table-column>
          <el-table-column label="错误信息" min-width="200">
            <template #default="{ row }">
              <span v-if="!row.isValid" style="color: #f56c6c;">{{ row.errorMessage }}</span>
              <span v-else-if="row.priceWarning" style="color: #e6a23c;">⚠ {{ row.priceWarning }}</span>
              <span v-else style="color: #909399;">—</span>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </template>

    <el-empty v-else-if="!loading" description="未找到该导入批次" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getImportBatch } from '../../api/index'

const route = useRoute()
const loading = ref(false)
const batch = ref(null)
const filterStatus = ref('all')

const filteredItems = computed(() => {
  if (!batch.value?.items) return []
  const items = batch.value.items
  if (filterStatus.value === 'valid') return items.filter(i => i.isValid)
  if (filterStatus.value === 'fail') return items.filter(i => !i.isValid)
  return items
})

function batchStatusType(status) {
  const map = { PendingApproval: 'warning', Approved: 'success', Rejected: 'danger', Cancelled: 'info' }
  return map[status] || 'info'
}

function formatTime(t) {
  if (!t) return ''
  const d = new Date(t)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function applyFilter() { /* computed handles this */ }

onMounted(async () => {
  loading.value = true
  try {
    const res = await getImportBatch(route.params.id)
    batch.value = res || null
  } catch { batch.value = null }
  loading.value = false
})
</script>
