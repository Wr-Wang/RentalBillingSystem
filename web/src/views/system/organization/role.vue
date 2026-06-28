<template>
  <div>
    <div class="page-header">
      <h2>角色管理</h2>
      <el-button type="primary" @click="showDialog = true">
        <el-icon><Plus /></el-icon>新增角色
      </el-button>
    </div>

    <el-table :data="roles" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="name" label="角色名称" width="150" />
      <el-table-column prop="code" label="角色代码" width="150" />
      <el-table-column prop="description" label="说明" min-width="200" />
      <el-table-column prop="isActive" label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '启用' : '停用' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="180" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="editRole(row)">编辑</el-button>
          <el-button text size="small" type="primary" @click="assignMenu(row)">分配菜单</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- Role Dialog -->
    <el-dialog v-model="showDialog" title="角色" width="400px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="角色名称"><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="角色代码"><el-input v-model="form.code" /></el-form-item>
        <el-form-item label="说明"><el-input v-model="form.description" type="textarea" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" @click="saveRole">保存</el-button>
      </template>
    </el-dialog>

    <!-- Assign Menu Dialog -->
    <el-dialog v-model="showMenuDialog" title="分配菜单权限" width="450px">
      <el-tree
        ref="menuTreeRef"
        :data="allMenus"
        show-checkbox
        node-key="id"
        :props="{ label: 'name', children: 'children' }"
        default-expand-all
      />
      <template #footer>
        <el-button @click="showMenuDialog = false">取消</el-button>
        <el-button type="primary" @click="saveMenuAssign">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const showDialog = ref(false)
const showMenuDialog = ref(false)
const menuTreeRef = ref(null)
const form = ref({ name: '', code: '', description: '' })

const roles = ref([
  { id: 'r1', name: '系统管理员', code: 'Admin', description: '系统配置、用户管理、审批流程', isActive: true },
  { id: 'r2', name: '运营主管', code: 'OpsSupervisor', description: '审核合同/费用/抄表', isActive: true },
  { id: 'r3', name: '运营人员', code: 'Operator', description: '日常房屋/合同/租客操作', isActive: true },
  { id: 'r4', name: '财务主管', code: 'FinanceSupervisor', description: '审核收款/会计/对账', isActive: true }
])

const allMenus = ref([
  { id: 'm1', name: '仪表盘', children: [] },
  { id: 'm2', name: '房屋管理', children: [] },
  { id: 'm3', name: '合同管理', children: [] },
  { id: 'm4', name: '收款管理', children: [] },
  { id: 'm5', name: '系统设置', children: [
    { id: 'm5-1', name: '用户管理' },
    { id: 'm5-2', name: '角色管理' }
  ]}
])

function editRole(row) { Object.assign(form.value, row); showDialog.value = true }
function assignMenu(row) { showMenuDialog.value = true }
function saveRole() { ElMessage.success('保存成功'); showDialog.value = false }
function saveMenuAssign() { ElMessage.success('菜单权限已更新'); showMenuDialog.value = false }
</script>
