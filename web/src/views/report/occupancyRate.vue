<template>
  <div>
    <div class="page-header">
      <h2>出租率统计</h2>
    </div>

    <div class="stat-cards">
      <div class="stat-card"><div class="label">总房间数</div><div class="value">120</div></div>
      <div class="stat-card"><div class="label">已出租</div><div class="value" style="color: #67c23a;">105</div></div>
      <div class="stat-card"><div class="label">空置</div><div class="value" style="color: #e6a23c;">12</div></div>
      <div class="stat-card"><div class="label">维修中</div><div class="value" style="color: #f56c6c;">3</div></div>
    </div>

    <el-row :gutter="16">
      <el-col :span="12">
        <el-card>
          <template #header>出租率</template>
          <div style="text-align: center; padding: 20px;">
            <el-progress type="dashboard" :percentage="87.5" :stroke-width="10" color="#409eff" />
            <p style="margin-top: 16px; color: #909399;">整体出租率 87.5%</p>
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>各楼栋出租率</template>
          <el-table :data="details" stripe>
            <el-table-column prop="buildingName" label="楼栋" />
            <el-table-column prop="total" label="总数" />
            <el-table-column prop="rented" label="已租" />
            <el-table-column prop="rate" label="出租率">
              <template #default="{ row }">
                <el-progress :percentage="row.rate" :color="row.rate > 90 ? '#67c23a' : row.rate > 70 ? '#e6a23c' : '#f56c6c'" />
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
const details = [
  { buildingName: 'A栋', total: 50, rented: 45, rate: 90.0 },
  { buildingName: 'B栋', total: 40, rented: 35, rate: 87.5 },
  { buildingName: 'C栋', total: 30, rented: 25, rate: 83.3 }
]
</script>
