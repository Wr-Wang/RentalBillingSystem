<template>
  <div class="journal-page">
    <div class="page-header">
      <h1>日记账</h1>
      <div class="page-actions">
        <el-button size="small" @click="fetchData"><el-icon><Refresh /></el-icon>刷新</el-button>
        <el-button type="primary" size="small" @click="handleGenerate">+ 出账</el-button>
        <el-button v-if="selectedIds.length > 0" type="warning" size="small" :loading="posting" @click="handlePost">
          过账（{{ selectedIds.length }}）
        </el-button>
      </div>
    </div>

    <div class="filter-bar">
      <el-form :inline="true" :model="filters" size="small" label-width="auto">
        <el-form-item label="账期">
          <el-date-picker v-model="filters.period" type="month" placeholder="账期" value-format="YYYY-MM" clearable editable="false" style="width:130px;" @change="handleFilterChange" />
        </el-form-item>
        <el-form-item label="合同">
          <el-select v-model="filters.contractId" placeholder="全部" clearable filterable style="width:130px;" @change="handleFilterChange">
            <el-option v-for="c in contracts" :key="c.id" :label="c.contractNo + (c.tenantName ? ' - ' + c.tenantName : '')" :value="c.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="费用类型">
          <el-select v-model="filters.feeCodeId" placeholder="全部" clearable filterable style="width:140px;" @change="handleFilterChange">
            <el-option v-for="fc in feeCodes" :key="fc.id" :label="fc.name" :value="fc.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="GL">
          <el-select v-model="filters.glPosted" placeholder="全部" clearable style="width:90px;" @change="handleFilterChange">
            <el-option label="已入账" :value="true" />
            <el-option label="未入账" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item label="账单月">
          <el-date-picker v-model="filters.billMonth" type="month" placeholder="账单月" value-format="YYYY-MM" clearable editable="false" style="width:130px;" @change="handleFilterChange" />
        </el-form-item>
        <el-form-item label="生成账单">
          <el-select v-model="filters.isBilled" placeholder="全部" clearable style="width:100px;" @change="handleFilterChange">
            <el-option label="已生成" :value="true" />
            <el-option label="未生成" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleFilterChange">查询</el-button>
          <el-button @click="resetFilters">重置</el-button>
        </el-form-item>
      </el-form>
    </div>

    <el-card shadow="never" style="margin-top:16px;">
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span style="font-weight:600;">应收明细</span>
          <span style="font-size:13px;color:#909399;">
            合计: <strong style="color:#e6a23c;">¥{{ totalAmount.toLocaleString() }}</strong>
            &nbsp;|&nbsp;共 {{ total }} 条
          </span>
        </div>
      </template>

      <el-table :data="list" v-loading="loading" stripe style="width:100%;" @row-click="viewDetail" highlight-current-row @selection-change="onSelectionChange">
        <el-table-column type="selection" width="40" :selectable="r => !r.glPosted" />
        <el-table-column type="index" label="#" width="45" fixed />
        <el-table-column prop="contractNo" label="合同号" width="180" fixed>
          <template #default="{ row }">
            <span style="font-family:monospace;font-size:13px;">{{ row.contractNo || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="费用类型" width="120">
          <template #default="{ row }">
            <el-tag :type="row.chargeType === 'OneTime' ? 'warning' : 'primary'" size="small" effect="plain">
              {{ row.feeName || row.entryType || '-' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="period" width="95" align="center">
          <template #header>
            <el-tooltip content="费用归属期，该笔应收对应的实际业务月份" placement="top">
              <span>账期 <el-icon><InfoFilled /></el-icon></span>
            </el-tooltip>
          </template>
        </el-table-column>
        <el-table-column prop="billMonth" width="85" align="center">
          <template #header>
            <el-tooltip content="预计到账月，按出账日计算（每月25日20:00为截点：之前出账→次月，之后→下下月）" placement="top">
              <span>账单月 <el-icon><InfoFilled /></el-icon></span>
            </el-tooltip>
          </template>
          <template #default="{ row }">
            <span>{{ row.billMonth || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="金额" width="120" align="right" sortable>
          <template #default="{ row }">
            <span style="font-weight:500;">¥{{ (row.amount || 0).toLocaleString() }}</span>
          </template>
        </el-table-column>
        <el-table-column label="到期日" width="120" align="center">
          <template #default="{ row }">{{ formatDate(row.dueDate) }}</template>
        </el-table-column>
        <el-table-column label="GL状态" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="row.glPosted ? 'success' : 'warning'" size="small" effect="dark">
              {{ row.glPosted ? '已入账' : '未入账' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="生成账单" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isBilled ? 'success' : 'info'" size="small" effect="plain">
              {{ row.isBilled ? '已生成' : '未生成' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="billedAt" label="出账时间" min-width="160" show-overflow-tooltip />
      </el-table>

      <div style="margin-top:16px;display:flex;justify-content:space-between;align-items:center;">
        <span style="font-size:13px;color:#909399;">共 {{ total }} 条记录</span>
        <el-pagination
          v-model:current-page="page"
          :page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next"
          @current-change="fetchData"
          @size-change="fetchData"
          background
        />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { formatDate } from '@/utils/chinaTime'
import { getJournals, generateJournals, getFeeCodes, postJournals, getContracts } from '@/api'
import { Refresh, InfoFilled } from '@element-plus/icons-vue'
import { useUserStore } from '@/store/user'

const router = useRouter()
const userStore = useUserStore()
const list = ref([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const feeCodes = ref([])
const contracts = ref([])
const filters = ref({ period: null, billMonth: null, contractId: null, feeCodeId: null, glPosted: null, isBilled: null })
const selectedIds = ref([])
const posting = ref(false)

const totalAmount = computed(() =>
  list.value.reduce((s, r) => s + (r.amount || 0), 0)
)

const fetchData = async () => {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: pageSize.value }
    if (filters.value.period && /^\d{4}-\d{2}$/.test(filters.value.period)) params.period = filters.value.period
    if (filters.value.billMonth && /^\d{4}-\d{2}$/.test(filters.value.billMonth)) params.billMonth = filters.value.billMonth
    if (filters.value.contractId) params.contractId = filters.value.contractId
    if (filters.value.feeCodeId) params.feeCodeId = filters.value.feeCodeId
    if (filters.value.glPosted !== null && filters.value.glPosted !== '') params.glPosted = filters.value.glPosted
    if (filters.value.isBilled !== null && filters.value.isBilled !== '') params.isBilled = filters.value.isBilled

    const res = await getJournals(params)
    list.value = (res.items || []).map(r => ({
      id: r.id,
      contractNo: r.contractNo || '',
      feeName: r.feeName || '',
      chargeType: r.chargeType || '',
      entryType: r.entryType || '',
      period: r.period || '',
      amount: r.amount || 0,
      dueDate: r.dueDate || '',
      glPosted: r.glPosted || false,
      isBilled: r.isBilled || false,
      billMonth: r.billMonth || '',
      billedAt: r.billedAt ? (r.billedAt.slice ? r.billedAt.slice(0, 16).replace('T', ' ') : r.billedAt) : ''
    }))
    total.value = res.total || 0
  } catch (e) {
    console.error('Journal fetch error:', e)
    list.value = []
  }
  finally { loading.value = false }
}

const fetchFeeCodes = async () => {
  try {
    const res = await getFeeCodes({ pageSize: 200 })
    feeCodes.value = (res.items || res.data || res || [])
  } catch { feeCodes.value = [] }
}

const fetchContracts = async () => {
  try {
    const res = await getContracts({ pageSize: 200 })
    contracts.value = (res.items || res.data || res || [])
  } catch { contracts.value = [] }
}

const handleGenerate = async () => {
  try {
    await generateJournals({})
    await fetchData()
  } catch { /* ignore */ }
}

const resetFilters = () => {
  filters.value = { period: null, billMonth: null, contractId: null, feeCodeId: null, glPosted: null, isBilled: null }
  page.value = 1
  fetchData()
}

const handleFilterChange = () => {
  page.value = 1
  fetchData()
}

function viewDetail(row) {
}

function onSelectionChange(rows) {
  selectedIds.value = rows.map(r => r.id)
}

async function handlePost() {
  if (selectedIds.value.length === 0) return
  posting.value = true
  try {
    const res = await postJournals(selectedIds.value)
    ElMessage.success(`已过账 ${res.posted} 条`)
    selectedIds.value = []
    await fetchData()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '过账失败')
  }
  finally { posting.value = false }
}

onMounted(() => {
  fetchFeeCodes()
  fetchContracts()
  // 等待用户公司信息加载完成后再请求数据（确保 companyId 已注入）
  if (userStore.profileLoaded) {
    fetchData()
  } else {
    const unwatch = watch(() => userStore.profileLoaded, (val) => {
      if (val) { fetchData(); unwatch() }
    })
  }
})
</script>

<style scoped>
.journal-page { max-width: 100%; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.page-header h1 { margin: 0; font-size: 20px; }
.page-actions { display: flex; gap: 8px; }
.filter-bar { background: #f5f7fa; border-radius: 6px; padding: 12px 16px 4px; margin-bottom: 16px; }
.filter-bar .el-form-item { margin-bottom: 8px; }
.filter-bar .el-form--inline .el-form-item { margin-right: 12px; }
:deep(.el-table th.el-table__cell) { background-color: #f5f7fa; font-weight: 600; color: #303133; }
.el-card + .el-card { margin-top: 16px; }
</style>
