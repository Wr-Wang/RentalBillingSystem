<template>
  <div>
    <div class="page-header">
      <h2>仪表盘</h2>
      <el-date-picker v-model="currentDate" type="month" placeholder="选择月份" />
    </div>

    <!-- Stat Cards -->
    <div class="stat-cards">
      <div class="stat-card" style="border-left: 4px solid #409eff;">
        <div class="label">今日收款</div>
        <div class="value" style="color: #409eff;">¥ {{ formatMoney(todayStats.received) }}</div>
        <div class="sub">笔数: {{ todayStats.count }} 笔</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #67c23a;">
        <div class="label">本月应收</div>
        <div class="value" style="color: #67c23a;">¥ {{ formatMoney(monthStats.receivable) }}</div>
        <div class="sub">已收: ¥ {{ formatMoney(monthStats.received) }}</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #e6a23c;">
        <div class="label">本月欠费</div>
        <div class="value" style="color: #e6a23c;">¥ {{ formatMoney(monthStats.overdue) }}</div>
        <div class="sub">收租率: {{ monthStats.collectionRate }}%</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #f56c6c;">
        <div class="label">逾期合同</div>
        <div class="value" style="color: #f56c6c;">{{ overdueContracts }}</div>
        <div class="sub">待催缴: {{ pendingCollection }} 户</div>
      </div>
    </div>

    <el-row :gutter="16">
      <!-- Collection Rate Chart -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <span>收租率</span>
          </template>
          <div style="height: 300px;">
            <v-chart :option="collectionRateOption" autoresize />
          </div>
        </el-card>
      </el-col>

      <!-- Recent 7 Days Trend -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <span>近7日收款趋势</span>
          </template>
          <div style="height: 300px;">
            <v-chart :option="trendOption" autoresize />
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-top: 16px;">
      <!-- Todo List -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <span>待办事项</span>
          </template>
          <el-table :data="todoList" style="width: 100%">
            <el-table-column prop="type" label="类型" width="100">
              <template #default="{ row }">
                <el-tag :type="row.tagType" size="small">{{ row.type }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="content" label="内容" />
            <el-table-column prop="date" label="日期" width="120" />
            <el-table-column label="操作" width="80">
              <template #default>
                <el-button text size="small" type="primary">去处理</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>

      <!-- Recent Receipts -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <span>最近收款</span>
          </template>
          <el-table :data="recentReceipts" style="width: 100%">
            <el-table-column prop="receiptNo" label="收据号" width="140" />
            <el-table-column prop="contractNo" label="合同号" width="120" />
            <el-table-column prop="amount" label="金额" width="100">
              <template #default="{ row }">¥{{ formatMoney(row.amount) }}</template>
            </el-table-column>
            <el-table-column prop="status" label="状态" width="90">
              <template #default="{ row }">
                <el-tag :type="row.status === '已确认' ? 'success' : 'warning'" size="small">{{ row.status }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="receivedDate" label="收款日期" width="100" />
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { PieChart, BarChart, LineChart } from 'echarts/charts'
import { TitleComponent, TooltipComponent, LegendComponent, GridComponent } from 'echarts/components'

use([
  CanvasRenderer, PieChart, BarChart, LineChart,
  TitleComponent, TooltipComponent, LegendComponent, GridComponent
])

const currentDate = ref(new Date())

function formatMoney(val) {
  return (val || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

// Mock data
const todayStats = { received: 85600, count: 12 }
const monthStats = { receivable: 1280000, received: 1058000, overdue: 222000, collectionRate: 82.7 }
const overdueContracts = 23
const pendingCollection = 15

const todoList = [
  { type: '收款确认', tagType: 'warning', content: 'B栋-301 房租收款 ¥5,000 待确认', date: '2026-06-27' },
  { type: '审批', tagType: 'primary', content: 'C栋-502 提前解约申请（押金¥8,000）', date: '2026-06-26' },
  { type: '催缴', tagType: 'danger', content: 'A栋-203 逾期15天 欠费¥12,500', date: '2026-06-25' },
  { type: '抄表', tagType: 'info', content: 'B栋 6月水电气抄表待录入（12户）', date: '2026-06-24' }
]

const recentReceipts = [
  { receiptNo: 'SJ-20260627-001', contractNo: 'HT-2026-012', amount: 5600, status: '已确认', receivedDate: '2026-06-27' },
  { receiptNo: 'SJ-20260626-003', contractNo: 'HT-2026-008', amount: 8200, status: '已确认', receivedDate: '2026-06-26' },
  { receiptNo: 'SJ-20260626-002', contractNo: 'HT-2026-015', amount: 4300, status: '待确认', receivedDate: '2026-06-26' },
  { receiptNo: 'SJ-20260625-005', contractNo: 'HT-2026-003', amount: 12000, status: '已确认', receivedDate: '2026-06-25' }
]

// Collection Rate Pie Chart
const collectionRateOption = computed(() => ({
  tooltip: { trigger: 'item' },
  legend: { bottom: '0%' },
  series: [
    {
      type: 'pie',
      radius: ['40%', '70%'],
      center: ['50%', '45%'],
      avoidLabelOverlap: false,
      itemStyle: { borderRadius: 10, borderColor: '#fff', borderWidth: 2 },
      label: { show: true, formatter: '{b}: {d}%' },
      emphasis: { label: { show: true, fontSize: 16, fontWeight: 'bold' } },
      data: [
        { value: monthStats.received, name: '已收款', itemStyle: { color: '#67c23a' } },
        { value: monthStats.overdue, name: '欠费', itemStyle: { color: '#e6a23c' } }
      ]
    }
  ]
}))

// 7-day Trend
const trendOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  xAxis: { type: 'category', data: ['06-21', '06-22', '06-23', '06-24', '06-25', '06-26', '06-27'] },
  yAxis: { type: 'value', axisLabel: { formatter: '¥{value}' } },
  series: [
    {
      name: '收款金额',
      type: 'bar',
      data: [35000, 42000, 28000, 56000, 48000, 72000, 85600],
      itemStyle: { color: '#409eff', borderRadius: [4, 4, 0, 0] }
    },
    {
      name: '笔数',
      type: 'line',
      data: [5, 8, 4, 9, 7, 11, 12],
      yAxisIndex: 0,
      itemStyle: { color: '#67c23a' }
    }
  ],
  grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true }
}))
</script>
