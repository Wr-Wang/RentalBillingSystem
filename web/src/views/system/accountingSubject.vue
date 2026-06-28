<template>
  <div>
    <div class="page-header">
      <h2>会计科目管理</h2>
      <el-button type="primary" @click="openCreate"><el-icon><Plus /></el-icon>新增科目</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="treeData" stripe v-loading="loading" row-key="id" default-expand-all :tree-props="{ children: 'children' }" style="width:100%">
        <el-table-column prop="code" label="科目编码" width="120" />
        <el-table-column prop="name" label="科目名称" min-width="250" />
        <el-table-column label="方向" width="80">
          <template #default="{ row }">{{ row.direction === 'Debit' ? '借方' : '贷方' }}</template>
        </el-table-column>
        <el-table-column prop="level" label="级别" width="60" />
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
            <el-button v-if="!row.children?.length" text size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="showDialog" :title="isEdit ? '编辑科目' : '新增科目'" width="500px">
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="科目编码" prop="code">
          <el-input v-model="form.code" :disabled="isEdit" />
        </el-form-item>
        <el-form-item label="科目名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="方向">
          <el-select v-model="form.direction" style="width:100%">
            <el-option label="借方" value="Debit" />
            <el-option label="贷方" value="Credit" />
          </el-select>
        </el-form-item>
        <el-form-item label="上级科目">
          <el-tree-select v-model="form.parentCode" :data="treeData" :props="{ label: 'name', value: 'code', children: 'children' }" placeholder="不选则为顶级科目" clearable style="width:100%" />
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
import { getAccountingSubjects, createAccountingSubject, updateAccountingSubject, deleteAccountingSubject } from '../../api/index'

const loading = ref(false); const treeData = ref([])
const showDialog = ref(false); const isEdit = ref(false); const saving = ref(false); const formRef = ref(null)
const form = ref({ id: null, code: '', name: '', direction: 'Debit', parentCode: null })
const rules = { code: [{ required: true, message: '请输入科目编码', trigger: 'blur' }], name: [{ required: true, message: '请输入科目名称', trigger: 'blur' }] }

async function fetchList() { loading.value = true; try { const r = await getAccountingSubjects(); treeData.value = Array.isArray(r) ? r : [] } catch { treeData.value = [] }; loading.value = false }

function openCreate() { isEdit.value = false; form.value = { id: null, code: '', name: '', direction: 'Debit', parentCode: null }; showDialog.value = true }
function openEdit(row) { isEdit.value = true; form.value = { id: row.id, code: row.code, name: row.name, direction: row.direction, parentCode: row.parentCode || null }; showDialog.value = true }

async function save() {
  if (!formRef.value) return; if (!(await formRef.value.validate().catch(() => false))) return; saving.value = true
  try {
    if (isEdit.value) {
      await updateAccountingSubject(form.value.id, { name: form.value.name, direction: form.value.direction })
      ElMessage.success('已更新')
    } else {
      await createAccountingSubject({ code: form.value.code, name: form.value.name, direction: form.value.direction, parentCode: form.value.parentCode || undefined })
      ElMessage.success('已创建')
    }
    showDialog.value = false; await fetchList()
  } catch { ElMessage.error(isEdit.value ? '更新失败' : '创建失败') }
  saving.value = false
}

async function toggleStatus(row) {
  const action = row.isActive ? '停用' : '启用'
  try { await ElMessageBox.confirm(`确定${action}「${row.name}」？`, '提示', { type: 'warning' }); await updateAccountingSubject(row.id, { isActive: !row.isActive }); ElMessage.success(`已${action}`); await fetchList() }
  catch (e) { if (e !== 'cancel') ElMessage.error(`${action}失败`) }
}

async function handleDelete(row) { try { await ElMessageBox.confirm(`确定删除「${row.name}」？`, '提示', { type: 'warning' }); await deleteAccountingSubject(row.id); ElMessage.success('已删除'); await fetchList() } catch (e) {} }

onMounted(() => fetchList())
</script>
