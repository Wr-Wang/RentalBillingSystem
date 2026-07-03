<template>
  <div>
    <div class="page-header">
      <h2>自动续签配置</h2>
      <el-button @click="fetchConfig" :loading="loading">刷新</el-button>
    </div>

    <el-card v-loading="loading">
      <el-alert title="配置自动续签策略 — 系统每天检查到期合同，根据规则自动发起续签审批" type="info" show-icon :closable="false" style="margin-bottom: 16px;" />

      <el-form :model="config" label-width="160px" style="max-width: 600px;">
        <el-form-item label="提前天数">
          <el-input-number v-model="config.advanceDays" :min="1" :max="90" style="width: 200px;" />
          <span style="margin-left: 8px; color: #909399;">到期前 N 天触发续签</span>
        </el-form-item>

        <el-form-item label="租金规则">
          <el-select v-model="config.rentRule" style="width: 200px;">
            <el-option label="保持相同" value="Same" />
            <el-option label="固定百分比上浮" value="Percentage" />
            <el-option label="按市场价格" value="MarketPrice" />
          </el-select>
        </el-form-item>

        <el-form-item v-if="config.rentRule === 'Percentage'" label="上浮百分比">
          <el-input-number v-model="config.rentIncreasePercent" :precision="2" :min="0" :max="100" style="width: 200px;" />
          <span style="margin-left: 8px;">%</span>
        </el-form-item>

        <el-form-item label="期限规则">
          <el-select v-model="config.termRule" style="width: 200px;">
            <el-option label="保持相同期限" value="Same" />
            <el-option label="固定月数" value="FixedMonths" />
          </el-select>
        </el-form-item>

        <el-form-item v-if="config.termRule === 'FixedMonths'" label="续签月数">
          <el-input-number v-model="config.termMonths" :min="1" :max="60" style="width: 200px;" />
        </el-form-item>

        <el-form-item label="逾期处理">
          <el-select v-model="config.overdueAction" style="width: 200px;">
            <el-option label="阻止续签" value="Block" />
            <el-option label="警告后继续" value="WarnAndContinue" />
            <el-option label="跳过检查" value="Skip" />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="saveConfig" :loading="saving">保存</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/user'
import { getAutoRenewConfig, saveAutoRenewConfig } from '@/api'

const userStore = useUserStore()
const loading = ref(false)
const saving = ref(false)

const config = reactive({
  advanceDays: 7,
  rentRule: 'Same',
  rentIncreasePercent: null,
  termRule: 'Same',
  termMonths: null,
  overdueAction: 'Block'
})

async function fetchConfig() {
  const companyId = userStore.effectiveCompanyId || userStore.homeCompanyId
  if (!companyId) return
  loading.value = true
  try {
    const res = await getAutoRenewConfig(companyId)
    if (res) {
      config.advanceDays = res.advanceDays ?? 7
      config.rentRule = res.rentRule || 'Same'
      config.rentIncreasePercent = res.rentIncreasePercent ?? null
      config.termRule = res.termRule || 'Same'
      config.termMonths = res.termMonths ?? null
      config.overdueAction = res.overdueAction || 'Block'
    }
  } catch { /* 无配置时使用默认值 */ }
  loading.value = false
}

async function saveConfig() {
  const companyId = userStore.effectiveCompanyId || userStore.homeCompanyId
  if (!companyId) { ElMessage.error('无法获取公司信息'); return }
  saving.value = true
  try {
    await saveAutoRenewConfig({ companyId, ...config })
    ElMessage.success('保存成功')
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '保存失败')
  }
  saving.value = false
}

onMounted(fetchConfig)
</script>
