<template>
  <div>
    <div class="page-header">
      <h2>节假日管理</h2>
      <div style="display:flex;gap:8px;">
        <el-select v-model="searchYear" style="width:120px;" @change="fetchList">
          <el-option v-for="y in yearOptions" :key="y" :label="y + '年'" :value="y" />
        </el-select>
        <el-button type="primary" @click="openCreate"><el-icon><Plus /></el-icon>新增</el-button>
        <el-button @click="generateYearData" :loading="generating">国务院导入</el-button>
      </div>
    </div>

    <!-- 统计卡片 -->
    <el-row :gutter="12" style="margin-bottom:12px;">
      <el-col :span="6"><el-card shadow="never"><div style="text-align:center;"><div style="font-size:24px;font-weight:700;color:#409eff;">{{ totalHolidays }}</div><div style="font-size:12px;color:#909399;">放假天数</div></div></el-card></el-col>
      <el-col :span="6"><el-card shadow="never"><div style="text-align:center;"><div style="font-size:24px;font-weight:700;color:#e6a23c;">{{ totalWorkdays }}</div><div style="font-size:12px;color:#909399;">调休上班</div></div></el-card></el-col>
      <el-col :span="6"><el-card shadow="never"><div style="text-align:center;"><div style="font-size:24px;font-weight:700;color:#67c23a;">{{ daysOff }}</div><div style="font-size:12px;color:#909399;">连续最长假期</div></div></el-card></el-col>
      <el-col :span="6"><el-card shadow="never"><div style="text-align:center;"><div style="font-size:24px;font-weight:700;color:#909399;">{{ searchYear }}</div><div style="font-size:12px;color:#909399;">当前年份</div></div></el-card></el-col>
    </el-row>

    <!-- 日历视图 + 列表 -->
    <el-card shadow="never">
      <el-tabs v-model="viewMode">
        <el-tab-pane label="日历视图" name="calendar">
          <div style="display:flex;flex-wrap:wrap;gap:4px;">
            <template v-for="m in 12" :key="m">
              <div style="width:31%;min-width:280px;border:1px solid #ebeef5;border-radius:6px;padding:8px;margin-bottom:8px;">
                <div style="font-weight:600;font-size:14px;margin-bottom:6px;color:#303133;">{{ m }}月</div>
                <div style="display:grid;grid-template-columns:repeat(7,1fr);gap:2px;font-size:11px;">
                  <div v-for="d in weekDays" :key="d" style="text-align:center;color:#909399;font-weight:500;">{{ d }}</div>
                  <div v-for="day in calendarDays(m)" :key="day.key"
                    :style="dayStyle(day)" :title="day.tip">{{ day.label }}</div>
                </div>
              </div>
            </template>
          </div>
        </el-tab-pane>
        <el-tab-pane label="列表视图" name="list">
          <el-table :data="list" stripe v-loading="loading" size="small">
            <el-table-column type="index" label="#" width="45" />
            <el-table-column label="日期" width="130">
              <template #default="{row}"><span :style="{color:isWeekend(row.holidayDate)?'#e6a23c':'#303133'}">{{ row.dateDisplay||row.holidayDate }}</span></template>
            </el-table-column>
            <el-table-column label="星期" width="70">
              <template #default="{row}">{{ getWeekday(row.holidayDate) }}</template>
            </el-table-column>
            <el-table-column prop="name" label="名称" min-width="150" />
            <el-table-column label="类型" width="100">
              <template #default="{row}">
                <el-tag :type="row.isWorkingDay?'warning':'danger'" size="small" effect="dark" round>{{ row.isWorkingDay?'调休上班':'放假' }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="140" fixed="right">
              <template #default="{row}">
                <el-button text size="small" @click="openEdit(row)">编辑</el-button>
                <el-button text size="small" type="danger" @click="handleDelete(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <el-dialog v-model="showDialog" :title="isEdit?'编辑':'新增节假日'" width="450px">
      <el-form :model="form" label-width="90px" :rules="rules" ref="formRef">
        <el-form-item label="日期" prop="holidayDate"><el-date-picker v-model="form.holidayDate" type="date" value-format="YYYY-MM-DD" style="width:100%" /></el-form-item>
        <el-form-item label="名称"><el-input v-model="form.name" placeholder="如：国庆节" /></el-form-item>
        <el-form-item label="类型"><el-select v-model="form.isWorkingDay" style="width:100%"><el-option label="放假" :value="false" /><el-option label="调休上班" :value="true" /></el-select></el-form-item>
      </el-form>
      <template #footer><el-button @click="showDialog=false">取消</el-button><el-button type="primary" :loading="saving" @click="save">保存</el-button></template>
    </el-dialog>

    <el-dialog v-model="showImportResult" title="导入结果" width="600px">
      <div style="margin-bottom:12px"><el-tag type="success" style="margin-right:8px">新增 {{ importResult.importedCount||0 }} 条</el-tag><el-tag type="warning">跳过 {{ importResult.skippedCount||0 }} 条</el-tag></div>
      <el-tabs v-if="importResult.imported?.length||importResult.skipped?.length">
        <el-tab-pane v-if="importResult.imported?.length" :label="'新增 ('+importResult.importedCount+')'"><el-table :data="importResult.imported" size="small" max-height="300"><el-table-column prop="holidayDate" label="日期" width="120"/><el-table-column prop="name" label="名称" min-width="120"/><el-table-column label="类型" width="80"><template #default="{row}">{{ row.isWorkingDay?'调休上班':'放假' }}</template></el-table-column></el-table></el-tab-pane>
        <el-tab-pane v-if="importResult.skipped?.length" :label="'跳过 ('+importResult.skippedCount+')'"><el-table :data="importResult.skipped" size="small" max-height="300"><el-table-column prop="holidayDate" label="日期" width="120"/><el-table-column prop="name" label="名称" min-width="120"/><el-table-column label="类型" width="80"><template #default="{row}">{{ row.isWorkingDay?'调休上班':'放假' }}</template></el-table-column></el-table></el-tab-pane>
      </el-tabs>
      <template #footer><el-button @click="showImportResult=false">关闭</el-button></template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getHolidayCalendars, createHolidayCalendar, updateHolidayCalendar, deleteHolidayCalendar, importHolidayYear } from '../../api/index'

const loading = ref(false); const list = ref([])
const searchYear = ref(new Date().getFullYear())
const yearOptions = computed(() => { const y = new Date().getFullYear(); return [y - 1, y, y + 1, y + 2] })
const viewMode = ref('calendar')
const weekDays = ['一','二','三','四','五','六','日']

const totalHolidays = computed(() => list.value.filter(h => !h.isWorkingDay).length)
const totalWorkdays = computed(() => list.value.filter(h => h.isWorkingDay).length)
const daysOff = computed(() => {
  let max = 0, cur = 0
  const sorted = list.value.filter(h => !h.isWorkingDay).sort((a,b) => (a.holidayDate||a.dateDisplay||'') > (b.holidayDate||b.dateDisplay||'') ? 1 : -1)
  for (let i = 0; i < sorted.length; i++) {
    const d = sorted[i].holidayDate || sorted[i].dateDisplay
    if (!d) continue
    if (i > 0) {
      const prev = sorted[i-1].holidayDate || sorted[i-1].dateDisplay
      const diff = (new Date(d) - new Date(prev)) / 86400000
      if (diff === 1) { cur++; max = Math.max(max, cur) } else { cur = 1 }
    } else { cur = 1 }
  }
  return max + '天'
})

const holidayLookup = ref({})
function getWeekday(d) { if (!d) return ''; const w = ['一','二','三','四','五','六','日']; return '周' + w[new Date(d).getDay() === 0 ? 6 : new Date(d).getDay() - 1] }
function isWeekend(d) { if (!d) return false; const day = new Date(d).getDay(); return day === 0 || day === 6 }

function calendarDays(month) {
  const year = searchYear.value
  const first = new Date(year, month - 1, 1)
  const last = new Date(year, month, 0)
  const days = []
  const startPadding = first.getDay() === 0 ? 6 : first.getDay() - 1
  for (let i = 0; i < startPadding; i++) days.push({ key: 'p' + i, label: '' })
  for (let d = 1; d <= last.getDate(); d++) {
    const dateStr = `${year}-${String(month).padStart(2,'0')}-${String(d).padStart(2,'0')}`
    const info = holidayLookup.value[dateStr]
    const isWeekendDay = new Date(year, month-1, d).getDay() === 0 || new Date(year, month-1, d).getDay() === 6
    days.push({
      key: dateStr, label: d, date: dateStr,
      isHoliday: info && !info.isWorkingDay,
      isWorkday: info && info.isWorkingDay,
      isWeekend: isWeekendDay && !info,
      tip: info ? info.name : '',
      info
    })
  }
  return days
}

function dayStyle(day) {
  if (!day.label) return { visibility: 'hidden' }
  let bg = '#fff', color = '#303133', radius = '4px', cursor = 'default'
  if (day.isHoliday) { bg = '#fef0f0'; color = '#f56c6c'; cursor = 'pointer' }
  else if (day.isWorkday) { bg = '#fdf6ec'; color = '#e6a23c'; cursor = 'pointer' }
  else if (day.isWeekend) { color = '#c0c4cc' }
  return { background: bg, color, borderRadius: radius, textAlign: 'center', padding: '2px 0', cursor, fontSize: '12px' }
}

async function fetchList() {
  loading.value = true
  try {
    const res = await getHolidayCalendars({ year: searchYear.value })
    list.value = Array.isArray(res) ? res : []
    holidayLookup.value = {}
    list.value.forEach(h => { holidayLookup.value[h.holidayDate] = h })
  } catch { list.value = [] }
  loading.value = false
}

const showDialog = ref(false); const isEdit = ref(false); const saving = ref(false)
const formRef = ref(null); const form = ref({ id: null, holidayDate: '', name: '', isWorkingDay: false })
const rules = { holidayDate: [{ required: true, message: '请选择日期', trigger: 'blur' }] }

function openCreate() { isEdit.value = false; form.value = { id: null, holidayDate: '', name: '', isWorkingDay: false }; showDialog.value = true }
function openEdit(row) { isEdit.value = true; form.value = { id: row.id, holidayDate: row.holidayDate, name: row.name || '', isWorkingDay: row.isWorkingDay || false }; showDialog.value = true }
async function save() {
  if (!formRef.value) return; const valid = await formRef.value.validate().catch(() => false); if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) { await updateHolidayCalendar(form.value.id, { name: form.value.name||undefined, isWorkingDay: form.value.isWorkingDay }); ElMessage.success('已更新') }
    else { await createHolidayCalendar({ holidayDate: form.value.holidayDate, name: form.value.name||undefined, isWorkingDay: form.value.isWorkingDay, companyId: '00000000-0000-0000-0000-000000000000' }); ElMessage.success('已创建') }
    showDialog.value = false; await fetchList()
  } catch { ElMessage.error('操作失败') }
  saving.value = false
}
async function handleDelete(row) {
  try { await ElMessageBox.confirm(`确定删除「${row.name||row.holidayDate}」？`,'提示',{type:'warning'}); await deleteHolidayCalendar(row.id); ElMessage.success('已删除'); await fetchList() } catch {}
}

const generating = ref(false); const showImportResult = ref(false); const importResult = ref({})
async function generateYearData() {
  generating.value = true
  try { const res = await importHolidayYear(searchYear.value); importResult.value = res||{}; showImportResult.value = true; ElMessage.success(res?.message||'导入完成'); await fetchList() }
  catch { ElMessage.error('导入失败，请检查网络') }
  generating.value = false
}

onMounted(fetchList)
</script>
<style scoped>
.page-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:12px;}
</style>
