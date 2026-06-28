<template>
  <div>
    <div class="page-header">
      <h2>银行流水导入</h2>
    </div>

    <el-card>
      <template #header>上传银行流水</template>
      <el-upload
        drag
        action="#"
        :auto-upload="false"
        style="margin-bottom: 16px;"
      >
        <el-icon class="el-icon--upload" :size="48"><UploadFilled /></el-icon>
        <div class="el-upload__text">
          将银行流水 Excel 文件拖到此处，或 <em>点击选择</em>
        </div>
        <template #tip>
          <div class="el-upload__tip">支持 .xlsx, .xls 格式，请使用标准银行流水模板</div>
        </template>
      </el-upload>

      <el-button type="primary"><el-icon><Download /></el-icon>下载模板</el-button>
      <el-divider />

      <h3>导入预览</h3>
      <el-table :data="previewData" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="transactionDate" label="交易日期" width="120" />
        <el-table-column prop="transactionRef" label="交易号" width="180" />
        <el-table-column prop="remitterName" label="付款人" width="120" />
        <el-table-column prop="amount" label="金额" width="110">
          <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="summary" label="摘要" min-width="200" />
        <el-table-column prop="matchStatus" label="匹配状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.matchStatus === '已匹配' ? 'success' : 'info'" size="small">{{ row.matchStatus }}</el-tag>
          </template>
        </el-table-column>
      </el-table>
      <div style="text-align: center; margin-top: 16px;">
        <el-button type="primary" size="large" @click="importData">
          <el-icon><Upload /></el-icon>确认导入
        </el-button>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const previewData = ref([
  { transactionDate: '2026-06-27', transactionRef: 'BANK20260627001', remitterName: '张三', amount: 5460, summary: '房租转账-A栋101', matchStatus: '已匹配' },
  { transactionDate: '2026-06-27', transactionRef: 'BANK20260627002', remitterName: '李四', amount: 4000, summary: '房租', matchStatus: '待匹配' },
  { transactionDate: '2026-06-26', transactionRef: 'BANK20260626003', remitterName: '王五', amount: 7030, summary: '租金-202606', matchStatus: '已匹配' }
])

function importData() {
  ElMessage.success('导入成功，共3条记录')
}
</script>
