<template>
  <div>
    <div class="page-header">
      <h2>审批历史</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="搜索标题" clearable style="width: 200px;" />
      <el-select v-model="search.status" placeholder="审批结果" clearable style="width: 140px;">
        <el-option label="已通过" value="Approved" />
        <el-option label="已驳回" value="Rejected" />
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
          <el-tag :type="row.status === 'Approved' ? 'success' : 'danger'" size="small">
            {{ row.status === 'Approved' ? '已通过' : '已驳回' }}
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

    <!-- Detail Dialog -->
    <el-dialog v-model="showDetail" :title="'审批详情 - ' + (detail.title || '')" width="600px">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="审批类型">{{ detail.approvalTypeName }}</el-descriptions-item>
        <el-descriptions-item label="申请人">{{ detail.submitterName }}</el-descriptions-item>
        <el-descriptions-item label="审批级别">共{{ detail.maxLevel }}级</el-descriptions-item>
        <el-descriptions-item label="最终结果">
          <el-tag :type="detail.status === 'Approved' ? 'success' : 'danger'" size="small">
            {{ detail.status === 'Approved' ? '已通过' : '已驳回' }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="申请原因" :span="2">{{ detail.description || '无' }}</el-descriptions-item>
      </el-descriptions>

      <h4 style="margin: 16px 0 8px;">审批记录</h4>
      <el-timeline v-if="detail.records && detail.records.length > 0">
        <el-timeline-item
          v-for="(record, index) in detail.records"
          :key="index"
          :timestamp="formatTime(record.createdAt)"
          :type="record.action === 'Approved' ? 'success' : record.action === 'Rejected' ? 'danger' : 'primary'"
        >
          <p>{{ record.approverName }} - {{ record.action === 'Approved' ? '通过' : record.action === 'Rejected' ? '驳回' : record.action }}</p>
          <p v-if="record.comment" style="color: #909399; font-size: 12px;">备注: {{ record.comment }}</p>
        </el-timeline-item>
      </el-timeline>
      <el-empty v-else description="暂无审批记录" />
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { getMyApprovalRequests } from '../../api/index'

const loading = ref(false)
const showDetail = ref(false)
const detail = ref({})
const search = reactive({ keyword: '', status: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })
const historyList = ref([])

function formatTime(t) {
  if (!t) return ''
  const d = new Date(t)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

async function fetchHistory() {
  loading.value = true
  try {
    const res = await getMyApprovalRequests()
    let list = Array.isArray(res) ? res : []
    // 过滤已完成的审批
    list = list.filter(r => r.status === 'Approved' || r.status === 'Rejected')
    if (search.status) list = list.filter(r => r.status === search.status)
    if (search.keyword) list = list.filter(r => (r.title || '').includes(search.keyword))
    historyList.value = list
    pagination.total = list.length
  } catch (e) {
    historyList.value = []
    pagination.total = 0
  }
  loading.value = false
}

function resetSearch() { search.keyword = ''; search.status = ''; fetchHistory() }

function viewDetail(row) {
  detail.value = row
  showDetail.value = true
}

onMounted(() => { fetchHistory() })
</script>
