<template>
  <div>
    <!-- ===== 任务列表 ===== -->
    <template v-if="!selectedJob">
      <div class="page-header">
        <h2>调度任务管理</h2>
        <div style="display:flex;gap:8px;align-items:center;">
          <el-popover placement="bottom" :width="280" trigger="hover">
            <template #reference>
              <el-button text size="small" style="font-size:12px;">📋 状态说明</el-button>
            </template>
            <div style="font-size:12px;line-height:2.1;">
              <div><el-tag :color="statusColor('Pending')" style="color:#fff;border:0;margin-right:6px;" size="small">待执行</el-tag> 未到执行时间或等待上游</div>
              <div><el-tag :color="statusColor('Processing')" style="color:#fff;border:0;margin-right:6px;" size="small">处理中</el-tag> 已被调度引擎抢占</div>
              <div><el-tag :color="statusColor('Running')" style="color:#fff;border:0;margin-right:6px;" size="small">执行中</el-tag> 任务正在执行（心跳30s）</div>
              <div><el-tag :color="statusColor('Completed')" style="color:#fff;border:0;margin-right:6px;" size="small">已完成</el-tag> 执行成功</div>
              <div><el-tag :color="statusColor('Failed')" style="color:#fff;border:0;margin-right:6px;" size="small">失败</el-tag> 执行失败，阻断下游任务</div>
              <div><el-tag :color="statusColor('Skipped')" style="color:#fff;border:0;margin-right:6px;" size="small">已跳过</el-tag> 被上游阻断或手动跳过</div>
              <div><el-tag :color="statusColor('Paused')" style="color:#fff;border:0;margin-right:6px;" size="small">已暂停</el-tag> 手动暂停，调度引擎忽略</div>
              <div><el-tag :color="statusColor('Cancelled')" style="color:#fff;border:0;margin-right:6px;" size="small">已取消</el-tag> 手动取消（终态）</div>
              <div style="border-top:1px solid #eee;margin:6px 0 4px;padding-top:4px;"></div>
              <div><el-tag :color="statusColor('Stale')" style="color:#fff;border:0;margin-right:6px;" size="small">僵死</el-tag> 进程崩溃，等待恢复</div>
              <div><el-tag :color="statusColor('Reversed')" style="color:#fff;border:0;margin-right:6px;" size="small">已撤销</el-tag> 被管理员反转</div>
            </div>
          </el-popover>
        </div>
      </div>
      <el-row :gutter="16">
        <el-col v-for="job in jobs" :key="job.id" :span="8" style="margin-bottom:16px;">
          <el-card shadow="hover" :body-style="{ padding:'14px' }">
            <div style="display:flex;justify-content:space-between;">
              <div style="flex:1;">
                <div style="font-weight:600;font-size:15px;">
                  {{ getTemplateIcon(job.templateCode) }} {{ getJobDisplayName(job) }}
                  <el-tag :type="job.isActive?'success':'info'" size="small" effect="dark" round style="margin-left:6px;">{{ job.isActive?'已启用':'已停用' }}</el-tag>
                </div>
                <div style="font-size:12px;color:#909399;margin-top:2px;">{{ job.description||'-' }}</div>
                <div style="margin-top:6px;display:flex;gap:4px;flex-wrap:wrap;">
                  <el-tag size="small" effect="plain">
                    <span v-if="job.scheduleType==='Monthly'">📅 每月{{ job.dayOfMonth||1 }}日</span>
                    <span v-else>📅 每日</span>
                    {{ String(job.hour).padStart(2,'0') }}:{{ String(job.minute).padStart(2,'0') }}
                  </el-tag>
                </div>
                <div style="margin-top:6px;font-size:12px;color:#606266;line-height:2;">
                  <div><span style="color:#909399;display:inline-block;width:64px;">⏱ 上次:</span>
                    <template v-if="job._lastRun">
                      <el-tag :color="statusColor(job.lastRunStatus)" style="color:#fff;border:0;margin-right:4px;" size="small">{{ statusLabel(job.lastRunStatus) }}</el-tag>
                      <span>{{ job._lastRun }}</span>
                    </template>
                    <span v-else style="color:#c0c4cc;">暂无执行记录</span>
                  </div>
                  <div><span style="color:#909399;display:inline-block;width:64px;">📌 下次:</span>
                    <template v-if="job._nextRun">
                      <el-tag :color="statusColor('Pending')" style="color:#fff;border:0;margin-right:4px;" size="small">待执行</el-tag>
                      <span style="color:#409eff;">{{ job._nextRun }}</span>
                    </template>
                    <template v-else-if="job._pendingCount">
                      <el-tag :color="statusColor('Pending')" style="color:#fff;border:0;margin-right:4px;" size="small">{{ job._pendingCount }}条待执行</el-tag>
                    </template>
                    <span v-else-if="!job.isActive" style="color:#c0c4cc;">任务已停用</span>
                    <span v-else style="color:#c0c4cc;">暂无排期，请先生成</span>
                  </div>
                </div>
              </div>
              <el-button text type="primary" size="small" @click="selectJob(job)">详情 ▶</el-button>
            </div>
          </el-card>
        </el-col>
        <el-col v-if="jobs.length===0" :span="24"><el-empty description="暂无调度任务" /></el-col>
      </el-row>
    </template>

    <!-- ===== 任务详情 ===== -->
    <template v-else>
      <el-card>
        <template #header>
          <div style="display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:8px;">
            <span>
              <el-button text @click="selectedJob=null">◀ 返回</el-button>
              <strong>{{ getTemplateIcon(selectedJob.templateCode) }} {{ getJobDisplayName(selectedJob) }}</strong>
              <el-tag size="small" effect="plain" style="margin-left:6px;">
                {{ selectedJob.scheduleType==='Daily'?'每日':`每月${selectedJob.dayOfMonth||1}日` }} {{ String(selectedJob.hour).padStart(2,'0') }}:{{ String(selectedJob.minute).padStart(2,'0') }}
              </el-tag>
            </span>
            <span style="display:flex;gap:6px;flex-wrap:wrap;">
              <el-button size="small" v-permission="'system:schedulerexecute'" @click="handleExecute(selectedJob)">▶ 手动执行</el-button>
              <el-tag :type="selectedJob.isActive?'success':'info'" size="small" effect="dark" round>{{ selectedJob.isActive?'已启用':'已停用' }}</el-tag>
              <el-button size="small" v-permission="'system:scheduleredit'" @click="openEditJob(selectedJob)">✏️ 编辑</el-button>
              <el-button size="small" type="danger" v-permission="'system:schedulerdelete'" @click="confirmDeleteJob(selectedJob)">🗑 删除</el-button>
            </span>
          </div>
          <div style="margin-top:8px;display:flex;gap:24px;font-size:13px;color:#606266;background:#f5f7fa;padding:8px 12px;border-radius:6px;">
            <span>📄 {{ selectedJob.description||'无描述' }}</span>
            <span>⏱ {{ selectedJob.scheduleType==='Daily'?'每天':`每月${selectedJob.dayOfMonth||1}日` }} {{ String(selectedJob.hour).padStart(2,'0') }}:{{ String(selectedJob.minute).padStart(2,'0') }}</span>
            <el-tag :type="selectedJob.isActive?'success':'info'" size="small" effect="dark" round>{{ selectedJob.isActive?'已启用':'已停用' }}</el-tag>
            <el-button text type="primary" size="small" @click="$router.push('/system/scheduler/monitor')">📊 查看执行监控 →</el-button>
          </div>
        </template>

        <el-tabs v-model="activeTab">
          <el-tab-pane label="执行排期" name="schedule">
            <div style="display:flex;gap:8px;margin-bottom:10px;align-items:center;">
              <el-button size="small" :loading="genLoading" v-permission="'system:schedulergenerate'" @click="handleGenerate">生成默认排期</el-button>
              <el-button size="small" type="danger" :disabled="selectedExecs.length===0" v-permission="'system:schedulerexecbatchdelete'" @click="handleBatchDelete">批量删除({{ selectedExecs.length }})</el-button>
              <el-button size="small" type="primary" plain v-permission="'system:scheduleradd'" @click="openAddExec">+ 自定义排期</el-button>
            </div>
            <el-table :data="executions" stripe size="small" v-loading="execLoading" @selection-change="onSelectionChange">
              <el-table-column type="selection" width="40" />
              <el-table-column prop="month" label="月份" width="90" />
              <el-table-column label="计划时间" width="200"><template #default="{row}">{{ chinaTime.formatDate(row.targetDate) }}<el-tag v-if="row.isAdjusted" type="warning" size="small" style="margin-left:4px;">调整</el-tag><el-tag v-if="row.isCustom" type="info" size="small" style="margin-left:4px;">自定义</el-tag></template></el-table-column>
              <el-table-column label="状态" width="80"><template #default="{row}"><el-tag :color="statusColor(row.status)" style="color:#fff;border:0" size="small">{{ statusLabel(row.status) }}</el-tag></template></el-table-column>
              <el-table-column prop="reason" label="说明" min-width="160" />
              <el-table-column label="操作" width="220" fixed="right"><template #default="{row}">
                <!-- Failed: 只能重试，不可跳过（闭合依赖链） -->
                <el-button v-if="row.status==='Failed'" text type="primary" size="small" @click="handleRetryExec(row)">重试</el-button>
                <!-- Pending: 可暂停或取消 -->
                <el-button v-if="row.status==='Pending'" text size="small" @click="handlePauseExec(row)">暂停</el-button>
                <el-button v-if="row.status==='Pending'" text type="danger" size="small" @click="handleCancelExec(row)">取消</el-button>
                <!-- Skipped/Paused: 可恢复 -->
                <el-button v-if="row.status==='Skipped'||row.status==='Paused'" text type="primary" size="small" @click="handleResumeExec(row)">恢复</el-button>
                <!-- Completed/Processing: 无操作 -->
                <el-button v-if="row.status==='Processing'||row.status==='Running'" text disabled size="small">执行中</el-button>
                <!-- 通用 -->
                <el-button text size="small" v-permission="'system:schedulerexcedit'" @click="openEditExec(row)">编辑</el-button>
                <el-button v-if="row.status!=='Processing'&&row.status!=='Running'" text type="danger" size="small" v-permission="'system:schedulerexecdelete'" @click="confirmDeleteExec(row)">删除</el-button>
              </template></el-table-column>
            </el-table>
          </el-tab-pane>
        </el-tabs>
      </el-card>
    </template>

    <!-- 新建/编辑任务 -->
    <el-dialog :draggable="true" v-model="jobFormVisible" :title="isEditingJob?'编辑任务':'新建任务'" width="500px">
      <el-form label-width="100px">
        <el-form-item label="任务名称"><el-input v-model="jobForm.jobName" /></el-form-item>
        <el-form-item label="调度类型"><el-radio-group v-model="jobForm.scheduleType"><el-radio value="Daily">每天</el-radio><el-radio value="Monthly">每月</el-radio></el-radio-group></el-form-item>
        <el-form-item label="执行时间"><el-time-picker v-model="jobForm.runTime" format="HH:mm" value-format="HH:mm" style="width:100%" /></el-form-item>
        <el-form-item label="启用"><el-switch v-model="jobForm.isActive" /></el-form-item>
        <el-form-item label="描述"><el-input v-model="jobForm.description" type="textarea" :rows="2" /></el-form-item>
      </el-form>
      <template #footer><el-button @click="jobFormVisible=false">取消</el-button><el-button type="primary" @click="saveJob">保存</el-button></template>
    </el-dialog>

    <!-- 排期编辑 -->
    <el-drawer v-model="execDrawerVisible" :title="isAddingExec?'添加排期':'调整排期'" :size="400">
      <el-form label-width="100px">
        <el-form-item label="执行时间"><el-date-picker v-model="execForm.targetDate" type="datetime" value-format="YYYY-MM-DDTHH:mm:ss" style="width:100%" /></el-form-item>
        <el-form-item label="状态"><el-select v-model="execForm.status" style="width:100%"><el-option label="待执行" value="Pending" /><el-option label="成功" value="Success" /><el-option label="失败" value="Failed" /></el-select></el-form-item>
        <el-form-item label="原因"><el-input v-model="execForm.reason" type="textarea" :rows="2" /></el-form-item>
      </el-form>
      <template #footer><el-button @click="execDrawerVisible=false">取消</el-button><el-button type="primary" @click="saveExec">保存</el-button></template>
    </el-drawer>

    <el-dialog :draggable="true" v-model="execConfirmVisible" title="确认手动执行" width="500px">
      <div v-if="execJob" style="padding:4px 0;">
        <el-descriptions :column="2" border size="small">
          <el-descriptions-item label="任务名称" :span="2"><strong>{{ execJob.jobName }}</strong></el-descriptions-item>
          <el-descriptions-item label="调度类型">{{ execJob.scheduleType==='Daily'?'每日':`每月${execJob.dayOfMonth||1}日` }}</el-descriptions-item>
          <el-descriptions-item label="执行时间">{{ String(execJob.hour).padStart(2,'0') }}:{{ String(execJob.minute).padStart(2,'0') }}</el-descriptions-item>
          <el-descriptions-item label="描述" :span="2">{{ execJob.description||'-' }}</el-descriptions-item>
          <el-descriptions-item label="上次执行">{{ lastRunText }}</el-descriptions-item>
          <el-descriptions-item label="上次状态"><el-tag v-if="execJob.lastRunStatus" size="small" :type="execJob.lastRunStatus==='Success'?'success':'danger'">{{ execJob.lastRunStatus }}</el-tag><span v-else>-</span></el-descriptions-item>
        </el-descriptions>
        <el-alert type="warning" :closable="false" style="margin-top:12px;">
          <template #title>执行后将立即触发该任务，确认继续？</template>
        </el-alert>
      </div>
      <template #footer>
        <el-button @click="execConfirmVisible=false">取消</el-button>
        <el-button type="primary" :loading="execRunning" @click="doExecute">确认执行</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { chinaTime } from '@/utils/chinaTime'
import { getSchedulerJobs, getSchedulerTemplates, createSchedulerJob, updateSchedulerJob, deleteSchedulerJob,
  getExecutions, createExecution, updateExecution, deleteExecution, generateExecutions,
  executeJob, deleteFutureExecutions, retryExecution,
  skipExecution, pauseExecution, cancelExecution, resumeExecution } from '../../../api/index'

const jobs = ref([]); const templates = ref([])
const selectedJob = ref(null); const activeTab = ref('schedule')
const executions = ref([]); const execLoading = ref(false)

const JOB_ICONS = { BillJob:'📅', SettleJob:'💰', AutoRenewJob:'🔄', CollectionJob:'📢', RenewalReminderJob:'🔔' }
function getTemplateIcon(c) { return JOB_ICONS[c]||'⚙️' }

// 任务中文显示名
const JOB_DISPLAY = { BillJob:'月度应收生成(BillJob)', SettleJob:'月度结算(SettleJob)', AutoRenewJob:'自动续签(AutoRenewJob)', CollectionJob:'催缴任务(CollectionJob)', RenewalReminderJob:'续签提醒(RenewalReminderJob)' }
function getJobDisplayName(job) { return JOB_DISPLAY[job.jobName] || job.jobName }

async function fetchJobs() {
  try {
    const [j,t]=await Promise.all([getSchedulerJobs(),getSchedulerTemplates()])
    jobs.value=Array.isArray(j)?j:[]; templates.value=Array.isArray(t)?t:[]
    // 加载每个任务的上次/下次执行时间
    for (const job of jobs.value) {
      try {
        const execs = await getExecutions(job.id, { months: 6 })
        const list = Array.isArray(execs) ? execs : (execs.items || [])
        const pending = list.filter(e => e.status === 'Pending')
        // 下次执行（最近的 Pending）
        const next = pending.sort((a,b) => new Date(a.targetDate)-new Date(b.targetDate))[0]
        job._nextRun = next ? chinaTime.formatDate(next.targetDate) : null
        job._pendingCount = pending.length
        // 上次执行（最近的 Completed/Failed/Processing）
        const done = list.filter(e => e.status !== 'Pending')
        const last = done.sort((a,b) => new Date(b.targetDate)-new Date(a.targetDate))[0]
        job._lastRun = last ? chinaTime.formatDate(last.targetDate) : null
        if (last) job.lastRunStatus = last.status
      } catch { job._nextRun = null; job._lastRun = null }
    }
  } catch { jobs.value = [] }
}
async function selectJob(job) {
  selectedJob.value=job; activeTab.value='schedule'
  await fetchExecutions()
}
async function fetchExecutions() {
  if(!selectedJob.value) return; execLoading.value=true
  try { const r=await getExecutions(selectedJob.value.id,{months:6}); executions.value=Array.isArray(r)?r:(r.items||[]) }
  catch { executions.value=[] }
  execLoading.value=false
}

const execConfirmVisible = ref(false)
const execJob = ref(null)
const execRunning = ref(false)
const lastRunText = ref('')

function handleExecute(job) {
  execJob.value = job
  lastRunText.value = job.lastRunAt ? chinaTime.formatDate(job.lastRunAt) : '从未执行'
  execConfirmVisible.value = true
}

async function doExecute() {
  if (!execJob.value) return
  execRunning.value = true
  try {
    const r = await executeJob(execJob.value.jobName, { mode:'execute', companyId:'00000000-0000-0000-0000-000000000000', targetMonth: chinaTime.currentMonth() })
    ElMessage.success(r?.result||'任务已触发')
    execConfirmVisible.value = false
  } catch {
    ElMessage.error('执行失败')
  } finally {
    execRunning.value = false
  }
}

const createWizardVisible=ref(false); const createForm=reactive({templateCode:''})
function openCreateWizard() { createForm.templateCode=''; createWizardVisible.value=true }
function createWizardStep2() {
  if(!createForm.templateCode){ElMessage.warning('请选择任务类型');return}
  createWizardVisible.value=false; isEditingJob.value=false
  jobForm.jobName=''; jobForm.scheduleType='Monthly'; jobForm.runTime='08:00'; jobForm.isActive=true; jobForm.description=''; jobForm.editingId=''
  if(createForm.templateCode!=='__custom__'){const t=templates.value.find(t=>t.code===createForm.templateCode);if(t){jobForm.jobName=t.displayName;jobForm.scheduleType=t.defaultScheduleType;jobForm.runTime=`${String(t.defaultHour).padStart(2,'0')}:${String(t.defaultMinute).padStart(2,'0')}`;jobForm.description=t.description||''}}
  jobFormVisible.value=true
}

const isEditingJob=ref(false); const jobFormVisible=ref(false)
const jobForm=reactive({jobName:'',scheduleType:'Monthly',runTime:'08:00',isActive:true,description:'',editingId:''})
function openEditJob(job) {
  isEditingJob.value=true; jobForm.editingId=job.id; jobForm.jobName=job.jobName; jobForm.scheduleType=job.scheduleType||'Monthly'
  jobForm.runTime=`${String(job.hour).padStart(2,'0')}:${String(job.minute).padStart(2,'0')}`; jobForm.isActive=job.isActive; jobForm.description=job.description||''; jobFormVisible.value=true
}
async function saveJob() {
  if(!jobForm.jobName||!jobForm.runTime){ElMessage.warning('请填写完整');return}
  if(jobs.value.find(j=>j.jobName===jobForm.jobName&&j.id!==jobForm.editingId)){ElMessage.warning('已存在同名任务');return}
  const [h,m]=(jobForm.runTime||'08:00').split(':').map(Number)
  try {
    if(isEditingJob.value){await updateSchedulerJob(jobForm.editingId,{jobName:jobForm.jobName,scheduleType:jobForm.scheduleType,hour:h,minute:m,isActive:jobForm.isActive,description:jobForm.description})}
    else{await createSchedulerJob({jobName:jobForm.jobName,scheduleType:jobForm.scheduleType,hour:h,minute:m,description:jobForm.description,templateCode:createForm.templateCode!=='__custom__'?createForm.templateCode:null})}
    ElMessage.success(isEditingJob.value?'已更新':'已创建'); jobFormVisible.value=false; await fetchJobs()
  }catch(e){ElMessage.error(e?.response?.data?.message||'操作失败')}
}
async function confirmDeleteJob(job) {
  try{await ElMessageBox.confirm(`确定删除"${job.jobName}"？`,'确认',{type:'warning'});await deleteSchedulerJob(job.id);ElMessage.success('已删除');await fetchJobs()}catch{}
}

const execDrawerVisible=ref(false); const isAddingExec=ref(false); const genLoading=ref(false)
const selectedExecs=ref([])
function onSelectionChange(rows) { selectedExecs.value = rows }
async function handleBatchDelete() {
  if (selectedExecs.value.length === 0) return
  try {
    await ElMessageBox.confirm(`确定删除选中的 ${selectedExecs.value.length} 条排期？`,'确认',{type:'warning'})
    for (const row of selectedExecs.value) {
      await deleteExecution(selectedJob.value.id, row.id)
    }
    ElMessage.success(`已删除 ${selectedExecs.value.length} 条`)
    selectedExecs.value = []
    const remaining = await fetchExecutions()
    if (remaining === 0) {
      ElMessageBox.confirm('执行排期已全部删除，是否重新生成？','提示',{confirmButtonText:'生成',cancelButtonText:'不生成',type:'info'})
        .then(() => handleGenerate()).catch(() => {})
    }
  } catch {}
}
const execForm=reactive({targetDate:'',status:'Pending',reason:'',editingId:''})
function openEditExec(r){isAddingExec.value=false;execForm.editingId=r.id;execForm.targetDate=r.targetDate;execForm.status=r.status||'Pending';execForm.reason=r.reason||'';execDrawerVisible.value=true}
function openAddExec(){isAddingExec.value=true;execForm.editingId='';execForm.targetDate='';execForm.status='Pending';execForm.reason='';execDrawerVisible.value=true}
async function saveExec(){
  if(!execForm.reason||!execForm.targetDate){ElMessage.warning('请填写完整');return}
  try{const p={targetDate:execForm.targetDate,status:execForm.status,reason:execForm.reason}
    if(isAddingExec.value){await createExecution(selectedJob.value.id,p)}else{await updateExecution(selectedJob.value.id,execForm.editingId,p)}
    ElMessage.success(isAddingExec.value?'已添加':'已更新');execDrawerVisible.value=false;await fetchExecutions()
  }catch{ElMessage.error('操作失败')}
}
async function handleRetryExec(r){
  try {
    await ElMessageBox.confirm(`确认重试此排期？将立即执行，成功后自动触发下游任务。`,'确认重试',{type:'info'})
    const res = await retryExecution(selectedJob.value.id, r.id)
    if (res.status === 'Completed') {
      ElMessage.success(`✅ 重试成功：${res.message}`)
    } else {
      ElMessage.error(`❌ 重试失败：${res.error}`)
    }
    await fetchExecutions()
    // 刷新任务卡片状态
    await fetchJobs()
  } catch {}
}
async function confirmDeleteExec(r){
  try {
    await ElMessageBox.confirm('确定删除此排期？','提示',{type:'warning'})
    await deleteExecution(selectedJob.value.id, r.id)
    ElMessage.success('已删除')
    const remaining = await fetchExecutions()
    // 删除后如果列表为空，询问是否自动生成
    if (remaining === 0) {
      ElMessageBox.confirm('执行排期已全部删除，是否重新生成默认排期？','提示',{confirmButtonText:'生成',cancelButtonText:'不生成',type:'info'})
        .then(() => handleGenerate())
        .catch(() => {})
    }
  } catch {}
}
async function handleSkipExec(r){
  try {
    await ElMessageBox.confirm(`确认跳过此排期？跳过后的任务将不再执行。`,'确认跳过',{type:'warning'})
    await skipExecution(selectedJob.value.id, r.id, { reason: '手动跳过' })
    ElMessage.success('已跳过')
    await fetchExecutions(); await fetchJobs()
  } catch {}
}
async function handlePauseExec(r){
  try {
    await ElMessageBox.confirm(`确认暂停此排期？暂停后调度引擎将忽略此排期。`,'确认暂停',{type:'info'})
    await pauseExecution(selectedJob.value.id, r.id, { reason: '手动暂停' })
    ElMessage.success('已暂停')
    await fetchExecutions(); await fetchJobs()
  } catch {}
}
async function handleCancelExec(r){
  try {
    await ElMessageBox.confirm(`确认取消此排期？取消后不可恢复。`,'确认取消',{type:'warning',confirmButtonText:'确认取消',confirmButtonClass:'el-button--danger'})
    await cancelExecution(selectedJob.value.id, r.id, { reason: '手动取消' })
    ElMessage.success('已取消')
    await fetchExecutions(); await fetchJobs()
  } catch {}
}
async function handleResumeExec(r){
  try {
    await resumeExecution(selectedJob.value.id, r.id, { reason: '手动恢复' })
    ElMessage.success('已恢复为待执行')
    await fetchExecutions(); await fetchJobs()
  } catch {}
}
async function handleGenerate(){
  try{genLoading.value=true;const r=await generateExecutions(selectedJob.value.id);ElMessage.success(`生成 ${r.generated||0} 条`);await fetchExecutions()}
  catch{ElMessage.error('生成失败')}finally{genLoading.value=false}
}

function statusColor(s){return {Completed:'#67c23a',Success:'#67c23a',Failed:'#f56c6c',Processing:'#409eff',Running:'#409eff',Pending:'#909399',Skipped:'#e6a23c',Paused:'#d4a017',Cancelled:'#b0b0b0',Stale:'#e6a23c',Reversed:'#9e9e9e'}[s]||'#909399'}
function statusLabel(s){return {Pending:'待执行',Processing:'处理中',Running:'执行中',Completed:'已完成',Success:'成功',Failed:'失败',Skipped:'已跳过',Paused:'已暂停',Cancelled:'已取消',Stale:'僵死',Reversed:'已撤销'}[s]||s}

onMounted(fetchJobs)
</script>
<style scoped>
.page-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:16px;}
</style>
