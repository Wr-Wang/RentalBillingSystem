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
              <el-tag :color="statusColor(row.status)" style="color:#fff;border:0" size="small">
                {{ row.status === 'Pending' ? '待执行' : row.status === 'Running' ? '执行中' : row.status === 'Success' ? '执行成功' : row.status === 'Failed' ? '执行失败' : row.status === 'Skipped' ? '已跳过' : row.status === 'Paused' ? '已暂停' : row.status === 'Cancelled' ? '已取消' : row.status === 'Retrying' ? '重试中' : row.status === 'TimedOut' ? '已超时' : '-' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="reason" label="原因" min-width="180" />
          <el-table-column label="操作" width="200" fixed="right">
            <template #default="{ row }">
              <el-button text type="primary" size="small" @click="showScheduleDetail(row)">查看</el-button>
              <el-button text type="primary" size="small" @click="openEditSchedule(row)">编辑</el-button>
              <el-button text type="danger" size="small" @click="removeSchedule(row._id)">删除</el-button>
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
              <el-button text size="small" type="primary" @click="showLogDetail(row)">查看</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </template>

    <!-- 首次进入时显示列表/日历 -->
    <el-card v-if="!selectedJob" style="margin-top: 0;">
      <el-tabs v-model="viewTab">
        <el-tab-pane label="列表视图" name="list">
          <el-table :data="sortedLogs" stripe size="small" :default-sort="{ prop: 'fireTime', order: 'descending' }">
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
      :title="isAddingNew ? '添加自定义排期' : '调整执行日期'"
      :size="450"
    >
      <!-- 添加自定义排期 -->
      <template v-if="isAddingNew">
        <el-form label-width="110px">
          <el-form-item label="任务"><span>{{ selectedJob?.displayName }}</span></el-form-item>
          <el-form-item label="计划执行时间">
            <el-date-picker
              v-model="editForm.scheduledAt"
              type="datetime"
              value-format="YYYY-MM-DD HH:mm"
              format="YYYY-MM-DD HH:mm"
              placeholder="选择执行日期和时间"
              style="width:100%"
            />
          </el-form-item>
          <el-form-item label="状态">
            <el-select v-model="editForm.status" style="width:100%">
              <el-option label="待执行" value="Pending" />
              <el-option label="执行中" value="Running" />
              <el-option label="执行成功" value="Success" />
              <el-option label="执行失败" value="Failed" />
              <el-option label="已跳过" value="Skipped" />
              <el-option label="已暂停" value="Paused" />
              <el-option label="已取消" value="Cancelled" />
              <el-option label="重试中" value="Retrying" />
              <el-option label="已超时" value="TimedOut" />
            </el-select>
          </el-form-item>
          <el-form-item label="原因说明">
            <el-input v-model="editForm.reason" type="textarea" :rows="2" placeholder="说明添加此排期的原因" />
          </el-form-item>
        </el-form>
      </template>
      <!-- 编辑排期 -->
      <template v-else-if="editingId">
        <el-form label-width="110px">
          <el-form-item label="任务"><span>{{ selectedJob?.displayName }}</span></el-form-item>
          <el-form-item label="原计划"><span style="color:#909399;">{{ currentSchedule?.originalDate }}</span></el-form-item>
          <el-form-item label="调整时间">
            <el-date-picker
              v-model="editForm.scheduledAt"
              type="datetime"
              value-format="YYYY-MM-DD HH:mm"
              format="YYYY-MM-DD HH:mm"
              placeholder="选择调整后的执行时间"
              style="width:100%"
            />
          </el-form-item>
          <el-form-item label="状态">
            <el-select v-model="editForm.status" style="width:100%">
              <el-option label="待执行" value="Pending" /><el-option label="执行中" value="Running" />
              <el-option label="执行成功" value="Success" /><el-option label="执行失败" value="Failed" />
              <el-option label="已跳过" value="Skipped" /><el-option label="已暂停" value="Paused" />
              <el-option label="已取消" value="Cancelled" /><el-option label="重试中" value="Retrying" />
              <el-option label="已超时" value="TimedOut" />
            </el-select>
          </el-form-item>
          <el-form-item label="调整原因">
            <el-input v-model="editForm.reason" type="textarea" :rows="2" placeholder="如：25日逢周末，提前至24日" />
          </el-form-item>
          <el-form-item label="同时调整">
            <div style="display:flex;flex-wrap:wrap;gap:6px;">
              <el-checkbox v-for="s in schedules" :key="s._id" v-model="s._batch" :label="s.month" :disabled="s._id === editingId" />
            </div>
            <div style="font-size:12px;color:#909399;margin-top:4px;">勾选后续月份同步应用此调整</div>
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

    <!-- 排期详情 Dialog -->
    <el-dialog v-model="scheduleDetailVisible" title="排期详情" :width="500">
      <template v-if="scheduleDetail">
        <el-descriptions :column="1" border>
          <el-descriptions-item label="任务名">{{ scheduleDetail.jobName }}</el-descriptions-item>
          <el-descriptions-item label="月份">{{ scheduleDetail.month }}</el-descriptions-item>
          <el-descriptions-item label="计划时间">{{ scheduleDetail.targetDate }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :color="statusColor(scheduleDetail.status)" style="color:#fff;border:0" size="small">
              {{ scheduleDetail.status === 'Success' ? '执行成功' : scheduleDetail.status === 'Failed' ? '执行失败' : scheduleDetail.status === 'Pending' ? '待执行' : scheduleDetail.status === 'Running' ? '执行中' : scheduleDetail.status === 'Custom' ? '自定义' : scheduleDetail.status === 'Skipped' ? '已跳过' : scheduleDetail.status === 'Paused' ? '已暂停' : scheduleDetail.status || '-' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="原因">{{ scheduleDetail.reason || '-' }}</el-descriptions-item>
          <el-descriptions-item label="调整">{{ scheduleDetail.isAdjusted ? '是' : '否' }}</el-descriptions-item>
        </el-descriptions>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getSchedulerJobs, updateSchedulerJob } from '../../api/index'

// ==================== Job 定义 ====================
const jobs = ref([])
const loading = ref(false)

// 解析 CronExpression → { defaultDay, defaultHour, defaultMinute, type }
function parseCron(cron) {
  const parts = (cron || '').split(' ')
  // 标准 Cron: sec min hour day-of-month month day-of-week
  // 0 0 8 25 * ?  → 每月25日08:00
  // 0 0 2 * * ?   → 每天02:00
  const minute = parts.length > 1 ? parseInt(parts[1]) : 0
  const hour = parts.length > 2 ? parseInt(parts[2]) : 0
  const day = parts.length > 3 ? parseInt(parts[3]) : null
  return {
    defaultDay: day && !isNaN(day) && day > 0 ? day : null,
    defaultHour: hour || 0,
    defaultMinute: minute || 0,
    isDaily: !day || isNaN(day) || day <= 0  // 无指定日 → 每天执行
  }
}

// 从 Description 提取显示名
const JOB_NAMES = {
  MonthlyFeeBillJob: { displayName: '📅 月度应收生成', shortName: '月度应收' },
  LateFeeCalcJob: { displayName: '💰 滞纳金计算', shortName: '滞纳金' },
  AutoRenewJob: { displayName: '🔄 自动续签', shortName: '续签' },
  CollectionJob: { displayName: '📢 催缴任务', shortName: '催缴' }
}

async function fetchJobs() {
  loading.value = true
  try {
    const res = await getSchedulerJobs()
    const raw = Array.isArray(res) ? res : []
    jobs.value = raw.map(j => {
      const meta = JOB_NAMES[j.jobName] || { displayName: j.jobName, shortName: j.jobName }
      const cron = parseCron(j.cronExpression)
      const nextTime = cron.isDaily
        ? `每天 ${String(cron.defaultHour).padStart(2, '0')}:${String(cron.defaultMinute).padStart(2, '0')}`
        : `每月${cron.defaultDay}日 ${String(cron.defaultHour).padStart(2, '0')}:${String(cron.defaultMinute).padStart(2, '0')}`
      // 解析 Description 中的排期状态映射（兼容新旧格式）
      let scheduleStates = {}
      let deletedMonths = []
      let customSchedules = []
      let globalUpdatedAt = null
      try {
        if (j.description) {
          const parsed = JSON.parse(j.description)
          if (parsed.scheduleStates) {
            // 新格式: { scheduleStates: {...}, deletedMonths: [...], customSchedules: [...], updatedAt: "..." }
            scheduleStates = parsed.scheduleStates
            deletedMonths = Array.isArray(parsed.deletedMonths) ? parsed.deletedMonths : []
            customSchedules = Array.isArray(parsed.customSchedules) ? parsed.customSchedules : []
            globalUpdatedAt = parsed.updatedAt
          } else if (parsed.month) {
            // 旧格式: { status, reason, month, updatedAt } → 转为映射
            scheduleStates[parsed.month] = { status: parsed.status, reason: parsed.reason }
            globalUpdatedAt = parsed.updatedAt
          }
        }
      } catch (e) {}
      return {
        id: j.id,
        name: j.jobName,
        displayName: meta.displayName,
        shortName: meta.shortName,
        enabled: j.isActive,
        cronExpression: j.cronExpression,
        description: j.description || '',
        scheduleStates,
        deletedMonths,
        customSchedules,
        nextTime,
        lastRun: globalUpdatedAt ? new Date(globalUpdatedAt).toLocaleString() + ' (已编辑)' : '-',
        defaultDay: cron.defaultDay,
        defaultHour: cron.defaultHour,
        defaultMinute: cron.defaultMinute
      }
    })
  } catch (e) {
    jobs.value = []
  }
  loading.value = false
}

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
  // 过滤已删除的月份
  const filteredMonths = new Set(job.deletedMonths || [])
  const list = []
  for (let i = 1; i <= 6; i++) {
    const m = now.getMonth() + i
    const y = now.getFullYear() + Math.floor((now.getMonth() + i - 1) / 12)
    const month = (m % 12 || 12)
    const year = y
    const label = `${year}-${String(month).padStart(2, '0')}`
    if (filteredMonths.has(label)) continue  // 跳过已删除的月份

    if (job.defaultDay !== null) {
      const day = job.defaultDay
      const d = new Date(year, month - 1, day)
      const weekday = ['日', '一', '二', '三', '四', '五', '六'][d.getDay()]
      const adjusted = (i === 1)
      list.push({
        _id: crypto.randomUUID(),
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
        _id: crypto.randomUUID(),
        jobName: job.name, month: label,
        targetDate: `每日 ${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`,
        status: 'Pending', reason: '每日执行', isAdjusted: false,
        originalDate: `每日 ${String(job.defaultHour).padStart(2, '0')}:${String(job.defaultMinute).padStart(2, '0')}`
      })
    }
  }
  // 从 Description 恢复各月份排期的状态（scheduleStates 映射）
  if (job.scheduleStates && list.length > 0) {
    list.forEach(s => {
      const state = job.scheduleStates[s.month]
      if (state) {
        if (state.status) s.status = state.status
        if (state.reason) s.reason = state.reason
        if (state.isAdjusted) s.isAdjusted = true
      }
    })
  }
  // 追加自定义排期（用户手动添加的执行日期）
  const customs = job.customSchedules || []
  customs.forEach(cs => {
    // 跳过已被 deletedMonths 过滤掉的月份对应的自定义排期
    // （deletedMonths 过滤在 loadSchedules 开头通过 continue 跳过循环，不影响这里）
    list.push({
      _id: crypto.randomUUID(),
      jobName: job.name,
      month: cs.month,
      targetDate: cs.targetDate,
      status: cs.status || 'Pending',
      reason: cs.reason || '自定义',
      isAdjusted: true,
      originalDate: cs.originalDate || cs.targetDate
    })
  })
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
const editingId = ref(null)  // 正在编辑的排期 _id，null 表示新增模式
const editForm = reactive({ scheduledAt: '', status: 'Pending', reason: '' })
const isAddingNew = computed(() => editingId.value === null)
const currentSchedule = computed(() => {
  if (editingSchedule.value) return editingSchedule.value
  if (editingId.value) return schedules.value.find(s => s._id === editingId.value) || null
  return null
})

function statusColor(status) {
  const colors = { Success: '#67c23a', Failed: '#f56c6c', TimedOut: '#e6a23c', Running: '#409eff', Retrying: '#9c27b0', Paused: '#009688', Pending: '#607d8b', Skipped: '#9e9e9e', Cancelled: '#795548' }
  return colors[status] || '#909399'
}

function openEditSchedule(row) {
  editingSchedule.value = row
  editingId.value = row._id
  // 从 targetDate 解析日期时间
  const match = row.targetDate.match(/(\d{4}-\d{2}-\d{2})\(.*?\)\s*(\d{2}:\d{2})/)
  if (match) {
    editForm.scheduledAt = `${match[1]} ${match[2]}`
  } else {
    editForm.scheduledAt = ''
  }
  editForm.status = row.status || 'Pending'
  editForm.reason = row.reason !== '默认' ? row.reason : ''
  schedules.value.forEach(s => (s._batch = false))
  drawerVisible.value = true
}

async function removeSchedule(id) {
  const idx = schedules.value.findIndex(s => s._id === id)
  if (idx < 0) return
  const removedMonth = schedules.value[idx].month
  schedules.value.splice(idx, 1)
  // 持久化删除操作到后端
  if (selectedJob.value?.id) {
    try {
      const newStates = { ...(selectedJob.value.scheduleStates || {}) }
      delete newStates[removedMonth]  // 同时清理该月的状态
      const newDeleted = [...(selectedJob.value.deletedMonths || [])]
      if (!newDeleted.includes(removedMonth)) newDeleted.push(removedMonth)
      await updateSchedulerJob(selectedJob.value.id, {
        description: JSON.stringify({
          scheduleStates: newStates,
          deletedMonths: newDeleted,
          updatedAt: new Date().toISOString()
        })
      })
      selectedJob.value.scheduleStates = newStates
      selectedJob.value.deletedMonths = newDeleted
    } catch (e) { /* 静默失败 */ }
  }
  ElMessage.success('排期已删除')
}

function openAdd() {
  editingSchedule.value = null
  editingId.value = null
  editForm.scheduledAt = ''
  editForm.status = 'Pending'
  editForm.reason = ''
  drawerVisible.value = true
}

async function saveAdjust() {
  if (!editForm.reason) {
    ElMessage.warning('请填写调整原因')
    return
  }
  // 新增自定义排期
  if (!editingId.value) {
    if (!editForm.scheduledAt) {
      ElMessage.warning('请选择计划执行时间')
      return
    }
    const parts = editForm.scheduledAt.split(' ')
    const dateParts = parts[0].split('-')
    const timeParts = parts[1].split(':')
    const year = parseInt(dateParts[0]), month = parseInt(dateParts[1])
    const day = parseInt(dateParts[2]), hour = parseInt(timeParts[0]), minute = parseInt(timeParts[1])
    const label = `${year}-${String(month).padStart(2, '0')}`
    const d = new Date(year, month - 1, day)
    const weekday = ['日', '一', '二', '三', '四', '五', '六'][d.getDay()]
    const targetDate = `${label}-${String(day).padStart(2, '0')}(${weekday}) ${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
    schedules.value.push({
      _id: crypto.randomUUID(),
      jobName: selectedJob.value.name, month: label,
      targetDate, status: editForm.status,
      reason: editForm.reason, isAdjusted: true, originalDate: targetDate
    })
    // 持久化自定义排期到后端
    if (selectedJob.value?.id) {
      try {
        const newCustom = [...(selectedJob.value.customSchedules || [])]
        newCustom.push({ month: label, targetDate, originalDate: targetDate, status: editForm.status, reason: editForm.reason })
        // 该月的默认 cron 排期不再显示（仅显示自定义排期）
        const newDeleted = [...(selectedJob.value.deletedMonths || [])]
        if (!newDeleted.includes(label)) newDeleted.push(label)
        await updateSchedulerJob(selectedJob.value.id, {
          description: JSON.stringify({
            scheduleStates: selectedJob.value.scheduleStates || {},
            deletedMonths: newDeleted,
            customSchedules: newCustom,
            updatedAt: new Date().toISOString()
          })
        })
        selectedJob.value.deletedMonths = newDeleted
        selectedJob.value.customSchedules = newCustom
      } catch (e) { /* 静默失败 */ }
    }
    drawerVisible.value = false
    ElMessage.success('自定义排期已添加')
    return
  }
  // 根据 _id 找到当前编辑的行
  const rowIdx = schedules.value.findIndex(s => s._id === editingId.value)
  if (rowIdx < 0) {
    ElMessage.error('找不到要编辑的排期')
    return
  }
  const row = schedules.value[rowIdx]
  // 更新当前行
  if (editForm.scheduledAt) {
    const parts = editForm.scheduledAt.split(' ')
    const dateParts = parts[0].split('-')
    const timeParts = parts[1].split(':')
    const year = parseInt(dateParts[0]), month = parseInt(dateParts[1])
    const day = parseInt(dateParts[2]), hour = parseInt(timeParts[0]), minute = parseInt(timeParts[1])
    const monthLabel = `${year}-${String(month).padStart(2, '0')}`
    const d = new Date(year, month - 1, day)
    const weekday = ['日', '一', '二', '三', '四', '五', '六'][d.getDay()]
    row.targetDate = `${monthLabel}-${String(day).padStart(2, '0')}(${weekday}) ${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
    row.month = monthLabel
  }
  row.status = editForm.status
  row.isAdjusted = true
  row.reason = editForm.reason
  // 批量更新后续勾选的行（用 rowIdx 判断位置）
  schedules.value.forEach((s, i) => {
    if (s._batch && i > rowIdx) {
      if (editForm.scheduledAt) {
        const parts = editForm.scheduledAt.split(' ')
        const dateParts = parts[0].split('-')
        const timeParts = parts[1].split(':')
        const day = parseInt(dateParts[2]), hour = parseInt(timeParts[0]), minute = parseInt(timeParts[1])
        const monthLabel = s.month
        const monthNum = parseInt(monthLabel.split('-')[1])
        const d = new Date(parseInt(monthLabel.split('-')[0]), monthNum - 1, day)
        const weekday = ['日', '一', '二', '三', '四', '五', '六'][d.getDay()]
        s.targetDate = `${monthLabel}-${String(day).padStart(2, '0')}(${weekday}) ${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
      }
      s.isAdjusted = true
      s.reason = editForm.reason
      s._batch = false
    }
  })
  drawerVisible.value = false
  // 持久化状态到后端（合并到 scheduleStates 映射中）
  if (selectedJob.value?.id && editForm.status) {
    const monthKey = row.month || ''
    try {
      const newStates = { ...(selectedJob.value.scheduleStates || {}) }
      if (monthKey) {
        newStates[monthKey] = { status: editForm.status, reason: editForm.reason, isAdjusted: true }
      }
      // 编辑的是 cron 排期（非自定义）时，从 deletedMonths 中恢复
      const isCustom = selectedJob.value.customSchedules?.some(cs => cs.month === monthKey)
      const newDeleted = [...(selectedJob.value.deletedMonths || [])]
      if (!isCustom) {
        const delIdx = monthKey ? newDeleted.indexOf(monthKey) : -1
        if (delIdx >= 0) newDeleted.splice(delIdx, 1)
      }
      await updateSchedulerJob(selectedJob.value.id, {
        description: JSON.stringify({
          scheduleStates: newStates,
          deletedMonths: newDeleted,
          updatedAt: new Date().toISOString()
        })
      })
      // 同步更新本地的 scheduleStates 和 deletedMonths
      selectedJob.value.scheduleStates = newStates
      selectedJob.value.deletedMonths = newDeleted
    } catch (e) { /* 静默失败 */ }
  }
  ElMessage.success('排期已更新')
}

async function generateDefault() {
  if (!selectedJob.value) return
  // 清空所有自定义状态（编辑 + 删除 + 自定义排期）
  selectedJob.value.scheduleStates = {}
  selectedJob.value.deletedMonths = []
  selectedJob.value.customSchedules = []
  if (selectedJob.value.id) {
    try {
      await updateSchedulerJob(selectedJob.value.id, {
        description: JSON.stringify({
          scheduleStates: {},
          deletedMonths: [],
          customSchedules: [],
          updatedAt: new Date().toISOString()
        })
      })
    } catch (e) { /* 静默失败 */ }
  }
  loadSchedules(selectedJob.value)
  ElMessage.success('已重置为默认排期')
}

// ==================== 排期详情 ====================
const scheduleDetailVisible = ref(false)
const scheduleDetail = ref(null)

function showScheduleDetail(row) {
  scheduleDetail.value = row
  scheduleDetailVisible.value = true
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

// 保存 Job 启用/停用
async function toggleJobEnabled(job) {
  try {
    await updateSchedulerJob(job.id, { isActive: !job.enabled })
    job.enabled = !job.enabled
    ElMessage.success(job.enabled ? '已启用' : '已停用')
  } catch (e) {
    ElMessage.error('操作失败')
  }
}

onMounted(() => fetchJobs())
</script>
