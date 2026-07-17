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
        <el-descriptions-item label="证件号码">{{ tenant.idCard || '-' }}</el-descriptions-item>
        <el-descriptions-item label="电话">{{ tenant.phone || '-' }}</el-descriptions-item>
        <el-descriptions-item label="邮箱">{{ tenant.email || '-' }}</el-descriptions-item>
      </el-descriptions>
    </el-card>

    <el-card style="margin-bottom: 16px;">
      <template #header>关联合同</template>
      <el-table :data="currentContracts" v-loading="contractsLoading" stripe>
        <el-table-column prop="contractNo" label="合同号" width="220" />
        <el-table-column prop="roomFullCode" label="房屋" width="120" />
        <el-table-column prop="startDate" label="起租" width="110" />
        <el-table-column prop="endDate" label="到期" width="110" />
        <el-table-column prop="rentAmount" label="月租金">
          <template #default="{ row }">¥{{ (row.rentAmount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="status" label="状态">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Active' ? 'success' : 'info'" size="small">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="80">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="$router.push('/contracts/' + row.id)">查看</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div v-if="!contractsLoading && currentContracts.length === 0" style="padding:20px;text-align:center;color:#909399;">
        暂无关联合同
      </div>
    </el-card>

    <el-card v-if="billHistory.length > 0" style="margin-bottom: 16px;">
      <template #header>账单历史</template>
      <el-table :data="billHistory" stripe>
        <el-table-column prop="period" label="账期" width="100" />
        <el-table-column label="金额" width="120">
          <template #default="{ row }">¥{{ (row.totalAmount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="已付" width="120">
          <template #default="{ row }">¥{{ (row.paidAmount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="dueDate" label="到期日" width="110" />
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Paid' ? 'success' : row.status === 'Overdue' ? 'danger' : 'warning'" size="small">
              {{ row.status === 'Paid' ? '已付清' : row.status === 'Overdue' ? '逾期' : '待收' }}
            </el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getTenant, getContracts, getDebitNotes } from '../../api/index'

const route = useRoute()

const tenant = ref({
  name: '', idCard: '', phone: '', email: ''
})
const currentContracts = ref([])
const contractsLoading = ref(false)
const billHistory = ref([])

const statusLabelMap = {
  Draft: '草稿', PendingApproval: '待审批', Active: '活跃',
  Suspended: '已暂停', Expired: '已到期', Terminated: '已终止', Renewed: '已续签'
}

function statusLabel(status) {
  return statusLabelMap[status] || status
}

onMounted(async () => {
  const id = route.params.id
  try {
    const res = await getTenant(id)
    tenant.value = {
      name: res.name || '',
      idCard: res.idCard || '',
      phone: res.phone || '',
      email: res.email || ''
    }

    // 加载关联合同（通过 tenantId 过滤）
    contractsLoading.value = true
    try {
      const contractRes = await getContracts({ tenantId: id, pageSize: 50 })
      const items = contractRes.items || contractRes.data || []
      currentContracts.value = items.map(c => ({
        id: c.id,
        contractNo: c.contractNo,
        roomFullCode: c.roomFullCode || '',
        startDate: c.startDate || '',
        endDate: c.endDate || '',
        rentAmount: c.rentAmount || 0,
        status: c.status || 'Unknown'
      }))
    } catch {
      // 静默
    }
    contractsLoading.value = false

    // 加载账单历史
    try {
      const billRes = await getDebitNotes({ tenantId: id, pageSize: 50 })
      const bills = billRes.items || billRes.data || billRes || []
      billHistory.value = Array.isArray(bills) ? bills.map(d => ({
        period: d.period || '',
        totalAmount: d.totalAmount || 0,
        paidAmount: d.totalReceived || 0,
        dueDate: d.dueDate || '',
        paidDate: '',
        status: d.status === 'Paid' ? 'Paid' : d.status === 'Pending' ? 'Pending' : 'Overdue'
      })) : []
    } catch {
      // 静默
    }
  } catch {
    ElMessage.error('加载租客详情失败')
  }
})
</script>
