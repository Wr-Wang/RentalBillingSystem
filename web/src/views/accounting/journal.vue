<template>
  <div>
    <div class="page-header">
      <h2>日记账</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width:240px;" @change="loadData" />
      <el-select v-model="filterSubjectCode" filterable clearable placeholder="科目" style="width:200px;margin-left:12px;" @change="loadData">
        <el-option v-for="s in subjects" :key="s.code" :label="s.code + ' ' + s.name" :value="s.code" />
      </el-select>
      <el-select v-model="filterDirection" clearable placeholder="方向" style="width:100px;margin-left:12px;" @change="loadData">
        <el-option label="借方" value="Debit" />
        <el-option label="贷方" value="Credit" />
      </el-select>
    </div>

    <el-table :data="entries" stripe v-loading="loading" @row-click="viewVoucherDetail">
      <el-table-column prop="voucherNo" label="凭证号" width="160" />
      <el-table-column prop="accountingSubjectName" label="会计科目" width="200" />
      <el-table-column prop="direction" label="方向" width="70">
        <template #default="{ row }">{{ row.direction === 'Debit' ? '借' : '贷' }}</template>
      </el-table-column>
      <el-table-column label="金额" width="150" align="right">
        <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</template>
      </el-table-column>
      <el-table-column prop="summary" label="摘要" min-width="200" show-overflow-tooltip />
      <el-table-column prop="period" label="期间" width="80" />
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
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getJournalEntries, getAccountingSubjects, getVoucher } from '@/api'

const route = useRoute()
const router = useRouter()
const loading = ref(false)
const entries = ref([])
const dateRange = ref(null)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const subjects = ref([])
const filterSubjectCode = ref(route.query.subjectCode || '')
const filterDirection = ref('')

async function loadSubjects() {
  try { subjects.value = await getAccountingSubjects() || [] }
  catch { subjects.value = [] }
}

async function loadData() {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: pageSize.value }
    if (dateRange.value) {
      params.startDate = dateRange.value[0].toISOString().slice(0, 10)
      params.endDate = dateRange.value[1].toISOString().slice(0, 10)
    }
    const res = await getJournalEntries(params)
    const items = res.items || []
    total.value = res.total ?? items.length
    // 前端过滤（科目 + 方向）
    let filtered = items
    if (filterSubjectCode.value) {
      filtered = filtered.filter(e => e.subjectCode === filterSubjectCode.value)
    }
    if (filterDirection.value) {
      filtered = filtered.filter(e => e.direction === filterDirection.value)
    }
    entries.value = filtered.map(e => ({
      id: e.id,
      voucherId: e.voucherId,
      voucherNo: e.voucherNo || '',
      accountingSubjectName: e.accountingSubjectName || '',
      subjectCode: e.subjectCode || '',
      direction: e.direction || 'Debit',
      amount: e.amount || 0,
      summary: e.summary || '',
      period: e.period || '',
      createdAt: e.createdAt || ''
    }))
  } catch {
    ElMessage.error('加载日记账失败')
  }
  finally { loading.value = false }
}

async function viewVoucherDetail(row) {
  if (row.voucherId) {
    router.push({ name: 'AccountingVouchers', query: { highlight: row.voucherId } })
  }
}

onMounted(() => {
  loadSubjects()
  loadData()
})
</script>
<style scoped>
.search-bar {
  margin-bottom: 16px;
}
</style>
