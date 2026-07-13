<template>
  <div>
    <div class="page-header"><h2>试算平衡表</h2></div>
    <div class="search-bar">
      <el-date-picker v-model="periodMonth" type="month" placeholder="截止月份" value-format="YYYY-MM" style="width:160px;" />
      <el-date-picker v-model="endDate" type="date" placeholder="截止日期" style="width:200px;" />
      <el-checkbox v-model="hideZero" label="隐藏余额为零的科目" style="margin:0 12px;" />
      <el-button type="primary" @click="loadData">查询</el-button>
      <el-button @click="exportExcel" :disabled="items.length === 0">导出 Excel</el-button>
    </div>
    <el-table :data="displayItems" stripe v-loading="loading" show-summary :summary-method="summary" @row-click="handleRowClick" style="cursor:pointer;">
      <el-table-column label="科目编码" width="120">
        <template #default="{ row }">{{ row.code }}</template>
      </el-table-column>
      <el-table-column label="科目名称" width="200">
        <template #default="{ row }">{{ row.name }}</template>
      </el-table-column>
      <el-table-column label="方向" width="70">
        <template #default="{ row }">{{ (row.direction || row.Direction) === 'Debit' ? '借' : '贷' }}</template>
      </el-table-column>
      <el-table-column label="借方发生额" width="150" align="right">
        <template #default="{ row }">¥{{ (row.debitAmount || row.DebitAmount || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</template>
      </el-table-column>
      <el-table-column label="贷方发生额" width="150" align="right">
        <template #default="{ row }">¥{{ (row.creditAmount || row.CreditAmount || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</template>
      </el-table-column>
      <el-table-column label="余额" width="150" align="right">
        <template #default="{ row }">
          <span :style="{ color: (row.balance || row.Balance || 0) < 0 ? '#f56c6c' : '#67c23a' }">
            ¥{{ (row.balance || row.Balance || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}
          </span>
        </template>
      </el-table-column>
    </el-table>
    <div style="margin-top:8px;color:#999;font-size:13px;">
      提示：点击行可查看该科目明细（跳转日记账）
    </div>
  </div>
</template>
<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getTrialBalance } from '@/api'
import { exportToExcel } from '@/utils/exportExcel'
const router = useRouter()
const loading = ref(false)
const endDate = ref(new Date())
const periodMonth = ref('')
const hideZero = ref(true)
const items = ref([])
const totals = ref({ debit: 0, credit: 0 })

const displayItems = computed(() => {
  if (!hideZero.value) return items.value
  return items.value.filter(row => {
    const balance = row.balance || row.Balance || 0
    return balance !== 0
  })
})

async function loadData() {
  loading.value = true
  try {
    let date = null
    if (periodMonth.value) {
      // 取月份最后一天
      const [y, m] = periodMonth.value.split('-').map(Number)
      const lastDay = new Date(y, m, 0).getDate()
      date = `${periodMonth.value}-${String(lastDay).padStart(2, '0')}`
    } else if (endDate.value) {
      date = new Date(endDate.value).toISOString().slice(0, 10)
    }
    const res = await getTrialBalance({ endDate: date })
    items.value = res.subjects || []
    totals.value = { debit: res.totalDebit || 0, credit: res.totalCredit || 0 }
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}
function summary() { return [null, '合计', '', `¥${totals.value.debit.toFixed(2)}`, `¥${totals.value.credit.toFixed(2)}`, ''] }
function handleRowClick(row) {
  router.push({ name: 'AccountingJournal', query: { subjectCode: row.code } })
}
function exportExcel() {
  const rows = items.value.map(r => [
    r.code, r.name,
    (r.direction || r.Direction) === 'Debit' ? '借' : '贷',
    r.debitAmount || r.DebitAmount || 0,
    r.creditAmount || r.CreditAmount || 0,
    r.balance || r.Balance || 0
  ])
  exportToExcel([
    { name: '试算平衡表', columns: ['科目编码', '科目名称', '方向', '借方发生额', '贷方发生额', '余额'], rows }
  ], '试算平衡表')
}
onMounted(loadData)
</script>
