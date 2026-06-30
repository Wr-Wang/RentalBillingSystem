<template>
  <div>
    <div class="page-header">
      <h2>审批中心</h2>
      <div class="table-actions">
        <el-button @click="$router.push('/approvals/myrequests')">
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
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="viewDetail(row)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 审批详情对话框 -->
    <ApprovalDetailDialog
      v-model="showDetail"
      :approval-id="currentId"
      :show-actions="true"
      @approved="fetchPending"
      @rejected="fetchPending"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getPendingApprovals } from '../../api/index'
import ApprovalDetailDialog from './ApprovalDetailDialog.vue'

const loading = ref(false)
const pendingList = ref([])
const approvedThisMonth = ref(0)
const showDetail = ref(false)
const currentId = ref('')

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
    approvedThisMonth.value = pendingList.value.filter(r => r.status === 'Approved').length
  } catch (e) {
    pendingList.value = []
    approvedThisMonth.value = 0
  }
  loading.value = false
}

function viewDetail(row) {
  currentId.value = row.id
  showDetail.value = true
}

onMounted(() => { fetchPending() })
</script>

<style scoped>
.stat-cards { display: flex; gap: 16px; margin-bottom: 16px; }
.stat-card { flex: 1; background: #fff; padding: 16px 20px; border-radius: 8px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); }
.stat-card .label { font-size: 13px; color: #909399; }
.stat-card .value { font-size: 28px; font-weight: bold; margin-top: 4px; }
</style>
