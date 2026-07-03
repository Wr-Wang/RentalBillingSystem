<template>
  <div>
    <div class="page-header">
      <h2>费用收入统计</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>
    <div class="search-bar">
      <el-date-picker v-model="searchPeriod" type="month" placeholder="选择账期" @change="loadData" />
      <el-button type="primary" @click="loadData">查询</el-button>
    </div>

    <el-card>
      <table class="fee-table">
        <thead>
          <tr>
            <th>收费项目</th>
            <th>本期应收</th>
            <th>本期实收</th>
            <th>本期欠费</th>
            <th>收租率</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in feeStats" :key="row.name">
            <td>{{ row.name }}</td>
            <td>¥{{ row.receivable.toLocaleString() }}</td>
            <td>¥{{ row.received.toLocaleString() }}</td>
            <td>¥{{ row.overdue.toLocaleString() }}</td>
            <td>
              <el-progress :percentage="row.rate" :color="row.rate > 90 ? '#67c23a' : row.rate > 70 ? '#e6a23c' : '#f56c6c'" />
            </td>
          </tr>
          <tr class="total-row">
            <td><strong>合计</strong></td>
            <td><strong>¥{{ total.receivable.toLocaleString() }}</strong></td>
            <td><strong>¥{{ total.received.toLocaleString() }}</strong></td>
            <td><strong>¥{{ total.overdue.toLocaleString() }}</strong></td>
            <td><el-progress :percentage="total.rate" color="#409eff" /></td>
          </tr>
        </tbody>
      </table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getFeeRevenue } from '../../api/index'

const searchPeriod = ref(null)
const feeStats = ref([])
const loading = ref(false)

const total = computed(() => {
  const list = feeStats.value
  const receivable = list.reduce((s, r) => s + (r.receivable || 0), 0)
  const received = list.reduce((s, r) => s + (r.received || 0), 0)
  const overdue = receivable - received
  return { receivable, received, overdue, rate: receivable > 0 ? Math.round(received / receivable * 100 * 10) / 10 : 0 }
})

async function loadData() {
  loading.value = true
  try {
    let period
    if (searchPeriod.value) {
      const d = new Date(searchPeriod.value)
      period = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
    }
    const res = await getFeeRevenue({ period })
    const items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    if (Array.isArray(items)) {
      feeStats.value = items.map(item => ({
        name: item.feeName || item.name || '未知',
        receivable: item.totalAmount || item.receivable || 0,
        received: item.totalReceived || item.received || 0,
        overdue: (item.totalAmount || 0) - (item.totalReceived || 0),
        rate: (item.totalAmount || 0) > 0
          ? Math.round((item.totalReceived || 0) / (item.totalAmount || 1) * 100 * 10) / 10
          : 0
      }))
    }
  } catch {
    ElMessage.error('加载费用收入统计失败')
  }
  loading.value = false
}

onMounted(loadData)
</script>

<style scoped>
.fee-table {
  width: 100%;
  border-collapse: collapse;
}
.fee-table th, .fee-table td {
  padding: 12px 16px;
  text-align: left;
  border-bottom: 1px solid #ebeef5;
}
.fee-table th {
  background: #f5f7fa;
  font-weight: 600;
  color: #606266;
}
.fee-table tbody tr:hover {
  background: #f5f7fa;
}
.total-row {
  background: #f0f9eb;
}
</style>
