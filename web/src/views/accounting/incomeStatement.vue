<template>
  <div>
    <div class="page-header"><h2>利润表</h2></div>
    <div class="search-bar">
      <el-date-picker v-model="periodMonth" type="month" placeholder="所属月份" value-format="YYYY-MM" style="width:160px;" />
      <el-button type="primary" @click="loadData">查询</el-button>
      <el-button @click="exportExcel" :disabled="revenue.length === 0 && expenses.length === 0">导出 Excel</el-button>
    </div>

    <div v-loading="loading" style="max-width:600px;">
      <h3 style="margin:16px 0;">一、营业收入</h3>
      <el-table :data="revenue" stripe show-summary :summary-method="revenueSummary">
        <el-table-column label="科目" width="200">
          <template #default="{ row }">{{ row.code }} {{ row.name }}</template>
        </el-table-column>
        <el-table-column label="金额" width="150" align="right">
          <template #default="{ row }">¥{{ row.amount.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</template>
        </el-table-column>
      </el-table>

      <h3 style="margin:16px 0 8px;">二、营业成本及费用</h3>
      <el-table :data="expenses" stripe show-summary :summary-method="expenseSummary">
        <el-table-column label="科目" width="200">
          <template #default="{ row }">{{ row.code }} {{ row.name }}</template>
        </el-table-column>
        <el-table-column label="金额" width="150" align="right">
          <template #default="{ row }">¥{{ row.amount.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</template>
        </el-table-column>
      </el-table>

      <div style="margin-top:20px;padding:16px;background:#f0f9eb;border-radius:4px;">
        <span style="font-weight:bold;">三、净利润</span>
        <span style="float:right;font-weight:bold;font-size:18px;color:#67c23a;">
          ¥{{ netIncome.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getIncomeStatement } from '@/api'
import { exportToExcel } from '@/utils/exportExcel'

const loading = ref(false)
const periodMonth = ref('')
const revenue = ref([])
const expenses = ref([])
const netIncome = ref(0)

async function loadData() {
  loading.value = true
  try {
    let params = {}
    if (periodMonth.value) {
      const [y, m] = periodMonth.value.split('-').map(Number)
      const lastDay = new Date(y, m, 0).getDate()
      params.startDate = `${periodMonth.value}-01`
      params.endDate = `${periodMonth.value}-${String(lastDay).padStart(2, '0')}`
    }
    const res = await getIncomeStatement(params)
    revenue.value = res.revenue || []
    expenses.value = res.expenses || []
    netIncome.value = res.netIncome || 0
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}

function revenueSummary() { return ['合计', `¥${revenue.value.reduce((s, r) => s + r.amount, 0).toFixed(2)}`] }
function expenseSummary() { return ['合计', `¥${expenses.value.reduce((s, r) => s + r.amount, 0).toFixed(2)}`] }

function exportExcel() {
  const revRows = revenue.value.map(r => [r.code, r.name, r.amount])
  const expRows = expenses.value.map(r => [r.code, r.name, r.amount])
  exportToExcel([
    { name: '收入', columns: ['科目编码', '科目名称', '金额'], rows: revRows },
    { name: '费用', columns: ['科目编码', '科目名称', '金额'], rows: expRows }
  ], '利润表')
}

onMounted(loadData)
</script>
<style scoped>
.search-bar { margin-bottom: 16px; }
</style>
