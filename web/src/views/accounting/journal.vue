<template>
  <div>
    <div class="page-header"><h2>日记账</h2></div>
    <el-table :data="entries" stripe v-loading="loading">
      <el-table-column prop="voucherNo" label="凭证号" width="160" />
      <el-table-column prop="accountingSubjectName" label="会计科目" width="200" />
      <el-table-column prop="direction" label="方向" width="70">
        <template #default="{ row }">{{ row.direction === 'Debit' ? '借' : '贷' }}</template>
      </el-table-column>
      <el-table-column prop="amount" label="金额" width="150" align="right">
        <template #default="{ row }">¥{{ row.amount?.toFixed(2) }}</template>
      </el-table-column>
      <el-table-column prop="summary" label="摘要" min-width="200" />
      <el-table-column prop="createdAt" label="日期" width="160" />
    </el-table>
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue'
import { getVouchers } from '@/api'
const loading = ref(false)
const entries = ref([])
async function loadData() {
  loading.value = true
  try {
    const res = await getVouchers()
    entries.value = res
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}
onMounted(loadData)
</script>
