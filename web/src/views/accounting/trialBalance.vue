<template>
  <div>
    <div class="page-header"><h2>试算平衡表</h2></div>
    <div class="search-bar">
      <el-date-picker v-model="endDate" type="date" placeholder="截止日期" style="width:200px;" />
      <el-button type="primary" @click="loadData">查询</el-button>
    </div>
    <el-table :data="items" stripe v-loading="loading" show-summary :summary-method="summary">
      <el-table-column prop="code" label="科目编码" width="120" />
      <el-table-column prop="name" label="科目名称" width="200" />
      <el-table-column prop="direction" label="方向" width="70">
        <template #default="{ row }">{{ row.direction === 'Debit' ? '借' : '贷' }}</template>
      </el-table-column>
      <el-table-column prop="debitAmount" label="借方发生额" width="150" align="right">
        <template #default="{ row }">¥{{ row.debitAmount?.toFixed(2) }}</template>
      </el-table-column>
      <el-table-column prop="creditAmount" label="贷方发生额" width="150" align="right">
        <template #default="{ row }">¥{{ row.creditAmount?.toFixed(2) }}</template>
      </el-table-column>
      <el-table-column prop="balance" label="余额" width="150" align="right">
        <template #default="{ row }">¥{{ row.balance?.toFixed(2) }}</template>
      </el-table-column>
    </el-table>
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue'
import { getTrialBalance } from '@/api'
const loading = ref(false)
const endDate = ref(new Date())
const items = ref([])
const totals = ref({ debit: 0, credit: 0 })
async function loadData() {
  loading.value = true
  try {
    const date = endDate.value ? new Date(endDate.value).toISOString().slice(0, 10) : undefined
    const res = await getTrialBalance({ endDate: date })
    items.value = res.subjects || []
    totals.value = { debit: res.totalDebit || 0, credit: res.totalCredit || 0 }
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}
function summary() { return [null, '合计', '', `¥${totals.value.debit.toFixed(2)}`, `¥${totals.value.credit.toFixed(2)}`, ''] }
onMounted(loadData)
</script>
