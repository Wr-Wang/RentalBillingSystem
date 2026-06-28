<template>
  <div>
    <div class="page-header">
      <h2>节假日管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="openCreate"><el-icon><Plus /></el-icon>新增</el-button>
        <el-button @click="generateYearData" :loading="generating">导入国务院节假日安排</el-button>
      </div>
    </div>

    <el-card shadow="never" class="search-bar">
      <el-form :inline="true">
        <el-form-item label="年份">
          <el-select v-model="searchYear" style="width: 120px;">
            <el-option v-for="y in yearOptions" :key="y" :label="y + '年'" :value="y" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never">
      <el-table :data="list" stripe v-loading="loading" style="width:100%">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column label="日期" min-width="160">
          <template #default="{ row }">{{ row.dateDisplay || row.holidayDate }}</template>
        </el-table-column>
        <el-table-column prop="name" label="名称" min-width="150" />
        <el-table-column label="类型" width="120">
          <template #default="{ row }">
            <el-tag :type="row.isWorkingDay ? 'success' : 'danger'" size="small">
              {{ row.isWorkingDay ? '调休上班' : '放假' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
            <el-button text size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 导入结果 Dialog -->
    <el-dialog v-model="showImportResult" title="导入结果" width="600px">
      <div style="margin-bottom:12px">
        <el-tag type="success" style="margin-right:8px">新增 {{ importResult.importedCount || 0 }} 条</el-tag>
        <el-tag type="warning">跳过 {{ importResult.skippedCount || 0 }} 条（已存在）</el-tag>
      </div>
      <el-tabs v-if="importResult.imported?.length || importResult.skipped?.length">
        <el-tab-pane v-if="importResult.imported?.length" :label="'新增 (' + importResult.importedCount + ')'">
          <el-table :data="importResult.imported" size="small" max-height="300">
            <el-table-column prop="holidayDate" label="日期" width="130" />
            <el-table-column prop="name" label="名称" width="150" />
            <el-table-column label="类型" width="100">
              <template #default="{ row }">{{ row.isWorkingDay ? '调休上班' : '放假' }}</template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
        <el-tab-pane v-if="importResult.skipped?.length" :label="'跳过 (' + importResult.skippedCount + ')'">
          <el-table :data="importResult.skipped" size="small" max-height="300">
            <el-table-column prop="holidayDate" label="日期" width="130" />
            <el-table-column prop="name" label="名称" width="150" />
            <el-table-column label="类型" width="100">
              <template #default="{ row }">{{ row.isWorkingDay ? '调休上班' : '放假' }}</template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
      </el-tabs>
      <template #footer><el-button @click="showImportResult = false">关闭</el-button></template>
    </el-dialog>

    <!-- 新增/编辑 Dialog -->
    <el-dialog v-model="showDialog" :title="isEdit ? '编辑节假日' : '新增节假日'" width="500px">
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="日期" prop="holidayDate">
          <el-date-picker v-model="form.holidayDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
        </el-form-item>
        <el-form-item label="名称">
          <el-input v-model="form.name" placeholder="如：国庆节" />
        </el-form-item>
        <el-form-item label="类型">
          <el-select v-model="form.isWorkingDay" style="width:100%">
            <el-option :label="'放假'" :value="false" />
            <el-option :label="'调休上班'" :value="true" />
          </el-select>
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
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getHolidayCalendars, createHolidayCalendar, updateHolidayCalendar, deleteHolidayCalendar, importHolidayYear } from '../../api/index'

const loading = ref(false)
const list = ref([])
const searchYear = ref(new Date().getFullYear())
const yearOptions = computed(() => {
  const y = new Date().getFullYear()
  return [y - 1, y, y + 1, y + 2]
})

const showDialog = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref(null)
const form = ref({ id: null, holidayDate: '', name: '', isWorkingDay: false })
const rules = {
  holidayDate: [{ required: true, message: '请选择日期', trigger: 'blur' }]
}

const generating = ref(false)
const showImportResult = ref(false)
const importResult = ref({})

async function fetchList() {
  loading.value = true
  try {
    const res = await getHolidayCalendars({ year: searchYear.value })
    list.value = Array.isArray(res) ? res : []
  } catch (e) {
    list.value = []
  }
  loading.value = false
}

function openCreate() {
  isEdit.value = false
  form.value = { id: null, holidayDate: '', name: '', isWorkingDay: false }
  showDialog.value = true
}

function openEdit(row) {
  isEdit.value = true
  form.value = {
    id: row.id,
    holidayDate: row.holidayDate,
    name: row.name || '',
    isWorkingDay: row.isWorkingDay || false
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
      await updateHolidayCalendar(form.value.id, {
        name: form.value.name || undefined,
        isWorkingDay: form.value.isWorkingDay
      })
      ElMessage.success('已更新')
    } else {
      await createHolidayCalendar({
        holidayDate: form.value.holidayDate,
        name: form.value.name || undefined,
        isWorkingDay: form.value.isWorkingDay,
        companyId: '00000000-0000-0000-0000-000000000000'
      })
      ElMessage.success('已创建')
    }
    showDialog.value = false
    await fetchList()
  } catch (e) {
    ElMessage.error(isEdit.value ? '更新失败' : '创建失败')
  }
  saving.value = false
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm(`确定删除「${row.name || row.holidayDate}」？`, '提示', { type: 'warning' })
    await deleteHolidayCalendar(row.id)
    ElMessage.success('已删除')
    await fetchList()
  } catch (e) { /* cancel */ }
}

async function generateYearData() {
  generating.value = true
  try {
    const res = await importHolidayYear(searchYear.value)
    importResult.value = res || {}
    showImportResult.value = true
    ElMessage.success(res?.message || `导入完成`)
    await fetchList()
  } catch (e) {
    ElMessage.error('导入失败，请检查网络连接')
  }
  generating.value = false
}

onMounted(() => fetchList())
</script>

<style scoped>
.search-bar { margin-bottom: 16px; display: flex; gap: 12px; align-items: center; }
.table-actions { display: flex; gap: 8px; }
</style>
