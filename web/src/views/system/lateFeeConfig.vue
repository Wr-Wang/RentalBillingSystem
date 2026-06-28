<template>
  <div>
    <div class="page-header">
      <h2>滞纳金配置</h2>
    </div>

    <el-card>
      <el-form :model="config" label-width="140px" style="max-width: 500px;">
        <el-form-item label="日利率">
          <el-input-number v-model="config.dailyRate" :precision="4" :step="0.0001" :min="0" style="width: 200px;" />
          <span style="margin-left: 8px;">（{{ (config.dailyRate * 100).toFixed(4) }}% / 天）</span>
        </el-form-item>
        <el-form-item label="年化利率">
          <span style="line-height: 32px; color: #909399;">{{ (config.dailyRate * 365 * 100).toFixed(2) }}%</span>
        </el-form-item>
        <el-form-item label="宽限期">
          <el-input-number v-model="config.gracePeriodDays" :min="0" style="width: 200px;" />
          <span style="margin-left: 8px;">天（到期后X天内不计滞纳金）</span>
        </el-form-item>
        <el-form-item label="滞纳金上限">
          <el-input-number v-model="config.maxPercentOfPrincipal" :precision="2" :min="0" :max="100" style="width: 200px;" />
          <span style="margin-left: 8px;">% 本金</span>
        </el-form-item>
        <el-form-item label="最低滞纳金">
          <el-input-number v-model="config.minLateFeeAmount" :precision="2" :min="0" style="width: 200px;" />
          <span style="margin-left: 8px;">元（低于此值不计）</span>
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="config.effectiveDate" type="date" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="save">保存配置</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

const config = reactive({
  dailyRate: 0.0005,
  gracePeriodDays: 3,
  maxPercentOfPrincipal: 100,
  minLateFeeAmount: 1,
  effectiveDate: '2026-01-01'
})

function save() {
  ElMessage.success('滞纳金配置已保存')
}
</script>
