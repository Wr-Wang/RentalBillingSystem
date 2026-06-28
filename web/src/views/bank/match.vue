<template>
  <div>
    <div class="page-header">
      <h2>自动匹配</h2>
      <el-button type="primary" @click="autoMatch">自动匹配</el-button>
    </div>

    <el-table :data="unmatchedItems" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="transactionRef" label="交易号" width="180" />
      <el-table-column prop="remitterName" label="付款人" width="120" />
      <el-table-column prop="amount" label="金额" width="110">
        <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="summary" label="银行摘要" min-width="200" />
      <el-table-column label="建议匹配" width="200">
        <template #default="{ row }">
          <el-select v-model="row.suggestedMatch" filterable placeholder="选择应收" style="width: 100%">
            <el-option label="HT-2026-001 6月 ¥5,460" value="r1" />
            <el-option label="HT-2026-002 6月 ¥4,030" value="r2" />
          </el-select>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="80">
        <template #default>
          <el-button text size="small" type="primary">匹配</el-button>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const unmatchedItems = ref([
  { transactionRef: 'BANK20260627002', remitterName: '李四', amount: 4000, summary: '房租', suggestedMatch: '' },
  { transactionRef: 'BANK20260625004', remitterName: '赵六', amount: 150, summary: '管理费', suggestedMatch: '' }
])

function autoMatch() {
  ElMessage.success('自动匹配完成，共匹配 2 条')
}
</script>
