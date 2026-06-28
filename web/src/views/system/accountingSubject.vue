<template>
  <div>
    <div class="page-header">
      <h2>会计科目管理</h2>
      <el-button type="primary" @click="showDialog = true"><el-icon><Plus /></el-icon>新增科目</el-button>
    </div>

    <el-table :data="list" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="code" label="科目编码" width="120" />
      <el-table-column prop="name" label="科目名称" width="200" />
      <el-table-column prop="direction" label="方向" width="80">
        <template #default="{ row }">{{ row.direction === 'Debit' ? '借方' : '贷方' }}</template>
      </el-table-column>
      <el-table-column prop="level" label="级别" width="60" />
      <el-table-column prop="isActive" label="启用" width="60">
        <template #default="{ row }"><el-switch v-model="row.isActive" /></template>
      </el-table-column>
      <el-table-column label="操作" width="120">
        <template #default="{ row }"><el-button text size="small" type="primary">编辑</el-button></template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="会计科目" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="科目编码"><el-input v-model="form.code" /></el-form-item>
        <el-form-item label="科目名称"><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="方向"><el-select v-model="form.direction" style="width: 100%"><el-option label="借方" value="Debit" /><el-option label="贷方" value="Credit" /></el-select></el-form-item>
        <el-form-item label="级别"><el-input-number v-model="form.level" :min="1" :max="5" /></el-form-item>
        <el-form-item label="上级科目"><el-tree-select v-model="form.parentId" :data="list" :props="{ label: 'name', value: 'id' }" placeholder="不选则为顶级" clearable style="width: 100%" /></el-form-item>
      </el-form>
      <template #footer><el-button @click="showDialog = false">取消</el-button><el-button type="primary" @click="save">保存</el-button></template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const showDialog = ref(false)
const form = ref({ code: '', name: '', direction: 'Debit', level: 1, parentId: null })

const list = ref([
  { id: 's1', code: '1002', name: '银行存款', direction: 'Debit', level: 1, isActive: true, parentId: null },
  { id: 's2', code: '1122', name: '应收账款', direction: 'Debit', level: 1, isActive: true, parentId: null },
  { id: 's3', code: '6001', name: '主营业务收入', direction: 'Credit', level: 1, isActive: true, parentId: null },
  { id: 's4', code: '2221', name: '应交税费', direction: 'Credit', level: 1, isActive: true, parentId: null }
])

function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
