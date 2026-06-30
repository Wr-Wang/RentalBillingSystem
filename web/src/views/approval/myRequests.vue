<template>
  <div>
    <div class="page-header">
      <h2>我提交的审批</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-table :data="myRequests" stripe v-loading="loading">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column label="申请编号" width="150">
        <template #default="{ row }">{{ row.id?.slice(0, 8) || '-' }}</template>
      </el-table-column>
      <el-table-column prop="approvalTypeName" label="审批类型" width="120" />
      <el-table-column prop="title" label="业务摘要" min-width="200" />
      <el-table-column prop="currentLevel" label="当前进度" width="90">
        <template #default="{ row }">{{ row.currentLevel }}/{{ row.maxLevel }}级</template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Approved' ? 'success' : row.status === 'Rejected' ? 'danger' : 'warning'" size="small">
            {{ row.status === 'Pending' ? '审批中' : row.status === 'Approved' ? '已通过' : '已驳回' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="提交时间" width="150">
        <template #default="{ row }">{{ formatTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="完成时间" width="150">
        <template #default="{ row }">{{ formatTime(row.completedAt) || '-' }}</template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getMyApprovalRequests } from '../../api/index'

const loading = ref(false)
const myRequests = ref([])

function formatTime(t) {
  if (!t) return ''
  const d = new Date(t)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

async function fetchMyRequests() {
  loading.value = true
  try {
    const res = await getMyApprovalRequests()
    myRequests.value = Array.isArray(res) ? res : []
  } catch (e) {
    myRequests.value = []
  }
  loading.value = false
}

onMounted(() => { fetchMyRequests() })
</script>
