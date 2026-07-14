<template>
  <!--
    ★ 组件根容器
    BEM 命名：块名 app-input-number
    :class 对象语法：当 disabled 为 true 时，渲染 class="app-input-number is-disabled"
  -->
  <div class="app-input-number" :class="{ 'is-disabled': disabled }">

    <!-- ────── 减号按钮 ────── -->
    <button
      class="app-input-number__decrease"                            /* BEM 元素 */
      :class="{ 'is-disabled': modelValue <= min }"                 /* 到达最小值时显示禁用样式 */
      :disabled="disabled || modelValue <= min"                     /* 禁用时不可点击 */
      @click="decrease"                                             /* 点击触发减少 */
      type="button"
    >
      <el-icon><Minus /></el-icon>
    </button>

    <!--
      ────── 数字输入框 ──────
      注意这里用的是 :value + @input（手动单向绑定），而不是 v-model。
      原因：v-model 会全权接管输入，但我们需要在 emit 之前先经过 clamp() 做范围限制，
      如果 v-model 直接把非法值写入了父组件数据，clamp 就来不及了。
    -->
    <input
      :value="String(modelValue)"       /* 将数值转字符串显示（input 的 value 只能是 string）*/
      :disabled="disabled"
      type="number"                      /* 移动端弹出数字键盘 */
      :min="min"                         /* 浏览器原生校验（虽然不是主要约束，聊胜于无）*/
      :max="max"
      :step="step"
      :placeholder="placeholder"
      @input="handleInput"              /* 每次按键都触发，实时更新 */
      @blur="handleBlur"                /* 失焦时修正无效值 */
      class="app-input-number__input"
    />

    <!-- ────── 加号按钮 ────── -->
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
/**
 * =========================================================================
 *  AppInputNumber.vue — 带增减按钮的数字输入框组件
 *
 *  功能：
 *    封装了「－ 数字输入框 ＋」的典型组合，支持 v-model 双向绑定。
 *    常用于表单中需要限制范围的数字输入（如排序值、楼层数等）。
 *
 *  使用示例：
 *    <AppInputNumber v-model="sortOrder" :min="0" :max="999" />
 *
 *  设计思路：
 *    为什么不用 Element Plus 的 el-input-number？
 *    因为 el-input-number 的样式与项目整体风格不完全匹配，
 *    且本项目只需要最基本的增减 + 范围限制功能，自建组件更轻量可控。
 * =========================================================================
 */

// ---- 导入仅仅用了图标组件 ----
// 注意：这里没有 import Vue 的 ref/computed/onMounted 等
// 因为这个组件足够简单，只依赖 props 和函数，不需要内部响应式状态
import { Minus, Plus } from '@element-plus/icons-vue'

// =========================================================================
// defineProps — 声明组件接收哪些外部参数（父→子）
// =========================================================================
//
// 为什么叫 defineProps 而不是 props？
//   因为这是编译宏（compiler macro），在 <script setup> 中自动可用，
//   不需要从 vue 中 import。编译时会被替换为实际的运行时代码。
//
// 参数说明：
const props = defineProps({
  /** 当前值（v-model 绑定目标） — 必须用 modelValue 这个名字才能支持 v-model */
  modelValue: { type: Number, default: 0 },

  /** 最小值（默认 -Infinity = 不限制下限）*/
  min: { type: Number, default: -Infinity },

  /** 最大值（默认 Infinity = 不限制上限）*/
  max: { type: Number, default: Infinity },

  /** 点击按钮一次增加/减少的步长 */
  step: { type: Number, default: 1 },

  /** 是否禁用整个组件 */
  disabled: { type: Boolean, default: false },

  /** 输入框占位文本 */
  placeholder: { type: String, default: '' },
})

// =========================================================================
// defineEmits — 声明组件可以触发哪些事件（子→父）
// =========================================================================
//
// update:modelValue 是 v-model 的协议事件名。
// 父组件写 v-model="x" 等效于：
//   :model-value="x" + @update:model-value="(val) => x = val"
//
// 所以组件内调用 emit('update:modelValue', newVal) 就等于父组件的 x = newVal
const emit = defineEmits(['update:modelValue'])

// =========================================================================
// decrease — 减一
// =========================================================================
// 触发时机：用户点击「－」按钮
// 逻辑：当前值 - 步长 → 限制范围 → 通知父组件
function decrease() {
  // props.modelValue 可能是 0 / null / undefined，用 || 0 兜底
  const val = clamp((props.modelValue || 0) - props.step)
  emit('update:modelValue', val)
}

// =========================================================================
// increase — 加一
// =========================================================================
// 触发时机：用户点击「＋」按钮
function increase() {
  const val = clamp((props.modelValue || 0) + props.step)
  emit('update:modelValue', val)
}

// =========================================================================
// handleInput — 键盘输入
// =========================================================================
// 触发时机：用户在输入框中逐键输入
// 注意：这里不校验输入合法性（不合法就忽略），交给 handleBlur 修正
function handleInput(e) {
  const num = Number(e.target.value)          // input 的 value 永远是 string，转 number
  if (!isNaN(num)) {                           // 只处理有效数字
    emit('update:modelValue', clamp(num))
  }
  // 如果输入的是非数字（如空字符串、abc），直接忽略，不 emit
}

// =========================================================================
// handleBlur — 失焦修正
// =========================================================================
// 触发时机：输入框失去焦点
// 作用：处理边界情况——输入框为空、非法值、小数越界等
function handleBlur() {
  // 情况一：modelValue 是 null / undefined / NaN → 恢复为最小值或 0
  if (props.modelValue === null || props.modelValue === undefined || isNaN(Number(props.modelValue))) {
    emit('update:modelValue', clamp(props.min !== -Infinity ? props.min : 0))
  } else {
    // 情况二：值本身有效，但可能不在 [min, max] 范围内 → clamp 修正
    emit('update:modelValue', clamp(props.modelValue))
  }
}

// =========================================================================
// clamp — 范围限制工具函数
// =========================================================================
// 将数值限制在 [min, max] 闭区间内
// 这是一个纯函数（输入相同 → 输出相同，不依赖外部状态）
// 因为逻辑简单且不需要缓存，所以用普通函数而不是 computed
//
// 参数 val：要限制的数值
// 返回值：限制后的数值
function clamp(val) {
  if (props.min !== -Infinity && val < props.min) return props.min   // 低于下限 → 取下限
  if (props.max !== Infinity && val > props.max) return props.max   // 高于上限 → 取上限
  return val                                                          // 在范围内 → 原值返回
}
</script>

<style scoped>
/**
 * =========================================================================
 * scoped 样式说明
 *
 * scoped 的作用：
 *   Vue 编译器会为组件的每个 DOM 元素自动生成一个唯一的 data-v-xxxxx 属性，
 *   同时为所有 CSS 选择器加上 [data-v-xxxxx]，从而实现样式隔离。
 *
 *   例如：
 *     编译前 .app-input-number { ... }
 *     编译后 .app-input-number[data-v-7ba5bd90] { ... }
 *
 *   这意味着这些样式只作用于本组件，不会泄漏到全局，也不受外部样式影响。
 *
 * CSS 变量用法：
 *   大量使用 var(--el-xxx, fallback) 语法，第一值是 Element Plus 的 CSS 变量，
 *   第二值是兜底值。这样即使父组件没有挂载 Element Plus 主题，也有基本样式。
 * =========================================================================
 */

/* ────── 外层容器 — flex 行内排列，左减号｜中间输入框｜右加号 ────── */
.app-input-number {
  display: inline-flex;           /* 行内弹性盒，可与文字并排 */
  align-items: center;            /* 三个子元素垂直居中 */
  width: 100%;                    /* 撑满父容器宽度 */
  border: 1px solid var(--el-border-color, #dcdfe6);    /* Element Plus 边框色，兜底浅灰 */
  border-radius: var(--el-border-radius-base, 4px);     /* 圆角 */
  overflow: hidden;               /* 圆角溢出隐藏（让按钮和输入框不冲出圆角）*/
  background: var(--el-fill-color-blank, #fff);         /* 背景白色 */
  transition: border-color 0.2s;  /* 边框颜色渐变动画 */
}

/* ────── 容器 hover 时边框变色 ────── */
.app-input-number:hover {
  border-color: var(--el-border-color-hover, #c0c4cc);
}

/* ────── 禁用态 — 整体变灰 ────── */
.app-input-number.is-disabled {
  border-color: var(--el-border-color-light, #e4e7ed);
  background: var(--el-disabled-bg-color, #f5f7fa);
  cursor: not-allowed;            /* 禁用光标 */
}

/* ────── 减号/加号按钮（共用样式）────── */
.app-input-number__decrease,
.app-input-number__increase {
  flex-shrink: 0;                 /* 不被压缩 */
  flex-basis: 24px;               /* 固定基础宽度 */
  display: inline-flex;           /* 使图标居中 */
  align-items: center;
  justify-content: center;
  width: 24px;                    /* 固定宽 */
  height: 30px;                   /* 固定高 */
  border: none;
  background: var(--el-fill-color-light, #f5f7fa);    /* 浅灰背景，与输入域区分 */
  color: var(--el-text-color-primary, #303133);
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;           /* 背景色/文字色均渐变动画 */
  user-select: none;              /* 防止点击按钮时文字被选中 */
  outline: none;                  /* 去掉焦点轮廓 */
  padding: 0;
  line-height: 1;                 /* 防止行高撑大按钮 */
}

/* ────── 按钮 hover（非禁用态）—— 变蓝 ────── */
.app-input-number__decrease:hover:not(.is-disabled),
.app-input-number__increase:hover:not(.is-disabled) {
  background: var(--el-color-primary-light-8, #d9ecff);   /* 浅蓝背景 */
  color: var(--el-color-primary, #409eff);                 /* 蓝色文字 */
}

/* ────── 按钮禁用态 — 变灰不可点 ────── */
.app-input-number__decrease.is-disabled,
.app-input-number__increase.is-disabled {
  color: var(--el-text-color-placeholder, #c0c4cc);
  cursor: not-allowed;
  background: var(--el-disabled-bg-color, #f5f7fa);
}

/* ────── 数字输入框 ────── */
.app-input-number__input {
  flex: 1;                        /* 占据剩余空间（中间拉伸）*/
  min-width: 0;                   /* flex 子项允许收缩到 0 */
  width: 0;                       /* 配合 flex:1 让浏览器自行分配宽度 */
  height: 30px;
  border: none;
  outline: none;
  text-align: center;             /* 数字居中 */
  font-size: 13px;
  font-family: inherit;           /* 继承系统字体 */
  color: var(--el-text-color-primary, #303133);
  background: transparent;        /* 透明背景，露出父容器的白色 */
  -moz-appearance: textfield;     /* Firefox：隐藏原生数字输入框的上下箭头按钮 */
  padding: 0;
  box-sizing: border-box;
}

/* ────── Chrome/Safari：隐藏 input[type=number] 的上下箭头按钮 ────── */
.app-input-number__input::-webkit-outer-spin-button,
.app-input-number__input::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

/* ────── 占位文本样式 ────── */
.app-input-number__input::placeholder {
  color: var(--el-text-color-placeholder, #c0c4cc);
}

/* ────── 输入框禁用态 ────── */
.app-input-number__input:disabled {
  color: var(--el-text-color-placeholder, #c0c4cc);
  cursor: not-allowed;
}
</style>
