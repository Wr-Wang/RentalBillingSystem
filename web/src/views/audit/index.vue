<template>
  <div>
    <div class="page-header">
      <h2>变更审计</h2>
    </div>

    <!-- 搜索栏 -->
    <div class="search-bar">
      <el-select v-model="search.tableName" placeholder="选择实体类型" clearable style="width: 160px;" @change="fetchHistory">
        <el-option label="公司" value="Companies" />
        <el-option label="审批类型" value="ApprovalTypes" />
        <el-option label="审批级别" value="ApprovalLevelConfigs" />
        <el-option label="房型" value="RoomTypes" />
        <el-option label="收费项目" value="FeeCodes" />
        <el-option label="菜单" value="Menus" />
        <el-option label="角色" value="Roles" />
        <el-option label="用户" value="Users" />
        <el-option label="合同" value="Contracts" />
        <el-option label="房间" value="Rooms" />
        <el-option label="应收" value="ReceivablePlans" />
        <el-option label="收款" value="Receipts" />
      </el-select>
      <el-input v-model="search.recordId" placeholder="记录ID" clearable style="width: 200px;" @clear="fetchHistory" @keyup.enter="fetchHistory" />
      <el-date-picker
        v-model="search.dateRange"
        type="daterange"
        range-separator="至"
        start-placeholder="开始日期"
        end-placeholder="结束日期"
        style="width: 220px;"
        @change="fetchHistory"
      />
      <el-button type="primary" @click="fetchHistory">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <!-- 审计统计 -->
    <el-card style="margin-bottom: 16px;">
      <template #header>审计统计</template>
      <el-row :gutter="16">
        <el-col :span="6">今日变更: {{ stats.todayCount }} 次</el-col>
        <el-col :span="6">本周变更: {{ stats.weekCount }} 次</el-col>
        <el-col :span="6">本月变更: {{ stats.monthCount }} 次</el-col>
        <el-col :span="6">涉及表: {{ stats.totalTables }} 张</el-col>
      </el-row>
    </el-card>

    <!-- 变更时间线 -->
    <el-card>
      <template #header>变更时间线</template>
      <el-timeline v-if="auditLogs.length > 0">
        <el-timeline-item
          v-for="(log, index) in auditLogs"
          :key="index"
          :timestamp="formatTime(log.auditChangedAt)"
          :type="log.auditAction === 'Update' ? 'primary' : log.auditAction === 'Insert' ? 'success' : 'danger'"
        >
          <h4>
            {{ log.auditAction === 'Insert' ? '创建' : log.auditAction === 'Update' ? '更新' : '删除' }}
            - {{ search.tableName || '未知' }}
          </h4>
          <p>记录ID: {{ log.entityId }}</p>
          <p v-if="log.changes && Object.keys(log.changes).length > 0" style="color: #909399; font-size: 12px;">
            <span v-for="(val, key) in log.changes" :key="key">
              {{ key }}: {{ val === null ? 'null' : val }} &nbsp;
            </span>
          </p>
          <el-button v-if="auditLogs.length >= 2" text size="small" type="primary" @click="compareVersion(log)">
            版本对比
          </el-button>
        </el-timeline-item>
      </el-timeline>
      <el-empty v-else description="暂无审计记录" />
    </el-card>

    <!-- 分页 -->
    <div style="margin-top: 16px; display: flex; justify-content: flex-end;">
      <el-pagination
        v-model:current-page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @change="fetchHistory"
      />
    </div>

    <!-- 版本对比 Dialog -->
    <el-dialog v-model="showCompare" title="版本对比" width="700px">
      <el-table :data="compareData" stripe>
        <el-table-column prop="field" label="字段" width="150" />
        <el-table-column prop="oldValue" label="旧值" min-width="200" />
        <el-table-column prop="newValue" label="新值" min-width="200" />
        <el-table-column label="变化" width="80">
          <template #default="{ row }">
            <el-tag v-if="row.changed" type="warning" size="small">已变更</el-tag>
            <el-tag v-else type="info" size="small">不变</el-tag>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="showCompare = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getAuditHistory, getAuditStats, compareAuditVersions } from '../../api/index'

const search = reactive({
  tableName: 'Companies',
  recordId: '',
  dateRange: null
})

const pagination = reactive({
  page: 1,
  pageSize: 10,
  total: 0
})

const stats = reactive({
  todayCount: 0,
  weekCount: 0,
  monthCount: 0,
  totalTables: 0
})

const auditLogs = ref([])
const showCompare = ref(false)
const compareData = ref([])
const loading = ref(false)

async function fetchHistory() {
  if (!search.tableName) return
  loading.value = true
  try {
    const params = {
      page: pagination.page,
      pageSize: pagination.pageSize,
      recordId: search.recordId || undefined
    }
    if (search.dateRange && search.dateRange.length === 2) {
      params.startDate = search.dateRange[0].toISOString()
      params.endDate = search.dateRange[1].toISOString()
    }
    const res = await getAuditHistory(search.tableName, params)
    auditLogs.value = res.items || []
    pagination.total = res.total || 0
  } catch (e) {
    auditLogs.value = []
    pagination.total = 0
  }
  loading.value = false
}

async function fetchStats() {
  try {
    const res = await getAuditStats()
    if (res) {
      stats.todayCount = res.todayCount ?? 0
      stats.weekCount = res.weekCount ?? 0
      stats.monthCount = res.monthCount ?? 0
      stats.totalTables = res.totalTables ?? 0
    }
  } catch (e) {
    // 静默失败，保留默认值
  }
}

function resetSearch() {
  search.tableName = 'Companies'
  search.recordId = ''
  search.dateRange = null
  pagination.page = 1
  fetchHistory()
}

function formatTime(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}

async function compareVersion(log) {
  try {
    // 获取该记录的所有审计版本，取前两个做对比
    const res = await compareAuditVersions(search.tableName, log.entityId, 1, 1)
    if (res && res.length > 0) {
      compareData.value = res
    } else {
      compareData.value = [
        { field: '（示例）', oldValue: '无历史版本', newValue: '-', changed: false }
      ]
    }
    showCompare.value = true
  } catch (e) {
    ElMessage.warning('版本对比暂时不可用')
  }
}

onMounted(() => {
  fetchHistory()
  fetchStats()
})
</script>
