<template>
  <div>
    <div class="page-header">
      <h2>费用收入统计</h2>
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
import { computed } from 'vue'

const feeStats = [
  { name: '房租费', receivable: 500000, received: 480000, overdue: 20000, rate: 96.0 },
  { name: '水费', receivable: 12000, received: 10500, overdue: 1500, rate: 87.5 },
  { name: '电费', receivable: 25000, received: 22000, overdue: 3000, rate: 88.0 },
  { name: '卫生费', receivable: 3000, received: 2800, overdue: 200, rate: 93.3 },
  { name: '管理费', receivable: 15000, received: 14000, overdue: 1000, rate: 93.3 },
  { name: '燃气费', receivable: 8000, received: 6000, overdue: 2000, rate: 75.0 },
  { name: '网费', receivable: 8000, received: 7500, overdue: 500, rate: 93.8 }
]

const total = computed(() => {
  const receivable = feeStats.reduce((s, r) => s + r.receivable, 0)
  const received = feeStats.reduce((s, r) => s + r.received, 0)
  const overdue = receivable - received
  return { receivable, received, overdue, rate: Math.round(received / receivable * 100 * 10) / 10 }
})
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
