<template>
  <div>
    <!-- Page Header -->
    <div class="page-header">
      <h2>总账</h2>
      <div class="header-actions">
        <el-button-group>
          <el-button size="small" @click="prevMonth"><el-icon><ArrowLeft /></el-icon></el-button>
          <el-button size="small" disabled style="width: 140px; font-weight: 600;">
            {{ periodLabel }}
          </el-button>
          <el-button size="small" @click="nextMonth"><el-icon><ArrowRight /></el-icon></el-button>
        </el-button-group>
      </div>
    </div>

    <!-- Search / Filter Bar -->
    <div class="filter-bar">
      <el-form :inline="true" :model="filter" size="small" label-width="auto">
        <div class="filter-row">
          <el-form-item label="期间">
            <el-date-picker v-model="filter.period" type="month" placeholder="选择期间" value-format="YYYY-MM"
              style="width: 140px;" @change="fetchData" />
          </el-form-item>
          <el-form-item label="层级">
            <el-select v-model="filter.subjectLevel" placeholder="全部" clearable filterable style="width: 95px;">
              <el-option label="全部" :value="null" />
              <el-option label="1级" :value="1" />
              <el-option label="2级" :value="2" />
              <el-option label="3级" :value="3" />
              <el-option label="叶子" :value="4" />
            </el-select>
          </el-form-item>
          <el-form-item label="来源">
            <el-select v-model="filter.sourceType" placeholder="全部" clearable filterable style="width: 105px;">
              <el-option label="全部" value="" />
              <el-option label="收款" value="Receipt" />
              <el-option label="过账" value="JournalPost" />
              <el-option label="出账" value="BillJob" />
              <el-option label="冲销" value="Reverse" />
              <el-option label="结算" value="SettleOffset" />
            </el-select>
          </el-form-item>
        </div>
        <div class="filter-row" style="margin-top: 8px;">
          <el-form-item label="科目">
            <el-select v-model="filter.subjectCode" filterable clearable placeholder="选择科目" style="width: 220px;">
              <el-option v-for="s in subjectOptions" :key="s.code" :label="s.code + ' ' + s.name" :value="s.code" />
            </el-select>
          </el-form-item>
          <el-form-item label="合同">
            <el-select v-model="filter.contractNo" filterable clearable placeholder="合同号" style="width: 180px;">
              <el-option v-for="c in contractOptions" :key="c.id" :label="c.contractNo + ' - ' + c.tenantName" :value="c.contractNo" />
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-checkbox v-model="filter.hideZero" label="隐藏零余额" />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="fetchData">查询</el-button>
            <el-button @click="exportExcel" :disabled="!hasData">导出</el-button>
            <el-button @click="fetchData"><el-icon><Refresh /></el-icon></el-button>
          </el-form-item>
        </div>
      </el-form>
    </div>

    <!-- Stats Summary -->
    <el-row :gutter="12" class="stats-row">
      <el-col :span="6">
        <el-card shadow="never">
          <div class="stat-label">期初借方</div>
          <div class="stat-value" style="color: #409eff;">¥{{ fmt(totals.openingDebit) }}</div>
          <div class="stat-count">{{ itemsFlat.length }} 个科目</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never">
          <div class="stat-label">期初贷方</div>
          <div class="stat-value" style="color: #e6a23c;">¥{{ fmt(totals.openingCredit) }}</div>
          <div class="stat-count">{{ itemsFlat.length }} 个科目</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never">
          <div class="stat-label">本期借方</div>
          <div class="stat-value" style="color: #67c23a;">¥{{ fmt(totals.periodDebit) }}</div>
          <div class="stat-count">{{ itemsFlat.length }} 个科目</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never">
          <div class="stat-label">本期贷方</div>
          <div class="stat-value" style="color: #f56c6c;">¥{{ fmt(totals.periodCredit) }}</div>
          <div class="stat-count">{{ itemsFlat.length }} 个科目</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Balance Check -->
    <div class="balance-check">
      <span class="check-item" :class="balanceCheck.opening ? 'pass' : 'fail'">
        期初平衡: {{ balanceCheck.opening ? '✓' : '✗' }}
        <span v-if="!balanceCheck.opening" class="diff">差额 ¥{{ fmt(balanceCheck.openingDiff) }}</span>
      </span>
      <span class="check-item" :class="balanceCheck.period ? 'pass' : 'fail'">
        本期平衡: {{ balanceCheck.period ? '✓' : '✗' }}
        <span v-if="!balanceCheck.period" class="diff">差额 ¥{{ fmt(balanceCheck.periodDiff) }}</span>
      </span>
      <span class="check-item" :class="balanceCheck.closing ? 'pass' : 'fail'">
        期末平衡: {{ balanceCheck.closing ? '✓' : '✗' }}
        <span v-if="!balanceCheck.closing" class="diff">差额 ¥{{ fmt(balanceCheck.closingDiff) }}</span>
      </span>
      <span v-if="balanceCheck.all" class="check-all">✓ 借贷平衡</span>
    </div>

    <!-- Main Table -->
    <el-card shadow="never" v-loading="loading">
      <el-empty v-if="!loading && itemsFlat.length === 0" description="当前期间无总账数据" />
      <el-table
        v-else
        ref="tableRef"
        :data="treeData"
        v-loading="loading"
        stripe
        row-key="code"
        :tree-props="{ children: 'children' }"
        default-expand-all
        show-summary
        :summary-method="summaryMethod"
        highlight-current-row
        @row-click="handleRowClick"
        style="width: 100%;"
      >
        <el-table-column label="科目编码" min-width="280" sortable>
          <template #default="{ row }">
            <span :style="{ paddingLeft: (row.level - 1) * 1.2 + 'em' }">{{ row.code }} {{ row.name }}</span>
          </template>
        </el-table-column>
        <el-table-column label="方向" width="60" align="center" prop="direction">
          <template #default="{ row }">{{ row.direction === 'Debit' ? '借' : '贷' }}</template>
        </el-table-column>
        <el-table-column width="150" align="right" sortable prop="openingDebit">
          <template #header>
            <span>期初借方 <el-tooltip content="本期间开始时的借方余额" placement="top"><el-icon style="margin-left:2px;color:#c0c4cc;font-size:13px;vertical-align:-2px;"><InfoFilled /></el-icon></el-tooltip></span>
          </template>
          <template #default="{ row }">{{ cellValue(row.openingDebit) }}</template>
        </el-table-column>
        <el-table-column width="150" align="right" sortable prop="openingCredit">
          <template #header>
            <span>期初贷方 <el-tooltip content="本期间开始时的贷方余额" placement="top"><el-icon style="margin-left:2px;color:#c0c4cc;font-size:13px;vertical-align:-2px;"><InfoFilled /></el-icon></el-tooltip></span>
          </template>
          <template #default="{ row }">{{ cellValue(row.openingCredit) }}</template>
        </el-table-column>
        <el-table-column width="150" align="right" sortable prop="periodDebit">
          <template #header>
            <span>本期借方 <el-tooltip content="本期间内发生的借方发生额合计" placement="top"><el-icon style="margin-left:2px;color:#c0c4cc;font-size:13px;vertical-align:-2px;"><InfoFilled /></el-icon></el-tooltip></span>
          </template>
          <template #default="{ row }">{{ cellValue(row.periodDebit) }}</template>
        </el-table-column>
        <el-table-column width="150" align="right" sortable prop="periodCredit">
          <template #header>
            <span>本期贷方 <el-tooltip content="本期间内发生的贷方发生额合计" placement="top"><el-icon style="margin-left:2px;color:#c0c4cc;font-size:13px;vertical-align:-2px;"><InfoFilled /></el-icon></el-tooltip></span>
          </template>
          <template #default="{ row }">{{ cellValue(row.periodCredit) }}</template>
        </el-table-column>
        <el-table-column width="150" align="right" sortable prop="ytdDebit">
          <template #header>
            <span>累计借方 <el-tooltip content="年初至本期末的累计借方发生额" placement="top"><el-icon style="margin-left:2px;color:#c0c4cc;font-size:13px;vertical-align:-2px;"><InfoFilled /></el-icon></el-tooltip></span>
          </template>
          <template #default="{ row }">{{ cellValue(row.ytdDebit) }}</template>
        </el-table-column>
        <el-table-column width="150" align="right" sortable prop="ytdCredit">
          <template #header>
            <span>累计贷方 <el-tooltip content="年初至本期末的累计贷方发生额" placement="top"><el-icon style="margin-left:2px;color:#c0c4cc;font-size:13px;vertical-align:-2px;"><InfoFilled /></el-icon></el-tooltip></span>
          </template>
          <template #default="{ row }">{{ cellValue(row.ytdCredit) }}</template>
        </el-table-column>
        <el-table-column width="150" align="right" sortable prop="closingDebit">
          <template #header>
            <span>期末借方 <el-tooltip content="本期间结束时的借方余额 = 期初借方 + 本期借方 - 期初贷方 - 本期贷方" placement="top"><el-icon style="margin-left:2px;color:#c0c4cc;font-size:13px;vertical-align:-2px;"><InfoFilled /></el-icon></el-tooltip></span>
          </template>
          <template #default="{ row }">
            <span :style="{ color: row.closingDebit > 0 ? '#67c23a' : row.closingDebit < 0 ? '#f56c6c' : '#c0c4cc' }">
              {{ cellValue(row.closingDebit) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column width="150" align="right" sortable prop="closingCredit">
          <template #header>
            <span>期末贷方 <el-tooltip content="本期间结束时的贷方余额 = 期初贷方 + 本期贷方 - 期初借方 - 本期借方" placement="top"><el-icon style="margin-left:2px;color:#c0c4cc;font-size:13px;vertical-align:-2px;"><InfoFilled /></el-icon></el-tooltip></span>
          </template>
          <template #default="{ row }">
            <span :style="{ color: row.closingCredit > 0 ? '#e6a23c' : row.closingCredit < 0 ? '#f56c6c' : '#c0c4cc' }">
              {{ cellValue(row.closingCredit) }}
            </span>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Detail Drawer -->
    <el-drawer v-model="drawerVisible" :title="drawerTitle" direction="rtl" size="520px">
      <template v-if="drawerData">
        <div class="drawer-balance">
          <div class="drawer-subject">{{ drawerData.subjectCode }} {{ drawerData.subjectName }}</div>
          <div class="drawer-period">{{ drawerData.period }}</div>
          <el-row :gutter="8" class="drawer-summary">
            <el-col :span="8">
              期初<br><strong>{{ fmt(drawerData.openingDebit - drawerData.openingCredit) }}</strong>
            </el-col>
            <el-col :span="8">
              本期借<br><strong style="color:#67c23a;">{{ fmt(drawerData.periodDebit) }}</strong>
            </el-col>
            <el-col :span="8">
              本期贷<br><strong style="color:#f56c6c;">{{ fmt(drawerData.periodCredit) }}</strong>
            </el-col>
          </el-row>
          <div class="drawer-closing">
            期末余额: <strong>¥{{ fmt(drawerData.closingDebit - drawerData.closingCredit) }}</strong>
            <span style="margin-left: 8px; color: #909399;">
              (借 {{ fmt(drawerData.closingDebit) }} / 贷 {{ fmt(drawerData.closingCredit) }})
            </span>
          </div>
        </div>

        <div v-for="group in drawerData.groupedByContract" :key="group.contractNo" class="drawer-group">
          <div class="drawer-group-title">
            合同: {{ group.contractNo }}
            <span style="float:right;font-weight:400;font-size:13px;">
              借 {{ fmt(group.subtotalDebit) }} / 贷 {{ fmt(group.subtotalCredit) }}
            </span>
          </div>
          <el-table :data="group.entries" stripe size="small" style="width:100%;">
            <el-table-column label="日期" width="100">
              <template #default="{ row }">{{ row.date }}</template>
            </el-table-column>
            <el-table-column label="合同" width="200" prop="contractNo" />
            <el-table-column label="来源" width="80">
              <template #default="{ row }">
                <el-tag :type="sourceTagType(row.sourceType)" size="small">{{ sourceLabel(row.sourceType) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="摘要" min-width="160" show-overflow-tooltip prop="description" />
            <el-table-column label="借方" width="120" align="right">
              <template #default="{ row }">{{ row.direction === 'Debit' ? fmt(row.amount) : '-' }}</template>
            </el-table-column>
            <el-table-column label="贷方" width="120" align="right">
              <template #default="{ row }">{{ row.direction === 'Credit' ? fmt(row.amount) : '-' }}</template>
            </el-table-column>
          </el-table>
        </div>
      </template>

      <template #footer>
        <el-button @click="goToLedger">跳转明细账</el-button>
        <el-button @click="drawerVisible = false">关闭</el-button>
      </template>
    </el-drawer>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { ArrowLeft, ArrowRight, Refresh, InfoFilled } from '@element-plus/icons-vue'
import { getGLBalance, getGLDetail, getAccountingSubjects, getContracts } from '@/api'
import { useUserStore } from '@/store/user'
import { exportToExcel } from '@/utils/exportExcel'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()
const tableRef = ref(null)
const loading = ref(false)
const treeData = ref([])
const itemsFlat = ref([])
const totals = reactive({ openingDebit: 0, openingCredit: 0, periodDebit: 0, periodCredit: 0, ytdDebit: 0, ytdCredit: 0, closingDebit: 0, closingCredit: 0 })
const subjectOptions = ref([])
const contractOptions = ref([])
const drawerVisible = ref(false)
const drawerData = ref(null)
const hasData = computed(() => itemsFlat.value.length > 0)

// 筛选条件
const filter = reactive({
  period: '',
  subjectLevel: null,
  subjectCode: '',
  contractNo: '',
  sourceType: '',
  hideZero: true
})

// 期间标签
const periodLabel = computed(() => {
  if (!filter.period) return '请选择期间'
  const [y, m] = filter.period.split('-')
  return `${y}年${parseInt(m)}月`
})

// 借贷平衡校验
const balanceCheck = computed(() => {
  const opDiff = Math.abs(totals.openingDebit - totals.openingCredit)
  const pdDiff = Math.abs(totals.periodDebit - totals.periodCredit)
  const clDiff = Math.abs(totals.closingDebit - totals.closingCredit)
  return {
    opening: opDiff < 0.01,
    period: pdDiff < 0.01,
    closing: clDiff < 0.01,
    all: opDiff < 0.01 && pdDiff < 0.01 && clDiff < 0.01,
    openingDiff: totals.openingDebit - totals.openingCredit,
    periodDiff: totals.periodDebit - totals.periodCredit,
    closingDiff: totals.closingDebit - totals.closingCredit
  }
})

// 抽屉标题
const drawerTitle = computed(() => {
  if (!drawerData.value) return ''
  return `${drawerData.value.subjectCode} ${drawerData.value.subjectName} · ${drawerData.value.period}`
})

// ========== 工具函数 ==========

function fmt(v) {
  return (v || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 })
}

function cellValue(v) {
  return v ? `¥${fmt(v)}` : '-'
}

function sourceLabel(s) {
  const map = { Receipt: '收款', JournalPost: '过账', BillJob: '出账', Reverse: '冲销', SettleOffset: '结算' }
  return map[s] || s
}

function sourceTagType(s) {
  const map = { Receipt: 'success', JournalPost: 'primary', BillJob: 'warning', Reverse: 'danger', SettleOffset: 'info' }
  return map[s] || ''
}

function flattenTree(items) {
  const flat = []
  function walk(list) {
    for (const item of list) {
      flat.push(item)
      if (item.children && item.children.length > 0) walk(item.children)
    }
  }
  walk(items)
  return flat
}

// ========== 数据加载 ==========

async function loadSubjects() {
  try {
    const res = await getAccountingSubjects()
    subjectOptions.value = Array.isArray(res) ? res : []
  } catch { subjectOptions.value = [] }
}

async function loadContracts() {
  try {
    const res = await getContracts({ pageSize: 200 })
    const items = res.items || res.data || []
    contractOptions.value = items.map(c => ({
      id: c.id,
      contractNo: c.contractNo,
      tenantName: c.tenants?.length > 0 ? c.tenants[0].tenantName : ''
    }))
  } catch { contractOptions.value = [] }
}

async function fetchData() {
  if (!filter.period) {
    ElMessage.warning('请选择会计期间')
    return
  }
  loading.value = true
  try {
    const params = {
      period: filter.period,
      hideZero: filter.hideZero
    }
    if (filter.subjectLevel) params.subjectLevel = filter.subjectLevel
    if (filter.subjectCode) params.subjectCode = filter.subjectCode
    if (filter.contractNo) params.contractNo = filter.contractNo
    if (filter.sourceType) params.sourceType = filter.sourceType

    const res = await getGLBalance(params)
    treeData.value = Array.isArray(res.items) ? res.items : []
    itemsFlat.value = flattenTree(treeData.value)
    Object.assign(totals, res.totals || {})
    syncUrl()
  } catch (e) {
    ElMessage.error('加载总账失败')
    console.error(e)
  } finally { loading.value = false }
}

async function handleRowClick(row) {
  if (!filter.period || !row.code) return
  try {
    const params = { period: filter.period, subjectCode: row.code }
    if (filter.contractNo) params.contractNo = filter.contractNo
    const res = await getGLDetail(params)
    drawerData.value = res
    drawerVisible.value = true
  } catch {
    ElMessage.error('加载明细失败')
  }
}

function goToLedger() {
  if (!drawerData.value) return
  drawerVisible.value = false
  router.push({ name: 'AccountingLedger', query: { subjectCode: drawerData.value.subjectCode } })
}

// ========== 期间快捷切换 ==========

function shiftMonth(p, delta) {
  if (!p) {
    const d = new Date()
    return `${d.getFullYear()}-${String(d.getMonth() + 1 + delta).padStart(2, '0')}`
  }
  const [y, m] = p.split('-').map(Number)
  const total = y * 12 + (m - 1) + delta
  const ny = Math.floor(total / 12)
  const nm = total % 12 + 1
  return `${ny}-${String(nm).padStart(2, '0')}`
}

function prevMonth() {
  filter.period = shiftMonth(filter.period, -1)
  fetchData()
}

function nextMonth() {
  filter.period = shiftMonth(filter.period, 1)
  fetchData()
}

// ========== URL 参数同步 ==========

function syncUrl() {
  const query = {}
  if (filter.period) query.period = filter.period
  if (filter.subjectCode) query.subjectCode = filter.subjectCode
  if (filter.contractNo) query.contractNo = filter.contractNo
  if (filter.sourceType) query.sourceType = filter.sourceType
  if (filter.subjectLevel) query.subjectLevel = String(filter.subjectLevel)
  if (!filter.hideZero) query.hideZero = '0'
  router.replace({ query })
}

function restoreFromUrl() {
  const q = route.query
  if (q.period) filter.period = q.period
  if (q.subjectCode) filter.subjectCode = q.subjectCode
  if (q.contractNo) filter.contractNo = q.contractNo
  if (q.sourceType) filter.sourceType = q.sourceType
  if (q.subjectLevel) filter.subjectLevel = parseInt(q.subjectLevel)
  if (q.hideZero === '0') filter.hideZero = false
}

// ========== 合计行 ==========

function summaryMethod() {
  const t = totals
  return [
    null, '合计', '',
    `¥${fmt(t.openingDebit)}`,
    `¥${fmt(t.openingCredit)}`,
    `¥${fmt(t.periodDebit)}`,
    `¥${fmt(t.periodCredit)}`,
    `¥${fmt(t.ytdDebit)}`,
    `¥${fmt(t.ytdCredit)}`,
    `¥${fmt(t.closingDebit)}`,
    `¥${fmt(t.closingCredit)}`
  ]
}

// ========== 导出 Excel ==========

function exportExcel() {
  const rows = itemsFlat.value.map(r => [
    r.code, r.name, r.direction === 'Debit' ? '借' : '贷',
    r.openingDebit, r.openingCredit, r.periodDebit, r.periodCredit,
    r.ytdDebit, r.ytdCredit, r.closingDebit, r.closingCredit
  ])
  exportToExcel([
    {
      name: '总账汇总',
      columns: ['科目编码', '科目名称', '方向', '期初借方', '期初贷方', '本期借方', '本期贷方', '累计借方', '累计贷方', '期末借方', '期末贷方'],
      rows
    }
  ], `总账_${filter.period}`)
}

// ========== 打印 ==========

function handlePrint() {
  window.print()
}

// ========== 初始化 ==========

onMounted(() => {
  restoreFromUrl()
  loadSubjects()
  loadContracts()
  if (!filter.period) {
    const d = new Date()
    filter.period = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
  }
  // 立即加载一次（watch 在 setup 阶段已触发，但当时 period 可能还未设置）
  fetchData()
})

// 监听公司视角就绪后自动加载
watch(() => userStore.effectiveCompanyId, (newId) => {
  if (newId && filter.period) fetchData()
}, { immediate: true })
</script>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}
.filter-bar {
  margin-bottom: 16px;
  padding: 12px 16px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.04);
}
.filter-bar :deep(.el-form-item) {
  margin-bottom: 0;
}
.filter-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.filter-row :deep(.el-form-item) {
  margin-right: 0;
}
.stats-row {
  margin-bottom: 8px;
}
.stat-label {
  font-size: 13px;
  color: #909399;
  margin-bottom: 4px;
}
.stat-value {
  font-size: 22px;
  font-weight: 700;
}
.stat-count {
  font-size: 11px;
  color: #c0c4cc;
  margin-top: 2px;
}
.balance-check {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 8px 16px;
  margin-bottom: 12px;
  background: #fafafa;
  border-radius: 4px;
  font-size: 13px;
}
.check-item {
  display: flex;
  align-items: center;
  gap: 4px;
}
.check-item.pass { color: #67c23a; }
.check-item.fail { color: #f56c6c; }
.diff { font-size: 12px; opacity: 0.8; }
.check-all {
  color: #67c23a;
  font-weight: 600;
  margin-left: auto;
}
.drawer-balance {
  padding: 16px;
  background: #f5f7fa;
  border-radius: 8px;
  margin-bottom: 16px;
}
.drawer-subject {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 4px;
}
.drawer-period {
  font-size: 13px;
  color: #909399;
  margin-bottom: 12px;
}
.drawer-summary {
  text-align: center;
  font-size: 13px;
}
.drawer-summary strong {
  font-size: 16px;
}
.drawer-closing {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #e4e7ed;
  text-align: center;
  font-size: 14px;
}
.drawer-group {
  margin-bottom: 16px;
}
.drawer-group-title {
  font-size: 14px;
  font-weight: 600;
  padding: 8px 12px;
  background: #f0f5ff;
  border-radius: 4px;
  margin-bottom: 4px;
}
</style>

<style>
/* 打印样式 */
@media print {
  .page-header .header-actions,
  .search-bar,
  .stats-row,
  .balance-check,
  .el-table__header-wrapper { background: #fff !important; }
  .el-card { box-shadow: none !important; border: none !important; }
  .el-table__footer-wrapper { display: table-row-group; }
}
</style>
