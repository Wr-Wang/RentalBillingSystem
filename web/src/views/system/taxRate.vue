<template>
  <div>
    <div class="page-header">
      <h2>税率配置</h2>
      <el-button type="primary" @click="showDialog = true"><el-icon><Plus /></el-icon>新增税率</el-button>
    </div>

    <el-table :data="list" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="feeCodeName" label="费用类型" width="120" />
      <el-table-column prop="taxRate" label="税率">
        <template #default="{ row }">{{ (row.taxRate * 100).toFixed(2) }}%</template>
      </el-table-column>
      <el-table-column prop="effectiveDate" label="生效日期" width="100" />
      <el-table-column prop="isActive" label="启用" width="60">
        <template #default="{ row }"><el-switch v-model="row.isActive" /></template>
      </el-table-column>
      <el-table-column label="操作" width="120">
        <template #default="{ row }"><el-button text size="small" type="primary">编辑</el-button></template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="税率" width="400px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="费用类型"><el-select v-model="form.feeCodeId" style="width: 100%"><el-option label="房租费" value="RENT" /><el-option label="水费" value="WATER" /><el-option label="电费" value="ELECTRIC" /></el-select></el-form-item>
        <el-form-item label="税率 (%)"><el-input-number v-model="form.taxRate" :precision="4" :step="0.01" :min="0" :max="100" style="width: 100%" /></el-form-item>
        <el-form-item label="生效日期"><el-date-picker v-model="form.effectiveDate" type="date" style="width: 100%" /></el-form-item>
      </el-form>
      <template #footer><el-button @click="showDialog = false">取消</el-button><el-button type="primary" @click="save">保存</el-button></template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const showDialog = ref(false)
const form = ref({ feeCodeId: '', taxRate: 6, effectiveDate: '' })

const list = ref([
  { feeCodeName: '房租费', taxRate: 0.06, effectiveDate: '2026-01-01', isActive: true },
  { feeCodeName: '水费', taxRate: 0.06, effectiveDate: '2026-01-01', isActive: true },
  { feeCodeName: '电费', taxRate: 0.06, effectiveDate: '2026-01-01', isActive: true }
])

function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
