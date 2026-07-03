<template>
  <div>
    <div class="page-header">
      <h2>银行流水导入</h2>
    </div>
    <el-card>
      <template #header>银行流水列表</template>
      <div class="search-bar">
        <el-select v-model="search.status" placeholder="状态" clearable style="width:140px;">
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
        <el-table-column prop="amount" label="金额" width="110">
          <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="balance" label="余额" width="120">
          <template #default="{ row }">¥{{ row.balance?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="description" label="摘要" min-width="200" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Matched' ? 'success' : row.status === 'Reconciled' ? 'primary' : 'info'" size="small">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
      </el-table>
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
        <el-button type="primary" @click="createReconciliation">创建</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { getBankStatements, createBankReconciliation } from '@/api'
import { ElMessage } from 'element-plus'

const loading = ref(false)
const statements = ref([])
const search = reactive({ status: '' })
const showCreateReconciliation = ref(false)
const reconForm = reactive({ startDate: '', endDate: '', openingBalance: 0, closingBalance: 0 })

async function loadData() {
  loading.value = true
  try {
    const res = await getBankStatements({ status: search.status || undefined })
    statements.value = res
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

async function createReconciliation() {
  try {
    await createBankReconciliation({ ...reconForm, companyId: JSON.parse(localStorage.getItem('user') || '{}').defaultCompanyId, status: 'InProgress' })
    ElMessage.success('对账已创建')
    showCreateReconciliation.value = false
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  }
}

onMounted(loadData)
</script>
