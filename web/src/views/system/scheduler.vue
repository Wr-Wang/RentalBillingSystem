<template>
  <div>
    <div class="page-header">
      <h2>调度任务管理</h2>
    </div>

    <!-- Job 卡片总览 -->
    <el-row :gutter="16" style="margin-bottom: 16px;">
      <el-col v-for="job in jobs" :key="job.name" :span="6">
        <el-card shadow="hover" :body-style="{ padding: '14px' }">
          <div style="display: flex; justify-content: space-between; align-items: center;">
            <div>
              <div style="font-weight: 600; font-size: 15px;">{{ job.displayName }}</div>
              <div style="font-size: 12px; color: #909399; margin-top: 4px;">{{ job.name }}</div>
              <div style="font-size: 13px; margin-top: 6px; color: #606266;">
                下次: {{ job.nextTime || '未设置' }}
              </div>
            </div>
            <el-button text type="primary" size="small" @click="selectJob(job)">配置 ▶</el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 选中 Job 的详情 -->
    <template v-if="selectedJob">
      <el-card style="margin-bottom: 16px;">
        <template #header>
          <div style="display: flex; justify-content: space-between; align-items: center;">
            <span>
              <el-button text @click="selectedJob = null" style="margin-right: 8px;">◀ 返回</el-button>
              <strong>{{ selectedJob.displayName }}</strong>
              <span style="font-size: 12px; color: #909399; margin-left: 8px;">（{{ selectedJob.name }}）</span>
            </span>
            <span>
              <el-tag :type="selectedJob.enabled ? 'success' : 'info'" size="small">
                {{ selectedJob.enabled ? '已启用' : '已停用' }}
              </el-tag>
              <el-tag type="info" size="small" style="margin-left: 8px;">
                上次: {{ selectedJob.lastRun || '无' }}
              </el-tag>
            </span>
          </div>
        </template>

        <!-- 未来 N 个月排期列表 -->
        <div style="margin-bottom: 12px; font-weight: 500;">执行排期</div>
        <el-table :data="schedules" stripe size="small" style="margin-bottom: 12px;">
          <el-table-column prop="month" label="月份" width="100" />
          <el-table-column label="计划执行时间" width="200">
            <template #default="{ row }">
              <span>{{ row.targetDate }}</span>
              <el-tag v-if="row.isAdjusted" type="warning" size="small" style="margin-left: 6px;">已调整</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="status" label="状态" width="90">
            <template #default="{ row }">
              <el-tag :type="row.status === 'Executed' ? 'success' : row.status === 'Skipped' ? 'info' : 'primary'" size="small">
                {{ row.status === 'Pending' ? '待执行' : row.status === 'Executed' ? '已执行' : '已跳过' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="reason" label="原因" min-width="180" />
          <el-table-column label="操作" width="70" fixed="right">
            <template #default="{ row, $index }">
              <el-button v-if="row.status === 'Pending'" text type="primary" size="small" @click="openAdjust(row, $index)">改</el-button>
              <span v-else style="color: #c0c4cc; font-size: 12px;">-</span>
            </template>
          </el-table-column>
        </el-table>

        <div style="display: flex; gap: 8px; margin-bottom: 16px;">
          <el-button size="small" @click="generateDefault">批量生成默认排期</el-button>
          <el-button size="small" type="primary" plain @click="openAdd">+ 添加自定义排期</el-button>
        </div>

        <!-- 执行历史 -->
        <div style="font-weight: 500; margin-bottom: 8px;">执行历史</div>
        <el-table :data="logs" stripe size="small">
          <el-table-column prop="fireTime" label="实际执行时间" width="170" />
          <el-table-column label="结果" width="70">
            <template #default="{ row }">
              <el-tag :type="row.status === 'Success' ? 'success' : row.status === 'Failed' ? 'danger' : 'warning'" size="small">
                {{ row.status === 'Success' ? '✅' : row.status === 'Failed' ? '❌' : '⚠️' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="processedCount" label="处理数" width="70" />
          <el-table-column prop="errorCount" label="错误" width="60" />
          <el-table-column prop="durationMs" label="耗时" width="80">
            <template #default="{ row }">{{ row.durationMs }}ms</template>
          </el-table-column>
          <el-table-column label="详情" width="60">
            <template #default="{ row }">
              <el-button text size="small" type="primary" @click="showLogDetail(row)">查</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </template>

    <!-- 首次进入时显示列表/日历 -->
    <el-card v-if="!selectedJob" style="margin-top: 0;">
      <el-tabs v-model="viewTab">
        <el-tab-pane label="列表视图" name="list">
          <el-table :data="sortedLogs" stripe size="small" default-sort="{ prop: 'fireTime', order: 'descending' }">
            <el-table-column prop="jobName" label="Job" width="180" sortable="custom" />
            <el-table-column prop="fireTime" label="执行时间" width="170" sortable="custom" />
            <el-table-column label="结果" width="70">
              <template #default="{ row }">
                <el-tag :type="row.status === 'Success' ? 'success' : row.status === 'Failed' ? 'danger' : 'warning'" size="small">
                  {{ row.status === 'Success' ? '✅' : row.status === 'Failed' ? '❌' : '⚠️' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="processedCount" label="处理数" width="70" />
            <el-table-column prop="durationMs" label="耗时" width="80">
              <template #default="{ row }">{{ row.durationMs }}ms</template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
        <el-tab-pane label="日历视图" name="calendar">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
            <el-button text @click="prevMonth">◀</el-button>
            <span style="font-weight: 600; font-size: 18px;">{{ calendarYear }}年{{ calendarMonth }}月</span>
            <el-button text @click="nextMonth">▶</el-button>
          </div>
          <table style="width: 100%; border-collapse: collapse; table-layout: fixed;">
            <thead>
              <tr>
                <th v-for="d in ['日','一','二','三','四','五','六']" :key="d"
                  style="padding: 8px; text-align: center; background: #f5f7fa; border: 1px solid #ebeef5; font-weight: 500;">{{ d }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(week, wi) in calendarWeeks" :key="wi">
                <td v-for="(cell, ci) in week" :key="ci"
                  style="padding: 4px; text-align: center; border: 1px solid #ebeef5; cursor: default; position: relative; height: 90px; vertical-align: top; width: 14.28%;"
                  :style="getCellStyle(cell)">
                  <div style="font-size: 14px; font-weight: cell?.isToday ? 700 : 400;">{{ cell?.day || '' }}</div>
                  <div v-if="cell?.jobs?.length" style="margin-top: 4px;">
                    <el-tag v-for="j in cell.jobs.slice(0, 2)" :key="j.jobName"
                      size="small" :type="j.type" :title="j.displayName"
                      style="display: block; margin-bottom: 3px; max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; cursor: pointer;"
                      @click="selectJobByName(j.jobName)"
                      @mouseenter="hoverCell = cell"
                      @mouseleave="hoverCell = null">
                      {{ j.shortName }}
                    </el-tag>
                    <div v-if="cell.jobs.length > 2" style="font-size: 11px; color: #909399;">+{{ cell.jobs.length - 2 }}个</div>
                  </div>

                  <!-- 悬停弹窗 -->
                  <div v-if="hoverCell?.day === cell?.day && cell?.jobs?.length"
                    style="position: absolute; bottom: 100%; left: 50%; transform: translateX(-50%); z-index: 1000;
                      background: #fff; border: 1px solid #dcdfe6; border-radius: 6px; padding: 10px 14px;
                      box-shadow: 0 4px 16px rgba(0,0,0,0.15); width: 260px; text-align: left; pointer-events: none;">
                    <div style="font-weight: 600; margin-bottom: 6px; font-size: 14px;">{{ cell.month }}月{{ cell.day }}日 执行任务</div>
                    <div v-for="j in cell.jobs" :key="j.jobName" style="padding: 6px 0; border-bottom: 1px solid #f0f0f0;">
                      <div style="display: flex; justify-content: space-between; align-items: center;">
                        <span style="font-weight: 500;">{{ j.displayName }}</span>
                        <el-tag :type="j.type" size="small">{{ j.time }}</el-tag>
                      </div>
                      <div style="font-size: 12px; color: #909399; margin-top: 2px;">{{ j.reason || '' }}</div>
                    </div>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!-- 调整执行日期 Drawer -->
    <el-drawer
      v-model="drawerVisible"
      title="调整执行日期"
      :size="450"
    >
      <template v-if="editingSchedule">
        <el-form label-width="100px">
          <el-form-item label="任务">
            <span>{{ selectedJob.displayName }}</span>
          </el-form-item>
          <el-form-item label="月份">
            <span>{{ editingSchedule.month }}</span>
          </el-form-item>
          <el-form-item label="原计划">
            <span style="color: #909399;">{{ editingSchedule.originalDate }}</span>
          </el-form-item>
          <el-form-item label="调整到">
            <div style="display: flex; gap: 8px; align-items: center;">
              <el-input-number v-model="editForm.day" :min="1" :max="31" size="small" style="width: 90px;" controls-position="right" />
              <span style="color: #909399;">日</span>
              <el-select v-model="editForm.hour" size="small" style="width: 90px;">
                <el-option v-for="h in 24" :key="h - 1" :label="`${String(h - 1).padStart(2, '0')}`" :value="h - 1" />
              </el-select>
              <span style="color: #909399;">:</span>
              <el-select v-model="editForm.minute" size="small" style="width: 90px;">
                <el-option v-for="m in 60" :key="m - 1" :label="`${String(m - 1).padStart(2, '0')}`" :value="m - 1" />
              </el-select>
            </div>
          </el-form-item>
          <el-form-item label="调整原因">
            <el-input v-model="editForm.reason" type="textarea" :rows="2" placeholder="如：25日逢周末，提前至24日" />
          </el-form-item>
          <el-form-item label="同时调整">
            <div style="display: flex; flex-wrap: wrap; gap: 6px;">
              <el-checkbox v-for="(s, i) in schedules" :key="i" v-model="s._batch" :label="s.month" :disabled="i === editingIndex" />
            </div>
            <div style="font-size: 12px; color: #909399; margin-top: 4px;">勾选后续月份同步应用此调整</div>
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="drawerVisible = false">取消</el-button>
        <el-button type="primary" @click="saveAdjust">保存</el-button>
      </template>
    </el-drawer>

    <!-- 日志详情 Dialog -->
    <el-dialog v-model="logDialogVisible" title="执行详情" :width="500">
      <template v-if="logDetail">
        <pre style="background: #f5f7fa; padding: 12px; border-radius: 4px; font-size: 13px; white-space: pre-wrap;">{{ JSON.stringify(logDetail, null, 2) }}</pre>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { ElMessage } from 'element-plus'

// ==================== Job 定义 ====================
const jobs = ref([
  {
    name: 'MonthlyFeeBillJob', displayName: '📅 月度应收生成', enabled: true,
    nextTime: '07-25 08:00', lastRun: '06-25 08:00 ✅',
    defaultDay: 25, defaultHour: 8, defaultMinute: 0, shortName: '月度应收'
  },
  {
    name: 'LateFeeCalcJob', displayName: '💰 滞纳金计算', enabled: true,
    nextTime: '每天 02:00', lastRun: '06-27 02:00 ✅',
    defaultDay: null, defaultHour: 2, defaultMinute: 0, shortName: '滞纳金'
  },
  {
    name: 'AutoRenewJob', displayName: '🔄 自动续签', enabled: true,
    nextTime: '每天 00:00', lastRun: '06-27 00:00 ✅',
    defaultDay: null, defaultHour: 0, defaultMinute: 0, shortName: '续签'
  },
  {
    name: 'CollectionJob', displayName: '📢 催缴任务', enabled: true,
    nextTime: '每天 09:00', lastRun: '06-27 09:00 ✅',
    defaultDay: null, defaultHour: 9, defaultMinute: 0, shortName: '催缴'
  }
])

// ==================== 视图切换 ====================
const viewTab = ref('list')

// ==================== 日历数据 ====================
const calendarYear = ref(new Date().getFullYear())
const calendarMonth = ref(new Date().getMonth() + 1)
const hoverCell = ref(null)      // 当前悬停的日期单元格

function prevMonth() {
  if (calendarMonth.value === 1) { calendarMonth.value = 12; calendarYear.value-- }
  else { calendarMonth.value-- }
}
function nextMonth() {
  if (calendarMonth.value === 12) { calendarMonth.value = 1; calendarYear.value++ }
  else { calendarMonth.value++ }
}

// 构建日历网格 + 每日任务
const calendarWeeks = computed(() => {
  const y = calendarYear.value, m = calendarMonth.value
  const firstDay = new Date(y, m - 1, 1).getDay()
  const daysInMonth = new Date(y, m, 0).getDate()
  const today = new Date()
  const weeks = []
  let week = new Array(7).fill(null)

  // 填充月初空白
  for (let d = 0; d < firstDay; d++) week[d] = null

  for (let day = 1; day <= daysInMonth; day++) {
    const idx = firstDay + day - 1
    const wi = Math.floor(idx / 7)
    const ci = idx % 7
    if (wi > 0 && ci === 0) week = new Array(7).fill(null)
    const cell = {
      day, month: m, year: y,
      isToday: y === today.getFullYear() && m === today.getMonth() + 1 && day === today.getDate(),
      jobs: getJobsForDate(y, m, day)
    }
    week[ci] = cell
    if (ci === 6 || day === daysInMonth) {
      if (week.some(c => c !== null)) weeks.push([...week])
      week = new Array(7).fill(null)
    }
  }
  return weeks
})

function getJobsForDate(year, month, day) {
  const results = []
  jobs.value.forEach(job => {
    if (job.defaultDay === null) {
      // 每天执行的 Job
      results.push({
        jobName: job.name, displayName: job.displayName, shortName: job.shortName,
        time: `${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`,
        type: 'primary', reason: '每日执行'
      })
    } else if (job.defaultDay === day) {
      // 月度 Job：匹配默认执行日
      results.push({
        jobName: job.name, displayName: job.displayName, shortName: job.shortName,
        time: `${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`,
        type: 'success', reason: '计划执行日'
      })
    }
  })
  // 检查是否有调整过的排期（schedules 数据）
  const label = `${year}-${String(month).padStart(2, '0')}`
  const sched = allSchedules.value.find(s => s.month === label && s.day === day)
  if (sched) {
    results.push({
      jobName: sched.jobName, displayName: `⚡ ${jobs.value.find(j => j.name === sched.jobName)?.displayName || sched.jobName}`,
      shortName: `⚡${jobs.value.find(j => j.name === sched.jobName)?.shortName || sched.jobName}`,
      time: sched.time, type: 'warning', reason: sched.reason
    })
  }
  return results
}

function getCellStyle(cell) {
  if (!cell) return {}
  const style = {}
  if (cell.isToday) style.background = '#ecf5ff'
  if (cell.jobs?.length) style.background = style.background || '#f0f9eb'
  return style
}

function selectJobByName(name) {
  const job = jobs.value.find(j => j.name === name)
  if (job) selectJob(job)
}

// 所有排期的全局集合（用于日历）
const allSchedules = ref([])
function refreshAllSchedules() {
  const all = []
  jobs.value.forEach(job => {
    if (job.defaultDay !== null) {
      for (let i = 0; i < 12; i++) {
        const m = new Date().getMonth() + 1 + i
        const y = new Date().getFullYear() + Math.floor((new Date().getMonth() + i) / 12)
        const month = (m % 12 || 12)
        const day = job.defaultDay
        all.push({
          jobName: job.name, month: `${y}-${String(month).padStart(2, '0')}`,
          day, time: `${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`,
          reason: '默认'
        })
      }
    }
  })
  allSchedules.value = all
}
refreshAllSchedules()

// ==================== 选中 Job & 排期 ====================
const selectedJob = ref(null)
const schedules = ref([])
const logs = ref([
  { jobName: 'MonthlyFeeBillJob', fireTime: '2026-06-25 08:00:30', status: 'Success', processedCount: 156, errorCount: 0, durationMs: 3200 },
  { jobName: 'MonthlyFeeBillJob', fireTime: '2026-05-25 08:01:15', status: 'Success', processedCount: 148, errorCount: 2, durationMs: 4100 },
  { jobName: 'LateFeeCalcJob', fireTime: '2026-06-27 02:00:10', status: 'Success', processedCount: 25, errorCount: 0, durationMs: 850 },
  { jobName: 'AutoRenewJob', fireTime: '2026-06-27 00:00:05', status: 'Success', processedCount: 3, errorCount: 0, durationMs: 230 },
  { jobName: 'CollectionJob', fireTime: '2026-06-27 09:00:20', status: 'Success', processedCount: 12, errorCount: 0, durationMs: 1200 },
  { jobName: 'MonthlyFeeBillJob', fireTime: '2026-04-25 08:00:05', status: 'Skipped', processedCount: 0, errorCount: 0, durationMs: 500 }
])

function selectJob(job) {
  selectedJob.value = job
  loadSchedules(job)
  loadLogs(job)
}

function loadSchedules(job) {
  const now = new Date()
  const list = []
  for (let i = 1; i <= 6; i++) {
    const m = now.getMonth() + i
    const y = now.getFullYear() + Math.floor((now.getMonth() + i - 1) / 12)
    const month = (m % 12 || 12)
    const year = y
    const label = `${year}-${String(month).padStart(2, '0')}`

    if (job.defaultDay !== null) {
      const day = job.defaultDay
      const d = new Date(year, month - 1, day)
      const weekday = ['日', '一', '二', '三', '四', '五', '六'][d.getDay()]
      const adjusted = (i === 1)
      list.push({
        jobName: job.name, month: label,
        targetDate: adjusted
          ? `${label}-${String(day - 1).padStart(2, '0')}(${weekday === '六' ? '五' : weekday}) ${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`
          : `${label}-${String(day).padStart(2, '0')}(${weekday}) ${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`,
        status: 'Pending', reason: adjusted ? '25日逢周六提前' : '默认',
        isAdjusted: adjusted,
        originalDate: `${label}-${String(day).padStart(2, '0')} ${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`
      })
    } else {
      list.push({
        jobName: job.name, month: label,
        targetDate: `每日 ${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`,
        status: 'Pending', reason: '每日执行', isAdjusted: false,
        originalDate: `每日 ${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`
      })
    }
  }
  schedules.value = list
}

function loadLogs(job) {
  logs.value = [
    { jobName: job.name, fireTime: '2026-06-25 08:00:30', status: 'Success', processedCount: 156, errorCount: 0, durationMs: 3200 },
    { jobName: job.name, fireTime: '2026-05-25 08:01:15', status: 'Success', processedCount: 148, errorCount: 2, durationMs: 4100 },
    { jobName: job.name, fireTime: '2026-04-25 08:00:05', status: 'Skipped', processedCount: 0, errorCount: 0, durationMs: 500 }
  ]
}

// ==================== 调整 Drawer ====================
const drawerVisible = ref(false)
const editingSchedule = ref(null)
const editingIndex = ref(-1)
const editForm = reactive({ day: 1, hour: 8, minute: 0, reason: '' })

function openAdjust(row, index) {
  editingSchedule.value = row
  editingIndex.value = index
  // 从 targetDate 解析当前值
  const match = row.targetDate.match(/(\d{4}-\d{2})-(\d{2})/)
  if (match) {
    editForm.day = parseInt(match[2])
  } else {
    editForm.day = selectedJob.value.defaultDay || 1
  }
  editForm.hour = selectedJob.value.defaultHour
  editForm.minute = selectedJob.value.defaultMinute
  editForm.reason = row.reason !== '默认' ? row.reason : ''
  // 重置批量勾选
  schedules.value.forEach(s => (s._batch = false))
  drawerVisible.value = true
}

function openAdd() {
  editingSchedule.value = null
  editingIndex.value = -1
  editForm.day = selectedJob.value.defaultDay || 1
  editForm.hour = selectedJob.value.defaultHour
  editForm.minute = selectedJob.value.defaultMinute
  editForm.reason = ''
  schedules.value.forEach(s => (s._batch = false))
  drawerVisible.value = true
}

function saveAdjust() {
  if (!editForm.reason) {
    ElMessage.warning('请填写调整原因')
    return
  }
  // 更新当前行
  if (editingIndex.value >= 0) {
    const row = schedules.value[editingIndex.value]
    const monthLabel = row.month
    const d = new Date(parseInt(monthLabel.split('-')[0]), parseInt(monthLabel.split('-')[1]) - 1, editForm.day)
    const weekday = ['日', '一', '二', '三', '四', '五', '六'][d.getDay()]
    row.targetDate = `${monthLabel}-${String(editForm.day).padStart(2, '0')}(${weekday}) ${String(editForm.hour).padStart(2, '0')}:${String(editForm.minute).padStart(2, '0')}`
    row.isAdjusted = true
    row.reason = editForm.reason
  }
  // 批量更新后续勾选的行
  schedules.value.forEach((s, i) => {
    if (s._batch && i > editingIndex.value) {
      const monthLabel = s.month
      const d = new Date(parseInt(monthLabel.split('-')[0]), parseInt(monthLabel.split('-')[1]) - 1, editForm.day)
      const weekday = ['日', '一', '二', '三', '四', '五', '六'][d.getDay()]
      s.targetDate = `${monthLabel}-${String(editForm.day).padStart(2, '0')}(${weekday}) ${String(editForm.hour).padStart(2, '0')}:${String(editForm.minute).padStart(2, '0')}`
      s.isAdjusted = true
      s.reason = editForm.reason
      s._batch = false
    }
  })
  drawerVisible.value = false
  ElMessage.success('排期已更新')
}

function generateDefault() {
  if (!selectedJob.value) return
  loadSchedules(selectedJob.value)
  ElMessage.success('已重新生成默认排期')
}

// ==================== 日志详情 ====================
const logDialogVisible = ref(false)
const logDetail = ref(null)
// 列表视图：按执行时间倒序
const sortedLogs = computed(() => {
  return [...logs.value].sort((a, b) => b.fireTime.localeCompare(a.fireTime))
})

function showLogDetail(row) {
  logDetail.value = row
  logDialogVisible.value = true
}
</script>
