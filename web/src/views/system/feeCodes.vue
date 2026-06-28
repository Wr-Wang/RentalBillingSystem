<template>
  <div>
    <div class="page-header">
      <h2>收费项目管理</h2>
      <el-button type="primary" @click="openCreate"><el-icon><Plus /></el-icon>新增费用</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="list" stripe v-loading="loading" style="width:100%">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="code" label="编码" width="100" />
        <el-table-column prop="name" label="名称" width="140" />
        <el-table-column label="计费方式" width="110">
          <template #default="{ row }">{{ row.billingMode === 'MeterBased' ? '按表计量' : '固定金额' }}</template>
        </el-table-column>
        <el-table-column prop="unit" label="单位" width="80" />
        <el-table-column prop="category" label="类别" width="80" />
        <el-table-column prop="sortOrder" label="排序" width="60" />
        <el-table-column label="状态" width="70">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
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

    <el-dialog v-model="showDialog" :title="isEdit ? '编辑收费项目' : '新增收费项目'" width="500px">
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="编码" prop="code">
          <el-input v-model="form.code" :disabled="isEdit" />
        </el-form-item>
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="计费方式">
          <el-select v-model="form.billingMode" style="width:100%">
            <el-option label="固定金额" value="FixedAmount" />
            <el-option label="按表计量" value="MeterBased" />
          </el-select>
        </el-form-item>
        <el-form-item label="计量单位">
          <el-input v-model="form.unit" placeholder="如：元/吨" />
        </el-form-item>
        <el-form-item label="类别">
          <el-select v-model="form.category" style="width:100%">
            <el-option label="房租" value="Rent" />
            <el-option label="水电" value="Utility" />
            <el-option label="物业" value="Property" />
            <el-option label="其他" value="Other" />
          </el-select>
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="form.sortOrder" :min="0" />
        </el-form-item>
        <el-form-item label="必填">
          <el-switch v-model="form.isRequired" />
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
import { getFeeCodes, createFeeCode, updateFeeCode, deleteFeeCode } from '../../api/index'

const loading = ref(false)
const list = ref([])
const showDialog = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref(null)
const form = ref({ id: null, code: '', name: '', billingMode: 'FixedAmount', unit: '', sortOrder: 0, category: 'Other', isRequired: false })
const rules = {
  code: [{ required: true, message: '请输入编码', trigger: 'blur' }],
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }]
}

async function fetchList() {
  loading.value = true
  try { const res = await getFeeCodes(); list.value = Array.isArray(res) ? res : [] }
  catch { list.value = [] }
  loading.value = false
}

function openCreate() {
  isEdit.value = false
  form.value = { id: null, code: '', name: '', billingMode: 'FixedAmount', unit: '', sortOrder: 0, category: 'Other', isRequired: false }
  showDialog.value = true
}

function openEdit(row) {
  isEdit.value = true
  form.value = {
    id: row.id, code: row.code, name: row.name, billingMode: row.billingMode || 'FixedAmount',
    unit: row.unit || '', sortOrder: row.sortOrder || 0, category: row.category || 'Other', isRequired: row.isRequired || false
  }
  showDialog.value = true
}

async function save() {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await updateFeeCode(form.value.id, {
        name: form.value.name, billingMode: form.value.billingMode, unit: form.value.unit || undefined,
        sortOrder: form.value.sortOrder, category: form.value.category, isRequired: form.value.isRequired
      })
      ElMessage.success('已更新')
    } else {
      await createFeeCode({
        code: form.value.code, name: form.value.name, billingMode: form.value.billingMode,
        unit: form.value.unit || undefined, sortOrder: form.value.sortOrder, category: form.value.category, isRequired: form.value.isRequired
      })
      ElMessage.success('已创建')
    }
    showDialog.value = false; await fetchList()
  } catch { ElMessage.error(isEdit.value ? '更新失败' : '创建失败') }
  saving.value = false
}

async function toggleStatus(row) {
  const action = row.isActive ? '停用' : '启用'
  try {
    await ElMessageBox.confirm(`确定${action}「${row.name}」？`, '提示', { type: 'warning' })
    await updateFeeCode(row.id, { isActive: !row.isActive })
    ElMessage.success(`已${action}`); await fetchList()
  } catch (e) { if (e !== 'cancel') ElMessage.error(`${action}失败`) }
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm(`确定删除「${row.name}」？`, '提示', { type: 'warning' })
    await deleteFeeCode(row.id)
    ElMessage.success('已删除'); await fetchList()
  } catch (e) { /* cancel */ }
}

onMounted(() => fetchList())
</script>
