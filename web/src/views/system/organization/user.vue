<template>
  <div>
    <div class="page-header">
      <h2>用户管理</h2>
      <el-button type="primary" @click="openCreate">
        <el-icon><Plus /></el-icon>新增用户
      </el-button>
    </div>

    <el-table :data="list" stripe v-loading="loading">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="username" label="用户名" width="110" />
      <el-table-column prop="displayName" label="姓名" width="100" />
      <el-table-column prop="phone" label="手机号" width="120" />
      <el-table-column prop="email" label="邮箱" min-width="150" />
      <el-table-column label="所属公司" width="130">
        <template #default="{ row }">
          <el-tag v-if="row.isSuperAdmin" type="danger" size="small">超级管理员</el-tag>
          <el-tag v-else-if="row.homeCompanyName" type="info" size="small">{{ row.homeCompanyName }}</el-tag>
          <span v-else class="text-muted">—</span>
        </template>
      </el-table-column>
      <el-table-column label="角色" width="200">
        <template #default="{ row }">
          <el-tag v-for="r in (row.roleNames || [])" :key="r" size="small" style="margin-right:4px">{{ r }}</el-tag>
          <span v-if="!row.roleNames?.length" class="text-muted">—</span>
        </template>
      </el-table-column>
      <el-table-column label="状态" width="70">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '启用' : '停用' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="180" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button text size="small" :type="row.isActive ? 'danger' : 'success'" @click="toggleStatus(row)">
            {{ row.isActive ? '停用' : '启用' }}
          </el-button>
          <el-button text size="small" type="primary" @click="openChangePwd(row)">改密</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 新增/编辑用户 Dialog -->
    <el-dialog v-model="showDialog" :title="isEdit ? '编辑用户' : '新增用户'" width="550px">
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="用户名" prop="username">
          <el-input v-model="form.username" :disabled="isEdit" />
        </el-form-item>
        <el-form-item label="密码" prop="password" v-if="!isEdit">
          <el-input v-model="form.password" type="password" show-password placeholder="留空默认为 123456" />
        </el-form-item>
        <el-form-item label="姓名">
          <el-input v-model="form.displayName" />
        </el-form-item>
        <el-form-item label="手机号">
          <el-input v-model="form.phone" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="form.email" />
        </el-form-item>
        <el-form-item label="所属公司">
          <el-select v-model="form.homeCompanyId" placeholder="选择公司（选填）" clearable style="width:100%">
            <el-option label="系统用户（无归属）" :value="null" />
            <el-option v-for="l in companyList" :key="l.id" :label="l.name" :value="l.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="超级管理员">
          <el-switch v-model="form.isSuperAdmin" />
          <span style="font-size:12px;color:#909399;margin-left:8px">开启后不受数据权限限制</span>
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="form.roleIds" multiple style="width:100%">
            <el-option v-for="r in roleList" :key="r.id" :label="r.name" :value="r.id" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="save">保存</el-button>
      </template>
    </el-dialog>

    <!-- 修改密码 Dialog -->
    <el-dialog v-model="showPwdDialog" title="修改密码" width="400px">
      <el-form :model="pwdForm" label-width="100px" :rules="pwdRules" ref="pwdFormRef">
        <el-form-item label="新密码" prop="password">
          <el-input v-model="pwdForm.password" type="password" show-password />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showPwdDialog = false">取消</el-button>
        <el-button type="primary" :loading="savingPwd" @click="savePwd">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getUsers, createUser, updateUser, getRoles, getCompanies } from '../../../api/index'

const loading = ref(false)
const showDialog = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref(null)
const list = ref([])
const roleList = ref([])
const companyList = ref([])

const form = ref({
  id: null,
  username: '',
  password: '',
  displayName: '',
  phone: '',
  email: '',
  homeCompanyId: null,
  isSuperAdmin: false,
  roleIds: []
})

const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }]
}

// 修改密码
const showPwdDialog = ref(false)
const savingPwd = ref(false)
const pwdFormRef = ref(null)
const pwdForm = ref({ id: null, password: '' })
const pwdRules = {
  password: [{ required: true, message: '请输入新密码', trigger: 'blur' }]
}

async function fetchUsers() {
  loading.value = true
  try {
    const res = await getUsers()
    list.value = Array.isArray(res) ? res : []
  } catch (e) {
    list.value = []
  }
  loading.value = false
}

async function fetchRoles() {
  try {
    const res = await getRoles()
    roleList.value = Array.isArray(res) ? res : []
  } catch (e) {
    roleList.value = []
  }
}

async function fetchCompanies() {
  try {
    const res = await getCompanies({ pageSize: 100 })
    companyList.value = Array.isArray(res) ? (res.items || res) : []
  } catch (e) {
    companyList.value = []
  }
}

function openCreate() {
  isEdit.value = false
  form.value = { id: null, username: '', password: '', displayName: '', phone: '', email: '', homeCompanyId: null, isSuperAdmin: false, roleIds: [] }
  showDialog.value = true
}

function openEdit(row) {
  isEdit.value = true
  form.value = {
    id: row.id,
    username: row.username,
    password: '',
    displayName: row.displayName || '',
    phone: row.phone || '',
    email: row.email || '',
    homeCompanyId: row.homeCompanyId || null,
    isSuperAdmin: row.isSuperAdmin || false,
    roleIds: row.roleIds ? [...row.roleIds] : []
  }
  showDialog.value = true
}

function openChangePwd(row) {
  pwdForm.value = { id: row.id, password: '' }
  showPwdDialog.value = true
}

async function save() {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      const data = {
        displayName: form.value.displayName || undefined,
        phone: form.value.phone || undefined,
        email: form.value.email || undefined,
        homeCompanyId: form.value.homeCompanyId || undefined,
        isSuperAdmin: form.value.isSuperAdmin,
        roleIds: form.value.roleIds.length > 0 ? form.value.roleIds : []
      }
      await updateUser(form.value.id, data)
      ElMessage.success('用户已更新')
    } else {
      const data = {
        username: form.value.username,
        password: form.value.password || '123456',
        displayName: form.value.displayName,
        phone: form.value.phone || undefined,
        email: form.value.email || undefined,
        homeCompanyId: form.value.homeCompanyId || undefined,
        isSuperAdmin: form.value.isSuperAdmin,
        roleIds: form.value.roleIds.length > 0 ? form.value.roleIds : undefined
      }
      await createUser(data)
      ElMessage.success('用户已创建')
    }
    showDialog.value = false
    await fetchUsers()
  } catch (e) {
    ElMessage.error(isEdit.value ? '更新用户失败' : '创建用户失败')
  }
  saving.value = false
}

async function savePwd() {
  if (!pwdFormRef.value) return
  const valid = await pwdFormRef.value.validate().catch(() => false)
  if (!valid) return
  savingPwd.value = true
  try {
    await updateUser(pwdForm.value.id, { password: pwdForm.value.password })
    ElMessage.success('密码已修改')
    showPwdDialog.value = false
  } catch (e) {
    ElMessage.error('修改密码失败')
  }
  savingPwd.value = false
}

async function toggleStatus(row) {
  const action = row.isActive ? '停用' : '启用'
  try {
    await ElMessageBox.confirm(`确定${action}用户「${row.displayName}」吗？`, '提示', {
      confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning'
    })
    await updateUser(row.id, { isActive: !row.isActive })
    ElMessage.success(`用户已${action}`)
    await fetchUsers()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(`${action}用户失败`)
  }
}

onMounted(() => {
  fetchUsers()
  fetchRoles()
  fetchCompanies()
})
</script>

<style scoped>
.page-actions { display: flex; gap: 8px; }
.text-muted { color: #c0c4cc; }
</style>
