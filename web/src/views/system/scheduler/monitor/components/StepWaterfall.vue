<template>
  <div class="step-waterfall">
    <!-- 基本信息 -->
    <div class="detail-header" v-if="logDetail">
      <div class="header-row">
        <span class="task-name">{{ logDetail.taskName }}</span>
        <el-tag :type="statusTagType(logDetail.status)" size="small" effect="dark" round>
          {{ statusLabel(logDetail.status) }}
        </el-tag>
        <el-tag v-if="logDetail.runMode === 'DryRun'" type="warning" size="small" effect="plain">预执行</el-tag>
      </div>
      <div class="meta-row">
        <span>⏱ 耗时: <strong>{{ formatDuration(logDetail.totalDurationMs) }}</strong></span>
        <span v-if="logDetail.successCount != null">✅ 成功: {{ logDetail.successCount }}</span>
        <span v-if="logDetail.failCount">❌ 失败: {{ logDetail.failCount }}</span>
        <span v-if="logDetail.warningCount">⚠️ 警告: {{ logDetail.warningCount }}</span>
        <span>触发: {{ {Scheduled:'自动',Manual:'手动',Event:'事件'}[logDetail.triggerType] || logDetail.triggerType }}</span>
        <span v-if="logDetail.targetMonth">账期: {{ logDetail.targetMonth }}</span>
      </div>
      <div class="summary-row" v-if="logDetail.summary">{{ logDetail.summary }}</div>
      <div class="error-row" v-if="logDetail.errorMessage" style="color:#f56c6c;font-size:12px;margin-top:4px;">
        {{ logDetail.errorMessage }}
      </div>
    </div>

    <!-- 瀑布图 -->
    <div v-if="steps.length > 0" class="waterfall-container">
      <div class="waterfall-title">步骤执行瀑布图</div>
      <div class="waterfall-header">
        <span class="wf-label">步骤</span>
        <span class="wf-bar-area">
          <span class="wf-time-axis">0s</span>
          <span v-for="t in tickMarks" :key="t" class="wf-tick">{{ t }}s</span>
        </span>
        <span class="wf-status">状态</span>
        <span class="wf-duration">耗时</span>
        <span class="wf-impact">影响</span>
      </div>
      <div
        v-for="step in flatSteps"
        :key="step.id"
        class="waterfall-row"
        :class="{ 'is-child': step.isChild }"
      >
        <span class="wf-label" :style="{ paddingLeft: step.isChild ? '24px' : '0' }">
          <el-tooltip :content="step.stepDisplayName" placement="top">
            <span>{{ step.stepDisplayName }}</span>
          </el-tooltip>
        </span>
        <span class="wf-bar-area">
          <span
            class="wf-bar"
            :style="{ width: barWidth(step), backgroundColor: barColor(step) }"
          >
            <span v-if="step.durationMs && maxDuration > 0 && (step.durationMs / maxDuration) > 0.15" class="bar-text">
              {{ (step.durationMs / 1000).toFixed(3) }}s
            </span>
          </span>
        </span>
        <span class="wf-status">
          <el-tag :type="step.status === 'Completed' ? 'success' : step.status === 'Failed' ? 'danger' : 'info'" size="small" effect="plain" style="border:0;">
            {{ step.status === 'Completed' ? '✅' : step.status === 'Failed' ? '❌' : step.status === 'Skipped' ? '⏭️' : '🔄' }}
          </el-tag>
        </span>
        <span class="wf-duration">{{ step.durationMs != null ? (step.durationMs / 1000).toFixed(3) + 's' : '-' }}</span>
        <span class="wf-impact">
          <el-tag v-if="step.affectedCount != null" size="small">{{ step.affectedCount }} 条</el-tag>
          <span v-else>-</span>
        </span>
      </div>
    </div>
    <el-empty v-else-if="logDetail && steps.length === 0" description="暂无步骤数据" />
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  logDetail: { type: Object, default: null },
  steps: { type: Array, default: () => [] }
})

const maxDuration = computed(() => {
  if (props.steps.length === 0) return 0
  return Math.max(...props.steps.map(s => s.durationMs || 0), 1)
})

const tickMarks = computed(() => {
  if (maxDuration.value <= 0) return []
  const totalSec = Math.ceil(maxDuration.value / 1000)
  const ticks = []
  // Generate 3-5 ticks
  const step = Math.max(1, Math.ceil(totalSec / 4))
  for (let t = step; t < totalSec; t += step) {
    ticks.push(t)
  }
  return ticks
})

const flatSteps = computed(() => {
  const result = []
  const parentMap = {}
  // Sort by SortOrder
  const sorted = [...props.steps].sort((a, b) => a.sortOrder - b.sortOrder)
  for (const s of sorted) {
    if (!s.parentId) {
      result.push({ ...s, isChild: false })
      parentMap[s.id] = result.length - 1
    }
  }
  for (const s of sorted) {
    if (s.parentId) {
      result.push({ ...s, isChild: true })
    }
  }
  return result
})

function barWidth(step) {
  if (!step.durationMs || maxDuration.value <= 0) return '0%'
  const pct = (step.durationMs / maxDuration.value) * 100
  return Math.max(pct, 3) + '%'
}

function barColor(step) {
  if (step.status === 'Completed') return '#67c23a'
  if (step.status === 'Failed') return '#f56c6c'
  if (step.status === 'Skipped') return '#909399'
  return '#409eff'
}

function statusTagType(status) {
  if (status === 'Completed') return 'success'
  if (status === 'Failed') return 'danger'
  if (status === 'Running') return 'primary'
  if (status === 'Stale') return 'warning'
  return 'info'
}

function statusLabel(status) {
  const map = { Completed: '成功', Failed: '失败', Running: '运行中', Stale: '异常中断', Reversed: '已反转' }
  return map[status] || status
}

function formatDuration(ms) {
  if (!ms) return '-'
  if (ms < 1000) return ms + 'ms'
  return (ms / 1000).toFixed(3) + 's'
}
</script>

<style scoped>
.step-waterfall { font-size: 13px; }
.detail-header { margin-bottom: 16px; padding: 12px; background: #f5f7fa; border-radius: 6px; }
.header-row { display: flex; align-items: center; gap: 8px; margin-bottom: 6px; }
.task-name { font-weight: 600; font-size: 15px; }
.meta-row { display: flex; gap: 16px; font-size: 12px; color: #606266; flex-wrap: wrap; }
.summary-row { margin-top: 6px; font-size: 12px; color: #606266; }
.waterfall-title { font-weight: 600; font-size: 14px; margin-bottom: 10px; }
.waterfall-header {
  display: flex; align-items: center; font-size: 12px; color: #909399;
  padding: 4px 0; border-bottom: 1px solid #ebeef5; margin-bottom: 4px;
}
.waterfall-row {
  display: flex; align-items: center; padding: 6px 0;
  border-bottom: 1px solid #f2f2f2; transition: background 0.15s;
}
.waterfall-row:hover { background: #fafafa; }
.wf-label { flex: 0 0 120px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: 13px; }
.wf-bar-area { flex: 1; display: flex; align-items: center; position: relative; height: 24px; margin: 0 8px; }
.wf-bar {
  height: 20px; border-radius: 4px; display: flex; align-items: center;
  justify-content: flex-end; padding-right: 4px; min-width: 12px; transition: width 0.3s;
}
.bar-text { color: #fff; font-size: 11px; white-space: nowrap; }
.wf-time-axis { position: absolute; left: 0; top: -14px; font-size: 10px; color: #c0c4cc; }
.wf-tick { position: absolute; font-size: 10px; color: #c0c4cc; top: -14px; }
.wf-status { flex: 0 0 36px; text-align: center; }
.wf-duration { flex: 0 0 48px; text-align: right; font-size: 12px; color: #606266; }
.wf-impact { flex: 0 0 60px; text-align: right; }
.is-child { color: #909399; }
.is-child .wf-label { font-size: 12px; }
</style>
