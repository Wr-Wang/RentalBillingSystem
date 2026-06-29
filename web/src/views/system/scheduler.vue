<template>
  <div>
    <!-- ===== 第一层：任务卡片总览 ===== -->
    <template v-if="!selectedJob">
      <div class="page-header">
        <h2>调度任务管理</h2>
        <el-button type="primary" size="small" @click="openCreateWizard">+ 新建任务</el-button>
      </div>

      <el-row :gutter="16">
        <el-col v-for="job in jobs" :key="job.id" :span="6" style="margin-bottom: 16px;">
          <el-card shadow="hover" :body-style="{ padding: '14px' }">
            <div style="display:flex;justify-content:space-between;align-items:flex-start;">
              <div>
                <div style="font-weight:600;font-size:15px;">{{ job.templateCode ? getTemplateIcon(job.templateCode) : '⚙️' }} {{ job.jobName }}</div>
                <div style="font-size:12px;color:#909399;margin-top:4px;">{{ job.scheduleType === 'Daily' ? '每天' : '每月' }} {{ String(job.hour).padStart(2,'0') }}:{{ String(job.minute).padStart(2,'0') }}</div>
                <div style="margin-top:6px;">
                  <el-tag :type="job.isActive ? 'success' : 'info'" size="small">
                    {{ job.isActive ? '已启用' : '已停用' }}
                  </el-tag>
                  <el-tag v-if="job.lastRunStatus" size="small" style="margin-left:4px;">
                    上次: {{ job.lastRunStatus }}
                  </el-tag>
                </div>
              </div>
              <el-button text type="primary" size="small" @click="selectJob(job)">配置 ▶</el-button>
            </div>
            <div style="margin-top:8px;display:flex;gap:8px;border-top:1px solid #f0f0f0;padding-top:8px;">
              <el-button text size="small" @click="openEditJob(job)">编辑</el-button>
              <el-button text size="small" type="danger" @click="confirmDeleteJob(job)">删除</el-button>
            </div>
          </el-card>
        </el-col>
        <el-col v-if="jobs.length === 0" :span="24">
          <el-empty description="暂无调度任务，点击右上角新建" />
        </el-col>
      </el-row>
    </template>

    <!-- ===== 第二层：排期管理 ===== -->
    <template v-else>
      <el-card>
        <template #header>
          <div style="display:flex;justify-content:space-between;align-items:center;">
            <span>
              <el-button text @click="selectedJob = null" style="margin-right:8px;">◀ 返回任务列表</el-button>
              <strong>{{ getTemplateIcon(selectedJob.templateCode) }} {{ selectedJob.jobName }}</strong>
              <span style="font-size:12px;color:#909399;margin-left:8px;">{{ selectedJob.scheduleType === 'Daily' ? '每天' : '每月' }} {{ String(selectedJob.hour).padStart(2,'0') }}:{{ String(selectedJob.minute).padStart(2,'0') }}</span>
            </span>
            <span>
              <el-tag :type="selectedJob.isActive ? 'success' : 'info'" size="small">
                {{ selectedJob.isActive ? '已启用' : '已停用' }}
              </el-tag>
              <el-button text size="small" style="margin-left:8px;" @click="openEditJob(selectedJob)">编辑任务</el-button>
            </span>
          </div>
        </template>

        <div style="margin-bottom:12px;font-weight:500;">执行排期</div>
        <el-table :data="executions" stripe size="small" style="margin-bottom:12px;" v-loading="execLoading">
          <el-table-column prop="month" label="月份" width="100" />
          <el-table-column label="计划执行时间" width="200">
            <template #default="{ row }">
              <span>{{ formatDate(row.targetDate) }}</span>
              <el-tag v-if="row.isAdjusted" type="warning" size="small" style="margin-left:6px;">已调整</el-tag>
              <el-tag v-if="row.isCustom" type="info" size="small" style="margin-left:4px;">自定义</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="status" label="状态" width="90">
            <template #default="{ row }">
              <el-tag :color="statusColor(row.status)" style="color:#fff;border:0" size="small">{{ statusLabel(row.status) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="reason" label="原因" min-width="180" />
          <el-table-column label="操作" width="200" fixed="right">
            <template #default="{ row }">
              <el-button text type="primary" size="small" @click="openEditExec(row)">编辑</el-button>
              <el-button text type="danger" size="small" @click="confirmDeleteExec(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>

        <div style="display:flex;gap:8px;margin-bottom:16px;">
          <el-button size="small" :loading="genLoading" @click="handleGenerate">批量生成默认排期</el-button>
          <el-button size="small" type="primary" plain @click="openAddExec">+ 添加自定义排期</el-button>
        </div>
      </el-card>
    </template>

    <!-- ===== 新建任务向导 Step1：选择模板 ===== -->
    <el-dialog v-model="createWizardVisible" title="新建调度任务" width="550px" :close-on-click-modal="false">
      <div style="margin-bottom:12px;font-weight:500;">选择任务类型：</div>
      <el-row :gutter="12">
        <el-col v-for="tpl in templates" :key="tpl.code" :span="8" style="margin-bottom:12px;">
          <el-card :class="['template-card', { selected: createForm.templateCode === tpl.code }]"
            shadow="hover" :body-style="{ padding: '12px', cursor: 'pointer' }"
            @click="createForm.templateCode = tpl.code">
            <div style="font-size:24px;text-align:center;">{{ tpl.icon || '⚙️' }}</div>
            <div style="font-weight:600;text-align:center;margin-top:4px;">{{ tpl.displayName }}</div>
            <div style="font-size:11px;color:#909399;text-align:center;">{{ tpl.defaultScheduleType === 'Daily' ? '每天' : '每月' }} {{ String(tpl.defaultHour).padStart(2,'0') }}:{{ String(tpl.defaultMinute).padStart(2,'0') }}</div>
          </el-card>
        </el-col>
        <el-col :span="8" style="margin-bottom:12px;">
          <el-card :class="['template-card', { selected: createForm.templateCode === '__custom__' }]"
            shadow="hover" :body-style="{ padding: '12px', cursor: 'pointer' }"
            @click="createForm.templateCode = '__custom__'">
            <div style="font-size:24px;text-align:center;">⚙️</div>
            <div style="font-weight:600;text-align:center;margin-top:4px;">自定义任务</div>
            <div style="font-size:11px;color:#909399;text-align:center;">手动配置</div>
          </el-card>
        </el-col>
      </el-row>
      <template #footer>
        <el-button @click="createWizardVisible = false">取消</el-button>
        <el-button type="primary" @click="createWizardStep2">下一步：配置参数</el-button>
      </template>
    </el-dialog>

    <!-- ===== 新建/编辑任务 Step2：配置参数 ===== -->
    <el-dialog v-model="jobFormVisible" :title="isEditingJob ? '编辑调度任务' : '新建调度任务 — 配置参数'" width="500px">
      <el-form label-width="100px">
        <el-form-item label="模板" v-if="!isEditingJob && createForm.templateCode && createForm.templateCode !== '__custom__'">
          <span>{{ getTemplateName(createForm.templateCode) }}</span>
        </el-form-item>
        <el-form-item label="任务名称">
          <el-input v-model="jobForm.jobName" placeholder="输入任务名称" />
        </el-form-item>
        <el-form-item label="调度类型">
          <el-radio-group v-model="jobForm.scheduleType">
            <el-radio value="Daily">每天</el-radio>
            <el-radio value="Monthly">每月</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="执行时间">
          <el-time-picker v-model="jobForm.runTime" format="HH:mm" value-format="HH:mm"
            placeholder="选择执行时间" style="width:100%" />
        </el-form-item>
        <el-form-item label="是否启用">
          <el-switch v-model="jobForm.isActive" />
        </el-form-item>
        <el-form-item label="任务描述">
          <el-input v-model="jobForm.description" type="textarea" :rows="2" placeholder="可选" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="jobFormVisible = false">取消</el-button>
        <el-button type="primary" @click="saveJob">保存</el-button>
      </template>
    </el-dialog>

    <!-- ===== 调整排期 Drawer ===== -->
    <el-drawer v-model="execDrawerVisible" :title="isAddingExec ? '添加自定义排期' : '调整执行日期'" :size="450">
      <el-form label-width="110px">
        <el-form-item label="任务"><span>{{ selectedJob?.jobName }}</span></el-form-item>
        <el-form-item v-if="!isAddingExec" label="原计划">
          <span style="color:#909399;">{{ execForm.originalDate ? formatDate(execForm.originalDate) : '-' }}</span>
        </el-form-item>
        <el-form-item label="执行时间">
          <el-date-picker v-model="execForm.targetDate" type="datetime" value-format="YYYY-MM-DD HH:mm"
            placeholder="选择执行时间" style="width:100%" />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="execForm.status" style="width:100%">
            <el-option label="待执行" value="Pending" />
            <el-option label="执行中" value="Running" />
            <el-option label="执行成功" value="Success" />
            <el-option label="执行失败" value="Failed" />
            <el-option label="已跳过" value="Skipped" />
            <el-option label="已暂停" value="Paused" />
          </el-select>
        </el-form-item>
        <el-form-item label="原因">
          <el-input v-model="execForm.reason" type="textarea" :rows="2" placeholder="填写调整原因" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="execDrawerVisible = false">取消</el-button>
        <el-button type="primary" @click="saveExec">保存</el-button>
      </template>
    </el-drawer>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getSchedulerJobs, getSchedulerTemplates, createSchedulerJob,
  updateSchedulerJob, deleteSchedulerJob,
  getExecutions, createExecution, updateExecution, deleteExecution, generateExecutions
} from '../../api/index'

const jobs = ref([])
const templates = ref([])
const loading = ref(false)

async function fetchJobs() {
  loading.value = true
  try {
    const [jobRes, tplRes] = await Promise.all([
      getSchedulerJobs(),
      getSchedulerTemplates()
    ])
    jobs.value = Array.isArray(jobRes) ? jobRes : []
    templates.value = Array.isArray(tplRes) ? tplRes : []
  } catch (e) {
    jobs.value = []
    templates.value = []
  }
  loading.value = false
}

const JOB_ICONS = { MonthlyFeeBill: '📅', LateFeeCalc: '💰', AutoRenew: '🔄', Collection: '📢' }

function getTemplateIcon(code) { return JOB_ICONS[code] || '⚙️' }

function getTemplateName(code) {
  const tpl = templates.value.find(t => t.code === code)
  return tpl ? tpl.displayName : code
}

// ===== 新建任务向导 =====
const createWizardVisible = ref(false)
const createForm = reactive({ templateCode: '' })

function openCreateWizard() {
  createForm.templateCode = ''
  createWizardVisible.value = true
}

function createWizardStep2() {
  if (!createForm.templateCode) {
    ElMessage.warning('请选择一个任务类型')
    return
  }
  createWizardVisible.value = false
  isEditingJob.value = false
  jobForm.jobName = ''
  jobForm.scheduleType = 'Monthly'
  jobForm.runTime = '08:00'
  jobForm.isActive = true
  jobForm.description = ''
  jobForm.editingId = ''

  if (createForm.templateCode !== '__custom__') {
    const tpl = templates.value.find(t => t.code === createForm.templateCode)
    if (tpl) {
      jobForm.jobName = tpl.displayName
      jobForm.scheduleType = tpl.defaultScheduleType
      jobForm.runTime = `${String(tpl.defaultHour).padStart(2,'0')}:${String(tpl.defaultMinute).padStart(2,'0')}`
      jobForm.description = tpl.description || ''
    }
  }
  jobFormVisible.value = true
}

// ===== 编辑任务 =====
const isEditingJob = ref(false)
const jobFormVisible = ref(false)
const jobForm = reactive({
  jobName: '', scheduleType: 'Monthly', runTime: '08:00',
  isActive: true, description: '', editingId: ''
})

function openEditJob(job) {
  isEditingJob.value = true
  jobForm.editingId = job.id
  jobForm.jobName = job.jobName
  jobForm.scheduleType = job.scheduleType || 'Monthly'
  jobForm.runTime = `${String(job.hour).padStart(2,'0')}:${String(job.minute).padStart(2,'0')}`
  jobForm.isActive = job.isActive
  jobForm.description = job.description || ''
  jobFormVisible.value = true
}

async function saveJob() {
  if (!jobForm.jobName || !jobForm.runTime) {
    ElMessage.warning('请填写任务名称和执行时间')
    return
  }
  const duplicateName = jobs.value.find(j =>
    j.jobName === jobForm.jobName && j.id !== jobForm.editingId
  )
  if (duplicateName) {
    ElMessage.warning('已存在同名任务，请修改任务名称')
    return
  }
  const [hour, minute] = (jobForm.runTime || '08:00').split(':').map(Number)
  try {
    if (isEditingJob.value) {
      await updateSchedulerJob(jobForm.editingId, {
        jobName: jobForm.jobName,
        scheduleType: jobForm.scheduleType,
        hour, minute,
        isActive: jobForm.isActive,
        description: jobForm.description
      })
      ElMessage.success('任务已更新')
    } else {
      await createSchedulerJob({
        jobName: jobForm.jobName,
        scheduleType: jobForm.scheduleType,
        hour, minute,
        description: jobForm.description,
        templateCode: createForm.templateCode !== '__custom__' ? createForm.templateCode : null
      })
      ElMessage.success('任务已创建')
    }
    jobFormVisible.value = false
    await fetchJobs()
  } catch (e) {
    const msg = e?.response?.data?.message || e?.message || '操作失败'
    ElMessage.error(msg)
  }
}

async function confirmDeleteJob(job) {
  try {
    await ElMessageBox.confirm(`确定删除调度任务"${job.jobName}"？该任务的所有排期数据将被同时删除。`, '确认删除', { type: 'warning' })
    await deleteSchedulerJob(job.id)
    ElMessage.success('已删除')
    await fetchJobs()
  } catch (e) { /* cancelled */ }
}

// ===== 选中 Job =====
const selectedJob = ref(null)
const executions = ref([])
const execLoading = ref(false)

async function selectJob(job) {
  selectedJob.value = job
  await fetchExecutions()
}

async function fetchExecutions() {
  if (!selectedJob.value) return
  execLoading.value = true
  try {
    const res = await getExecutions(selectedJob.value.id, { months: 6 })
    executions.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e) {
    executions.value = []
  }
  execLoading.value = false
}

// ===== 排期 CRUD =====
const execDrawerVisible = ref(false)
const isAddingExec = ref(false)
const execForm = reactive({
  targetDate: '', status: 'Pending', reason: '', editingId: '', originalDate: null
})

function openEditExec(row) {
  isAddingExec.value = false
  execForm.editingId = row.id
  execForm.targetDate = row.targetDate
  execForm.status = row.status || 'Pending'
  execForm.reason = row.reason || ''
  execForm.originalDate = row.originalDate || null
  execDrawerVisible.value = true
}

function openAddExec() {
  isAddingExec.value = true
  execForm.editingId = ''
  execForm.targetDate = ''
  execForm.status = 'Pending'
  execForm.reason = ''
  execForm.originalDate = null
  execDrawerVisible.value = true
}

async function saveExec() {
  if (!execForm.reason) { ElMessage.warning('请填写原因'); return }
  if (!execForm.targetDate) { ElMessage.warning('请选择执行时间'); return }
  if (isAddingExec.value) {
    const dup = executions.value.find(e => e.targetDate === execForm.targetDate && e.reason === execForm.reason)
    if (dup) { ElMessage.warning('该排期已存在，请勿重复添加'); return }
  }
  try {
    const payload = { targetDate: execForm.targetDate, status: execForm.status, reason: execForm.reason }
    if (isAddingExec.value) {
      await createExecution(selectedJob.value.id, payload)
      ElMessage.success('自定义排期已添加')
    } else {
      await updateExecution(selectedJob.value.id, execForm.editingId, payload)
      ElMessage.success('排期已更新')
    }
    execDrawerVisible.value = false
    await fetchExecutions()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || e?.message || '操作失败')
  }
}

async function confirmDeleteExec(row) {
  try {
    await ElMessageBox.confirm('确定删除此排期？', '提示', { type: 'warning' })
    await deleteExecution(selectedJob.value.id, row.id)
    ElMessage.success('已删除')
    await fetchExecutions()
  } catch (e) { /* cancelled */ }
}

const genLoading = ref(false)
async function handleGenerate() {
  try {
    genLoading.value = true
    const res = await generateExecutions(selectedJob.value.id)
    ElMessage.success(`已生成 ${res.generated || 0} 条默认排期`)
    await fetchExecutions()
  } catch (e) {
    ElMessage.error('生成失败')
  } finally {
    genLoading.value = false
  }
}

function statusColor(s) {
  const colors = { Success: '#67c23a', Failed: '#f56c6c', Running: '#409eff', Pending: '#607d8b', Skipped: '#9e9e9e', Paused: '#009688' }
  return colors[s] || '#909399'
}

function statusLabel(s) {
  const map = { Pending: '待执行', Running: '执行中', Success: '执行成功', Failed: '执行失败', Skipped: '已跳过', Paused: '已暂停' }
  return map[s] || s
}

function formatDate(d) {
  if (!d) return ''
  const dt = new Date(d)
  const weekdays = ['日','一','二','三','四','五','六']
  return `${dt.getFullYear()}-${String(dt.getMonth()+1).padStart(2,'0')}-${String(dt.getDate()).padStart(2,'0')}(${weekdays[dt.getDay()]}) ${String(dt.getHours()).padStart(2,'0')}:${String(dt.getMinutes()).padStart(2,'0')}`
}

onMounted(() => fetchJobs())
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.template-card { border: 2px solid transparent; transition: all .2s; }
.template-card:hover { border-color: #409eff; }
.template-card.selected { border-color: #409eff; background: #ecf5ff; }
</style>
