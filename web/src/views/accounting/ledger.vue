<template>
  <div>
    <div class="page-header"><h2>明细账</h2></div>
    <div class="search-bar">
      <el-select v-model="subjectCode" filterable placeholder="选择科目" style="width:260px;" @change="loadData">
        <el-option v-for="s in subjects" :key="s.code" :label="s.code + ' ' + s.name" :value="s.code" />
      </el-select>
      <el-date-picker v-model="dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width:240px;margin-left:12px;" @change="loadData" />
      <el-button type="primary" @click="loadData" :disabled="!subjectCode">查询</el-button>
      <el-button @click="exportExcel" :disabled="entries.length === 0">导出 Excel</el-button>
    </div>

    <template v-if="subjectCode && !loading">
      <div style="margin:12px 0;color:#666;">
        科目：<strong>{{ subjectLabel }}</strong>
        <span style="margin-left:24px;">期初余额：<strong>¥{{ openingBalance.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</strong></span>
        <span style="margin-left:24px;">期末余额：<strong>¥{{ endingBalance.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</strong></span>
      </div>

      <el-table :data="entries" stripe v-loading="loading" show-summary :summary-method="summary">
        <el-table-column label="日期" width="110">
          <template #default="{ row }">{{ row.voucherDate }}</template>
        </el-table-column>
        <el-table-column label="凭证号" width="160">
          <template #default="{ row }">{{ row.voucherNo }}</template>
        </el-table-column>
        <el-table-column label="摘要" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">{{ row.summary }}</template>
        </el-table-column>
        <el-table-column label="借方金额" width="150" align="right">
          <template #default="{ row }">{{ row.direction === 'Debit' ? '¥' + row.amount.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) : '-' }}</template>
        </el-table-column>
        <el-table-column label="贷方金额" width="150" align="right">
          <template #default="{ row }">{{ row.direction === 'Credit' ? '¥' + row.amount.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) : '-' }}</template>
        </el-table-column>
        <el-table-column label="余额" width="150" align="right">
          <template #default="{ row }">
            <span :style="{ color: row.balance < 0 ? '#f56c6c' : '#67c23a', fontWeight: 'bold' }">
              ¥{{ row.balance.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}
            </span>
          </template>
        </el-table-column>
      </el-table>
    </template>

    <el-empty v-else-if="!loading" description="请选择一个科目查看明细" style="margin-top:60px;" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { getLedger, getAccountingSubjects } from '@/api'
import { exportToExcel } from '@/utils/exportExcel'

const loading = ref(false)
const subjects = ref([])
const subjectCode = ref('')
const dateRange = ref(null)
const entries = ref([])
const openingBalance = ref(0)
const endingBalance = ref(0)

const subjectLabel = computed(() => {
  const s = subjects.value.find(s => s.code === subjectCode.value)
  return s ? `${s.code} ${s.name}` : subjectCode.value
})

async function loadSubjects() {
  try { subjects.value = await getAccountingSubjects() || [] }
  catch { subjects.value = [] }
}

async function loadData() {
  if (!subjectCode.value) return
  loading.value = true
  try {
    const params = { subjectCode: subjectCode.value }
    if (dateRange.value) {
      params.startDate = dateRange.value[0].toISOString().slice(0, 10)
      params.endDate = dateRange.value[1].toISOString().slice(0, 10)
    }
    const res = await getLedger(params)
    entries.value = (res.entries || []).map(e => ({
      ...e,
      _debit: e.direction === 'Debit' ? e.amount : 0,
      _credit: e.direction === 'Credit' ? e.amount : 0
    }))
    openingBalance.value = res.openingBalance || 0
    endingBalance.value = res.endingBalance || 0
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}

function summary() {
  const d = entries.value.reduce((s, e) => s + e._debit, 0)
  const c = entries.value.reduce((s, e) => s + e._credit, 0)
  return [null, null, '合计', `¥${d.toFixed(2)}`, `¥${c.toFixed(2)}`, `¥${endingBalance.value.toFixed(2)}`]
}

function exportExcel() {
  const rows = entries.value.map(e => [
    e.voucherDate, e.voucherNo, e.summary,
    e.direction === 'Debit' ? e.amount : 0,
    e.direction === 'Credit' ? e.amount : 0,
    e.balance
  ])
  const title = `${subjectLabel.value} 明细账`
  exportToExcel([{ name: '明细账', columns: ['日期', '凭证号', '摘要', '借方金额', '贷方金额', '余额'], rows }], title)
}

onMounted(loadSubjects)
</script>
<style scoped>
.search-bar { margin-bottom: 16px; }
</style>
