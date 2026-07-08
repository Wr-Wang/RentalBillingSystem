<template>
  <div>
    <div class="page-header">
      <h2>调度执行监控</h2>
      <div style="display:flex;gap:8px;">
        <el-button size="small" :loading="loading" @click="fetchAll">🔄 刷新</el-button>
        <el-button size="small" type="primary" plain @click="$router.push('/system/scheduler/monitor/logs')">📋 查看执行日志</el-button>
        <el-button size="small" plain @click="$router.push('/system/scheduler')">⚙️ 管理任务</el-button>
      </div>
    </div>

    <!-- 统计卡片 -->
    <el-row :gutter="16" style="margin-bottom:16px;">
      <el-col :span="6" v-for="card in statCards" :key="card.label">
        <el-card shadow="hover" :body-style="{ padding: '16px' }">
          <div class="stat-card">
            <div class="stat-label">{{ card.label }}</div>
            <div class="stat-value" :style="{ color: card.color }">
              {{ card.value }}
              <span v-if="card.suffix" class="stat-suffix">{{ card.suffix }}</span>
            </div>
            <div v-if="card.sub" class="stat-sub" :style="{ color: card.subColor }">{{ card.sub }}</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-bottom:16px;">
      <!-- 成功率趋势 -->
      <el-col :span="12">
        <el-card shadow="hover">
          <template #header><span>📈 近30天成功率趋势</span></template>
          <div ref="trendChartRef" style="width:100%;height:280px;"></div>
          <div v-if="trendLoading" class="chart-placeholder">加载中...</div>
        </el-card>
      </el-col>
      <!-- 各任务耗时 -->
      <el-col :span="12">
        <el-card shadow="hover">
          <template #header><span>⏱ 各任务平均耗时</span></template>
          <div ref="durationChartRef" style="width:100%;height:280px;"></div>
          <div v-if="durationLoading" class="chart-placeholder">加载中...</div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16">
      <!-- 今日异常任务 -->
      <el-col :span="12">
        <el-card shadow="hover">
          <template #header>
            <span>⚠️ 今日异常任务</span>
            <el-button text type="primary" size="small" style="float:right;" @click="$router.push('/system/scheduler/monitor/logs')">查看更多 →</el-button>
          </template>
          <el-table :data="errorLogs" stripe size="small" v-loading="logsLoading" empty-text="今日暂无异常" @row-click="openStepDrawer" style="cursor:pointer;" max-height="320">
            <el-table-column label="任务" min-width="130">
              <template #default="{row}">{{ row.taskName }}</template>
            </el-table-column>
            <el-table-column label="时间" width="140">
              <template #default="{row}">{{ formatDate(row.startedAt) }}</template>
            </el-table-column>
            <el-table-column label="状态" width="70">
              <template #default="{row}">
                <el-tag :type="row.status === 'Failed' ? 'danger' : 'warning'" size="small" effect="dark" round>
                  {{ row.status === 'Failed' ? '失败' : '僵死' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="耗时" width="60">
              <template #default="{row}">{{ formatDuration(row.totalDurationMs) }}</template>
            </el-table-column>
            <el-table-column label="错误信息" min-width="160">
              <template #default="{row}">{{ row.errorMessage || row.summary || '-' }}</template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
      <!-- 失败原因聚合 -->
      <el-col :span="12">
        <el-card shadow="hover" style="height:100%;">
          <template #header><span>📊 失败原因 Top-N（近30天）</span></template>
          <div v-if="failures.length > 0" class="failure-list">
            <div v-for="(f, i) in failures.slice(0, 5)" :key="i" class="failure-item">
              <div class="failure-rank">#{{ i + 1 }}</div>
              <div class="failure-info">
                <div class="failure-category">{{ f.errorCategory }}</div>
                <el-progress
                  :percentage="f.percentage"
                  :color="failureColor(f.percentage)"
                  :stroke-width="14"
                  :text-inside="true"
                >
                  <span style="font-size:11px;">{{ f.count }}次 ({{ f.percentage }}%)</span>
                </el-progress>
              </div>
              <div class="failure-trend" :class="{ up: f.trend === '↑', down: f.trend === '↓' }">
                {{ f.trend }}
              </div>
            </div>
          </div>
          <el-empty v-else description="近30天无失败记录" />
        </el-card>
      </el-col>
    </el-row>

    <!-- 步骤详情 Drawer -->
    <el-drawer v-model="stepDrawerVisible" title="步骤详情" :size="700" destroy-on-close>
      <StepWaterfall :log-detail="currentDetail" :steps="currentSteps" />
    </el-drawer>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'
import {
  getMonitorDashboard, getMonitorTrend, getMonitorDuration,
  getMonitorFailures, queryMonitorLogs, getMonitorLogDetail
} from '../../../../api/index'
import StepWaterfall from './components/StepWaterfall.vue'

const loading = ref(false)
const trendLoading = ref(false)
const durationLoading = ref(false)
const logsLoading = ref(false)
const trendChartRef = ref(null)
const durationChartRef = ref(null)

// Dashboard 统计
const stats = reactive({
  todayTotal: 0, todaySuccess: 0, todayFailed: 0,
  runningCount: 0, yesterdayTotal: 0, successRate: 100, diffYesterday: 0
})

const statCards = computed(() => [
  { label: '今日执行', value: stats.todayTotal, suffix: '次', color: '#409eff', sub: stats.diffYesterday >= 0 ? `较昨 +${stats.diffYesterday} ▲` : `较昨 ${stats.diffYesterday} ▼`, subColor: stats.diffYesterday >= 0 ? '#67c23a' : '#f56c6c' },
  { label: '执行成功', value: stats.todaySuccess, suffix: `次 (${stats.successRate}%)`, color: '#67c23a', sub: '' },
  { label: '执行失败', value: stats.todayFailed, suffix: `次 (${stats.todayTotal > 0 ? ((stats.todayFailed / stats.todayTotal) * 100).toFixed(1) : 0}%)`, color: '#f56c6c', sub: '' },
  { label: '运行中', value: stats.runningCount, suffix: '个', color: stats.runningCount > 0 ? '#409eff' : '#909399', sub: '' },
])

// 趋势图
let trendChart = null
let durationChart = null

// 异常日志
const errorLogs = ref([])
const failures = ref([])

function failureColor(pct) {
  if (pct >= 30) return '#f56c6c'
  if (pct >= 10) return '#e6a23c'
  return '#67c23a'
}

async function fetchDashboard() {
  try {
    const data = await getMonitorDashboard()
    Object.assign(stats, data)
  } catch {}
}

async function fetchTrend() {
  trendLoading.value = true
  try {
    const data = await getMonitorTrend(30)
    await nextTick()
    if (trendChartRef.value) {
      trendChart = echarts.init(trendChartRef.value)
      trendChart.setOption({
        tooltip: { trigger: 'axis' },
        grid: { left: 40, right: 20, top: 20, bottom: 30 },
        xAxis: { type: 'category', data: data.map(d => d.date?.slice(5) || ''), axisLabel: { fontSize: 11 } },
        yAxis: { type: 'value', min: 0, max: 100, axisLabel: { formatter: '{value}%' } },
        series: [{
          type: 'line', data: data.map(d => d.successRate), smooth: true,
          lineStyle: { color: '#409eff', width: 2 },
          areaStyle: { color: 'rgba(64,158,255,0.1)' },
          itemStyle: { color: '#409eff' },
          markPoint: {
            data: data.filter(d => d.successRate < 80).map(d => ({
              name: d.date, value: d.successRate, coord: [d.date?.slice(5) || '', d.successRate]
            }))
          }
        }]
      })
    }
  } finally { trendLoading.value = false }
}

async function fetchDuration() {
  durationLoading.value = true
  try {
    const data = await getMonitorDuration(30)
    await nextTick()
    if (durationChartRef.value) {
      durationChart = echarts.init(durationChartRef.value)
      durationChart.setOption({
        tooltip: { trigger: 'axis', formatter: p => `${p[0].name}<br/>平均耗时: ${(p[0].value / 1000).toFixed(1)}s<br/>执行次数: ${data.find(d => d.taskName === p[0].name)?.executionCount || '-'}` },
        grid: { left: 100, right: 20, top: 10, bottom: 30 },
        xAxis: { type: 'value', axisLabel: { formatter: v => (v / 1000).toFixed(0) + 's' } },
        yAxis: { type: 'category', data: data.map(d => d.taskName).reverse(), axisLabel: { fontSize: 11 } },
        series: [{
          type: 'bar', data: data.map(d => d.avgDurationMs).reverse(),
          itemStyle: { color: '#67c23a', borderRadius: [0, 4, 4, 0] },
          label: { show: true, position: 'right', formatter: p => (p.value / 1000).toFixed(1) + 's', fontSize: 11 }
        }]
      })
    }
  } finally { durationLoading.value = false }
}

async function fetchErrorLogs() {
  logsLoading.value = true
  try {
    // 今日失败的日志
    const today = new Date()
    const startStr = `${today.getFullYear()}-${String(today.getMonth()+1).padStart(2,'0')}-${String(today.getDate()).padStart(2,'0')}T00:00:00Z`
    const r = await queryMonitorLogs({
      status: 'Failed', startTime: startStr, pageSize: 20
    })
    errorLogs.value = r.items || []
  } catch { errorLogs.value = [] }
  finally { logsLoading.value = false }
}

async function fetchFailures() {
  try {
    const data = await getMonitorFailures(30)
    failures.value = data || []
  } catch { failures.value = [] }
}

async function fetchAll() {
  loading.value = true
  await Promise.all([
    fetchDashboard(), fetchTrend(), fetchDuration(),
    fetchErrorLogs(), fetchFailures()
  ])
  loading.value = false
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

function formatDate(d) {
  if (!d) return ''
  const dt = new Date(d)
  return `${dt.getFullYear()}-${String(dt.getMonth()+1).padStart(2,'0')}-${String(dt.getDate()).padStart(2,'0')} ${String(dt.getHours()).padStart(2,'0')}:${String(dt.getMinutes()).padStart(2,'0')}`
}

function formatDuration(ms) {
  if (!ms) return '-'
  return (ms / 1000).toFixed(1) + 's'
}

onMounted(fetchAll)
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.stat-card { text-align: center; }
.stat-label { font-size: 12px; color: #909399; margin-bottom: 4px; }
.stat-value { font-size: 28px; font-weight: 700; }
.stat-suffix { font-size: 13px; font-weight: 400; margin-left: 2px; }
.stat-sub { font-size: 11px; margin-top: 2px; }
.chart-placeholder { display: flex; align-items: center; justify-content: center; height: 260px; color: #c0c4cc; }

.failure-list { display: flex; flex-direction: column; gap: 10px; }
.failure-item { display: flex; align-items: center; gap: 8px; }
.failure-rank { flex: 0 0 24px; font-weight: 600; color: #909399; font-size: 13px; text-align: center; }
.failure-info { flex: 1; }
.failure-category { font-size: 12px; color: #606266; margin-bottom: 2px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.failure-trend { flex: 0 0 20px; text-align: center; font-size: 16px; font-weight: 600; }
.failure-trend.up { color: #f56c6c; }
.failure-trend.down { color: #67c23a; }
</style>
