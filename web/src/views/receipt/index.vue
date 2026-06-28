<template>
  <div>
    <div class="page-header">
      <h2>收款管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="$router.push('/receipts/register')">
          <el-icon><Plus /></el-icon>收款登记
        </el-button>
        <el-button @click="$router.push('/receipts/confirm')">
          <el-icon><Select /></el-icon>待确认({{ pendingCount }})
        </el-button>
      </div>
    </div>

    <!-- Tabs -->
    <el-tabs v-model="activeTab">
      <el-tab-pane label="应收账单" name="receivables">
        <div class="search-bar">
          <el-input v-model="search.keyword" placeholder="合同号/租客" clearable style="width: 200px;" />
          <el-select v-model="search.status" placeholder="状态" clearable style="width: 140px;">
            <el-option label="待收款" value="Pending" />
            <el-option label="部分已收" value="Partial" />
            <el-option label="已付清" value="Paid" />
          </el-select>
          <el-date-picker v-model="search.month" type="month" placeholder="账期" style="width: 140px;" />
          <el-button type="primary">查询</el-button>
          <el-button @click="resetSearch">重置</el-button>
          <el-button @click="generateAll">手动生成应收</el-button>
        </div>

        <el-table :data="receivableList" stripe default-expand-all row-key="id">
          <el-table-column type="selection" width="50" />
          <el-table-column prop="contractNo" label="合同号" width="130" />
          <el-table-column prop="tenantName" label="租客" width="100" />
          <el-table-column prop="period" label="账期" width="80" />
          <el-table-column prop="dueDate" label="到期日" width="90" />
          <el-table-column prop="totalAmount" label="应收总额" width="110">
            <template #default="{ row }">¥{{ row.totalAmount?.toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="totalReceived" label="已收" width="100">
            <template #default="{ row }">¥{{ row.totalReceived?.toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="status" label="状态" width="90">
            <template #default="{ row }">
              <el-tag :type="row.status === 'Paid' ? 'success' : row.status === 'Partial' ? 'warning' : 'danger'" size="small">
                {{ row.status === 'Paid' ? '已付清' : row.status === 'Partial' ? '部分' : '待收款' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column type="expand" width="50">
            <template #default="{ row }">
              <el-table :data="row.details" size="small">
                <el-table-column prop="feeName" label="费用项目" width="120" />
                <el-table-column prop="amount" label="金额" width="100">
                  <template #default="{ row: d }">¥{{ d.amount?.toLocaleString() }}</template>
                </el-table-column>
                <el-table-column prop="received" label="已收" width="100">
                  <template #default="{ row: d }">¥{{ d.received?.toLocaleString() }}</template>
                </el-table-column>
                <el-table-column prop="description" label="说明" min-width="200" />
              </el-table>
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>

      <el-tab-pane label="收款记录" name="receipts">
        <el-table :data="receiptList" stripe>
          <el-table-column prop="receiptNo" label="收据号" width="160" />
          <el-table-column prop="contractNo" label="合同号" width="120" />
          <el-table-column prop="amount" label="金额" width="110">
            <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="channelName" label="支付通道" width="100" />
          <el-table-column prop="remitterName" label="付款人" width="100" />
          <el-table-column prop="status" label="状态" width="100">
            <template #default="{ row }">
              <el-tag :type="row.status === 'Confirmed' ? 'success' : row.status === 'PendingConfirm' ? 'warning' : 'danger'" size="small">
                {{ row.status === 'Confirmed' ? '已确认' : row.status === 'PendingConfirm' ? '待确认' : row.status === 'Reversed' ? '已冲销' : '已退款' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="receivedDate" label="收款日期" width="100" />
          <el-table-column label="操作" width="160" fixed="right">
            <template #default="{ row }">
              <el-button text size="small" type="primary" @click="viewReceipt(row)">查看</el-button>
              <el-button text size="small" type="success" v-if="row.status === 'PendingConfirm'" @click="confirmReceipt(row)">确认</el-button>
              <el-button text size="small" type="danger" v-if="row.status === 'Confirmed'" @click="reverseReceipt(row)">冲销</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>

      <el-tab-pane label="押金管理" name="deposits">
        <el-table :data="depositList" stripe>
          <el-table-column prop="contractNo" label="合同号" width="120" />
          <el-table-column prop="tenantName" label="租客" width="100" />
          <el-table-column prop="depositAmount" label="押金金额" width="120">
            <template #default="{ row }">¥{{ row.depositAmount?.toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="currentBalance" label="当前余额" width="120">
            <template #default="{ row }">¥{{ row.currentBalance?.toLocaleString() }}</template>
          </el-table-column>
          <el-table-column label="操作" width="180">
            <template #default="{ row }">
              <el-button text size="small" type="primary" @click="refundDeposit(row)">退还</el-button>
              <el-button text size="small" type="warning" @click="deductDeposit(row)">扣款</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const activeTab = ref('receivables')
const pendingCount = ref(5)

const search = reactive({ keyword: '', status: '', month: null })

// Receivable list with expandable details
const receivableList = ref([
  { id: 'r1', contractNo: 'HT-2026-001', tenantName: '张三', period: '2026-06', dueDate: '2026-06-05', totalAmount: 5460, totalReceived: 0, status: 'Pending', details: [
    { feeName: '房租费', amount: 5200, received: 0, description: '月租金' },
    { feeName: '卫生费', amount: 30, received: 0, description: '月固定' },
    { feeName: '管理费', amount: 150, received: 0, description: '月固定' },
    { feeName: '网费', amount: 80, received: 0, description: '月固定' }
  ]},
  { id: 'r2', contractNo: 'HT-2026-002', tenantName: '李四', period: '2026-06', dueDate: '2026-06-05', totalAmount: 4030, totalReceived: 4000, status: 'Partial', details: [
    { feeName: '房租费', amount: 3800, received: 3800, description: '月租金' },
    { feeName: '卫生费', amount: 30, received: 200, description: '月固定' },
    { feeName: '管理费', amount: 150, received: 0, description: '月固定' },
    { feeName: '网费', amount: 80, received: 0, description: '月固定' }
  ]},
  { id: 'r3', contractNo: 'HT-2026-003', tenantName: '王五', period: '2026-06', dueDate: '2026-06-05', totalAmount: 7030, totalReceived: 7030, status: 'Paid', details: [
    { feeName: '房租费', amount: 6800, received: 6800, description: '月租金' },
    { feeName: '水费', amount: 100, received: 100, description: '15吨×6元/吨' },
    { feeName: '电费', amount: 80, received: 80, description: '100度×0.8元/度' },
    { feeName: '管理费', amount: 150, received: 150, description: '月固定' },
  ]}
])

const receiptList = ref([
  { id: 'rc1', receiptNo: 'SJ-20260627-001', contractNo: 'HT-2026-001', amount: 5460, channelName: '银行转账', remitterName: '张三', status: 'Confirmed', receivedDate: '2026-06-27' },
  { id: 'rc2', receiptNo: 'SJ-20260626-002', contractNo: 'HT-2026-002', amount: 4000, channelName: '支付宝', remitterName: '李四', status: 'PendingConfirm', receivedDate: '2026-06-26' },
  { id: 'rc3', receiptNo: 'SJ-20260625-003', contractNo: 'HT-2026-003', amount: 7030, channelName: '银行转账', remitterName: '王五', status: 'Confirmed', receivedDate: '2026-06-25' }
])

const depositList = ref([
  { contractNo: 'HT-2026-001', tenantName: '张三', depositAmount: 10400, currentBalance: 10400 },
  { contractNo: 'HT-2026-002', tenantName: '李四', depositAmount: 7600, currentBalance: 7600 },
  { contractNo: 'HT-2026-003', tenantName: '王五', depositAmount: 13600, currentBalance: 13600 }
])

function resetSearch() { search.keyword = ''; search.status = ''; search.month = null }
function generateAll() { ElMessage.success('应收已全部生成') }

function viewReceipt(row) { ElMessage.info('查看收据详情') }
function confirmReceipt(row) {
  ElMessageBox.confirm(`确认收款 ${row.receiptNo} 金额 ¥${row.amount?.toLocaleString()} 到账？`, '确认').then(() => {
    row.status = 'Confirmed'
    ElMessage.success('收款已确认')
  }).catch(() => {})
}
function reverseReceipt(row) {
  ElMessageBox.confirm(`确定冲销收据 ${row.receiptNo} 吗？此操作需要审批。`, '提示').then(() => {
    ElMessage.success('冲销申请已提交审批')
  }).catch(() => {})
}
function refundDeposit(row) { ElMessage.info('退还押金功能待实现') }
function deductDeposit(row) { ElMessage.info('押金扣款功能待实现') }
</script>
