<template>
  <div>
    <div class="page-header">
      <h2>催缴配置</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-card style="margin-bottom: 16px;">
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span>催缴阶段定义</span>
          <el-button size="small" type="primary" @click="showAddStage = true">新增阶段</el-button>
        </div>
      </template>
      <el-table :data="stages" v-loading="stagesLoading" stripe>
        <el-table-column prop="stageNo" label="阶段序号" width="80" />
        <el-table-column prop="stageName" label="阶段名称" />
        <el-table-column label="逾期天数范围">
          <template #default="{ row }">{{ row.overdueDaysFrom }} ~ {{ row.overdueDaysTo }} 天</template>
        </el-table-column>
        <el-table-column prop="actionType" label="动作类型" width="100">
          <template #default="{ row }">{{ actionTypeLabel(row.actionType) }}</template>
        </el-table-column>
        <el-table-column label="自动执行" width="80">
          <template #default="{ row }">
            <el-switch :model-value="row.isAuto" @change="toggleAuto(row)" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button text size="small" type="danger" @click="deleteStage(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Add Stage Dialog -->
    <el-dialog :draggable="true" v-model="showAddStage" title="新增催缴阶段" width="450px">
      <el-form :model="newStage" label-width="110px">
        <el-form-item label="阶段序号">
          <el-input-number v-model="newStage.stageNo" :min="1" style="width:100%;" />
        </el-form-item>
        <el-form-item label="阶段名称">
          <el-input v-model="newStage.stageName" />
        </el-form-item>
        <el-form-item label="逾期起始天数">
          <el-input-number v-model="newStage.overdueDaysFrom" :min="0" style="width:100%;" />
        </el-form-item>
        <el-form-item label="逾期截止天数">
          <el-input-number v-model="newStage.overdueDaysTo" :min="0" style="width:100%;" />
        </el-form-item>
        <el-form-item label="动作类型">
          <el-select v-model="newStage.actionType" style="width:100%;">
            <el-option label="短信提醒" value="SMS" />
            <el-option label="电话催缴" value="CALL" />
            <el-option label="上门催缴" value="VISIT" />
            <el-option label="律师函" value="LEGAL" />
          </el-select>
        </el-form-item>
        <el-form-item label="自动执行">
          <el-switch v-model="newStage.isAuto" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddStage = false">取消</el-button>
        <el-button type="primary" :loading="stageSaving" @click="addStage">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '@/store/user'
import {
  getCollectionStages, createCollectionStage, updateCollectionStage, deleteCollectionStage,
} from '@/api'

const userStore = useUserStore()
const stages = ref([])
const stagesLoading = ref(false)
const stageSaving = ref(false)
const showAddStage = ref(false)

const newStage = reactive({
  stageNo: 1, stageName: '', overdueDaysFrom: 1, overdueDaysTo: 7,
  actionType: 'SMS', isAuto: true
})

const actionTypeLabels = { SMS: '短信', CALL: '电话', VISIT: '上门', LEGAL: '律师函', Email: '邮件' }
function actionTypeLabel(t) { return actionTypeLabels[t] || t }

async function loadStages() {
  stagesLoading.value = true
  try {
    const list = await getCollectionStages()
    stages.value = Array.isArray(list)
      ? list.map(s => ({
          id: s.id, stageNo: s.stageNo, stageName: s.stageName,
          overdueDaysFrom: s.overdueDaysFrom, overdueDaysTo: s.overdueDaysTo,
          actionType: s.actionType, isAuto: s.isAuto !== false, companyId: s.companyId
        }))
      : []
  } catch { ElMessage.error('加载催缴阶段失败') }
  finally { stagesLoading.value = false }
}

async function addStage() {
  if (!newStage.stageName) { ElMessage.warning('请输入阶段名称'); return }
  if (newStage.overdueDaysFrom > newStage.overdueDaysTo) { ElMessage.warning('起始天数不能大于截止天数'); return }
  const companyId = userStore.effectiveCompanyId
  if (!companyId) { ElMessage.warning('请先选择公司'); return }

  stageSaving.value = true
  try {
    await createCollectionStage({
      stageNo: newStage.stageNo,
      stageName: newStage.stageName,
      overdueDaysFrom: newStage.overdueDaysFrom,
      overdueDaysTo: newStage.overdueDaysTo,
      actionType: newStage.actionType,
      isAuto: newStage.isAuto,
      companyId
    })
    ElMessage.success('阶段已创建')
    showAddStage.value = false
    newStage.stageNo = stages.value.length + 1
    newStage.stageName = ''; newStage.overdueDaysFrom = 1; newStage.overdueDaysTo = 7
    newStage.actionType = 'SMS'; newStage.isAuto = true
    await loadStages()
  } catch { ElMessage.error('创建失败') }
  finally { stageSaving.value = false }
}

async function toggleAuto(row) {
  try {
    await updateCollectionStage(row.id, {
      id: row.id, stageNo: row.stageNo, stageName: row.stageName,
      overdueDaysFrom: row.overdueDaysFrom, overdueDaysTo: row.overdueDaysTo,
      actionType: row.actionType, isAuto: !row.isAuto, companyId: row.companyId
    })
    row.isAuto = !row.isAuto
  } catch { ElMessage.error('更新失败') }
}

async function deleteStage(row) {
  try {
    await ElMessageBox.confirm(`确定删除催缴阶段「${row.stageName}」吗？`, '提示', { type: 'warning' })
    await deleteCollectionStage(row.id)
    ElMessage.success('已删除')
    await loadStages()
  } catch (e) { if (e !== 'cancel') ElMessage.error('删除失败') }
}

onMounted(loadStages)
</script>
<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
</style>
