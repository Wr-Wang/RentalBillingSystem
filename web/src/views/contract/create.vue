<template>
  <div>
    <div class="page-header">
      <h2>新建合同</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-steps :active="step" align-center class="wizard-steps">
      <el-step title="选择房屋" />
      <el-step title="选择租客" />
      <el-step title="租金押金" />
      <el-step title="费用配置" />
      <el-step title="完成" />
    </el-steps>

    <!-- Step 1: Select Room -->
    <el-card v-show="step === 0">
      <template #header>选择房屋</template>
      <div class="search-bar">
        <el-input v-model="roomSearch" placeholder="搜索房间号" clearable style="width: 200px;" />
        <el-button type="primary" @click="loadRooms">查询</el-button>
      </div>
      <el-row :gutter="16" v-loading="roomsLoading">
        <el-col :xs="12" :sm="8" :md="6" :lg="4" v-for="room in filteredRooms" :key="room.id" style="margin-bottom:12px;">
          <el-card :class="['room-card', { 'is-selected': selectedRoom?.id === room.id }]" shadow="hover" @click="selectRoom(room)">
            <div class="room-code">{{ room.fullCode }}</div>
            <div class="room-card-body">
              <p><label>座楼：</label>{{ room.buildingName }}</p>
              <p><label>房型：</label>{{ room.roomTypeName || '-' }}</p>
              <p><label>面积：</label>{{ room.area ? room.area + ' m²' : '-' }}</p>
              <p class="room-rent">¥{{ (room.standardRent || 0).toLocaleString() }}<span>/月</span></p>
            </div>
            <div v-if="selectedRoom?.id === room.id" class="room-check">✓</div>
          </el-card>
        </el-col>
        <el-empty v-if="!roomsLoading && filteredRooms.length === 0" description="未找到匹配的房屋" />
      </el-row>
      <div style="text-align: right; margin-top: 16px;">
        <el-button type="primary" :disabled="!selectedRoom" @click="step = 1">下一步</el-button>
      </div>
    </el-card>

    <!-- Step 2: Select Tenant -->
    <el-card v-show="step === 1">
      <template #header>选择租客（可多选）</template>
      <div class="search-bar">
        <el-input v-model="tenantSearch" placeholder="搜索租客姓名/电话" clearable style="width: 220px;" @keyup.enter="loadTenants" />
        <el-button type="primary" @click="loadTenants">查询</el-button>
        <el-button @click="showNewTenant = true">新增租客</el-button>
      </div>
      <el-row :gutter="16" v-loading="tenantsLoading">
        <el-col :xs="12" :sm="8" :md="6" v-for="t in filteredTenants" :key="t.id" style="margin-bottom:12px;">
          <el-card :class="['tenant-card', { 'is-selected': selectedTenants.some(s => s.id === t.id) }]" shadow="hover" @click="toggleTenant(t)">
            <div class="tenant-name">{{ t.name }}</div>
            <div class="tenant-body">
              <p><label>证件：</label>{{ t.idCard || '-' }}</p>
              <p><label>电话：</label>{{ t.phone || '-' }}</p>
            </div>
            <div v-if="selectedTenants.some(s => s.id === t.id)" class="tenant-check">✓</div>
          </el-card>
        </el-col>
        <el-empty v-if="!tenantsLoading && filteredTenants.length === 0" description="未找到匹配的租客" />
      </el-row>
      <div v-if="selectedTenants.length > 0" style="margin:12px 0;">
        <span style="font-size:13px;color:#606266;">已选租客：</span>
        <el-tag v-for="t in selectedTenants" :key="t.id" closable @close="removeSelectedTenant(t)" style="margin-right:6px;margin-top:4px;">
          {{ t.name }}（{{ t.phone || '无电话' }}）
        </el-tag>
      </div>
      <div style="text-align: right; margin-top: 16px;">
        <el-button @click="step = 0">上一步</el-button>
        <el-button type="primary" :disabled="selectedTenants.length === 0" @click="step = 2">下一步（{{ selectedTenants.length }}人）</el-button>
      </div>
    </el-card>

    <!-- Step 3: Rent & Deposit -->
    <el-card v-show="step === 2">
      <template #header>租金与押金</template>
      <el-row :gutter="24">
        <el-col :span="12">
          <el-card shadow="never" class="form-card">
            <template #header>费用设置</template>
            <el-form :model="contractForm" label-width="110px" label-position="left">
              <el-form-item label="月租金">
                <el-input-number v-model="contractForm.rentAmount" :min="0" :precision="2" style="width:180px;" />
                <span style="margin-left:10px;color:#909399;font-size:13px;">建议: ¥{{ selectedRoom?.standardRent || 0 }}</span>
              </el-form-item>
              <el-form-item label="押金">
                <el-input-number v-model="contractForm.depositAmount" :min="0" :precision="2" style="width:180px;" />
                <el-button text size="small" type="primary" style="margin-left:8px;" @click="contractForm.depositAmount = (contractForm.rentAmount || 0) * 2">= 2个月租金</el-button>
              </el-form-item>
              <el-form-item label="付款周期">
                <el-select v-model="contractForm.paymentCycle" style="width:180px;">
                  <el-option label="月付" value="Monthly" />
                  <el-option label="季付" value="Quarterly" />
                  <el-option label="半年付" value="HalfYearly" />
                  <el-option label="年付" value="Yearly" />
                </el-select>
              </el-form-item>
              <el-form-item label="付款到期日">
                <el-select v-model="contractForm.paymentDueDay" style="width:180px;">
                  <el-option v-for="d in 28" :key="d" :label="'每月' + d + '日'" :value="d" />
                </el-select>
              </el-form-item>
              <el-form-item label="押金抵最后月租">
                <el-switch v-model="contractForm.allowDepositAsLastRent" />
              </el-form-item>
            </el-form>
          </el-card>
        </el-col>
        <el-col :span="12">
          <el-card shadow="never" class="form-card">
            <template #header>租期设置</template>
            <el-form :model="contractForm" label-width="110px" label-position="left">
              <el-form-item label="起租日期">
                <el-date-picker v-model="contractForm.startDate" type="date" style="width:100%;" />
              </el-form-item>
              <el-form-item label="到期日期">
                <el-date-picker v-model="contractForm.endDate" type="date" style="width:100%;" />
              </el-form-item>
              <el-form-item v-if="contractForm.startDate && contractForm.endDate" label="租期">
                <el-tag type="info">{{ calcMonths(contractForm.startDate, contractForm.endDate) }} 个月</el-tag>
                <span style="margin-left:8px;color:#909399;font-size:13px;">
                  共 ¥{{ ((contractForm.rentAmount || 0) * calcMonths(contractForm.startDate, contractForm.endDate)).toLocaleString() }}
                </span>
              </el-form-item>
            </el-form>
          </el-card>
          <el-card shadow="never" class="form-card" style="margin-top:12px;">
            <template #header>费用汇总</template>
            <div class="summary-row"><span>月租金</span><span>¥{{ (contractForm.rentAmount || 0).toLocaleString() }}</span></div>
            <div class="summary-row"><span>押金</span><span>¥{{ (contractForm.depositAmount || 0).toLocaleString() }}</span></div>
            <div class="summary-row" v-if="contractForm.startDate && contractForm.endDate">
              <span>合同总额</span><span>¥{{ ((contractForm.rentAmount || 0) * calcMonths(contractForm.startDate, contractForm.endDate) + (contractForm.depositAmount || 0)).toLocaleString() }}</span>
            </div>
          </el-card>
        </el-col>
      </el-row>
      <div style="text-align: right; margin-top: 16px;">
        <el-button @click="step = 1">上一步</el-button>
        <el-button type="primary" @click="step = 3">下一步</el-button>
      </div>
    </el-card>

    <!-- Step 4: Fee Config -->
    <el-card v-show="step === 3">
      <template #header>
        费用配置
        <span style="font-size:13px;color:#909399;font-weight:normal;margin-left:12px;">勾选需要绑定的收费项目并设置金额</span>
      </template>
      <el-alert title="租金和押金已在上一步设置，此处配置其他附加费用" type="info" :closable="false" show-icon style="margin-bottom:16px;" />
      <el-row :gutter="16">
        <el-col :xs="12" :sm="8" :md="6" v-for="fee in feeCodesList.filter(f => f.code !== 'RENT' && f.code !== 'DEPOSIT')" :key="fee.id" style="margin-bottom:12px;">
          <el-card :class="['fee-card', { 'is-selected': fee._enabled }]" shadow="hover" @click="toggleFee(fee)">
            <div v-if="fee._enabled" class="fee-check">✓</div>
            <div class="fee-card-header">
              <span class="fee-name">{{ fee.name || fee.feeName }}</span>
            </div>
            <div class="fee-card-body">
              <p><label>类型：</label>{{ fee.chargeType === 'OneTime' ? '一次性' : '周期性' }}</p>
              <p><label>方式：</label>{{ fee.billingMode === 'MeterBased' ? '按表计量' : '固定金额' }}</p>
              <template v-if="fee._enabled">
                <el-divider style="margin:8px 0;" />
                <div @click.stop>
                  <el-form-item label="金额" style="margin-bottom:0;">
                    <el-input-number v-model="fee._amount" :min="0" :precision="2" size="small" style="width:140px;" controls-position="right" />
                  </el-form-item>
                </div>
              </template>
            </div>
          </el-card>
        </el-col>
        <el-empty v-if="feeCodesList.length === 0" description="暂无可用收费项目" style="margin-top:20px;" />
      </el-row>
      <div style="text-align: right; margin-top: 16px;">
        <el-button @click="step = 2">上一步</el-button>
        <el-button type="primary" @click="step = 4">下一步</el-button>
      </div>
    </el-card>

    <!-- Step 5: Confirm -->
    <el-card v-show="step === 4">
      <template #header>确认信息</template>
      <el-descriptions :column="2" border>
        <el-descriptions-item label="房屋">{{ selectedRoom?.fullCode }}</el-descriptions-item>
        <el-descriptions-item label="租客">{{ selectedTenants.map(t => t.name).join('、') }}</el-descriptions-item>
        <el-descriptions-item label="起租日期">{{ fmtDate(contractForm.startDate) }}</el-descriptions-item>
        <el-descriptions-item label="到期日期">{{ fmtDate(contractForm.endDate) }}</el-descriptions-item>
      </el-descriptions>

      <h4 style="margin:16px 0 8px;">费用明细</h4>
      <el-table :data="allFeeItems" stripe style="margin-top:12px;">
        <el-table-column prop="name" label="收费项目" min-width="140" />
        <el-table-column label="金额" width="180" align="right">
          <template #default="{ row }" style="font-size:15px;">
            <span style="font-weight:bold;">¥{{ (row.amount || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="type" label="类型" width="100" align="center" />
        <el-table-column prop="mode" label="计费方式" width="120" align="center" />
      </el-table>

      <div style="text-align: center; margin-top: 24px;">
        <el-button @click="step = 3">上一步</el-button>
        <el-button type="primary" size="large" @click="submitContract" :loading="submitting">
          提交审批
        </el-button>
      </div>
    </el-card>

    <!-- New Tenant Dialog -->
    <el-dialog v-model="showNewTenant" title="新增租客" width="450px">
      <el-form :model="newTenantForm" label-width="100px">
        <el-form-item label="姓名">
          <el-input v-model="newTenantForm.name" />
        </el-form-item>
        <el-form-item label="身份证号">
          <el-input v-model="newTenantForm.idCard" />
        </el-form-item>
        <el-form-item label="电话">
          <el-input v-model="newTenantForm.phone" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showNewTenant = false">取消</el-button>
        <el-button type="primary" :loading="creatingTenant" @click="addNewTenant">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/user'
import { getHousingUnits, getTenants, getFeeCodes, createTenant, submitContractCreateRequest } from '@/api'

const router = useRouter()
const userStore = useUserStore()
const step = ref(0)
const submitting = ref(false)
const creatingTenant = ref(false)

const roomSearch = ref('')
const tenantSearch = ref('')
const selectedRoom = ref(null)
const selectedTenants = ref([])

function toggleTenant(t) {
  const idx = selectedTenants.value.findIndex(x => x.id === t.id)
  if (idx >= 0) selectedTenants.value.splice(idx, 1)
  else selectedTenants.value.push(t)
}
function removeSelectedTenant(t) {
  selectedTenants.value = selectedTenants.value.filter(x => x.id !== t.id)
}
const showNewTenant = ref(false)
const roomsLoading = ref(false)
const tenantsLoading = ref(false)

const newTenantForm = reactive({ name: '', idCard: '', phone: '' })

const allRooms = ref([])
const allTenants = ref([])
const feeCodesList = ref([])

const contractForm = reactive({
  rentAmount: null, depositAmount: null,
  startDate: fmtDate(new Date()), endDate: '', paymentDueDay: 5,
  paymentCycle: 'Monthly', allowDepositAsLastRent: false
})

function calcMonths(start, end) {
  if (!start || !end) return 0
  const s = new Date(start), e = new Date(end)
  return (e.getFullYear() - s.getFullYear()) * 12 + e.getMonth() - s.getMonth() + 1
}
function fmtDate(d) {
  if (!d) return '-'
  const dt = new Date(d)
  if (isNaN(dt)) return d
  const y = dt.getFullYear()
  const m = String(dt.getMonth() + 1).padStart(2, '0')
  const day = String(dt.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

const filteredRooms = computed(() => {
  let list = allRooms.value
  if (roomSearch.value) {
    const kw = roomSearch.value.toLowerCase()
    list = list.filter(r => (r.fullCode || '').toLowerCase().includes(kw))
  }
  return list
})

const filteredTenants = computed(() => {
  let list = allTenants.value
  if (tenantSearch.value) {
    const kw = tenantSearch.value.toLowerCase()
    list = list.filter(t => (t.name || '').toLowerCase().includes(kw) || (t.phone || '').includes(kw))
  }
  return list
})

async function loadRooms() {
  roomsLoading.value = true
  try {
    const res = await getHousingUnits({ pageSize: 500 })
    const items = res.items || res.data || res || []
    allRooms.value = items.filter(u => u.status === 'Vacant').map(u => ({
      id: u.id,
      fullCode: u.fullCode || u.name || '',
      buildingName: u.buildingName || '',
      roomNo: u.unitNo || '',
      roomTypeName: u.roomTypeName || '',
      area: u.area || 0,
      standardRent: u.baseRentAmount || 0
    }))
  } catch { ElMessage.error('加载房屋数据失败') }
  roomsLoading.value = false
}

async function loadTenants() {
  tenantsLoading.value = true
  try {
    const res = await getTenants({ keyword: tenantSearch.value || undefined })
    const items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    allTenants.value = items.map(t => ({
      id: t.id,
      name: t.name || '',
      idCard: t.idCard || '',
      phone: t.phone || ''
    }))
  } catch { /* 静默 */ }
  tenantsLoading.value = false
}

async function loadFeeCodes() {
  try {
    const res = await getFeeCodes({})
    const items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    feeCodesList.value = items.map(f => ({ ...f, _enabled: false, _amount: f.defaultPrice || 0 }))
  } catch { /* 静默 */ }
}

function toggleFee(fee) { fee._enabled = !fee._enabled }

const selectedFees = computed(() => feeCodesList.value.filter(f => f._enabled))

const allFeeItems = computed(() => {
  const items = [
    { name: '租金', amount: contractForm.rentAmount || 0, type: '周期性', mode: '月付' },
    { name: '押金', amount: contractForm.depositAmount || 0, type: '一次性', mode: '固定金额' }
  ]
  for (const f of selectedFees.value) {
    items.push({
      name: f.name || f.feeName,
      amount: f._amount || 0,
      type: f.chargeType === 'OneTime' ? '一次性' : '周期性',
      mode: f.billingMode === 'MeterBased' ? '按表计量' : '固定金额'
    })
  }
  return items.filter(i => i.amount > 0)
})

function selectRoom(row) {
  selectedRoom.value = row
  if (row) {
    contractForm.rentAmount = row.standardRent || 0
    contractForm.depositAmount = row.standardRent || 0
  }
}

async function addNewTenant() {
  if (!newTenantForm.name) { ElMessage.warning('请输入姓名'); return }
  const companyId = userStore.effectiveCompanyId || userStore.homeCompanyId
  if (!companyId) { ElMessage.warning('请先选择公司'); return }
  creatingTenant.value = true
  try {
    const res = await createTenant({
      name: newTenantForm.name,
      idCard: newTenantForm.idCard || undefined,
      phone: newTenantForm.phone || undefined,
      companyId
    })
    allTenants.value.push({
      id: res.id,
      name: res.name,
      idCard: res.idCard || '',
      phone: res.phone || ''
    })
    ElMessage.success('租客已创建')
    showNewTenant.value = false
    newTenantForm.name = ''; newTenantForm.idCard = ''; newTenantForm.phone = ''
  } catch { ElMessage.error('创建租客失败') }
  creatingTenant.value = false
}

function feeCodeId(code) {
  const f = feeCodesList.value.find(f => f.code === code)
  return f ? f.id : null
}

async function submitContract() {
  if (!selectedRoom.value || selectedTenants.value.length === 0) { ElMessage.warning('请完成所有步骤'); return }
  const companyId = userStore.effectiveCompanyId || userStore.homeCompanyId
  if (!companyId) { ElMessage.warning('请先选择公司'); return }
  if (!contractForm.startDate) { ElMessage.warning('请填写起租日期'); return }

  const fees = []
  const rentId = feeCodeId('RENT')
  if (rentId && contractForm.rentAmount > 0)
    fees.push({ feeCodeId: rentId, amount: contractForm.rentAmount, billingMode: 'FixedAmount', chargeType: 'Recurring' })
  const depositId = feeCodeId('DEPOSIT')
  if (depositId && contractForm.depositAmount > 0)
    fees.push({ feeCodeId: depositId, amount: contractForm.depositAmount, billingMode: 'FixedAmount', chargeType: 'OneTime' })
  for (const f of selectedFees.value) {
    fees.push({ feeCodeId: f.id, amount: f._amount || 0, billingMode: f.billingMode === 'MeterBased' ? 'MeterBased' : 'FixedAmount', chargeType: f.chargeType === 'OneTime' ? 'OneTime' : 'Recurring' })
  }

  submitting.value = true
  try {
    const payload = {
      roomId: selectedRoom.value.id,
      startDate: fmtDate(contractForm.startDate),
      paymentCycle: contractForm.paymentCycle || 'Monthly',
      companyId,
      tenantIds: selectedTenants.value.map(t => t.id),
      fees
    }
    if (contractForm.endDate) payload.endDate = fmtDate(contractForm.endDate)
    const res = await submitContractCreateRequest(payload)
    if (res.status === 'Active') {
      ElMessage.success('合同已直接激活')
      router.push('/contracts/' + res.contractId)
    } else {
      ElMessage.success('合同已提交审批')
      router.push('/approvals/myrequests')
    }
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  }
  submitting.value = false
}

onMounted(() => {
  loadRooms()
  loadTenants()
  loadFeeCodes()
})
</script>

<style scoped>
.room-card { cursor: pointer; transition: all 0.2s; position: relative; }
.room-card:hover { border-color: #409eff; }
.room-card.is-selected { border-color: #67c23a; background: #f0f9eb; }
.room-code { font-weight: bold; font-size: 15px; margin-bottom: 8px; }
.room-card-body p { margin: 4px 0; font-size: 13px; color: #606266; }
.room-card-body label { color: #909399; }
.room-rent { margin-top: 8px !important; font-size: 18px !important; font-weight: bold; color: #e6a23c; }
.room-rent span { font-size: 12px; font-weight: normal; color: #909399; }
.room-check { position: absolute; top: 8px; right: 10px; color: #67c23a; font-weight: bold; font-size: 18px; }
.form-card { border: 1px solid #ebeef5; border-radius: 4px; }
.form-card :deep(.el-card__header) { padding: 10px 16px; font-weight: bold; background: #fafafa; }
.form-card :deep(.el-card__body) { padding: 16px; }
.summary-row { display: flex; justify-content: space-between; padding: 6px 0; font-size: 14px; border-bottom: 1px dashed #ebeef5; }
.summary-row:last-child { border-bottom: none; font-weight: bold; }
.tenant-card { cursor: pointer; position: relative; transition: all 0.2s; }
.tenant-card:hover { border-color: #409eff; }
.tenant-card.is-selected { border-color: #67c23a; background: #f0f9eb; }
.tenant-name { font-weight: bold; font-size: 15px; margin-bottom: 8px; }
.tenant-body p { margin: 4px 0; font-size: 13px; color: #606266; }
.tenant-body label { color: #909399; }
.tenant-check { position: absolute; top: 8px; right: 10px; color: #67c23a; font-weight: bold; font-size: 18px; }
.fee-card { cursor: pointer; transition: all 0.2s; position: relative; }
.fee-card:hover { border-color: #409eff; }
.fee-check { position: absolute; top: 8px; right: 10px; color: #67c23a; font-weight: bold; font-size: 18px; z-index: 1; }
.fee-card.is-selected { border-color: #67c23a; background: #f0f9eb; }
.fee-card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px; }
.fee-name { font-weight: bold; font-size: 14px; }
.fee-card-body p { margin: 3px 0; font-size: 13px; color: #606266; }
.fee-card-body label { color: #909399; }
.search-bar { margin-bottom: 16px; }
</style>
