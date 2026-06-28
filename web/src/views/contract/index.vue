<template>
  <div>
    <div class="page-header">
      <h2>合同管理</h2>
      <el-button type="primary" @click="$router.push('/contracts/create')">
        <el-icon><Plus /></el-icon>新建合同
      </el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="合同号/租客名/房间号" clearable style="width: 220px;" />
      <el-select v-model="search.status" placeholder="合同状态" clearable style="width: 140px;">
        <el-option label="草稿" value="Draft" />
        <el-option label="待审批" value="PendingApproval" />
        <el-option label="活跃" value="Active" />
        <el-option label="已暂停" value="Suspended" />
        <el-option label="已到期" value="Expired" />
        <el-option label="已终止" value="Terminated" />
        <el-option label="已续签" value="Renewed" />
      </el-select>
      <el-date-picker v-model="search.dateRange" type="daterange" range-separator="至" start-placeholder="到期开始" end-placeholder="到期结束" style="width: 240px;" />
      <el-button type="primary" @click="handleSearch">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-table :data="contractList" stripe style="width: 100%">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="contractNo" label="合同号" width="150" />
      <el-table-column prop="roomName" label="房屋" width="100" />
      <el-table-column prop="tenantName" label="租客" width="100" />
      <el-table-column prop="rentAmount" label="月租金" width="100">
        <template #default="{ row }">¥{{ row.rentAmount?.toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="startDate" label="起租" width="95" />
      <el-table-column prop="endDate" label="到期" width="95" />
      <el-table-column prop="status" label="状态" width="95">
        <template #default="{ row }">
          <el-tag :type="statusTypeMap[row.status] || 'info'" size="small">{{ statusLabelMap[row.status] || row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="240" fixed="right">
        <template #default="{ row }">
          <div style="display: flex; gap: 2px; flex-wrap: wrap;">
            <el-button text size="small" type="primary" @click="$router.push('/contracts/' + row.id)">详情</el-button>
            <el-button v-if="row.status === 'Active' || row.status === 'Suspended'" text size="small" type="warning" @click="showModifyRent(row)">调租</el-button>
            <el-button v-if="row.status === 'Active' || row.status === 'Suspended'" text size="small" type="warning" @click="showModifyFee(row)">调价</el-button>
            <el-button v-if="row.status === 'Active'" text size="small" type="primary" @click="handleRenew(row)">续签</el-button>
            <el-button v-if="row.status === 'Active'" text size="small" type="danger" @click="handleTerminate(row)">终止</el-button>
            <el-button v-if="row.status === 'Active'" text size="small" type="warning" @click="handleSuspend(row)">暂停</el-button>
            <el-button v-if="row.status === 'Suspended'" text size="small" type="success" @click="handleResume(row)">恢复</el-button>
            <el-button v-if="row.status === 'Expired'" text size="small" type="primary" @click="handleRenew(row)">续签</el-button>
          </div>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" :page-sizes="[10, 20, 50]" layout="total, sizes, prev, pager, next" />
    </div>

    <!-- Terminate Dialog -->
    <el-dialog v-model="showTerminate" title="合同终止" width="500px">
      <el-form :model="terminateForm" label-width="100px">
        <el-form-item label="终止类型">
          <el-radio-group v-model="terminateForm.type">
            <el-radio label="EXPIRED">到期终止</el-radio>
            <el-radio label="EARLY">提前解约</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="实际终止日">
          <el-date-picker v-model="terminateForm.actualEndDate" type="date" />
        </el-form-item>
        <el-form-item label="终止原因">
          <el-input v-model="terminateForm.reason" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showTerminate = false">取消</el-button>
        <el-button type="primary" @click="submitTerminate">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Modify Rent Dialog -->
    <el-dialog v-model="showModifyRentDialog" title="合同租金调整" width="550px">
      <el-alert title="租金调整需要经过审批，金额越大审批级别越高。" type="info" show-icon :closable="false" style="margin-bottom: 16px;" />
      <el-form :model="modifyRentForm" label-width="120px">
        <el-descriptions :column="2" border style="margin-bottom: 16px;">
          <el-descriptions-item label="合同号">{{ modifyRentTarget?.contractNo }}</el-descriptions-item>
          <el-descriptions-item label="当前月租">¥{{ modifyRentTarget?.rentAmount?.toLocaleString() }}</el-descriptions-item>
        </el-descriptions>
        <el-form-item label="新租金 (元/月)">
          <el-input-number v-model="modifyRentForm.newRentAmount" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="调整差额">
          <span :style="{ color: rentDiff >= 0 ? '#f56c6c' : '#67c23a', fontWeight: 'bold', fontSize: '16px' }">
            {{ rentDiff >= 0 ? '+' : '' }}¥{{ rentDiff.toLocaleString() }}
          </span>
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="modifyRentForm.effectiveDate" type="date" />
        </el-form-item>
        <el-form-item label="调整原因">
          <el-input v-model="modifyRentForm.reason" type="textarea" :rows="3" placeholder="请说明调价原因，如：市场行情变化、合同约定涨幅等" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showModifyRentDialog = false">取消</el-button>
        <el-button type="primary" @click="submitModifyRent">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Modify Fee Dialog -->
    <el-dialog v-model="showModifyFeeDialog" title="合同费用调价" width="600px">
      <el-alert title="费用中途调价需要运营主管审批。" type="info" show-icon :closable="false" style="margin-bottom: 16px;" />
      <el-descriptions :column="2" border style="margin-bottom: 16px;">
        <el-descriptions-item label="合同号">{{ modifyFeeTarget?.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="租客">{{ modifyFeeTarget?.tenantName }}</el-descriptions-item>
      </el-descriptions>
      <el-table :data="modifyFeeForm.items" stripe>
        <el-table-column prop="feeName" label="收费项目" width="110" />
        <el-table-column prop="chargeMethod" label="计费方式" width="90" />
        <el-table-column label="当前价格" width="110">
          <template #default="{ row }">{{ row.oldPrice }}</template>
        </el-table-column>
        <el-table-column label="新价格" width="120">
          <template #default="{ row }">
            <el-input-number v-model="row.newPrice" :min="0" :precision="row.chargeMethod === '按表计量' ? 4 : 2" size="small" style="width: 100px;" />
          </template>
        </el-table-column>
        <el-table-column label="涨幅" width="80">
          <template #default="{ row }">
            <span :style="{ color: row.newPrice > row.oldPriceVal ? '#f56c6c' : row.newPrice < row.oldPriceVal ? '#67c23a' : '#909399' }">
              {{ row.newPrice > row.oldPriceVal ? '↑' : row.newPrice < row.oldPriceVal ? '↓' : '-' }}
              {{ row.oldPriceVal ? Math.abs((row.newPrice - row.oldPriceVal) / row.oldPriceVal * 100).toFixed(1) + '%' : '' }}
            </span>
          </template>
        </el-table-column>
      </el-table>
      <el-form style="margin-top: 12px;">
        <el-form-item label="生效日期">
          <el-date-picker v-model="modifyFeeForm.effectiveDate" type="date" />
        </el-form-item>
        <el-form-item label="调价原因">
          <el-input v-model="modifyFeeForm.reason" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showModifyFeeDialog = false">取消</el-button>
        <el-button type="primary" @click="submitModifyFee">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Renew Dialog -->
    <el-dialog v-model="showRenewDialog" title="合同续签" width="550px">
      <el-alert title="续签条件与原合同一致时可简化审批。" type="success" show-icon :closable="false" style="margin-bottom: 16px;" />
      <el-form :model="renewForm" label-width="120px">
        <el-descriptions :column="2" border style="margin-bottom: 16px;">
          <el-descriptions-item label="原合同号">{{ renewTarget?.contractNo }}</el-descriptions-item>
          <el-descriptions-item label="当前月租">¥{{ renewTarget?.rentAmount?.toLocaleString() }}</el-descriptions-item>
          <el-descriptions-item label="原到期日">{{ renewTarget?.endDate }}</el-descriptions-item>
        </el-descriptions>
        <el-form-item label="新合同月租">
          <el-input-number v-model="renewForm.rentAmount" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="新起租日期">
          <el-date-picker v-model="renewForm.startDate" type="date" />
        </el-form-item>
        <el-form-item label="新到期日期">
          <el-date-picker v-model="renewForm.endDate" type="date" />
        </el-form-item>
        <el-form-item label="押金">
          <el-input-number v-model="renewForm.depositAmount" :min="0" :precision="2" style="width: 200px;" />
          <span style="margin-left: 8px; color: #909399;">原押金: ¥{{ renewTarget?.depositAmount?.toLocaleString() || 0 }}</span>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="renewForm.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showRenewDialog = false">取消</el-button>
        <el-button type="primary" @click="submitRenew">提交续签</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()

const search = reactive({ keyword: '', status: '', dateRange: null })
const pagination = reactive({ page: 1, pageSize: 10, total: 25 })

const statusTypeMap = {
  'Draft': 'info', 'PendingApproval': 'warning', 'Active': 'success',
  'Suspended': '', 'Expired': 'danger', 'Terminated': 'danger', 'Renewed': 'primary'
}
const statusLabelMap = {
  'Draft': '草稿', 'PendingApproval': '待审批', 'Active': '活跃',
  'Suspended': '已暂停', 'Expired': '已到期', 'Terminated': '已终止', 'Renewed': '已续签'
}

const contractList = ref([
  { id: 'c1', contractNo: 'HT-2026-001', roomName: 'A栋-101', tenantName: '张三', rentAmount: 5200, depositAmount: 10400, startDate: '2026-01-01', endDate: '2027-12-31', status: 'Active' },
  { id: 'c2', contractNo: 'HT-2026-002', roomName: 'A栋-102', tenantName: '李四', rentAmount: 3800, depositAmount: 7600, startDate: '2026-02-01', endDate: '2027-01-31', status: 'Active' },
  { id: 'c3', contractNo: 'HT-2026-003', roomName: 'B栋-201', tenantName: '王五', rentAmount: 6800, depositAmount: 13600, startDate: '2026-03-15', endDate: '2027-03-14', status: 'Active' },
  { id: 'c4', contractNo: 'HT-2026-004', roomName: 'B栋-202', tenantName: '赵六', rentAmount: 5000, depositAmount: 10000, startDate: '2026-01-01', endDate: '2026-06-30', status: 'Expired' },
  { id: 'c5', contractNo: 'HT-2026-005', roomName: 'A栋-201', tenantName: '孙七', rentAmount: 4500, depositAmount: 9000, startDate: '2026-04-01', endDate: '2027-03-31', status: 'PendingApproval' }
])

// === Terminate ===
const showTerminate = ref(false)
const terminateForm = reactive({ type: 'EARLY', actualEndDate: '', reason: '' })
const currentContract = ref(null)

// === Modify Rent ===
const showModifyRentDialog = ref(false)
const modifyRentTarget = ref(null)
const modifyRentForm = reactive({ newRentAmount: 0, effectiveDate: '', reason: '' })

const rentDiff = computed(() => {
  const old = modifyRentTarget.value?.rentAmount || 0
  return modifyRentForm.newRentAmount - old
})

// === Modify Fee ===
const showModifyFeeDialog = ref(false)
const modifyFeeTarget = ref(null)
const modifyFeeForm = reactive({ items: [], effectiveDate: '', reason: '' })

// === Renew ===
const showRenewDialog = ref(false)
const renewTarget = ref(null)
const renewForm = reactive({ rentAmount: 0, startDate: '', endDate: '', depositAmount: 0, remark: '' })

// === Event Handlers ===
function handleSearch() { ElMessage.info('搜索功能待API接入') }
function resetSearch() { search.keyword = ''; search.status = ''; search.dateRange = null }

function handleTerminate(row) {
  currentContract.value = row
  showTerminate.value = true
}
function submitTerminate() {
  ElMessage.success('合同终止申请已提交审批，请等待各级审批人审批')
  showTerminate.value = false
}

function handleSuspend(row) {
  ElMessageBox.confirm(`确定暂停合同 ${row.contractNo} 吗？暂停期间不生成应收。`, '提示').then(() => {
    row.status = 'Suspended'
    ElMessage.success('合同已暂停')
  }).catch(() => {})
}
function handleResume(row) {
  ElMessageBox.confirm(`确定恢复合同 ${row.contractNo} 吗？恢复后将正常生成应收。`, '提示').then(() => {
    row.status = 'Active'
    ElMessage.success('合同已恢复')
  }).catch(() => {})
}

// === Modify Rent ===
function showModifyRent(row) {
  modifyRentTarget.value = row
  modifyRentForm.newRentAmount = row.rentAmount
  modifyRentForm.effectiveDate = ''
  modifyRentForm.reason = ''
  showModifyRentDialog.value = true
}
function submitModifyRent() {
  if (!modifyRentForm.newRentAmount || modifyRentForm.newRentAmount <= 0) {
    ElMessage.warning('请输入有效的租金金额')
    return
  }
  if (!modifyRentForm.reason) {
    ElMessage.warning('请填写调整原因')
    return
  }
  const diff = modifyRentForm.newRentAmount - (modifyRentTarget.value?.rentAmount || 0)
  const approvalLevel = Math.abs(diff) > 5000 ? '2级(部门经理)' : '1级(运营主管)'
  ElMessage.success(`租金调整申请已提交${approvalLevel}审批，等待审批人处理`)
  showModifyRentDialog.value = false
}

// === Modify Fee ===
function showModifyFee(row) {
  modifyFeeTarget.value = row
  // Mock fee configs for the target contract
  modifyFeeForm.items = [
    { feeName: '房租费', chargeMethod: '固定金额', oldPrice: '¥' + row.rentAmount.toLocaleString(), oldPriceVal: row.rentAmount, newPrice: row.rentAmount },
    { feeName: '水费', chargeMethod: '按表计量', oldPrice: '6.00 元/吨', oldPriceVal: 6.00, newPrice: 6.00 },
    { feeName: '电费', chargeMethod: '按表计量', oldPrice: '0.80 元/度', oldPriceVal: 0.80, newPrice: 0.80 },
    { feeName: '管理费', chargeMethod: '固定金额', oldPrice: '¥150', oldPriceVal: 150, newPrice: 150 },
    { feeName: '网费', chargeMethod: '固定金额', oldPrice: '¥80', oldPriceVal: 80, newPrice: 80 }
  ]
  modifyFeeForm.effectiveDate = ''
  modifyFeeForm.reason = ''
  showModifyFeeDialog.value = true
}
function submitModifyFee() {
  if (!modifyFeeForm.reason) {
    ElMessage.warning('请填写调价原因')
    return
  }
  ElMessage.success('费用调价申请已提交运营主管审批')
  showModifyFeeDialog.value = false
}

// === Renew ===
function handleRenew(row) {
  renewTarget.value = row
  renewForm.rentAmount = row.rentAmount
  renewForm.depositAmount = row.depositAmount || 0
  renewForm.remark = ''
  // Set default dates
  const endDate = new Date(row.endDate)
  const nextDay = new Date(endDate)
  nextDay.setDate(nextDay.getDate() + 1)
  renewForm.startDate = nextDay.toISOString().split('T')[0]
  const newEnd = new Date(nextDay)
  newEnd.setFullYear(newEnd.getFullYear() + 1)
  newEnd.setDate(newEnd.getDate() - 1)
  renewForm.endDate = newEnd.toISOString().split('T')[0]
  showRenewDialog.value = true
}
function submitRenew() {
  if (!renewForm.rentAmount || !renewForm.startDate || !renewForm.endDate) {
    ElMessage.warning('请填写完整的续签信息')
    return
  }
  ElMessage.success('续签合同已创建，等待运营主管审批')
  showRenewDialog.value = false
}
</script>
