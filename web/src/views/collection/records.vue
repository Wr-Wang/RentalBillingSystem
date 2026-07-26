<template>
  <div>
    <div class="page-header">
      <h2>催缴记录</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="合同号" clearable style="width: 200px;" @clear="fetchRecords" @keyup.enter="fetchRecords" />
      <el-select v-model="search.status" placeholder="状态" clearable filterable style="width: 120px;" @change="fetchRecords">
        <el-option label="全部" value="" />
        <el-option label="待发送" value="Pending" />
        <el-option label="已发送" value="Sent" />
        <el-option label="失败" value="Failed" />
      </el-select>
      <el-button type="primary" @click="fetchRecords">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-table :data="records" v-loading="loading" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="contractNo" label="合同号" width="180" />
      <el-table-column label="催缴阶段" width="100">
        <template #default="{ row }">S{{ row.stageNo }} {{ getStageName(row.stageNo) }}</template>
      </el-table-column>
      <el-table-column prop="channel" label="渠道" width="80">
        <template #default="{ row }">{{ channelLabel(row.channel) }}</template>
      </el-table-column>
      <el-table-column prop="content" label="催缴内容" min-width="200" show-overflow-tooltip />
      <el-table-column label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="statusType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" width="160">
        <template #default="{ row }">{{ row.createdAt ? new Date(row.createdAt).toLocaleString() : '-' }}</template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination
        v-model:page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @current-change="fetchRecords"
        @size-change="fetchRecords"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getCollectionRecords, getCollectionStages } from '@/api'

const loading = ref(false)
const records = ref([])
const stages = ref([])

const search = reactive({ keyword: '', status: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })

const channelLabels = { SMS: '短信', PHONE: '电话', VISIT: '上门', LEGAL: '律师函', Email: '邮件' }
function channelLabel(c) { return channelLabels[c] || c }

function statusType(s) {
  return { Pending: 'info', Sent: 'success', Failed: 'danger' }[s] || 'info'
}
function statusLabel(s) {
  return { Pending: '待发送', Sent: '已发送', Failed: '失败' }[s] || s
}

// 阶段序号 → 阶段名称
function getStageName(stageNo) {
  const found = stages.value.find(s => s.stageNo === stageNo)
  return found ? found.stageName : ''
}

async function fetchRecords() {
  loading.value = true
  try {
    const res = await getCollectionRecords({})
    let items = Array.isArray(res) ? res : (res.items || res.data || res || [])

    // 前端过滤
    if (search.keyword) {
      const kw = search.keyword.toLowerCase()
      items = items.filter(r =>
        (r.contractNo || '').toLowerCase().includes(kw)
      )
    }
    if (search.status) {
      items = items.filter(r => r.status === search.status)
    }

    pagination.total = items.length
    const start = (pagination.page - 1) * pagination.pageSize
    records.value = items.slice(start, start + pagination.pageSize)
  } catch { ElMessage.error('加载催缴记录失败') }
  finally { loading.value = false }
}

function resetSearch() {
  search.keyword = ''
  search.status = ''
  pagination.page = 1
  fetchRecords()
}

onMounted(async () => {
  try {
    const list = await getCollectionStages()
    stages.value = Array.isArray(list) ? list : []
  } catch { /* 静默 */ }
  fetchRecords()
})
</script>
<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.search-bar { display: flex; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
</style>
