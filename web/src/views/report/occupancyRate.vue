<template>
  <div>
    <div class="page-header">
      <h2>出租率统计</h2>
    </div>

    <div class="stat-cards">
      <div class="stat-card"><div class="label">总房间数</div><div class="value">{{ stats.totalRooms }}</div></div>
      <div class="stat-card"><div class="label">已出租</div><div class="value" style="color: #67c23a;">{{ stats.rented }}</div></div>
      <div class="stat-card"><div class="label">空置</div><div class="value" style="color: #e6a23c;">{{ stats.vacant }}</div></div>
      <div class="stat-card"><div class="label">维修中</div><div class="value" style="color: #f56c6c;">{{ stats.maintenance }}</div></div>
    </div>

    <el-row :gutter="16">
      <el-col :span="12">
        <el-card>
          <template #header>出租率</template>
          <div style="text-align: center; padding: 20px;">
            <el-progress type="dashboard" :percentage="stats.overallRate" :stroke-width="10" color="#409eff" />
            <p style="margin-top: 16px; color: #909399;">整体出租率 {{ stats.overallRate }}%</p>
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>各楼栋出租率</template>
          <el-table :data="details" stripe style="width:100%">
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
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getOccupancyRate } from '../../api/index'

const stats = reactive({
  totalRooms: 0,
  rented: 0,
  vacant: 0,
  maintenance: 0,
  overallRate: 0
})

const details = ref([])
const loading = ref(false)

async function loadData() {
  loading.value = true
  try {
    const res = await getOccupancyRate({ period: undefined })
    const items = res.items || res.data || res
    if (Array.isArray(items) && items.length > 0) {
      let totalRooms = 0, totalRented = 0
      const buildingDetails = items.map(item => {
        const total = item.TotalRooms || item.totalRooms || item.total || 0
        const rented = item.RentedRooms || item.rentedRooms || item.rented || 0
        const rate = total > 0 ? Math.round(rented / total * 100 * 10) / 10 : 0
        totalRooms += total
        totalRented += rented
        return {
          buildingName: item.BuildingName || item.buildingName || item.name || '未知',
          total,
          rented,
          rate
        }
      })
      details.value = buildingDetails
      Object.assign(stats, {
        totalRooms,
        rented: totalRented,
        vacant: totalRooms - totalRented,
        maintenance: 0,
        overallRate: totalRooms > 0 ? Math.round(totalRented / totalRooms * 100 * 10) / 10 : 0
      })
    }
  } catch { ElMessage.error('加载出租率统计失败') }
  loading.value = false
}

onMounted(loadData)
</script>

<style scoped>
.stat-cards {
  display: flex;
  gap: 16px;
  margin-bottom: 16px;
}
.stat-card {
  flex: 1;
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  text-align: center;
  box-shadow: 0 1px 4px rgba(0,0,0,0.06);
}
.stat-card .label {
  font-size: 13px;
  color: #909399;
  margin-bottom: 8px;
}
.stat-card .value {
  font-size: 28px;
  font-weight: 700;
  color: #303133;
}
</style>
