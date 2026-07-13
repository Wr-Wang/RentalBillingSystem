<template>
  <div>
    <div class="page-header">
      <h2>会计期间</h2>
      <el-button type="primary" @click="showOpenDialog = true">开启新账期</el-button>
    </div>

    <el-table :data="periods" v-loading="loading" stripe>
      <el-table-column prop="period" label="会计期间" width="160" />
      <el-table-column label="状态" width="120">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Open' ? 'success' : row.status === 'Closed' ? 'warning' : 'danger'" size="small">
            {{ row.status === 'Open' ? '开启' : row.status === 'Closed' ? '已结账' : '已锁定' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="openedAt" label="开启时间" width="180" />
      <el-table-column prop="closedAt" label="结账时间" width="180" />
      <el-table-column label="操作" width="250" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="success" v-if="row.status === 'Open'" :loading="row._closing" @click="handleClose(row)">结账</el-button>
          <el-button text size="small" type="primary" v-if="row.status === 'Closed'" :loading="row._reopening" @click="handleReopen(row)">反结账</el-button>
          <el-button text size="small" type="danger" v-if="row.status === 'Closed'" :loading="row._locking" @click="handleLock(row)">锁定</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 开启新账期对话框 -->
    <el-dialog v-model="showOpenDialog" title="开启新账期" width="400px">
      <el-form :model="openForm">
        <el-form-item label="年份">
          <el-date-picker v-model="openForm.month" type="month" placeholder="选择月份" value-format="YYYY-MM" style="width:100%;" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showOpenDialog = false">取消</el-button>
        <el-button type="primary" :loading="opening" @click="handleOpen">确认开启</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getAccountingPeriods, openAccountingPeriod, closeAccountingPeriod, reopenAccountingPeriod, lockAccountingPeriod } from '@/api'

const loading = ref(false)
const periods = ref([])
const showOpenDialog = ref(false)
const opening = ref(false)
const openForm = ref({ month: '' })

async function fetchPeriods() {
  loading.value = true
  try {
    const res = await getAccountingPeriods()
    periods.value = (res || []).map(p => ({
      id: p.id,
      period: p.period || '',
      status: p.status || 'Open',
      openedAt: p.openedAt || '',
      closedAt: p.closedAt || '',
      _closing: false,
      _reopening: false,
      _locking: false
    }))
  } catch { ElMessage.error('加载会计期间失败') }
  finally { loading.value = false }
}

async function handleOpen() {
  if (!openForm.value.month) { ElMessage.warning('请选择月份'); return }
  opening.value = true
  try {
    await openAccountingPeriod({ period: openForm.value.month })
    ElMessage.success('账期已开启')
    showOpenDialog.value = false
    openForm.value.month = ''
    await fetchPeriods()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || e?.message || '开启失败')
  }
  opening.value = false
}

async function handleClose(row) {
  try {
    await ElMessageBox.confirm(`确定结账「${row.period}」吗？结账后将不能生成该期间的凭证。`, '确认')
    row._closing = true
    await closeAccountingPeriod(row.id)
    ElMessage.success('账期已结账')
    await fetchPeriods()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('结账失败')
  }
  row._closing = false
}

async function handleReopen(row) {
  try {
    await ElMessageBox.confirm(`确定反结账「${row.period}」吗？`, '确认')
    row._reopening = true
    await reopenAccountingPeriod(row.id)
    ElMessage.success('账期已反结账')
    await fetchPeriods()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('反结账失败')
  }
  row._reopening = false
}

async function handleLock(row) {
  try {
    await ElMessageBox.confirm(`确定锁定「${row.period}」吗？锁定后将不可反结账。`, '确认')
    row._locking = true
    await lockAccountingPeriod(row.id)
    ElMessage.success('账期已锁定')
    await fetchPeriods()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('锁定失败')
  }
  row._locking = false
}

onMounted(fetchPeriods)
</script>
