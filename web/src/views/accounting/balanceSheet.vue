<template>
  <div>
    <div class="page-header"><h2>资产负债表</h2></div>
    <div class="search-bar">
      <el-date-picker v-model="endDate" type="date" placeholder="截止日期" style="width:200px;" />
      <el-button type="primary" @click="loadData">查询</el-button>
      <el-button @click="exportExcel" :disabled="Object.keys(categories).length === 0">导出 Excel</el-button>
      <span style="margin-left:16px;color:#999;">日期：{{ displayDate }}</span>
    </div>

    <div v-loading="loading">
      <el-row :gutter="20" v-for="(subCategories, catName) in categories" :key="catName">
        <el-col :span="24">
          <h3 style="margin:16px 0 8px;border-bottom:2px solid #409eff;padding-bottom:4px;">{{ catName }}</h3>
        </el-col>
        <el-col :span="24" v-for="(items, subCat) in subCategories" :key="subCat">
          <h4 style="margin:8px 0;color:#666;">{{ subCat }}</h4>
          <el-table :data="items" stripe show-summary :summary-method="(p) => subSummary(p, items)">
            <el-table-column label="科目编码" width="120">
              <template #default="{ row }">{{ row.code }}</template>
            </el-table-column>
            <el-table-column label="科目名称" width="200">
              <template #default="{ row }">{{ row.name }}</template>
            </el-table-column>
            <el-table-column label="期末余额" width="150" align="right">
              <template #default="{ row }">¥{{ row.balance.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</template>
            </el-table-column>
          </el-table>
        </el-col>
      </el-row>

      <el-empty v-if="!loading && Object.keys(categories).length === 0" description="暂无数据" style="margin-top:40px;" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { getBalanceSheet } from '@/api'
import { exportToExcel } from '@/utils/exportExcel'

const loading = ref(false)
const endDate = ref(new Date())
const categories = ref({})
const reportDate = ref('')

const displayDate = computed(() => reportDate.value || '-')

async function loadData() {
  loading.value = true
  try {
    const date = endDate.value ? new Date(endDate.value).toISOString().slice(0, 10) : undefined
    const res = await getBalanceSheet({ endDate: date })
    categories.value = res.categories || {}
    reportDate.value = res.endDate || ''
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}

function subSummary(_, items) {
  const total = items.reduce((s, r) => s + r.balance, 0)
  return [null, '小计', `¥${total.toLocaleString('zh-CN', { minimumFractionDigits: 2 })}`]
}

function exportExcel() {
  const rows = []
  for (const [catName, subCategories] of Object.entries(categories.value)) {
    rows.push([catName, '', ''])
    for (const [subCat, items] of Object.entries(subCategories)) {
      for (const item of items) {
        rows.push([item.code, item.name, item.balance])
      }
      const subTotal = items.reduce((s, r) => s + r.balance, 0)
      rows.push(['', `${subCat} 小计`, subTotal])
    }
  }
  exportToExcel([{ name: '资产负债表', columns: ['科目编码', '科目名称', '期末余额'], rows }], '资产负债表')
}

onMounted(loadData)
</script>
<style scoped>
.search-bar { margin-bottom: 16px; }
</style>
