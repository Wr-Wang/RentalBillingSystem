<template>
  <div>
    <div class="page-header">
      <h2>审批历史</h2>
      <div class="table-actions">
        <el-button @click="$router.push('/approvals')">返回审批中心</el-button>
      </div>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="搜索标题" clearable style="width: 200px;" @clear="fetchHistory" @keyup.enter="fetchHistory" />
      <el-select v-model="search.status" placeholder="审批结果" clearable style="width: 140px;" @change="fetchHistory">
        <el-option label="待审批" value="Pending" />
        <el-option label="已通过" value="Approved" />
        <el-option label="已驳回" value="Rejected" />
        <el-option label="已撤回" value="Cancelled" />
      </el-select>
      <el-button type="primary" @click="fetchHistory">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-table :data="historyList" stripe v-loading="loading">
      <el-table-column label="申请编号" width="150">
        <template #default="{ row }">{{ row.id?.slice(0, 8) || '-' }}</template>
      </el-table-column>
      <el-table-column prop="approvalTypeName" label="审批类型" width="120" />
      <el-table-column prop="submitterName" label="申请人" width="100" />
      <el-table-column prop="title" label="业务摘要" min-width="200" />
      <el-table-column prop="status" label="最终结果" width="100">
        <template #default="{ row }">
          <el-tag :type="statusTagType(row.status)" size="small">
            {{ statusLabel(row.status) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="提交时间" width="150">
        <template #default="{ row }">{{ formatTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="完成时间" width="150">
        <template #default="{ row }">{{ formatTime(row.completedAt) || '-' }}</template>
      </el-table-column>
      <el-table-column label="操作" width="80">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="viewDetail(row)">查看</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination
        v-model:current-page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @change="fetchHistory"
      />
    </div>

    <ApprovalDetailDialog
      v-model="showDetail"
      :approval-id="currentId"
      @approved="fetchHistory"
      @rejected="fetchHistory"
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { getApprovalHistoryList } from '../../api/index'
import ApprovalDetailDialog from './ApprovalDetailDialog.vue'

const loading = ref(false)
const showDetail = ref(false)
const currentId = ref('')
const search = reactive({ keyword: '', status: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })
const historyList = ref([])

function formatTime(t) {
  if (!t) return ''
  const d = new Date(t)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function statusLabel(status) {
  const map = { Pending: '待审批', Approved: '已通过', Rejected: '已驳回', Cancelled: '已撤回' }
  return map[status] || status
}

function statusTagType(status) {
  const map = { Pending: 'warning', Approved: 'success', Rejected: 'danger', Cancelled: 'info' }
  return map[status] || 'info'
}

async function fetchHistory() {
  loading.value = true
  try {
    const params = {
      page: pagination.page,
      pageSize: pagination.pageSize
    }
    if (search.keyword) params.keyword = search.keyword
    if (search.status) params.status = search.status

    const res = await getApprovalHistoryList(params)
    historyList.value = res?.items || []
    pagination.total = res?.total || 0
  } catch (e) {
    historyList.value = []
    pagination.total = 0
  }
  loading.value = false
}

function resetSearch() {
  search.keyword = ''
  search.status = ''
  pagination.page = 1
  fetchHistory()
}

function viewDetail(row) {
  currentId.value = row.id
  showDetail.value = true
}

onMounted(() => { fetchHistory() })
</script>
