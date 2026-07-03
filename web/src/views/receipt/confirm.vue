<template>
  <div>
    <div class="page-header">
      <h2>收款确认</h2>
      <el-button @click="$router.push('/receipts')">返回列表</el-button>
    </div>
    <el-table :data="receipts" stripe v-loading="loading" style="width:100%;">
      <el-table-column prop="receiptNo" label="收款单号" min-width="180" />
      <el-table-column prop="amount" label="金额" width="130" align="right">
        <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="receivedDate" label="收款日期" width="110" />
      <el-table-column prop="referenceNo" label="参考号" min-width="150" />
      <el-table-column prop="status" label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Confirmed' ? 'success' : row.status === 'Rejected' ? 'danger' : 'warning'" size="small" style="width:64px;text-align:center;">
            {{ {Pending:'待确认',Confirmed:'已确认',Rejected:'已驳回'}[row.status] || row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="170" fixed="right">
        <template #default="{ row }">
          <el-button size="small" type="primary" :disabled="row.status !== 'Pending'" @click="confirm(row.id)">确认到账</el-button>
          <el-button size="small" type="danger" :disabled="row.status !== 'Pending'" @click="reject(row.id)">驳回</el-button>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue'
import { getReceipts, confirmReceipt as apiConfirm, rejectReceipt as apiReject } from '@/api'
import { ElMessage } from 'element-plus'
const loading = ref(false)
const receipts = ref([])
async function loadData() {
  loading.value = true
  try {
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const res = await getReceipts({ companyId: user.defaultCompanyId })
    receipts.value = res
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}
async function confirm(id) {
  try { await apiConfirm(id); ElMessage.success('已确认'); await loadData() }
  catch (e) { ElMessage.error(e?.response?.data?.message || '确认失败') }
}
async function reject(id) {
  try { await apiReject(id, { reason: '驳回' }); ElMessage.success('已驳回'); await loadData() }
  catch (e) { ElMessage.error(e?.response?.data?.message || '驳回失败') }
}
onMounted(loadData)
</script>
