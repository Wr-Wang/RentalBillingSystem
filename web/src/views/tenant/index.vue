<template>
  <div>
    <div class="page-header">
      <h2>租客管理</h2>
      <el-button type="primary" @click="openCreate">
        <el-icon><Plus /></el-icon>新增租客
      </el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="姓名/身份证/电话" clearable style="width: 220px;" @clear="fetchList" @keyup.enter="fetchList" />
      <el-button type="primary" @click="fetchList">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-table :data="tenantList" v-loading="loading" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="name" label="姓名" width="120" />
      <el-table-column prop="idCard" label="证件号码" width="200" />
      <el-table-column prop="phone" label="电话" width="140" />
      <el-table-column prop="email" label="邮箱" min-width="180" />
      <el-table-column label="操作" width="150" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="$router.push('/tenants/' + row.id)">详情</el-button>
          <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button text size="small" type="danger" @click="deleteTenant(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination
        v-model:page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @current-change="fetchList"
        @size-change="fetchList"
      />
    </div>

    <el-dialog v-model="showDialog" :title="isEdit ? '编辑租客' : '新增租客'" width="450px">
      <el-form :model="tenantForm" label-width="100px">
        <el-form-item label="姓名">
          <el-input v-model="tenantForm.name" />
        </el-form-item>
        <el-form-item label="证件号码">
          <el-input v-model="tenantForm.idCard" />
        </el-form-item>
        <el-form-item label="电话">
          <el-input v-model="tenantForm.phone" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="tenantForm.email" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="saveTenant">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '../../store/user'
import { getTenants, createTenant, updateTenant, deleteTenant as deleteTenantApi } from '../../api/index'

const userStore = useUserStore()
const search = reactive({ keyword: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })
const tenantList = ref([])
const loading = ref(false)
const showDialog = ref(false)
const isEdit = ref(false)
const saving = ref(false)

const tenantForm = reactive({
  id: null,
  name: '',
  idCard: '',
  phone: '',
  email: ''
})

function getEffectiveCompanyId() {
  return userStore.effectiveCompanyId || userStore.companyId
}

async function fetchList() {
  loading.value = true
  try {
    const companyId = getEffectiveCompanyId()
    if (!companyId) {
      tenantList.value = []
      pagination.total = 0
      return
    }
    const params = {
      companyId,
      page: pagination.page,
      pageSize: pagination.pageSize,
      keyword: search.keyword || undefined
    }
    const res = await getTenants(params)
    const items = res.items || res.data || []
    tenantList.value = items.map(t => ({
      id: t.id,
      name: t.name || '',
      idCard: t.idCard || '',
      phone: t.phone || '',
      email: t.email || '',
      currentContract: '-'
    }))
    pagination.total = res.total ?? items.length
  } catch {
    ElMessage.error('加载租客列表失败')
  }
  loading.value = false
}

function resetSearch() {
  search.keyword = ''
  pagination.page = 1
  fetchList()
}

function openCreate() {
  isEdit.value = false
  tenantForm.id = null
  tenantForm.name = ''
  tenantForm.idCard = ''
  tenantForm.phone = ''
  tenantForm.email = ''
  showDialog.value = true
}

function openEdit(row) {
  isEdit.value = true
  tenantForm.id = row.id
  tenantForm.name = row.name
  tenantForm.idCard = row.idCard
  tenantForm.phone = row.phone
  tenantForm.email = row.email
  showDialog.value = true
}

async function saveTenant() {
  if (!tenantForm.name) {
    ElMessage.warning('请输入姓名')
    return
  }
  const companyId = getEffectiveCompanyId()
  if (!companyId) { ElMessage.warning('请先选择公司'); return }

  saving.value = true
  try {
    const data = {
      name: tenantForm.name,
      idCard: tenantForm.idCard || undefined,
      phone: tenantForm.phone || undefined,
      email: tenantForm.email || undefined,
      companyId
    }
    if (isEdit.value) {
      await updateTenant(tenantForm.id, data)
      ElMessage.success('租客信息已更新')
    } else {
      await createTenant(data)
      ElMessage.success('租客创建成功')
    }
    showDialog.value = false
    await fetchList()
  } catch (e) {
    ElMessage.error(isEdit.value ? '更新租客失败' : '创建租客失败')
  }
  saving.value = false
}

async function deleteTenant(row) {
  try {
    await ElMessageBox.confirm(`确定删除租客「${row.name}」吗？`, '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await deleteTenantApi(row.id)
    ElMessage.success('租客已删除')
    await fetchList()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('删除租客失败')
  }
}

onMounted(() => {
  if (getEffectiveCompanyId()) fetchList()
})
</script>
