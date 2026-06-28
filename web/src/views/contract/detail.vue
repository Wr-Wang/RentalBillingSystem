<template>
  <div>
    <div class="page-header">
      <h2>合同详情</h2>
      <div class="table-actions">
        <el-button v-if="isActive" type="warning" @click="showModifyRent = true">租金调整</el-button>
        <el-button v-if="isActive || contract.status === 'Suspended'" type="warning" @click="showModifyFee = true">费用调价</el-button>
        <el-button v-if="isActive" type="primary" @click="showRenew = true">续签</el-button>
        <el-button v-if="isActive" type="danger" @click="showTerminate = true">终止合同</el-button>
        <el-button v-if="isActive" type="warning" @click="showSuspend = true">暂停</el-button>
        <el-button v-if="contract.status === 'Suspended'" type="success" @click="handleResume">恢复</el-button>
        <el-button v-if="isActive || contract.status === 'Suspended'" type="primary" @click="showOtherModify = true">修改信息</el-button>
        <el-button @click="$router.back()">返回</el-button>
      </div>
    </div>

    <!--===============================================================-->
    <!-- 1. Basic Info Card                                               -->
    <!--===============================================================-->
    <el-card style="margin-bottom: 16px;">
      <template #header>
        <span>基本信息</span>
        <el-tag v-if="contract.status === 'Active'" type="success" size="small" style="margin-left: 8px;">活跃</el-tag>
        <el-tag v-else :type="tagTypeFor(contract.status)" size="small" style="margin-left: 8px;">{{ statusLabel(contract.status) }}</el-tag>
      </template>
      <el-descriptions :column="3" border>
        <el-descriptions-item label="合同号">{{ contract.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="房屋">{{ contract.roomName }}</el-descriptions-item>
        <el-descriptions-item label="租客">{{ contract.tenantName }}</el-descriptions-item>
        <el-descriptions-item label="月租金">
          <span style="font-weight: bold; font-size: 16px; color: #409eff;">¥{{ contract.rentAmount?.toLocaleString() }}</span>
        </el-descriptions-item>
        <el-descriptions-item label="押金">¥{{ contract.depositAmount?.toLocaleString() }}</el-descriptions-item>
        <el-descriptions-item label="付款到期日">每月{{ contract.paymentDueDay || 5 }}日</el-descriptions-item>
        <el-descriptions-item label="起租日期">{{ contract.startDate }}</el-descriptions-item>
        <el-descriptions-item label="到期日期">{{ contract.endDate }}</el-descriptions-item>
        <el-descriptions-item label="押金抵最后月租">{{ contract.allowDepositAsLastRent ? '是' : '否' }}</el-descriptions-item>
        <el-descriptions-item label="租客电话" :span="2">{{ contract.tenantPhone || '-' }}</el-descriptions-item>
      </el-descriptions>
    </el-card>

    <!--===============================================================-->
    <!-- 2. Tabs: Fee Config / Deposit / Change History                  -->
    <!--===============================================================-->
    <el-card style="margin-bottom: 16px;">
      <el-tabs v-model="activeTab">
        <!-------- 2a. Fee Config ------>
        <el-tab-pane label="费用配置" name="fee">
          <div style="margin-bottom: 12px;">
            <el-button type="primary" size="small" @click="showModifyFee = true">
              <el-icon><Edit /></el-icon>费用调价
            </el-button>
            <span style="margin-left: 12px; color: #909399; font-size: 13px;">调价后按生效日期分段计价，历史价格可追溯</span>
          </div>
          <el-table :data="feeConfigs" stripe>
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="feeName" label="收费项目" width="110" />
            <el-table-column prop="chargeMethod" label="计费方式" width="90" />
            <el-table-column label="当前价格" width="110">
              <template #default="{ row }">
                <span style="font-weight: bold;">{{ row.chargeMethod === '固定金额' ? '¥' + row.unitPrice : row.unitPrice + (row.unit || '') }}</span>
              </template>
            </el-table-column>
            <el-table-column label="历史价格" min-width="200">
              <template #default="{ row }">
                <el-tag v-for="(h, i) in row.history" :key="i" size="small" style="margin-right: 4px; margin-bottom: 2px;" :type="i === 0 ? '' : 'info'">
                  {{ h.price }} <span style="font-size: 11px;">({{ h.date }})</span>
                </el-tag>
                <span v-if="!row.history?.length" style="color: #c0c4cc;">无历史</span>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="80">
              <template #default>
                <el-button text size="small" type="primary" @click="showModifyFee = true">调整</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-tab-pane>

        <!-------- 2b. Deposit Records ------>
        <el-tab-pane label="押金记录" name="deposit">
          <el-table :data="depositLogs" stripe>
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="date" label="日期" width="110" />
            <el-table-column prop="action" label="操作" width="100">
              <template #default="{ row }">
                <el-tag :type="row.action === '收取' ? 'success' : row.action === '退还' ? 'danger' : row.action === '扣款' ? 'warning' : ''" size="small">{{ row.action }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="amount" label="金额" width="120">
              <template #default="{ row }">
                <span :style="{ color: row.amount > 0 && row.action === '收取' ? '#67c23a' : '#f56c6c', fontWeight: 'bold' }">
                  {{ row.amount > 0 ? '+' : '' }}¥{{ row.amount?.toLocaleString() }}
                </span>
              </template>
            </el-table-column>
            <el-table-column prop="balance" label="余额" width="120">
              <template #default="{ row }">¥{{ row.balance?.toLocaleString() }}</template>
            </el-table-column>
            <el-table-column prop="remark" label="备注" min-width="150" />
          </el-table>
        </el-tab-pane>

        <!-------- 2c. Change History ------>
        <el-tab-pane label="变更历史" name="history">
          <el-timeline>
            <el-timeline-item
              v-for="(item, index) in changeHistory"
              :key="index"
              :timestamp="item.date"
              :type="item.type"
              :hollow="item.hollow"
              size="large"
            >
              <h4>{{ item.title }}</h4>
              <p v-if="item.detail" style="color: #606266;">{{ item.detail }}</p>
              <p v-if="item.operator" style="font-size: 12px; color: #909399;">操作人: {{ item.operator }}</p>

              <!-- Show field changes if present -->
              <el-table v-if="item.changes?.length" :data="item.changes" size="small" stripe style="margin-top: 8px; max-width: 500px;">
                <el-table-column prop="field" label="变更字段" width="100" />
                <el-table-column prop="oldValue" label="旧值" width="120" />
                <el-table-column prop="newValue" label="新值" width="120" />
              </el-table>

              <!-- Show approval info if present -->
              <div v-if="item.approval" style="margin-top: 4px;">
                <el-tag size="small" :type="item.approval.status === '已通过' ? 'success' : item.approval.status === '审批中' ? 'warning' : 'danger'">
                  {{ item.approval.status }} — {{ item.approval.level }}
                </el-tag>
              </div>
            </el-timeline-item>

            <el-timeline-item timestamp="合同创建" type="primary">
              <h4>合同签订</h4>
              <p style="color: #606266;">初始月租 ¥{{ contract.rentAmount?.toLocaleString() }}，押金 ¥{{ contract.depositAmount?.toLocaleString() }}</p>
            </el-timeline-item>
          </el-timeline>
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!--===============================================================-->
    <!-- 3. Receivable Timeline                                         -->
    <!--===============================================================-->
    <el-card>
      <template #header>
        <span>应收时间线</span>
        <el-button text type="primary" size="small" style="float: right;" @click="generateReceivables">手动生成应收</el-button>
      </template>
      <el-table :data="receivableTimeline" stripe default-expand-all row-key="id">
        <el-table-column prop="period" label="账期" width="80" />
        <el-table-column prop="dueDate" label="到期日" width="90" />
        <el-table-column prop="amount" label="应收" width="100">
          <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="received" label="已收" width="100">
          <template #default="{ row }">¥{{ row.received?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="欠费" width="100">
          <template #default="{ row }">
            <span v-if="row.amount - row.received > 0" style="color: #f56c6c;">¥{{ (row.amount - row.received).toLocaleString() }}</span>
            <span v-else style="color: #67c23a;">已结清</span>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Paid' ? 'success' : row.status === 'Partial' ? 'warning' : row.status === 'Cancelled' ? 'info' : 'danger'" size="small">
              {{ row.status === 'Paid' ? '已付清' : row.status === 'Partial' ? '部分' : row.status === 'Cancelled' ? '已取消' : '待收款' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column type="expand" width="50">
          <template #default="{ row }">
            <el-table :data="row.details" size="small">
              <el-table-column prop="feeName" label="费用项目" width="110" />
              <el-table-column prop="amount" label="金额" width="90"><template #default="{ row: d }">¥{{ d.amount?.toLocaleString() }}</template></el-table-column>
              <el-table-column prop="received" label="已收" width="90"><template #default="{ row: d }">¥{{ d.received?.toLocaleString() }}</template></el-table-column>
              <el-table-column prop="description" label="计算说明 / 调价说明" min-width="220" />
            </el-table>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!--===============================================================-->
    <!-- MODAL: Rent Adjustment                                        -->
    <!--===============================================================-->
    <el-dialog v-model="showModifyRent" title="合同租金调整" width="580px">
      <el-alert
        title="租金调整说明：租金调整将根据调整差额自动路由到对应审批级别（差额≤5000元: 运营主管1级审批；差额>5000元: 部门经理2级审批）。生效日期起按新租金生成应收。"
        type="info"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />
      <el-descriptions :column="2" border style="margin-bottom: 16px;">
        <el-descriptions-item label="合同号">{{ contract.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="当前月租">¥{{ contract.rentAmount?.toLocaleString() }}</el-descriptions-item>
        <el-descriptions-item label="房屋">{{ contract.roomName }}</el-descriptions-item>
        <el-descriptions-item label="租客">{{ contract.tenantName }}</el-descriptions-item>
      </el-descriptions>
      <el-form :model="rentForm" label-width="110px">
        <el-form-item label="新月租金 (元)">
          <el-input-number v-model="rentForm.newAmount" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="调整差额">
          <span :style="{ color: rentDiff >= 0 ? '#f56c6c' : '#67c23a', fontWeight: 'bold', fontSize: '18px' }">
            {{ rentDiff >= 0 ? '↑ 涨' : '↓ 降' }} ¥{{ Math.abs(rentDiff).toLocaleString() }}
            ({{ contract.rentAmount ? (rentDiff / contract.rentAmount * 100).toFixed(1) : 0 }}%)
          </span>
        </el-form-item>
        <el-form-item label="审批级别">
          <el-tag :type="rentApprovalLevel === '1级(运营主管)' ? 'warning' : 'danger'">
            {{ rentApprovalLevel }}
          </el-tag>
        </el-form-item>
        <el-form-item label="生效日期（含当日）">
          <el-date-picker v-model="rentForm.effectiveDate" type="date" style="width: 200px;" />
          <span style="margin-left: 8px; color: #909399;">此日期起按新租金计费</span>
        </el-form-item>
        <el-form-item label="调整原因">
          <el-input v-model="rentForm.reason" type="textarea" :rows="3" placeholder="必填：如市场行情变化、合同约定年度涨幅、租客协商等" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showModifyRent = false">取消</el-button>
        <el-button type="primary" @click="submitRentAdjust">提交审批</el-button>
      </template>
    </el-dialog>

    <!--===============================================================-->
    <!-- MODAL: Fee Price Adjustment                                   -->
    <!--===============================================================-->
    <el-dialog v-model="showModifyFee" title="合同费用中途调价" width="700px">
      <el-alert
        title="费用调价将提交运营主管（1级）审批。按生效日期分段计价：生效日前按原价格，生效日起（含当日）按新价格。"
        type="info"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />
      <el-descriptions :column="3" border style="margin-bottom: 16px;">
        <el-descriptions-item label="合同号">{{ contract.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="房屋">{{ contract.roomName }}</el-descriptions-item>
        <el-descriptions-item label="租客">{{ contract.tenantName }}</el-descriptions-item>
      </el-descriptions>
      <el-table :data="feeAdjustItems" stripe>
        <el-table-column type="index" label="#" width="45" />
        <el-table-column prop="feeName" label="项目" width="90" />
        <el-table-column prop="chargeMethod" label="方式" width="80" />
        <el-table-column label="当前价格" width="100">
          <template #default="{ row }">
            <span v-if="row.oldPrice !== undefined">{{ row.oldPrice }}</span>
            <span v-else style="color: #c0c4cc;">-</span>
          </template>
        </el-table-column>
        <el-table-column label="新价格" width="120">
          <template #default="{ row }">
            <el-input-number v-model="row.newPrice" :min="0" :precision="row.chargeMethod === '按表计量' ? 4 : 2" size="small" :step="row.chargeMethod === '按表计量' ? 0.5 : 50" style="width: 100px;" />
            <span v-if="row.unit" style="margin-left: 2px; font-size: 12px; color: #909399;">{{ row.unit }}</span>
          </template>
        </el-table-column>
        <el-table-column label="调幅" width="80">
          <template #default="{ row }">
            <span v-if="row.oldPrice !== undefined && row.oldPrice !== null && row.oldPrice !== 0" :style="{ color: row.newPrice > row.oldPrice ? '#f56c6c' : row.newPrice < row.oldPrice ? '#67c23a' : '#909399' }">
              {{ row.newPrice > row.oldPrice ? '↑' : row.newPrice < row.oldPrice ? '↓' : '→' }}
              {{ Math.abs((row.newPrice - row.oldPrice) / row.oldPrice * 100).toFixed(1) }}%
            </span>
          </template>
        </el-table-column>
      </el-table>
      <el-form style="margin-top: 16px;">
        <el-form-item label="生效日期">
          <el-date-picker v-model="feeAdjustEffectiveDate" type="date" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="调价原因">
          <el-input v-model="feeAdjustReason" type="textarea" :rows="2" placeholder="必填" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showModifyFee = false">取消</el-button>
        <el-button type="primary" @click="submitFeeAdjust">提交审批</el-button>
      </template>
    </el-dialog>

    <!--===============================================================-->
    <!-- MODAL: Other Field Modify (CONTRACT_MODIFY_OTHER)              -->
    <!--===============================================================-->
    <el-dialog v-model="showOtherModify" title="修改合同信息" width="550px">
      <el-alert
        title="以下字段变更无需审批（0级），修改后立即生效。租金和费用调整请使用专用功能。"
        type="success"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />
      <el-form :model="otherForm" label-width="120px">
        <el-form-item label="租客电话">
          <el-input v-model="otherForm.tenantPhone" />
        </el-form-item>
        <el-form-item label="付款到期日">
          <el-select v-model="otherForm.paymentDueDay" style="width: 200px;">
            <el-option v-for="d in 28" :key="d" :label="'每月' + d + '日'" :value="d" />
          </el-select>
        </el-form-item>
        <el-form-item label="押金抵最后月租">
          <el-switch v-model="otherForm.allowDepositAsLastRent" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="otherForm.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showOtherModify = false">取消</el-button>
        <el-button type="primary" @click="submitOtherModify">保存修改</el-button>
      </template>
    </el-dialog>

    <!--===============================================================-->
    <!-- MODAL: Renew Contract                                         -->
    <!--===============================================================-->
    <el-dialog v-model="showRenew" title="合同续签" width="580px">
      <el-alert
        title="续签将创建新合同，原合同状态更新为「已续签」。如续签条件与原合同一致则走简化审批（1级-运营主管）。"
        type="success"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />
      <el-descriptions :column="2" border style="margin-bottom: 16px;">
        <el-descriptions-item label="原合同号">{{ contract.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="原到期日">{{ contract.endDate }}</el-descriptions-item>
        <el-descriptions-item label="当前月租">¥{{ contract.rentAmount?.toLocaleString() }}</el-descriptions-item>
        <el-descriptions-item label="当前押金">¥{{ contract.depositAmount?.toLocaleString() }}</el-descriptions-item>
      </el-descriptions>
      <el-form :model="renewForm" label-width="120px">
        <el-form-item label="新月租金 (元)">
          <el-input-number v-model="renewForm.rentAmount" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="新起租日期">
          <el-date-picker v-model="renewForm.startDate" type="date" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="新到期日期">
          <el-date-picker v-model="renewForm.endDate" type="date" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="押金处理">
          <el-radio-group v-model="renewForm.depositHandling">
            <el-radio label="TRANSFER">原押金延续</el-radio>
            <el-radio label="NEW">重新收取押金</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item v-if="renewForm.depositHandling === 'NEW'" label="新押金">
          <el-input-number v-model="renewForm.newDeposit" :min="0" :precision="2" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="续签备注">
          <el-input v-model="renewForm.remark" type="textarea" :rows="2" placeholder="如有特殊条款或变更说明" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showRenew = false">取消</el-button>
        <el-button type="primary" @click="submitRenew">提交续签审批</el-button>
      </template>
    </el-dialog>

    <!--===============================================================-->
    <!-- MODAL: Terminate                                              -->
    <!--===============================================================-->
    <el-dialog v-model="showTerminate" title="合同终止" width="520px">
      <el-alert
        title="提前解约将根据押金金额自动路由审批级别（押金≤5000:1级运营主管; 押金5000~50000:2级部门经理; 押金50000+:3级总经理）。请确保已与租客协商一致。"
        type="warning"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />
      <el-form :model="terminateForm" label-width="110px">
        <el-form-item label="终止类型">
          <el-radio-group v-model="terminateForm.type">
            <el-radio label="EXPIRED">到期终止</el-radio>
            <el-radio label="EARLY">提前解约</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="实际搬离日">
          <el-date-picker v-model="terminateForm.actualEndDate" type="date" style="width: 200px;" />
        </el-form-item>
        <el-form-item label="押金退还">
          <el-radio-group v-model="terminateForm.depositReturn">
            <el-radio label="FULL">全额退还</el-radio>
            <el-radio label="DEDUCT">扣款后退还</el-radio>
            <el-radio label="LAST_RENT">抵扣最后月租</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="终止原因">
          <el-input v-model="terminateForm.reason" type="textarea" :rows="3" placeholder="必填" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showTerminate = false">取消</el-button>
        <el-button type="primary" @click="submitTerminate">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Suspend Confirm -->
    <el-dialog v-model="showSuspend" title="暂停合同" width="400px">
      <p>暂停期间将不生成新的应收计划。确定暂停合同 <strong>{{ contract.contractNo }}</strong> 吗？</p>
      <el-form style="margin-top: 12px;">
        <el-form-item label="暂停原因">
          <el-input v-model="suspendReason" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showSuspend = false">取消</el-button>
        <el-button type="warning" @click="handleSuspend">确定暂停</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'

const route = useRoute()
const router = useRouter()
const activeTab = ref('fee')

/* ================================================================
 * Mock Data: Contract
 * ================================================================ */
const contract = ref({
  id: route.params.id,
  contractNo: 'HT-2026-001',
  roomName: 'A栋-101',
  tenantName: '张三',
  tenantPhone: '13800138001',
  rentAmount: 5200,
  depositAmount: 10400,
  startDate: '2026-01-01',
  endDate: '2027-12-31',
  status: 'Active',
  paymentDueDay: 5,
  allowDepositAsLastRent: false,
  remark: ''
})

const isActive = computed(() => contract.value.status === 'Active')

function tagTypeFor(status) {
  const map = { Active: 'success', Suspended: '', Expired: 'danger', Terminated: 'danger', Draft: 'info', PendingApproval: 'warning', Renewed: 'primary' }
  return map[status] || 'info'
}
function statusLabel(status) {
  const map = { Active: '活跃', Draft: '草稿', PendingApproval: '待审批', Suspended: '已暂停', Expired: '已到期', Terminated: '已终止', Renewed: '已续签' }
  return map[status] || status
}

/* ================================================================
 * Fee Configs with Price History
 * ================================================================ */
const feeConfigs = ref([
  { feeName: '房租费', chargeMethod: '固定金额', unitPrice: '¥5,200', unit: '', history: [{ price: '¥5,000', date: '2026-01-01生效' }, { price: '¥5,200', date: '2026-07-01生效' }] },
  { feeName: '水费', chargeMethod: '按表计量', unitPrice: '6.00', unit: '元/吨', history: [{ price: '5.00', date: '2026-01-01生效' }, { price: '6.00', date: '2026-04-01生效' }] },
  { feeName: '电费', chargeMethod: '按表计量', unitPrice: '0.80', unit: '元/度', history: [{ price: '0.85', date: '2026-01-01生效' }, { price: '0.80', date: '2026-06-01生效' }] },
  { feeName: '卫生费', chargeMethod: '固定金额', unitPrice: '¥30', unit: '', history: [] },
  { feeName: '管理费', chargeMethod: '固定金额', unitPrice: '¥150', unit: '', history: [{ price: '¥120', date: '2026-01-01生效' }, { price: '¥150', date: '2026-03-01生效' }] },
  { feeName: '网费', chargeMethod: '固定金额', unitPrice: '¥80', unit: '', history: [] }
])

/* ================================================================
 * Deposit Logs
 * ================================================================ */
const depositLogs = ref([
  { date: '2026-01-01', action: '收取', amount: 10400, balance: 10400, remark: '合同签订押金（2个月租金）' },
])

/* ================================================================
 * Change History Timeline
 * ================================================================ */
const changeHistory = ref([
  {
    date: '2026-07-01',
    title: '租金调涨',
    detail: '月租金 ¥5,000 → ¥5,200（涨幅4%）',
    operator: '运营主管-李四',
    type: 'warning',
    hollow: false,
    changes: [
      { field: '月租金', oldValue: '¥5,000', newValue: '¥5,200' }
    ],
    approval: { status: '已通过', level: '1级-运营主管' }
  },
  {
    date: '2026-06-01',
    title: '电费调降',
    detail: '电费单价 ¥0.85/度 → ¥0.80/度（降幅5.9%）',
    operator: '运营主管-李四',
    type: 'success',
    hollow: false,
    changes: [
      { field: '电费单价', oldValue: '0.85 元/度', newValue: '0.80 元/度' }
    ],
    approval: { status: '已通过', level: '运营主管审批' }
  },
  {
    date: '2026-04-01',
    title: '水费调涨',
    detail: '水费单价 ¥5.00/吨 → ¥6.00/吨（涨幅20%）',
    operator: '运营主管-李四',
    type: 'warning',
    hollow: false,
    changes: [
      { field: '水费单价', oldValue: '5.00 元/吨', newValue: '6.00 元/吨' }
    ],
    approval: { status: '已通过', level: '运营主管审批' }
  },
  {
    date: '2026-03-01',
    title: '管理费调涨',
    detail: '管理费 ¥120 → ¥150（涨幅25%）',
    operator: '运营主管-李四',
    type: 'warning',
    hollow: false,
    changes: [
      { field: '管理费', oldValue: '¥120', newValue: '¥150' }
    ],
    approval: { status: '已通过', level: '运营主管审批' }
  },
  {
    date: '2026-02-15',
    title: '租客电话更新',
    detail: '电话: 13800138000 → 13800138001',
    operator: '运营-张三',
    type: 'info',
    hollow: true,
    changes: [
      { field: '租客电话', oldValue: '13800138000', newValue: '13800138001' }
    ]
  }
])

/* ================================================================
 * Receivable Timeline
 * ================================================================ */
const receivableTimeline = ref([
  {
    id: 'rp1', period: '2026-08', dueDate: '2026-08-05', amount: 5460, received: 0, status: 'Pending',
    details: [
      { feeName: '房租费', amount: 5200, received: 0, description: '新租金¥5,200(自2026-07-01起)' },
      { feeName: '卫生费', amount: 30, received: 0, description: '月固定' },
      { feeName: '管理费', amount: 150, received: 0, description: '¥150/月' },
      { feeName: '网费', amount: 80, received: 0, description: '¥80/月' }
    ]
  },
  {
    id: 'rp2', period: '2026-07', dueDate: '2026-07-05', amount: 5460, received: 2000, status: 'Partial',
    details: [
      { feeName: '房租费', amount: 5200, received: 2000, description: '调价后¥5,200(原¥5,000)' },
      { feeName: '卫生费', amount: 30, received: 0, description: '月固定' },
      { feeName: '管理费', amount: 150, received: 0, description: '¥150/月' },
      { feeName: '网费', amount: 80, received: 0, description: '¥80/月' }
    ]
  },
  {
    id: 'rp3', period: '2026-06', dueDate: '2026-06-05', amount: 5260, received: 5260, status: 'Paid',
    details: [
      { feeName: '房租费', amount: 5000, received: 5000, description: '调价前¥5,000' },
      { feeName: '卫生费', amount: 30, received: 30, description: '月固定' },
      { feeName: '管理费', amount: 150, received: 150, description: '¥150/月' },
      { feeName: '网费', amount: 80, received: 80, description: '¥80/月' }
    ]
  }
])

/* ================================================================
 * Rent Adjustment (CONTRACT_MODIFY — AmountBased 1~2级)
 * ================================================================ */
const showModifyRent = ref(false)
const rentForm = reactive({ newAmount: 5200, effectiveDate: '', reason: '' })

const rentDiff = computed(() => rentForm.newAmount - (contract.value.rentAmount || 0))
const rentApprovalLevel = computed(() => {
  const absDiff = Math.abs(rentDiff.value)
  if (absDiff <= 5000) return '1级(运营主管)'
  return '2级(部门经理)'
})

function submitRentAdjust() {
  if (!rentForm.newAmount || rentForm.newAmount <= 0) { ElMessage.warning('请输入有效的新租金'); return }
  if (!rentForm.effectiveDate) { ElMessage.warning('请选择生效日期'); return }
  if (!rentForm.reason) { ElMessage.warning('请填写调整原因'); return }

  // Push to change history
  changeHistory.value.unshift({
    date: new Date().toISOString().split('T')[0],
    title: '租金调整审批中',
    detail: `月租金 ¥${contract.value.rentAmount?.toLocaleString()} → ¥${rentForm.newAmount.toLocaleString()}（${rentForm.reason}）`,
    operator: '当前用户',
    type: 'warning',
    hollow: false,
    changes: [{ field: '月租金', oldValue: '¥' + contract.value.rentAmount?.toLocaleString(), newValue: '¥' + rentForm.newAmount.toLocaleString() }],
    approval: { status: '审批中', level: rentApprovalLevel.value }
  })

  contract.value.rentAmount = rentForm.newAmount
  ElMessage.success(`租金调整申请已提交${rentApprovalLevel.value}审批`)
  showModifyRent.value = false
}

/* ================================================================
 * Fee Price Adjustment (CONTRACT_FEE_CHANGE — Fixed 1级)
 * ================================================================ */
const showModifyFee = ref(false)
const feeAdjustEffectiveDate = ref('')
const feeAdjustReason = ref('')

const feeAdjustItems = reactive([
  { feeName: '房租费', chargeMethod: '固定金额', oldPrice: 5200, newPrice: 5200, unit: '' },
  { feeName: '水费', chargeMethod: '按表计量', oldPrice: 6.00, newPrice: 6.00, unit: '元/吨' },
  { feeName: '电费', chargeMethod: '按表计量', oldPrice: 0.80, newPrice: 0.80, unit: '元/度' },
  { feeName: '管理费', chargeMethod: '固定金额', oldPrice: 150, newPrice: 150, unit: '' },
  { feeName: '卫生费', chargeMethod: '固定金额', oldPrice: 30, newPrice: 30, unit: '' },
  { feeName: '网费', chargeMethod: '固定金额', oldPrice: 80, newPrice: 80, unit: '' }
])

function submitFeeAdjust() {
  if (!feeAdjustReason.value) { ElMessage.warning('请填写调价原因'); return }
  if (!feeAdjustEffectiveDate.value) { ElMessage.warning('请选择生效日期'); return }

  const changedItems = feeAdjustItems.filter(item => item.newPrice !== item.oldPrice)
  if (changedItems.length === 0) { ElMessage.warning('没有费用项目价格发生变化'); return }

  // Update fee configs display and push history
  changedItems.forEach(item => {
    const config = feeConfigs.value.find(c => c.feeName === item.feeName)
    if (config) {
      if (!config.history) config.history = []
      config.history.unshift({
        price: (item.chargeMethod === '固定金额' ? '¥' : '') + item.newPrice + (item.unit || ''),
        date: (feeAdjustEffectiveDate.value || '新') + '生效'
      })
    }
    changeHistory.value.unshift({
      date: new Date().toISOString().split('T')[0],
      title: `${item.feeName}调价审批中`,
      detail: `${item.feeName}: ${item.oldPrice} → ${item.newPrice}（${feeAdjustReason.value}）`,
      operator: '当前用户',
      type: 'warning',
      hollow: false,
      changes: [{ field: item.feeName, oldValue: String(item.oldPrice), newValue: String(item.newPrice) }],
      approval: { status: '审批中', level: '运营主管(1级)' }
    })
  })

  ElMessage.success(`费用调价申请已提交运营主管审批，涉及 ${changedItems.length} 项费用`)
  showModifyFee.value = false
}

/* ================================================================
 * Other Modify (CONTRACT_MODIFY_OTHER — Fixed 0级)
 * ================================================================ */
const showOtherModify = ref(false)
const otherForm = reactive({
  tenantPhone: '13800138001',
  paymentDueDay: 5,
  allowDepositAsLastRent: false,
  remark: ''
})

function submitOtherModify() {
  // Record changes
  const changes = []
  if (otherForm.tenantPhone !== contract.value.tenantPhone) {
    changes.push({ field: '租客电话', oldValue: contract.value.tenantPhone || '-', newValue: otherForm.tenantPhone })
    contract.value.tenantPhone = otherForm.tenantPhone
  }
  if (otherForm.paymentDueDay !== contract.value.paymentDueDay) {
    changes.push({ field: '付款到期日', oldValue: '每月' + contract.value.paymentDueDay + '日', newValue: '每月' + otherForm.paymentDueDay + '日' })
    contract.value.paymentDueDay = otherForm.paymentDueDay
  }
  if (otherForm.allowDepositAsLastRent !== contract.value.allowDepositAsLastRent) {
    changes.push({ field: '押金抵最后月租', oldValue: contract.value.allowDepositAsLastRent ? '是' : '否', newValue: otherForm.allowDepositAsLastRent ? '是' : '否' })
    contract.value.allowDepositAsLastRent = otherForm.allowDepositAsLastRent
  }

  if (changes.length > 0) {
    changeHistory.value.unshift({
      date: new Date().toISOString().split('T')[0],
      title: '合同信息修改',
      detail: changes.map(c => `${c.field}: ${c.oldValue} → ${c.newValue}`).join('；'),
      operator: '当前用户',
      type: 'info',
      hollow: true,
      changes
    })
  }
  ElMessage.success('合同信息已更新')
  showOtherModify.value = false
}

/* ================================================================
 * Renew (CONTRACT_RENEW — Fixed 1级)
 * ================================================================ */
const showRenew = ref(false)
const renewForm = reactive({
  rentAmount: 5200,
  startDate: '',
  endDate: '',
  depositHandling: 'TRANSFER',
  newDeposit: 10400,
  remark: ''
})

function submitRenew() {
  if (!renewForm.rentAmount || !renewForm.startDate || !renewForm.endDate) {
    ElMessage.warning('请填写完整的续签信息')
    return
  }
  ElMessage.success(`续签审批已提交运营主管（1级审批），新合同将关联到 ${contract.value.roomName}`)
  showRenew.value = false
}

/* ================================================================
 * Terminate (CONTRACT_TERMINATE — AmountBased 1~3级)
 * ================================================================ */
const showTerminate = ref(false)
const terminateForm = reactive({
  type: 'EARLY',
  actualEndDate: '',
  depositReturn: 'FULL',
  reason: ''
})

const terminateApprovalLevel = computed(() => {
  const deposit = contract.value.depositAmount || 0
  if (deposit <= 5000) return '1级(运营主管)'
  if (deposit <= 50000) return '2级(部门经理)'
  return '3级(总经理)'
})

function submitTerminate() {
  if (!terminateForm.reason) { ElMessage.warning('请填写终止原因'); return }
  if (!terminateForm.actualEndDate) { ElMessage.warning('请选择实际搬离日'); return }

  if (terminateForm.type === 'EARLY') {
    ElMessage.success(`提前解约申请已提交${terminateApprovalLevel.value}审批`)
  } else {
    ElMessage.success('到期终止申请已提交')
  }
  showTerminate.value = false
}

/* ================================================================
 * Suspend / Resume
 * ================================================================ */
const showSuspend = ref(false)
const suspendReason = ref('')

function handleSuspend() {
  contract.value.status = 'Suspended'
  changeHistory.value.unshift({
    date: new Date().toISOString().split('T')[0],
    title: '合同暂停',
    detail: `暂停原因: ${suspendReason.value || '未填写'}`,
    operator: '当前用户',
    type: 'info',
    hollow: true
  })
  ElMessage.success('合同已暂停，暂停期间不生成应收')
  showSuspend.value = false
}

function handleResume() {
  ElMessageBox.confirm(`确定恢复合同 ${contract.value.contractNo} 吗？恢复后将正常生成应收。`, '提示').then(() => {
    contract.value.status = 'Active'
    changeHistory.value.unshift({
      date: new Date().toISOString().split('T')[0],
      title: '合同恢复',
      operator: '当前用户',
      type: 'success',
      hollow: true
    })
    ElMessage.success('合同已恢复')
  }).catch(() => {})
}

function generateReceivables() {
  ElMessage.success('应收已成功生成')
}
</script>
<style scoped>
.el-timeline h4 {
  margin: 0 0 4px;
}
</style>
