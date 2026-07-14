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
        <el-table-column type="index" label="阶段" width="60" />
        <el-table-column prop="name" label="阶段名称" />
        <el-table-column label="逾期天数阈值">
          <template #default="{ row }">{{ row.daysOverdue }} 天</template>
        </el-table-column>
        <el-table-column prop="isActive" label="启用">
          <template #default="{ row }">
            <el-switch v-model="row.isActive" @change="toggleStage(row)" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button text size="small" type="danger" @click="deleteStage(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card>
      <template #header>滞纳金配置</template>
      <el-form :model="lateFeeConfig" label-width="140px" style="max-width: 500px;">
        <el-form-item label="日利率">
          <el-input-number v-model="lateFeeConfig.dailyRate" :precision="4" :step="0.0001" :min="0" style="width: 180px;" />
          <span style="margin-left: 8px;">%</span>
        </el-form-item>
        <el-form-item label="宽限期（天）">
          <el-input-number v-model="lateFeeConfig.graceDays" :min="0" style="width: 180px;" />
        </el-form-item>
        <el-form-item label="滞纳金上限">
          <el-input-number v-model="lateFeeConfig.maxRate" :precision="2" :min="0" style="width: 180px;" />
          <span style="margin-left: 8px;">% 本金</span>
        </el-form-item>
        <el-form-item label="最低滞纳金">
          <el-input-number v-model="lateFeeConfig.minAmount" :precision="2" :min="0" style="width: 180px;" />
          <span style="margin-left: 8px;">元</span>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="configSaving" @click="saveConfig">保存配置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- Add Stage Dialog -->
    <el-dialog v-model="showAddStage" title="新增催缴阶段" width="400px">
      <el-form :model="newStage" label-width="100px">
        <el-form-item label="阶段名称">
          <el-input v-model="newStage.name" />
        </el-form-item>
        <el-form-item label="逾期天数">
          <el-input-number v-model="newStage.daysOverdue" :min="0" style="width:100%;" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="newStage.sortOrder" :min="0" style="width:100%;" />
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
  getActiveLateFeeConfig, saveLateFeeConfig
} from '@/api'

const userStore = useUserStore()
const stages = ref([])
const stagesLoading = ref(false)
const configSaving = ref(false)
const stageSaving = ref(false)
const showAddStage = ref(false)

const lateFeeConfig = reactive({
  dailyRate: 0.05, graceDays: 3, maxRate: 100, minAmount: 1, effectiveDate: ''
})

const newStage = reactive({
  name: '', daysOverdue: 1, sortOrder: 0
})

async function loadStages() {
  stagesLoading.value = true
  try {
    const list = await getCollectionStages()
    stages.value = Array.isArray(list)
      ? list.map(s => ({ id: s.id, name: s.name, daysOverdue: s.daysOverdue, isActive: s.isActive !== false, sortOrder: s.sortOrder || 0 }))
      : []
  } catch { ElMessage.error('加载催缴阶段失败') }
  finally { stagesLoading.value = false }
}

async function loadLateFeeConfig() {
  try {
    const cfg = await getActiveLateFeeConfig()
    if (cfg && cfg.id) {
      lateFeeConfig.dailyRate = (cfg.dailyRate || 0) * 100  // 转为百分比显示
      lateFeeConfig.graceDays = cfg.graceDays || 0
      lateFeeConfig.maxRate = cfg.maxRate || 100
      lateFeeConfig.minAmount = cfg.minAmount || 1
      lateFeeConfig.effectiveDate = cfg.effectiveDate || ''
    }
  } catch { /* 静默 */ }
}

async function addStage() {
  if (!newStage.name) { ElMessage.warning('请输入阶段名称'); return }
  const companyId = userStore.effectiveCompanyId || userStore.companyId
  if (!companyId) { ElMessage.warning('请先选择公司'); return }

  stageSaving.value = true
  try {
    await createCollectionStage({
      name: newStage.name,
      daysOverdue: newStage.daysOverdue,
      sortOrder: newStage.sortOrder,
      isActive: true,
      companyId
    })
    ElMessage.success('阶段已创建')
    showAddStage.value = false
    newStage.name = ''; newStage.daysOverdue = 1; newStage.sortOrder = 0
    await loadStages()
  } catch { ElMessage.error('创建失败') }
  finally { stageSaving.value = false }
}

async function toggleStage(row) {
  try {
    await updateCollectionStage(row.id, {
      id: row.id,
      name: row.name,
      daysOverdue: row.daysOverdue,
      isActive: row.isActive,
      sortOrder: row.sortOrder || 0,
      companyId: userStore.effectiveCompanyId || userStore.companyId
    })
  } catch { ElMessage.error('更新失败'); row.isActive = !row.isActive }
}

async function deleteStage(row) {
  try {
    await ElMessageBox.confirm(`确定删除催缴阶段「${row.name}」吗？`, '提示', { type: 'warning' })
    await deleteCollectionStage(row.id)
    ElMessage.success('已删除')
    await loadStages()
  } catch (e) { if (e !== 'cancel') ElMessage.error('删除失败') }
}

async function saveConfig() {
  configSaving.value = true
  try {
    await saveLateFeeConfig({
      dailyRate: lateFeeConfig.dailyRate / 100,  // 转为小数
      graceDays: lateFeeConfig.graceDays,
      maxRate: lateFeeConfig.maxRate,
      minAmount: lateFeeConfig.minAmount,
      effectiveDate: new Date().toISOString().slice(0, 10)
    })
    ElMessage.success('保存成功')
  } catch { ElMessage.error('保存失败') }
  finally { configSaving.value = false }
}

onMounted(() => {
  loadStages()
  loadLateFeeConfig()
})
</script>
