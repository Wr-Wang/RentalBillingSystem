<template>
  <div>
    <div class="page-header">
      <h2>菜单权限配置</h2>
      <el-button type="primary" @click="openCreate">
        <el-icon><Plus /></el-icon>新增菜单
      </el-button>
    </div>

    <el-card>
      <el-table :data="menuTree" row-key="id" default-expand-all :tree-props="{ children: 'children' }" stripe v-loading="loading">
        <el-table-column prop="name" label="菜单名称" min-width="200" />
        <el-table-column prop="path" label="路由路径" width="200" />
        <el-table-column prop="icon" label="图标" width="80">
          <template #default="{ row }">
            <el-icon v-if="row.icon"><component :is="row.icon" /></el-icon>
          </template>
        </el-table-column>
        <el-table-column prop="permissionCode" label="权限代码" width="160" />
        <el-table-column prop="sortOrder" label="排序" width="60" />
        <el-table-column prop="isActive" label="启用" width="60">
          <template #default="{ row }">
            <el-switch v-model="row.isActive" disabled />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
            <el-button text size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增/编辑菜单 Dialog -->
    <el-dialog v-model="showDialog" :title="isEdit ? '编辑菜单' : '新增菜单'" width="500px">
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="菜单名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="路由路径">
          <el-input v-model="form.path" placeholder="如 /buildings" />
        </el-form-item>
        <el-form-item label="图标">
          <el-input v-model="form.icon" placeholder="Element Plus 图标名，如 HomeFilled" />
        </el-form-item>
        <el-form-item label="权限代码">
          <el-input v-model="form.permissionCode" placeholder="如 building:view" />
        </el-form-item>
        <el-form-item label="上级菜单">
          <el-tree-select v-model="form.parentId" :data="menuTree" :props="{ label: 'name', value: 'id' }" placeholder="不选则为顶级菜单" clearable style="width: 100%" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="form.sortOrder" :min="0" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="saveMenu">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getMenus, createMenu, updateMenu, deleteMenu } from '../../../api/index'

const loading = ref(false)
const showDialog = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref(null)
const menuTree = ref([])

const form = reactive({
  id: null,
  name: '',
  path: '',
  icon: '',
  permissionCode: '',
  parentId: null,
  sortOrder: 0
})

const rules = {
  name: [{ required: true, message: '请输入菜单名称', trigger: 'blur' }]
}

async function fetchMenus() {
  loading.value = true
  try {
    const res = await getMenus()
    menuTree.value = Array.isArray(res) ? res : []
  } catch (e) {
    menuTree.value = []
  }
  loading.value = false
}

function openCreate() {
  isEdit.value = false
  Object.assign(form, { id: null, name: '', path: '', icon: '', permissionCode: '', parentId: null, sortOrder: 0 })
  showDialog.value = true
}

function openEdit(row) {
  isEdit.value = true
  Object.assign(form, {
    id: row.id,
    name: row.name,
    path: row.path || '',
    icon: row.icon || '',
    permissionCode: row.permissionCode || '',
    parentId: row.parentId || null,
    sortOrder: row.sortOrder || 0
  })
  showDialog.value = true
}

async function saveMenu() {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    const data = {
      name: form.name,
      path: form.path || undefined,
      icon: form.icon || undefined,
      permissionCode: form.permissionCode || undefined,
      parentId: form.parentId || undefined,
      sortOrder: form.sortOrder
    }
    if (isEdit.value) {
      await updateMenu(form.id, data)
      ElMessage.success('菜单已更新')
    } else {
      await createMenu(data)
      ElMessage.success('菜单已创建')
    }
    showDialog.value = false
    await fetchMenus()
  } catch (e) {
    ElMessage.error(isEdit.value ? '更新菜单失败' : '创建菜单失败')
  }
  saving.value = false
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm(`确定删除「${row.name}」吗？`, '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await deleteMenu(row.id)
    ElMessage.success('菜单已删除')
    await fetchMenus()
  } catch (e) {
    if (e !== 'cancel') {
      ElMessage.error('删除菜单失败')
    }
  }
}

onMounted(() => fetchMenus())
</script>
