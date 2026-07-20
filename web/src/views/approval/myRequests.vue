<template>
  <div>
    <div class="page-header">
      <h2>我提交的审批</h2>
      <div class="table-actions">
        <el-button @click="$router.push('/approvals')">返回审批中心</el-button>
      </div>
    </div>

    <el-card v-loading="loading">
      <template #header>我的提交记录</template>
      <el-table :data="myRequests" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column label="申请编号" width="180">
          <template #default="{ row }">{{ row.requestNo || '-' }}</template>
        </el-table-column>
        <el-table-column prop="approvalTypeName" label="审批类型" width="120" />
        <el-table-column prop="title" label="业务摘要" min-width="200" />
        <el-table-column prop="currentLevel" label="当前进度" width="90">
          <template #default="{ row }">{{ row.currentLevel }}/{{ row.maxLevel }}级</template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="approvalStatusTagType(row.status)" size="small">
              {{ approvalStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="提交时间" width="150">
          <template #default="{ row }">{{ formatTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="完成时间" width="150">
          <template #default="{ row }">{{ formatTime(row.completedAt) || '-' }}</template>
        </el-table-column>
        <el-table-column label="操作" width="140" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="viewDetail(row)">详情</el-button>
            <el-button
              v-if="row.status === 'Pending'"
              text size="small"
              type="warning"
              @click="withdraw(row)"
            >撤回</el-button>
            <el-tag v-else-if="row.status === 'Rejected'" size="small" type="danger" effect="plain">已驳回</el-tag>
            <el-tag v-else-if="row.status === 'Cancelled'" size="small" type="info" effect="plain">已撤回</el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 审批详情对话框 -->
    <ApprovalDetailDialog
      v-model="showDetail"
      :approval-id="currentId"
      @approved="fetchMyRequests"
      @rejected="fetchMyRequests"
      @cancelled="fetchMyRequests"
    />
  </div>
</template>

<script setup>
import { approvalStatusText, approvalStatusTagType } from '@/utils/statusHelper'
import { formatTime } from '@/utils/chinaTime'
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getMyApprovalRequests, cancelApproval } from '../../api/index'
import ApprovalDetailDialog from './ApprovalDetailDialog.vue'

const loading = ref(false)
const myRequests = ref([])
const showDetail = ref(false)
const currentId = ref('')

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

async function withdraw(row) {
  try {
    await ElMessageBox.confirm('确定要撤回该审批申请吗？撤回后可重新提交。', '撤回确认', {
      confirmButtonText: '撤回', cancelButtonText: '取消', type: 'warning'
    })
    await cancelApproval(row.id, { reason: '提交人撤回' })
    ElMessage.success('已撤回')
    await fetchMyRequests()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('撤回失败')
  }
}

function viewDetail(row) {
  currentId.value = row.id
  showDetail.value = true
}

onMounted(() => { fetchMyRequests() })
</script>
