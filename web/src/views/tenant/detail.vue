<template>
  <div>
    <div class="page-header">
      <h2>租客详情</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-card style="margin-bottom: 16px;">
      <template #header>基本信息</template>
      <el-descriptions :column="2" border>
        <el-descriptions-item label="姓名">{{ tenant.name }}</el-descriptions-item>
        <el-descriptions-item label="证件类型">{{ tenant.identityType === 'PRC_ID' ? '身份证' : '护照' }}</el-descriptions-item>
        <el-descriptions-item label="证件号">{{ tenant.identityNo }}</el-descriptions-item>
        <el-descriptions-item label="电话">{{ tenant.phone }}</el-descriptions-item>
        <el-descriptions-item label="邮箱">{{ tenant.email || '-' }}</el-descriptions-item>
      </el-descriptions>
    </el-card>

    <el-card style="margin-bottom: 16px;">
      <template #header>当前合同</template>
      <el-table :data="currentContracts" stripe>
        <el-table-column prop="contractNo" label="合同号" width="150" />
        <el-table-column prop="roomName" label="房屋" width="120" />
        <el-table-column prop="startDate" label="起租" width="110" />
        <el-table-column prop="endDate" label="到期" width="110" />
        <el-table-column prop="rentAmount" label="月租金">
          <template #default="{ row }">¥{{ row.rentAmount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="status" label="状态">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Active' ? 'success' : 'info'" size="small">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card>
      <template #header>账单历史</template>
      <el-table :data="billHistory" stripe>
        <el-table-column prop="period" label="账期" width="90" />
        <el-table-column prop="totalAmount" label="应收" width="110">
          <template #default="{ row }">¥{{ row.totalAmount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="paidAmount" label="实收" width="110">
          <template #default="{ row }">¥{{ row.paidAmount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="dueDate" label="到期日" width="100" />
        <el-table-column prop="paidDate" label="付款日" width="100" />
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Paid' ? 'success' : row.status === 'Overdue' ? 'danger' : 'warning'" size="small">
              {{ row.status === 'Paid' ? '已付清' : row.status === 'Overdue' ? '逾期' : '待收款' }}
            </el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const tenant = ref({
  id: route.params.id,
  name: '张三',
  identityType: 'PRC_ID',
  identityNo: '110101199001011234',
  phone: '13800138001',
  email: 'zhangsan@email.com'
})

const currentContracts = ref([
  { contractNo: 'HT-2026-001', roomName: 'A栋-101', startDate: '2026-01-01', endDate: '2027-12-31', rentAmount: 5200, status: 'Active' }
])

const billHistory = ref([
  { period: '2026-06', totalAmount: 5460, paidAmount: 5460, dueDate: '2026-06-05', paidDate: '2026-06-03', status: 'Paid' },
  { period: '2026-05', totalAmount: 5460, paidAmount: 5460, dueDate: '2026-05-05', paidDate: '2026-05-04', status: 'Paid' },
  { period: '2026-04', totalAmount: 5460, paidAmount: 5460, dueDate: '2026-04-05', paidDate: '2026-04-02', status: 'Paid' },
  { period: '2026-03', totalAmount: 5460, paidAmount: 5460, dueDate: '2026-03-05', paidDate: '2026-03-03', status: 'Paid' }
])
</script>
