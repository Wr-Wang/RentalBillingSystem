<template>
  <div>
    <div class="page-header">
      <h2>余额调节表</h2>
      <div class="table-actions">
        <el-button @click="loadData"><el-icon><Refresh /></el-icon>刷新</el-button>
      </div>
    </div>

    <div class="search-bar">
      <el-select v-model="selectedReconId" placeholder="选择对账会话" clearable filterable style="width:300px;" @change="loadReconDetail">
        <el-option v-for="r in reconciliations" :key="r.id" :label="r.startDate + ' ~ ' + r.endDate + ' (' + (r.status || '') + ')'" :value="r.id" />
      </el-select>
    </div>

    <el-row :gutter="16">
      <el-col :span="12">
        <el-card>
          <template #header>银行存款余额调节表</template>
          <el-descriptions :column="1" border>
            <el-descriptions-item label="银行对账单余额">
              <span style="font-weight: bold; font-size: 16px;">¥{{ formatMoney(bankBalance) }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="加：企收银未收">
              <span>¥{{ formatMoney(enterpriseReceiptNotBank) }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="减：企付银未付">
              <span>¥{{ formatMoney(enterprisePaymentNotBank) }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="调节后银行余额">
              <span style="font-weight: bold; color: #67c23a; font-size: 16px;">¥{{ formatMoney(adjustedBankBalance) }}</span>
            </el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>企业账面余额调节表</template>
          <el-descriptions :column="1" border>
            <el-descriptions-item label="企业账面余额">
              <span style="font-weight: bold; font-size: 16px;">¥{{ formatMoney(bookBalance) }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="加：银收企未收">
              <span>¥{{ formatMoney(bankReceiptNotEnterprise) }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="减：银付企未付">
              <span>¥{{ formatMoney(bankPaymentNotEnterprise) }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="调节后企业余额">
              <span style="font-weight: bold; color: #67c23a; font-size: 16px;">¥{{ formatMoney(adjustedBookBalance) }}</span>
            </el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>

    <el-card style="margin-top: 16px;">
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span>调节结果与未达账项</span>
          <el-tag :type="isBalanced ? 'success' : 'danger'" size="medium">
            差额：¥{{ formatMoney(Math.abs(adjustedBankBalance - adjustedBookBalance)) }}
            {{ isBalanced ? '✅ 平衡' : '❌ 不平衡' }}
          </el-tag>
        </div>
      </template>

      <el-descriptions :column="2" border style="margin-bottom:16px;">
        <el-descriptions-item label="银行调节后余额"><span style="font-weight:bold;">¥{{ formatMoney(adjustedBankBalance) }}</span></el-descriptions-item>
        <el-descriptions-item label="企业调节后余额"><span style="font-weight:bold;">¥{{ formatMoney(adjustedBookBalance) }}</span></el-descriptions-item>
      </el-descriptions>

      <el-table :data="unreconciledItems" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column label="类型" width="120">
          <template #default="{ row }">
            <el-tag :type="row.type === '企收银未收' ? 'warning' : 'info'" size="small">{{ row.type }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="说明" min-width="250" />
        <el-table-column label="金额" width="110">
          <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="date" label="日期" width="100" />
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getBankReconciliations, getBankStatements, getBankMatches } from '@/api'
import { useUserStore } from '@/store/user'

const userStore = useUserStore()
const reconciliations = ref([])
const selectedReconId = ref('')
const statements = ref([])
const matches = ref([])
const loading = ref(false)

function formatMoney(v) { return (v || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }

// 当前选中的对账会话
const selectedRecon = computed(() =>
  reconciliations.value.find(r => r.id === selectedReconId.value)
)

// 银行对账单余额 = 对账会话的期末余额
const bankBalance = computed(() => selectedRecon.value?.closingBalance || selectedRecon.value?.closingbalance || 0)

// 企业账面余额 = 对账会话的系统总额（已匹配收款总额）
const bookBalance = computed(() => selectedRecon.value?.systemTotal || selectedRecon.value?.systemtotal || 0)

// 已匹配的流水 ID 集合
const matchedStatementIds = computed(() => new Set(matches.value.map(m => m.bankStatementId || m.bankstatementid)))

// 企收银未收 = 未匹配的正数金额流水
const enterpriseReceiptNotBank = computed(() =>
  statements.value
    .filter(s => !matchedStatementIds.value.has(s.id) && (s.amount || 0) > 0)
    .reduce((s, r) => s + (r.amount || 0), 0)
)

// 企付银未付 = 未匹配的负数金额流水
const enterprisePaymentNotBank = computed(() =>
  statements.value
    .filter(s => !matchedStatementIds.value.has(s.id) && (s.amount || 0) < 0)
    .reduce((s, r) => s + Math.abs(r.amount || 0), 0)
)

const bankReceiptNotEnterprise = computed(() => enterprisePaymentNotBank)
const bankPaymentNotEnterprise = computed(() => enterpriseReceiptNotBank)

const adjustedBankBalance = computed(() => bankBalance.value + enterpriseReceiptNotBank.value - enterprisePaymentNotBank.value)
const adjustedBookBalance = computed(() => bookBalance.value + bankReceiptNotEnterprise.value - bankPaymentNotEnterprise.value)

const isBalanced = computed(() => Math.abs(adjustedBankBalance.value - adjustedBookBalance.value) < 0.01)

const unreconciledItems = computed(() => {
  return statements.value
    .filter(s => !matchedStatementIds.value.has(s.id))
    .map(s => ({
      type: (s.amount || 0) > 0 ? '企收银未收' : '银付企未付',
      description: s.description || '',
      amount: Math.abs(s.amount || 0),
      date: s.transactionDate || ''
    }))
})

async function loadData() {
  loading.value = true
  try {
    const companyId = userStore.effectiveCompanyId
    const [reconRes, stmtRes] = await Promise.all([
      getBankReconciliations({ companyId: companyId || undefined }),
      getBankStatements({ companyId: companyId || undefined })
    ])
    reconciliations.value = Array.isArray(reconRes) ? reconRes : (reconRes.items || reconRes.data || [])
    statements.value = Array.isArray(stmtRes) ? stmtRes : (stmtRes.items || stmtRes.data || [])

    if (reconciliations.value.length > 0 && !selectedReconId.value) {
      selectedReconId.value = reconciliations.value[0].id
    }
    if (selectedReconId.value) await loadReconDetail()
  } catch { ElMessage.error('加载数据失败') }
  finally { loading.value = false }
}

async function loadReconDetail() {
  if (!selectedReconId.value) { matches.value = []; return }
  try {
    const res = await getBankMatches({})
    matches.value = Array.isArray(res) ? res : (res.items || res.data || [])
  } catch { /* 静默 */ }
}

onMounted(loadData)
</script>
