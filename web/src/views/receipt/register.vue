<template>
  <div>
    <div class="page-header"><h2>收款登记</h2><el-button @click="$router.back()">返回</el-button></div>
    <el-card>
      <el-form :model="form" label-width="100px">
        <el-form-item label="收款金额">
          <el-input-number v-model="form.amount" :min="0.01" :precision="2" style="width:100%;" />
        </el-form-item>
        <el-form-item label="收款日期">
          <el-date-picker v-model="form.receivedDate" type="date" style="width:100%;" />
        </el-form-item>
        <el-form-item label="关联合同">
          <el-select v-model="form.contractId" clearable filterable placeholder="选填，按合同号搜索" style="width:100%;">
            <el-option v-for="c in contractOptions" :key="c.id" :label="c.contractNo + ' - ' + c.tenantName" :value="c.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="收款单号">
          <el-input v-model="form.receiptNo" placeholder="自动生成" />
        </el-form-item>
      </el-form>
      <div style="text-align:center;padding-top:16px;">
        <el-button type="primary" @click="submit" :loading="submitting">登记</el-button>
      </div>
    </el-card>
  </div>
</template>
<script setup>
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/store/user'
import { createReceipt, getContracts, getPaymentChannels } from '@/api'
import { ElMessage } from 'element-plus'

const router = useRouter()
const userStore = useUserStore()
const form = reactive({ receiptNo: '', amount: 0, receivedDate: '', contractId: '', paymentChannelId: '' })
const submitting = ref(false)
const contractOptions = ref([])
const paymentChannels = ref([])

function getEffectiveCompanyId() {
  return userStore.effectiveCompanyId || userStore.homeCompanyId
}

async function fetchContracts() {
  try {
    const res = await getContracts({ pageSize: 200 })
    const items = res.items || res.data || []
    contractOptions.value = items.map(c => ({
      id: c.id,
      contractNo: c.contractNo,
      tenantName: c.tenants?.length > 0 ? c.tenants[0].tenantName : ''
    }))
  } catch { /* 静默 */ }
}

async function submit() {
  if (form.amount <= 0) { ElMessage.warning('请输入金额'); return }
  const companyId = getEffectiveCompanyId()
  if (!companyId) { ElMessage.warning('请先选择公司'); return }

  submitting.value = true
  try {
    await createReceipt({
      receiptNo: form.receiptNo || undefined,
      amount: form.amount,
      receivedDate: form.receivedDate || new Date().toISOString().slice(0, 10),
      companyId,
      contractId: form.contractId || undefined,
      paymentChannelId: form.paymentChannelId || undefined
    })
    ElMessage.success('登记成功')
    router.push('/receipts')
  } catch (e) { ElMessage.error(e?.response?.data?.message || '登记失败') }
  finally { submitting.value = false }
}

async function fetchPaymentChannels() {
  try { const res = await getPaymentChannels(); paymentChannels.value = Array.isArray(res) ? res : (res.items || res.data || []) }
  catch { /* */ }
}

onMounted(() => { fetchContracts(); fetchPaymentChannels() })
</script>
