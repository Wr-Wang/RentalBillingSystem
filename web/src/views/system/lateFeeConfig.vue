<template>
  <div>
    <div class="page-header">
      <h2>滞纳金配置</h2>
      <el-button @click="fetchConfig" :loading="loading">刷新</el-button>
    </div>

    <el-card v-loading="loading">
      <el-form :model="config" label-width="140px" style="max-width: 500px;">
        <el-form-item label="日利率">
          <el-input-number v-model="config.dailyRate" :precision="5" :step="0.0001" :min="0" :max="1" style="width: 200px;" />
          <span style="margin-left: 8px;">（{{ (config.dailyRate * 100).toFixed(3) }}% / 天）</span>
        </el-form-item>
        <el-form-item label="年化利率">
          <span style="line-height: 32px; color: #909399;">{{ (config.dailyRate * 365 * 100).toFixed(2) }}%</span>
        </el-form-item>
        <el-form-item label="宽限期">
          <el-input-number v-model="config.graceDays" :min="0" :max="365" style="width: 200px;" />
          <span style="margin-left: 8px;">天（到期后X天内不计滞纳金）</span>
        </el-form-item>
        <el-form-item label="滞纳金上限">
          <el-input-number v-model="config.maxRate" :precision="2" :min="0" :max="1000" style="width: 200px;" />
          <span style="margin-left: 8px;">% 本金（空=不设上限）</span>
        </el-form-item>
        <el-form-item label="最低滞纳金">
          <el-input-number v-model="config.minAmount" :precision="2" :min="0" style="width: 200px;" />
          <span style="margin-left: 8px;">元（低于此值不计）</span>
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="config.effectiveDate" type="date" value-format="YYYY-MM-DD" style="width:200px;" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="save" :loading="saving">保存配置</el-button>
          <el-button v-if="historyList.length" @click="showHistory = !showHistory">
            {{ showHistory ? '收起历史' : '历史记录' }}
          </el-button>
        </el-form-item>
      </el-form>

      <el-table v-if="showHistory" :data="historyList" stripe size="small" style="margin-top:16px;">
        <el-table-column label="日利率" width="100">
          <template #default="{ row }">{{ (row.dailyRate * 100).toFixed(3) }}%</template>
        </el-table-column>
        <el-table-column prop="graceDays" label="宽限期" width="80" />
        <el-table-column label="上限" width="80">
          <template #default="{ row }">{{ row.maxRate ?? '无' }}%</template>
        </el-table-column>
        <el-table-column prop="effectiveDate" label="生效日期" width="120" />
        <el-table-column prop="isActive" label="状态" width="70">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'info'" size="small">{{ row.isActive ? '生效' : '历史' }}</el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'

const loading = ref(false)
const saving = ref(false)
const showHistory = ref(false)
const historyList = ref([])
const config = reactive({
  dailyRate: 0.0005, graceDays: 3, maxRate: 100, minAmount: 1, effectiveDate: ''
})

async function fetchConfig() {
  loading.value = true
  try {
    const res = await fetch('/api/latefeeconfig/active', { headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') } })
    if (res.ok) {
      const data = await res.json()
      config.dailyRate = data.dailyRate ?? 0.0005
      config.graceDays = data.graceDays ?? 3
      config.maxRate = data.maxRate ?? 100
      config.minAmount = data.minAmount ?? 1
      config.effectiveDate = data.effectiveDate || ''
    }
    const historyRes = await fetch('/api/latefeeconfig', { headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') } })
    if (historyRes.ok) historyList.value = await historyRes.json()
  } catch (e) { /* ignore */ }
  loading.value = false
}

async function save() {
  if (!config.effectiveDate) { ElMessage.warning('请选择生效日期'); return }
  saving.value = true
  try {
    const res = await fetch('/api/latefeeconfig', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + localStorage.getItem('token') },
      body: JSON.stringify(config)
    })
    if (res.ok) {
      ElMessage.success('滞纳金配置已保存')
      await fetchConfig()
    } else {
      const err = await res.json()
      ElMessage.error(err?.message || '保存失败')
    }
  } catch (e) {
    ElMessage.error('保存失败')
  }
  saving.value = false
}

onMounted(() => fetchConfig())
</script>
