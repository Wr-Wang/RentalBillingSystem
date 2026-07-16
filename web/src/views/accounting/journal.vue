<template>
  <div>
    <div class="page-header">
      <h2>日记账</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>

    <el-card shadow="never" style="margin-bottom: 16px;">
      <el-form :inline="true" size="small" label-width="auto">
        <el-form-item label="合同号">
          <el-input v-model="filterContractNo" placeholder="合同号" clearable style="width:160px;" @clear="loadData" @keyup.enter="loadData">
            <template #append><el-button @click="loadData"><el-icon><Search /></el-icon></el-button></template>
          </el-input>
        </el-form-item>
        <el-form-item label="费用类型">
          <el-select v-model="filterFeeCodeId" filterable clearable placeholder="选择费用类型" style="width:200px;" @change="loadData">
            <el-option v-for="fc in feeCodes" :key="fc.id" :label="fc.name" :value="fc.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="期间">
          <el-date-picker v-model="filterPeriod" type="month" placeholder="选择期间" clearable style="width:150px;" @change="loadData" value-format="yyyy-MM" />
        </el-form-item>
        <el-form-item label="科目">
          <el-select v-model="filterSubjectCode" filterable clearable placeholder="科目" style="width:200px;" @change="loadData">
            <el-option v-for="s in subjects" :key="s.code" :label="s.code + ' ' + s.name" :value="s.code" />
          </el-select>
        </el-form-item>
        <el-form-item label="方向">
          <el-select v-model="filterDirection" clearable placeholder="方向" style="width:100px;" @change="loadData">
            <el-option label="借方" value="Debit" />
            <el-option label="贷方" value="Credit" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData"><el-icon><Search /></el-icon>查询</el-button>
          <el-button @click="resetFilters">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never">
      <el-table :data="entries" stripe v-loading="loading" style="width:100%;" @row-click="viewVoucherDetail" highlight-current-row>
        <el-table-column type="index" label="#" width="50" fixed />
        <el-table-column prop="voucherNo" label="凭证号" width="150" fixed />
        <el-table-column prop="contractNo" label="合同号" width="150">
          <template #default="{ row }">
            <span style="font-family:monospace;font-size:13px;">{{ row.contractNo || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="feeName" label="费用类型" width="120">
          <template #default="{ row }">
            <el-tag size="small" :type="row.chargeType === 'OneTime' ? 'warning' : 'primary'" effect="plain">
              {{ row.feeName || '-' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="accountingSubjectName" label="会计科目" min-width="160" show-overflow-tooltip />
        <el-table-column prop="direction" label="方向" width="60" align="center">
          <template #default="{ row }">
            <span :style="{ color: row.direction === 'Debit' ? '#409eff' : '#e6a23c', fontWeight: 600 }">
              {{ row.direction === 'Debit' ? '借' : '贷' }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="金额" width="150" align="right" sortable>
          <template #default="{ row }">
            <span style="font-weight:500;">¥{{ (row.amount || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="summary" label="摘要" min-width="200" show-overflow-tooltip />
        <el-table-column prop="period" label="期间" width="80" align="center" />
        <el-table-column prop="createdAt" label="日期" width="160" />
      </el-table>

      <div style="margin-top:16px;display:flex;justify-content:space-between;align-items:center;">
        <span style="font-size:13px;color:#909399;">共 {{ total }} 条记录</span>
        <el-pagination
          v-model:page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next"
          @current-change="loadData"
          @size-change="loadData"
          background
        />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Search, Refresh } from '@element-plus/icons-vue'
import { getJournals, getAccountingSubjects, getFeeCodes } from '@/api'

const route = useRoute()
const router = useRouter()
const loading = ref(false)
const entries = ref([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

// Filters
const filterContractNo = ref('')
const filterFeeCodeId = ref(null)
const filterPeriod = ref('')
const filterSubjectCode = ref(route.query.subjectCode || '')
const filterDirection = ref('')

// Lookup data
const subjects = ref([])
const feeCodes = ref([])

async function loadSubjects() {
  try { subjects.value = await getAccountingSubjects() || [] }
  catch { subjects.value = [] }
}

async function loadFeeCodes() {
  try { feeCodes.value = await getFeeCodes({ pageSize: 200 }) || [] }
  catch { feeCodes.value = [] }
}

async function loadData() {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: pageSize.value }
    if (filterContractNo.value) params.contractNo = filterContractNo.value
    if (filterFeeCodeId.value) params.feeCodeId = filterFeeCodeId.value
    if (filterPeriod.value) params.period = filterPeriod.value
    if (filterSubjectCode.value) params.subjectCode = filterSubjectCode.value

    const res = await getJournals(params)
    const items = res.items || []
    total.value = res.total ?? items.length

    let filtered = items
    if (filterDirection.value) {
      filtered = filtered.filter(e => e.direction === filterDirection.value)
    }
    entries.value = filtered.map(e => ({
      id: e.id,
      voucherId: e.voucherId,
      voucherNo: e.voucherNo || '',
      contractNo: e.contractNo || '',
      feeName: e.feeName || '',
      chargeType: e.chargeType || '',
      accountingSubjectName: e.accountingSubjectName || '',
      subjectCode: e.subjectCode || '',
      direction: e.direction || 'Debit',
      amount: e.amount || 0,
      summary: e.summary || '',
      period: e.period || '',
      createdAt: e.createdAt || ''
    }))
  } catch {
    ElMessage.error('加载日记账失败')
  }
  finally { loading.value = false }
}

function resetFilters() {
  filterContractNo.value = ''
  filterFeeCodeId.value = null
  filterPeriod.value = ''
  filterSubjectCode.value = ''
  filterDirection.value = ''
  page.value = 1
  loadData()
}

async function viewVoucherDetail(row) {
  if (row.voucherId) {
    router.push({ name: 'AccountingVouchers', query: { highlight: row.voucherId } })
  }
}

onMounted(() => {
  loadSubjects()
  loadFeeCodes()
  loadData()
})
</script>

<style scoped>
.search-bar {
  margin-bottom: 16px;
}
:deep(.el-table th.el-table__cell) {
  background-color: #f5f7fa;
  font-weight: 600;
  color: #303133;
}
</style>
