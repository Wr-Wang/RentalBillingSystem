<template>
  <div>
    <div class="page-header">
      <h2>多公司总览</h2>
      <div class="page-actions">
        <el-date-picker
          v-model="queryMonth"
          type="month"
          placeholder="选择月份"
          value-format="YYYY-MM"
          @change="fetchData"
          style="width:160px"
        />
        <el-button @click="fetchData">
          <el-icon><Refresh /></el-icon>刷新
        </el-button>
      </div>
    </div>

    <!-- 顶部概览卡片 -->
    <el-row :gutter="16" class="stat-cards">
      <el-col :span="4">
        <el-card shadow="never">
          <div class="stat-item">
            <div class="stat-label">公司总数</div>
            <div class="stat-value">{{ summary.companyTotal }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card shadow="never">
          <div class="stat-item">
            <div class="stat-label">总楼栋数</div>
            <div class="stat-value">{{ summary.buildingTotal }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card shadow="never">
          <div class="stat-item">
            <div class="stat-label">总房间数</div>
            <div class="stat-value">{{ summary.roomTotal }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card shadow="never">
          <div class="stat-item">
            <div class="stat-label">综合出租率</div>
            <div class="stat-value highlight">{{ summary.avgOccupancyRate }}%</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card shadow="never">
          <div class="stat-item">
            <div class="stat-label">本月应收</div>
            <div class="stat-value money">{{ formatMoney(summary.monthlyReceivable) }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card shadow="never" class="card-collection">
          <div class="stat-item">
            <div class="stat-label">综合收租率</div>
            <div class="stat-value highlight">{{ summary.avgCollectionRate }}%</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 各公司对比表格 -->
    <el-card shadow="never" class="section-card">
      <template #header>
        <span>各公司经营指标对比</span>
      </template>
      <el-table :data="companyStats" stripe @row-click="handleRowClick" style="cursor:pointer">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="name" label="公司名称" min-width="140">
          <template #default="{ row }">
            <el-button text type="primary" @click="switchToCompany(row)">{{ row.name }}</el-button>
          </template>
        </el-table-column>
        <el-table-column prop="buildingCount" label="楼栋数" width="80" align="center" />
        <el-table-column prop="roomCount" label="房间数" width="80" align="center" />
        <el-table-column prop="rentedCount" label="在租" width="70" align="center" />
        <el-table-column prop="occupancyRate" label="出租率" width="100" align="center">
          <template #default="{ row }">
            <el-progress :percentage="row.occupancyRate || 0" :stroke-width="14" />
          </template>
        </el-table-column>
        <el-table-column prop="monthlyReceivable" label="本月应收" width="130" align="right">
          <template #default="{ row }">{{ formatMoney(row.monthlyReceivable) }}</template>
        </el-table-column>
        <el-table-column prop="monthlyReceived" label="本月实收" width="130" align="right">
          <template #default="{ row }">{{ formatMoney(row.monthlyReceived) }}</template>
        </el-table-column>
        <el-table-column prop="collectionRate" label="收租率" width="120" align="center">
          <template #default="{ row }">
            <el-progress :percentage="row.collectionRate || 0" :stroke-width="14"
              :color="row.collectionRate >= 90 ? '#67c23a' : row.collectionRate >= 70 ? '#e6a23c' : '#f56c6c'" />
          </template>
        </el-table-column>
        <el-table-column prop="overdueAmount" label="逾期金额" width="120" align="right">
          <template #default="{ row }">
            <span class="overdue">{{ formatMoney(row.overdueAmount) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="isActive" label="状态" width="70" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">
              {{ row.isActive ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 图表区域 -->
    <el-row :gutter="16">
      <el-col :span="12">
        <el-card shadow="never" class="section-card">
          <template #header>
            <span>收租率对比</span>
          </template>
          <div style="height:320px">
            <v-chart :option="collectionRateChart" autoresize />
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card shadow="never" class="section-card">
          <template #header>
            <span>欠费排行 Top 10</span>
          </template>
          <div style="height:320px">
            <v-chart :option="overdueChart" autoresize />
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-top:16px">
      <el-col :span="12">
        <el-card shadow="never" class="section-card">
          <template #header>
            <span>出租率对比</span>
          </template>
          <div style="height:320px">
            <v-chart :option="occupancyChart" autoresize />
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card shadow="never" class="section-card">
          <template #header>
            <span>月度收租率趋势</span>
          </template>
          <div style="height:320px">
            <v-chart :option="trendChart" autoresize />
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../../store/user'
import { ElMessage } from 'element-plus'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { BarChart, LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { getCompanies, getCompanyStats } from '../../api/index'

use([BarChart, LineChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer])

const router = useRouter()
const userStore = useUserStore()
const queryMonth = ref('2026-06')

const summary = ref({
  companyTotal: 0,
  buildingTotal: 0,
  roomTotal: 0,
  avgOccupancyRate: 0,
  monthlyReceivable: 0,
  avgCollectionRate: 0
})

const companyStats = ref([])

function formatMoney(val) {
  if (!val && val !== 0) return '¥0.00'
  return '¥' + Number(val).toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}


async function fetchData() {
  try {
    const res = await getCompanies({ pageSize: 100, isActive: undefined })
    const companies = res.items || res.data || []
    let totalBuildingCount = 0
    let totalRoomCount = 0
    let totalOccupancyRate = 0
    let activeCount = 0
    const statsList = []

    for (const c of companies) {
      let stats = { buildingCount: 0, roomCount: 0, occupancyRate: 0, collectionRate: 0 }
      try {
        stats = await getCompanyStats(c.id)
      } catch { /* use defaults */ }

      const entry = {
        id: c.id,
        name: c.name,
        buildingCount: stats.buildingCount || c.buildingCount || 0,
        roomCount: stats.roomCount || c.roomCount || 0,
        rentedCount: Math.round((stats.roomCount || 0) * (stats.occupancyRate || 0) / 100),
        occupancyRate: stats.occupancyRate || 0,
        monthlyReceivable: 0,
        monthlyReceived: 0,
        collectionRate: 0,
        overdueAmount: 0,
        isActive: c.isActive !== false
      }
      statsList.push(entry)

      if (entry.isActive) {
        totalBuildingCount += entry.buildingCount
        totalRoomCount += entry.roomCount
        totalOccupancyRate += entry.occupancyRate
        activeCount++
      }
    }

    companyStats.value = statsList
    summary.value = {
      companyTotal: companies.length,
      buildingTotal: totalBuildingCount,
      roomTotal: totalRoomCount,
      avgOccupancyRate: activeCount > 0 ? Math.round(totalOccupancyRate / activeCount) : 0,
      monthlyReceivable: 0,
      avgCollectionRate: 0
    }
  } catch {
    ElMessage.error('加载公司总览数据失败')
  }
}

// 图表配置
const collectionRateChart = computed(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true },
  xAxis: {
    type: 'category',
    data: companyStats.value.filter(l => l.isActive).map(l => l.name),
    axisLabel: { rotate: 30 }
  },
  yAxis: { type: 'value', max: 100, axisLabel: { formatter: '{value}%' } },
  series: [{
    type: 'bar',
    data: companyStats.value.filter(l => l.isActive).map(l => ({
      value: l.collectionRate,
      itemStyle: {
        color: l.collectionRate >= 90 ? '#67c23a' : l.collectionRate >= 70 ? '#e6a23c' : '#f56c6c'
      }
    })),
    barWidth: '40%',
    label: { show: true, position: 'top', formatter: '{c}%' }
  }]
}))

const overdueChart = computed(() => {
  const sorted = [...companyStats.value].sort((a, b) => b.overdueAmount - a.overdueAmount)
  return {
    tooltip: { trigger: 'axis', formatter: params => `${params[0].name}<br/>逾期金额: ¥${Number(params[0].value).toLocaleString()}` },
    grid: { left: '10%', right: '4%', bottom: '15%', containLabel: true },
    xAxis: {
      type: 'category',
      data: sorted.map(l => l.name),
      axisLabel: { rotate: 30 }
    },
    yAxis: { type: 'value', axisLabel: { formatter: v => '¥' + (v / 10000).toFixed(1) + '万' } },
    series: [{
      type: 'bar',
      data: sorted.map(l => ({
        value: l.overdueAmount,
        itemStyle: { color: '#f56c6c' }
      })),
      barWidth: '40%',
      label: { show: true, position: 'top', formatter: p => '¥' + (p.value / 10000).toFixed(1) + '万' }
    }]
  }
})

const occupancyChart = computed(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: '3%', right: '4%', bottom: '15%', containLabel: true },
  xAxis: {
    type: 'category',
    data: companyStats.value.filter(l => l.isActive).map(l => l.name),
    axisLabel: { rotate: 30 }
  },
  yAxis: { type: 'value', max: 100, axisLabel: { formatter: '{value}%' } },
  series: [{
    type: 'bar',
    data: companyStats.value.filter(l => l.isActive).map(l => ({
      value: l.occupancyRate,
      itemStyle: { color: '#409eff' }
    })),
    barWidth: '40%',
    label: { show: true, position: 'top', formatter: '{c}%' }
  }]
}))

const trendChart = computed(() => ({
  tooltip: { trigger: 'axis' },
  legend: { data: ['公司A', '公司B', '公司C'], bottom: 0 },
  grid: { left: '3%', right: '4%', bottom: '25%', containLabel: true },
  xAxis: {
    type: 'category',
    data: ['1月', '2月', '3月', '4月', '5月', '6月']
  },
  yAxis: { type: 'value', max: 100, axisLabel: { formatter: '{value}%' } },
  series: [
    { name: '张建国', type: 'line', data: [88, 90, 87, 92, 89, 92], smooth: true },
    { name: '李春华', type: 'line', data: [82, 85, 83, 86, 84, 88], smooth: true },
    { name: '王芳投资有限公司', type: 'line', data: [93, 94, 92, 95, 94, 95], smooth: true }
  ]
}))

function handleRowClick(row) {
  switchToCompany(row)
}

function switchToCompany(row) {
  if (userStore.isSuperAdmin) {
    userStore.switchToCompany(row.id)
    ElMessage.success(`已切换到「${row.name}」视角`)
    router.push('/dashboard')
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.page-actions {
  display: flex;
  gap: 8px;
  align-items: center;
}
.stat-cards {
  margin-bottom: 16px;
}
.stat-item {
  text-align: center;
  padding: 8px 0;
}
.stat-label {
  font-size: 13px;
  color: #909399;
  margin-bottom: 8px;
}
.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #303133;
}
.stat-value.highlight {
  color: #409eff;
}
.stat-value.money {
  color: #e6a23c;
  font-size: 20px;
}
.section-card {
  margin-bottom: 16px;
}
.overdue {
  color: #f56c6c;
  font-weight: 600;
}
</style>
