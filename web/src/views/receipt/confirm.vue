<template>
  <div>
    <div class="page-header">
      <h2>收款确认</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <div class="stat-cards">
      <div class="stat-card" style="border-left: 4px solid #e6a23c;">
        <div class="label">待确认收款</div>
        <div class="value" style="color: #e6a23c;">{{ pendingList.length }} 笔</div>
        <div class="sub">合计 ¥{{ pendingTotal.toLocaleString() }}</div>
      </div>
    </div>

    <el-card>
      <template #header>待确认列表</template>
      <el-table :data="pendingList" stripe>
        <el-table-column type="selection" width="50" />
        <el-table-column prop="receiptNo" label="收据号" width="160" />
        <el-table-column prop="contractNo" label="合同号" width="120" />
        <el-table-column prop="remitterName" label="付款人" width="100" />
        <el-table-column prop="amount" label="金额" width="110">
          <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="channelName" label="支付通道" width="100" />
        <el-table-column prop="transactionRef" label="交易号" width="150" />
        <el-table-column prop="receivedDate" label="收款日期" width="100" />
      </el-table>
      <div style="margin-top: 16px; text-align: center;">
        <el-button type="success" size="large" @click="batchConfirm">
          <el-icon><Select /></el-icon>确认到账
        </el-button>
        <el-button type="danger" size="large" @click="batchReject">
          <el-icon><Close /></el-icon>驳回
        </el-button>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()

const pendingList = ref([
  { id: 'rc1', receiptNo: 'SJ-20260627-001', contractNo: 'HT-2026-001', remitterName: '张三', amount: 5460, channelName: '银行转账', transactionRef: 'BANK20260627001', receivedDate: '2026-06-27' },
  { id: 'rc2', receiptNo: 'SJ-20260627-002', contractNo: 'HT-2026-005', remitterName: '孙七', amount: 4500, channelName: '支付宝', transactionRef: 'ALI20260627002', receivedDate: '2026-06-27' },
  { id: 'rc3', receiptNo: 'SJ-20260626-003', contractNo: 'HT-2026-008', remitterName: '周八', amount: 8200, channelName: '银行转账', transactionRef: 'BANK20260626003', receivedDate: '2026-06-26' }
])

const pendingTotal = computed(() => pendingList.value.reduce((s, r) => s + r.amount, 0))

function batchConfirm() {
  ElMessageBox.confirm('确认所选收款已到账？', '确认').then(() => {
    ElMessage.success('收款已确认，会计凭证已自动生成')
  }).catch(() => {})
}

function batchReject() {
  ElMessageBox.confirm('确定驳回所选收款？', '提示').then(() => {
    ElMessage.success('已驳回')
  }).catch(() => {})
}
</script>
