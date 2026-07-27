<template>
  <div class="address-cascader">
    <div class="cascader-row">
      <el-cascader
        v-model="selectedCodes"
        :props="cascaderProps"
        :placeholder="cascaderPlaceholder"
        clearable
        collapse-tags
        collapse-tags-tooltip
        style="width: 100%"
        @change="onCascaderChange"
      />
    </div>
    <div class="detail-row" style="margin-top: 8px">
      <el-input
        v-model="detailText"
        :placeholder="detailPlaceholder"
        type="textarea"
        :rows="2"
        @input="composeAddress"
      />
    </div>
    <div v-if="composed" class="preview-row" style="margin-top: 6px; color: #909399; font-size: 13px">
      完整地址：{{ composed }}
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import { getRegionProvinces, getRegionChildren } from '../api'

const props = defineProps({
  modelValue: { type: String, default: '' },
  placeholder: { type: String, default: '请输入详细地址（路/号/小区/大厦）' },
  maxLevel: { type: Number, default: 5 },
})

const emit = defineEmits(['update:modelValue'])

const selectedCodes = ref([])
const detailText = ref('')
const composed = ref('')
// 名称缓存：code → name，在 lazyLoad 时同步填充
const nameMap = reactive({})

// 级联选择器配置：懒加载 + 同步缓存名称
const cascaderProps = {
  lazy: true,
  lazyLoad: async (node, resolve) => {
    const list = node.level === 0
      ? await getRegionProvinces()
      : await getRegionChildren(node.value)
    const options = list.map(item => {
      nameMap[item.code] = item.name  // 同步缓存名称
      return {
        value: item.code,
        label: item.name,
        leaf: item.level >= props.maxLevel
      }
    })
    resolve(options)
  }
}

const cascaderPlaceholder = `选择省/市${props.maxLevel >= 4 ? '/街道' : ''}${props.maxLevel >= 5 ? '/社区' : ''}`

function onCascaderChange(values) {
  if (!values || values.length === 0) {
    composed.value = detailText.value || ''
    emit('update:modelValue', composed.value)
    return
  }
  composeAddress()
}

function composeAddress() {
  const codes = selectedCodes.value || []
  const detail = detailText.value || ''
  const names = codes.map(c => nameMap[c] || c)
  composed.value = names.join('') + detail
  emit('update:modelValue', composed.value)
}

// 初始值回填：有旧值时放入详细地址框，级联留空让用户重选
watch(() => props.modelValue, (val) => {
  if (val && !composed.value) {
    detailText.value = val
    composed.value = val
  }
}, { immediate: true })
</script>
