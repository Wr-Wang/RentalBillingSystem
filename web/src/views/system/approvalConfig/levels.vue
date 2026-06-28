<template>
  <div>
    <div class="page-header">
      <h2>审批级别配置 - {{ typeName || '未知类型' }}</h2>
      <el-button @click="$router.back()">◀ 返回</el-button>
    </div>
    <el-card shadow="never">
      <template #header>
        <span>级别列表</span>
        <el-button text type="primary" style="float:right" @click="openCreate">新增级别</el-button>
      </template>
      <el-table :data="levels" stripe v-loading="loading" style="width:100%">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="level" label="级序号" width="80" />
        <el-table-column prop="roleName" label="审批角色" width="150" />
        <el-table-column label="最小金额" width="120">
          <template #default="{ row }">{{ row.minAmount !== null && row.minAmount !== undefined ? '¥' + row.minAmount : '-' }}</template>
        </el-table-column>
        <el-table-column label="最大金额" width="120">
          <template #default="{ row }">{{ row.maxAmount !== null && row.maxAmount !== undefined ? '¥' + row.maxAmount : '-' }}</template>
        </el-table-column>
        <el-table-column label="操作" width="160">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
            <el-button text size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
    <el-dialog v-model="showDialog" :title="isEdit ? '编辑级别' : '新增级别'" width="500px">
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="审批角色" prop="roleId">
          <el-select v-model="form.roleId" style="width:100%">
            <el-option v-for="r in roleList" :key="r.id" :label="r.name" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="最小金额">
          <el-input-number v-model="form.minAmount" :min="0" :precision="2" style="width:100%" />
        </el-form-item>
        <el-form-item label="最大金额">
          <el-input-number v-model="form.maxAmount" :min="0" :precision="2" style="width:100%" />
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
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getApprovalLevels, createApprovalLevel, updateApprovalLevel, deleteApprovalLevel, getRoles } from '../../../api/index'

const route = useRoute()
const typeId = ref(route.query.typeId || '')
const typeName = ref(route.query.typeName || '')
const loading = ref(false); const levels = ref([])
const roleList = ref([])
const showDialog = ref(false); const isEdit = ref(false); const saving = ref(false); const formRef = ref(null)
const form = ref({ id: null, roleId: '', minAmount: 0, maxAmount: 0 })
const rules = { roleId: [{ required: true, message: '请选择审批角色', trigger: 'change' }] }

async function fetchLevels() {
  if (!typeId.value) return; loading.value = true
  try { const res = await getApprovalLevels(typeId.value); levels.value = Array.isArray(res) ? res : [] }
  catch { levels.value = [] }; loading.value = false
}
async function fetchRoles() { try { const res = await getRoles(); roleList.value = Array.isArray(res) ? res : [] } catch { roleList.value = [] } }

function openCreate() { isEdit.value = false; form.value = { id: null, roleId: '', minAmount: 0, maxAmount: 99999999 }; showDialog.value = true }
function openEdit(row) { isEdit.value = true; form.value = { id: row.id, roleId: row.roleId, minAmount: row.minAmount || 0, maxAmount: row.maxAmount || 99999999 }; showDialog.value = true }

async function save() {
  if (!formRef.value) return; if (!(await formRef.value.validate().catch(() => false))) return; saving.value = true
  try {
    if (isEdit.value) {
      await updateApprovalLevel(form.value.id, { roleId: form.value.roleId, minAmount: form.value.minAmount, maxAmount: form.value.maxAmount })
      ElMessage.success('已更新')
    } else {
      await createApprovalLevel(typeId.value, { approvalTypeId: typeId.value, level: levels.value.length + 1, roleId: form.value.roleId, minAmount: form.value.minAmount, maxAmount: form.value.maxAmount })
      ElMessage.success('已创建')
    }
    showDialog.value = false; await fetchLevels()
  } catch { ElMessage.error(isEdit.value ? '更新失败' : '创建失败') }
  saving.value = false
}
async function handleDelete(row) {
  try { await ElMessageBox.confirm('确定删除此级别？', '提示', { type: 'warning' }); await deleteApprovalLevel(row.id); ElMessage.success('已删除'); await fetchLevels() }
  catch (e) { /* cancel */ }
}
onMounted(() => { fetchLevels(); fetchRoles() })
</script>
