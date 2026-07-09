<template>
  <div>
    <div class="page-header">
      <h2>合同详情</h2>
      <div class="table-actions">
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
        <!-------- 2a. Recurring Fee Config ------>
        <el-tab-pane label="周期费用" name="recurring">
          <div style="margin-bottom: 12px; display:flex; gap:8px; align-items:center; flex-wrap:wrap;">
            <el-button type="primary" size="small" @click="openFeeAdjust">批量调价</el-button>
            <el-button size="small" @click="openAddFeeConfig">添加费用</el-button>
            <el-button size="small" @click="fetchFeeConfigs" :loading="feeConfigLoading">刷新</el-button>
            <el-tag v-if="monthlyTotal > 0" type="warning" effect="plain">
              月度合计: ¥{{ monthlyTotal.toLocaleString() }}
            </el-tag>
            <span style="color: #909399; font-size: 13px; margin-left:auto;">修改后需提交审批，按生效日期分段计价</span>
          </div>
          <el-table :data="recurringConfigs" v-loading="feeConfigLoading" stripe style="width:100%;" @expand-change="onFeeConfigExpand">
            <el-table-column type="expand" width="30">
              <template #default="{ row }">
                <el-table :data="row.history" size="small" stripe v-loading="row.loadingHistory" style="margin:8px 0;">
                  <el-table-column label="金额" width="100"><template #default="{ row: h }">¥{{ (h.amount || 0).toLocaleString() }}</template></el-table-column>
                  <el-table-column label="生效日期" width="120"><template #default="{ row: h }">{{ h.effectiveDate ? formatDate(h.effectiveDate) : '-' }}</template></el-table-column>
                  <el-table-column label="到期日期" width="120"><template #default="{ row: h }">{{ h.expiryDate ? formatDate(h.expiryDate) : '至今' }}</template></el-table-column>
                  <el-table-column label="状态" width="70"><template #default="{ row: h }"><el-tag :type="h.isActive ? 'success' : 'info'" size="small">{{ h.isActive ? '已启用' : '已过期' }}</el-tag></template></el-table-column>
                  <el-table-column label="创建时间" min-width="140"><template #default="{ row: h }">{{ h.createdAt ? formatDate(h.createdAt) : '' }}</template></el-table-column>
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
                <span style="font-size:13px;">{{ row.effectiveDate ? formatDate(row.effectiveDate) : '-' }} ~ {{ row.expiryDate ? formatDate(row.expiryDate) : '至今' }}</span>
              </template>
            </el-table-column>
            <el-table-column label="状态" width="65" align="center">
              <template #default="{ row }">
                <span :style="{ color: row.isActive ? '#67c23a' : '#909399', fontSize: '12px' }">{{ row.isActive ? '已启用' : '已停用' }}</span>
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

        <!-------- 2b. OneTime Fee Config ------>
        <el-tab-pane label="一次性收费" name="onetime">
          <div style="margin-bottom: 12px; display:flex; gap:8px; align-items:center; flex-wrap:wrap;">
            <el-button type="primary" size="small" @click="openAddOneTimeFee">添加收费</el-button>
            <el-button size="small" @click="fetchFeeConfigs" :loading="feeConfigLoading">刷新</el-button>
          </div>
          <el-table :data="oneTimeConfigs" stripe style="width:100%;" @expand-change="onOneTimeExpand">
            <el-table-column type="expand" width="30">
              <template #default="{ row }">
                <!-- DEPOSIT 行展开后显示押金流水 -->
                <div v-if="row.feeCode === 'DEPOSIT'">
                  <el-table :data="row._depositLogs || []" size="small" stripe v-loading="row._loadingLogs" style="margin:8px 0;">
                    <el-table-column label="日期" width="100"><template #default="{ row: h }">{{ h.date || '-' }}</template></el-table-column>
                    <el-table-column label="操作" width="80"><template #default="{ row: h }"><el-tag :type="h.action === '收取' ? 'success' : h.action === '退还' ? 'danger' : 'warning'" size="small">{{ h.action }}</el-tag></template></el-table-column>
                    <el-table-column label="金额" width="100"><template #default="{ row: h }"><span :style="{ color: h.amount > 0 && h.action === '收取' ? '#67c23a' : '#f56c6c', fontWeight: 'bold' }">{{ h.amount > 0 ? '+' : '' }}¥{{ h.amount?.toLocaleString() }}</span></template></el-table-column>
                    <el-table-column label="余额" width="100"><template #default="{ row: h }">¥{{ h.balance?.toLocaleString() }}</template></el-table-column>
                    <el-table-column label="备注" min-width="150"><template #default="{ row: h }">{{ h.remark || '-' }}</template></el-table-column>
                  </el-table>
                  <span v-if="!row._depositLogs?.length && !row._loadingLogs" style="color:#909399;font-size:13px;padding:8px;">暂无押金流水记录</span>
                </div>
                <!-- 其他 OneTime 费用展开显示空状态 -->
                <div v-else style="padding:8px;color:#909399;font-size:13px;">暂无明细记录</div>
              </template>
            </el-table-column>
            <el-table-column label="收费项目" min-width="120">
              <template #default="{ row }">
                <span :style="{ color: row.isActive ? '#303133' : '#c0c4cc' }">{{ row.feeName }}</span>
              </template>
            </el-table-column>
            <el-table-column label="金额" min-width="120">
              <template #default="{ row }">
                <span v-if="row.isActive" style="font-weight:bold;font-size:14px;">¥{{ (row.amount || 0).toLocaleString() }}</span>
                <span v-else style="color:#c0c4cc;text-decoration:line-through;">¥{{ (row.amount || 0).toLocaleString() }}</span>
              </template>
            </el-table-column>
            <el-table-column label="生效期" min-width="200">
              <template #default="{ row }">
                <span style="font-size:13px;">{{ row.effectiveDate ? formatDate(row.effectiveDate) : '-' }} ~ {{ row.expiryDate ? formatDate(row.expiryDate) : '至今' }}</span>
              </template>
            </el-table-column>
            <el-table-column label="状态" width="65" align="center">
              <template #default="{ row }">
                <span :style="{ color: row.isActive ? '#67c23a' : '#909399', fontSize: '12px' }">{{ row.isActive ? '已启用' : '已停用' }}</span>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!feeConfigLoading && oneTimeConfigs.length === 0" description="暂无一次性收费配置" :image-size="60" style="padding:20px 0;" />
        </el-tab-pane>

        <!-------- 2c. Change History ------>
        <el-tab-pane label="租户" name="tenants">
          <div style="margin-bottom:12px;display:flex;gap:8px;align-items:center;flex-wrap:wrap;">
            <el-button type="primary" size="small" @click="openAddTenantDialog">添加租户</el-button>
            <el-button size="small" @click="fetchContractTenants" :loading="tenantLoading">刷新</el-button>
            <span style="color:#909399;font-size:13px;margin-left:auto;">共 {{ contractTenants.length }} 人</span>
          </div>
          <el-table :data="contractTenants" stripe v-loading="tenantLoading" style="width:100%;">
            <el-table-column type="index" label="#" width="45" />
            <el-table-column prop="tenantName" label="姓名" min-width="90" />
            <el-table-column prop="tenantPhone" label="电话" width="130" />
            <el-table-column prop="idCard" label="身份证" width="180" />
            <el-table-column prop="email" label="邮箱" min-width="150" />
            <el-table-column label="角色" width="80" align="center">
              <template #default="{ row }">
                <el-tag v-if="row.isPrimary" type="success" size="small">主租户</el-tag>
                <span v-else style="color:#909399;">合租</span>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="210" fixed="right">
              <template #default="{ row }">
                <el-button v-if="!row.isPrimary" text size="small" type="primary" @click="setPrimaryTenant(row)">设为主租户</el-button>
                <el-button text size="small" @click="$router.push('/tenants/'+row.tenantId)">详情</el-button>
                <el-button v-if="contractTenants.length>1" text size="small" type="danger" @click="confirmRemoveTenant(row)">解绑</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!tenantLoading && contractTenants.length===0" description="暂无租户，点击「添加租户」创建" :image-size="60" />
        </el-tab-pane>


      

        <!-------- 2d. Tenants ------>
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
              
            </el-timeline-item>
          </el-timeline>
        </el-tab-pane></el-tabs>
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
            <el-button type="primary" size="small" @click="showReceivablePreviewDialog = true">生成应收</el-button>
            <el-button size="small" @click="showSupplementaryFee = true">补充收费</el-button>
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
    <!-- MODAL: Fee Price Adjustment                                   -->
    <!--===============================================================-->
    <el-dialog v-model="showModifyFee" :draggable="true" title="合同费用中途调价" width="820px">
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
        <el-table-column prop="feeName" label="项目" width="120" />
        <el-table-column prop="chargeMethod" label="方式" width="80" />
        <el-table-column label="当前价格" width="120">
          <template #default="{ row }">
            <span v-if="row.oldPrice !== undefined">{{ row.oldPrice }}</span>
            <span v-else style="color: #c0c4cc;">-</span>
          </template>
        </el-table-column>
        <el-table-column label="生效日期" width="160">
          <template #default="{ row }">
            <el-date-picker v-model="row.effectiveDate" type="date" value-format="YYYY-MM-DD" size="small" style="width:115px" :disabled-date="d => row._minDate && d.getTime() < row._minDate.getTime()" />
          </template>
        </el-table-column>
        <el-table-column label="新价格" width="140">
          <template #default="{ row }">
            <el-input-number v-model="row.newPrice" :min="0" :precision="row.chargeMethod === '按表计量' ? 4 : 2" size="small" :step="row.chargeMethod === '按表计量' ? 0.5 : 50" style="width: 100px;" />
            <span v-if="row.unit" style="margin-left: 2px; font-size: 12px; color: #909399;">{{ row.unit }}</span>
          </template>
        </el-table-column>
        <el-table-column label="调幅" width="100">
          <template #default="{ row }">
            <span v-if="row.oldPrice !== undefined && row.oldPrice !== null && row.oldPrice !== 0" :style="{ color: row.newPrice > row.oldPrice ? '#f56c6c' : row.newPrice < row.oldPrice ? '#67c23a' : '#909399' }">
              {{ row.newPrice > row.oldPrice ? '↑' : row.newPrice < row.oldPrice ? '↓' : '→' }}
              {{ Math.abs((row.newPrice - row.oldPrice) / row.oldPrice * 100).toFixed(1) }}%
            </span>
          </template>
        </el-table-column>
      </el-table>
      <el-form style="margin-top: 16px;">
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
          <el-date-picker v-model="feeConfigForm.effectiveDate" type="date" value-format="YYYY-MM-DD" style="width:200px" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showFeeConfigDialog = false">取消</el-button>
        <el-button type="primary" @click="submitFeeConfig" :loading="feeConfigSaving">添加</el-button>
      </template>
    </el-dialog>

    <!-- MODAL: Adjust Fee Config (版本化调价)                          -->
    <el-dialog v-model="showAdjustDialog" :draggable="true" title="费用调价" width="480px">
      <el-alert title="调价后原价格将在生效日前一天自动到期，新价格从生效日起执行。" type="info" show-icon :closable="false" style="margin-bottom:16px;" />
      <el-form label-width="100px">
        <el-form-item label="当前价格">
          <span style="font-weight:bold;font-size:16px;">¥{{ (adjustCurrentAmount || 0).toLocaleString() }}</span>
          <span style="margin-left: 8px; color: #909399; font-size: 12px;">当前生效日: {{ adjustCurrentEffDate || '-' }}</span>
        </el-form-item>
        <el-form-item label="新价格">
          <el-input-number v-model="adjustForm.newAmount" :min="0" :precision="2" style="width:200px" /> 元/月
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="adjustForm.effectiveDate" type="date" value-format="YYYY-MM-DD" style="width:200px" />
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
    <el-dialog v-model="showOtherModify" title="修改合同信息" width="600px">
      <el-alert
        title="起止日期、付款周期等变更需提交审批；租客电话、备注等直接生效。"
        type="info"
        show-icon
        :closable="false"
        style="margin-bottom: 16px;"
      />
      <el-form :model="otherForm" label-width="120px">
        <el-form-item label="起租日期">
          <el-date-picker v-model="otherForm.startDate" type="date" value-format="YYYY-MM-DD" style="width:200px" />
        </el-form-item>
        <el-form-item label="到期日期">
          <el-date-picker v-model="otherForm.endDate" type="date" value-format="YYYY-MM-DD" style="width:200px" />
        </el-form-item>
        <el-form-item label="付款周期">
          <el-select v-model="otherForm.paymentCycle" style="width:200px;">
            <el-option label="月付" value="Monthly" />
            <el-option label="季付" value="Quarterly" />
            <el-option label="年付" value="Yearly" />
          </el-select>
        </el-form-item>
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

        </el-descriptions>
        <el-form :model="renewForm" label-width="120px">
          <el-form-item label="新月租金 (元)">
            <el-input-number v-model="renewForm.rentAmount" :min="0" :precision="2" style="width: 200px;" />
          </el-form-item>
          <el-form-item label="新到期日期">
            <el-date-picker v-model="renewForm.endDate" type="date" value-format="YYYY-MM-DD" style="width: 200px;" />
            <span style="margin-left:8px;color:#909399;font-size:12px;">起租日自动延续：{{ contract.endDate }} 次日</span>
          </el-form-item>
          <el-form-item label="押金处理">
            <el-radio-group v-model="renewForm.depositHandling">
              <el-radio label="TRANSFER">原押金延续</el-radio>
              <el-radio label="NEW">重新收取押金</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item v-if="renewForm.depositHandling === 'NEW'" label="新押金金额">
            <el-input-number v-model="renewForm.newDeposit" :min="0" :precision="2" style="width: 200px;" />

          </el-form-item>
          <!-- 押金处理摘要 -->
          <el-form-item label=" ">
            <div v-if="renewForm.depositHandling === 'NEW'" style="padding:6px 10px;background:#f5f7fa;border-radius:4px;font-size:13px;line-height:1.8;">


            </div>
            <div v-else style="padding:6px 10px;background:#f5f7fa;border-radius:4px;font-size:13px;line-height:1.8;">
              <div>押金：原押金延续至新合同</div>
              <div style="color:#909399;">说明：旧合同押金转出 → 新合同押金转入</div>
            </div>
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
    <el-dialog v-model="showSuspend" title="暂停合同" width="450px">
      <el-alert title="暂停期间将不生成新的应收计划。审批通过后执行暂停操作。" type="info" show-icon :closable="false" style="margin-bottom:16px;" />
      <el-form style="margin-top: 12px;">
        <el-form-item label="暂停原因">
          <el-input v-model="suspendReason" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showSuspend = false">取消</el-button>
        <el-button type="primary" @click="previewSuspend" :loading="suspendPreviewLoading">预览变更</el-button>
      </template>
    </el-dialog>
    <el-dialog v-model="showSuspendPreview" title="暂停合同 — 预览" width="450px">
      <p>暂停原因: {{ suspendReason }}</p>
      <p style="margin-top:8px;">将冻结 <strong>{{ suspendPreviewData?.frozenPeriods?.length || 0 }}</strong> 个月应收计划</p>
      <el-checkbox v-model="suspendConfirmed" style="margin-top:12px;">我已确认以上信息</el-checkbox>
      <template #footer>
        <el-button @click="showSuspendPreview = false">返回修改</el-button>
        <el-button type="warning" @click="submitSuspendApproval" :disabled="!suspendConfirmed" :loading="suspendSubmitting">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Supplementary Fee Dialog -->
    <el-dialog v-model="showSupplementaryFee" title="补充收费" width="550px">
      <el-form label-width="100px">
        <el-form-item label="收费项目">
          <el-select v-model="suppForm.feeCodeId" placeholder="选择收费项目" style="width:100%">
            <el-option v-for="fc in feeCodeList" :key="fc.id" :label="fc.name" :value="fc.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="月金额">
          <el-input-number v-model="suppForm.amount" :min="0" :precision="2" style="width:200px" /> 元
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="suppForm.effectiveDate" type="date" value-format="YYYY-MM-DD" style="width:200px"
            :disabled-date="d => d > new Date()" />
          <span style="margin-left:8px;color:#909399;font-size:12px;">从该日起追溯至当前月</span>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showSupplementaryFee = false">取消</el-button>
        <el-button type="primary" @click="submitSupplementaryFee" :loading="suppSubmitting">提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Receivable Preview Dialog -->
    <el-dialog v-model="showReceivablePreviewDialog" title="生成应收 — 预览" width="700px">
      <el-descriptions :column="2" border style="margin-bottom:16px;">
        <el-descriptions-item label="合同号">{{ contract.contractNo }}</el-descriptions-item>
        <el-descriptions-item label="账期范围">{{ contract.startDate }} ~ {{ contract.endDate }}</el-descriptions-item>
      </el-descriptions>
      <div v-if="receivablePreviewLoading" style="text-align:center;padding:20px;">加载中...</div>
      <template v-else>
        <el-table :data="receivablePreviewItems" stripe size="small" v-if="receivablePreviewItems.length > 0">
          <el-table-column prop="period" label="账期" width="80" />
          <el-table-column prop="feeName" label="费用项目" width="100" />
          <el-table-column label="金额" width="120" align="right">
            <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
          </el-table-column>
          <el-table-column prop="dueDate" label="到期日" width="95" />
        </el-table>
        <div style="text-align:right;margin-top:12px;font-size:15px;">
          应收合计: <strong style="color:#e6a23c;">¥{{ receivablePreviewTotal.toLocaleString() }}</strong>
        </div>
      </template>
      <template #footer>
        <el-button @click="showReceivablePreviewDialog = false">取消</el-button>
        <el-button type="primary" @click="submitReceivableGenerate" :loading="receivableSubmitting">提交审批</el-button>
      </template>
    </el-dialog>
    <!-- Add Tenant Dialog -->
    <el-dialog v-model="showAddTenant" title="添加租户" width="600px">
      <el-tabs v-model="addTenantTab">
        <el-tab-pane label="选择已有租客" name="existing">
          <el-input v-model="tenantSearchKeyword" placeholder="搜索姓名/电话/身份证" style="margin-bottom:12px;">
            <template #append><el-button @click="searchTenants">搜索</el-button></template>
          </el-input>
          <el-table :data="searchResults" stripe max-height="250" v-loading="searchLoading">
            <el-table-column prop="name" label="姓名" width="90" />
            <el-table-column prop="phone" label="电话" width="130" />
            <el-table-column prop="idCard" label="身份证" width="170" />
            <el-table-column label="操作" width="80">
              <template #default="{ row }">
                <el-button size="small" type="primary" @click="selectExistingTenant(row)"
                  :disabled="contractTenants.some(t=>t.tenantId===row.id)">选择</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
        <el-tab-pane label="新建租客" name="new">
          <el-form :model="newTenantForm" label-width="90px">
            <el-form-item label="姓名"><el-input v-model="newTenantForm.name" /></el-form-item>
            <el-form-item label="电话"><el-input v-model="newTenantForm.phone" /></el-form-item>
            <el-form-item label="身份证"><el-input v-model="newTenantForm.idCard" /></el-form-item>
            <el-form-item label="邮箱"><el-input v-model="newTenantForm.email" /></el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>
      <div v-if="pendingAddTenants.length>0" style="margin-top:12px;">
        <span style="font-size:13px;color:#606266;">待添加：</span>
        <el-tag v-for="t in pendingAddTenants" :key="t.id" closable @close="removePendingTenant(t)" style="margin-right:6px;">{{ t.name }}</el-tag>
      </div>
      <el-alert title="添加租户将提交审批，通过后生效" type="info" show-icon :closable="false" style="margin-top:12px;" />
      <template #footer>
        <el-button @click="showAddTenant = false">取消</el-button>
        <el-button type="primary" @click="submitAddTenant" :loading="addTenantLoading" :disabled="pendingAddTenants.length===0">确认提交审批</el-button>
      </template>
    </el-dialog>

    <!-- Remove Tenant Dialog -->
    <el-dialog v-model="showRemoveTenant" title="解绑租客" width="450px">
      <p>确认将租客 <strong>{{ removingTenant?.tenantName }}</strong> 从本合同解绑？</p>
      <el-form style="margin-top:12px;">
        <el-form-item label="解绑原因">
          <el-input v-model="removeTenantReason" type="textarea" :rows="2" placeholder="必填" />
        </el-form-item>
      </el-form>
      <el-alert title="解绑操作将提交审批，通过后生效" type="info" show-icon :closable="false" style="margin-top:12px;" />
      <template #footer>
        <el-button @click="showRemoveTenant = false">取消</el-button>
        <el-button type="primary" @click="submitRemoveTenant" :loading="removeTenantLoading" :disabled="!removeTenantReason">确认提交审批</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { submitApproval, getApprovalTypes, getRoles, createApprovalType, createApprovalLevel, getContract, updateContract, terminateContract, renewContract, suspendContract, resumeContract, feeAdjust, getReceivables, generateReceivables as apiGenerateReceivables, getDeposits, getContractFeeConfigs, createContractFeeConfig, updateContractFeeConfig, adjustContractFeeConfig, getContractFeeConfigHistory, getFeeCodes, previewRenewal, submitRenewal, getRenewalHistory, getRenewalChain, getAllowedOperations, getContractChanges, handleApiError } from '@/api/index.js'

const route = useRoute()
const router = useRouter()
const activeTab = ref('recurring')
watch(activeTab, (tab) => {
  if (tab === 'tenants') fetchContractTenants()
})

/* ================================================================
 * Real Data: Contract from API
 * ================================================================ */
const contract = ref({
  id: route.params.id,
  contractNo: '',
  roomName: '',
  tenantName: '',
  tenantPhone: '',

  startDate: '',
  endDate: '',
  status: '',
  remark: ''
})
const loading = ref(true)
const feeConfigLoading = ref(false)
const receivableLoading = ref(false)
const generatingReceivables = ref(false)

// 租户Tab
const tenantLoading = ref(false)
const contractTenants = ref([])
const showAddTenant = ref(false)
const addTenantTab = ref("existing")
const tenantSearchKeyword = ref("")
const searchResults = ref([])
const searchLoading = ref(false)
const pendingAddTenants = ref([])
const newTenantForm = reactive({ name: "", phone: "", idCard: "", email: "" })
const addTenantLoading = ref(false)
const showRemoveTenant = ref(false)
const removingTenant = ref(null)
const removeTenantReason = ref("")
const removeTenantLoading = ref(false)
const feeConfigs = ref([])
const depositLogs = ref([])
const receivableTimeline = ref([])

// 补充收费
const showSupplementaryFee = ref(false)
const suppSubmitting = ref(false)
const suppForm = reactive({ feeCodeId: null, amount: 0, effectiveDate: '' })

// 生成应收预览
const showReceivablePreviewDialog = ref(false)
const receivablePreviewLoading = ref(false)
const receivableSubmitting = ref(false)
const receivablePreviewItems = ref([])
const receivablePreviewTotal = ref(0)
const receivableStats = computed(() => {
  const totalAmount = receivableTimeline.value.reduce((s, r) => s + (r.amount || 0), 0)
  const totalReceived = receivableTimeline.value.reduce((s, r) => s + (r.received || 0), 0)
  return { totalAmount, totalDue: totalAmount - totalReceived }
})
const recurringConfigs = computed(() => {
  const map = new Map()
  for (const f of feeConfigs.value) {
    if (f.chargeType === 'Recurring' && f.isActive && !f.expiryDate && f.feeCodeId) {
      map.set(f.feeCodeId, { ...f, history: [] })
    }
  }
  for (const f of feeConfigs.value) {
    if (f.chargeType === 'Recurring' && !map.has(f.feeCodeId) && f.feeCodeId) {
      map.set(f.feeCodeId, { ...f, history: [] })
    }
  }
  return [...map.values()]
})
const oneTimeConfigs = computed(() => {
  const map = new Map()
  for (const f of feeConfigs.value) {
    if (f.chargeType === 'OneTime' && f.isActive && !f.expiryDate && f.feeCodeId) {
      map.set(f.feeCodeId, { ...f, history: [], _depositLogs: [], _loadingLogs: false })
    }
  }
  for (const f of feeConfigs.value) {
    if (f.chargeType === 'OneTime' && !map.has(f.feeCodeId) && f.feeCodeId) {
      map.set(f.feeCodeId, { ...f, history: [], _depositLogs: [], _loadingLogs: false })
    }
  }
  return [...map.values()]
})
const monthlyTotal = computed(() => {
  return recurringConfigs.value.filter(f => f.isActive && f.billingMode === 'FixedAmount').reduce((s, f) => s + (f.amount || 0), 0)
})
const nowStr = () => {
  const d = new Date()
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

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
        rentAmount: (c.feeConfigs || []).find(f => f.feeCodeName === '房租费' || f.feeCodeName === 'RENT')?.amount || 0,
        depositAmount: (c.feeConfigs || []).find(f => f.feeCodeName === '押金' || f.feeCodeName === 'DEPOSIT')?.amount || 0,
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
        action: d.action === 'Create' ? '收取'
          : d.action === 'Return' || d.action === 'Refund' ? '退还'
          : d.action === 'Deduct' ? '扣款'
          : d.action === 'TransferOut' ? '押金转出'
          : d.action === 'TransferIn' ? '押金转入'
          : d.action === 'Collection' ? '收取'
          : d.action || '收取',
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
    await fetchChanges()
  } catch (e) {
    ElMessage.error('加载合同详情失败')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchContract()
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
const adjustCurrentEffDate = ref('')
const adjustForm = reactive({ newAmount: 0, effectiveDate: '' })

// 当前生效的费用（按 feeCodeId 去重，取最新一条）
// 当前生效的费用（按 feeCodeId 去重，取最新一条）已由 recurringConfigs / oneTimeConfigs 替代
// 保留此 computed 仅用于 fee adjust 弹窗中的冲突校验
const currentFeeConfigs = computed(() => {
  const map = new Map()
  // 先取当前生效的配置（IsActive=true && ExpiryDate=null）
  for (const f of feeConfigs.value) {
    if (f.isActive && !f.expiryDate && f.feeCodeId) {
      map.set(f.feeCodeId, { ...f, history: [] })
    }
  }
  // 若某个费用项目没有生效中的配置，则取最新的那条
  for (const f of feeConfigs.value) {
    if (!map.has(f.feeCodeId) && f.feeCodeId) {
      map.set(f.feeCodeId, { ...f, history: [] })
    }
  }
  return [...map.values()]
})

async function fetchChanges() {
  try {
    const res = await getContractChanges(route.params.id)
    const typeColor = { RENT_ADJUST: 'warning', FEE_ADJUST: 'warning', TERMINATE: 'danger', SUSPEND: 'info', RESUME: 'success', CONTRACT_CREATE: 'primary', SUPPLEMENTARY_FEE: 'primary', DEPOSIT_CHANGE: 'warning' }
    changeHistory.value = (res || []).map(h => ({
      date: h.createdAt ? (h.createdAt.split('T')[0] || h.createdAt.substring(0, 10)) + ' ' + (h.createdAt.split('T')[1] ? h.createdAt.split('T')[1].substring(0, 5) : '') : '',
      title: h.title,
      detail: h.detail || '',
      operator: h.operatorName || '系统',
      type: typeColor[h.changeType] || 'primary',
      hollow: false,
      changes: (h.oldValue || h.newValue) ? [{ field: '金额', oldValue: h.oldValue ? '¥' + Number(h.oldValue).toFixed(2) : '-', newValue: h.newValue ? '¥' + Number(h.newValue).toFixed(2) : '-' }] : [],
      approval: null
    }))
  } catch { /* 静默 */ }
}

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
      feeCode: f.feeCode || '',
      chargeType: f.chargeType || 'Recurring',
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
  availableFeeCodes.value = feeCodeList.value.filter(f => !usedIds.has(f.id) && f.chargeType === 'Recurring')
}

// 展开一次性收费行时加载押金流水
async function onOneTimeExpand(row) {
  if (row.feeCode === 'DEPOSIT' && !row._depositLogs?.length && !row._loadingLogs) {
    row._loadingLogs = true
    try {
      const depRes = await getDeposits({ contractId: contract.value?.id || route.params.id })
      const depItems = depRes.items || depRes.data || depRes || []
      row._depositLogs = depItems.map(d => ({
        date: d.createdAt?.split('T')[0] || '',
        action: d.action === 'Create' ? '收取'
          : d.action === 'Return' || d.action === 'Refund' ? '退还'
          : d.action === 'Deduct' ? '扣款'
          : d.action === 'TransferOut' ? '押金转出'
          : d.action === 'TransferIn' ? '押金转入'
          : d.action || '收取',
        amount: d.amount || 0,
        balance: d.balance || 0,
        remark: d.remark || ''
      }))
    } catch { row._depositLogs = [] }
    row._loadingLogs = false
  }
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
    handleApiError(e, '操作失败')
  }
  feeConfigSaving.value = false
}

// 版本化调价
function openAdjustFeeConfig(row) {
  adjustFeeConfigId.value = row.id
  adjustFeeCodeId.value = row.feeCodeId
  adjustCurrentAmount.value = row.amount
  adjustCurrentEffDate.value = row.effectiveDate || ''
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

  // ★ 前端预校验：日期区间冲突检测
  try {
    const history = await getContractFeeConfigHistory(contract.value.id, adjustFeeCodeId.value)
    const newEff = new Date(adjustForm.effectiveDate)
    const hasConflict = history.some(cfg => {
      if (cfg.id === adjustFeeConfigId.value) return false
      if (!cfg.effectiveDate) return false
      const cfgExp = cfg.expiryDate ? new Date(cfg.expiryDate) : new Date('9999-12-31')
      return newEff <= cfgExp
    })
    if (hasConflict) {
      ElMessage.error('生效日期与已有费用配置记录冲突，请调整')
      return
    }
    // 生效日期必须晚于当前配置的生效日期
    const currentEff = feeConfigs.value.find(c => c.feeCodeId === adjustFeeCodeId.value && c.isActive && !c.expiryDate)?.effectiveDate
    if (currentEff) {
      const curDate = new Date(currentEff)
      if (newEff <= curDate) {
        ElMessage.error('生效日期必须晚于当前配置的生效日期 ' + currentEff)
        return
      }
    }
  } catch (e) {
    ElMessage.warning('校验日期冲突失败，请稍后重试')
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
    handleApiError(e, '调价失败')
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


// 将字符串 ID 转为 GUID 格式（模拟数据使用，已有 GUID 则直接返回）
function toGuidId(id) {
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id)) return id
  const hex = Array.from(String(id)).reduce((h, c) => { const n = c.charCodeAt(0).toString(16); return h + (n.length < 2 ? '0' + n : n) }, '').padEnd(32, '0').slice(0, 32)
  return `${hex.slice(0,8)}-${hex.slice(8,12)}-${hex.slice(12,16)}-${hex.slice(16,20)}-${hex.slice(20,32)}`
}

/* ================================================================
 * Fee Price Adjustment (CONTRACT_FEE_CHANGE — Fixed 1级)
 * ================================================================ */
const showModifyFee = ref(false)
const feeAdjustItems = reactive([])
const feeAdjustEffectiveDate = ref('')
const feeAdjustReason = ref('')
/** 费用调价 DatePicker 最小可选日期（当前配置最早生效日 + 2天） */
const feeAdjustMinDate = computed(() => {
  const dates = feeConfigs.value
    .filter(f => f.isActive && f.effectiveDate)
    .map(f => new Date(f.effectiveDate))
    .filter(d => !isNaN(d.getTime()))
  if (dates.length === 0) return null
  const min = new Date(Math.min(...dates))
  min.setDate(min.getDate() + 2)
  return min
})
function disabledFeeDate(time) {
  if (!feeAdjustMinDate.value) return false
  return time.getTime() < feeAdjustMinDate.value.getTime()
}

function openFeeAdjust() {
  showModifyFee.value = true
  const activeConfigs = feeConfigs.value.filter(f => f.isActive && f.chargeType === 'Recurring')
  if (activeConfigs.length > 0) {
    feeAdjustItems.splice(0, feeAdjustItems.length, ...activeConfigs.map(f => {
      const effDate = f.effectiveDate ? new Date(f.effectiveDate) : null
      const minDate = effDate ? new Date(effDate.getTime() + 86400000) : null
      const defaultEff = minDate ? minDate.toISOString().split('T')[0] : ''
      return {
        feeCodeId: f.feeCodeId || '',
        feeName: f.feeName,
        chargeMethod: f.billingMode === 'MeterBased' ? '按表计量' : '固定金额',
        oldPrice: typeof f.amount === 'number' ? f.amount : parseFloat(f.amount) || 0,
        oldPriceVal: typeof f.amount === 'number' ? f.amount : parseFloat(f.amount) || 0,
        newPrice: typeof f.amount === 'number' ? f.amount : parseFloat(f.amount) || 0,
        unit: f.unit || '',
        effectiveDate: defaultEff,
        _minDate: minDate,
        _originalEff: f.effectiveDate || ''
      }
    }))
  }
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

async function submitFeeAdjust() {
  if (!feeAdjustReason.value) { ElMessage.warning('请填写调价原因'); return }
  const changedItems = feeAdjustItems.filter(item => item.newPrice !== item.oldPrice)
  if (changedItems.length === 0) { ElMessage.warning('没有费用项目价格发生变化'); return }

  // ★ 前端预校验：日期区间冲突检测（从DB捞取历史配置，前端计算）
  for (const item of changedItems) {
    try {
      const history = await getContractFeeConfigHistory(contract.value.id, item.feeCodeId)
      const currentActive = feeConfigs.value.find(c => c.feeCodeId === item.feeCodeId && c.isActive && !c.expiryDate)
      if (!item.effectiveDate) { ElMessage.error(`请选择「${item.feeName}」的生效日期`); return }
      const newEff = new Date(item.effectiveDate)
      const hasConflict = history.some(cfg => {
        if (cfg.id === currentActive?.id) return false
        if (!cfg.effectiveDate) return false
        const cfgExp = cfg.expiryDate ? new Date(cfg.expiryDate) : new Date('9999-12-31')
        return newEff <= cfgExp
      })
      if (hasConflict) {
        ElMessage.error(`「${item.feeName}」的生效日期与已有记录冲突，请调整生效日期`)
        return
      }
      // 生效日期必须晚于当前配置的生效日期
      if (currentActive?.effectiveDate && newEff <= new Date(currentActive.effectiveDate)) {
        ElMessage.error(`「${item.feeName}」的生效日期必须晚于当前配置的生效日期 ${currentActive.effectiveDate}`)
        return
      }
    } catch (e) {
      ElMessage.warning(`「${item.feeName}」校验日期冲突失败，请稍后重试`)
      return
    }
  }

  try {
    const items = changedItems.map(i => ({
      feeCodeId: i.feeCodeId || '',
      feeName: i.feeName,
      oldAmount: i.oldPriceVal || i.oldPrice,
      newAmount: i.newPrice,
      billingMode: i.chargeMethod === '按表计量' ? 'MeterBased' : 'FixedAmount',
      unit: i.unit || '',
      effectiveDate: i.effectiveDate || ''
    }))

    const res = await feeAdjust(toGuidId(contract.value.id), {
      reason: feeAdjustReason.value,
      items
    })

    changedItems.forEach(item => {
      const config = feeConfigs.value.find(c => c.feeName === item.feeName)
      if (config) {
        if (!config.history) config.history = []
        config.history.unshift({
          price: (item.chargeMethod === '固定金额' ? '¥' : '') + item.newPrice + (item.unit || ''),
          date: (item.effectiveDate || '新') + '生效'
        })
      }
      changeHistory.value.unshift({
        date: nowStr(),
        title: `${item.feeName}调价审批中`,
        detail: `${item.feeName}: ${Number(item.oldPrice).toFixed(2)} → ${Number(item.newPrice).toFixed(2)}（${feeAdjustReason.value}）`,
        operator: '当前用户',
        type: 'warning',
        hollow: false,
        changes: [{ field: item.feeName, oldValue: String(item.oldPrice), newValue: String(item.newPrice) }],
        approval: { status: '审批中', level: '运营主管(1级)' }
      })
    })

    ElMessage.success(res?.message || `费用调价申请已提交审批，涉及 ${changedItems.length} 项费用`)
    showModifyFee.value = false
    await fetchFeeConfigs()
  } catch (e) {
    handleApiError(e, '提交失败')
  }
}

/* ================================================================
 * Other Modify (CONTRACT_MODIFY_OTHER — Fixed 0级)
 * ================================================================ */
const showOtherModify = ref(false)
const otherForm = reactive({
  startDate: '',
  endDate: '',
  paymentCycle: 'Monthly',
  tenantPhone: '13800138001',
  paymentDueDay: 5,
  allowDepositAsLastRent: false,
  remark: ''
})

async function submitOtherModify() {
  try {
    const res = await fetch('/api/contracts/' + route.params.id + '/modifysubmit', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        startDate: otherForm.startDate || null,
        endDate: otherForm.endDate || null,
        paymentCycle: otherForm.paymentCycle,
        tenantPhone: otherForm.tenantPhone,
        paymentDueDay: otherForm.paymentDueDay,
        allowDepositAsLastRent: otherForm.allowDepositAsLastRent
      })
    }).then(r => r.json())
    if (res.status === 'PendingApproval') {
      ElMessage.success('修改申请已提交审批')
    } else {
      ElMessage.success('合同信息已更新')
    }
    showOtherModify.value = false
    await fetchContract()
  } catch (e) {
    ElMessage.error('提交失败')
  }
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
    renewForm.rentAmount = preview.defaultRenewalInfo?.currentRentAmount || 0
    renewForm.endDate = preview.defaultRenewalInfo?.suggestedEndDate || ''
    renewForm.depositHandling = 'TRANSFER'
    renewForm.newDeposit = 20
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
    handleApiError(e, '续签提交失败')
  }
}

/* ================================================================
 * Terminate (CONTRACT_TERMINATE — AmountBased 1~3级)
 * ================================================================ */
const showSuspend = ref(false)
const suspendReason = ref('')
const showSuspendPreview = ref(false)
const suspendPreviewLoading = ref(false)
const suspendSubmitting = ref(false)
const suspendPreviewData = ref(null)
const suspendConfirmed = ref(false)
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
    const res = await terminateContract(toGuidId(contract.value.id), {
      terminateType: terminateForm.type || 'EARLY',
      actualEndDate: terminateForm.actualEndDate,
      depositReturn: terminateForm.depositReturn || 'FULL',
      reason: terminateForm.reason
    })
    if (res?.status === 'Pending' || res?.id) {
      changeHistory.value.unshift({
        date: nowStr(),
        title: '合同终止审批中',
        detail: `终止原因: ${terminateForm.reason}，搬离日: ${terminateForm.actualEndDate}`,
        operator: '当前用户',
        type: 'danger',
        hollow: false
      })
      ElMessage.success('终止申请已提交审批')
    } else {
      contract.value.status = 'Terminated'
      changeHistory.value.unshift({
        date: nowStr(),
        title: '合同终止',
        detail: `终止原因: ${terminateForm.reason}，搬离日: ${terminateForm.actualEndDate}`,
        operator: '当前用户',
        type: 'danger',
        hollow: false
      })
      ElMessage.success('合同已终止')
    }
    showTerminate.value = false
  } catch (e) {
    handleApiError(e, '终止失败')
  }
}

async function previewSuspend() {
  if (!suspendReason.value) { ElMessage.warning('请填写暂停原因'); return }
  suspendPreviewLoading.value = true
  try {
    const res = await fetch('/api/contracts/' + route.params.id + '/suspendpreview', {
      method: 'POST', headers: { 'Content-Type': 'application/json' }
    }).then(r => r.json())
    suspendPreviewData.value = res
    showSuspend.value = false
    showSuspendPreview.value = true
  } catch (e) {
    ElMessage.error('预览失败')
  }
  suspendPreviewLoading.value = false
}

async function submitSuspendApproval() {
  suspendSubmitting.value = true
  try {
    const res = await fetch('/api/contracts/' + route.params.id + '/suspendsubmit', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason: suspendReason.value })
    }).then(r => r.json())
    if (res.status === 'PendingApproval') {
      ElMessage.success('暂停申请已提交审批')
    } else {
      contract.value.status = 'Suspended'
      ElMessage.success('合同已暂停')
    }
    showSuspendPreview.value = false
  } catch (e) { ElMessage.error('提交失败') }
  suspendSubmitting.value = false
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
        date: nowStr(),
        title: '合同恢复',
        operator: '当前用户',
        type: 'success',
        hollow: true
      })
      ElMessage.success('合同已恢复')
    } catch (e) {
      handleApiError(e, '恢复失败')
    }
  }).catch(() => {})
}

async function submitSupplementaryFee() {
  if (!suppForm.feeCodeId) { ElMessage.warning('请选择收费项目'); return }
  if (!suppForm.amount || suppForm.amount <= 0) { ElMessage.warning('请输入有效金额'); return }
  if (!suppForm.effectiveDate) { ElMessage.warning('请选择生效日期'); return }
  suppSubmitting.value = true
  try {
    const res = await fetch('/api/contracts/' + route.params.id + '/supplementaryfee/request', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ feeCodeId: suppForm.feeCodeId, amount: suppForm.amount, effectiveDate: suppForm.effectiveDate })
    }).then(r => r.json())
    if (res.status === 'PendingApproval' || res.status === 'Completed') {
      ElMessage.success('补充收费请求已提交' + (res.status === 'PendingApproval' ? '审批' : ''))
      showSupplementaryFee.value = false
      await fetchReceivables()
    } else {
      ElMessage.error(res.message || '提交失败')
    }
  } catch (e) { ElMessage.error('提交失败') }
  suppSubmitting.value = false
}


// === 租户Tab函数 ===
async function fetchContractTenants() {
  tenantLoading.value = true
  try {
    const { getContractTenants } = await import('@/api')
    const res = await getContractTenants(route.params.id)
    contractTenants.value = res.tenants || []
  } catch { contractTenants.value = [] }
  tenantLoading.value = false
}
async function openAddTenantDialog() {
  showAddTenant.value = true
  pendingAddTenants.value = []
  tenantSearchKeyword.value = ''
  searchResults.value = []
}
async function searchTenants() {
  searchLoading.value = true
  try {
    const { getTenants } = await import('@/api')
    const res = await getTenants({ keyword: tenantSearchKeyword.value || undefined, pageSize: 50 })
    const list = Array.isArray(res) ? res : (res.items || res.data || [])
    searchResults.value = list.filter(t => !contractTenants.value.some(ct => ct.tenantId === t.id))
  } catch { searchResults.value = [] }
  searchLoading.value = false
}
function selectExistingTenant(row) {
  if (!pendingAddTenants.value.some(t => t.id === row.id))
    pendingAddTenants.value.push({ id: row.id, name: row.name, phone: row.phone })
}
function removePendingTenant(t) {
  pendingAddTenants.value = pendingAddTenants.value.filter(x => x.id !== t.id)
}
async function submitAddTenant() {
  if (pendingAddTenants.value.length === 0) return
  addTenantLoading.value = true
  try {
    const { addContractTenant } = await import('@/api')
    for (const t of pendingAddTenants.value)
      await addContractTenant(route.params.id, { tenantId: t.id, isPrimary: false })
    ElMessage.success('添加租户申请已提交审批')
    showAddTenant.value = false; pendingAddTenants.value = []
    await fetchContractTenants()
  } catch (e) { ElMessage.error('提交失败') }
  addTenantLoading.value = false
}
async function setPrimaryTenant(row) {
  try {
    const { setContractPrimaryTenant } = await import('@/api')
    await setContractPrimaryTenant(route.params.id, row.tenantId)
    ElMessage.success('主租户已更新')
    await fetchContractTenants()
  } catch (e) { ElMessage.error('设置失败') }
}
function confirmRemoveTenant(row) {
  removingTenant.value = row; removeTenantReason.value = ''; showRemoveTenant.value = true
}
async function submitRemoveTenant() {
  if (!removingTenant.value || !removeTenantReason.value) return
  removeTenantLoading.value = true
  try {
    const { removeContractTenant } = await import('@/api')
    await removeContractTenant(route.params.id, removingTenant.value.tenantId, { reason: removeTenantReason.value })
    ElMessage.success('解绊申请已提交审批')
    showRemoveTenant.value = false; await fetchContractTenants()
  } catch (e) { ElMessage.error('提交失败') }
  removeTenantLoading.value = false
}

async function generateReceivables() {
  // Show preview dialog and load preview data
  showReceivablePreviewDialog.value = true
  receivablePreviewLoading.value = true
  try {
    const res = await fetch('/api/receivables/preview', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ contractId: contract.value.id })
    }).then(r => r.json())
    receivablePreviewItems.value = res.items || []
    receivablePreviewTotal.value = res.totalAmount || 0
  } catch (e) {
    receivablePreviewItems.value = []
    receivablePreviewTotal.value = 0
  }
  receivablePreviewLoading.value = false
}

async function submitReceivableGenerate() {
  receivableSubmitting.value = true
  try {
    const res = await fetch('/api/receivables/generaterequest', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ contractId: contract.value.id })
    }).then(r => r.json())
    if (res.status === 'PendingApproval' || res.message) {
      ElMessage.success(res.message || '应收生成请求已提交审批')
      showReceivablePreviewDialog.value = false
      await fetchReceivables()
    } else {
      ElMessage.error(res.message || '提交失败')
    }
  } catch (e) { ElMessage.error('提交失败') }
  receivableSubmitting.value = false
}

// Change Requests removed

/** 格式化日期（仅日期）为 yyyy-MM-dd，兼容 Date 对象/时间戳/各种字符串 */
function formatDate(d) {
  if (!d) return ''
  if (typeof d === 'string' && /^\d{4}-\d{2}-\d{2}/.test(d)) return d.slice(0, 10)
  const dt = new Date(d)
  if (!isNaN(dt.getTime())) {
    const pad = n => String(n).padStart(2, '0')
    return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}`
  }
  // 兜底：英文月份格式提取
  if (typeof d === 'string') {
    const m = d.match(/\b(\d{4})\b.*\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b.*\b(\d{1,2})\b/i)
      || d.match(/\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b.*\b(\d{1,2})\b.*\b(\d{4})\b/i)
    if (m) {
      const monthMap = { Jan:'01', Feb:'02', Mar:'03', Apr:'04', May:'05', Jun:'06',
        Jul:'07', Aug:'08', Sep:'09', Oct:'10', Nov:'11', Dec:'12' }
      const year = m[1].length === 4 ? m[1] : m[3]
      const month = monthMap[(m[1].length === 4 ? m[2] : m[1]).toLowerCase()]
      const day = String(m[1].length === 4 ? m[3] : m[2]).padStart(2, '0')
      if (year && month) return `${year}-${month}-${day}`
    }
    const fallback = d.match(/(\d{4})[^\d](\d{1,2})[^\d](\d{1,2})/)
    if (fallback) return `${fallback[1]}-${String(fallback[2]).padStart(2,'0')}-${String(fallback[3]).padStart(2,'0')}`
    return d.slice(0, 10)
  }
  return ''
}
</script>
<style scoped>
.el-timeline h4 {
  margin: 0 0 4px;
}
</style>
