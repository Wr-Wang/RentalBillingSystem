<template>
  <div>
    <div class="page-header">
      <h2>收租率统计</h2>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="searchMonth" type="month" placeholder="选择月份" />
      <el-select v-model="search.buildingId" placeholder="选择座楼" clearable style="width: 140px;">
        <el-option label="全部" value="" />
        <el-option label="A栋" value="A" />
        <el-option label="B栋" value="B" />
        <el-option label="C栋" value="C" />
      </el-select>
      <el-button type="primary">查询</el-button>
    </div>

    <div class="stat-cards">
      <div class="stat-card" style="border-left: 4px solid #409eff;">
        <div class="label">应收总额</div>
        <div class="value" style="color: #409eff;">¥1,280,000</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #67c23a;">
        <div class="label">实收总额</div>
        <div class="value" style="color: #67c23a;">¥1,058,000</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #e6a23c;">
        <div class="label">欠费总额</div>
        <div class="value" style="color: #e6a23c;">¥222,000</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #409eff;">
        <div class="label">收租率</div>
        <div class="value" style="color: #409eff;">82.7%</div>
      </div>
    </div>

    <el-card>
      <template #header>收租率详情</template>
      <el-table :data="details" stripe>
        <el-table-column prop="buildingName" label="座楼" />
        <el-table-column prop="totalRooms" label="房间数" />
        <el-table-column prop="rentedRooms" label="已租数" />
        <el-table-column prop="receivable" label="应收金额">
          <template #default="{ row }">¥{{ row.receivable?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="received" label="实收金额">
          <template #default="{ row }">¥{{ row.received?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="overdue" label="欠费金额">
          <template #default="{ row }">¥{{ row.overdue?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="rate" label="收租率">
          <template #default="{ row }">
            <el-progress :percentage="row.rate" :color="row.rate > 90 ? '#67c23a' : row.rate > 70 ? '#e6a23c' : '#f56c6c'" />
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const searchMonth = ref(new Date())
const search = ref({ buildingId: '' })

const details = ref([
  { buildingName: 'A栋', totalRooms: 50, rentedRooms: 45, receivable: 520000, received: 480000, overdue: 40000, rate: 92.3 },
  { buildingName: 'B栋', totalRooms: 40, rentedRooms: 35, receivable: 380000, received: 320000, overdue: 60000, rate: 84.2 },
  { buildingName: 'C栋', totalRooms: 30, rentedRooms: 25, receivable: 380000, received: 258000, overdue: 122000, rate: 67.9 }
])
</script>
