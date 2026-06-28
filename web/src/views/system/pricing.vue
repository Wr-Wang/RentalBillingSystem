<template>
  <div>
    <div class="page-header">
      <h2>定价标准管理</h2>
      <el-button type="primary" @click="showDialog = true"><el-icon><Plus /></el-icon>新增定价</el-button>
    </div>

    <el-card>
      <template #header>房型 × 楼层级别 定价矩阵</template>
      <el-table :data="list" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="roomTypeName" label="房型" width="120" />
        <el-table-column prop="floorLevelName" label="楼层级别" width="100" />
        <el-table-column prop="buildingName" label="楼栋" width="100">
          <template #default="{ row }">{{ row.buildingName || '全局默认' }}</template>
        </el-table-column>
        <el-table-column prop="standardUnitPrice" label="标准租金" width="120">
          <template #default="{ row }">¥{{ row.standardUnitPrice?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="effectiveDate" label="生效日期" width="100" />
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="editItem(row)">编辑</el-button>
            <el-button text size="small" type="danger" @click="deleteItem(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card style="margin-top: 16px;">
      <template #header>楼层级别定义</template>
      <el-table :data="floorLevels" stripe>
        <el-table-column prop="code" label="编码" width="120" />
        <el-table-column prop="name" label="名称" width="120" />
        <el-table-column prop="description" label="说明" />
      </el-table>
    </el-card>

    <el-dialog v-model="showDialog" title="定价标准" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="房型"><el-select v-model="form.roomTypeId" style="width: 100%"><el-option v-for="r in roomTypes" :key="r.id" :label="r.name" :value="r.id" /></el-select></el-form-item>
        <el-form-item label="楼层级别"><el-select v-model="form.floorLevelBandId" style="width: 100%"><el-option v-for="f in floorLevels" :key="f.code" :label="f.name" :value="f.code" /></el-select></el-form-item>
        <el-form-item label="楼栋"><el-select v-model="form.buildingId" placeholder="不选则为全局默认" clearable style="width: 100%"><el-option label="A栋" value="b1" /><el-option label="B栋" value="b2" /></el-select></el-form-item>
        <el-form-item label="标准租金"><el-input-number v-model="form.standardUnitPrice" :min="0" :precision="2" style="width: 100%" /></el-form-item>
        <el-form-item label="生效日期"><el-date-picker v-model="form.effectiveDate" type="date" style="width: 100%" /></el-form-item>
      </el-form>
      <template #footer><el-button @click="showDialog = false">取消</el-button><el-button type="primary" @click="save">保存</el-button></template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const showDialog = ref(false)
const form = ref({ roomTypeId: '', floorLevelBandId: '', buildingId: null, standardUnitPrice: 0, effectiveDate: '' })

const roomTypes = ref([{ id: 'rt1', name: '两室一厅' }, { id: 'rt2', name: '一室一厅' }, { id: 'rt3', name: '三室一厅' }])
const floorLevels = ref([
  { code: 'LOW', name: '低层', description: '1-5层' },
  { code: 'MID', name: '中层', description: '6-12层' },
  { code: 'HIGH', name: '高层', description: '13-17层' },
  { code: 'TOP_FLOOR', name: '顶层', description: '顶层' }
])

const list = ref([
  { roomTypeName: '两室一厅', floorLevelName: '低层', buildingName: null, standardUnitPrice: 4800, effectiveDate: '2026-01-01' },
  { roomTypeName: '两室一厅', floorLevelName: '中层', buildingName: null, standardUnitPrice: 5200, effectiveDate: '2026-01-01' },
  { roomTypeName: '两室一厅', floorLevelName: '高层', buildingName: null, standardUnitPrice: 5600, effectiveDate: '2026-01-01' },
  { roomTypeName: '一室一厅', floorLevelName: '中层', buildingName: 'A栋', standardUnitPrice: 3800, effectiveDate: '2026-01-01' }
])

function editItem(row) { ElMessage.info('编辑功能待实现') }
function deleteItem(row) { ElMessageBox.confirm('确定删除？', '提示').then(() => ElMessage.success('已删除')).catch(() => {}) }
function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
