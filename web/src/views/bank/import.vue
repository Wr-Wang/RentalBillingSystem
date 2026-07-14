<template>
  <div>
    <div class="page-header">
      <h2>银行流水导入</h2>
    </div>
    <el-card>
      <template #header>银行流水列表</template>
      <div class="search-bar">
        <el-select v-model="search.status" placeholder="状态" clearable style="width:140px;" @change="loadData">
          <el-option label="未匹配" value="Unmatched" />
          <el-option label="已匹配" value="Matched" />
          <el-option label="已对账" value="Reconciled" />
        </el-select>
        <el-button type="primary" @click="loadData">查询</el-button>
        <el-button type="success" @click="showCreateReconciliation = true">新建对账</el-button>
      </div>
      <el-table :data="statements" stripe v-loading="loading">
        <el-table-column prop="transactionDate" label="交易日期" width="120" />
        <el-table-column prop="referenceNo" label="交易号" width="180" />
        <el-table-column prop="counterparty" label="对方" width="120" />
        <el-table-column label="金额" width="110">
          <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="余额" width="120">
          <template #default="{ row }">¥{{ (row.balance || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="description" label="摘要" min-width="200" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Matched' ? 'success' : row.status === 'Reconciled' ? 'primary' : 'info'" size="small">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
      </el-table>

      <div style="margin-top:16px;text-align:right;">
        <el-pagination
          v-model:page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next"
          @current-change="loadData"
          @size-change="loadData"
        />
      </div>
    </el-card>

    <el-dialog v-model="showCreateReconciliation" title="新建对账" width="500px">
      <el-form :model="reconForm" label-width="100px">
        <el-form-item label="开始日期"><el-date-picker v-model="reconForm.startDate" type="date" style="width:100%;" /></el-form-item>
        <el-form-item label="结束日期"><el-date-picker v-model="reconForm.endDate" type="date" style="width:100%;" /></el-form-item>
        <el-form-item label="期初余额"><el-input-number v-model="reconForm.openingBalance" :min="0" style="width:100%;" /></el-form-item>
        <el-form-item label="期末余额"><el-input-number v-model="reconForm.closingBalance" :min="0" style="width:100%;" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCreateReconciliation = false">取消</el-button>
        <el-button type="primary" :loading="creating" @click="createReconciliation">创建</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useUserStore } from '@/store/user'
import { getBankStatements, getBankReconciliations, createBankReconciliation } from '@/api'
import { ElMessage } from 'element-plus'

const userStore = useUserStore()
const loading = ref(false)
const creating = ref(false)
const statements = ref([])
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const search = reactive({ status: '' })
const showCreateReconciliation = ref(false)
const reconForm = reactive({ startDate: '', endDate: '', openingBalance: 0, closingBalance: 0 })

function getEffectiveCompanyId() {
  return userStore.effectiveCompanyId
}

async function loadData() {
  loading.value = true
  try {
    const companyId = getEffectiveCompanyId()
    const res = await getBankStatements({
      companyId: companyId || undefined,
      status: search.status || undefined
    })
    let items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    total.value = items.length
    const start = (page.value - 1) * pageSize.value
    statements.value = items.slice(start, start + pageSize.value)
  } catch { /* 静默 */ }
  finally { loading.value = false }
}

async function createReconciliation() {
  const companyId = getEffectiveCompanyId()
  if (!companyId) { ElMessage.warning('请先选择公司'); return }
  creating.value = true
  try {
    await createBankReconciliation({
      startDate: reconForm.startDate || undefined,
      endDate: reconForm.endDate || undefined,
      openingBalance: reconForm.openingBalance,
      closingBalance: reconForm.closingBalance,
      companyId,
      status: 'InProgress'
    })
    ElMessage.success('对账已创建')
    showCreateReconciliation.value = false
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  }
  creating.value = false
}

onMounted(loadData)
</script>
