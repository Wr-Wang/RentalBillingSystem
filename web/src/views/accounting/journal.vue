<template>
  <div>
    <div class="page-header">
      <h2>日记账</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width:240px;" @change="loadData" />
    </div>

    <el-table :data="entries" stripe v-loading="loading">
      <el-table-column prop="voucherNo" label="凭证号" width="160" />
      <el-table-column prop="accountingSubjectName" label="会计科目" width="200" />
      <el-table-column prop="direction" label="方向" width="70">
        <template #default="{ row }">{{ row.direction === 'Debit' ? '借' : '贷' }}</template>
      </el-table-column>
      <el-table-column label="金额" width="150" align="right">
        <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</template>
      </el-table-column>
      <el-table-column prop="summary" label="摘要" min-width="200" show-overflow-tooltip />
      <el-table-column prop="createdAt" label="日期" width="160" />
    </el-table>

    <div style="margin-top:16px;text-align:right;">
      <el-pagination
        v-model:page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next"
        @current-change="loadData"
        @size-change="loadData"
      />
    </div>
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getJournalEntries } from '@/api'
const loading = ref(false)
const entries = ref([])
const dateRange = ref(null)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

async function loadData() {
  loading.value = true
  try {
    const params = {}
    if (dateRange.value) {
      params.startDate = dateRange.value[0].toISOString().slice(0, 10)
      params.endDate = dateRange.value[1].toISOString().slice(0, 10)
    }
    const res = await getJournalEntries(params)
    let items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    total.value = items.length
    const start = (page.value - 1) * pageSize.value
    entries.value = items.slice(start, start + pageSize.value).map(e => ({
      id: e.id,
      voucherNo: e.voucherNo || '',
      accountingSubjectName: e.accountingSubjectName || '',
      direction: e.direction || 'Debit',
      amount: e.amount || 0,
      summary: e.summary || '',
      createdAt: e.createdAt || ''
    }))
  } catch {
    ElMessage.error('加载日记账失败')
  }
  finally { loading.value = false }
}
onMounted(loadData)
</script>
<style scoped>
.search-bar {
  margin-bottom: 16px;
}
</style>
