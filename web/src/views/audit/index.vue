<template>
  <div>
    <div class="page-header">
      <h2>变更审计</h2>
    </div>

    <div class="search-bar">
      <el-select v-model="search.tableName" placeholder="选择实体类型" clearable style="width: 160px;">
        <el-option label="合同" value="Contracts" />
        <el-option label="房间" value="Rooms" />
        <el-option label="应收" value="ReceivablePlans" />
        <el-option label="收款" value="Receipts" />
      </el-select>
      <el-input v-model="search.recordId" placeholder="记录ID" clearable style="width: 200px;" />
      <el-select v-model="search.operator" placeholder="操作人" clearable style="width: 140px;">
        <el-option label="张三" value="张三" />
        <el-option label="李四" value="李四" />
      </el-select>
      <el-date-picker v-model="search.dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width: 220px;" />
      <el-button type="primary">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-card style="margin-bottom: 16px;">
      <template #header>审计统计</template>
      <el-row :gutter="16">
        <el-col :span="6">今日变更: 23 次</el-col>
        <el-col :span="6">本周变更: 156 次</el-col>
        <el-col :span="6">本月变更: 682 次</el-col>
        <el-col :span="6">涉及表: 15 张</el-col>
      </el-row>
    </el-card>

    <el-card>
      <template #header>变更时间线</template>
      <el-timeline>
        <el-timeline-item
          v-for="(log, index) in auditLogs"
          :key="index"
          :timestamp="log.changedAt"
          :type="log.action === 'Update' ? 'primary' : log.action === 'Create' ? 'success' : 'danger'"
        >
          <h4>{{ log.operator }} - {{ log.action === 'Create' ? '创建' : log.action === 'Update' ? '更新' : '删除' }}</h4>
          <p>实体: {{ log.tableName }} | ID: {{ log.recordId }}</p>
          <p v-if="log.changes" style="color: #909399; font-size: 12px;">
            <span v-for="(change, key) in log.changes" :key="key">
              {{ key }}: {{ change.old }} → {{ change.new }} &nbsp;
            </span>
          </p>
          <el-button text size="small" type="primary" @click="compareVersion(log)">版本对比</el-button>
          <el-button text size="small" type="warning" @click="rollback(log)">回滚</el-button>
        </el-timeline-item>
      </el-timeline>
    </el-card>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" layout="total, sizes, prev, pager, next" />
    </div>

    <!-- Version Compare Dialog -->
    <el-dialog v-model="showCompare" title="版本对比" width="700px">
      <el-table :data="compareData" stripe>
        <el-table-column prop="field" label="字段" width="150" />
        <el-table-column prop="oldValue" label="旧值" min-width="200" />
        <el-table-column prop="newValue" label="新值" min-width="200" />
        <el-table-column prop="changed" label="变化" width="80">
          <template #default="{ row }">
            <el-tag v-if="row.changed" type="warning" size="small">已变更</el-tag>
            <el-tag v-else type="info" size="small">不变</el-tag>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button type="warning" @click="doRollback">回滚到此版本</el-button>
        <el-button @click="showCompare = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const search = reactive({ tableName: '', recordId: '', operator: '', dateRange: null })
const pagination = reactive({ page: 1, pageSize: 10, total: 200 })
const showCompare = ref(false)

const auditLogs = ref([
  { operator: '张三', action: 'Update', tableName: 'Contracts', recordId: 'c1', changedAt: '2026-06-27 14:30:00', changes: { Status: { old: 'Active', new: 'Terminated' } } },
  { operator: '李四', action: 'Create', tableName: 'Rooms', recordId: 'r10', changedAt: '2026-06-27 10:00:00', changes: null },
  { operator: '王五', action: 'Update', tableName: 'Receipts', recordId: 'rc5', changedAt: '2026-06-26 16:00:00', changes: { Status: { old: 'PendingConfirm', new: 'Confirmed' } } }
])

const compareData = ref([])

function resetSearch() { search.tableName = ''; search.recordId = ''; search.operator = ''; search.dateRange = null }

function compareVersion(log) {
  compareData.value = [
    { field: 'Status', oldValue: 'Active', newValue: 'Terminated', changed: true },
    { field: 'ActualEndDate', oldValue: 'null', newValue: '2026-06-27', changed: true },
    { field: 'RentAmount', oldValue: '5200.00', newValue: '5200.00', changed: false }
  ]
  showCompare.value = true
}

function doRollback() {
  ElMessageBox.confirm('确定回滚到该版本？此操作需要管理员权限。', '警告').then(() => {
    ElMessage.success('已回滚到指定版本')
    showCompare.value = false
  }).catch(() => {})
}

function rollback(log) {
  ElMessageBox.confirm(`确定回滚记录 ${log.recordId}？`, '提示').then(() => {
    ElMessage.success('已回滚')
  }).catch(() => {})
}
</script>
