<template>
  <div>
    <div class="page-header">
      <h2>房型管理</h2>
      <el-button type="primary" @click="openCreate"><el-icon><Plus /></el-icon>新增房型</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="list" stripe v-loading="loading" style="width:100%">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="name" label="名称" min-width="160" />
        <el-table-column prop="description" label="说明" min-width="250" />
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
            <el-button text size="small" :type="row.isActive ? 'warning' : 'success'" @click="toggleStatus(row)">
              {{ row.isActive ? '停用' : '启用' }}
            </el-button>
            <el-button text size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="showDialog" :title="isEdit ? '编辑房型' : '新增房型'" width="500px">
      <el-form :model="form" label-width="80px" :rules="rules" ref="formRef">
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" placeholder="如：一室一厅" />
        </el-form-item>
        <el-form-item label="说明">
          <el-input v-model="form.description" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getRoomTypes, createRoomType, updateRoomType, deleteRoomType } from '../../api/index'

const loading = ref(false)
const list = ref([])
const showDialog = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref(null)
const form = ref({ id: null, name: '', description: '' })
const rules = {
  name: [{ required: true, message: '请输入房型名称', trigger: 'blur' }]
}

async function fetchList() {
  loading.value = true
  try {
    const res = await getRoomTypes()
    list.value = Array.isArray(res) ? res : []
  } catch { list.value = [] }
  loading.value = false
}

function openCreate() {
  isEdit.value = false
  form.value = { id: null, name: '', description: '' }
  showDialog.value = true
}

function openEdit(row) {
  isEdit.value = true
  form.value = { id: row.id, name: row.name, description: row.description || '' }
  showDialog.value = true
}

async function save() {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await updateRoomType(form.value.id, { name: form.value.name, description: form.value.description || undefined })
      ElMessage.success('已更新')
    } else {
      await createRoomType({ name: form.value.name, description: form.value.description || undefined })
      ElMessage.success('已创建')
    }
    showDialog.value = false
    await fetchList()
  } catch { ElMessage.error(isEdit.value ? '更新失败' : '创建失败') }
  saving.value = false
}

async function toggleStatus(row) {
  const action = row.isActive ? '停用' : '启用'
  try {
    await ElMessageBox.confirm(`确定${action}「${row.name}」？`, '提示', { type: 'warning' })
    await updateRoomType(row.id, { isActive: !row.isActive })
    ElMessage.success(`已${action}`)
    await fetchList()
  } catch (e) { if (e !== 'cancel') ElMessage.error(`${action}失败`) }
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm(`确定删除「${row.name}」？`, '提示', { type: 'warning' })
    await deleteRoomType(row.id)
    ElMessage.success('已删除')
    await fetchList()
  } catch (e) { /* cancel */ }
}

onMounted(() => fetchList())
</script>
