<template>
  <div>
    <div class="page-header">
      <h2>审批中心</h2>
      <div class="table-actions">
        <el-button @click="$router.push('/approvals/my-requests')">
          <el-icon><EditPen /></el-icon>我提交的审批
        </el-button>
        <el-button @click="$router.push('/approvals/history')">
          <el-icon><Timer /></el-icon>审批历史
        </el-button>
      </div>
    </div>

    <div class="stat-cards">
      <div class="stat-card" style="border-left: 4px solid #409eff;">
        <div class="label">待我审批</div>
        <div class="value" style="color: #409eff;">{{ pendingList.length }}</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #67c23a;">
        <div class="label">本月已审批</div>
        <div class="value" style="color: #67c23a;">{{ approvedThisMonth }}</div>
      </div>
    </div>

    <el-card v-loading="loading">
      <template #header>待审批列表</template>
      <el-table :data="pendingList" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column label="申请编号" width="150">
          <template #default="{ row }">{{ row.id?.slice(0, 8) || '-' }}</template>
        </el-table-column>
        <el-table-column prop="approvalTypeName" label="审批类型" width="120">
          <template #default="{ row }">
            <el-tag size="small">{{ row.approvalTypeName || '通用审批' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="submitterName" label="申请人" width="100" />
        <el-table-column prop="title" label="业务摘要" min-width="200" />
        <el-table-column prop="currentLevel" label="当前级别" width="90">
          <template #default="{ row }">第{{ row.currentLevel }}级/共{{ row.maxLevel }}级</template>
        </el-table-column>
        <el-table-column prop="currentLevelName" label="审批角色" width="120" />
        <el-table-column label="提交时间" width="150">
          <template #default="{ row }">{{ formatTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="viewDetail(row)">详情</el-button>
            <el-button type="success" size="small" @click="viewDetail(row)">通过</el-button>
            <el-button type="danger" size="small" @click="viewDetail(row)">驳回</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Approval Detail Dialog -->
    <el-dialog v-model="showDetail" :title="'审批详情 - ' + (detail.title || '')" width="600px">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="审批类型">{{ detail.approvalTypeName }}</el-descriptions-item>
        <el-descriptions-item label="申请人">{{ detail.submitterName }}</el-descriptions-item>
        <el-descriptions-item label="当前级别">第{{ detail.currentLevel }}级/共{{ detail.maxLevel }}级</el-descriptions-item>
        <el-descriptions-item label="审批角色">{{ detail.currentLevelName || '-' }}</el-descriptions-item>
        <el-descriptions-item label="提交时间" :span="2">{{ formatTime(detail.createdAt) }}</el-descriptions-item>
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
        <el-timeline-item v-if="detail.status === 'Pending'" timestamp="待处理" type="primary">
          <p>等待 {{ detail.currentLevelName || '下一级' }} 审批</p>
        </el-timeline-item>
      </el-timeline>
      <el-empty v-else description="暂无审批记录" />

      <template #footer>
        <el-input v-model="approvalComment" placeholder="审批意见（可选）" style="margin-bottom: 12px;" />
        <el-button type="success" @click="submitApproval('approve')" :loading="submitting">通过</el-button>
        <el-button type="danger" @click="submitApproval('reject')" :loading="submitting">驳回</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getPendingApprovals, approveApproval, rejectApproval } from '../../api/index'

const loading = ref(false)
const submitting = ref(false)
const pendingList = ref([])
const approvedThisMonth = ref(0)
const showDetail = ref(false)
const approvalComment = ref('')
const detail = ref({})

function formatTime(t) {
  if (!t) return ''
  const d = new Date(t)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

async function fetchPending() {
  loading.value = true
  try {
    const res = await getPendingApprovals()
    pendingList.value = Array.isArray(res) ? res : []
    approvedThisMonth.value = pendingList.value.length
  } catch (e) {
    pendingList.value = []
    approvedThisMonth.value = 0
  }
  loading.value = false
}

async function submitApproval(action) {
  if (!detail.value.id) return
  submitting.value = true
  try {
    if (action === 'approve') {
      await approveApproval(detail.value.id, { comment: approvalComment.value || null })
      ElMessage.success('审批通过')
    } else {
      await rejectApproval(detail.value.id, { comment: approvalComment.value || '驳回' })
      ElMessage.success('已驳回')
    }
    showDetail.value = false
    approvalComment.value = ''
    await fetchPending()
  } catch (e) {
    ElMessage.error('操作失败')
  }
  submitting.value = false
}

// 行按钮：仅打开详情对话框，在对话框中执行审批操作
function viewDetail(row) {
  detail.value = row
  approvalComment.value = ''
  showDetail.value = true
}

onMounted(() => { fetchPending() })
</script>
