<template>
  <div>
    <div class="page-header">
      <h2>催缴配置</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-card>
      <template #header>催缴阶段定义</template>
      <el-table :data="stages" stripe>
        <el-table-column type="index" label="阶段" width="60" />
        <el-table-column prop="stageName" label="阶段名称" />
        <el-table-column label="逾期天数范围">
          <template #default="{ row }">{{ row.overdueDaysFrom }} - {{ row.overdueDaysTo }} 天</template>
        </el-table-column>
        <el-table-column prop="actionType" label="动作类型" />
        <el-table-column prop="isAuto" label="自动执行">
          <template #default="{ row }">
            <el-switch v-model="row.isAuto" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default>
            <el-button text size="small" type="primary">编辑</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card style="margin-top: 16px;">
      <template #header>滞纳金配置</template>
      <el-form :model="lateFeeConfig" label-width="140px" style="max-width: 500px;">
        <el-form-item label="日利率">
          <el-input-number v-model="lateFeeConfig.dailyRate" :precision="4" :step="0.0001" :min="0" style="width: 180px;" />
          <span style="margin-left: 8px;">%</span>
        </el-form-item>
        <el-form-item label="宽限期（天）">
          <el-input-number v-model="lateFeeConfig.gracePeriodDays" :min="0" style="width: 180px;" />
        </el-form-item>
        <el-form-item label="滞纳金上限">
          <el-input-number v-model="lateFeeConfig.maxPercent" :precision="2" :min="0" style="width: 180px;" />
          <span style="margin-left: 8px;">% 本金</span>
        </el-form-item>
        <el-form-item label="最低滞纳金">
          <el-input-number v-model="lateFeeConfig.minAmount" :precision="2" :min="0" style="width: 180px;" />
          <span style="margin-left: 8px;">元</span>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="saveConfig">保存配置</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

const stages = ref([
  { stageName: '短信提醒', overdueDaysFrom: 1, overdueDaysTo: 7, actionType: '短信', isAuto: true },
  { stageName: '电话催缴', overdueDaysFrom: 8, overdueDaysTo: 15, actionType: '电话', isAuto: true },
  { stageName: '催缴函', overdueDaysFrom: 16, overdueDaysTo: 30, actionType: '催缴函', isAuto: false },
  { stageName: '法律催缴', overdueDaysFrom: 31, overdueDaysTo: 365, actionType: '律师函', isAuto: false }
])

const lateFeeConfig = reactive({
  dailyRate: 0.05, gracePeriodDays: 3, maxPercent: 100, minAmount: 1
})

function saveConfig() {
  ElMessage.success('保存成功')
}
</script>
