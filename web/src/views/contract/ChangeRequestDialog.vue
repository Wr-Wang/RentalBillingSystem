<template>
  <el-dialog v-model="dialogVisible" title="新建变更请求" width="680px" :before-close="handleClose">
    <!-- 提示 -->
    <el-alert
      title="变更请求提交后将进入审批流程，审批通过后自动生效"
      type="info"
      show-icon
      :closable="false"
      style="margin-bottom: 16px;"
    />

    <!-- 合同基本信息 -->
    <el-descriptions :column="2" border style="margin-bottom: 16px;" v-if="contract">
      <el-descriptions-item label="合同号">{{ contract.contractNo }}</el-descriptions-item>
      <el-descriptions-item label="租客">{{ contract.tenantName }}</el-descriptions-item>
      <el-descriptions-item label="月租金">¥{{ (contract.rentAmount || 0).toLocaleString() }}</el-descriptions-item>
      <el-descriptions-item label="状态">
        <el-tag size="small">{{ contract.status }}</el-tag>
      </el-descriptions-item>
    </el-descriptions>

    <!-- 表单 -->
    <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
      <el-form-item label="变更类型" prop="changeType">
        <el-select v-model="form.changeType" style="width: 100%;" @change="onChangeTypeChange">
          <el-option label="租金调整" value="RENT_ADJUST" />
          <el-option label="费用调价" value="FEE_ADJUST" />
          <el-option label="合同条款修改" value="TERMS_MODIFY" />
          <el-option label="其他" value="OTHER" />
        </el-select>
      </el-form-item>

      <el-form-item label="原因说明" prop="reason">
        <el-input v-model="form.reason" type="textarea" :rows="3" placeholder="请说明变更原因" />
      </el-form-item>

      <el-form-item label="生效日期">
        <el-date-picker v-model="form.effectiveDate" type="date" placeholder="可选，留空表示立即生效" style="width: 100%;" />
      </el-form-item>

      <!-- 变更项表格 -->
      <el-form-item label="变更内容" v-if="form.items.length > 0">
        <el-table :data="form.items" stripe size="small" style="width: 100%;">
          <el-table-column label="目标" width="120">
            <template #default="{ row }">
              {{ row.targetType === 'Contract' ? '合同' : row.targetType === 'ContractFeeConfig' ? '费用配置' : row.targetType }}
            </template>
          </el-table-column>
          <el-table-column label="字段" width="120" prop="fieldName" />
          <el-table-column label="原值" width="100" prop="oldValue" />
          <el-table-column label="新值" width="100" prop="newValue" />
          <el-table-column label="操作" width="60">
            <template #default="{ $index }">
              <el-button text size="small" type="danger" @click="form.items.splice($index, 1)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="handleClose">取消</el-button>
      <el-button type="primary" @click="submit" :loading="submitting" :disabled="form.items.length === 0">
        提交审批
      </el-button>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, watch, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/user'
import { createChangeRequest, submitChangeRequest } from '@/api'

const props = defineProps({
  contractId: { type: String, required: true },
  contract: { type: Object, default: null },
  visible: { type: Boolean, default: false }
})

const emit = defineEmits(['close', 'submitted'])

const userStore = useUserStore()
const formRef = ref(null)
const submitting = ref(false)

const dialogVisible = ref(false)
watch(() => props.visible, (v) => { dialogVisible.value = v })

const form = reactive({
  changeType: '',
  reason: '',
  effectiveDate: null,
  items: []
})

const rules = {
  changeType: [{ required: true, message: '请选择变更类型', trigger: 'change' }],
  reason: [{ required: true, message: '请输入变更原因', trigger: 'blur' }]
}

const contractFeeConfigs = ref([])

// 切换变更类型时预填变更项
function onChangeTypeChange(type) {
  form.items = []
  if (type === 'RENT_ADJUST' && props.contract) {
    form.items.push({
      targetType: 'Contract',
      targetId: props.contractId,
      fieldName: 'RentAmount',
      oldValue: String(props.contract.rentAmount || 0),
      newValue: String(props.contract.rentAmount || 0)
    })
  } else if (type === 'FEE_ADJUST') {
    // 加载费用配置列表（由父组件传入或动态加载）
    form.items.push({
      targetType: 'ContractFeeConfig',
      targetId: null,
      fieldName: 'Amount',
      oldValue: '',
      newValue: ''
    })
  }
}

function handleClose() {
  dialogVisible.value = false
  emit('close')
}

async function submit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  submitting.value = true
  try {
    const companyId = userStore.effectiveCompanyId || userStore.homeCompanyId
    if (!companyId) {
      ElMessage.error('无法获取公司信息')
      return
    }

    const items = form.items.map(item => ({
      targetType: item.targetType,
      targetId: item.targetType === 'Contract' ? null : (item.targetId || null),
      fieldName: item.fieldName,
      oldValue: item.oldValue || null,
      newValue: item.newValue,
      newValueDecimal: item.fieldName === 'RentAmount' || item.fieldName === 'Amount'
        ? (parseFloat(item.newValue) || 0) : null,
      oldValueDecimal: item.fieldName === 'RentAmount' || item.fieldName === 'Amount'
        ? (parseFloat(item.oldValue) || 0) : null
    }))

    // 创建变更请求
    const cr = await createChangeRequest({
      contractId: props.contractId,
      companyId,
      changeType: form.changeType,
      reason: form.reason,
      effectiveDate: form.effectiveDate || null,
      items
    })

    // 提交审批
    await submitChangeRequest(cr.id)

    ElMessage.success('变更请求已提交审批')
    dialogVisible.value = false
    emit('submitted', cr)
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  } finally {
    submitting.value = false
  }
}
</script>
