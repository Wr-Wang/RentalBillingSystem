<template>
  <div class="journal-page">
    <div class="page-header">
      <h1>日记账</h1>
      <el-button type="primary" @click="handleGenerate">+ 出账</el-button>
    </div>

    <el-card shadow="never">
      <el-form :inline="true" :model="filters" size="default">
        <el-form-item label="账期">
          <el-date-picker v-model="filters.period" type="month" placeholder="选择账期" value-format="yyyy-MM" />
        </el-form-item>
        <el-form-item label="合同号">
          <el-input v-model="filters.contractNo" placeholder="合同号" clearable />
        </el-form-item>
        <el-form-item label="费用类型">
          <el-select v-model="filters.feeCodeId" placeholder="全部" clearable>
            <el-option label="全部" value="" />
          </el-select>
        </el-form-item>
        <el-form-item label="GL状态">
          <el-select v-model="filters.glPosted" placeholder="全部" clearable>
            <el-option label="全部" value="" />
            <el-option label="已入账" :value="true" />
            <el-option label="未入账" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchData">查询</el-button>
          <el-button @click="resetFilters">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never" style="margin-top: 16px;">
      <el-table :data="list" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="billedAt" label="出账时间" width="160" />
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="period" label="账期" width="80" />
        <el-table-column prop="entryType" label="费用类型" width="100" />
        <el-table-column prop="amount" label="金额" width="120" align="right">
          <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="dueDate" label="到期日" width="100" />
        <el-table-column label="GL状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.glPosted ? 'success' : 'warning'" size="small">
              {{ row.glPosted ? '已入账' : '未入账' }}
            </el-tag>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination
        v-model:current-page="page"
        :page-size="pageSize"
        :total="total"
        layout="prev, pager, next, total"
        @current-change="fetchData"
        style="margin-top: 16px; justify-content: flex-end;" />
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getJournals, generateJournals } from '@/api'

const list = ref([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const filters = ref({ period: '', contractNo: '', feeCodeId: '', glPosted: '' })

const fetchData = async () => {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: pageSize.value, ...filters.value }
    const res = await getJournals(params)
    list.value = res.items || []
    total.value = res.total || 0
  } finally {
    loading.value = false
  }
}

const handleGenerate = async () => {
  try {
    await generateJournals({})
    await fetchData()
  } catch (e) { /* ignore */ }
}

const resetFilters = () => {
  filters.value = { period: '', contractNo: '', feeCodeId: '', glPosted: '' }
  page.value = 1
  fetchData()
}

onMounted(fetchData)
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h1 { margin: 0; font-size: 20px; }
</style>
