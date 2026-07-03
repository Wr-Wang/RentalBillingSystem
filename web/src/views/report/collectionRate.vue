<template>
  <div>
    <div class="page-header">
      <h2>收租率统计</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="searchMonth" type="month" placeholder="选择月份" @change="loadData" />
      <el-button type="primary" @click="loadData">查询</el-button>
    </div>

    <div class="stat-cards">
      <div class="stat-card" style="border-left: 4px solid #409eff;">
        <div class="label">应收总额</div>
        <div class="value" style="color: #409eff;">¥{{ formatMoney(stats.totalReceivable) }}</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #67c23a;">
        <div class="label">实收总额</div>
        <div class="value" style="color: #67c23a;">¥{{ formatMoney(stats.totalReceived) }}</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #e6a23c;">
        <div class="label">欠费总额</div>
        <div class="value" style="color: #e6a23c;">¥{{ formatMoney(stats.totalOverdue) }}</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #409eff;">
        <div class="label">收租率</div>
        <div class="value" style="color: #409eff;">{{ stats.rate }}%</div>
      </div>
    </div>

    <el-card>
      <template #header>收租率详情（按账期）</template>
      <el-table :data="details" v-loading="loading" stripe>
        <el-table-column prop="period" label="账期" width="100" />
        <el-table-column prop="totalPlans" label="应收笔数" width="90" />
        <el-table-column label="应收金额">
          <template #default="{ row }">¥{{ (row.receivable || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="实收金额">
          <template #default="{ row }">¥{{ (row.received || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="欠费金额">
          <template #default="{ row }">¥{{ (row.overdue || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="收租率" width="180">
          <template #default="{ row }">
            <el-progress :percentage="row.rate" :color="row.rate > 90 ? '#67c23a' : row.rate > 70 ? '#e6a23c' : '#f56c6c'" />
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getCollectionRate } from '@/api'

const searchMonth = ref(new Date())
const loading = ref(false)
const details = ref([])
const stats = reactive({ totalReceivable: 0, totalReceived: 0, totalOverdue: 0, rate: 0 })

function formatMoney(v) { return (v || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }

async function loadData() {
  loading.value = true
  try {
    const d = searchMonth.value || new Date()
    const period = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
    const res = await getCollectionRate({ period })
    const items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    details.value = items.map(item => ({
      period: item.period || '',
      totalPlans: item.totalPlans || 0,
      receivable: item.totalAmount || item.receivable || 0,
      received: item.totalReceived || item.received || 0,
      overdue: (item.totalAmount || 0) - (item.totalReceived || 0),
      rate: item.rate || 0
    }))
    const tr = details.value.reduce((s, r) => s + r.receivable, 0)
    const td = details.value.reduce((s, r) => s + r.received, 0)
    stats.totalReceivable = tr
    stats.totalReceived = td
    stats.totalOverdue = tr - td
    stats.rate = tr > 0 ? Math.round(td / tr * 100 * 10) / 10 : 0
  } catch {
    ElMessage.error('加载收租率数据失败')
  }
  loading.value = false
}

onMounted(loadData)
</script>

<style scoped>
.stat-cards {
  display: flex; gap: 16px; margin-bottom: 16px;
}
.stat-card {
  flex: 1; background: #fff; border-radius: 8px; padding: 20px; text-align: center; box-shadow: 0 1px 4px rgba(0,0,0,0.06);
}
.stat-card .label {
  font-size: 13px; color: #909399; margin-bottom: 8px;
}
.stat-card .value {
  font-size: 24px; font-weight: 700;
}
</style>
