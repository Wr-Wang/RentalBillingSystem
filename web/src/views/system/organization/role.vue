<template>
  <div>
    <div class="page-header">
      <h2>角色管理</h2>
      <el-button type="primary" @click="openCreate">
        <el-icon><Plus /></el-icon>新增角色
      </el-button>
    </div>

    <el-table :data="roles" stripe v-loading="loading">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="name" label="角色名称" width="150" />
      <el-table-column prop="code" label="角色代码" width="150" />
      <el-table-column prop="description" label="说明" min-width="200" />
      <el-table-column label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '启用' : '停用' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button text size="small" type="primary" @click="assignMenu(row)">分配菜单</el-button>
          <el-button text size="small" type="danger" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 新增/编辑角色 Dialog -->
    <el-dialog v-model="showDialog" :title="isEdit ? '编辑角色' : '新增角色'" width="450px">
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="角色名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="角色代码" prop="code">
          <el-input v-model="form.code" :disabled="isEdit" />
        </el-form-item>
        <el-form-item label="说明">
          <el-input v-model="form.description" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="saveRole">保存</el-button>
      </template>
    </el-dialog>

    <!-- 分配菜单权限 Dialog -->
    <el-dialog v-model="showMenuDialog" title="分配菜单权限" width="600px" class="menu-assign-dialog">
      <div class="menu-assign-toolbar">
        <el-input
          v-model="menuFilter"
          placeholder="搜索菜单名称"
          clearable
          size="small"
          style="width: 220px"
          @input="filterMenuTree"
        />
        <div class="menu-assign-actions">
          <el-button size="small" @click="setMenuExpandAll(true)">展开全部</el-button>
          <el-button size="small" @click="setMenuExpandAll(false)">收起全部</el-button>
          <el-button size="small" @click="clearMenuChecked">清空勾选</el-button>
        </div>
      </div>

      <div v-loading="menuLoading" class="menu-tree-wrap">
        <el-tree
          ref="menuTreeRef"
          :data="allMenus"
          show-checkbox
          node-key="id"
          :props="{ label: 'name', children: 'children' }"
          :default-checked-keys="checkedMenuIds"
          :filter-node-method="matchMenuNode"
          default-expand-all
          highlight-current
        >
          <template #default="{ data }">
            <span class="menu-node">
              <el-icon v-if="data.icon" :size="14" style="margin-right: 4px;">
                <component :is="data.icon" />
              </el-icon>
              <span :class="{ 'menu-label': !data._hasLeafChildren, 'btn-label': !!data._hasLeafChildren }">
                {{ data.name }}
              </span>
              <el-tag v-if="data.permissionCode" size="small" class="perm-tag">{{ data.permissionCode }}</el-tag>
              <span v-if="data._hasLeafChildren" class="hint-marker"></span>
            </span>
          </template>
        </el-tree>
      </div>

      <template #footer>
        <el-button @click="showMenuDialog = false">取消</el-button>
        <el-button type="primary" :loading="savingMenu" @click="saveMenuAssign">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getRoles, createRole, updateRole, deleteRole, getRole, getMenus, updateRoleMenus } from '../../../api/index'
import { useUserStore } from '../../../store/user'

const loading = ref(false)
const showDialog = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref(null)
const roles = ref([])

const form = ref({ id: null, name: '', code: '', description: '' })
const rules = {
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
  code: [{ required: true, message: '请输入角色代码', trigger: 'blur' }]
}

// 菜单权限分配
const showMenuDialog = ref(false)
const savingMenu = ref(false)
const menuLoading = ref(false)
const menuTreeRef = ref(null)
const allMenus = ref([])
const checkedMenuIds = ref([])
const currentRoleId = ref(null)
const menuFilter = ref('')

async function fetchRoles() {
  loading.value = true
  try {
    const res = await getRoles()
    roles.value = Array.isArray(res) ? res : []
  } catch (e) {
    roles.value = []
  }
  loading.value = false
}

async function fetchMenus() {
  try {
    const res = await getMenus()
    let menus = Array.isArray(res) ? res : []
    if (!useUserStore().isSuperAdmin) {
      menus = filterSystemMenus(menus)
    }
    allMenus.value = menus
  } catch (e) {
    allMenus.value = []
  }
}

function filterSystemMenus(menus) {
  return menus.filter(m => {
    if (m.scope === 'System') return false
    if (m.children?.length) m.children = filterSystemMenus(m.children)
    return true
  })
}

function filterMenuTree() {
  if (menuTreeRef.value) {
    menuTreeRef.value.filter(menuFilter.value)
  }
}

function matchMenuNode(keyword, data) {
  if (!keyword) return true
  return data.name.toLowerCase().includes(keyword.toLowerCase())
}

function setMenuExpandAll(expand) {
  const tree = menuTreeRef.value
  if (!tree || !tree.$el) return
  const icons = tree.$el.querySelectorAll('.el-tree-node__expand-icon')
  icons.forEach(icon => {
    const isExpanded = icon.classList.contains('expanded')
    if ((expand && !isExpanded) || (!expand && isExpanded)) {
      icon.click()
    }
  })
}

function clearMenuChecked() {
  if (!menuTreeRef.value) return
  menuTreeRef.value.setCheckedKeys([])
}

function openCreate() {
  isEdit.value = false
  form.value = { id: null, name: '', code: '', description: '' }
  showDialog.value = true
}

function openEdit(row) {
  isEdit.value = true
  form.value = { id: row.id, name: row.name, code: row.code, description: row.description || '' }
  showDialog.value = true
}

async function saveRole() {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    const data = {
      name: form.value.name,
      code: form.value.code,
      description: form.value.description || undefined
    }
    if (isEdit.value) {
      await updateRole(form.value.id, data)
      ElMessage.success('角色已更新')
    } else {
      await createRole(data)
      ElMessage.success('角色已创建')
    }
    showDialog.value = false
    await fetchRoles()
  } catch (e) {
    ElMessage.error(isEdit.value ? '更新角色失败' : '创建角色失败')
  }
  saving.value = false
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm(`确定删除角色「${row.name}」吗？`, '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await deleteRole(row.id)
    ElMessage.success('角色已删除')
    await fetchRoles()
  } catch (e) {
    if (e !== 'cancel') {
      ElMessage.error('删除角色失败')
    }
  }
}

async function assignMenu(row) {
  currentRoleId.value = row.id
  checkedMenuIds.value = []
  menuFilter.value = ''
  menuLoading.value = true

  try {
    const res = await getRole(row.id)
    if (res && res.menuIds) {
      checkedMenuIds.value = res.menuIds
    }
  } catch (e) {
    checkedMenuIds.value = []
  }

  menuLoading.value = false
  showMenuDialog.value = true
}

async function saveMenuAssign() {
  if (!currentRoleId.value || !menuTreeRef.value) return
  savingMenu.value = true
  try {
    const checkedKeys = menuTreeRef.value.getCheckedKeys()
    const halfCheckedKeys = menuTreeRef.value.getHalfCheckedKeys()
    await updateRoleMenus(currentRoleId.value, [...checkedKeys, ...halfCheckedKeys])
    ElMessage.success('菜单权限已更新')
    showMenuDialog.value = false
  } catch (e) {
    ElMessage.error('保存菜单权限失败')
  }
  savingMenu.value = false
}

onMounted(() => {
  fetchRoles()
  fetchMenus()
})
</script>

<style scoped>
.menu-assign-dialog :deep(.el-dialog__body) {
  padding-top: 12px;
  padding-bottom: 4px;
}

.menu-assign-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  gap: 8px;
  flex-wrap: wrap;
}

.menu-assign-actions {
  display: flex;
  gap: 6px;
}

.menu-tree-wrap {
  max-height: 420px;
  overflow-y: auto;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  padding: 4px 0;
}

.menu-label {
  font-weight: 600;
  font-size: 14px;
  color: #303133;
}

.btn-label {
  font-size: 13px;
  color: #409eff;
  font-weight: 500;
}

.perm-tag {
  margin-left: 6px;
  font-size: 11px;
  opacity: 0.65;
}

.menu-node {
  display: flex;
  align-items: center;
  gap: 4px;
}
</style>
