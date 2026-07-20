<template>
  <!-- ====== 新增费用（一次性，无历史配置） ====== -->
  <div v-if="isNewFee" class="fee-card fee-card-new">
    <div class="fee-card-header">
      <div class="fee-card-title">
        <el-tag size="small" type="warning" effect="plain" class="charge-badge">一次性</el-tag>
        <span class="fee-name">{{ item.feeName }}</span>
      </div>
    </div>

    <div class="new-fee-body">
      <div class="new-fee-row">
        <span class="new-fee-label">金额</span>
        <span class="new-fee-value">¥{{ formattedAmount(item.newAmount) }}</span>
      </div>
      <div class="new-fee-row">
        <span class="new-fee-label">生效日期</span>
        <span class="new-fee-value">{{ formatDate(item.effectiveDate) || '-' }}</span>
      </div>
      <div class="new-fee-row">
        <span class="new-fee-label">计费方式</span>
        <span class="new-fee-value">{{ billingLabel(item.billingMode) }}</span>
      </div>
      <div class="new-fee-row">
        <span class="new-fee-label">收费类型</span>
        <span class="new-fee-value">一次性收费</span>
      </div>
    </div>
  </div>

  <!-- ====== 调价费用（有历史配置，新旧对比） ====== -->
  <div v-else class="fee-card" :class="{ 'fee-card-changed': isChanged }">
    <div class="fee-card-header">
      <div class="fee-card-title">
        <el-tag size="small" type="primary" effect="plain" class="charge-badge">周期性</el-tag>
        <span class="fee-name">{{ item.feeName }}</span>
      </div>
      <div v-if="isChanged" class="fee-card-change-badge">
        <span :style="{ color: isIncrease ? '#f56c6c' : '#67c23a', fontWeight: 'bold', fontSize: '15px' }">
          <el-icon :size="16" style="vertical-align: middle;">
            <Top v-if="isIncrease" />
            <Bottom v-else />
          </el-icon>
          {{ changePercent }}
        </span>
      </div>
      <el-tag v-else size="small" type="info" effect="plain">不变</el-tag>
    </div>

    <div class="fee-card-body">
      <div class="fee-config-col">
        <div class="config-col-title">当前配置</div>
        <div class="config-field">
          <span class="field-label">金额</span>
          <span class="field-value old-amount" :class="{ 'line-through': isChanged }">
            ¥{{ formattedAmount(item.oldAmount) }}
          </span>
        </div>
        <div class="config-field">
          <span class="field-label">生效</span>
          <span class="field-value">{{ formatDate(item.oldEffectiveDate) || '无历史配置' }}</span>
        </div>
        <div class="config-field">
          <span class="field-label">到期</span>
          <span class="field-value">{{ formatDate(item.oldExpiryDate) || '至今' }}</span>
        </div>
        <div class="config-field">
          <span class="field-label">计费</span>
          <span class="field-value">{{ billingLabel(item.oldBillingMode || item.billingMode) }}</span>
        </div>
        <div class="config-field" v-if="item.oldUnit || item.unit">
          <span class="field-label">单位</span>
          <span class="field-value">{{ item.oldUnit || item.unit || '-' }}</span>
        </div>
      </div>

      <div class="config-arrow">
        <el-icon :size="22" color="#c0c4cc"><Right /></el-icon>
      </div>

      <div class="fee-config-col new-config-col">
        <div class="config-col-title">新配置</div>
        <div class="config-field">
          <span class="field-label">金额</span>
          <span class="field-value new-amount" :style="{ color: isIncrease ? '#e6a23c' : isDecrease ? '#67c23a' : '#303133' }">
            ¥{{ formattedAmount(item.newAmount) }}
          </span>
        </div>
        <div class="config-field">
          <span class="field-label">生效</span>
          <span class="field-value" style="color: #e6a23c;">{{ formatDate(item.effectiveDate) || '-' }}</span>
        </div>
        <div class="config-field">
          <span class="field-label">到期</span>
          <span class="field-value">&mdash;</span>
        </div>
        <div class="config-field">
          <span class="field-label">计费</span>
          <span class="field-value">{{ billingLabel(item.billingMode) }}</span>
        </div>
        <div class="config-field" v-if="item.unit">
          <span class="field-label">单位</span>
          <span class="field-value">{{ item.unit }}</span>
        </div>
      </div>
    </div>

    <div v-if="isChanged" class="fee-card-footer" :class="isIncrease ? 'diff-increase' : 'diff-decrease'">
      <span class="diff-label">差额</span>
      <span class="diff-amount">
        <el-icon :size="14" style="vertical-align: middle;">
          <Top v-if="isIncrease" />
          <Bottom v-else />
        </el-icon>
        ¥{{ formattedAmount(Math.abs(diffAmount)) }}
      </span>
      <span class="diff-separator">|</span>
      <span class="diff-pct">{{ changePercent }}</span>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Top, Bottom, Right } from '@element-plus/icons-vue'


import { formatDate } from '@/utils/chinaTime'const props = defineProps({
  item: { type: Object, required: true }
})

const diffAmount = computed(() => props.item.newAmount - props.item.oldAmount)
const isIncrease = computed(() => diffAmount.value > 0)
const isDecrease = computed(() => diffAmount.value < 0)
const isChanged = computed(() => diffAmount.value !== 0)
const isNewFee = computed(() => props.item.chargeType === 'OneTime')

const changePercent = computed(() => {
  if (props.item.oldAmount <= 0) return isIncrease.value ? '+∞' : '-∞'
  const pct = (diffAmount.value / props.item.oldAmount * 100)
  return `${pct >= 0 ? '+' : ''}${pct.toFixed(1)}%`
})

function formattedAmount(val) {
  return Number(val).toLocaleString(undefined, { minimumFractionDigits: 2 })
}

function billingLabel(mode) {
  if (mode === 'MeterBased') return '按表计量'
  if (mode === 'FixedAmount') return '固定金额'
  return mode || '-'
}


</script>

<style scoped>
.fee-card {
  border: 1px solid #ebeef5;
  border-radius: 8px;
  background: #fff;
  overflow: hidden;
  transition: box-shadow 0.2s;
}
.fee-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}
.fee-card-changed {
  border-left: 3px solid #e6a23c;
}

/* ====== Header ====== */
.fee-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  background: #fafafa;
  border-bottom: 1px solid #f2f3f5;
}
.fee-card-title {
  display: flex;
  align-items: center;
  gap: 8px;
}
.charge-badge {
  flex-shrink: 0;
}
.fee-name {
  font-weight: 600;
  font-size: 14px;
  color: #303133;
}
.fee-card-change-badge {
  display: flex;
  align-items: center;
  gap: 4px;
}

/* ====== Body ====== */
.fee-card-body {
  display: flex;
  padding: 12px 14px;
  gap: 0;
}
.fee-config-col {
  flex: 1;
  min-width: 0;
}
.new-config-col {
  background: #fffbee;
  border-radius: 6px;
  padding: 8px 10px;
  margin: -8px -10px;
}
.config-col-title {
  font-size: 12px;
  color: #909399;
  margin-bottom: 8px;
  font-weight: 500;
}
.config-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  flex-shrink: 0;
  padding: 0 4px;
}
.config-field {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 3px 0;
  font-size: 13px;
}
.field-label {
  color: #909399;
  flex-shrink: 0;
}
.field-value {
  color: #303133;
  font-weight: 500;
  text-align: right;
}
.old-amount {
  color: #909399;
}
.line-through {
  text-decoration: line-through;
}
.new-amount {
  font-weight: bold;
  font-size: 14px;
}

/* ====== Footer ====== */
.fee-card-footer {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 8px 14px;
  font-size: 13px;
  font-weight: 500;
}
.diff-increase {
  background: #fef0f0;
  color: #f56c6c;
}
.diff-decrease {
  background: #f0f9eb;
  color: #67c23a;
}
.diff-label {
  color: inherit;
  opacity: 0.7;
}
.diff-amount {
  font-weight: bold;
}
.diff-separator {
  opacity: 0.3;
}
.diff-pct {
  font-weight: bold;
}

/* ====== 新增费用（一次性，无历史配置） ====== */
.fee-card-new {
  border-left: 3px solid #409eff;
}
.new-fee-body {
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.new-fee-row {
  display: flex;
  align-items: center;
  padding: 4px 0;
}
.new-fee-label {
  width: 80px;
  font-size: 13px;
  color: #909399;
  flex-shrink: 0;
}
.new-fee-value {
  font-size: 14px;
  color: #303133;
  font-weight: 500;
}
</style>
