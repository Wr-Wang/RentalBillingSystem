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
        <el-input v-model="roomSearch" placeholder="搜索房间号/地址" clearable style="width: 200px;" />
        <el-select v-model="selectedBuilding" placeholder="选择座楼" clearable style="width: 140px;">
          <el-option label="A栋" value="A" />
          <el-option label="B栋" value="B" />
          <el-option label="C栋" value="C" />
        </el-select>
      </div>
      <el-table :data="availableRooms" stripe @row-click="selectRoom" highlight-current-row>
        <el-table-column type="selection" width="50" />
        <el-table-column prop="fullCode" label="房间编号" />
        <el-table-column prop="buildingName" label="座楼" />
        <el-table-column prop="roomNo" label="房间号" />
        <el-table-column prop="roomTypeName" label="房型" />
        <el-table-column prop="area" label="面积(m²)" />
        <el-table-column prop="standardRent" label="标准租金">
          <template #default="{ row }">¥{{ row.standardRent?.toLocaleString() }}</template>
        </el-table-column>
      </el-table>
      <div style="text-align: right; margin-top: 16px;">
        <el-button type="primary" :disabled="!selectedRoom" @click="step = 1">下一步</el-button>
      </div>
    </el-card>

    <!-- Step 2: Select Tenant -->
    <el-card v-show="step === 1">
      <template #header>选择租客</template>
      <div class="search-bar">
        <el-input v-model="tenantSearch" placeholder="搜索租客姓名/电话" clearable style="width: 220px;" />
        <el-button @click="showNewTenant = true">新增租客</el-button>
      </div>
      <el-table :data="tenantList" stripe @row-click="selectTenant" highlight-current-row>
        <el-table-column type="selection" width="50" />
        <el-table-column prop="name" label="姓名" />
        <el-table-column prop="identityNo" label="身份证号" />
        <el-table-column prop="phone" label="电话" />
      </el-table>
      <div style="text-align: right; margin-top: 16px;">
        <el-button @click="step = 0">上一步</el-button>
        <el-button type="primary" :disabled="!selectedTenant" @click="step = 2">下一步</el-button>
      </div>
    </el-card>

    <!-- Step 3: Rent & Deposit -->
    <el-card v-show="step === 2">
      <template #header>租金与押金</template>
      <el-form :model="contractForm" label-width="120px">
        <el-form-item label="月租金 (元)">
          <el-input-number v-model="contractForm.rentAmount" :min="0" :precision="2" style="width: 200px;" />
          <span style="margin-left: 8px; color: #909399;">建议价: ¥{{ selectedRoom?.standardRent || 0 }}</span>
        </el-form-item>
        <el-form-item label="押金 (元)">
          <el-input-number v-model="contractForm.depositAmount" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="起租日期">
          <el-date-picker v-model="contractForm.startDate" type="date" />
        </el-form-item>
        <el-form-item label="到期日期">
          <el-date-picker v-model="contractForm.endDate" type="date" />
        </el-form-item>
        <el-form-item label="付款到期日">
          <el-select v-model="contractForm.paymentDueDay" style="width: 200px;">
            <el-option v-for="d in 28" :key="d" :label="'每月' + d + '日'" :value="d" />
          </el-select>
        </el-form-item>
        <el-form-item label="押金抵最后月租">
          <el-switch v-model="contractForm.allowDepositAsLastRent" />
        </el-form-item>
      </el-form>
      <div style="text-align: right; margin-top: 16px;">
        <el-button @click="step = 1">上一步</el-button>
        <el-button type="primary" @click="step = 3">下一步</el-button>
      </div>
    </el-card>

    <!-- Step 4: Fee Config -->
    <el-card v-show="step === 3">
      <template #header>费用配置</template>
      <p style="color: #909399; margin-bottom: 16px;">
        配置合同绑定的收费项目和金额/单价
      </p>
      <el-table :data="feeConfigs" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="feeName" label="收费项目" />
        <el-table-column prop="chargeMethod" label="计费方式" />
        <el-table-column label="金额/单价">
          <template #default="{ row, $index }">
            <el-input-number v-model="row.unitPrice" :min="0" :precision="row.chargeMethod === '固定金额' ? 2 : 4" style="width: 160px;" />
            <span v-if="row.chargeMethod === '按表计量'" style="margin-left: 8px;">{{ row.unit }}</span>
          </template>
        </el-table-column>
        <el-table-column label="初始读数" v-if="hasMeterBased">
          <template #default="{ row }">
            <el-input-number v-if="row.chargeMethod === '按表计量'" v-model="row.initialReading" :min="0" :precision="2" style="width: 120px;" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="80">
          <template #default="{ row, $index }">
            <el-switch v-model="row.enabled" />
          </template>
        </el-table-column>
      </el-table>
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
        <el-descriptions-item label="租客">{{ selectedTenant?.name }}</el-descriptions-item>
        <el-descriptions-item label="月租金">¥{{ contractForm.rentAmount }}</el-descriptions-item>
        <el-descriptions-item label="押金">¥{{ contractForm.depositAmount }}</el-descriptions-item>
        <el-descriptions-item label="起租日期">{{ contractForm.startDate }}</el-descriptions-item>
        <el-descriptions-item label="到期日期">{{ contractForm.endDate }}</el-descriptions-item>
      </el-descriptions>

      <el-table :data="feeConfigs.filter(f => f.enabled)" stripe style="margin-top: 16px;">
        <el-table-column prop="feeName" label="收费项目" />
        <el-table-column prop="unitPrice" label="金额/单价">
          <template #default="{ row }">{{ row.chargeMethod === '固定金额' ? '¥' + row.unitPrice : row.unitPrice }}</template>
        </el-table-column>
        <el-table-column prop="chargeMethod" label="计费方式" />
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
          <el-input v-model="newTenantForm.identityNo" />
        </el-form-item>
        <el-form-item label="电话">
          <el-input v-model="newTenantForm.phone" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showNewTenant = false">取消</el-button>
        <el-button type="primary" @click="addNewTenant">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const router = useRouter()
const step = ref(0)
const submitting = ref(false)

const roomSearch = ref('')
const tenantSearch = ref('')
const selectedBuilding = ref('')
const selectedRoom = ref(null)
const selectedTenant = ref(null)
const showNewTenant = ref(false)

const newTenantForm = reactive({ name: '', identityNo: '', phone: '' })

const availableRooms = ref([
  { id: 'r1', fullCode: 'A-1-101', buildingName: 'A栋', roomNo: '101', roomTypeName: '两室一厅', area: 85, standardRent: 5200 },
  { id: 'r2', fullCode: 'A-1-102', buildingName: 'A栋', roomNo: '102', roomTypeName: '一室一厅', area: 55, standardRent: 3800 },
  { id: 'r3', fullCode: 'B-1-101', buildingName: 'B栋', roomNo: '101', roomTypeName: '两室一厅', area: 88, standardRent: 5000 }
])

const tenantList = ref([
  { id: 't1', name: '张三', identityNo: '110101199001011234', phone: '13800138001' },
  { id: 't2', name: '李四', identityNo: '110101199002021235', phone: '13800138002' },
  { id: 't3', name: '王五', identityNo: '110101199003031236', phone: '13800138003' }
])

const contractForm = reactive({
  rentAmount: 0, depositAmount: 0,
  startDate: '', endDate: '', paymentDueDay: 5,
  allowDepositAsLastRent: false
})

const feeConfigs = ref([
  { feeName: '房租费', chargeMethod: '固定金额', unitPrice: 5200, unit: '', enabled: true, initialReading: 0 },
  { feeName: '水费', chargeMethod: '按表计量', unitPrice: 6.00, unit: '元/吨', enabled: true, initialReading: 0 },
  { feeName: '电费', chargeMethod: '按表计量', unitPrice: 0.80, unit: '元/度', enabled: true, initialReading: 0 },
  { feeName: '卫生费', chargeMethod: '固定金额', unitPrice: 30, unit: '', enabled: true, initialReading: 0 },
  { feeName: '管理费', chargeMethod: '固定金额', unitPrice: 150, unit: '', enabled: true, initialReading: 0 },
  { feeName: '燃气费', chargeMethod: '按表计量', unitPrice: 3.50, unit: '元/方', enabled: true, initialReading: 0 },
  { feeName: '网费', chargeMethod: '固定金额', unitPrice: 80, unit: '', enabled: true, initialReading: 0 },
  { feeName: '电视费', chargeMethod: '固定金额', unitPrice: 25, unit: '', enabled: false, initialReading: 0 }
])

const hasMeterBased = computed(() => feeConfigs.value.some(f => f.chargeMethod === '按表计量' && f.enabled))

function selectRoom(row) { selectedRoom.value = row; contractForm.rentAmount = row.standardRent }
function selectTenant(row) { selectedTenant.value = row }

function addNewTenant() {
  tenantList.value.push({ id: 't' + Date.now(), ...newTenantForm })
  ElMessage.success('租客已添加')
  showNewTenant.value = false
}

function submitContract() {
  submitting.value = true
  setTimeout(() => {
    submitting.value = false
    ElMessage.success('合同创建成功，已提交审批')
    router.push('/contracts')
  }, 1000)
}
</script>
