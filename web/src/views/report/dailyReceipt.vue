<template>
  <div>
    <div class="page-header">
      <h2>收款日报</h2>
      <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="searchDate" type="date" placeholder="选择日期" @change="loadData" />
      <el-button type="primary" @click="loadData">查询</el-button>
    </div>

    <div class="stat-cards">
      <div class="stat-card"><div class="label">收款笔数</div><div class="value">{{ stats.count }}</div></div>
      <div class="stat-card"><div class="label">收款总额</div><div class="value" style="color: #67c23a;">¥{{ formatMoney(stats.total) }}</div></div>
      <div class="stat-card"><div class="label">已确认</div><div class="value" style="color: #409eff;">¥{{ formatMoney(stats.confirmed) }}</div></div>
      <div class="stat-card"><div class="label">待确认</div><div class="value" style="color: #e6a23c;">¥{{ formatMoney(stats.pending) }}</div></div>
    </div>

    <el-card>
      <template #header>按状态汇总</template>
      <el-table :data="detailsList" v-loading="loading" stripe style="width:100%">
        <el-table-column label="状态" width="120">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Confirmed' ? 'success' : 'warning'" size="small">
              {{ row.status === 'Confirmed' ? '已确认' : '待确认' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="笔数" prop="cnt" width="80" />
        <el-table-column label="金额" min-width="150">
          <template #default="{ row }">¥{{ (row.total || 0).toLocaleString() }}</template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getDailyReceipt } from '@/api'

const searchDate = ref(new Date())
const loading = ref(false)
const detailsList = ref([])
const stats = reactive({ count: 0, total: 0, confirmed: 0, pending: 0 })

function formatMoney(v) { return (v || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }

async function loadData() {
  loading.value = true
  try {
    const d = searchDate.value || new Date()
    const date = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
    const res = await getDailyReceipt({ date })
    // 后端返回 { date, details: [{ status, cnt, total }] }
    const items = res?.details || []
    detailsList.value = Array.isArray(items) ? items.map(item => ({
      status: item.status || 'Pending',
      cnt: item.cnt || 0,
      total: item.total || 0
    })) : []

    stats.count = detailsList.value.reduce((s, r) => s + r.cnt, 0)
    stats.total = detailsList.value.reduce((s, r) => s + r.total, 0)
    stats.confirmed = detailsList.value.filter(r => r.status === 'Confirmed').reduce((s, r) => s + r.total, 0)
    stats.pending = detailsList.value.filter(r => r.status !== 'Confirmed').reduce((s, r) => s + r.total, 0)
  } catch {
    ElMessage.error('加载收款日报失败')
  }
  loading.value = false
}

onMounted(loadData)
</script>

<style scoped>
.stat-cards {
  display: flex; gap: 16px; margin-bottom: 16px;
}
.stat-card {
  flex: 1; background: #fff; border-radius: 8px; padding: 20px; text-align: center; box-shadow: 0 1px 4px rgba(0,0,0,0.06);
}
.stat-card .label {
  font-size: 13px; color: #909399; margin-bottom: 8px;
}
.stat-card .value {
  font-size: 28px; font-weight: 700;
  color: #303133;
}
</style>
