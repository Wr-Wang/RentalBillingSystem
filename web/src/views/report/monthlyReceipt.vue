<template>
  <div>
    <div class="page-header">
      <h2>收款月报</h2>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="searchMonth" type="month" placeholder="选择月份" />
      <el-button type="primary">查询</el-button>
      <el-button><el-icon><Download /></el-icon>导出</el-button>
    </div>

    <el-row :gutter="16" style="margin-bottom: 16px;">
      <el-col :span="8">
        <el-card>
          <div class="label">本月收款</div>
          <div style="font-size: 28px; font-weight: 600; color: #67c23a;">¥1,058,000</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="label">日均收款</div>
          <div style="font-size: 28px; font-weight: 600; color: #409eff;">¥40,692</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="label">环比上月</div>
          <div style="font-size: 28px; font-weight: 600; color: #67c23a;">+5.2%</div>
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
import { ref, computed } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart } from 'echarts/charts'
import { TitleComponent, TooltipComponent, GridComponent } from 'echarts/components'

use([CanvasRenderer, BarChart, TitleComponent, TooltipComponent, GridComponent])

const searchMonth = ref(new Date())

const chartOption = computed(() => ({
  tooltip: { trigger: 'axis' },
  xAxis: { type: 'category', data: Array.from({length: 30}, (_, i) => (i + 1) + '日') },
  yAxis: { type: 'value', axisLabel: { formatter: '¥{value}' } },
  series: [{
    type: 'bar',
    data: [35000, 42000, 28000, 56000, 48000, 72000, 85600, 45000, 32000, 58000, 41000, 39000, 62000, 51000, 47000, 69000, 38000, 44000, 53000, 67000, 35000, 42000, 28000, 56000, 48000, 72000, 85600, 45000, 32000, 38000],
    itemStyle: { color: '#409eff', borderRadius: [4, 4, 0, 0] }
  }],
  grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true }
}))
</script>
