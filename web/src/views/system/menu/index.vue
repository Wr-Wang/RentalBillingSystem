<template>
  <div>
    <div class="page-header">
      <h2>菜单权限配置</h2>
      <el-button type="primary" @click="showDialog = true">
        <el-icon><Plus /></el-icon>新增菜单
      </el-button>
    </div>

    <el-card>
      <el-table :data="menuTree" row-key="id" default-expand-all :tree-props="{ children: 'children' }" stripe>
        <el-table-column prop="name" label="菜单名称" min-width="200" />
        <el-table-column prop="path" label="路由路径" width="200" />
        <el-table-column prop="icon" label="图标" width="80">
          <template #default="{ row }">
            <el-icon v-if="row.icon"><component :is="row.icon" /></el-icon>
          </template>
        </el-table-column>
        <el-table-column prop="permissionCode" label="权限代码" width="160" />
        <el-table-column prop="sortOrder" label="排序" width="60" />
        <el-table-column prop="isEnabled" label="启用" width="60">
          <template #default="{ row }"><el-switch v-model="row.isEnabled" /></template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="editMenu(row)">编辑</el-button>
            <el-button text size="small" type="danger" @click="deleteMenu(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="showDialog" title="菜单" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="菜单名称"><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="路由路径"><el-input v-model="form.path" /></el-form-item>
        <el-form-item label="图标"><el-input v-model="form.icon" /></el-form-item>
        <el-form-item label="权限代码"><el-input v-model="form.permissionCode" /></el-form-item>
        <el-form-item label="上级菜单">
          <el-tree-select v-model="form.parentId" :data="menuTree" :props="{ label: 'name', value: 'id' }" placeholder="不选则为顶级" clearable style="width: 100%" />
        </el-form-item>
        <el-form-item label="排序"><el-input-number v-model="form.sortOrder" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" @click="saveMenu">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const showDialog = ref(false)
const form = ref({ name: '', path: '', icon: '', permissionCode: '', parentId: null, sortOrder: 0 })

const menuTree = ref([
  { id: 'm1', name: '仪表盘', path: '/dashboard', icon: 'DataAnalysis', permissionCode: 'dashboard:view', sortOrder: 1, isEnabled: true, children: [] },
  { id: 'm2', name: '房屋管理', path: '/buildings', icon: 'HomeFilled', permissionCode: 'building:view', sortOrder: 2, isEnabled: true, children: [
    { id: 'm2-1', name: '房间列表', path: '/buildings', icon: '', permissionCode: 'building:list', sortOrder: 1, isEnabled: true },
    { id: 'm2-2', name: '批量导入', path: '/buildings/import', icon: '', permissionCode: 'building:import', sortOrder: 2, isEnabled: true }
  ]},
  { id: 'm3', name: '合同管理', path: '/contracts', icon: 'Document', permissionCode: 'contract:view', sortOrder: 3, isEnabled: true, children: [] }
])

function editMenu(row) { Object.assign(form.value, row); showDialog.value = true }
function deleteMenu(row) {
  ElMessageBox.confirm(`确定删除 ${row.name} 吗？`, '提示').then(() => {
    ElMessage.success('已删除')
  }).catch(() => {})
}
function saveMenu() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
