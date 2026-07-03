<template>
  <div>
    <div class="page-header"><h2>收款登记</h2><el-button @click="$router.back()">返回</el-button></div>
    <el-card>
      <el-form :model="form" label-width="100px">
        <el-form-item label="收款单号"><el-input v-model="form.receiptNo" placeholder="自动生成" /></el-form-item>
        <el-form-item label="收款金额"><el-input-number v-model="form.amount" :min="0.01" :precision="2" style="width:100%;" /></el-form-item>
        <el-form-item label="收款日期"><el-date-picker v-model="form.receivedDate" type="date" style="width:100%;" /></el-form-item>
        <el-form-item label="合同ID"><el-input v-model="form.contractId" placeholder="选填" /></el-form-item>
      </el-form>
      <div style="text-align:center;padding-top:16px;">
        <el-button type="primary" @click="submit" :loading="submitting">登记</el-button>
      </div>
    </el-card>
  </div>
</template>
<script setup>
import { reactive, ref } from 'vue'
import { createReceipt } from '@/api'
import { ElMessage } from 'element-plus'
const form = reactive({ receiptNo: '', amount: 0, receivedDate: '', contractId: '' })
const submitting = ref(false)
async function submit() {
  if (form.amount <= 0) { ElMessage.warning('请输入金额'); return }
  submitting.value = true
  try {
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    await createReceipt({ receiptNo: form.receiptNo || `RC-${Date.now()}`, amount: form.amount, receivedDate: form.receivedDate || new Date().toISOString().slice(0, 10), companyId: user.defaultCompanyId, contractId: form.contractId || undefined })
    ElMessage.success('登记成功')
  } catch (e) { ElMessage.error(e?.response?.data?.message || '登记失败') }
  finally { submitting.value = false }
}
</script>
