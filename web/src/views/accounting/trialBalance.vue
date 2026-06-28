<template>
  <div>
    <div class="page-header">
      <h2>试算平衡表</h2>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="searchDate" type="date" placeholder="选择日期" />
      <el-button type="primary">查询</el-button>
    </div>

    <el-card>
      <el-table :data="trialBalanceData" stripe>
        <el-table-column prop="code" label="科目编码" width="120" />
        <el-table-column prop="name" label="科目名称" width="200" />
        <el-table-column prop="openingDebit" label="期初借方" width="120">
          <template #default="{ row }">¥{{ (row.openingDebit || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="openingCredit" label="期初贷方" width="120">
          <template #default="{ row }">¥{{ (row.openingCredit || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="currentDebit" label="本期借方" width="120">
          <template #default="{ row }">¥{{ (row.currentDebit || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="currentCredit" label="本期贷方" width="120">
          <template #default="{ row }">¥{{ (row.currentCredit || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="endingDebit" label="期末借方" width="120">
          <template #default="{ row }">¥{{ (row.endingDebit || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="endingCredit" label="期末贷方" width="120">
          <template #default="{ row }">¥{{ (row.endingCredit || 0).toLocaleString() }}</template>
        </el-table-column>
      </el-table>
    </el-card>

    <div style="margin-top: 16px; text-align: center;">
      <el-alert
        :title="isBalanced ? '✓ 借贷平衡' : '✗ 借贷不平衡！'"
        :type="isBalanced ? 'success' : 'error'"
        show-icon
        :closable="false"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const searchDate = ref(new Date())

const trialBalanceData = ref([
  { code: '1002', name: '银行存款', openingDebit: 500000, openingCredit: 0, currentDebit: 128500, currentCredit: 85000, endingDebit: 543500, endingCredit: 0 },
  { code: '112201', name: '应收账款-房租', openingDebit: 150000, openingCredit: 0, currentDebit: 156000, currentCredit: 128500, endingDebit: 177500, endingCredit: 0 },
  { code: '600101', name: '主营业务收入-房租', openingDebit: 0, openingCredit: 0, currentDebit: 0, currentCredit: 147169.81, endingDebit: 0, endingCredit: 147169.81 },
  { code: '222101', name: '应交税费-增值税(6%)', openingDebit: 0, openingCredit: 0, currentDebit: 0, currentCredit: 8830.19, endingDebit: 0, endingCredit: 8830.19 }
])

const isBalanced = computed(() => {
  const totalDebit = trialBalanceData.value.reduce((s, r) => s + (r.endingDebit || 0), 0)
  const totalCredit = trialBalanceData.value.reduce((s, r) => s + (r.endingCredit || 0), 0)
  return Math.abs(totalDebit - totalCredit) < 0.01
})
</script>
