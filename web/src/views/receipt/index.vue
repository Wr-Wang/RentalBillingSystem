<template>
  <div>
    <div class="page-header">
      <h2>收款管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="drawerVisible = true"><el-icon><Plus /></el-icon>收款登记</el-button>
        <el-button @click="$router.push('/receipts/confirm')"><el-icon><Select /></el-icon>待确认({{ pendingCount }})</el-button>
      </div>
    </div>

    <el-drawer v-model="drawerVisible" title="收款登记" direction="rtl" size="400px">
      <el-form :model="form" label-position="top">
        <el-form-item label="收款金额"><el-input-number v-model="form.amount" :min="0.01" :precision="2" style="width:100%;" /></el-form-item>
        <el-form-item label="收款日期"><el-date-picker v-model="form.receivedDate" type="date" style="width:100%;" /></el-form-item>
        <el-form-item label="关联合同 ID"><el-input v-model="form.contractId" placeholder="选填" /></el-form-item>
        <el-form-item label="收款单号"><el-input v-model="form.receiptNo" placeholder="自动生成" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="drawerVisible = false">取消</el-button>
        <el-button type="primary" @click="submit" :loading="submitting">登记</el-button>
      </template>
    </el-drawer>

    <el-tabs v-model="activeTab">
      <el-tab-pane label="收款记录" name="receipts">
        <el-table :data="receiptList" stripe v-loading="loading" style="width:100%;">
          <el-table-column prop="receiptNo" label="收据号" min-width="160" />
          <el-table-column prop="amount" label="金额" width="130" align="right">
            <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="receivedDate" label="收款日期" width="110" />
          <el-table-column prop="status" label="状态" width="100">
            <template #default="{ row }">
              <el-tag :type="row.status === 'Confirmed' ? 'success' : row.status === 'Pending' ? 'warning' : 'danger'" size="small" style="width:64px;text-align:center;">
                {{ {Confirmed:'已确认',Pending:'待确认',Rejected:'已驳回',Cancelled:'已取消'}[row.status] || row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="180" fixed="right">
            <template #default="{ row }">
              <el-button text size="small" type="success" v-if="row.status === 'Pending'" @click="confirmReceipt(row)">确认</el-button>
              <el-button text size="small" type="danger" v-if="row.status === 'Confirmed'" @click="reverseReceipt(row)">冲销</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>

      <el-tab-pane label="押金管理" name="deposits">
        <el-table :data="depositList" stripe v-loading="depositLoading" style="width:100%;">
          <el-table-column prop="contractId" label="合同 ID" min-width="200" />
          <el-table-column prop="action" label="类型" width="90">
            <template #default="{ row }">{{ {Create:'创建',Return:'退还',Deduct:'扣款'}[row.action] || row.action }}</template>
          </el-table-column>
          <el-table-column prop="amount" label="变动金额" width="120" align="right">
            <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="balance" label="余额" width="120" align="right">
            <template #default="{ row }">¥{{ (row.balance || 0).toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="createdAt" label="时间" width="170" />
          <el-table-column prop="remark" label="备注" min-width="150" />
        </el-table>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { getReceipts, getDeposits, createReceipt, confirmReceipt as apiConfirm, reverseReceipt as apiReverse } from '@/api'
import { ElMessage, ElMessageBox } from 'element-plus'

const activeTab = ref('receipts')
const loading = ref(false)
const depositLoading = ref(false)
const receiptList = ref([])
const depositList = ref([])
const pendingCount = computed(() => receiptList.value.filter(r => r.status === 'Pending').length)

// Drawer
const drawerVisible = ref(false)
const form = reactive({ receiptNo: '', amount: 0, receivedDate: '', contractId: '' })
const submitting = ref(false)
async function submit() {
  if (form.amount <= 0) { ElMessage.warning('请输入金额'); return }
  submitting.value = true
  try {
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    await createReceipt({ receiptNo: form.receiptNo || `RC-${Date.now()}`, amount: form.amount, receivedDate: form.receivedDate || new Date().toISOString().slice(0, 10), companyId: user.defaultCompanyId, contractId: form.contractId || undefined })
    ElMessage.success('登记成功')
    drawerVisible.value = false
    form.amount = 0; form.receiptNo = ''; form.receivedDate = ''; form.contractId = ''
    await loadReceipts()
  } catch (e) { ElMessage.error(e?.response?.data?.message || '登记失败') }
  finally { submitting.value = false }
}

async function loadReceipts() {
  loading.value = true
  try { receiptList.value = await getReceipts({}) || [] } catch {}
  finally { loading.value = false }
}
async function loadDeposits() {
  depositLoading.value = true
  try { depositList.value = await getDeposits({}) || [] } catch {}
  finally { depositLoading.value = false }
}
async function confirmReceipt(row) {
  try {
    await ElMessageBox.confirm(`确认收款 ${row.receiptNo} 金额 ¥${(row.amount || 0).toLocaleString()} 到账？`, '确认')
    await apiConfirm(row.id)
    ElMessage.success('收款已确认')
    await loadReceipts()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '确认失败') }
}
async function reverseReceipt(row) {
  try {
    await ElMessageBox.confirm(`确定冲销收据 ${row.receiptNo} 吗？`, '提示')
    await apiReverse(row.id, { reason: '手动冲销' })
    ElMessage.success('冲销成功')
    await loadReceipts()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '冲销失败') }
}
onMounted(() => { loadReceipts(); loadDeposits() })
</script>
