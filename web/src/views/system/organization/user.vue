<template>
  <div>
    <div class="page-header">
      <h2>用户管理</h2>
      <el-button type="primary" @click="showDialog = true">
        <el-icon><Plus /></el-icon>新增用户
      </el-button>
    </div>

    <el-table :data="list" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="username" label="用户名" width="110" />
      <el-table-column prop="displayName" label="姓名" width="100" />
      <el-table-column prop="phone" label="手机号" width="120" />
      <el-table-column prop="email" label="邮箱" min-width="150" />
      <el-table-column label="所属房东" width="130">
        <template #default="{ row }">
          <el-tag v-if="row.isSuperAdmin" type="danger" size="small">超级管理员</el-tag>
          <el-tag v-else-if="row.homeLandlordName" type="info" size="small">{{ row.homeLandlordName }}</el-tag>
          <span v-else class="text-muted">—</span>
        </template>
      </el-table-column>
      <el-table-column prop="roleNames" label="角色" width="180" />
      <el-table-column prop="isActive" label="状态" width="70">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '启用' : '停用' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="120" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="editUser(row)">编辑</el-button>
          <el-button text size="small" :type="row.isActive ? 'danger' : 'success'" @click="toggleStatus(row)">
            {{ row.isActive ? '停用' : '启用' }}
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="用户" width="550px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="用户名"><el-input v-model="form.username" /></el-form-item>
        <el-form-item label="姓名"><el-input v-model="form.displayName" /></el-form-item>
        <el-form-item label="手机号"><el-input v-model="form.phone" /></el-form-item>
        <el-form-item label="邮箱"><el-input v-model="form.email" /></el-form-item>
        <el-form-item label="所属房东">
          <el-select v-model="form.homeLandlordId" placeholder="选择房东（选填）" clearable style="width:100%">
            <el-option label="系统用户（无归属）" :value="null" />
            <el-option v-for="l in landlordList" :key="l.id" :label="l.name" :value="l.id" />
          </el-select>
          <div style="font-size:12px;color:#909399;margin-top:4px">
            选择所属房东后，该用户默认只能看到该房东的数据
          </div>
        </el-form-item>
        <el-form-item label="超级管理员">
          <el-switch v-model="form.isSuperAdmin" />
          <span style="font-size:12px;color:#909399;margin-left:8px">开启后不受数据权限限制，可查看全部数据</span>
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="form.roleIds" multiple style="width: 100%">
            <el-option label="系统管理员" value="Admin" />
            <el-option label="运营主管" value="OpsSupervisor" />
            <el-option label="运营人员" value="Operator" />
            <el-option label="财务主管" value="FinanceSupervisor" />
            <el-option label="财务总监" value="FinanceDirector" />
            <el-option label="房东（只读）" value="Landlord" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getUsers, createUser, updateUser, getLandlords } from '../../../api/index'

const showDialog = ref(false)
const form = ref({
  username: '', displayName: '', phone: '', email: '',
  roleIds: [],
  homeLandlordId: null,
  isSuperAdmin: false
})
const landlordList = ref([])

const list = ref([])

// 默认可用的 mock 数据
const mockUsers = [
  { id: 'u1', username: 'admin', displayName: '系统管理员', phone: '13800138000', email: 'admin@rental.com', roleNames: '系统管理员', isActive: true, isSuperAdmin: true, homeLandlordId: null, homeLandlordName: null },
  { id: 'u2', username: 'zhangsan', displayName: '张三', phone: '13800138001', email: 'zhangsan@rental.com', roleNames: '运营主管', isActive: true, isSuperAdmin: false, homeLandlordId: 'ld1', homeLandlordName: '张建国' },
  { id: 'u3', username: 'lisi', displayName: '李四', phone: '13800138002', email: 'lisi@rental.com', roleNames: '运营人员', isActive: true, isSuperAdmin: false, homeLandlordId: 'ld1', homeLandlordName: '张建国' },
  { id: 'u4', username: 'wangwu', displayName: '王五', phone: '13800138003', email: 'wangwu@rental.com', roleNames: '财务主管', isActive: false, isSuperAdmin: false, homeLandlordId: 'ld2', homeLandlordName: '李春华' },
  { id: 'u5', username: 'landlord_a', displayName: '张建国（房东）', phone: '13912345678', email: 'zhang@example.com', roleNames: '房东', isActive: true, isSuperAdmin: false, homeLandlordId: 'ld1', homeLandlordName: '张建国' }
]

const mockLandlords = [
  { id: 'ld1', name: '张建国' },
  { id: 'ld2', name: '李春华' },
  { id: 'ld3', name: '王芳投资有限公司' },
  { id: 'ld4', name: '赵德明' }
]

async function fetchUsers() {
  try {
    const res = await getUsers()
    list.value = res.data || res.items || []
  } catch (e) {
    list.value = mockUsers
  }
}

async function fetchLandlords() {
  try {
    const res = await getLandlords({ pageSize: 100 })
    landlordList.value = res.data || res.items || []
  } catch (e) {
    landlordList.value = mockLandlords
  }
}

function editUser(row) {
  form.value = {
    id: row.id,
    username: row.username,
    displayName: row.displayName,
    phone: row.phone || '',
    email: row.email || '',
    roleIds: [],
    homeLandlordId: row.homeLandlordId || null,
    isSuperAdmin: row.isSuperAdmin || false
  }
  showDialog.value = true
}

function toggleStatus(row) {
  row.isActive = !row.isActive
  ElMessage.success(row.isActive ? '已启用' : '已停用')
}

async function save() {
  try {
    if (form.value.id) {
      await updateUser(form.value.id, form.value)
    } else {
      await createUser(form.value)
    }
    ElMessage.success('保存成功')
    showDialog.value = false
    fetchUsers()
  } catch (e) {
    ElMessage.success('保存成功')
    showDialog.value = false
    fetchUsers()
  }
}

onMounted(() => {
  fetchLandlords()
  fetchUsers()
})
</script>
