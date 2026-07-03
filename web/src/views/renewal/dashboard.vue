<template>
  <div>
    <h2>待续签看板</h2>
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
            <div class="stat-value" style="color:#f56c6c;">{{ overdueCount }}</div>
            <div class="stat-label">逾期未处理</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card>
      <template #header>
        <span>到期合同列表</span>
      </template>
      <el-table :data="contracts" stripe v-loading="loading" style="width:100%;">
        <el-table-column prop="contractNo" label="合同编号" width="150" />
        <el-table-column prop="tenants" label="租客" width="120">
          <template #default="{ row }">{{ row.tenantName || '-' }}</template>
        </el-table-column>
        <el-table-column prop="rentAmount" label="月租金" width="120">
          <template #default="{ row }">¥{{ row.rentAmount }}</template>
        </el-table-column>
        <el-table-column prop="endDate" label="到期日" width="120" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Active' ? 'success' : 'info'">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button type="primary" size="small" @click="previewRenewal(row)">续签</el-button>
            <el-button size="small" @click="$router.push(`/contracts/${row.id}`)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { getContracts, previewRenewal } from '@/api'

const loading = ref(false)
const contracts = ref([])

const expiringCount = computed(() => contracts.value.filter(c => c.status === 'Active').length)
const pendingRenewalCount = ref(0)
const renewedCount = ref(0)
const overdueCount = ref(0)

async function loadData() {
  loading.value = true
  try {
    const res = await getContracts({ pageSize: 100 })
    contracts.value = res.items || []
    pendingRenewalCount.value = res.items?.filter(c => c.status === 'PendingApproval').length || 0
    renewedCount.value = res.items?.filter(c => c.status === 'Renewed').length || 0
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

function handlePreview(row) {
  previewRenewal(row.id)
}

onMounted(loadData)
</script>

<style scoped>
.stat-card { text-align: center; padding: 10px 0; }
.stat-value { font-size: 32px; font-weight: bold; }
.stat-label { font-size: 14px; color: #909399; margin-top: 8px; }
</style>
