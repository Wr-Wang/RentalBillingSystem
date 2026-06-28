<template>
  <div>
    <div class="page-header">
      <h2>房间详情</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-card style="margin-bottom: 16px;">
      <template #header>基本信息</template>
      <el-descriptions :column="3" border>
        <el-descriptions-item label="房间编号">{{ room.fullCode }}</el-descriptions-item>
        <el-descriptions-item label="座楼">{{ room.buildingName }}</el-descriptions-item>
        <el-descriptions-item label="楼层">{{ room.floorNo }}层</el-descriptions-item>
        <el-descriptions-item label="房型">{{ room.roomTypeName }}</el-descriptions-item>
        <el-descriptions-item label="面积">{{ room.area }} m²</el-descriptions-item>
        <el-descriptions-item label="朝向">{{ room.orientation || '-' }}</el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="statusMap[room.status]?.type">{{ statusMap[room.status]?.label }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="标准租金">¥{{ room.standardRent?.toLocaleString() || '-' }}</el-descriptions-item>
      </el-descriptions>
    </el-card>

    <el-card>
      <template #header>当前合同</template>
      <el-table :data="currentContracts" stripe>
        <el-table-column prop="contractNo" label="合同号" />
        <el-table-column prop="tenantName" label="租客" />
        <el-table-column prop="startDate" label="起租日期" />
        <el-table-column prop="endDate" label="到期日期" />
        <el-table-column prop="rentAmount" label="月租金">
          <template #default="{ row }">¥{{ row.rentAmount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="status" label="状态">
          <template #default="{ row }">
            <el-tag :type="contractStatusMap[row.status]" size="small">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="$router.push('/contracts/' + row.id)">查看</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const statusMap = {
  'Vacant': { label: '空置', type: 'info' },
  'Rented': { label: '已租', type: 'success' },
  'Maintenance': { label: '维修', type: 'warning' }
}

const contractStatusMap = {
  '活跃': 'success', '草稿': 'info', '已到期': 'warning', '已终止': 'danger'
}

const room = ref({
  id: route.params.id,
  fullCode: 'A-1-101',
  buildingName: 'A栋',
  floorNo: 1,
  roomNo: '101',
  roomTypeName: '两室一厅',
  area: 85,
  orientation: '南北通透',
  status: 'Rented',
  standardRent: 5200
})

const currentContracts = ref([
  { id: 'c1', contractNo: 'HT-2026-012', tenantName: '张三', startDate: '2026-01-15', endDate: '2027-01-14', rentAmount: 5200, status: '活跃' }
])
</script>
