<template>
  <div>
    <div class="page-header">
      <h2>收款登记</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-card>
      <template #header>选择应收账单</template>
      <div class="search-bar">
        <el-input v-model="search.keyword" placeholder="合同号/租客" clearable style="width: 200px;" />
        <el-button type="primary">查询</el-button>
      </div>

      <el-table :data="receivableList" stripe @selection-change="onSelectionChange">
        <el-table-column type="selection" width="50" />
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="tenantName" label="租客" width="100" />
        <el-table-column prop="period" label="账期" width="80" />
        <el-table-column prop="dueDate" label="到期日" width="100" />
        <el-table-column prop="totalAmount" label="应收金额" width="110">
          <template #default="{ row }">¥{{ row.totalAmount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="totalReceived" label="已收金额" width="100">
          <template #default="{ row }">¥{{ row.totalReceived?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="待收金额" width="100">
          <template #default="{ row }">¥{{ (row.totalAmount - row.totalReceived)?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column type="expand" width="50">
          <template #default="{ row }">
            <el-table :data="row.details" size="small">
              <el-table-column prop="feeName" label="费用项目" />
              <el-table-column prop="amount" label="金额">
                <template #default="{ row: d }">¥{{ d.amount }}</template>
              </el-table-column>
              <el-table-column prop="received" label="已收">
                <template #default="{ row: d }">¥{{ d.received }}</template>
              </el-table-column>
              <el-table-column prop="pending" label="待收">
                <template #default="{ row: d }">¥{{ d.amount - d.received }}</template>
              </el-table-column>
            </el-table>
          </template>
        </el-table-column>
      </el-table>

      <div style="margin-top: 16px;">
        <el-divider />
        <h3>收款信息</h3>
        <el-form :model="receiptForm" label-width="120px">
          <el-form-item label="收款总金额">
            <el-input-number v-model="receiptForm.amount" :min="0" :precision="2" style="width: 200px;" />
            <span style="margin-left: 8px; color: #909399;">选择应收合计: ¥{{ selectedTotal }}</span>
          </el-form-item>
          <el-form-item label="支付通道">
            <el-select v-model="receiptForm.channelId" style="width: 200px;">
              <el-option label="银行转账" value="bank" />
              <el-option label="支付宝" value="alipay" />
              <el-option label="微信支付" value="wechat" />
              <el-option label="现金" value="cash" />
            </el-select>
          </el-form-item>
          <el-form-item label="付款人">
            <el-input v-model="receiptForm.remitterName" style="width: 200px;" />
          </el-form-item>
          <el-form-item label="银行交易号">
            <el-input v-model="receiptForm.transactionRef" style="width: 300px;" />
          </el-form-item>
          <el-form-item label="收款日期">
            <el-date-picker v-model="receiptForm.receivedDate" type="date" />
          </el-form-item>
        </el-form>
      </div>

      <div style="text-align: center; margin-top: 20px;">
        <el-button type="primary" size="large" @click="submitReceipt" :disabled="!receiptForm.amount">
          登记收款
        </el-button>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const router = useRouter()
const search = reactive({ keyword: '' })
const selectedRows = ref([])

const receivableList = ref([
  { id: 'r1', contractNo: 'HT-2026-001', tenantName: '张三', period: '2026-06', dueDate: '2026-06-05', totalAmount: 5460, totalReceived: 0, details: [
    { feeName: '房租费', amount: 5200, received: 0 }, { feeName: '管理费', amount: 150, received: 0 }, { feeName: '网费', amount: 80, received: 0 }
  ]},
  { id: 'r2', contractNo: 'HT-2026-002', tenantName: '李四', period: '2026-06', dueDate: '2026-06-05', totalAmount: 4030, totalReceived: 4000, details: [
    { feeName: '房租费', amount: 3800, received: 3800 }, { feeName: '管理费', amount: 150, received: 150 }
  ]}
])

const receiptForm = reactive({
  amount: 0, channelId: 'bank', remitterName: '', transactionRef: '', receivedDate: new Date()
})

const selectedTotal = computed(() => {
  return selectedRows.value.reduce((sum, row) => sum + (row.totalAmount - row.totalReceived), 0)
})

function onSelectionChange(rows) { selectedRows.value = rows }

function submitReceipt() {
  ElMessage.success('收款登记成功，等待财务确认')
  router.push('/receipts')
}
</script>
