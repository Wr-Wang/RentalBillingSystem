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

    <el-card>
      <template #header>待审批列表</template>
      <el-table :data="pendingList" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="requestNo" label="申请编号" width="150" />
        <el-table-column prop="approvalTypeName" label="审批类型" width="120">
          <template #default="{ row }">
            <el-tag size="small">{{ row.approvalTypeName }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="submitterName" label="申请人" width="100" />
        <el-table-column prop="businessSummary" label="业务摘要" min-width="200" />
        <el-table-column prop="amount" label="金额" width="110">
          <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="currentLevel" label="当前级别" width="90">
          <template #default="{ row }">第{{ row.currentLevel }}级</template>
        </el-table-column>
        <el-table-column prop="submittedAt" label="提交时间" width="150" />
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button type="success" size="small" @click="handleApprove(row)">通过</el-button>
            <el-button type="danger" size="small" @click="handleReject(row)">驳回</el-button>
            <el-button text size="small" type="primary" @click="viewDetail(row)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Approval Detail Dialog -->
    <el-dialog v-model="showDetail" title="审批详情" width="600px">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="申请编号">{{ detail.requestNo }}</el-descriptions-item>
        <el-descriptions-item label="审批类型">{{ detail.approvalTypeName }}</el-descriptions-item>
        <el-descriptions-item label="申请人">{{ detail.submitterName }}</el-descriptions-item>
        <el-descriptions-item label="金额">¥{{ detail.amount?.toLocaleString() }}</el-descriptions-item>
        <el-descriptions-item label="提交时间">{{ detail.submittedAt }}</el-descriptions-item>
        <el-descriptions-item label="当前级别">第{{ detail.currentLevel }}级</el-descriptions-item>
        <el-descriptions-item label="申请原因" :span="2">{{ detail.reason }}</el-descriptions-item>
      </el-descriptions>

      <el-timeline style="margin-top: 16px;">
        <el-timeline-item v-for="(record, index) in approvalRecords" :key="index" :timestamp="record.operatedAt" :type="record.action === 'Approve' ? 'success' : 'danger'">
          <p>{{ record.operatorName }} - {{ record.action === 'Approve' ? '通过' : '驳回' }}</p>
          <p v-if="record.comment" style="color: #909399; font-size: 12px;">备注: {{ record.comment }}</p>
        </el-timeline-item>
        <el-timeline-item timestamp="待处理" type="primary">
          <p>等待 {{ detail.currentLevelName }} 审批</p>
        </el-timeline-item>
      </el-timeline>

      <template #footer>
        <el-input v-model="approvalComment" placeholder="审批意见（可选）" style="margin-bottom: 12px;" />
        <el-button type="success" @click="submitApprove">通过</el-button>
        <el-button type="danger" @click="submitReject">驳回</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const approvedThisMonth = ref(48)
const showDetail = ref(false)
const approvalComment = ref('')

const pendingList = ref([
  { id: 'a1', requestNo: 'SP-20260627-001', approvalTypeName: '提前解约', submitterName: '张三', businessSummary: 'B栋-502 提前解约，押金¥8,000', amount: 8000, currentLevel: 1, currentLevelName: '运营主管', reason: '租客工作调动需要搬家', submittedAt: '2026-06-27 10:30:00' },
  { id: 'a2', requestNo: 'SP-20260626-002', approvalTypeName: '批量导入房屋', submitterName: '李四', businessSummary: '导入C栋12套新房源', amount: 0, currentLevel: 2, currentLevelName: '部门经理', reason: '新收楼C栋房源录入', submittedAt: '2026-06-26 14:00:00' },
  { id: 'a3', requestNo: 'SP-20260625-003', approvalTypeName: '收款冲销', submitterName: '王五', businessSummary: '冲销收款SJ-20260624-001', amount: 5200, currentLevel: 1, currentLevelName: '财务主管', reason: '收款金额有误需冲销重做', submittedAt: '2026-06-25 09:15:00' },
  { id: 'a4', requestNo: 'SP-20260624-004', approvalTypeName: '应收减免', submitterName: '赵六', businessSummary: 'A-301 6月管理费减免', amount: 150, currentLevel: 1, currentLevelName: '运营主管', reason: '租客反映管理服务不到位', submittedAt: '2026-06-24 16:00:00' }
])

const detail = ref({})
const approvalRecords = ref([])

function handleApprove(row) {
  ElMessageBox.confirm(`确定通过 ${row.requestNo} 的审批吗？`, '确认').then(() => {
    ElMessage.success(`已通过 - 第${row.currentLevel}级审批完成`)
  }).catch(() => {})
}

function handleReject(row) {
  ElMessageBox.confirm(`确定驳回 ${row.requestNo} 吗？`, '提示').then(() => {
    ElMessage.success('已驳回')
  }).catch(() => {})
}

function viewDetail(row) {
  detail.value = row
  approvalRecords.value = [
    { operatorName: row.submitterName, action: 'Approve', comment: '', operatedAt: row.submittedAt }
  ]
  showDetail.value = true
}

function submitApprove() {
  ElMessage.success('已通过')
  showDetail.value = false
}

function submitReject() {
  ElMessage.success('已驳回')
  showDetail.value = false
}
</script>
