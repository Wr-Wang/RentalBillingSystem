<template>
  <div>
    <div class="page-header">
      <h2>凭证管理</h2>
    </div>

    <el-table :data="vouchers" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="voucherNo" label="凭证号" width="160" />
      <el-table-column prop="voucherDate" label="日期" width="100" />
      <el-table-column prop="totalDebit" label="借方合计" width="120">
        <template #default="{ row }">¥{{ row.totalDebit?.toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="totalCredit" label="贷方合计" width="120">
        <template #default="{ row }">¥{{ row.totalCredit?.toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="sourceType" label="来源" width="100" />
      <el-table-column prop="status" label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Posted' ? 'success' : row.status === 'Reversed' ? 'danger' : 'info'" size="small">
            {{ row.status === 'Draft' ? '草稿' : row.status === 'Posted' ? '已过账' : '已冲销' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="160" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="viewVoucher(row)">查看</el-button>
          <el-button text size="small" type="success" v-if="row.status === 'Draft'" @click="postVoucher(row)">过账</el-button>
          <el-button text size="small" type="danger" v-if="row.status === 'Posted'" @click="reverseVoucher(row)">冲销</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" layout="total, sizes, prev, pager, next" />
    </div>

    <!-- Voucher Detail Dialog -->
    <el-dialog v-model="showVoucherDetail" title="凭证详情" width="700px">
      <template #default>
        <el-descriptions :column="3" border style="margin-bottom: 16px;">
          <el-descriptions-item label="凭证号">{{ voucherDetail.voucherNo }}</el-descriptions-item>
          <el-descriptions-item label="日期">{{ voucherDetail.voucherDate }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="voucherDetail.status === 'Posted' ? 'success' : 'info'" size="small">{{ voucherDetail.status === 'Posted' ? '已过账' : '草稿' }}</el-tag>
          </el-descriptions-item>
        </el-descriptions>

        <el-table :data="voucherDetail.entries" stripe>
          <el-table-column type="index" label="行号" width="60" />
          <el-table-column prop="subjectCode" label="科目编码" width="100" />
          <el-table-column prop="subjectName" label="科目名称" width="180" />
          <el-table-column prop="summary" label="摘要" min-width="150" />
          <el-table-column prop="debitAmount" label="借方金额" width="120">
            <template #default="{ row }">{{ row.debitAmount ? '¥' + row.debitAmount?.toLocaleString() : '-' }}</template>
          </el-table-column>
          <el-table-column prop="creditAmount" label="贷方金额" width="120">
            <template #default="{ row }">{{ row.creditAmount ? '¥' + row.creditAmount?.toLocaleString() : '-' }}</template>
          </el-table-column>
        </el-table>
        <div style="margin-top: 8px; text-align: right; font-weight: bold;">
          借方合计: ¥{{ entriesDebitTotal.toLocaleString() }} | 贷方合计: ¥{{ entriesCreditTotal.toLocaleString() }}
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const pagination = reactive({ page: 1, pageSize: 10, total: 50 })
const showVoucherDetail = ref(false)

const vouchers = ref([
  { id: 'v1', voucherNo: 'PJZ-20260627-001', voucherDate: '2026-06-27', totalDebit: 5460, totalCredit: 5460, sourceType: '收款', status: 'Posted' },
  { id: 'v2', voucherNo: 'PJZ-20260627-002', voucherDate: '2026-06-27', totalDebit: 5200, totalCredit: 5200, sourceType: '收款', status: 'Posted' },
  { id: 'v3', voucherNo: 'PJZ-20260626-001', voucherDate: '2026-06-26', totalDebit: 4030, totalCredit: 4030, sourceType: '收款', status: 'Draft' }
])

const voucherDetail = ref({
  voucherNo: '', voucherDate: '', status: '', entries: []
})

const entriesDebitTotal = computed(() => voucherDetail.value.entries.reduce((s, e) => s + (e.debitAmount || 0), 0))
const entriesCreditTotal = computed(() => voucherDetail.value.entries.reduce((s, e) => s + (e.creditAmount || 0), 0))

function viewVoucher(row) {
  voucherDetail.value = {
    ...row,
    entries: [
      { subjectCode: '1002', subjectName: '银行存款', summary: '收到租金', debitAmount: null, creditAmount: row.totalDebit },
      { subjectCode: '112201', subjectName: '应收账款-房租', summary: '冲减应收', debitAmount: row.totalDebit, creditAmount: null }
    ]
  }
  showVoucherDetail.value = true
}

function postVoucher(row) {
  ElMessageBox.confirm(`确定过账凭证 ${row.voucherNo} 吗？`, '确认').then(() => {
    row.status = 'Posted'
    ElMessage.success('凭证已过账')
  }).catch(() => {})
}

function reverseVoucher(row) {
  ElMessageBox.confirm(`确定冲销凭证 ${row.voucherNo} 吗？此操作需要审批。`, '提示').then(() => {
    ElMessage.success('冲销申请已提交审批')
  }).catch(() => {})
}
</script>
