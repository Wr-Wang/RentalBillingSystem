<template>
  <div v-loading="loading">
    <div class="page-header">
      <h2>房源详情 - {{ unit?.fullCode || unit?.unitNo || '' }}</h2>
      <div class="table-actions">
        <el-button @click="$router.push('/buildings')">返回列表</el-button>
        <el-button type="primary" @click="goEdit">编辑房源</el-button>
        <el-button type="success" v-if="unit?.status === 'Vacant'">新建合同</el-button>
      </div>
    </div>

    <!-- 基本信息 -->
    <el-card style="margin-bottom: 16px;">
      <template #header><span>基本信息</span></template>
      <el-descriptions :column="3" border>
        <el-descriptions-item label="座楼名称" :span="1">{{ unit?.buildingName || '-' }}</el-descriptions-item>
        <el-descriptions-item label="座楼编码">{{ unit?.buildingCode || '-' }}</el-descriptions-item>
        <el-descriptions-item label="座楼地址">{{ unit?.buildingAddress || '-' }}</el-descriptions-item>
        <el-descriptions-item label="楼层">{{ unit?.floorName || '-' }}</el-descriptions-item>
        <el-descriptions-item label="房号">{{ unit?.unitNo || '-' }}</el-descriptions-item>
        <el-descriptions-item label="完整编码">{{ unit?.fullCode || '-' }}</el-descriptions-item>
        <el-descriptions-item label="房型">{{ unit?.roomTypeName || '-' }}</el-descriptions-item>
        <el-descriptions-item label="面积">{{ unit?.area ? unit.area + ' m²' : '-' }}</el-descriptions-item>
        <el-descriptions-item label="朝向">{{ unit?.orientation || '-' }}</el-descriptions-item>
        <el-descriptions-item label="标准租金">¥{{ (unit?.baseRentAmount || 0).toLocaleString() }}</el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="statusTagType(unit?.status)" size="small">{{ statusLabel(unit?.status) }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="创建时间">{{ unit?.createdAt ? formatDate(unit.createdAt) : '-' }}</el-descriptions-item>
      </el-descriptions>
    </el-card>

    <!-- 当前合同 -->
    <el-card style="margin-bottom: 16px;">
      <template #header>
        <span>当前合同</span>
        <el-button text type="primary" size="small" style="float: right;" v-if="unit?.status === 'Rented'">查看全部</el-button>
      </template>
      <el-empty v-if="!loading && contracts.length === 0" description="暂无合同" />
      <el-table :data="contracts" stripe v-else>
        <el-table-column prop="contractNo" label="合同号" width="150" />
        <el-table-column prop="rentAmount" label="月租金" width="120">
          <template #default="{ row }">¥{{ (row.rentAmount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.statusCode === 'Active' ? 'success' : 'info'" size="small">
              {{ row.statusCode === 'Active' ? '生效中' : row.statusCode || row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="起租日期" width="120">
          <template #default="{ row }">{{ row.startDate || '-' }}</template>
        </el-table-column>
        <el-table-column label="到期日期" width="120">
          <template #default="{ row }">{{ row.endDate || '-' }}</template>
        </el-table-column>
        <el-table-column label="操作" width="100">
          <template #default="{ row }">
            <el-button text size="small" type="primary">查看</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 最近操作 -->
    <el-card>
      <template #header><span>变更记录</span></template>
      <el-timeline>
        <el-timeline-item timestamp="房源创建" placement="top">
          <p>创建房源 {{ unit?.fullCode || unit?.unitNo || '' }}</p>
        </el-timeline-item>
      </el-timeline>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getHousingUnit } from '../../api/index'

const route = useRoute()
const router = useRouter()
const loading = ref(false)
const unit = ref(null)
const contracts = ref([])

function statusTagType(status) {
  const map = { Rented: 'success', Vacant: 'info', Maintenance: 'warning' }
  return map[status] || 'info'
}
function statusLabel(status) {
  const map = { Rented: '已租', Vacant: '空置', Maintenance: '维修' }
  return map[status] || status || '-'
}
function formatDate(dt) {
  if (!dt) return '-'
  const d = new Date(dt)
  return d.toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' }) + ' ' + d.toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
}
function goEdit() {
  router.push({ path: '/buildings', query: { editId: unit.value?.id } })
}

onMounted(async () => {
  loading.value = true
  try {
    const r = await getHousingUnit(route.params.id)
    unit.value = r || null
  } catch { unit.value = null }
  loading.value = false
})
</script>
