<template>
  <div>
    <div class="page-header">
      <h2>仪表盘</h2>
      <el-date-picker v-model="currentDate" type="month" placeholder="选择月份" @change="loadData" />
    </div>

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
      <el-col :span="12">
        <el-card>
          <template #header><span>收租率</span></template>
          <div style="height: 300px;">
            <v-chart :option="collectionRateOption" autoresize />
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header><span>近7日收款趋势</span></template>
          <div style="height: 300px;">
            <v-chart :option="trendOption" autoresize />
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-top: 16px;">
      <el-col :span="12">
        <el-card>
          <template #header><span>待办事项</span></template>
          <el-table :data="todoList" style="width: 100%" v-loading="todoLoading">
            <el-table-column prop="type" label="类型" width="100">
              <template #default="{ row }"><el-tag :type="row.tagType" size="small">{{ row.type }}</el-tag></template>
            </el-table-column>
            <el-table-column prop="content" label="内容" />
            <el-table-column prop="date" label="日期" width="120" />
            <el-table-column label="操作" width="80">
              <template #default="{ row }">
                <el-button text size="small" type="primary" @click="handleAction(row)">去处理</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header><span>最近收款</span></template>
          <el-table :data="recentReceipts" style="width: 100%" v-loading="receiptLoading">
            <el-table-column prop="receiptNo" label="收据号" width="140" />
            <el-table-column prop="contractNo" label="合同号" width="120" />
            <el-table-column prop="amount" label="金额" width="100">
              <template #default="{ row }">¥{{ formatMoney(row.amount) }}</template>
            </el-table-column>
            <el-table-column prop="status" label="状态" width="90">
              <template #default="{ row }">
                <el-tag :type="row.status === 'Confirmed' ? 'success' : 'warning'" size="small">{{ {Confirmed:'已确认',Pending:'待确认',Rejected:'已驳回'}[row.status] || row.status }}</el-tag>
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
import { ref, computed, onMounted } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { PieChart, BarChart, LineChart } from 'echarts/charts'
import { TitleComponent, TooltipComponent, LegendComponent, GridComponent } from 'echarts/components'
import { getReceipts, getContracts, getCollectionRate, getDailyReceipt, getMonthlyReceipt, getOverdueDetail } from '@/api'

use([CanvasRenderer, PieChart, BarChart, LineChart, TitleComponent, TooltipComponent, LegendComponent, GridComponent])

const currentDate = ref(new Date())
const todayStats = ref({ received: 0, count: 0 })
const monthStats = ref({ receivable: 0, received: 0, overdue: 0, collectionRate: 0 })
const overdueContracts = ref(0)
const pendingCollection = ref(0)
const recentReceipts = ref([])
const todoList = ref([])
const todoLoading = ref(false)
const receiptLoading = ref(false)

function formatMoney(val) { return (val || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }

async function loadData() {
  const p = currentDate.value ? `${currentDate.value.getFullYear()}-${String(currentDate.value.getMonth() + 1).padStart(2, '0')}` : undefined
  const today = new Date().toISOString().slice(0, 10)

  // 今日收款
  try { const dr = await getDailyReceipt({ date: today }); const details = dr.details || dr || []; const totalReceived = details.reduce((s, r) => s + (r.total || 0), 0); const totalCount = details.reduce((s, r) => s + (r.cnt || 0), 0); todayStats.value = { received: totalReceived, count: totalCount } } catch {}
  // 本月报
  try { const mr = await getMonthlyReceipt({ period: p }); const d = mr.plans || mr; const ta = d.totalAmount || 0; const tr = d.totalReceived || 0; monthStats.value = { receivable: ta, received: tr, overdue: ta - tr, collectionRate: ta > 0 ? ((tr / ta) * 100).toFixed(1) : 0 } } catch {}
  // 逾期明细 → 逾期合同数
  try { const od = await getOverdueDetail(); overdueContracts.value = od.length; pendingCollection.value = od.filter(p => p.daysOverdue > 7).length } catch {}
  // 最近收款
  receiptLoading.value = true
  try { const r = await getReceipts({}); recentReceipts.value = (r || []).slice(0, 5) } catch {}
  finally { receiptLoading.value = false }
  // 待办事项
  todoLoading.value = true
  try {
    const items = []
    const pendingReceipts = await getReceipts({ status: 'Pending' })
    if (pendingReceipts?.length > 0) items.push({ type: '收款确认', tagType: 'warning', content: `${pendingReceipts.length} 笔收款待确认`, date: today })
    const expiring = await getContracts({ pageSize: 1 })
    const allContracts = await getContracts({ pageSize: 100 })
    const actives = allContracts.items?.filter(c => c.status === 'Active') || []
    const expiringSoon = actives.filter(c => c.endDate && new Date(c.endDate) < new Date(Date.now() + 14 * 86400000))
    if (expiringSoon.length > 0) items.push({ type: '续签', tagType: 'primary', content: `${expiringSoon.length} 份合同即将到期`, date: today })
    const overduePlans = await getOverdueDetail()
    if (overduePlans.length > 0) items.push({ type: '催缴', tagType: 'danger', content: `${overduePlans.length} 户逾期欠费`, date: today })
    todoList.value = items.length > 0 ? items : [{ type: '提示', tagType: 'info', content: '暂无待办事项', date: today }]
  } catch {}
  finally { todoLoading.value = false }
}

function handleAction(row) {
  if (row.type === '收款确认') window.location.hash = '#/receipts/confirm'
  else if (row.type === '续签') window.location.hash = '#/renewal-dashboard'
  else if (row.type === '催缴') window.location.hash = '#/collection/overview'
}

const collectionRateOption = computed(() => ({
  tooltip: { trigger: 'item' },
  legend: { bottom: '0%' },
  series: [{ type: 'pie', radius: ['40%', '70%'], center: ['50%', '45%'], avoidLabelOverlap: false, itemStyle: { borderRadius: 10 }, label: { show: true, formatter: '{b}: {d}%' }, data: [{ value: monthStats.value.received, name: '已收款', itemStyle: { color: '#67c23a' } }, { value: Math.max(monthStats.value.overdue, 1), name: '欠费', itemStyle: { color: '#e6a23c' } }] }]
}))

const trendOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  xAxis: { type: 'category', data: trendLabels.value },
  yAxis: { type: 'value', axisLabel: { formatter: '¥{value}' } },
  series: [{ name: '收款金额', type: 'bar', data: trendData.value, itemStyle: { color: '#409eff', borderRadius: [4, 4, 0, 0] }, barMaxWidth: 30 }],
  grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true }
}))
const trendLabels = computed(() => { const d = []; for (let i = 6; i >= 0; i--) { const dt = new Date(); dt.setDate(dt.getDate() - i); d.push(`${String(dt.getMonth() + 1).padStart(2, '0')}-${String(dt.getDate()).padStart(2, '0')}`) }; return d })
const trendData = computed(() => [0, 0, 0, 0, 0, 0, 0])

onMounted(loadData)
</script>
