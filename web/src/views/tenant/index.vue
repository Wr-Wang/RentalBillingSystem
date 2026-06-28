<template>
  <div>
    <div class="page-header">
      <h2>租客管理</h2>
      <el-button type="primary" @click="showAddTenant = true">
        <el-icon><Plus /></el-icon>新增租客
      </el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="姓名/身份证/电话" clearable style="width: 220px;" />
      <el-button type="primary" @click="handleSearch">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-table :data="tenantList" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="name" label="姓名" width="120" />
      <el-table-column prop="identityNo" label="身份证号" width="200" />
      <el-table-column prop="phone" label="电话" width="140" />
      <el-table-column prop="email" label="邮箱" min-width="180" />
      <el-table-column prop="currentContract" label="当前合同" width="130" />
      <el-table-column label="操作" width="150" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="$router.push('/tenants/' + row.id)">详情</el-button>
          <el-button text size="small" type="primary" @click="editTenant(row)">编辑</el-button>
          <el-button text size="small" type="danger" @click="deleteTenant(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" layout="total, sizes, prev, pager, next" />
    </div>

    <el-dialog v-model="showAddTenant" title="新增租客" width="450px">
      <el-form :model="tenantForm" label-width="100px">
        <el-form-item label="姓名">
          <el-input v-model="tenantForm.name" />
        </el-form-item>
        <el-form-item label="证件类型">
          <el-select v-model="tenantForm.identityType" style="width: 100%">
            <el-option label="身份证" value="PRC_ID" />
            <el-option label="护照" value="PASSPORT" />
          </el-select>
        </el-form-item>
        <el-form-item label="证件号">
          <el-input v-model="tenantForm.identityNo" />
        </el-form-item>
        <el-form-item label="电话">
          <el-input v-model="tenantForm.phone" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="tenantForm.email" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddTenant = false">取消</el-button>
        <el-button type="primary" @click="saveTenant">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const search = reactive({ keyword: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 15 })
const showAddTenant = ref(false)

const tenantForm = reactive({
  name: '', identityType: 'PRC_ID', identityNo: '', phone: '', email: ''
})

const tenantList = ref([
  { id: 't1', name: '张三', identityType: 'PRC_ID', identityNo: '110101199001011234', phone: '13800138001', email: 'zhangsan@email.com', currentContract: 'HT-2026-001' },
  { id: 't2', name: '李四', identityType: 'PRC_ID', identityNo: '110101199002021235', phone: '13800138002', email: 'lisi@email.com', currentContract: 'HT-2026-002' },
  { id: 't3', name: '王五', identityType: 'PRC_ID', identityNo: '110101199003031236', phone: '13800138003', email: 'wangwu@email.com', currentContract: 'HT-2026-003' },
  { id: 't4', name: '赵六', identityType: 'PRC_ID', identityNo: '110101199004041237', phone: '13800138004', email: 'zhaoliu@email.com', currentContract: '-' },
  { id: 't5', name: '孙七', identityType: 'PRC_ID', identityNo: '110101199005051238', phone: '13800138005', email: 'sunqi@email.com', currentContract: '-' }
])

function handleSearch() { ElMessage.info('搜索功能待API接入') }
function resetSearch() { search.keyword = '' }

function editTenant(row) {
  Object.assign(tenantForm, row)
  showAddTenant.value = true
}

function deleteTenant(row) {
  ElMessageBox.confirm(`确定删除租客 ${row.name} 吗？`, '提示').then(() => {
    ElMessage.success('租客已删除')
  }).catch(() => {})
}

function saveTenant() {
  ElMessage.success('租客信息已保存')
  showAddTenant.value = false
}
</script>
