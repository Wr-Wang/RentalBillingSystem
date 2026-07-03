<template>
  <div>
    <div class="page-header">
      <h2>待续签看板</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>

    <el-row :gutter="20" style="margin-bottom:20px;">
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-value" style="color:#409eff;">{{ expiringCount }}</div>
            <div class="stat-label">即将到期（14天内）</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-value" style="color:#e6a23c;">{{ pendingRenewalCount }}</div>
            <div class="stat-label">续签审批中</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-value" style="color:#67c23a;">{{ renewedCount }}</div>
            <div class="stat-label">本月已续签</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-value" style="color:#f56c6c;">{{ overdueExpiredCount }}</div>
            <div class="stat-label">已到期未续签</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card>
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span>到期合同列表（{{ filteredContracts.length }} 份）</span>
          <el-radio-group v-model="filterStatus" size="small">
            <el-radio label="">全部</el-radio>
            <el-radio label="expiring">即将到期</el-radio>
            <el-radio label="overdue">已逾期</el-radio>
            <el-radio label="renewable">可续签</el-radio>
          </el-radio-group>
        </div>
      </template>
      <el-table :data="filteredContracts" stripe v-loading="loading" style="width:100%;">
        <el-table-column prop="contractNo" label="合同编号" width="150" />
        <el-table-column label="租客" width="120">
          <template #default="{ row }">{{ row.tenantName || '-' }}</template>
        </el-table-column>
        <el-table-column label="月租金" width="120">
          <template #default="{ row }">¥{{ (row.rentAmount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="endDate" label="到期日" width="120" />
        <el-table-column label="距到期" width="100">
          <template #default="{ row }">
            <el-tag :type="row.daysUntilEnd > 0 ? 'warning' : 'danger'" size="small">
              {{ row.daysUntilEnd > 0 ? row.daysUntilEnd + '天后' : '已过期' + Math.abs(row.daysUntilEnd) + '天' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button type="primary" size="small" :disabled="!row.renewable" @click="previewRenewal(row)">续签</el-button>
            <el-button size="small" @click="$router.push(`/contracts/${row.id}`)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getContracts, previewRenewal as apiPreviewRenewal } from '@/api'

const router = useRouter()
const loading = ref(false)
const contracts = ref([])
const filterStatus = ref('')

const statusTypeMap = {
  Draft: 'info', PendingApproval: 'warning', Active: 'success',
  Suspended: '', Expired: 'danger', Terminated: 'danger', Renewed: 'primary'
}
const statusLabelMap = {
  Draft: '草稿', PendingApproval: '待审批', Active: '活跃',
  Suspended: '已暂停', Expired: '已到期', Terminated: '已终止', Renewed: '已续签'
}
function statusType(s) { return statusTypeMap[s] || 'info' }
function statusLabel(s) { return statusLabelMap[s] || s }

// 今天日期
const today = new Date()
const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`

// 距离到期天数
function calcDaysUntilEnd(endDate) {
  if (!endDate) return 999
  const end = new Date(endDate)
  return Math.round((end - today) / (1000 * 60 * 60 * 24))
}

const filteredContracts = computed(() => {
  let list = contracts.value
  switch (filterStatus.value) {
    case 'expiring':
      list = list.filter(c => c.status === 'Active' && c.daysUntilEnd >= 0 && c.daysUntilEnd <= 14)
      break
    case 'overdue':
      list = list.filter(c => c.daysUntilEnd < 0 && (c.status === 'Active' || c.status === 'Expired'))
      break
    case 'renewable':
      list = list.filter(c => c.renewable)
      break
  }
  return list
})

const expiringCount = computed(() =>
  contracts.value.filter(c => c.status === 'Active' && c.daysUntilEnd >= 0 && c.daysUntilEnd <= 14).length
)
const pendingRenewalCount = computed(() =>
  contracts.value.filter(c => c.status === 'PendingApproval').length
)
const renewedCount = computed(() =>
  contracts.value.filter(c => c.status === 'Renewed').length
)
const overdueExpiredCount = computed(() =>
  contracts.value.filter(c => c.daysUntilEnd < 0 && (c.status === 'Active' || c.status === 'Expired')).length
)

async function loadData() {
  loading.value = true
  try {
    const res = await getContracts({ pageSize: 200, status: undefined })
    const items = res.items || res.data || []
    contracts.value = items.map(c => {
      const daysUntilEnd = calcDaysUntilEnd(c.endDate)
      const hasRenewal = c.hasRenewalContract || false
      return {
        id: c.id,
        contractNo: c.contractNo || '',
        tenantName: c.tenants?.length > 0 ? c.tenants[0].tenantName : '',
        rentAmount: c.rentAmount || 0,
        endDate: c.endDate || '',
        startDate: c.startDate || '',
        status: c.status || 'Unknown',
        daysUntilEnd,
        renewable: (c.status === 'Active' || c.status === 'Expired') && !hasRenewal && daysUntilEnd <= 30
      }
    })
  } catch { ElMessage.error('加载合同数据失败') }
  finally { loading.value = false }
}

async function previewRenewal(row) {
  try {
    const res = await apiPreviewRenewal(row.id)
    // 有续签预览数据则跳转到续签页面
    ElMessage.success(`合同 ${row.contractNo} 可续签，正在跳转...`)
    router.push(`/contracts/${row.id}`)
  } catch {
    ElMessage.error('续签预览失败，请检查合同状态')
  }
}

onMounted(loadData)
</script>

<style scoped>
.stat-card { text-align: center; padding: 10px 0; }
.stat-value { font-size: 32px; font-weight: bold; }
.stat-label { font-size: 14px; color: #909399; margin-top: 8px; }
</style>
