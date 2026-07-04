<template>
  <div class="app-input-number" :class="{ 'is-disabled': disabled }">
    <button
      class="app-input-number__decrease"
      :class="{ 'is-disabled': modelValue <= min }"
      :disabled="disabled || modelValue <= min"
      @click="decrease"
      type="button"
    >
      <el-icon><Minus /></el-icon>
    </button>
    <input
      :value="String(modelValue)"
      :disabled="disabled"
      type="number"
      :min="min"
      :max="max"
      :step="step"
      :placeholder="placeholder"
      @input="handleInput"
      @blur="handleBlur"
      class="app-input-number__input"
    />
    <button
      class="app-input-number__increase"
      :class="{ 'is-disabled': modelValue >= max }"
      :disabled="disabled || modelValue >= max"
      @click="increase"
      type="button"
    >
      <el-icon><Plus /></el-icon>
    </button>
  </div>
</template>

<script setup>
import { Minus, Plus } from '@element-plus/icons-vue'

const props = defineProps({
  modelValue: { type: Number, default: 0 },
  min: { type: Number, default: -Infinity },
  max: { type: Number, default: Infinity },
  step: { type: Number, default: 1 },
  disabled: { type: Boolean, default: false },
  placeholder: { type: String, default: '' },
})

const emit = defineEmits(['update:modelValue'])

function decrease() {
  const val = clamp((props.modelValue || 0) - props.step)
  emit('update:modelValue', val)
}

function increase() {
  const val = clamp((props.modelValue || 0) + props.step)
  emit('update:modelValue', val)
}

function handleInput(e) {
  const num = Number(e.target.value)
  if (!isNaN(num)) {
    emit('update:modelValue', clamp(num))
  }
}

function handleBlur() {
  // 空值或无效值时恢复为最小值
  if (props.modelValue === null || props.modelValue === undefined || isNaN(Number(props.modelValue))) {
    emit('update:modelValue', clamp(props.min !== -Infinity ? props.min : 0))
  } else {
    emit('update:modelValue', clamp(props.modelValue))
  }
}

function clamp(val) {
  if (props.min !== -Infinity && val < props.min) return props.min
  if (props.max !== Infinity && val > props.max) return props.max
  return val
}
</script>

<style scoped>
.app-input-number {
  display: inline-flex;
  align-items: center;
  width: 100%;
  border: 1px solid var(--el-border-color, #dcdfe6);
  border-radius: var(--el-border-radius-base, 4px);
  overflow: hidden;
  background: var(--el-fill-color-blank, #fff);
  transition: border-color 0.2s;
}

.app-input-number:hover {
  border-color: var(--el-border-color-hover, #c0c4cc);
}

.app-input-number.is-disabled {
  border-color: var(--el-border-color-light, #e4e7ed);
  background: var(--el-disabled-bg-color, #f5f7fa);
  cursor: not-allowed;
}

.app-input-number__decrease,
.app-input-number__increase {
  flex-shrink: 0;
  flex-basis: 24px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 30px;
  border: none;
  background: var(--el-fill-color-light, #f5f7fa);
  color: var(--el-text-color-primary, #303133);
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;
  user-select: none;
  outline: none;
  padding: 0;
  line-height: 1;
}

.app-input-number__decrease:hover:not(.is-disabled),
.app-input-number__increase:hover:not(.is-disabled) {
  background: var(--el-color-primary-light-8, #d9ecff);
  color: var(--el-color-primary, #409eff);
}

.app-input-number__decrease.is-disabled,
.app-input-number__increase.is-disabled {
  color: var(--el-text-color-placeholder, #c0c4cc);
  cursor: not-allowed;
  background: var(--el-disabled-bg-color, #f5f7fa);
}

.app-input-number__input {
  flex: 1;
  min-width: 0;
  width: 0;
  height: 30px;
  border: none;
  outline: none;
  text-align: center;
  font-size: 13px;
  font-family: inherit;
  color: var(--el-text-color-primary, #303133);
  background: transparent;
  -moz-appearance: textfield;
  padding: 0;
  box-sizing: border-box;
}

.app-input-number__input::-webkit-outer-spin-button,
.app-input-number__input::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.app-input-number__input::placeholder {
  color: var(--el-text-color-placeholder, #c0c4cc);
}

.app-input-number__input:disabled {
  color: var(--el-text-color-placeholder, #c0c4cc);
  cursor: not-allowed;
}
</style>
