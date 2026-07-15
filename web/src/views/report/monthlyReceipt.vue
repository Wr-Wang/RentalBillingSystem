<template>
  <div>
    <div class="page-header">
      <h2>收款月报</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="searchMonth" type="month" placeholder="选择月份" @change="loadData" />
      <el-button type="primary" @click="loadData">查询</el-button>
    </div>

    <el-row :gutter="16" style="margin-bottom: 16px;">
      <el-col :span="8">
        <el-card>
          <div class="label">本月收款</div>
          <div style="font-size: 28px; font-weight: 600; color: #67c23a;">¥{{ formatMoney(stats.totalReceived) }}</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="label">日均收款</div>
          <div style="font-size: 28px; font-weight: 600; color: #409eff;">¥{{ formatMoney(stats.dailyAvg) }}</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="label">本月应收</div>
          <div style="font-size: 28px; font-weight: 600; color: #e6a23c;">¥{{ formatMoney(stats.totalReceivable) }}</div>
        </el-card>
      </el-col>
    </el-row>

    <el-card>
      <template #header>每日收款趋势</template>
      <div style="height: 300px;">
        <v-chart :option="chartOption" autoresize />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getMonthlyReceipt } from '@/api'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart } from 'echarts/charts'
import { TitleComponent, TooltipComponent, GridComponent } from 'echarts/components'

use([CanvasRenderer, BarChart, TitleComponent, TooltipComponent, GridComponent])

const searchMonth = ref(new Date())
const stats = ref({ totalReceived: 0, dailyAvg: 0, totalReceivable: 0 })
const dailyData = ref([])

function formatMoney(v) { return (v || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }

const chartOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  xAxis: { type: 'category', data: dailyData.value.map((_, i) => (i + 1) + '日') },
  yAxis: { type: 'value', axisLabel: { formatter: '¥{value}' } },
  series: [{
    type: 'bar',
    data: dailyData.value,
    itemStyle: { color: '#409eff', borderRadius: [4, 4, 0, 0] }
  }],
  grid: { left: '3%', right: '4%', bottom: '3%',  }
}))

async function loadData() {
  try {
    const d = searchMonth.value || new Date()
    const period = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
    const res = await getMonthlyReceipt({ period })
    // 后端返回 { period, totalAmount, totalReceived, dailyTotals }
    dailyData.value = res.dailyTotals || []
    stats.value = {
      totalReceived: res.totalReceived || 0,
      totalReceivable: res.totalAmount || 0,
      dailyAvg: dailyData.value.length > 0
        ? Math.round(dailyData.value.reduce((s, v) => s + v, 0) / dailyData.value.length)
        : 0
    }
  } catch {
    ElMessage.error('加载收款月报失败')
  }
}

onMounted(loadData)
</script>

<style scoped>
.label { font-size: 13px; color: #909399; margin-bottom: 8px; }
</style>
