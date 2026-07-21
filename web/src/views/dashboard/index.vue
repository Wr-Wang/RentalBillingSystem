<template>
  <div class="dashboard">
    <!-- ========== 快捷操作 ========== -->
    <div class="quick-actions">
      <el-button type="primary" @click="$router.push('/receipts/register')" class="action-btn">
        <el-icon><Edit /></el-icon>收款登记
      </el-button>
      <el-button type="success" @click="$router.push('/bills/generate')" class="action-btn">
        <el-icon><DocumentAdd /></el-icon>生成账单
      </el-button>
      <el-button type="warning" @click="$router.push('/contracts/create')" class="action-btn">
        <el-icon><DocumentAdd /></el-icon>新建合同
      </el-button>
      <el-button @click="$router.push('/collection')" class="action-btn">
        <el-icon><Bell /></el-icon>催缴管理
      </el-button>
      <el-button @click="$router.push('/reports/companyoverview')" class="action-btn" v-if="userStore.isSuperAdmin">
        <el-icon><DataAnalysis /></el-icon>多公司总览
      </el-button>
    </div>

    <!-- ========== KPI 卡片 ========== -->
    <div class="kpi-grid">
      <div
        v-for="kpi in kpiCards"
        :key="kpi.key"
        class="kpi-card"
        :style="{ borderLeftColor: kpi.color }"
        @click="kpi.path && $router.push(kpi.path)"
      >
        <div class="kpi-header">
          <span class="kpi-label">{{ kpi.label }}</span>
          <el-icon v-if="kpi.icon" :size="18" :style="{ color: kpi.color }" class="kpi-icon">
            <component :is="kpi.icon" />
          </el-icon>
        </div>
        <div class="kpi-value" :style="{ color: kpi.color }">
          {{ kpi.prefix }}{{ kpi.value }}<span v-if="kpi.suffix" class="kpi-suffix">{{ kpi.suffix }}</span>
        </div>
        <div class="kpi-footer">
          <span class="kpi-sub" v-if="kpi.sub">{{ kpi.sub }}</span>
          <span class="kpi-trend" v-if="kpi.trend !== undefined" :class="kpi.trend >= 0 ? 'trend-up' : 'trend-down'">
            <el-icon><Top v-if="kpi.trend >= 0" /><Bottom v-else /></el-icon>
            {{ Math.abs(kpi.trend) }}%
          </span>
        </div>
      </div>
    </div>

    <!-- ========== 图表区 ========== -->
    <el-row :gutter="16" class="chart-row">
      <el-col :xs="24" :sm="24" :md="12" :lg="8">
        <el-card shadow="never" class="chart-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon><DataAnalysis /></el-icon> 本月收租率</span>
            </div>
          </template>
          <div class="chart-box">
            <v-chart :option="collectionPieOption" autoresize />
          </div>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="24" :md="12" :lg="8">
        <el-card shadow="never" class="chart-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon><TrendCharts /></el-icon> 近7日收款趋势</span>
            </div>
          </template>
          <div class="chart-box">
            <v-chart :option="weeklyTrendOption" autoresize />
          </div>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="24" :md="12" :lg="8">
        <el-card shadow="never" class="chart-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon><Coin /></el-icon> 各费用收入分布</span>
            </div>
          </template>
          <div class="chart-box">
            <v-chart :option="feeRevenueOption" autoresize />
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- ========== 中间区：欠费排行 + 即将到期 ========== -->
    <el-row :gutter="16" class="section-row">
      <!-- 欠费排行 -->
      <el-col :xs="24" :md="12">
        <el-card shadow="never" class="section-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon style="color:#f56c6c"><WarningFilled /></el-icon> 欠费排行 Top 6</span>
              <el-button text type="primary" size="small" @click="$router.push('/reports/overduedetail')">查看更多</el-button>
            </div>
          </template>
          <div class="overdue-list" v-loading="overdueLoading">
            <div v-for="(item, idx) in topOverdue" :key="item.id || idx" class="overdue-item" @click="$router.push('/collection')">
              <div class="overdue-rank" :class="'od-rank-' + (idx + 1)">{{ idx + 1 }}</div>
              <div class="overdue-info">
                <div class="overdue-name">{{ item.tenantName || item.contractNo || '未知' }}</div>
                <div class="overdue-desc">{{ item.roomFullCode || item.contractNo || '' }}</div>
              </div>
              <div class="overdue-amount">
                <div class="od-money">¥{{ formatMoney(item.balance || item.amount || 0) }}</div>
                <div class="od-days">{{ item.daysOverdue || 0 }} 天</div>
              </div>
            </div>
            <div v-if="topOverdue.length === 0" class="empty-state">
              <el-icon :size="32" style="color:#67c23a"><CircleCheckFilled /></el-icon>
              <span>暂无欠费记录</span>
            </div>
          </div>
        </el-card>
      </el-col>
      <!-- 即将到期合同 -->
      <el-col :xs="24" :md="12">
        <el-card shadow="never" class="section-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon style="color:#e6a23c"><Clock /></el-icon> 即将到期合同</span>
              <el-button text type="primary" size="small" @click="$router.push('/contracts')">查看更多</el-button>
            </div>
          </template>
          <div class="expiring-list" v-loading="contractLoading">
            <div v-for="(item, idx) in expiringContracts" :key="item.id" class="expiring-item" @click="$router.push('/contracts/' + item.id)">
              <div class="expiring-icon">
                <el-tag :type="getUrgencyTag(item.remainingDays)" size="small" effect="dark" class="days-tag">
                  {{ item.remainingDays }}天
                </el-tag>
              </div>
              <div class="expiring-info">
                <div class="expiring-name">{{ item.contractNo }}</div>
                <div class="expiring-desc">{{ item.tenantName || '未知租客' }} · {{ item.roomName || '' }}</div>
              </div>
              <div class="expiring-end">
                <div class="expiring-date">{{ item.endDate }}</div>
                <div class="expiring-status">
                  <el-tag :type="item.autoRenew ? 'success' : 'info'" size="small" effect="plain">
                    {{ item.autoRenew ? '自动续签' : '手动续签' }}
                  </el-tag>
                </div>
              </div>
            </div>
            <div v-if="expiringContracts.length === 0" class="empty-state">
              <el-icon :size="32" style="color:#67c23a"><CircleCheckFilled /></el-icon>
              <span>暂无即将到期合同</span>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- ========== 底部：最近收款 + 待办事项 ========== -->
    <el-row :gutter="16" class="section-row">
      <el-col :xs="24" :md="14">
        <el-card shadow="never" class="section-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon style="color:#409eff"><Money /></el-icon> 最近收款</span>
              <el-button text type="primary" size="small" @click="$router.push('/receipts')">查看全部</el-button>
            </div>
          </template>
          <el-table :data="recentReceipts" v-loading="receiptLoading" stripe style="width:100%">
            <el-table-column prop="receiptNo" label="收据号" min-width="150" />
            <el-table-column prop="contractNo" label="合同号" min-width="140" />
            <el-table-column label="金额" width="110" align="right">
              <template #default="{ row }">¥{{ formatMoney(row.amount) }}</template>
            </el-table-column>
            <el-table-column label="方式" width="90">
              <template #default="{ row }">
                <el-tag size="small" effect="plain">{{ row.paymentChannelName || '—' }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="状态" width="90" align="center">
              <template #default="{ row }">
                <el-tag :type="row.status === 'Confirmed' ? 'success' : row.status === 'Pending' ? 'warning' : 'danger'" size="small">
                  {{ statusMap[row.status] || row.status }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="receivedDate" label="日期" width="100" />
          </el-table>
        </el-card>
      </el-col>
      <el-col :xs="24" :md="10">
        <el-card shadow="never" class="section-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon style="color:#e6a23c"><List /></el-icon> 待办事项</span>
              <el-tag v-if="todoList.length" type="danger" size="small">{{ todoList.length }}</el-tag>
            </div>
          </template>
          <div class="todo-list" v-loading="todoLoading">
            <div v-for="(item, idx) in todoList" :key="idx" class="todo-item" @click="handleTodoAction(item)">
              <div class="todo-dot" :style="{ background: dotColor(item.tagType) }" />
              <div class="todo-body">
                <div class="todo-title">{{ item.content }}</div>
                <div class="todo-meta">
                  <el-tag :type="item.tagType" size="small" effect="plain">{{ item.type }}</el-tag>
                  <span class="todo-date">{{ item.date }}</span>
                </div>
              </div>
              <el-icon class="todo-arrow"><ArrowRight /></el-icon>
            </div>
            <div v-if="todoList.length === 0 && !todoLoading" class="empty-state">
              <el-icon :size="32" style="color:#67c23a"><CircleCheckFilled /></el-icon>
              <span>暂无待办事项</span>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/store/user'
import { ElMessage } from 'element-plus'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { PieChart, BarChart, LineChart } from 'echarts/charts'
import { TitleComponent, TooltipComponent, LegendComponent, GridComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { chinaTime } from '@/utils/chinaTime'
import {
  getReceipts, getContracts, getDailyReceipt,
  getMonthlyReceipt, getOverdueDetail, getPendingApprovals, getUnreadCounts,
  getFeeRevenue
} from '@/api'

use([CanvasRenderer, PieChart, BarChart, LineChart, TitleComponent, TooltipComponent, LegendComponent, GridComponent])

const router = useRouter()
const userStore = useUserStore()

// ==================== 基础状态 ====================
const loading = ref(false)
const receiptLoading = ref(false)
const todoLoading = ref(false)
const overdueLoading = ref(false)
const contractLoading = ref(false)

// 数据状态
const todayStats = ref({ received: 0, count: 0 })
const monthStats = ref({ receivable: 0, received: 0, overdue: 0, collectionRate: 0 })
const overdueContracts = ref(0)
const pendingCollection = ref(0)
const recentReceipts = ref([])
const todoList = ref([])
const trendData = ref([0, 0, 0, 0, 0, 0, 0])
const overdueList = ref([])
const allContracts = ref([])
const feeRevenueData = ref([])
const contractCount = ref(0)
const activeContractCount = ref(0)

// ==================== 工具函数 ====================
function formatMoney(val) {
  if (!val && val !== 0) return '0.00'
  return Number(val).toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const statusMap = { Confirmed: '已确认', Pending: '待确认', Rejected: '已驳回', Cancelled: '已取消' }

// ==================== KPI 卡片 ====================
const kpiCards = computed(() => [
  {
    key: 'todayReceived', label: '今日收款', icon: 'Money',
    value: formatMoney(todayStats.value.received), prefix: '¥',
    color: '#409eff', sub: `${todayStats.value.count} 笔`,
    path: '/receipts'
  },
  {
    key: 'monthReceivable', label: '本月应收', icon: 'Coin',
    value: formatMoney(monthStats.value.receivable), prefix: '¥',
    color: '#e6a23c', sub: '含所有费用项目',
    path: '/bills'
  },
  {
    key: 'monthReceived', label: '本月已收', icon: 'CircleCheck',
    value: formatMoney(monthStats.value.received), prefix: '¥',
    color: '#67c23a', sub: `${monthStats.value.collectionRate}% 收租率`,
    path: '/receipts'
  },
  {
    key: 'monthOverdue', label: '本月欠费', icon: 'WarningFilled',
    value: formatMoney(monthStats.value.overdue), prefix: '¥',
    color: '#f56c6c', sub: '待催缴',
    path: '/collection'
  },
  {
    key: 'collectionRate', label: '综合收租率', icon: 'TrendCharts',
    value: monthStats.value.collectionRate, suffix: '%',
    color: monthStats.value.collectionRate >= 90 ? '#67c23a' : monthStats.value.collectionRate >= 70 ? '#e6a23c' : '#f56c6c',
    sub: monthStats.value.collectionRate >= 90 ? '良好' : monthStats.value.collectionRate >= 70 ? '正常' : '偏低',
    path: '/reports/collectionrate'
  },
  {
    key: 'overdueContracts', label: '逾期合同', icon: 'WarningFilled',
    value: overdueContracts.value,
    color: overdueContracts.value > 0 ? '#f56c6c' : '#67c23a',
    sub: `待催缴 ${pendingCollection.value} 户`,
    path: '/collection'
  },
  {
    key: 'activeContracts', label: '在租合同', icon: 'Document',
    value: activeContractCount.value,
    color: '#409eff', sub: `共 ${contractCount.value} 份合同`,
    path: '/contracts'
  },
  {
    key: 'expiringSoon', label: '即将到期', icon: 'Clock',
    value: expiringContracts.value.length,
    color: expiringContracts.value.length > 0 ? '#e6a23c' : '#67c23a',
    sub: '30天内到期',
    path: '/renewaldashboard'
  },
  {
    key: 'pendingTasks', label: '待办事项', icon: 'List',
    value: todoList.value.length,
    color: todoList.value.length > 0 ? '#e6a23c' : '#67c23a',
    sub: todoList.value.length > 0 ? '待处理' : '全部完成',
    path: '#'
  }
])

// ==================== 图表：收租率饼图 ====================
const collectionPieOption = computed(() => ({
  tooltip: { trigger: 'item', formatter: '{b}: ¥{c} ({d}%)' },
  legend: { bottom: 0, left: 'center', icon: 'circle', itemWidth: 8, itemHeight: 8 },
  series: [{
    type: 'pie',
    radius: ['45%', '70%'],
    center: ['50%', '42%'],
    avoidLabelOverlap: false,
    label: { show: false },
    emphasis: { label: { show: true, fontSize: 14, fontWeight: 'bold' } },
    itemStyle: { borderRadius: 6, borderColor: '#fff', borderWidth: 2 },
    data: [
      { value: monthStats.value.received || 1, name: '已收款', itemStyle: { color: '#67c23a' } },
      { value: monthStats.value.overdue || 1, name: '欠费', itemStyle: { color: '#f56c6c' } }
    ]
  }]
}))

// ==================== 图表：近7日收款趋势 ====================
const weeklyTrendOption = computed(() => ({
  tooltip: { trigger: 'axis', formatter: p => `${p[0].axisValue}<br/>收款: ¥${formatMoney(p[0].value)}` },
  grid: { left: '8%', right: '4%', bottom: '12%', top: '5%' },
  xAxis: { type: 'category', data: trendLabels.value, axisLabel: { fontSize: 10 } },
  yAxis: { type: 'value', axisLabel: { formatter: v => '¥' + (v >= 10000 ? (v / 10000).toFixed(1) + '万' : v) }, splitLine: { lineStyle: { type: 'dashed', color: '#f0f0f0' } } },
  series: [{
    type: 'bar',
    data: trendData.value.map(v => ({
      value: v,
      itemStyle: {
        color: v > 0 ? '#409eff' : '#e0e0e0',
        borderRadius: [4, 4, 0, 0]
      }
    })),
    barMaxWidth: 32,
    label: { show: true, position: 'top', formatter: p => p.value > 0 ? '¥' + (p.value >= 10000 ? (p.value / 10000).toFixed(1) + '万' : p.value) : '', fontSize: 9 }
  }]
}))
const trendLabels = computed(() => {
  const d = []
  for (let i = 6; i >= 0; i--) {
    const dt = chinaTime.now()
    dt.setDate(dt.getDate() - i)
    d.push(`${String(dt.getMonth() + 1).padStart(2, '0')}-${String(dt.getDate()).padStart(2, '0')}`)
  }
  return d
})

// ==================== 图表：各费用收入分布 ====================
const feeRevenueOption = computed(() => ({
  tooltip: { trigger: 'axis', formatter: p => `${p[0].name}<br/>收入: ¥${formatMoney(p[0].value)}` },
  grid: { left: '8%', right: '4%', bottom: '18%', top: '5%' },
  xAxis: { type: 'category', data: feeRevenueData.value.map(f => f.name || f.feeCodeName || '其他'), axisLabel: { rotate: 20, fontSize: 9 } },
  yAxis: { type: 'value', axisLabel: { formatter: v => '¥' + (v >= 10000 ? (v / 10000).toFixed(1) + '万' : v) }, splitLine: { lineStyle: { type: 'dashed', color: '#f0f0f0' } } },
  series: [{
    type: 'bar',
    data: feeRevenueData.value.map((f, i) => ({
      value: f.total || f.amount || 0,
      itemStyle: { color: CHART_COLORS[i % CHART_COLORS.length], borderRadius: [4, 4, 0, 0] }
    })),
    barMaxWidth: 28,
    label: { show: true, position: 'top', formatter: p => p.value > 0 ? '¥' + (p.value >= 10000 ? (p.value / 10000).toFixed(1) + '万' : p.value) : '', fontSize: 9 }
  }]
}))

const CHART_COLORS = ['#409eff', '#67c23a', '#e6a23c', '#f56c6c', '#909399', '#b37feb', '#5cdbd3', '#ff85c0']

// ==================== 欠费排行 ====================
const topOverdue = computed(() => {
  return [...overdueList.value]
    .sort((a, b) => (b.balance || b.amount || 0) - (a.balance || a.amount || 0))
    .slice(0, 6)
})

// ==================== 即将到期合同 ====================
const expiringContracts = computed(() => {
  const now = chinaTime.now()
  const thirtyDays = 30 * 86400000
  return allContracts.value
    .filter(c => c.status === 'Active' && c.endDate)
    .map(c => {
      const end = new Date(c.endDate)
      const remaining = Math.ceil((end - now) / 86400000)
      return { ...c, remainingDays: remaining, endDate: formatDate(end) }
    })
    .filter(c => c.remainingDays >= 0 && c.remainingDays <= 30)
    .sort((a, b) => a.remainingDays - b.remainingDays)
    .slice(0, 6)
})

function formatDate(d) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function getUrgencyTag(days) {
  if (days <= 7) return 'danger'
  if (days <= 14) return 'warning'
  return 'info'
}

// ==================== 待办事项 ====================
function handleTodoAction(item) {
  const routes = {
    '收款确认': '/receipts/confirm',
    '待审批': '/approvals',
    '续签': '/renewaldashboard',
    '催缴': '/collection',
    '通知': '/notifications'
  }
  const path = routes[item.type]
  if (path) router.push(path)
}

function dotColor(type) {
  const map = { primary: '#409eff', success: '#67c23a', warning: '#e6a23c', danger: '#f56c6c', info: '#909399' }
  return map[type] || '#909399'
}

// ==================== 数据加载 ====================
async function loadData() {
  loading.value = true
  const now = chinaTime.now()
  const period = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`
  const today = chinaTime.today()
  const companyId = userStore.effectiveCompanyId

  try {
    // 今日收款
    const dr = await getDailyReceipt({ date: today, companyId })
    const details = dr.details || dr || []
    const totalReceived = details.reduce((s, r) => s + (r.total || 0), 0)
    const totalCount = details.reduce((s, r) => s + (r.cnt || 0), 0)
    todayStats.value = { received: totalReceived, count: totalCount }
  } catch { /* ignore */ }

  try {
    // 月度数据
    const mr = await getMonthlyReceipt({ period, companyId })
    const ta = mr.totalAmount || 0
    const tr = mr.totalReceived || 0
    monthStats.value = {
      receivable: ta,
      received: tr,
      overdue: ta - tr,
      collectionRate: ta > 0 ? Math.round((tr / ta) * 100) : 0
    }
  } catch { /* ignore */ }

  try {
    // 逾期明细
    const od = await getOverdueDetail({ companyId })
    overdueList.value = od || []
    overdueContracts.value = od?.length || 0
    pendingCollection.value = od?.filter(p => (p.daysOverdue || 0) > 7).length || 0
  } catch { /* ignore */ }

  // 最近收款
  receiptLoading.value = true
  try {
    const r = await getReceipts({ companyId, pageSize: 6 })
    recentReceipts.value = (r?.items || r || []).slice(0, 6)
  } catch { /* ignore */ }
  finally { receiptLoading.value = false }

  // 合同数据
  contractLoading.value = true
  try {
    const cr = await getContracts({ pageSize: 1000, companyId })
    const items = cr?.items || cr?.data || []
    allContracts.value = items
    contractCount.value = items.length
    activeContractCount.value = items.filter(c => c.status === 'Active').length
  } catch { /* ignore */ }
  finally { contractLoading.value = false }

  // 待办事项
  todoLoading.value = true
  try {
    const items = []
    const [pendingReceipts, allContractsRes, overduePlans, pendingApprovals, unreadCounts] = await Promise.all([
      getReceipts({ status: 'Pending', companyId }).catch(() => []),
      getContracts({ pageSize: 100, companyId }).catch(() => ({ items: [] })),
      getOverdueDetail({ companyId }).catch(() => []),
      getPendingApprovals().catch(() => null),
      getUnreadCounts().catch(() => null)
    ])
    if (pendingReceipts?.length > 0) items.push({ type: '收款确认', tagType: 'warning', content: `${pendingReceipts.length} 笔收款待确认`, date: today })
    if (pendingApprovals?.length > 0) items.push({ type: '待审批', tagType: 'primary', content: `${pendingApprovals.length} 个审批请求待处理`, date: today })
    const actives = (allContractsRes?.items || []).filter(c => c.status === 'Active')
    const expiringSoon = actives.filter(c => c.endDate && new Date(c.endDate) < new Date(chinaTime.now().getTime() + 14 * 86400000))
    if (expiringSoon.length > 0) items.push({ type: '续签', tagType: 'primary', content: `${expiringSoon.length} 份合同即将到期`, date: today })
    if (overduePlans?.length > 0) items.push({ type: '催缴', tagType: 'danger', content: `${overduePlans.length} 户逾期欠费`, date: today })
    const unreadTotal = unreadCounts?.Total || 0
    if (unreadTotal > 0) items.push({ type: '通知', tagType: 'info', content: `${unreadTotal} 条未读通知`, date: today })
    todoList.value = items.length > 0 ? items : [{ type: '提示', tagType: 'info', content: '暂无待办事项，一切正常', date: today }]
  } catch { /* ignore */ }
  finally { todoLoading.value = false }

  // 收租率趋势（近6月）
  try {
    await loadDailyTrend(period, companyId)
  } catch { /* ignore */ }

  // 费用收入分布
  try {
    const fr = await getFeeRevenue({ period })
    const raw = fr?.data || fr || []
    feeRevenueData.value = raw.slice(0, 8)
  } catch { /* ignore */ }

  loading.value = false
}

// ==================== 近7日收款趋势 ====================
async function loadDailyTrend(period, companyId) {
  const data = []
  for (let i = 6; i >= 0; i--) {
    const dt = chinaTime.now()
    dt.setDate(dt.getDate() - i)
    const dateStr = `${dt.getFullYear()}-${String(dt.getMonth() + 1).padStart(2, '0')}-${String(dt.getDate()).padStart(2, '0')}`
    try {
      const dr = await getDailyReceipt({ date: dateStr, companyId })
      const details = dr.details || dr || []
      const total = details.reduce((s, r) => s + (r.total || 0), 0)
      data.push(total)
    } catch { data.push(0) }
  }
  trendData.value = data
}

// ==================== 生命周期 ====================
onMounted(loadData)
</script>

<style scoped>
/* ==================== 页面容器 ==================== */
.dashboard {
  max-width: 1600px;
  margin: 0 auto;
}

/* ==================== 快捷操作 ==================== */
.quick-actions {
  display: flex;
  gap: 8px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.quick-actions .action-btn {
  border-radius: 8px;
  padding: 8px 16px;
  font-size: 13px;
}

/* ==================== KPI 卡片 ==================== */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(9, 1fr);
  gap: 12px;
  margin-bottom: 20px;
}

@media (max-width: 1400px) {
  .kpi-grid { grid-template-columns: repeat(5, 1fr); }
}
@media (max-width: 900px) {
  .kpi-grid { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 600px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
}

.kpi-card {
  background: #fff;
  border-radius: 10px;
  padding: 14px 14px 12px;
  border-left: 4px solid #409eff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.kpi-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
}

.kpi-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.kpi-label {
  font-size: 12px;
  color: #909399;
  font-weight: 500;
}

.kpi-icon {
  opacity: 0.7;
}

.kpi-value {
  font-size: 22px;
  font-weight: 700;
  line-height: 1.2;
  display: flex;
  align-items: baseline;
  gap: 2px;
}

.kpi-suffix {
  font-size: 14px;
  font-weight: 600;
}

.kpi-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 2px;
}

.kpi-sub {
  font-size: 11px;
  color: #909399;
}

.kpi-trend {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: 11px;
  font-weight: 600;
}
.kpi-trend.trend-up { color: #f56c6c; }
.kpi-trend.trend-down { color: #67c23a; }
.kpi-trend .el-icon { font-size: 12px; }

/* ==================== 图表 ==================== */
.chart-row {
  margin-bottom: 16px;
}

.chart-card {
  margin-bottom: 16px;
  border-radius: 10px;
}
.chart-card :deep(.el-card__header) {
  padding: 12px 18px;
  border-bottom: 1px solid #f0f2f5;
  font-weight: 600;
  font-size: 14px;
}
.chart-card :deep(.el-card__body) {
  padding: 0;
}
.chart-box {
  height: 280px;
  padding: 6px 4px 0;
}

.card-header-with-action {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.card-header-with-action .el-icon {
  margin-right: 4px;
}

/* ==================== Section 通用 ==================== */
.section-row {
  margin-bottom: 0;
}
.section-card {
  margin-bottom: 16px;
  border-radius: 10px;
}
.section-card :deep(.el-card__header) {
  padding: 12px 18px;
  border-bottom: 1px solid #f0f2f5;
  font-weight: 600;
  font-size: 14px;
}
.section-card :deep(.el-card__body) {
  padding: 8px 16px;
}

/* ==================== 欠费排行 ==================== */
.overdue-list {
  max-height: 340px;
  overflow-y: auto;
}

.overdue-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 8px;
  border-bottom: 1px solid #f5f5f5;
  cursor: pointer;
  transition: background 0.2s;
  border-radius: 6px;
}
.overdue-item:hover { background: #fafafa; }
.overdue-item:last-child { border-bottom: none; }

.overdue-rank {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  flex-shrink: 0;
  background: #f5f7fa;
  color: #909399;
}
.od-rank-1 { background: #fff0e0; color: #e6a23c; }
.od-rank-2 { background: #f0f5ff; color: #409eff; }
.od-rank-3 { background: #f5f0ff; color: #722ed1; }

.overdue-info {
  flex: 1;
  min-width: 0;
}
.overdue-name {
  font-size: 13px;
  font-weight: 600;
  color: #303133;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.overdue-desc {
  font-size: 11px;
  color: #909399;
  margin-top: 1px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.overdue-amount {
  text-align: right;
  flex-shrink: 0;
}
.od-money {
  font-size: 14px;
  font-weight: 700;
  color: #f56c6c;
}
.od-days {
  font-size: 11px;
  color: #909399;
  margin-top: 1px;
}

/* ==================== 即将到期 ==================== */
.expiring-list {
  max-height: 340px;
  overflow-y: auto;
}

.expiring-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 8px;
  border-bottom: 1px solid #f5f5f5;
  cursor: pointer;
  transition: background 0.2s;
  border-radius: 6px;
}
.expiring-item:hover { background: #fafafa; }
.expiring-item:last-child { border-bottom: none; }

.expiring-icon {
  flex-shrink: 0;
}
.days-tag {
  min-width: 48px;
  text-align: center;
}

.expiring-info {
  flex: 1;
  min-width: 0;
}
.expiring-name {
  font-size: 13px;
  font-weight: 600;
  color: #303133;
}
.expiring-desc {
  font-size: 11px;
  color: #909399;
  margin-top: 1px;
}

.expiring-end {
  text-align: right;
  flex-shrink: 0;
}
.expiring-date {
  font-size: 12px;
  color: #606266;
  font-weight: 500;
}
.expiring-status {
  margin-top: 2px;
}

/* ==================== 待办事项 ==================== */
.todo-list {
  max-height: 380px;
  overflow-y: auto;
}

.todo-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 11px 8px;
  border-bottom: 1px solid #f5f5f5;
  cursor: pointer;
  transition: background 0.2s;
  border-radius: 6px;
}
.todo-item:hover { background: #fafafa; }
.todo-item:last-child { border-bottom: none; }

.todo-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}
.todo-body {
  flex: 1;
  min-width: 0;
}
.todo-title {
  font-size: 13px;
  color: #303133;
  font-weight: 500;
}
.todo-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 3px;
}
.todo-date {
  font-size: 11px;
  color: #c0c4cc;
}
.todo-arrow {
  color: #c0c4cc;
  font-size: 14px;
  flex-shrink: 0;
}

/* ==================== 空状态 ==================== */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 32px 16px;
  color: #909399;
  font-size: 13px;
}

/* ==================== 表格覆盖 ==================== */
:deep(.el-table th.el-table__cell) {
  background: #f5f7fa !important;
  color: #606266;
  font-weight: 600;
  font-size: 12px;
}
:deep(.el-table .cell) {
  white-space: nowrap;
}
</style>
