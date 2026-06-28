<template>
  <div>
    <div class="page-header">
      <h2>审批历史</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="申请编号/类型" clearable style="width: 200px;" />
      <el-select v-model="search.status" placeholder="审批结果" clearable style="width: 140px;">
        <el-option label="已通过" value="Approved" />
        <el-option label="已驳回" value="Rejected" />
      </el-select>
      <el-date-picker v-model="search.dateRange" type="daterange" range-separator="至" start-placeholder="开始" end-placeholder="结束" style="width: 220px;" />
      <el-button type="primary">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-table :data="historyList" stripe>
      <el-table-column prop="requestNo" label="申请编号" width="150" />
      <el-table-column prop="approvalTypeName" label="审批类型" width="120" />
      <el-table-column prop="submitterName" label="申请人" width="100" />
      <el-table-column prop="businessSummary" label="业务摘要" min-width="200" />
      <el-table-column prop="status" label="最终结果" width="100">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Approved' ? 'success' : 'danger'" size="small">
            {{ row.status === 'Approved' ? '已通过' : '已驳回' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="submittedAt" label="提交时间" width="150" />
      <el-table-column prop="completedAt" label="完成时间" width="150" />
      <el-table-column label="操作" width="80">
        <template #default="{ row }">
          <el-button text size="small" type="primary">查看</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" layout="total, sizes, prev, pager, next" />
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'

const search = reactive({ keyword: '', status: '', dateRange: null })
const pagination = reactive({ page: 1, pageSize: 10, total: 120 })

const historyList = ref([
  { requestNo: 'SP-20260620-001', approvalTypeName: '提前解约', submitterName: '张三', businessSummary: 'C栋-601 提前解约 ¥10,000', status: 'Approved', submittedAt: '2026-06-20 09:00', completedAt: '2026-06-21 14:30' },
  { requestNo: 'SP-20260619-002', approvalTypeName: '批量导入房屋', submitterName: '李四', businessSummary: '导入C栋12套新房源', status: 'Approved', submittedAt: '2026-06-19 10:00', completedAt: '2026-06-20 11:00' },
  { requestNo: 'SP-20260618-003', approvalTypeName: '收款冲销', submitterName: '王五', businessSummary: '冲销SJ-20260617-001 ¥5,200', status: 'Rejected', submittedAt: '2026-06-18 14:00', completedAt: '2026-06-18 16:30' }
])

function resetSearch() { search.keyword = ''; search.status = ''; search.dateRange = null }
</script>
