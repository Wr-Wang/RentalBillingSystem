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

    <!-- 逾期分段统计卡片 -->
    <div class="stat-cards">
      <div class="severity-card" v-for="(s, i) in severityStats" :key="i" :style="'--card-color:' + s.color" @click="activeSeverity = s.key">
        <div class="card-top">
          <span class="card-count">{{ s.count }}</span>
          <span class="card-unit">户</span>
        </div>
        <div class="card-label">{{ s.name }}</div>
        <div class="card-amount">
          <span class="amount-label">欠费合计</span>
          <span class="amount-value">¥{{ s.totalAmount.toLocaleString() }}</span>
        </div>
      </div>
    </div>

    <!-- 快捷筛选 -->
    <el-card shadow="never" style="margin-bottom: 16px;">
      <div class="filter-bar">
        <el-radio-group v-model="activeSeverity" @change="onSeverityChange">
          <el-radio-button value="">全部逾期 ({{ allCount }})</el-radio-button>
          <el-radio-button value="mild">轻微 1个月以内 ({{ mildCount }})</el-radio-button>
          <el-radio-button value="moderate">中度 1-3个月 ({{ moderateCount }})</el-radio-button>
          <el-radio-button value="severe">严重 3个月+ ({{ severeCount }})</el-radio-button>
        </el-radio-group>
        <el-input v-model="searchKeyword" placeholder="合同号/租客" clearable style="width:200px;margin-left:auto;" @keyup.enter="loadData" />
      </div>
    </el-card>

    <!-- 逾期合同列表 -->
    <el-card shadow="never">
      <el-table :data="displayList" v-loading="loading" stripe size="small" @row-click="openManualDialog">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="contractNo" label="合同号" width="180" />
        <el-table-column prop="tenantName" label="租客" width="100" />
        <el-table-column prop="roomName" label="房屋" width="120" />
        <el-table-column label="欠费金额" width="120" align="right">
          <template #default="{ row }">¥{{ (row.overdueAmount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="逾期天数" width="100" align="center">
          <template #default="{ row }">
            <span :style="{ fontWeight: 600, color: row.overdueDays > 90 ? '#f56c6c' : row.overdueDays > 30 ? '#e6a23c' : '#67c23a' }">
              {{ row.overdueDays }} 天
            </span>
          </template>
        </el-table-column>
        <el-table-column label="逾期程度" width="110" align="center">
          <template #default="{ row }">
            <el-tag :type="severityType(row.overdueDays)" size="small" effect="dark" style="width:80px;text-align:center;">
              {{ severityLabel(row.overdueDays) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" size="small" @click.stop="openManualDialog(row)">
              <el-icon><Bell /></el-icon>催缴
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div v-if="displayList.length === 0 && !loading" class="empty-state">
        <el-icon :size="32" style="color:#67c23a"><CircleCheckFilled /></el-icon>
        <span>当前筛选条件下无逾期合同</span>
      </div>

      <el-pagination
        v-if="total > pageSize"
        v-model:page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        style="margin-top: 16px; justify-content: flex-end;"
        @current-change="onPageChange"
        @size-change="onPageChange"
      />
    </el-card>

    <!-- 手动催缴对话框 -->
    <el-dialog v-model="dialogVisible" title="发送催缴通知" width="500px" :close-on-click-modal="false">
      <el-form :model="manualForm" label-width="90px">
        <el-form-item label="合同">
          <el-tag>{{ manualForm.contractNo }}</el-tag>
          <span style="margin-left:8px;color:#909399;">{{ manualForm.tenantName }}</span>
        </el-form-item>
        <el-form-item label="欠费金额">
          <span style="color:#f56c6c;font-weight:700;">¥{{ (manualForm.overdueAmount || 0).toLocaleString() }}</span>
          <span style="margin-left:8px;color:#909399;">逾期 {{ manualForm.overdueDays }} 天</span>
        </el-form-item>
        <el-form-item label="催缴方式">
          <el-select v-model="manualForm.channel" style="width:100%">
            <el-option label="短信" value="SMS" />
            <el-option label="电话" value="Phone" />
            <el-option label="系统通知" value="WeChat" />
          </el-select>
        </el-form-item>
        <el-form-item label="催缴内容">
          <el-input v-model="manualForm.content" type="textarea" :rows="4" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="sending" @click="sendManual">发送</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/user'
import { getOverdueDetail, manualCollection } from '@/api'

const userStore = useUserStore()
const loading = ref(false)
const sending = ref(false)
const allContracts = ref([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const activeSeverity = ref('')
const searchKeyword = ref('')
const dialogVisible = ref(false)

const manualForm = ref({
  contractId: '', contractNo: '', tenantName: '',
  overdueAmount: 0, overdueDays: 0,
  channel: 'SMS',
  content: '您好，您的房租已逾期，请尽快缴纳以免产生利息。'
})

// 逾期分段统计（按月）
const severityStats = computed(() => [
  { key: 'mild', name: '轻微逾期 (1个月以内)', count: mildCount.value, totalAmount: mildTotal.value,  color: '#67c23a' },
  { key: 'moderate', name: '中度逾期 (1-3个月)', count: moderateCount.value, totalAmount: moderateTotal.value, color: '#e6a23c' },
  { key: 'severe', name: '严重逾期 (3个月+)',   count: severeCount.value, totalAmount: severeTotal.value,  color: '#f56c6c' },
])

function severityType(days) {
  if (days <= 30) return 'success'
  if (days <= 90) return 'warning'
  return 'danger'
}
function severityLabel(days) {
  if (days <= 30) return '1个月以内'
  if (days <= 90) return '1-3个月'
  return '3个月+'
}

// 分段的合同
const mildContracts = computed(() => allContracts.value.filter(c => c.overdueDays >= 1 && c.overdueDays <= 30))
const moderateContracts = computed(() => allContracts.value.filter(c => c.overdueDays >= 31 && c.overdueDays <= 90))
const severeContracts = computed(() => allContracts.value.filter(c => c.overdueDays > 90))
const allCount = computed(() => allContracts.value.length)
const mildCount = computed(() => mildContracts.value.length)
const moderateCount = computed(() => moderateContracts.value.length)
const severeCount = computed(() => severeContracts.value.length)
const mildTotal = computed(() => mildContracts.value.reduce((s, c) => s + c.overdueAmount, 0))
const moderateTotal = computed(() => moderateContracts.value.reduce((s, c) => s + c.overdueAmount, 0))
const severeTotal = computed(() => severeContracts.value.reduce((s, c) => s + c.overdueAmount, 0))

// 筛选 + 分页
const filteredContracts = computed(() => {
  let items = allContracts.value
  if (activeSeverity.value === 'mild') items = items.filter(c => c.overdueDays >= 1 && c.overdueDays <= 30)
  else if (activeSeverity.value === 'moderate') items = items.filter(c => c.overdueDays >= 31 && c.overdueDays <= 90)
  else if (activeSeverity.value === 'severe') items = items.filter(c => c.overdueDays > 90)
  if (searchKeyword.value) {
    const kw = searchKeyword.value.toLowerCase()
    items = items.filter(c => (c.contractNo || '').toLowerCase().includes(kw) || (c.tenantName || '').toLowerCase().includes(kw))
  }
  return items
})

const displayList = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return filteredContracts.value.slice(start, start + pageSize.value)
})

function onSeverityChange() { page.value = 1 }
function onPageChange() { /* 分页自动响应 computed */ }

function openManualDialog(row) {
  if (!row) return
  manualForm.value = {
    contractId: row.contractId || row.id,
    contractNo: row.contractNo || '',
    tenantName: row.tenantName || '',
    overdueAmount: row.overdueAmount || 0,
    overdueDays: row.overdueDays || 0,
    channel: 'SMS',
    content: `您好，${row.contractNo} 的房租已逾期 ${row.overdueDays || 0} 天，欠费 ¥${(row.overdueAmount || 0).toLocaleString()}，请尽快缴纳。`
  }
  dialogVisible.value = true
}

async function loadData() {
  loading.value = true
  try {
    const companyId = userStore.effectiveCompanyId
    const params = companyId ? { companyId } : {}
    const res = await getOverdueDetail(params)
    const items = Array.isArray(res) ? res : (res.items || res.data || res || [])

    // 按合同去重（每个合同只保留一条，取最大逾期天数、汇总欠费金额）
    const map = new Map()
    for (const item of items) {
      const key = item.contractId || item.id
      if (!key) continue
      const days = item.daysOverdue || 0
      const amount = item.amount || 0
      if (map.has(key)) {
        const existing = map.get(key)
        existing.overdueDays = Math.max(existing.overdueDays, days)
        existing.overdueAmount += amount
      } else {
        map.set(key, {
          id: key,
          contractId: key,
          contractNo: item.contractNo || '',
          tenantName: item.tenantName || '',
          roomName: item.roomFullCode || item.roomName || '',
          overdueAmount: amount,
          overdueDays: days
        })
      }
    }
    allContracts.value = Array.from(map.values())
    total.value = allContracts.value.length
  } catch {
    ElMessage.error('加载逾期数据失败')
  }
  loading.value = false
}

async function sendManual() {
  if (!manualForm.value.contractId) { ElMessage.warning('请选择合同'); return }
  sending.value = true
  try {
    await manualCollection({ contractId: manualForm.value.contractId, channel: manualForm.value.channel, content: manualForm.value.content })
    ElMessage.success('催缴已发送')
    dialogVisible.value = false
  } catch {
    ElMessage.error('发送失败')
  }
  sending.value = false
}

onMounted(loadData)
</script>

<style scoped>
.stat-cards {
  display: flex;
  gap: 16px;
  margin-bottom: 16px;
}
.severity-card {
  flex: 1;
  background: #fff;
  border-radius: 12px;
  padding: 20px 24px;
  cursor: pointer;
  transition: all 0.25s ease;
  border: 1px solid #f0f0f0;
  position: relative;
  overflow: hidden;
}
.severity-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 4px;
  background: var(--card-color);
}
.severity-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0,0,0,0.08);
  border-color: var(--card-color);
}
.card-top {
  display: flex;
  align-items: baseline;
  gap: 4px;
  margin-bottom: 6px;
}
.card-count {
  font-size: 36px;
  font-weight: 800;
  color: var(--card-color);
  line-height: 1;
}
.card-unit {
  font-size: 14px;
  color: #909399;
  font-weight: 500;
}
.card-label {
  font-size: 13px;
  color: #606266;
  margin-bottom: 14px;
  font-weight: 500;
}
.card-amount {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 12px;
  border-top: 1px dashed #f0f0f0;
}
.amount-label {
  font-size: 12px;
  color: #c0c4cc;
}
.amount-value {
  font-size: 15px;
  font-weight: 700;
  color: #303133;
}
.filter-bar {
  display: flex;
  align-items: center;
  gap: 12px;
}
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 48px 16px;
  color: #909399;
  font-size: 14px;
}
</style>
