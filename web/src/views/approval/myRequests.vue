<template>
  <div>
    <div class="page-header">
      <h2>我提交的审批</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-table :data="myRequests" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="requestNo" label="申请编号" width="150" />
      <el-table-column prop="approvalTypeName" label="审批类型" width="120" />
      <el-table-column prop="businessSummary" label="业务摘要" min-width="200" />
      <el-table-column prop="amount" label="金额" width="110">
        <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Approved' ? 'success' : row.status === 'Rejected' ? 'danger' : 'warning'" size="small">
            {{ row.status === 'Pending' ? '审批中' : row.status === 'Approved' ? '已通过' : '已驳回' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="submittedAt" label="提交时间" width="150" />
      <el-table-column prop="completedAt" label="完成时间" width="150" />
    </el-table>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const myRequests = ref([
  { requestNo: 'SP-20260620-001', approvalTypeName: '提前解约', businessSummary: 'C栋-601 提前解约', amount: 10000, status: 'Approved', submittedAt: '2026-06-20 09:00', completedAt: '2026-06-21 14:30' },
  { requestNo: 'SP-20260622-002', approvalTypeName: '批量导入房屋', businessSummary: '导入D栋15套房源', amount: 0, status: 'Pending', submittedAt: '2026-06-22 10:00', completedAt: '-' },
  { requestNo: 'SP-20260623-003', approvalTypeName: '应收减免', businessSummary: 'B-302 网费减免', amount: 80, status: 'Rejected', submittedAt: '2026-06-23 15:00', completedAt: '2026-06-24 09:00' }
])
</script>
