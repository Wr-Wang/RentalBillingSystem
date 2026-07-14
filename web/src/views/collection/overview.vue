<template>
  <div>
    <div class="page-header">
      <h2>催缴管理</h2>
      <div class="table-actions">
        <el-button @click="$router.push('/collection/config')">
          <el-icon><Setting /></el-icon>催缴配置
        </el-button>
        <el-button @click="$router.push('/collection/records')">
          <el-icon><Tickets /></el-icon>催缴记录
        </el-button>
        <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
      </div>
    </div>

    <!-- Overview Stats -->
    <el-row :gutter="16" class="stat-cards">
      <el-col :span="6" v-for="(s, i) in stageStats" :key="i">
        <el-card shadow="never" :style="{ borderLeft: '4px solid ' + s.color }">
          <div class="stat-item">
            <div class="stat-label">{{ s.name }}</div>
            <div class="stat-value" :style="{ color: s.color }">{{ s.count }}</div>
            <div class="sub">{{ s.desc }}</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16">
      <!-- Manual Collection -->
      <el-col :span="12">
        <el-card style="margin-bottom: 16px;">
          <template #header>手动催缴</template>
          <el-form :model="manualForm" label-width="100px">
            <el-form-item label="合同号">
              <el-select v-model="manualForm.contractId" filterable placeholder="选择逾期合同" style="width: 100%">
                <el-option v-for="c in overdueContracts" :key="c.id" :label="c.contractNo + ' - ' + c.tenantName" :value="c.id" />
              </el-select>
            </el-form-item>
            <el-form-item label="催缴方式">
              <el-select v-model="manualForm.channel" style="width: 100%">
                <el-option label="短信" value="SMS" />
                <el-option label="电话" value="Phone" />
                <el-option label="系统通知" value="WeChat" />
              </el-select>
            </el-form-item>
            <el-form-item label="催缴内容">
              <el-input v-model="manualForm.content" type="textarea" :rows="3" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :loading="sending" @click="sendManual">发送</el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <!-- Overdue List -->
      <el-col :span="12">
        <el-card style="margin-bottom: 16px;">
          <template #header>逾期合同列表</template>
          <el-table :data="overdueContracts" v-loading="loading" stripe size="small">
            <el-table-column prop="contractNo" label="合同号" width="120" />
            <el-table-column prop="tenantName" label="租客" width="80" />
            <el-table-column label="欠费金额" width="100">
              <template #default="{ row }">¥{{ (row.overdueAmount || 0).toLocaleString() }}</template>
            </el-table-column>
            <el-table-column prop="overdueDays" label="逾期天数" width="80" />
            <el-table-column label="阶段" width="80">
              <template #default="{ row }">
                <el-tag :type="stageTagType(row.stage)" size="small">S{{ row.stage }}</el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/user'
import { getOverdueDetail, getCollectionStages, manualCollection } from '@/api'

const userStore = useUserStore()
const loading = ref(false)
const sending = ref(false)
const overdueContracts = ref([])
const stages = ref([])
const stageStats = ref([])

const manualForm = reactive({
  contractId: '', channel: 'SMS',
  content: '您好，您的房租已逾期，请尽快缴纳以免产生滞纳金。'
})

const stageColors = ['#909399', '#e6a23c', '#f56c6c', '#c03636']
const stageDescs = ['即将进入下一阶段', '需重点跟进', '发送正式催缴函', '法务介入']

function stageTagType(stage) {
  if (stage <= 1) return 'info'
  if (stage <= 2) return 'warning'
  return 'danger'
}

async function loadData() {
  loading.value = true
  try {
    const companyId = userStore.effectiveCompanyId
    const params = companyId ? { companyId } : {}

    // 加载逾期合同数据
    const overdueRes = await getOverdueDetail(params)
    const overdueItems = Array.isArray(overdueRes) ? overdueRes : (overdueRes.items || overdueRes.data || overdueRes || [])

    // 加载催缴阶段定义
    let stagesList = []
    try {
      stagesList = await getCollectionStages()
      if (!Array.isArray(stagesList)) stagesList = []
    } catch { /* 静默 */ }
    stages.value = stagesList

    // 为每个逾期合同计算阶段
    overdueContracts.value = overdueItems.map(item => {
      const days = item.daysOverdue || 0
      let stage = stagesList.length
      for (let i = stagesList.length - 1; i >= 0; i--) {
        if (days >= (stagesList[i].daysOverdue || 0)) { stage = i + 1; break }
      }
      if (stage > stagesList.length) stage = stagesList.length
      return {
        id: item.contractId || item.id,
        contractNo: item.contractNo || '',
        tenantName: item.tenantName || '',
        overdueAmount: item.totalAmount ? item.totalAmount - (item.totalReceived || 0) : (item.amount || 0),
        overdueDays: days,
        stage
      }
    })

    // 计算各阶段统计
    stageStats.value = stagesList.map((s, i) => ({
      name: s.name || `阶段 ${i + 1}`,
      count: overdueContracts.value.filter(c => c.stage === i + 1).length,
      color: stageColors[i] || '#909399',
      desc: stageDescs[i] || ''
    }))
  } catch {
    ElMessage.error('加载催缴数据失败')
  }
  loading.value = false
}

async function sendManual() {
  if (!manualForm.contractId) { ElMessage.warning('请选择合同'); return }
  sending.value = true
  try {
    await manualCollection({ contractId: manualForm.contractId })
    ElMessage.success('催缴已发送')
  } catch {
    ElMessage.error('发送失败')
  }
  sending.value = false
}

onMounted(loadData)
</script>

<style scoped>
.stat-cards {
  margin-bottom: 16px;
}
.stat-item {
  text-align: center;
  padding: 8px 0;
}
.stat-label {
  font-size: 13px;
  color: #909399;
  margin-bottom: 8px;
}
.stat-value {
  font-size: 28px;
  font-weight: 700;
}
.sub {
  font-size: 12px;
  color: #c0c4cc;
  margin-top: 4px;
}
</style>
