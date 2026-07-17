<template>
  <div>
    <div class="page-header">
      <h2>催缴记录</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="合同号/租客" clearable style="width: 200px;" @clear="fetchRecords" @keyup.enter="fetchRecords" />
      <el-button type="primary" @click="fetchRecords">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-table :data="records" v-loading="loading" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="contractNo" label="合同号" width="180" />
      <el-table-column prop="contractId" label="合同 ID" width="200" show-overflow-tooltip />
      <el-table-column prop="stageName" label="阶段" width="100" />
      <el-table-column prop="createdAt" label="发送时间" width="160" />
      <el-table-column prop="contactResult" label="结果" min-width="150" />
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

const search = reactive({ keyword: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })

// 阶段映射
function getStageName(stageId) {
  const found = stages.value.find(s => s.id === stageId)
  return found ? found.name : stageId
}

async function fetchRecords() {
  loading.value = true
  try {
    const res = await getCollectionRecords({})
    let items = Array.isArray(res) ? res : (res.items || res.data || res || [])

    // 前端关键词过滤
    if (search.keyword) {
      const kw = search.keyword.toLowerCase()
      items = items.filter(r =>
        (r.contractNo || '').toLowerCase().includes(kw) ||
        (r.contractId || '').toLowerCase().includes(kw)
      )
    }

    pagination.total = items.length
    const start = (pagination.page - 1) * pagination.pageSize
    records.value = items.slice(start, start + pagination.pageSize).map(r => ({
      id: r.id,
      contractNo: r.contractNo || '',
      contractId: r.contractId || '',
      stageName: getStageName(r.collectionStageId),
      createdAt: r.createdAt || '',
      contactResult: r.contactResult || '-'
    }))
  } catch { ElMessage.error('加载催缴记录失败') }
  finally { loading.value = false }
}

function resetSearch() {
  search.keyword = ''
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
