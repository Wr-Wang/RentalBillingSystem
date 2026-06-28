<template>
  <div>
    <div class="page-header">
      <h2>税率配置</h2>
      <el-button type="primary" @click="openCreate"><el-icon><Plus /></el-icon>新增税率</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="list" stripe v-loading="loading" style="width:100%">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="name" label="名称" width="160" />
        <el-table-column label="税率" width="120">
          <template #default="{ row }">{{ row.rate }}%</template>
        </el-table-column>
        <el-table-column prop="effectiveDate" label="生效日期" width="120" />
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

    <el-dialog v-model="showDialog" :title="isEdit ? '编辑税率' : '新增税率'" width="500px">
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" placeholder="如：增值税普通发票" />
        </el-form-item>
        <el-form-item label="税率(%)" prop="rate">
          <el-input-number v-model="form.rate" :precision="2" :step="0.5" :min="0" :max="100" style="width:100%" />
        </el-form-item>
        <el-form-item label="生效日期" prop="effectiveDate">
          <el-date-picker v-model="form.effectiveDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
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
import { getTaxRateConfigs, createTaxRateConfig, updateTaxRateConfig, deleteTaxRateConfig } from '../../api/index'

const loading = ref(false); const list = ref([])
const showDialog = ref(false); const isEdit = ref(false); const saving = ref(false); const formRef = ref(null)
const form = ref({ id: null, name: '', rate: 6, effectiveDate: '' })
const rules = { name: [{ required: true, message: '请输入名称', trigger: 'blur' }], rate: [{ required: true, message: '请输入税率', trigger: 'blur' }], effectiveDate: [{ required: true, message: '请选择日期', trigger: 'blur' }] }

async function fetchList() { loading.value = true; try { const r = await getTaxRateConfigs(); list.value = Array.isArray(r) ? r : [] } catch { list.value = [] }; loading.value = false }
function openCreate() { isEdit.value = false; form.value = { id: null, name: '', rate: 6, effectiveDate: '' }; showDialog.value = true }
function openEdit(row) { isEdit.value = true; form.value = { id: row.id, name: row.name, rate: row.rate, effectiveDate: row.effectiveDate }; showDialog.value = true }
async function save() {
  if (!formRef.value) return; if (!(await formRef.value.validate().catch(() => false))) return; saving.value = true
  try {
    if (isEdit.value) { await updateTaxRateConfig(form.value.id, { name: form.value.name, rate: form.value.rate, effectiveDate: form.value.effectiveDate }); ElMessage.success('已更新') }
    else { await createTaxRateConfig({ name: form.value.name, rate: form.value.rate, effectiveDate: form.value.effectiveDate }); ElMessage.success('已创建') }
    showDialog.value = false; await fetchList()
  } catch { ElMessage.error(isEdit.value ? '更新失败' : '创建失败') }
  saving.value = false
}
async function toggleStatus(row) {
  const action = row.isActive ? '停用' : '启用'
  try { await ElMessageBox.confirm(`确定${action}「${row.name}」？`, '提示', { type: 'warning' }); await updateTaxRateConfig(row.id, { isActive: !row.isActive }); ElMessage.success(`已${action}`); await fetchList() }
  catch (e) { if (e !== 'cancel') ElMessage.error(`${action}失败`) }
}
async function handleDelete(row) { try { await ElMessageBox.confirm(`确定删除「${row.name}」？`, '提示', { type: 'warning' }); await deleteTaxRateConfig(row.id); ElMessage.success('已删除'); await fetchList() } catch (e) {} }
onMounted(() => fetchList())
</script>
