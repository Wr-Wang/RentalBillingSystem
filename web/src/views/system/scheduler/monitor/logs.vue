<template>
  <div>
    <div class="page-header">
      <h2>执行日志</h2>
      <div style="display:flex;gap:8px;">
        <el-button size="small" @click="$router.push('/system/scheduler/monitor')">◀ 返回总览</el-button>
        <el-button size="small" :loading="loading" @click="fetchLogs">🔄 刷新</el-button>
      </div>
    </div>

    <!-- 筛选栏 -->
    <el-card shadow="never" style="margin-bottom:12px;">
      <el-form :inline="true" size="small" label-width="0">
        <el-form-item>
          <el-select v-model="query.taskName" placeholder="任务名称" clearable style="width:150px;">
            <el-option label="全部任务" value="" />
            <el-option v-for="t in taskNames" :key="t" :label="t" :value="t" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-select v-model="query.status" placeholder="状态" clearable style="width:110px;">
            <el-option label="全部" value="" />
            <el-option label="成功" value="Completed" />
            <el-option label="失败" value="Failed" />
            <el-option label="运行中" value="Running" />
            <el-option label="僵死" value="Stale" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-select v-model="query.triggerType" placeholder="触发方式" clearable style="width:110px;">
            <el-option label="全部" value="" />
            <el-option label="自动" value="Scheduled" />
            <el-option label="手动" value="Manual" />
            <el-option label="事件" value="Event" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-select v-model="query.runMode" placeholder="模式" clearable style="width:90px;">
            <el-option label="全部" value="" />
            <el-option label="执行" value="Execute" />
            <el-option label="预执行" value="DryRun" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-select v-model="timeRange" placeholder="时间范围" clearable style="width:130px;" @change="onTimeRangeChange">
            <el-option label="全部" value="" />
            <el-option label="今日" value="today" />
            <el-option label="近7天" value="7d" />
            <el-option label="近30天" value="30d" />
            <el-option label="近90天" value="90d" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-input v-model="query.keyword" placeholder="搜索摘要/任务名" clearable style="width:180px;" @keyup.enter="fetchLogs" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchLogs">查询</el-button>
          <el-button @click="resetQuery">清空</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 批量操作栏 -->
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:10px;">
      <span style="font-size:13px;color:#909399;">
        共 <strong>{{ total }}</strong> 条记录
      </span>
      <div style="display:flex;gap:8px;">
        <el-button size="small" :disabled="selectedIds.length===0" @click="batchRetry">批量重试 ({{ selectedIds.length }})</el-button>
        <el-button size="small" type="danger" :disabled="selectedIds.length===0" @click="batchReverse">批量反转 ({{ selectedIds.length }})</el-button>
      </div>
    </div>

    <!-- 日志表格 -->
    <el-table :data="logs" stripe size="small" v-loading="loading"
      @selection-change="onSelectionChange" @row-click="openStepDrawer" style="cursor:pointer;">
      <el-table-column type="selection" width="36" @click.stop />
      <el-table-column label="任务" width="130">
        <template #default="{row}">
          <span>{{ row.taskName }}</span>
        </template>
      </el-table-column>
      <el-table-column label="执行时间" width="160">
        <template #default="{row}">{{ formatDate(row.startedAt) }}</template>
      </el-table-column>
      <el-table-column label="状态" width="80">
        <template #default="{row}">
          <el-tag :type="statusType(row.status)" size="small" effect="dark" round>
            {{ statusLabel(row.status) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="模式" width="60">
        <template #default="{row}">
          <el-tag v-if="row.runMode==='DryRun'" size="small" type="warning" effect="plain">预执行</el-tag>
          <span v-else style="color:#909399;font-size:12px;">执行</span>
        </template>
      </el-table-column>
      <el-table-column label="触发" width="60">
        <template #default="{row}">{{ triggerLabel(row.triggerType) }}</template>
      </el-table-column>
      <el-table-column label="耗时" width="60">
        <template #default="{row}">{{ formatDuration(row.totalDurationMs) }}</template>
      </el-table-column>
      <el-table-column label="结果" min-width="200">
        <template #default="{row}">{{ row.summary || row.errorMessage || '-' }}</template>
      </el-table-column>
      <el-table-column label="成功/失败" width="80">
        <template #default="{row}">
          <span v-if="row.successCount != null" style="color:#67c23a;">{{ row.successCount }}</span>
          <span v-if="row.failCount" style="color:#f56c6c;">/{{ row.failCount }}</span>
          <span v-else-if="row.successCount == null">-</span>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="100" fixed="right" @click.stop>
        <template #default="{row}">
          <el-button text size="small" @click.stop="openStepDrawer(row)">详情</el-button>
          <el-button v-if="row.status==='Failed'" text type="primary" size="small" @click.stop="handleRetry(row)">重试</el-button>
          <el-button v-if="row.status==='Completed'&&row.runMode!=='DryRun'&&row.taskName==='BillJob'"
            text type="warning" size="small" @click.stop="handleReverse(row)">反转</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分页 -->
    <div style="display:flex;justify-content:flex-end;margin-top:12px;">
      <el-pagination
        v-model:current-page="query.page"
        :page-size="query.pageSize"
        :total="total"
        :page-sizes="[10,20,50,100]"
        layout="sizes, prev, pager, next"
        @current-change="fetchLogs"
        @size-change="onPageSizeChange"
      />
    </div>

    <!-- 步骤详情 Drawer -->
    <el-drawer v-model="stepDrawerVisible" title="步骤详情" :size="700" destroy-on-close>
      <StepWaterfall :log-detail="currentDetail" :steps="currentSteps" />
    </el-drawer>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  queryMonitorLogs, getMonitorLogDetail,
  previewReverse
} from '../../../../api/index'
import { executeJob, reverseTask } from '../../../../api/index'
import StepWaterfall from './components/StepWaterfall.vue'

const loading = ref(false)
const logs = ref([])
const total = ref(0)
const selectedIds = ref([])
const taskNames = ['BillJob', 'SettleJob', 'AutoRenewJob', 'CollectionJob', 'RenewalReminderJob']

const query = reactive({
  taskName: '', status: '', triggerType: '', runMode: '',
  keyword: '', startTime: '', endTime: '', page: 1, pageSize: 20
})
const timeRange = ref('30d')

function onTimeRangeChange(val) {
  const now = new Date()
  const end = now.toISOString()
  if (!val) { query.startTime = ''; query.endTime = ''; return }
  const start = new Date(now)
  if (val === 'today') { start.setHours(0,0,0,0) }
  else if (val === '7d') { start.setDate(start.getDate() - 7) }
  else if (val === '30d') { start.setDate(start.getDate() - 30) }
  else if (val === '90d') { start.setDate(start.getDate() - 90) }
  query.startTime = start.toISOString()
  query.endTime = end
}

function resetQuery() {
  query.taskName = ''; query.status = ''; query.triggerType = ''
  query.runMode = ''; query.keyword = ''; query.startTime = ''
  query.endTime = ''; query.page = 1
  timeRange.value = ''
  fetchLogs()
}

function onPageSizeChange(size) {
  query.pageSize = size
  query.page = 1
  fetchLogs()
}

async function fetchLogs() {
  loading.value = true
  try {
    const r = await queryMonitorLogs({
      taskName: query.taskName || undefined,
      status: query.status || undefined,
      triggerType: query.triggerType || undefined,
      runMode: query.runMode || undefined,
      keyword: query.keyword || undefined,
      startTime: query.startTime || undefined,
      endTime: query.endTime || undefined,
      page: query.page,
      pageSize: query.pageSize
    })
    logs.value = r.items || []
    total.value = r.total || 0
  } catch {
    logs.value = []
    total.value = 0
  }
  loading.value = false
}

function onSelectionChange(rows) {
  selectedIds.value = rows.map(r => r.id)
}

// 步骤 Drawer
const stepDrawerVisible = ref(false)
const currentDetail = ref(null)
const currentSteps = ref([])

async function openStepDrawer(row) {
  try {
    const detail = await getMonitorLogDetail(row.id)
    currentDetail.value = detail
    currentSteps.value = detail.steps || []
    stepDrawerVisible.value = true
  } catch {
    ElMessage.error('获取步骤详情失败')
  }
}

// 重试
async function handleRetry(row) {
  try {
    await ElMessageBox.confirm(`确认重试任务"${row.taskName}"？`, '确认', { type: 'info' })
    await executeJob(row.taskName, {
      mode: 'execute',
      companyId: '00000000-0000-0000-0000-000000000000',
      targetMonth: row.targetMonth || new Date().toISOString().slice(0, 7)
    })
    ElMessage.success('重试任务已触发')
    await fetchLogs()
  } catch { /* cancelled or error */ }
}

async function batchRetry() {
  if (selectedIds.value.length === 0) return
  try {
    await ElMessageBox.confirm(`确认重试 ${selectedIds.value.length} 条失败任务？`, '确认', { type: 'warning' })
    // 仅重试失败的任务
    const failedLogs = logs.value.filter(l => selectedIds.value.includes(l.id) && l.status === 'Failed')
    for (const log of failedLogs) {
      await executeJob(log.taskName, {
        mode: 'execute',
        companyId: '00000000-0000-0000-0000-000000000000',
        targetMonth: log.targetMonth
      })
    }
    ElMessage.success(`已触发 ${failedLogs.length} 条重试`)
    await fetchLogs()
  } catch {}
}

// 反转
async function handleReverse(row) {
  try {
    // 预览
    const preview = await previewReverse(row.id)
    if (preview.hasPayment) {
      ElMessage.error('该账期已有收款记录，禁止反转')
      return
    }
    await ElMessageBox.confirm(
      `反转后将影响：\n📄 账单 ${preview.debitNoteCount} 条\n📊 应收计划 ${preview.receivablePlanCount} 条\n📝 凭证 ${preview.voucherCount} 条\n\n确认反转？`,
      '反转预览',
      { type: 'warning', confirmButtonText: '确认反转', cancelButtonText: '取消' }
    )
    await reverseTask(row.id, { reason: '管理员反转' })
    ElMessage.success('已反转')
    await fetchLogs()
  } catch { /* cancelled or error */ }
}

async function batchReverse() {
  if (selectedIds.value.length === 0) return
  try {
    await ElMessageBox.confirm(`确认反转选中的 ${selectedIds.value.length} 条？`, '确认', { type: 'warning' })
    for (const id of selectedIds.value) {
      await reverseTask(id, { reason: '批量反转' })
    }
    ElMessage.success('反转完成')
    await fetchLogs()
  } catch {}
}

function statusType(s) {
  return { Completed: 'success', Failed: 'danger', Running: 'primary', Stale: 'warning', Reversed: 'info' }[s] || 'info'
}
function statusLabel(s) {
  return { Completed: '成功', Failed: '失败', Running: '运行中', Stale: '僵死', Reversed: '已反转' }[s] || s
}
function triggerLabel(t) {
  return { Scheduled: '自动', Manual: '手动', Event: '事件' }[t] || t
}
function formatDate(d) {
  if (!d) return ''
  const dt = new Date(d)
  return `${dt.getFullYear()}-${String(dt.getMonth()+1).padStart(2,'0')}-${String(dt.getDate()).padStart(2,'0')} ${String(dt.getHours()).padStart(2,'0')}:${String(dt.getMinutes()).padStart(2,'0')}`
}
function formatDuration(ms) {
  if (!ms) return '-'
  return (ms / 1000).toFixed(3) + 's'
}

onMounted(fetchLogs)
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
</style>
