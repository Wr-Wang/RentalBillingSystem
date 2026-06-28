<template>
  <div>
    <div class="page-header">
      <h2>日记账</h2>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="search.dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width: 220px;" />
      <el-select v-model="search.subjectId" placeholder="科目" clearable style="width: 160px;" filterable>
        <el-option v-for="s in allSubjects" :key="s.id" :label="s.code + ' ' + s.name" :value="s.id" />
      </el-select>
      <el-button type="primary">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-card>
      <el-table :data="journalEntries" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="date" label="日期" width="100" />
        <el-table-column prop="voucherNo" label="凭证号" width="150" />
        <el-table-column prop="subjectCode" label="科目编码" width="100" />
        <el-table-column prop="subjectName" label="科目名称" width="180" />
        <el-table-column prop="summary" label="摘要" min-width="200" />
        <el-table-column prop="debitAmount" label="借方金额" width="120">
          <template #default="{ row }">{{ row.debitAmount ? '¥' + row.debitAmount?.toLocaleString() : '-' }}</template>
        </el-table-column>
        <el-table-column prop="creditAmount" label="贷方金额" width="120">
          <template #default="{ row }">{{ row.creditAmount ? '¥' + row.creditAmount?.toLocaleString() : '-' }}</template>
        </el-table-column>
      </el-table>
      <div style="margin-top: 12px; color: #909399;">
        借方合计: ¥{{ debitTotal.toLocaleString() }} &nbsp;&nbsp; 贷方合计: ¥{{ creditTotal.toLocaleString() }}
      </div>
    </el-card>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" layout="total, sizes, prev, pager, next" />
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'

const search = reactive({ dateRange: null, subjectId: '' })
const pagination = reactive({ page: 1, pageSize: 20, total: 200 })

const allSubjects = [
  { id: 's1', code: '1002', name: '银行存款' },
  { id: 's2', code: '112201', name: '应收账款-房租' },
  { id: 's3', code: '600101', name: '主营业务收入-房租' },
  { id: 's4', code: '222101', name: '应交税费-增值税(6%)' }
]

const journalEntries = ref([
  { date: '2026-06-27', voucherNo: 'PJZ-20260627-001', subjectCode: '1002', subjectName: '银行存款', summary: '收A-101 6月房租', debitAmount: null, creditAmount: 5460 },
  { date: '2026-06-27', voucherNo: 'PJZ-20260627-001', subjectCode: '112201', subjectName: '应收账款-房租', summary: '收A-101 6月房租', debitAmount: 5460, creditAmount: null },
  { date: '2026-06-27', voucherNo: 'PJZ-20260627-002', subjectCode: '112201', subjectName: '应收账款-房租', summary: 'A-101 6月收入确认', debitAmount: null, creditAmount: 5200 },
  { date: '2026-06-27', voucherNo: 'PJZ-20260627-002', subjectCode: '600101', subjectName: '主营业务收入-房租', summary: 'A-101 6月收入确认', debitAmount: 4905.66, creditAmount: null },
  { date: '2026-06-27', voucherNo: 'PJZ-20260627-002', subjectCode: '222101', subjectName: '应交税费-增值税(6%)', summary: 'A-101 6月收入确认', debitAmount: 294.34, creditAmount: null }
])

const debitTotal = computed(() => journalEntries.value.reduce((s, r) => s + (r.debitAmount || 0), 0))
const creditTotal = computed(() => journalEntries.value.reduce((s, r) => s + (r.creditAmount || 0), 0))

function resetSearch() { search.dateRange = null; search.subjectId = '' }
</script>
