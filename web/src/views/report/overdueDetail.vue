<template>
  <div>
    <div class="page-header">
      <h2>欠费明细表</h2>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="合同号/租客" clearable style="width: 200px;" />
      <el-select v-model="search.overdueRange" placeholder="逾期范围" clearable style="width: 140px;">
        <el-option label="1-7天" value="1-7" />
        <el-option label="8-15天" value="8-15" />
        <el-option label="16-30天" value="16-30" />
        <el-option label="30天以上" value="30+" />
      </el-select>
      <el-button type="primary">查询</el-button>
      <el-button><el-icon><Download /></el-icon>导出Excel</el-button>
    </div>

    <el-table :data="overdueList" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="contractNo" label="合同号" width="130" />
      <el-table-column prop="roomName" label="房屋" width="100" />
      <el-table-column prop="tenantName" label="租客" width="100" />
      <el-table-column prop="period" label="账期" width="80" />
      <el-table-column prop="dueDate" label="到期日" width="100" />
      <el-table-column prop="amount" label="欠费金额" width="110">
        <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="overdueDays" label="逾期天数" width="90" />
      <el-table-column prop="lateFee" label="滞纳金" width="100">
        <template #default="{ row }">¥{{ (row.lateFee || 0).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column label="操作" width="80">
        <template #default>
          <el-button text size="small" type="primary">催缴</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" layout="total, sizes, prev, pager, next" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const search = ref({ keyword: '', overdueRange: '' })
const pagination = ref({ page: 1, pageSize: 10, total: 50 })

const overdueList = ref([
  { contractNo: 'HT-2026-005', roomName: 'A-201', tenantName: '孙七', period: '2026-06', dueDate: '2026-06-05', amount: 4500, overdueDays: 22, lateFee: 49.50 },
  { contractNo: 'HT-2026-008', roomName: 'B-302', tenantName: '周八', period: '2026-06', dueDate: '2026-06-05', amount: 8200, overdueDays: 22, lateFee: 90.20 },
  { contractNo: 'HT-2026-012', roomName: 'C-101', tenantName: '吴九', period: '2026-06', dueDate: '2026-06-05', amount: 3800, overdueDays: 22, lateFee: 41.80 },
  { contractNo: 'HT-2026-015', roomName: 'C-202', tenantName: '郑十', period: '2026-06', dueDate: '2026-06-05', amount: 6000, overdueDays: 22, lateFee: 66.00 }
])
</script>
