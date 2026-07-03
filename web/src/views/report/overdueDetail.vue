<template>
  <div>
    <div class="page-header">
      <h2>欠费明细表</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="合同号/租客" clearable style="width: 200px;" @clear="loadData" @keyup.enter="loadData" />
      <el-button type="primary" @click="loadData">查询</el-button>
    </div>

    <el-table :data="filteredList" v-loading="loading" stripe style="width:100%">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="contractNo" label="合同号" width="130" />
      <el-table-column prop="roomName" label="房屋" width="100" />
      <el-table-column prop="tenantName" label="租客" width="100" />
      <el-table-column prop="period" label="账期" width="80" />
      <el-table-column label="欠费金额" width="110">
        <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="overdueDays" label="逾期天数" width="90" />
      <el-table-column label="滞纳金" min-width="100">
        <template #default="{ row }">¥{{ (row.lateFee || 0).toLocaleString() }}</template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination
        v-model:page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @current-change="loadData"
        @size-change="loadData"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getOverdueDetail } from '@/api'

const loading = ref(false)
const overdueList = ref([])
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const search = ref({ keyword: '' })

const filteredList = computed(() => {
  let items = overdueList.value
  if (search.value.keyword) {
    const kw = search.value.keyword.toLowerCase()
    items = items.filter(r =>
      (r.contractNo || '').toLowerCase().includes(kw) ||
      (r.tenantName || '').toLowerCase().includes(kw)
    )
  }
  total.value = items.length
  const start = (page.value - 1) * pageSize.value
  return items.slice(start, start + pageSize.value)
})

async function loadData() {
  loading.value = true
  try {
    const res = await getOverdueDetail({})
    const items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    overdueList.value = items.map(item => ({
      contractNo: item.contractNo || '',
      roomName: item.roomFullCode || item.roomName || '',
      tenantName: item.tenantName || '',
      period: item.period || '',
      dueDate: item.dueDate || '',
      amount: item.amount || 0,
      overdueAmount: (item.amount || 0) - (item.received || 0),
      overdueDays: item.daysOverdue || 0,
      lateFee: item.lateFee || 0
    }))
    total.value = overdueList.value.length
  } catch {
    ElMessage.error('加载欠费明细失败')
  }
  loading.value = false
}

onMounted(loadData)
</script>
