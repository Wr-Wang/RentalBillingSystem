<template>
  <div>
    <div class="page-header">
      <h2>合同详情</h2>
      <div class="table-actions">
        <el-button v-if="isActive" type="warning" @click="showModifyRent = true">租金调整</el-button>
        <el-button v-if="isActive || contract.status === 'Suspended'" type="warning" @click="openFeeAdjust">费用调价</el-button>
        <el-button v-if="isActive && !contract.hasRenewalContract" type="primary" @click="openRenewDialog">续签</el-button>
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
        <el-descriptions-item label="自动续签">
          <el-tag :type="contract.autoRenew ? 'success' : 'info'" size="small" style="cursor:pointer;" @click="toggleAutoRenew">
            {{ contract.autoRenew ? '已开启' : '已关闭' }}
          </el-tag>
        </el-descriptions-item>
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
          <div style="margin-bottom: 12px; display:flex; gap:8px; align-items:center; flex-wrap:wrap;">
            <el-button type="primary" size="small" @click="openFeeAdjust">批量调价</el-button>
            <el-button size="small" @click="openAddFeeConfig">添加费用</el-button>
            <el-button size="small" @click="fetchFeeConfigs" :loading="feeConfigLoading">刷新</el-button>
            <el-tag v-if="monthlyTotal > 0" type="warning" effect="plain">
              月度合计: ¥{{ monthlyTotal.toLocaleString() }}
            </el-tag>
            <span style="color: #909399; font-size: 13px; margin-left:auto;">修改后需提交审批，按生效日期分段计价</span>
          </div>
          <el-table :data="currentFeeConfigs" v-loading="feeConfigLoading" stripe style="width:100%;" @expand-change="onFeeConfigExpand">
            <el-table-column type="expand" width="30">
              <template #default="{ row }">
                <el-table :data="row.history" size="small" stripe v-loading="row.loadingHistory" style="margin:8px 0;">
                  <el-table-column label="金额" width="100"><template #default="{ row: h }">¥{{ (h.amount || 0).toLocaleString() }}</template></el-table-column>
                  <el-table-column label="生效日期" width="120"><template #default="{ row: h }">{{ h.effectiveDate || '-' }}</template></el-table-column>
                  <el-table-column label="到期日期" width="120"><template #default="{ row: h }">{{ h.expiryDate || '至今' }}</template></el-table-column>
                  <el-table-column label="状态" width="70"><template #default="{ row: h }"><el-tag :type="h.isActive ? 'success' : 'info'" size="small">{{ h.isActive ? '生效' : '已过期' }}</el-tag></template></el-table-column>
                  <el-table-column label="创建时间" min-width="140"><template #default="{ row: h }">{{ h.createdAt || '' }}</template></el-table-column>
                </el-table>
                <span v-if="!row.history?.length && !row.loadingHistory" style="color:#909399;font-size:13px;padding:8px;">暂无历史记录</span>
              </template>
            </el-table-column>
            <el-table-column label="收费项目" min-width="100">
              <template #default="{ row }">
                <span :style="{ color: row.isActive ? '#303133' : '#c0c4cc' }">{{ row.feeName }}</span>
              </template>
            </el-table-column>
            <el-table-column label="价格" min-width="120">
              <template #default="{ row }">
                <span v-if="row.isActive" style="font-weight:bold;font-size:14px;">¥{{ (row.amount || 0).toLocaleString() }}</span>
                <span v-else style="color:#c0c4cc;text-decoration:line-through;">¥{{ (row.amount || 0).toLocaleString() }}</span>
              </template>
            </el-table-column>
            <el-table-column label="生效期" min-width="200">
              <template #default="{ row }">
                <span style="font-size:13px;">{{ row.effectiveDate || '-' }} ~ {{ row.expiryDate || '至今' }}</span>
              </template>
            </el-table-column>
            <el-table-column label="状态" width="65" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isActive ? 'success' : 'info'" size="small" effect="plain">{{ row.isActive ? '启用' : '停用' }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="170" fixed="right">
              <template #default="{ row }">
                <el-button v-if="row.isActive" text size="small" type="primary" @click="openAdjustFeeConfig(row)">调价</el-button>
                <el-button text size="small" type="warning" @click="toggleFeeConfig(row)">{{ row.isActive ? '停用' : '启用' }}</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!feeConfigLoading && feeConfigs.length === 0" description="暂无费用配置，点击「添加费用」新增" :image-size="60" style="padding:20px 0;" />
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
        <div style="display:flex;align-items:center;justify-content:space-between;">
          <span>应收时间线</span>
          <div style="display:flex;gap:8px;align-items:center;">
            <span v-if="receivableStats.totalAmount > 0" style="font-size:13px;color:#909399;">
              应收合计: <strong style="color:#e6a23c;">¥{{ receivableStats.totalAmount.toLocaleString() }}</strong>
              | 未收: <strong style="color:#f56c6c;">¥{{ receivableStats.totalDue.toLocaleString() }}</strong>
            </span>
            <el-button type="primary" size="small" @click="generateReceivables" :loading="generatingReceivables">生成应收</el-button>
          </div>
        </div>
      </template>
      <el-table :data="receivableTimeline" stripe default-expand-all row-key="id" v-loading="receivableLoading" style="width:100%;">
        <el-table-column prop="period" label="账期" width="85" />
        <el-table-column prop="dueDate" label="到期日" width="95" />
        <el-table-column label="应收" min-width="110" align="right">
          <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
        </el-table-column>
        <el-table-column label="已收" min-width="110" align="right">
          <template #default="{ row }">
            <span :style="{ color: row.received > 0 ? '#67c23a' : '#c0c4cc' }">¥{{ (row.received || 0).toLocaleString() }}</span>
          </template>
        </el-table-column>
        <el-table-column label="欠费" min-width="120" align="right">
          <template #default="{ row }">
            <span v-if="(row.amount || 0) - (row.received || 0) > 0" style="color:#f56c6c;font-weight:bold;">
              ¥{{ ((row.amount || 0) - (row.received || 0)).toLocaleString() }}
            </span>
            <span v-else style="color:#67c23a;">已结清</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="85" align="center">
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
      <el-empty v-if="!receivableLoading && receivableTimeline.length === 0" description="暂无应收数据，点击「生成应收」创建" :image-size="60" style="padding:20px 0;" />

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
        <el-button type="primary" :loading="submittingRent" @click="submitRentAdjust">提交审批</el-button>
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
    <!-- MODAL: Add Fee Config                                          -->
    <el-dialog v-model="showFeeConfigDialog" title="添加费用配置" width="480px">
      <el-form :model="feeConfigForm" label-width="100px">
        <el-form-item label="收费项目">
          <el-select v-model="feeConfigForm.feeCodeId" placeholder="选择收费项目" style="width:100%">
            <el-option v-for="fc in availableFeeCodes" :key="fc.id" :label="fc.name" :value="fc.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="月金额">
          <el-input-number v-model="feeConfigForm.amount" :min="0" :precision="2" style="width:200px" /> 元
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="feeConfigForm.effectiveDate" type="date" style="width:200px" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showFeeConfigDialog = false">取消</el-button>
        <el-button type="primary" @click="submitFeeConfig" :loading="feeConfigSaving">添加</el-button>
      </template>
    </el-dialog>

    <!-- MODAL: Adjust Fee Config (版本化调价)                          -->
    <el-dialog v-model="showAdjustDialog" title="费用调价" width="480px">
      <el-alert title="调价后原价格将在生效日前一天自动到期，新价格从生效日起执行。" type="info" show-icon :closable="false" style="margin-bottom:16px;" />
      <el-form label-width="100px">
        <el-form-item label="当前价格">
          <span style="font-weight:bold;font-size:16px;">¥{{ (adjustCurrentAmount || 0).toLocaleString() }}</span>
        </el-form-item>
        <el-form-item label="新价格">
          <el-input-number v-model="adjustForm.newAmount" :min="0" :precision="2" style="width:200px" /> 元/月
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="adjustForm.effectiveDate" type="date" style="width:200px" />
          <span style="margin-left:8px;color:#909399;font-size:12px;">此日期起按新价计费</span>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAdjustDialog = false">取消</el-button>
        <el-button type="primary" @click="adjustFeeConfig" :loading="feeConfigSaving">提交调价</el-button>
      </template>
    </el-dialog>

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
    <!-- MODAL: Renew Contract (审批驱动)                               -->
    <!--===============================================================-->
    <el-dialog v-model="showRenew" title="合同续签" width="620px" :before-close="() => showRenew = false">
      <el-alert
        title="续签将创建新合同，原合同标记为已续签。提交前请确认续签信息，审批通过后自动执行。"
        type="success"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />
      <!-- 预览信息 -->
      <div v-if="renewLoading" style="text-align:center;padding:20px;">
        <el-icon class="is-loading" :size="24"><Loading /></el-icon>
        <p>加载续签预览信息...</p>
      </div>
      <template v-else>
        <!-- 检查结果 -->
        <div v-if="renewChecks.paymentStatus && !renewChecks.paymentStatus.passed" style="margin-bottom:12px;">
          <el-alert :title="`该合同有未结清欠费 ¥${renewChecks.paymentStatus.outstandingAmount?.toLocaleString()}，请先处理`" type="error" show-icon :closable="false" />
        </div>
        <div v-if="renewChecks.concurrentApprovals?.hasPending" style="margin-bottom:12px;">
          <el-alert :title="renewChecks.concurrentApprovals.blockedMessage || '该合同存在待审批的申请，请处理完成后再提交续签'" type="warning" show-icon :closable="false" />
        </div>
        <div v-if="renewChecks.concurrentApprovals?.alreadyRenewed" style="margin-bottom:12px;">
          <el-alert title="该合同已被续签，不可再次续签" type="warning" show-icon :closable="false" />
        </div>
        <!-- 市场参考价 -->
        <div v-if="renewChecks.marketPrice" style="margin-bottom:12px;padding:8px 12px;background:#f5f7fa;border-radius:4px;font-size:13px;color:#606266;">
          同户型市场参考价：¥{{ renewChecks.marketPrice.minPrice?.toLocaleString() }} ~ ¥{{ renewChecks.marketPrice.maxPrice?.toLocaleString() }}
          （均价 ¥{{ renewChecks.marketPrice.averagePrice?.toLocaleString() }}）
        </div>
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
          <el-form-item label="新到期日期">
            <el-date-picker v-model="renewForm.endDate" type="date" style="width: 200px;" />
            <span style="margin-left:8px;color:#909399;font-size:12px;">起租日自动延续：{{ contract.endDate }} 次日</span>
          </el-form-item>
          <el-form-item label="押金处理">
            <el-radio-group v-model="renewForm.depositHandling">
              <el-radio label="TRANSFER">原押金延续（¥{{ contract.depositAmount?.toLocaleString() }}）</el-radio>
              <el-radio label="NEW">重新收取押金</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item v-if="renewForm.depositHandling === 'NEW'" label="新押金金额">
            <el-input-number v-model="renewForm.newDeposit" :min="0" :precision="2" style="width: 200px;" />
            <span v-if="renewForm.newDeposit > (contract.depositAmount || 0)" style="margin-left:8px;color:#e6a23c;">
              需补交 ¥{{ (renewForm.newDeposit - (contract.depositAmount || 0)).toLocaleString() }}
            </span>
            <span v-else-if="renewForm.newDeposit < (contract.depositAmount || 0)" style="margin-left:8px;color:#67c23a;">
              退还 ¥{{ ((contract.depositAmount || 0) - renewForm.newDeposit).toLocaleString() }}
            </span>
          </el-form-item>
          <el-form-item label="续签备注">
            <el-input v-model="renewForm.remark" type="textarea" :rows="2" placeholder="如有特殊条款或变更说明" />
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="showRenew = false">取消</el-button>
        <el-button type="primary" :disabled="!canSubmitRenewal" @click="submitRenew" :loading="renewLoading">提交续签审批</el-button>
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
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { submitApproval, getApprovalTypes, getRoles, createApprovalType, createApprovalLevel, getContract, terminateContract, renewContract, suspendContract, resumeContract, getReceivables, generateReceivables as apiGenerateReceivables, getDeposits, getContractFeeConfigs, createContractFeeConfig, updateContractFeeConfig, adjustContractFeeConfig, getContractFeeConfigHistory, getFeeCodes, previewRenewal, submitRenewal, getRenewalHistory, getRenewalChain, getAllowedOperations } from '@/api/index.js'

const route = useRoute()
const router = useRouter()
const activeTab = ref('fee')

/* ================================================================
 * Real Data: Contract from API
 * ================================================================ */
const contract = ref({
  id: route.params.id,
  contractNo: '',
  roomName: '',
  tenantName: '',
  tenantPhone: '',
  rentAmount: 0,
  depositAmount: 0,
  startDate: '',
  endDate: '',
  status: '',
  remark: ''
})
const loading = ref(true)
const feeConfigLoading = ref(false)
const receivableLoading = ref(false)
const generatingReceivables = ref(false)
const feeConfigs = ref([])
const depositLogs = ref([])
const receivableTimeline = ref([])
const receivableStats = computed(() => {
  const totalAmount = receivableTimeline.value.reduce((s, r) => s + (r.amount || 0), 0)
  const totalReceived = receivableTimeline.value.reduce((s, r) => s + (r.received || 0), 0)
  return { totalAmount, totalDue: totalAmount - totalReceived }
})
const monthlyTotal = computed(() => {
  return feeConfigs.value.filter(f => f.isActive && f.billingMode === 'FixedAmount').reduce((s, f) => s + (f.amount || 0), 0)
})
const changeHistory = ref([])

async function fetchContract() {
  loading.value = true
  try {
    const c = await getContract(route.params.id)
    if (c) {
      contract.value = {
        id: c.id,
        contractNo: c.contractNo || '',
        roomName: c.roomFullCode || '',
        tenantName: c.tenants?.length > 0 ? c.tenants[0].tenantName : '',
        tenantPhone: c.tenants?.length > 0 ? c.tenants[0].tenantPhone : '',
        rentAmount: c.rentAmount || 0,
        depositAmount: c.depositAmount || 0,
        startDate: c.startDate || '',
        endDate: c.endDate || '',
        status: c.status || '',
        hasRenewalContract: c.hasRenewalContract || false,
        autoRenew: c.autoRenew !== false,
        remark: ''
      }

      // 费用配置
      feeConfigs.value = (c.feeConfigs || []).map(f => ({
        feeName: f.feeCodeName || f.feeCodeId,
        chargeMethod: f.billingMode === 'MeterBased' ? '按表计量' : '固定金额',
        unitPrice: f.billingMode === 'MeterBased' ? f.unitPrice : f.amount,
        unit: f.unit || '',
        history: []
      }))
    }

    // 押金记录
    try {
      const depRes = await getDeposits({ contractId: route.params.id })
      const depItems = depRes.items || depRes.data || depRes || []
      depositLogs.value = depItems.map(d => ({
        date: d.createdAt?.split('T')[0] || '',
        action: d.type === 'Refund' ? '退还' : d.type === 'Deduct' ? '扣款' : '收取',
        amount: d.amount || 0,
        balance: d.balance || 0,
        remark: d.remark || ''
      }))
    } catch { /* 押金接口暂不可用，保留空列表 */ }

    // 应收时间线
    try {
      const recRes = await getReceivables({ contractId: route.params.id, pageSize: 12 })
      const recItems = recRes.items || recRes.data || recRes || []
      receivableTimeline.value = recItems.map(r => ({
        id: r.id,
        period: r.period || '',
        dueDate: r.dueDate || '',
        amount: r.amount || 0,
        received: r.received || 0,
        status: r.status || 'Pending',
        details: (r.details || r.items || []).map(d => ({
          feeName: d.feeName || d.feeCodeName || '',
          amount: d.amount || 0,
          received: d.received || 0,
          description: d.description || ''
        }))
      }))
    } catch { /* 应收接口暂不可用，保留空列表 */ }

    // 加载费用配置
    await fetchFeeConfigs()
  } catch (e) {
    ElMessage.error('加载合同详情失败')
  } finally {
    loading.value = false
  }
}

onMounted(fetchContract)

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
 * Fee Config Management (CRUD)
 * ================================================================ */
const feeCodeList = ref([])
const availableFeeCodes = ref([])
const showFeeConfigDialog = ref(false)
const feeConfigSaving = ref(false)
const showAdjustDialog = ref(false)
const adjustFeeConfigId = ref(null)
const adjustFeeCodeId = ref(null)
const adjustCurrentAmount = ref(0)
const adjustForm = reactive({ newAmount: 0, effectiveDate: '' })

// 当前生效的费用（按 feeCodeId 去重，取最新一条）
const currentFeeConfigs = computed(() => {
  const map = new Map()
  for (const f of feeConfigs.value) {
    if (!map.has(f.feeCodeId)) {
      map.set(f.feeCodeId, { ...f, history: [] })
    }
  }
  return [...map.values()]
})

async function fetchFeeConfigs() {
  feeConfigLoading.value = true
  try {
    const contractId = contract.value?.id || route.params.id
    if (!contractId) return
    const res = await getContractFeeConfigs(contractId)
    const items = res || []
    feeConfigs.value = items.map(f => ({
      id: f.id,
      contractId: f.contractId,
      feeCodeId: f.feeCodeId,
      feeName: f.feeCodeName || f.feeCode,
      billingMode: f.billingMode,
      amount: f.amount,
      unit: f.unit || '',
      isActive: f.isActive !== false,
      effectiveDate: f.effectiveDate || null,
      expiryDate: f.expiryDate || null,
      history: [],
      loadingHistory: false
    }))
  } catch { /* 静默 */ }
  feeConfigLoading.value = false
  updateAvailableFeeCodes()
}

function updateAvailableFeeCodes() {
  if (feeCodeList.value.length === 0) {
    getFeeCodes({ pageSize: 100 }).then(res => {
      feeCodeList.value = res.items || res.data || res || []
    }).catch(() => {})
  }
  const usedIds = new Set(feeConfigs.value.filter(f => f.isActive).map(f => f.feeCodeId))
  availableFeeCodes.value = feeCodeList.value.filter(f => !usedIds.has(f.id))
}

function onFeeCodeChange(feeCodeId) {
  // 用于 FeeCode 选择后的后续处理
}

// 展开行时加载版本历史
async function onFeeConfigExpand(row) {
  if (row.history?.length > 0 || row.loadingHistory) return
  row.loadingHistory = true
  try {
    const res = await getContractFeeConfigHistory(route.params.id, row.feeCodeId)
    row.history = (res || []).map(h => ({
      amount: h.amount,
      effectiveDate: h.effectiveDate,
      expiryDate: h.expiryDate,
      isActive: h.isActive,
      createdAt: h.createdAt
    }))
  } catch { row.history = [] }
  row.loadingHistory = false
}

// 添加费用
const feeConfigForm = reactive({ feeCodeId: null, amount: 0, effectiveDate: '' })

function openAddFeeConfig() {
  feeConfigForm.feeCodeId = null
  feeConfigForm.amount = 0
  feeConfigForm.effectiveDate = new Date().toISOString().split('T')[0]
  showFeeConfigDialog.value = true
}

async function submitFeeConfig() {
  if (!feeConfigForm.feeCodeId) { ElMessage.warning('请选择收费项目'); return }
  if (!feeConfigForm.amount || feeConfigForm.amount <= 0) { ElMessage.warning('请输入有效金额'); return }
  if (!feeConfigForm.effectiveDate) { ElMessage.warning('请选择生效日期'); return }
  feeConfigSaving.value = true
  try {
    await createContractFeeConfig({
      contractId: contract.value.id,
      feeCodeId: feeConfigForm.feeCodeId,
      amount: feeConfigForm.amount,
      billingMode: 'FixedAmount',
      effectiveDate: feeConfigForm.effectiveDate
    })
    ElMessage.success('费用配置已添加')
    showFeeConfigDialog.value = false
    await fetchFeeConfigs()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
  feeConfigSaving.value = false
}

// 版本化调价
function openAdjustFeeConfig(row) {
  adjustFeeConfigId.value = row.id
  adjustFeeCodeId.value = row.feeCodeId
  adjustCurrentAmount.value = row.amount
  adjustForm.newAmount = row.amount
  adjustForm.effectiveDate = ''
  showAdjustDialog.value = true
}

async function adjustFeeConfig() {
  if (!adjustForm.newAmount || adjustForm.newAmount <= 0) { ElMessage.warning('请输入有效金额'); return }
  if (!adjustForm.effectiveDate) { ElMessage.warning('请选择生效日期'); return }
  if (adjustForm.newAmount === adjustCurrentAmount.value) {
    ElMessage.warning('新价格与当前价格相同，无需调价')
    return
  }
  feeConfigSaving.value = true
  try {
    await adjustContractFeeConfig({
      contractId: contract.value.id,
      feeCodeId: adjustFeeCodeId.value,
      newAmount: adjustForm.newAmount,
      effectiveDate: adjustForm.effectiveDate
    })
    ElMessage.success('调价申请已提交')
    showAdjustDialog.value = false
    await fetchFeeConfigs()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '调价失败')
  }
  feeConfigSaving.value = false
}

// 停用/启用
async function toggleFeeConfig(row) {
  try {
    await updateContractFeeConfig(row.id, {
      amount: row.amount,
      billingMode: row.billingMode,
      unit: row.unit,
      unitPrice: null,
      isActive: !row.isActive
    })
    ElMessage.success(row.isActive ? '已停用' : '已启用')
    await fetchFeeConfigs()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

/* ================================================================
 * Suspend / Resume (API connected)
 * ================================================================ */

/* ================================================================
 * Rent Adjustment (CONTRACT_MODIFY — AmountBased 1~2级)
 * ================================================================ */
const showModifyRent = ref(false)
const rentForm = reactive({ newAmount: 5200, effectiveDate: '', reason: '' })
const contractModifyTypeId = ref(null)
const submittingRent = ref(false)

// 将字符串 ID 转为 GUID 格式（模拟数据使用，已有 GUID 则直接返回）
function toGuidId(id) {
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id)) return id
  const hex = Array.from(String(id)).reduce((h, c) => { const n = c.charCodeAt(0).toString(16); return h + (n.length < 2 ? '0' + n : n) }, '').padEnd(32, '0').slice(0, 32)
  return `${hex.slice(0,8)}-${hex.slice(8,12)}-${hex.slice(12,16)}-${hex.slice(16,20)}-${hex.slice(20,32)}`
}

// 获取 CONTRACT_MODIFY 审批类型 ID（带缓存，不存在则自动创建）
async function ensureContractModifyTypeId() {
  if (contractModifyTypeId.value) return contractModifyTypeId.value
  try {
    const types = await getApprovalTypes()
    let found = types.find(t => t.code === 'CONTRACT_MODIFY')

    if (!found) {
      // 自动创建 CONTRACT_MODIFY 审批类型
      found = await createApprovalType({
        name: '修改合同租金',
        code: 'CONTRACT_MODIFY',
        description: '修改合同租金需要审批，金额越大审批级别越高。'
      })

      // 查找角色 ID
      const roles = await getRoles()
      const opsSup = roles.find(r => r.code === 'OpsSupervisor')
      const deptMgr = roles.find(r => r.code === 'DeptManager')

      // 创建 2 级审批配置
      if (opsSup) {
        await createApprovalLevel(found.id, { level: 1, roleId: opsSup.id, minAmount: 0, maxAmount: 5000 })
      }
      if (deptMgr) {
        await createApprovalLevel(found.id, { level: 2, roleId: deptMgr.id, minAmount: 5000, maxAmount: 99999999 })
      }
    }

    contractModifyTypeId.value = found?.id || null
    return contractModifyTypeId.value
  } catch {
    return null
  }
}

const rentDiff = computed(() => rentForm.newAmount - (contract.value.rentAmount || 0))
const rentApprovalLevel = computed(() => {
  const absDiff = Math.abs(rentDiff.value)
  if (absDiff <= 5000) return '1级(运营主管)'
  return '2级(部门经理)'
})

async function submitRentAdjust() {
  if (!rentForm.newAmount || rentForm.newAmount <= 0) { ElMessage.warning('请输入有效的新租金'); return }
  if (!rentForm.effectiveDate) { ElMessage.warning('请选择生效日期'); return }
  if (!rentForm.reason) { ElMessage.warning('请填写调整原因'); return }

  const approvalTypeId = await ensureContractModifyTypeId()
  if (!approvalTypeId) {
    ElMessage.error('未找到合同租金调整审批类型配置，请联系管理员')
    return
  }

  submittingRent.value = true
  try {
    await submitApproval({
      approvalTypeId: approvalTypeId,
      title: `合同租金调整 - ${contract.value.contractNo}`,
      description: `月租金 ¥${contract.value.rentAmount?.toLocaleString()} → ¥${rentForm.newAmount.toLocaleString()}，差额：${rentDiff.value >= 0 ? '+' : ''}¥${rentDiff.value.toLocaleString()}，生效日期：${rentForm.effectiveDate}，调整原因：${rentForm.reason}`,
      targetEntityId: toGuidId(contract.value.id),
      targetEntityType: 'Contract'
    })

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
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || e?.message || '提交审批失败，请重试')
  } finally {
    submittingRent.value = false
  }
}

/* ================================================================
 * Fee Price Adjustment (CONTRACT_FEE_CHANGE — Fixed 1级)
 * ================================================================ */
const feeAdjustEffectiveDate = ref('')
const feeAdjustReason = ref('')

const feeAdjustItems = reactive([])

const showModifyFee = ref(false)
// 打开弹窗时从 feeConfigs 加载当前价格
function openFeeAdjust() {
  showModifyFee.value = true
  if (feeConfigs.value.length > 0) {
    feeAdjustItems.splice(0, feeAdjustItems.length, ...feeConfigs.value.map(f => ({
      feeName: f.feeName,
      chargeMethod: f.chargeMethod,
      oldPrice: typeof f.unitPrice === 'number' ? f.unitPrice : parseFloat(f.unitPrice) || 0,
      newPrice: typeof f.unitPrice === 'number' ? f.unitPrice : parseFloat(f.unitPrice) || 0,
      unit: f.unit || ''
    })))
  }
}

async function submitFeeAdjust() {
  if (!feeAdjustReason.value) { ElMessage.warning('请填写调价原因'); return }
  if (!feeAdjustEffectiveDate.value) { ElMessage.warning('请选择生效日期'); return }

  const changedItems = feeAdjustItems.filter(item => item.newPrice !== item.oldPrice)
  if (changedItems.length === 0) { ElMessage.warning('没有费用项目价格发生变化'); return }

  // Submit approval for fee change
  const approvalTypeId = await ensureContractModifyTypeId()
  if (approvalTypeId) {
    try {
      const desc = changedItems.map(i =>
        `${i.feeName}: ${i.oldPrice} → ${i.newPrice}${i.unit ? ' (' + i.unit + ')' : ''}`
      ).join('；')
      await submitApproval({
        approvalTypeId,
        title: `合同费用调价 - ${contract.value.contractNo}`,
        description: `调价项目: ${desc}，生效日期: ${feeAdjustEffectiveDate.value}，原因: ${feeAdjustReason.value}`,
        targetEntityId: toGuidId(contract.value.id),
        targetEntityType: 'Contract'
      })
    } catch { /* 静默 */ }
  }

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

  ElMessage.success(`费用调价申请已提交审批，涉及 ${changedItems.length} 项费用`)
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
 * Renew (CONTRACT_RENEW — AmountBased 1~3级)
 * ================================================================ */
const showRenew = ref(false)
const renewForm = reactive({
  rentAmount: 5200,
  endDate: '',
  depositHandling: 'TRANSFER',
  newDeposit: 0,
  remark: ''
})
const renewPreview = ref(null)
const renewLoading = ref(false)
const renewChecks = ref({ paymentStatus: { passed: true }, concurrentApprovals: { hasPending: false } })

async function openRenewDialog() {
  showRenew.value = true
  renewLoading.value = true
  try {
    const preview = await previewRenewal(route.params.id)
    renewPreview.value = preview
    renewChecks.value = preview.checks || {}
    renewForm.rentAmount = preview.defaultRenewalInfo?.currentRentAmount || contract.value.rentAmount
    renewForm.endDate = preview.defaultRenewalInfo?.suggestedEndDate || ''
    renewForm.depositHandling = 'TRANSFER'
    renewForm.newDeposit = contract.value.depositAmount || 0
  } catch (e) {
    ElMessage.error('加载续签预览信息失败')
  } finally {
    renewLoading.value = false
  }
}

const canSubmitRenewal = computed(() => {
  const checks = renewChecks.value
  return checks.paymentStatus?.passed !== false
    && !checks.concurrentApprovals?.hasPending
    && !checks.concurrentApprovals?.alreadyRenewed
})

async function submitRenew() {
  if (!renewForm.rentAmount || !renewForm.endDate) {
    ElMessage.warning('请填写完整的续签信息')
    return
  }
  if (!canSubmitRenewal.value) {
    ElMessage.warning('存在待审批流或欠费，无法提交')
    return
  }
  try {
    const result = await submitRenewal(route.params.id, {
      newRentAmount: renewForm.rentAmount,
      newEndDate: renewForm.endDate,
      depositHandling: renewForm.depositHandling,
      newDepositAmount: renewForm.depositHandling === 'NEW' ? renewForm.newDeposit : null,
      remark: renewForm.remark
    })
    ElMessage.success(`续签申请已提交${result.status === 'Pending' ? '，等待审批' : ''}`)
    showRenew.value = false
    // 刷新合同状态
    contract.value.status = 'PendingApproval'
  } catch (e) {
    ElMessage.error(e?.response?.data?.error || e?.response?.data?.message || '续签提交失败')
  }
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

async function submitTerminate() {
  if (!terminateForm.reason) { ElMessage.warning('请填写终止原因'); return }
  if (!terminateForm.actualEndDate) { ElMessage.warning('请选择实际搬离日'); return }

  try {
    await terminateContract(toGuidId(contract.value.id), { reason: terminateForm.reason })
    contract.value.status = 'Terminated'
    changeHistory.value.unshift({
      date: new Date().toISOString().split('T')[0],
      title: '合同终止',
      detail: `终止原因: ${terminateForm.reason}，搬离日: ${terminateForm.actualEndDate}`,
      operator: '当前用户',
      type: 'danger',
      hollow: false
    })
    ElMessage.success('合同已终止')
    showTerminate.value = false
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '终止失败')
  }
}

/* ================================================================
 * Suspend / Resume
 * ================================================================ */
const showSuspend = ref(false)
const suspendReason = ref('')

async function handleSuspend() {
  if (!suspendReason.value) { ElMessage.warning('请填写暂停原因'); return }
  try {
    await suspendContract(toGuidId(contract.value.id))
    contract.value.status = 'Suspended'
    changeHistory.value.unshift({
      date: new Date().toISOString().split('T')[0],
      title: '合同暂停',
      detail: `暂停原因: ${suspendReason.value}`,
      operator: '当前用户',
      type: 'info',
      hollow: true
    })
    ElMessage.success('合同已暂停')
    showSuspend.value = false
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '暂停失败')
  }
}

async function toggleAutoRenew() {
  const newVal = !contract.value.autoRenew
  try {
    await updateContract(route.params.id, { autoRenew: newVal })
    contract.value.autoRenew = newVal
    ElMessage.success(newVal ? '自动续签已开启' : '自动续签已关闭')
  } catch (e) {
    ElMessage.error('操作失败')
  }
}

function handleResume() {
  ElMessageBox.confirm(`确定恢复合同 ${contract.value.contractNo} 吗？`, '提示').then(async () => {
    try {
      await resumeContract(toGuidId(contract.value.id))
      contract.value.status = 'Active'
      changeHistory.value.unshift({
        date: new Date().toISOString().split('T')[0],
        title: '合同恢复',
        operator: '当前用户',
        type: 'success',
        hollow: true
      })
      ElMessage.success('合同已恢复')
    } catch (e) {
      ElMessage.error(e?.response?.data?.message || '恢复失败')
    }
  }).catch(() => {})
}

async function generateReceivables() {
  try {
    await apiGenerateReceivables({ contractId: contract.value.id })
    ElMessage.success('应收已成功生成')
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '生成应收失败')
  }
}
</script>
<style scoped>
.el-timeline h4 {
  margin: 0 0 4px;
}
</style>
