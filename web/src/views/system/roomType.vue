<template>
  <div>
    <div class="page-header">
      <h2>房型管理</h2>
      <el-button type="primary" @click="showDialog = true"><el-icon><Plus /></el-icon>新增房型</el-button>
    </div>

    <el-table :data="list" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="category" label="类别" width="80" />
      <el-table-column prop="code" label="编码" width="160" />
      <el-table-column prop="name" label="名称" width="150" />
      <el-table-column prop="description" label="说明" min-width="200" />
      <el-table-column prop="sortOrder" label="排序" width="60" />
      <el-table-column prop="isActive" label="启用" width="60">
        <template #default="{ row }"><el-switch v-model="row.isActive" /></template>
      </el-table-column>
      <el-table-column label="操作" width="120">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="editItem(row)">编辑</el-button>
          <el-button text size="small" type="danger" @click="deleteItem(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="房型" width="500px">
      <el-form :model="form" label-width="80px">
        <el-form-item label="类别"><el-select v-model="form.category" style="width: 100%"><el-option label="整租" value="整租" /><el-option label="合租" value="合租" /></el-select></el-form-item>
        <el-form-item label="编码"><el-input v-model="form.code" /></el-form-item>
        <el-form-item label="名称"><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="说明"><el-input v-model="form.description" type="textarea" /></el-form-item>
      </el-form>
      <template #footer><el-button @click="showDialog = false">取消</el-button><el-button type="primary" @click="save">保存</el-button></template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const showDialog = ref(false)
const form = ref({ category: '整租', code: '', name: '', description: '' })

const list = ref([
  { category: '整租', code: 'STUDIO', name: '开间/单间', description: '', sortOrder: 1, isActive: true },
  { category: '整租', code: 'ONE_BR_ONE_LR', name: '一室一厅', description: '', sortOrder: 2, isActive: true },
  { category: '整租', code: 'TWO_BR_ONE_LR', name: '两室一厅', description: '', sortOrder: 3, isActive: true },
  { category: '整租', code: 'THREE_BR_ONE_LR', name: '三室一厅', description: '', sortOrder: 4, isActive: true },
  { category: '合租', code: 'MASTER_BR', name: '主卧', description: '', sortOrder: 10, isActive: true },
  { category: '合租', code: 'SECONDARY_BR', name: '次卧', description: '', sortOrder: 11, isActive: true }
])

function editItem(row) { Object.assign(form.value, row); showDialog.value = true }
function deleteItem(row) { ElMessageBox.confirm(`确定删除 ${row.name}？`, '提示').then(() => ElMessage.success('已删除')).catch(() => {}) }
function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
