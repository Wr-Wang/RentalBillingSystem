<template>
  <div>
    <div class="page-header">
      <h2>收款确认</h2>
      <el-button @click="$router.push('/receipts')">返回列表</el-button>
    </div>

    <el-card shadow="never" class="search-bar">
      <el-form inline>
        <el-date-picker v-model="dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width:240px;" @change="loadData" />
        <el-button type="primary" @click="loadData">刷新</el-button>
      </el-form>
    </el-card>

    <el-table :data="receipts" stripe v-loading="loading" style="width:100%;">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="receiptNo" label="收款单号" min-width="180" />
      <el-table-column label="金额" width="130" align="right">
        <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="receivedDate" label="收款日期" width="110" />
      <el-table-column prop="referenceNo" label="参考号" min-width="150" />
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Confirmed' ? 'success' : row.status === 'Rejected' ? 'danger' : 'warning'" size="small" style="width:64px;text-align:center;">
            {{ {Pending:'待确认',Confirmed:'已确认',Rejected:'已驳回'}[row.status] || row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button size="small" type="primary" :disabled="row.status !== 'Pending'" @click="confirm(row)">
            确认到账
          </el-button>
          <el-button size="small" type="danger" :disabled="row.status !== 'Pending'" @click="reject(row)">
            驳回
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top:16px;text-align:right;">
      <el-pagination
        v-model:page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @current-change="loadData"
        @size-change="loadData"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useUserStore } from '@/store/user'
import { getReceipts, confirmReceipt as apiConfirm, rejectReceipt as apiReject } from '@/api'
import { ElMessage, ElMessageBox } from 'element-plus'

const userStore = useUserStore()
const loading = ref(false)
const receipts = ref([])
const dateRange = ref(null)
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)

function getEffectiveCompanyId() {
  return userStore.effectiveCompanyId || userStore.companyId
}

async function loadData() {
  const companyId = getEffectiveCompanyId()
  if (!companyId) { receipts.value = []; total.value = 0; return }

  loading.value = true
  try {
    // 后端 GET /receipts?companyId=&status=Pending 只返回待确认的
    const res = await getReceipts({ companyId, status: undefined })  // status 空 = Pending（后端默认）
    let items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    // 日期过滤（前端）
    if (dateRange.value) {
      const [start, end] = dateRange.value.map(d => d.toISOString().slice(0, 10))
      items = items.filter(r => r.receivedDate >= start && r.receivedDate <= end)
    }
    total.value = items.length
    const start = (page.value - 1) * pageSize.value
    receipts.value = items.slice(start, start + pageSize.value)
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}

async function confirm(row) {
  try {
    await apiConfirm(row.id)
    ElMessage.success('已确认')
    await loadData()
  } catch (e) { ElMessage.error(e?.response?.data?.message || '确认失败') }
}

async function reject(row) {
  try {
    const { value } = await ElMessageBox.prompt(`驳回原因（${row.receiptNo}）：`, '驳回', {
      confirmButtonText: '确定', cancelButtonText: '取消', inputPlaceholder: '请输入驳回原因'
    })
    if (!value) { ElMessage.warning('请输入驳回原因'); return }
    await apiReject(row.id, { reason: value })
    ElMessage.success('已驳回')
    await loadData()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '驳回失败') }
}

onMounted(loadData)
</script>

<style scoped>
.search-bar {
  margin-bottom: 16px;
}
</style>
