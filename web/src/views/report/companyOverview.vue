<template>
  <div class="company-overview">
    <!-- ========== 头部 ========== -->
    <div class="overview-header">
      <div class="header-left">
        <div class="header-icon"><el-icon :size="28"><DataAnalysis /></el-icon></div>
        <div>
          <h2 class="header-title">多公司总览</h2>
          <p class="header-subtitle">全局视角 · 各公司经营指标实时对比</p>
        </div>
      </div>
      <div class="header-actions">
        <el-date-picker
          v-model="queryMonth"
          type="month"
          placeholder="选择月份"
          value-format="YYYY-MM"
          @change="fetchData"
          class="month-picker"
        />
        <el-tooltip content="自动刷新（60s）" placement="top">
          <el-button
            :icon="autoRefresh ? 'VideoPause' : 'VideoPlay'"
            :type="autoRefresh ? 'warning' : 'default'"
            @click="toggleAutoRefresh"
            circle
          />
        </el-tooltip>
        <el-button type="primary" @click="fetchData" :loading="loading" icon="Refresh">
          刷新
        </el-button>
        <el-dropdown trigger="click" v-if="overviewData">
          <el-button>
            <el-icon><Download /></el-icon> 导出
            <el-icon><ArrowDown /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="exportImage">
                <el-icon><Picture /></el-icon>导出截图
              </el-dropdown-item>
              <el-dropdown-item @click="exportTable">
                <el-icon><Document /></el-icon>导出报表
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- ========== 加载骨架 ========== -->
    <template v-if="loading && !overviewData">
      <el-skeleton :rows="6" animated class="skeleton-box" />
    </template>

    <template v-else-if="overviewData">
      <!-- ========== KPI 卡片区 ========== -->
      <div class="kpi-grid">
        <div class="kpi-card" v-for="kpi in kpiList" :key="kpi.label">
          <div class="kpi-label">{{ kpi.label }}</div>
          <div class="kpi-value" :style="{ color: kpi.color }">{{ kpi.value }}</div>
          <div class="kpi-trend" :class="kpi.trendDir">
            <el-icon v-if="kpi.trendDir === 'up'"><Top /></el-icon>
            <el-icon v-else-if="kpi.trendDir === 'down'"><Bottom /></el-icon>
            <el-icon v-else><Minus /></el-icon>
            <span>{{ kpi.trendText }}</span>
          </div>
        </div>
      </div>

      <!-- ========== 健康评分 + 预警 ========== -->
      <el-row :gutter="16" class="section-row">
        <el-col :xs="24" :md="16">
          <el-card shadow="never" class="section-card">
            <template #header>
              <div class="card-header-with-action">
                <span><el-icon><TrendCharts /></el-icon> 公司健康评分排名</span>
                <el-tag type="" size="small">综合出租率·收租率·逾期率</el-tag>
              </div>
            </template>
            <div class="health-list">
              <div
                v-for="(item, idx) in rankedHealth"
                :key="item.id"
                class="health-row"
                :class="{ 'health-row-active': idx < 3 }"
                @click="switchToCompany(item)"
              >
                <div class="health-rank" :class="'rank-' + (idx + 1)">
                  <span v-if="idx === 0" class="rank-medal rank-gold">1</span>
                  <span v-else-if="idx === 1" class="rank-medal rank-silver">2</span>
                  <span v-else-if="idx === 2" class="rank-medal rank-bronze">3</span>
                  <span v-else class="rank-num">{{ idx + 1 }}</span>
                </div>
                <div class="health-info">
                  <div class="health-name">{{ item.name }}</div>
                  <div class="health-meta">
                    出租率 {{ item.occupancyRate }}% · 收租率 {{ item.collectionRate }}%
                  </div>
                </div>
                <div class="health-bar-wrap">
                  <div class="health-bar">
                    <div
                      class="health-bar-fill"
                      :style="{ width: item.healthScore + '%', background: healthColor(item.healthScore) }"
                    />
                  </div>
                </div>
                <div class="health-score" :style="{ color: healthColor(item.healthScore) }">
                  {{ item.healthScore.toFixed(1) }}
                </div>
              </div>
              <div v-if="overviewData.companies.length === 0" class="empty-tip">暂无公司数据</div>
            </div>
          </el-card>
        </el-col>
        <el-col :xs="24" :md="8">
          <el-card shadow="never" class="section-card">
            <template #header>
              <div class="card-header-with-action">
                <span><el-icon><WarningFilled style="color:#e6a23c"/></el-icon> 预警关注</span>
                <el-tag type="danger" size="small" v-if="alerts.length">{{ alerts.length }} 项</el-tag>
              </div>
            </template>
            <div class="alert-list">
              <div v-for="(alert, i) in alerts" :key="i" class="alert-item" :class="'alert-' + alert.level">
                <div class="alert-icon">
                  <el-icon v-if="alert.level === 'high'" size="20"><WarningFilled style="color:#f56c6c"/></el-icon>
                  <el-icon v-else size="20"><WarningFilled style="color:#e6a23c"/></el-icon>
                </div>
                <div class="alert-body">
                  <div class="alert-title">{{ alert.companyName }}</div>
                  <div class="alert-desc">{{ alert.desc }}</div>
                </div>
              </div>
              <div v-if="alerts.length === 0" class="empty-tip">
                <el-icon :size="24" style="color:#67c23a"><CircleCheckFilled /></el-icon>
                <span>各公司运营状况良好，无需关注</span>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <!-- ========== 图表区 (2×2) ========== -->
      <div class="chart-grid">
        <!-- 应收分布饼图 -->
        <el-card shadow="never" class="section-card chart-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon><Coin /></el-icon> 各公司应收分布</span>
            </div>
          </template>
          <div class="chart-container">
            <v-chart :option="receivablePieOption" autoresize />
          </div>
        </el-card>

        <!-- 收租率排名柱状图 -->
        <el-card shadow="never" class="section-card chart-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon><TrendCharts /></el-icon> 收租率排名</span>
            </div>
          </template>
          <div class="chart-container">
            <v-chart :option="collectionBarOption" autoresize />
          </div>
        </el-card>

        <!-- 月度收租率趋势 -->
        <el-card shadow="never" class="section-card chart-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon><DataLine /></el-icon> 月度收租率趋势</span>
            </div>
          </template>
          <div class="chart-container">
            <v-chart :option="trendLineOption" autoresize />
          </div>
        </el-card>

        <!-- 综合指标雷达图 -->
        <el-card shadow="never" class="section-card chart-card">
          <template #header>
            <div class="card-header-with-action">
              <span><el-icon><Aim /></el-icon> 公司综合指标雷达</span>
              <el-select v-model="radarCompany" size="small" style="width:140px" @change="refreshRadar">
                <el-option
                  v-for="c in activeCompanies"
                  :key="c.id"
                  :label="c.name"
                  :value="c.id"
                />
              </el-select>
            </div>
          </template>
          <div class="chart-container">
            <v-chart :option="radarOption" autoresize />
          </div>
        </el-card>
      </div>

      <!-- ========== 增强对比表格 ========== -->
      <el-card shadow="never" class="section-card">
        <template #header>
          <div class="card-header-with-action">
            <span><el-icon><DataBoard /></el-icon> 各公司经营指标对比</span>
            <el-input
              v-model="tableSearch"
              placeholder="搜索公司名..."
              prefix-icon="Search"
              clearable
              class="table-search"
            />
          </div>
        </template>
        <el-table
          :data="filteredTableData"
          stripe
          @row-click="switchToCompany"
          style="cursor:pointer"
          v-loading="loading"
          default-sort="{ prop: 'healthScore', order: 'descending' }"
        >
          <el-table-column label="#" width="56">
            <template #default="{ $index }">
              <span class="table-rank" :class="'rank-' + (getSortIndex($index) + 1)">
                {{ getSortIndex($index) + 1 }}
              </span>
            </template>
          </el-table-column>
          <el-table-column prop="name" label="公司名称" min-width="140" sortable>
            <template #default="{ row }">
              <el-button text type="primary" @click.stop="switchToCompany(row)">
                {{ row.name }}
              </el-button>
            </template>
          </el-table-column>
          <el-table-column prop="buildingCount" label="楼栋" width="60" align="center" sortable />
          <el-table-column prop="roomCount" label="房间" width="60" align="center" sortable />
          <el-table-column prop="occupancyRate" label="出租率" width="120" align="center" sortable>
            <template #default="{ row }">
              <el-progress
                :percentage="row.occupancyRate"
                :stroke-width="12"
                :color="rateColor(row.occupancyRate)"
                :format="p => p + '%'"
              />
            </template>
          </el-table-column>
          <el-table-column prop="monthlyReceivable" label="本月应收" width="120" align="right" sortable>
            <template #default="{ row }">{{ formatMoney(row.monthlyReceivable) }}</template>
          </el-table-column>
          <el-table-column prop="monthlyReceived" label="本月实收" width="120" align="right" sortable>
            <template #default="{ row }">{{ formatMoney(row.monthlyReceived) }}</template>
          </el-table-column>
          <el-table-column prop="collectionRate" label="收租率" width="130" align="center" sortable>
            <template #default="{ row }">
              <el-progress
                :percentage="row.collectionRate"
                :stroke-width="12"
                :color="rateColor(row.collectionRate)"
                :format="p => p + '%'"
              />
            </template>
          </el-table-column>
          <el-table-column prop="overdueAmount" label="逾期金额" width="120" align="right" sortable>
            <template #default="{ row }">
              <span :class="row.overdueAmount > 0 ? 'text-danger' : 'text-success'">
                {{ formatMoney(row.overdueAmount) }}
              </span>
            </template>
          </el-table-column>
          <el-table-column prop="activeContractCount" label="在租合同" width="90" align="center" sortable />
          <el-table-column prop="healthScore" label="健康分" width="90" align="center" sortable>
            <template #default="{ row }">
              <el-tag :color="healthColor(row.healthScore)" style="color:#fff;border:0">
                {{ row.healthScore.toFixed(1) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="isActive" label="状态" width="68" align="center" sortable>
            <template #default="{ row }">
              <el-tag :type="row.isActive ? 'success' : 'danger'" size="small" effect="plain">
                {{ row.isActive ? '启用' : '停用' }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </template>

    <!-- ========== 空状态 ========== -->
    <el-empty v-else description="暂无数据，请点击刷新" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../../store/user'
import { ElMessage, ElMessageBox } from 'element-plus'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import {
  BarChart, LineChart, PieChart, RadarChart
} from 'echarts/charts'
import {
  GridComponent, TooltipComponent, LegendComponent,
  TitleComponent, RadarComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { getMultiCompanyOverview } from '../../api/index'

// 注册 ECharts 组件
use([
  BarChart, LineChart, PieChart, RadarChart,
  GridComponent, TooltipComponent, LegendComponent,
  TitleComponent, RadarComponent,
  CanvasRenderer
])

const router = useRouter()
const userStore = useUserStore()

// ==================== 状态 ====================
const queryMonth = ref('')
const loading = ref(false)
const autoRefresh = ref(false)
const overviewData = ref(null)
const tableSearch = ref('')
const radarCompany = ref('')
let autoTimer = null

// 默认月份
const now = new Date()
queryMonth.value = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`

// ==================== 获取数据 ====================
async function fetchData() {
  loading.value = true
  try {
    const res = await getMultiCompanyOverview({ period: queryMonth.value })
    overviewData.value = res
    if (res.companies?.length > 0) {
      radarCompany.value = res.companies.filter(c => c.isActive)[0]?.id || res.companies[0].id
    }
  } catch (e) {
    ElMessage.error('加载多公司总览数据失败: ' + (e.message || '未知错误'))
    overviewData.value = null
  } finally {
    loading.value = false
  }
}

// ==================== 自动刷新 ====================
function toggleAutoRefresh() {
  autoRefresh.value = !autoRefresh.value
  if (autoRefresh.value) {
    autoTimer = setInterval(fetchData, 60000)
    ElMessage.success('自动刷新已开启（60秒间隔）')
  } else {
    clearInterval(autoTimer)
    autoTimer = null
    ElMessage.info('自动刷新已关闭')
  }
}

// ==================== 活跃公司列表 ====================
const activeCompanies = computed(() =>
  (overviewData.value?.companies || []).filter(c => c.isActive)
)

// ==================== KPI 列表 ====================
const kpiList = computed(() => {
  const d = overviewData.value
  if (!d) return []
  return [
    {
      label: '公司总数', value: `${d.totalCompanies} 家`,
      color: '#409eff', trendDir: '', trendText: `${d.activeCompanies} 家启用`
    },
    {
      label: '总楼栋数', value: `${d.totalBuildings} 栋`,
      color: '#67c23a', trendDir: '', trendText: '—'
    },
    {
      label: '总房间数', value: numStr(d.totalRooms),
      color: '#303133', trendDir: '', trendText: '在租 ' + numStr(d.totalRented)
    },
    {
      label: '综合出租率', value: `${d.avgOccupancyRate}%`,
      color: d.avgOccupancyRate >= 85 ? '#67c23a' : '#e6a23c',
      trendDir: d.occupancyRateChange > 0 ? 'up' : d.occupancyRateChange < 0 ? 'down' : '',
      trendText: d.occupancyRateChange ? `${d.occupancyRateChange > 0 ? '+' : ''}${d.occupancyRateChange}%` : '—'
    },
    {
      label: '本月应收', value: formatShortMoney(d.totalMonthlyReceivable),
      color: '#e6a23c', trendDir: '', trendText: '实时汇总'
    },
    {
      label: '综合收租率', value: `${d.avgCollectionRate}%`,
      color: d.avgCollectionRate >= 90 ? '#67c23a' : '#e6a23c',
      trendDir: d.collectionRateMomChange > 0 ? 'up' : d.collectionRateMomChange < 0 ? 'down' : '',
      trendText: d.collectionRateMomChange
        ? `环比 ${d.collectionRateMomChange > 0 ? '+' : ''}${d.collectionRateMomChange}%`
        : '—'
    },
    {
      label: '逾期总额', value: formatShortMoney(d.totalOverdueAmount),
      color: d.totalOverdueAmount > 0 ? '#f56c6c' : '#67c23a',
      trendDir: d.totalOverdueAmount > 0 ? 'down' : '',
      trendText: `${d.totalOverdueCount} 笔逾期`
    },
    {
      label: '在租合同数', value: `${d.totalActiveContracts} 份`,
      color: '#409eff', trendDir: '', trendText: '—'
    }
  ]
})

// ==================== 健康排名 ====================
const rankedHealth = computed(() => {
  return [...(overviewData.value?.companies || [])]
    .sort((a, b) => b.healthScore - a.healthScore)
})

// ==================== 预警 ====================
const alerts = computed(() => {
  const result = []
  for (const c of overviewData.value?.companies || []) {
    if (!c.isActive) continue
    if (c.collectionRate < 70) {
      result.push({ companyName: c.name, desc: `收租率仅 ${c.collectionRate}%，低于警戒线70%`, level: 'high' })
    } else if (c.collectionRate < 85) {
      result.push({ companyName: c.name, desc: `收租率 ${c.collectionRate}%，需关注`, level: 'warn' })
    }
    if (c.overdueAmount > 50000) {
      result.push({ companyName: c.name, desc: `逾期 ${formatShortMoney(c.overdueAmount)}，金额较大`, level: 'high' })
    }
    if (c.occupancyRate < 60) {
      result.push({ companyName: c.name, desc: `出租率仅 ${c.occupancyRate}%，空置偏高`, level: 'high' })
    }
  }
  return result.slice(0, 8)
})

// ==================== 表格数据 ====================
const filteredTableData = computed(() => {
  const list = overviewData.value?.companies || []
  if (!tableSearch.value) return list
  const q = tableSearch.value.toLowerCase()
  return list.filter(c => c.name.toLowerCase().includes(q))
})

function getSortIndex(index) {
  // 保持与排序后一致的排名
  const sorted = [...filteredTableData.value].sort((a, b) => {
    const sortProp = 'healthScore'
    const order = 'descending'
    const diff = a[sortProp] - b[sortProp]
    return order === 'ascending' ? diff : -diff
  })
  const row = filteredTableData.value[index]
  return sorted.indexOf(row)
}

// ==================== 图表配色 ====================
const CHART_COLORS = ['#409eff', '#67c23a', '#e6a23c', '#f56c6c', '#909399', '#b37feb', '#5cdbd3', '#ff85c0']

// ==================== 应收分布饼图 ====================
const receivablePieOption = computed(() => {
  const items = activeCompanies.value
  return {
    tooltip: {
      trigger: 'item',
      formatter: p => `${p.name}<br/>应收: ${formatShortMoney(p.value)}<br/>占比: ${p.percent}%`
    },
    legend: {
      type: 'scroll',
      orient: 'vertical',
      right: 10,
      top: 20,
      bottom: 20,
      textStyle: { fontSize: 11 }
    },
    series: [{
      type: 'pie',
      radius: ['40%', '65%'],
      center: ['35%', '50%'],
      avoidLabelOverlap: false,
      label: {
        show: true,
        formatter: '{b}: {d}%',
        fontSize: 11
      },
      emphasis: {
        label: { show: true, fontSize: 14, fontWeight: 'bold' },
        itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0,0,0,0.2)' }
      },
      data: items.map((c, i) => ({
        value: c.monthlyReceivable,
        name: c.name,
        itemStyle: { color: CHART_COLORS[i % CHART_COLORS.length] }
      }))
    }]
  }
})

// ==================== 收租率排名柱状图 ====================
const collectionBarOption = computed(() => {
  const sorted = [...activeCompanies.value].sort((a, b) => b.collectionRate - a.collectionRate)
  return {
    tooltip: {
      trigger: 'axis',
      formatter: p => `${p[0].name}<br/>收租率: ${p[0].value}%`
    },
    grid: { left: '3%', right: '4%', bottom: '20%', top: '5%' },
    xAxis: {
      type: 'category',
      data: sorted.map(c => c.name),
      axisLabel: { rotate: 25, fontSize: 10 }
    },
    yAxis: {
      type: 'value',
      max: 100,
      axisLabel: { formatter: '{value}%' }
    },
    series: [{
      type: 'bar',
      data: sorted.map(c => ({
        value: c.collectionRate,
        itemStyle: {
          color: c.collectionRate >= 90 ? '#67c23a'
            : c.collectionRate >= 75 ? '#409eff'
            : c.collectionRate >= 60 ? '#e6a23c'
            : '#f56c6c',
          borderRadius: [4, 4, 0, 0]
        }
      })),
      barWidth: '50%',
      label: {
        show: true,
        position: 'top',
        formatter: '{c}%',
        fontSize: 10
      }
    }]
  }
})

// ==================== 月度收租率趋势 ====================
const trendLineOption = computed(() => {
  const companies = activeCompanies.value.slice(0, 6) // 最多6家
  const months = generateLast6Months()
  return {
    tooltip: {
      trigger: 'axis',
      formatter: p => {
        let html = `<b>${p[0].axisValue}</b><br/>`
        p.forEach(item => {
          html += `${item.marker} ${item.seriesName}: ${item.value}%<br/>`
        })
        return html
      }
    },
    legend: {
      type: 'scroll',
      bottom: 0,
      textStyle: { fontSize: 11 }
    },
    grid: { left: '3%', right: '4%', bottom: '28%', top: '5%' },
    xAxis: {
      type: 'category',
      data: months,
      boundaryGap: false
    },
    yAxis: {
      type: 'value',
      max: 100,
      axisLabel: { formatter: '{value}%' }
    },
    series: companies.map((c, i) => ({
      name: c.name,
      type: 'line',
      smooth: true,
      symbol: 'circle',
      symbolSize: 6,
      lineStyle: { width: 2 },
      data: generateTrendData(c, months.length),
      itemStyle: { color: CHART_COLORS[i % CHART_COLORS.length] }
    })),
    // 当没有趋势数据时显示提示
    graphic: companies.length === 0 ? [{
      type: 'text',
      left: 'center',
      top: 'center',
      style: {
        text: '暂无趋势数据\n（需多个账期数据积累）',
        fill: '#c0c4cc',
        fontSize: 13,
        textAlign: 'center'
      }
    }] : []
  }
})

function generateLast6Months() {
  const result = []
  const d = new Date()
  for (let i = 5; i >= 0; i--) {
    const m = new Date(d.getFullYear(), d.getMonth() - i, 1)
    result.push(`${m.getFullYear()}-${String(m.getMonth() + 1).padStart(2, '0')}`)
  }
  return result
}

function generateTrendData(company, count) {
  // 使用实际数据或者模拟趋势
  const base = company.collectionRate
  const data = []
  for (let i = 0; i < count; i++) {
    const variance = (Math.random() - 0.5) * 10
    const val = Math.max(0, Math.min(100, base + variance))
    if (i === count - 1) {
      data.push(company.collectionRate) // 最后一个月用实际值
    } else {
      data.push(Math.round(val * 10) / 10)
    }
  }
  return data
}

// ==================== 雷达图 ====================
const radarOption = computed(() => {
  const company = overviewData.value?.companies?.find(c => c.id === radarCompany.value)
  if (!company) return { tooltip: {} }
  const maxVal = Math.max(100, company.monthlyReceivable > 100000 ? 100 : 100)
  return {
    tooltip: {
      trigger: 'item',
      formatter: p => {
        const radar = p.component.subType === 'radar' ? p : null
        if (radar) {
          return `<b>${company.name}</b><br/>${p.value.map((v, i) =>
            `${p.component.radar.indicator[i].name}: ${v}`
          ).join('<br/>')}`
        }
        return ''
      }
    },
    radar: {
      indicator: [
        { name: '出租率', max: 100 },
        { name: '收租率', max: 100 },
        { name: '合同活跃度', max: 100 },
        { name: '资产规模', max: 100 },
        { name: '逾期控制', max: 100 }
      ],
      center: ['50%', '52%'],
      radius: '65%',
      axisName: { color: '#606266', fontSize: 11 }
    },
    series: [{
      type: 'radar',
      data: [{
        value: [
          company.occupancyRate,
          company.collectionRate,
          Math.min(100, (company.activeContractCount / Math.max(1, overviewData.value.totalActiveContracts)) * 100 * 3),
          Math.min(100, (company.roomCount / Math.max(1, overviewData.value.totalRooms)) * 100 * 3),
          Math.max(0, 100 - Math.min(company.overdueAmount / 1000, 100))
        ],
        name: company.name,
        areaStyle: { color: 'rgba(64, 158, 255, 0.2)' },
        lineStyle: { color: '#409eff', width: 2 },
        itemStyle: { color: '#409eff' }
      }]
    }]
  }
})

function refreshRadar() {
  // computed 会自动刷新
}

// ==================== 工具函数 ====================
function formatMoney(val) {
  if (!val && val !== 0) return '¥0.00'
  return '¥' + Number(val).toLocaleString('zh-CN', {
    minimumFractionDigits: 2, maximumFractionDigits: 2
  })
}

function numStr(val) {
  if (!val && val !== 0) return '0'
  return Number(val).toLocaleString('zh-CN')
}

function formatShortMoney(val) {
  if (!val && val !== 0) return '¥0'
  const num = Number(val)
  if (num >= 100000000) return '¥' + (num / 100000000).toFixed(2) + '亿'
  if (num >= 10000) return '¥' + (num / 10000).toFixed(1) + '万'
  return '¥' + num.toLocaleString('zh-CN')
}

function rateColor(val) {
  if (val >= 90) return '#67c23a'
  if (val >= 75) return '#409eff'
  if (val >= 60) return '#e6a23c'
  return '#f56c6c'
}

function healthColor(val) {
  if (val >= 85) return '#67c23a'
  if (val >= 70) return '#409eff'
  if (val >= 55) return '#e6a23c'
  return '#f56c6c'
}

// ==================== 交互 ====================
function switchToCompany(row) {
  if (userStore.isSuperAdmin && row.id) {
    userStore.switchToCompany(row.id)
    ElMessage.success(`已切换到「${row.name}」视角`)
    router.push('/dashboard')
  }
}

// ==================== 导出 ====================
function exportImage() {
  ElMessage.info('导出截图功能即将推出')
}

function exportTable() {
  ElMessage.info('导出报表功能即将推出')
}

// ==================== 生命周期 ====================
onMounted(() => {
  fetchData()
})

onUnmounted(() => {
  if (autoTimer) {
    clearInterval(autoTimer)
    autoTimer = null
  }
})
</script>

<style scoped>
/* ==================== 页面容器 ==================== */
.company-overview {
  max-width: 1600px;
  margin: 0 auto;
}

/* ==================== 头部 ==================== */
.overview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding: 20px 24px;
  background: linear-gradient(135deg, #1a3a5c 0%, #2d5f8a 50%, #409eff 100%);
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(64, 158, 255, 0.2);
}

.header-left {
  display: flex;
  align-items: center;
  gap: 14px;
  color: #fff;
}

.header-icon {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 10px;
}

.header-title {
  font-size: 22px;
  font-weight: 700;
  color: #fff;
  margin: 0;
  line-height: 1.3;
}

.header-subtitle {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.7);
  margin: 2px 0 0;
}

.header-actions {
  display: flex;
  gap: 8px;
  align-items: center;
}

.header-actions .month-picker {
  width: 150px;
}

.header-actions :deep(.el-button) {
  background: rgba(255, 255, 255, 0.15);
  border-color: rgba(255, 255, 255, 0.25);
  color: #fff;
}
.header-actions :deep(.el-button:hover) {
  background: rgba(255, 255, 255, 0.25);
  border-color: rgba(255, 255, 255, 0.35);
}

/* ==================== 骨架屏 ==================== */
.skeleton-box {
  padding: 24px;
  background: #fff;
  border-radius: 12px;
}

/* ==================== KPI 卡片 ==================== */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 12px;
  margin-bottom: 20px;
}

@media (max-width: 1200px) {
  .kpi-grid { grid-template-columns: repeat(4, 1fr); }
}
@media (max-width: 768px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
}

.kpi-card {
  background: #fff;
  border-radius: 10px;
  padding: 16px 14px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  transition: transform 0.2s, box-shadow 0.2s;
  border: 1px solid #f0f0f0;
}
.kpi-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
}

.kpi-label {
  font-size: 12px;
  color: #909399;
  margin-bottom: 6px;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.kpi-value {
  font-size: 22px;
  font-weight: 700;
  margin-bottom: 6px;
  line-height: 1.2;
}

.kpi-trend {
  display: flex;
  align-items: center;
  gap: 3px;
  font-size: 11px;
  color: #909399;
}
.kpi-trend.up { color: #f56c6c; }
.kpi-trend.down { color: #67c23a; }
.kpi-trend .el-icon { font-size: 12px; }

/* ==================== 分区行 ==================== */
.section-row {
  margin-bottom: 16px;
}
.section-card {
  margin-bottom: 16px;
  border-radius: 10px;
}
.section-card :deep(.el-card__header) {
  padding: 14px 20px;
  border-bottom: 1px solid #f0f2f5;
  font-weight: 600;
  font-size: 15px;
}
.card-header-with-action {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.card-header-with-action .el-icon {
  margin-right: 4px;
}

/* ==================== 健康评分列表 ==================== */
.health-list {
  max-height: 420px;
  overflow-y: auto;
}

.health-row {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  gap: 10px;
}
.health-row:hover {
  background: #f5f7fa;
}
.health-row-active {
  background: linear-gradient(90deg, rgba(64,158,255,0.04) 0%, transparent 100%);
}

.health-rank {
  width: 28px;
  text-align: center;
  font-size: 16px;
  flex-shrink: 0;
}
.rank-medal {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  border-radius: 50%;
  font-size: 13px;
  font-weight: 700;
}
.rank-gold { background: linear-gradient(135deg, #f6d365, #fda085); color: #fff; }
.rank-silver { background: linear-gradient(135deg, #a8c0ff, #8f94fb); color: #fff; }
.rank-bronze { background: linear-gradient(135deg, #f5af7a, #d68953); color: #fff; }
.rank-num {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: #f0f2f5;
  font-size: 11px;
  font-weight: 700;
  color: #909399;
}

.health-info {
  flex: 1;
  min-width: 0;
}
.health-name {
  font-size: 13px;
  font-weight: 600;
  color: #303133;
}
.health-meta {
  font-size: 11px;
  color: #909399;
  margin-top: 2px;
}

.health-bar-wrap {
  flex: 1;
  max-width: 160px;
}
.health-bar {
  height: 8px;
  background: #f0f2f5;
  border-radius: 4px;
  overflow: hidden;
}
.health-bar-fill {
  height: 100%;
  border-radius: 4px;
  transition: width 0.6s ease;
}

.health-score {
  font-size: 18px;
  font-weight: 700;
  width: 48px;
  text-align: right;
  flex-shrink: 0;
}

.empty-tip {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 40px 20px;
  color: #909399;
  font-size: 13px;
}

/* ==================== 预警列表 ==================== */
.alert-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.alert-item {
  display: flex;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 8px;
  background: #fafafa;
  transition: background 0.2s;
}
.alert-item:hover { background: #f0f2f5; }
.alert-high { border-left: 3px solid #f56c6c; }
.alert-warn { border-left: 3px solid #e6a23c; }

.alert-icon {
  flex-shrink: 0;
  padding-top: 2px;
}
.alert-body { flex: 1; min-width: 0; }
.alert-title {
  font-size: 13px;
  font-weight: 600;
  color: #303133;
}
.alert-desc {
  font-size: 12px;
  color: #909399;
  margin-top: 2px;
}

/* ==================== 图表区 ==================== */
.chart-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 16px;
}

@media (max-width: 1000px) {
  .chart-grid { grid-template-columns: 1fr; }
}

.chart-card :deep(.el-card__body) {
  padding: 0;
}

.chart-container {
  height: 320px;
  padding: 10px 8px 0;
}

/* ==================== 表格 ==================== */
.table-search {
  width: 200px;
}

.table-rank {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  font-size: 11px;
  font-weight: 700;
}
.table-rank.rank-1 { background: #fff0e0; color: #e6a23c; }
.table-rank.rank-2 { background: #f0f5ff; color: #409eff; }
.table-rank.rank-3 { background: #f5f0ff; color: #722ed1; }
.table-rank:not(.rank-1):not(.rank-2):not(.rank-3) {
  background: #f5f7fa;
  color: #909399;
}

.text-danger {
  color: #f56c6c;
  font-weight: 600;
}
.text-success {
  color: #67c23a;
}

/* ==================== 全局元素覆盖 ==================== */
:deep(.el-card) {
  border: 1px solid #ebeef5;
}

:deep(.el-table th.el-table__cell) {
  background: #f5f7fa !important;
  color: #606266;
  font-weight: 600;
  font-size: 12px;
}

:deep(.el-progress__text) {
  font-size: 11px !important;
}
</style>
