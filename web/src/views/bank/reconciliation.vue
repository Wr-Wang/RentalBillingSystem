<template>
  <div>
    <div class="page-header">
      <h2>余额调节表</h2>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="searchMonth" type="month" placeholder="选择月份" />
      <el-button type="primary">生成</el-button>
    </div>

    <el-row :gutter="16">
      <el-col :span="12">
        <el-card>
          <template #header>银行存款余额调节表</template>
          <el-descriptions :column="1" border>
            <el-descriptions-item label="银行对账单余额">
              <span style="font-weight: bold; font-size: 16px;">¥1,286,500.00</span>
            </el-descriptions-item>
            <el-descriptions-item label="加：企收银未收">
              <span>¥22,000.00</span>
            </el-descriptions-item>
            <el-descriptions-item label="减：企付银未付">
              <span>¥5,000.00</span>
            </el-descriptions-item>
            <el-descriptions-item label="调节后银行余额">
              <span style="font-weight: bold; color: #67c23a; font-size: 16px;">¥1,303,500.00</span>
            </el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>企业账面余额调节表</template>
          <el-descriptions :column="1" border>
            <el-descriptions-item label="企业账面余额">
              <span style="font-weight: bold; font-size: 16px;">¥1,298,500.00</span>
            </el-descriptions-item>
            <el-descriptions-item label="加：银收企未收">
              <span>¥8,000.00</span>
            </el-descriptions-item>
            <el-descriptions-item label="减：银付企未付">
              <span>¥3,000.00</span>
            </el-descriptions-item>
            <el-descriptions-item label="调节后企业余额">
              <span style="font-weight: bold; color: #67c23a; font-size: 16px;">¥1,303,500.00</span>
            </el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>

    <el-card style="margin-top: 16px;">
      <template #header>未达账项明细</template>
      <el-table :data="unreconciledItems" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="type" label="类型" width="120">
          <template #default="{ row }">
            <el-tag :type="row.type === '企收银未收' ? 'warning' : 'info'" size="small">{{ row.type }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="说明" min-width="250" />
        <el-table-column prop="amount" label="金额" width="110">
          <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="date" label="日期" width="100" />
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const searchMonth = ref(new Date())

const unreconciledItems = ref([
  { type: '企收银未收', description: 'A栋302 6月房租（已收款登记，银行未到账）', amount: 15000, date: '2026-06-27' },
  { type: '企收银未收', description: 'B栋201 6月房租（已收款登记，银行未到账）', amount: 7000, date: '2026-06-27' },
  { type: '银收企未收', description: '银行转入利息', amount: 8000, date: '2026-06-25' },
  { type: '银付企未付', description: '银行扣手续费', amount: 3000, date: '2026-06-26' }
])
</script>
