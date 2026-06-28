<template>
  <div>
    <div class="page-header">
      <h2>收费项目管理</h2>
      <el-button type="primary" @click="showDialog = true"><el-icon><Plus /></el-icon>新增费用</el-button>
    </div>

    <el-table :data="list" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="code" label="编码" width="100" />
      <el-table-column prop="name" label="名称" width="120" />
      <el-table-column prop="chargeMethod" label="计费方式" width="100">
        <template #default="{ row }">{{ row.chargeMethod === 'FixedAmount' ? '固定金额' : '按表计量' }}</template>
      </el-table-column>
      <el-table-column prop="defaultUnit" label="计量单位" width="100" />
      <el-table-column prop="sortOrder" label="排序" width="60" />
      <el-table-column prop="isActive" label="启用" width="60">
        <template #default="{ row }"><el-switch v-model="row.isActive" /></template>
      </el-table-column>
      <el-table-column prop="description" label="说明" min-width="200" />
      <el-table-column label="操作" width="120" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="editItem(row)">编辑</el-button>
          <el-button text size="small" type="primary" @click="configTemplate(row)">科目模板</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="收费项目" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="编码"><el-input v-model="form.code" /></el-form-item>
        <el-form-item label="名称"><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="计费方式">
          <el-select v-model="form.chargeMethod" style="width: 100%">
            <el-option label="固定金额" value="FixedAmount" />
            <el-option label="按表计量" value="MeterBased" />
          </el-select>
        </el-form-item>
        <el-form-item label="计量单位"><el-input v-model="form.defaultUnit" placeholder="如：元/吨" /></el-form-item>
        <el-form-item label="排序"><el-input-number v-model="form.sortOrder" /></el-form-item>
        <el-form-item label="说明"><el-input v-model="form.description" type="textarea" /></el-form-item>
      </el-form>
      <template #footer><el-button @click="showDialog = false">取消</el-button><el-button type="primary" @click="save">保存</el-button></template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const showDialog = ref(false)
const form = ref({ code: '', name: '', chargeMethod: 'FixedAmount', defaultUnit: '', sortOrder: 0, description: '' })

const list = ref([
  { code: 'RENT', name: '房租费', chargeMethod: 'FixedAmount', defaultUnit: '-', sortOrder: 1, isActive: true, description: '房屋租金' },
  { code: 'WATER', name: '水费', chargeMethod: 'MeterBased', defaultUnit: '元/吨', sortOrder: 2, isActive: true, description: '自来水费' },
  { code: 'ELECTRIC', name: '电费', chargeMethod: 'MeterBased', defaultUnit: '元/度', sortOrder: 3, isActive: true, description: '电费' },
  { code: 'SANITATION', name: '卫生费', chargeMethod: 'FixedAmount', defaultUnit: '-', sortOrder: 4, isActive: true, description: '垃圾清运费' },
  { code: 'MANAGEMENT', name: '管理费', chargeMethod: 'FixedAmount', defaultUnit: '-', sortOrder: 5, isActive: true, description: '物业管理费' },
  { code: 'GAS', name: '燃气费', chargeMethod: 'MeterBased', defaultUnit: '元/方', sortOrder: 7, isActive: true, description: '天然气费' },
  { code: 'INTERNET', name: '网费', chargeMethod: 'FixedAmount', defaultUnit: '-', sortOrder: 8, isActive: true, description: '宽带网络费' },
  { code: 'LATE_FEE', name: '滞纳金', chargeMethod: 'FixedAmount', defaultUnit: '-', sortOrder: 99, isActive: true, description: '逾期滞纳金' }
])

function editItem(row) { Object.assign(form.value, row); showDialog.value = true }
function configTemplate(row) { ElMessage.info('科目模板配置功能待实现') }
function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
