<template>
  <div>
    <div class="page-header"><h2>自动匹配</h2><el-button type="primary" @click="autoMatch">自动匹配</el-button></div>
    <el-table :data="statements" stripe v-loading="loading">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="referenceNo" label="交易号" width="180" />
      <el-table-column prop="counterparty" label="付款人" width="120" />
      <el-table-column prop="amount" label="金额" width="110">
        <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="description" label="摘要" min-width="200" />
      <el-table-column prop="status" label="状态" width="100">
        <template #default="{ row }"><el-tag :type="row.status === 'Matched' ? 'success' : 'info'" size="small">{{ row.status }}</el-tag></template>
      </el-table-column>
    </el-table>
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue'
import { getBankStatements, autoMatchBank } from '@/api'
import { ElMessage } from 'element-plus'
const loading = ref(false)
const statements = ref([])
async function loadData() {
  loading.value = true
  try {
    const res = await getBankStatements({ status: 'Unmatched' })
    statements.value = res
  } catch (e) { console.error(e) }
  finally { loading.value = false }
}
async function autoMatch() {
  try {
    const recons = await getBankStatements({})
    if (recons.length === 0) { ElMessage.warning('请先在流水导入页面创建对账'); return }
    const res = await autoMatchBank(recons[0].id)
    ElMessage.success(`自动匹配完成，共匹配 ${res.matched || 0} 条`)
    await loadData()
  } catch (e) { ElMessage.error(e?.response?.data?.message || '匹配失败') }
}
onMounted(loadData)
</script>
