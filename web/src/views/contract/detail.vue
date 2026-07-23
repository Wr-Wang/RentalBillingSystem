<!--
  =========================================================================
  合同详情（detail.vue）

  页面结构：
    ┌─ 操作按钮栏 ─────────────────────────────────────────────────┐
    │  费用调价 ｜ 续签 ｜ 终止 ｜  修改信息 ｜ 返回           │
    └────────────────────────────────────────────────────────────────┘
    ┌─ 基本信息卡片 ───────────────────────────────────────────────┐
    │  合同号 / 房屋 / 租客 / 起租 / 到期 / 付款周期 / 自动续签     │
    └────────────────────────────────────────────────────────────────┘
    ┌─ Tab 页 ─────────────────────────────────────────────────────┐
    │  周期费用 | 一次性费用 | 押金 | 变更历史                     │
    │  ┌────────────────────────────────────────────────────────┐  │
    │  │ (各 Tab 内容)                                          │  │
    │  └────────────────────────────────────────────────────────┘  │
    └────────────────────────────────────────────────────────────────┘

  功能点：
    - 费用调价（审批驱动）
    - 续签（审批驱动）
    - 终止（审批驱动）
    - 暂停/恢复
    - 修改合同信息（审批驱动）
    - 应收/账单/凭证查看
    - 时间线查看
  =========================================================================
-->
<template>
  <div>
    <!-- ★ 页面头部 + 操作按钮（按状态动态显示）-->
    <div class="page-header">
      <h2>合同详情</h2>
      <div class="table-actions">
        <el-button v-if="isActive || contract.status === 'Suspended'" type="warning" @click="openFeeAdjust">费用调价</el-button>
        <el-button v-if="isActive && !contract.hasRenewalContract" type="primary" @click="openRenewDialog">续签</el-button>
        <el-button v-if="isActive" type="danger" @click="showTerminate = true">终止合同</el-button>
        <el-button v-if="isActive || contract.status === 'Suspended'" type="primary" @click="showOtherModify = true">修改信息</el-button>
        <el-button @click="$router.back()">返回</el-button>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════
    1. 基本信息卡片
    ═══════════════════════════════════════════════════════════════════ -->
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
        <el-descriptions-item label="到期日期">{{ contract.endDate || '不限' }}</el-descriptions-item>
        <el-descriptions-item label="押金抵最后月租">{{ contract.allowDepositAsLastRent ? '是' : '否' }}</el-descriptions-item>
        <el-descriptions-item label="自动续签">
          <el-tag :type="contract.autoRenew ? 'success' : 'info'" size="small" style="cursor:pointer;" @click="toggleAutoRenew">
            {{ contract.autoRenew ? '已开启' : '已关闭' }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="租客电话" :span="2">{{ contract.tenantPhone || '-' }}</el-descriptions-item>
        <el-descriptions-item label="欠款余额">
          <span :style="{ color: (contract.outstandingBalance || 0) > 0 ? '#f56c6c' : '#67c23a', fontWeight: 'bold' }">
            ¥{{ (contract.outstandingBalance || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}
          </span>
        </el-descriptions-item>
        <el-descriptions-item label="预存金额">
          <span :style="{ color: (contract.prepaidBalance || 0) > 0 ? '#409eff' : '#c0c4cc', fontWeight: 'bold' }">
            ¥{{ (contract.prepaidBalance || 0).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}
          </span>
        </el-descriptions-item>
        <el-descriptions-item label=" " />
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
          <el-table :data="oneTimeConfigs" stripe style="width:100%;">
            <el-table-column label="收费项目" min-width="160">
              <template #default="{ row }">
                <span :style="{ color: row.isActive ? '#303133' : '#c0c4cc' }">{{ row.feeName }}</span>
              </template>
            </el-table-column>
            <el-table-column label="金额" min-width="160">
              <template #default="{ row }">
                <span v-if="row.isActive" style="font-weight:bold;font-size:14px;">¥{{ (row.amount || 0).toLocaleString() }}</span>
                <span v-else style="color:#c0c4cc;text-decoration:line-through;">¥{{ (row.amount || 0).toLocaleString() }}</span>
              </template>
            </el-table-column>
            <el-table-column label="状态" width="80" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isActive ? 'success' : 'info'" size="small">{{ row.isActive ? '已启用' : '已停用' }}</el-tag>
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
    <!-- 3. Receivable Timeline (按账期分组的卡片式设计)                  -->
    <!--===============================================================-->
    <el-card v-loading="receivableLoading" :body-style="{ paddingBottom: groupedTimeline.length === 0 ? '20px' : '0' }">
      <template #header>
        <div class="timeline-header">
          <span>应收时间线</span>
          <div class="timeline-actions">
            <span class="timeline-stats" v-if="receivableStats.totalAmount > 0">
              应收合计: <strong class="stat-amount">¥{{ receivableStats.totalAmount.toLocaleString() }}</strong>
              <span class="stat-sep">|</span>
              已入账: <strong class="stat-posted">¥{{ receivableStats.totalPosted.toLocaleString() }}</strong>
            </span>
            <el-button type="primary" size="small" @click="generateReceivables()">生成应收</el-button>
            <el-button size="small" @click="showSupplementaryFee = true">补充收费</el-button>
          </div>
        </div>
      </template>

      <!-- 空状态 -->
      <el-empty v-if="!receivableLoading && groupedTimeline.length === 0"
        description="暂无应收数据，点击「生成应收」创建" :image-size="60" />

      <!-- 分组时间线 -->
      <div v-else-if="!receivableLoading" class="timeline-container">
        <div v-for="(group, gi) in groupedTimeline" :key="group.period" class="period-group">
          <!-- 时间线连接器 -->
          <div class="period-connector">
            <div class="period-dot" :class="group.statusClass"></div>
            <div v-if="gi < groupedTimeline.length - 1" class="period-line"></div>
          </div>

          <!-- 账期卡片 -->
          <div class="period-card" :class="'card-' + group.statusClass">
            <div class="period-card-header">
              <div class="period-card-title">
                <span class="period-label">{{ group.period }}</span>
                <el-tag :type="group.statusTagType" size="small" effect="dark" class="period-status-badge">
                  {{ group.statusText }}
                </el-tag>
                <span class="period-due-label">到期日 {{ formatDate(group.dueDate) }}</span>
                <el-tag v-if="group.allPosted" size="small" type="success" effect="plain" style="margin-left:4px;">GL已入账</el-tag>
                <el-tag v-else-if="group.somePosted" size="small" type="warning" effect="plain" style="margin-left:4px;">GL部分入账</el-tag>
              </div>
              <div class="period-card-amounts">
                <span class="period-total">应收 ¥{{ group.totalAmount.toLocaleString() }}</span>
                <span v-if="group.receiptTotal > 0" class="period-received">已收 ¥{{ group.receiptTotal.toLocaleString() }}</span>
                <span v-if="group.unpaid > 0" class="period-unpaid">待收 ¥{{ group.unpaid.toLocaleString() }}</span>
                <span v-else-if="group.receiptTotal > 0" class="period-settled">已结清</span>
              </div>
            </div>

            <!-- 已入账明细 -->
            <div v-if="group.postedItems.length > 0" class="period-gl-section">
              <div class="period-gl-header posted-header">
                <span>已入账</span>
                <span class="period-gl-amt">¥{{ group.postedItems.reduce((s, i) => s + (i.amount || 0), 0).toLocaleString() }}</span>
              </div>
              <el-table :data="group.postedItems" size="small" stripe class="period-detail-table" :show-header="group.postedItems.length > 1">
                <el-table-column label="费用项目" min-width="120">
                  <template #default="{ row }">
                    <span>{{ row.feeName || row.entryType || '-' }}</span>
                    <el-tag v-if="row.chargeType === 'OneTime'" size="small" type="warning" effect="plain" style="margin-left:4px;">一次性</el-tag>
                    <el-tag v-else-if="row.chargeType === 'Recurring'" size="small" type="primary" effect="plain" style="margin-left:4px;">周期</el-tag>
                  </template>
                </el-table-column>
                <el-table-column label="金额" width="130" align="right">
                  <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
                </el-table-column>
                <el-table-column label="账单月" width="75" align="center">
                  <template #default="{ row }"><span>{{ row.billMonth || '-' }}</span></template>
                </el-table-column>
                <el-table-column label="出账时间" min-width="140" show-overflow-tooltip>
                  <template #default="{ row }">{{ row.billedAt ? formatTime(row.billedAt) : '-' }}</template>
                </el-table-column>
              </el-table>
            </div>

            <!-- 未入账明细 -->
            <div v-if="group.unpostedItems.length > 0" class="period-gl-section">
              <div class="period-gl-header unposted-header">
                <span>未入账</span>
                <span class="period-gl-amt">¥{{ group.unpostedItems.reduce((s, i) => s + (i.amount || 0), 0).toLocaleString() }}</span>
              </div>
              <el-table :data="group.unpostedItems" size="small" stripe class="period-detail-table" :show-header="group.unpostedItems.length > 1">
                <el-table-column label="费用项目" min-width="120">
                  <template #default="{ row }">
                    <span>{{ row.feeName || row.entryType || '-' }}</span>
                    <el-tag v-if="row.chargeType === 'OneTime'" size="small" type="warning" effect="plain" style="margin-left:4px;">一次性</el-tag>
                    <el-tag v-else-if="row.chargeType === 'Recurring'" size="small" type="primary" effect="plain" style="margin-left:4px;">周期</el-tag>
                  </template>
                </el-table-column>
                <el-table-column label="金额" width="130" align="right">
                  <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
                </el-table-column>
                <el-table-column label="账单月" width="75" align="center">
                  <template #default="{ row }"><span>{{ row.billMonth || '-' }}</span></template>
                </el-table-column>
                <el-table-column label="出账时间" min-width="140" show-overflow-tooltip>
                  <template #default="{ row }">{{ row.billedAt ? formatTime(row.billedAt) : '-' }}</template>
                </el-table-column>
              </el-table>
            </div>

            <!-- 收款记录 -->
            <div v-if="group.receiptItems.length > 0" class="period-receipt-section">
              <div class="period-receipt-header">
                <span class="period-receipt-icon">📥</span>
                <span>收款</span>
                <span class="period-receipt-total">
                  已收 ¥{{ (group.receiptTotal || 0).toLocaleString() }}
                </span>
              </div>
              <el-table :data="group.receiptItems" size="small" stripe class="period-detail-table">
                <el-table-column label="收据号" min-width="140">
                  <template #default="{ row }">{{ row.receiptNo }}</template>
                </el-table-column>
                <el-table-column label="金额" width="130" align="right">
                  <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
                </el-table-column>
                <el-table-column label="收款日期" width="100" align="center">
                  <template #default="{ row }">{{ formatDate(row.receivedDate) }}</template>
                </el-table-column>
                <el-table-column label="状态" width="70" align="center">
                  <template #default="{ row }">
                    <el-tag size="small" type="success">{{ row.status === 'Confirmed' ? '已确认' : row.status }}</el-tag>
                  </template>
                </el-table-column>
              </el-table>
            </div>
          </div>
        </div>
      </div>
    </el-card>

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
    <el-dialog :draggable="true" v-model="showFeeConfigDialog" :title="addFeeDialogMode === 'OneTime' ? '添加收费' : '添加费用配置'" width="480px">
      <el-form :model="feeConfigForm" label-width="100px">
        <el-form-item label="收费项目">
          <el-select v-model="feeConfigForm.feeCodeId" placeholder="选择收费项目" style="width:100%">
            <el-option v-for="fc in availableFeeCodes" :key="fc.id" :label="fc.name" :value="fc.id" />
          </el-select>
          <span style="margin-left:8px;color:#909399;font-size:12px;">仅显示{{ addFeeDialogMode === 'OneTime' ? '一次性' : '周期性' }}费用</span>
        </el-form-item>
        <el-form-item :label="addFeeDialogMode === 'OneTime' ? '金额' : '月金额'">
          <el-input-number v-model="feeConfigForm.amount" :min="0" :precision="2" style="width:200px" /> 元
        </el-form-item>
        <el-form-item label="生效日期">
          <el-date-picker v-model="feeConfigForm.effectiveDate" type="date" value-format="YYYY-MM-DD" style="width:200px" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showFeeConfigDialog = false">取消</el-button>
        <el-button type="primary" @click="submitFeeConfig" :loading="feeConfigSaving">提交审批</el-button>
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
    <el-dialog :draggable="true" v-model="showOtherModify" title="修改合同信息" width="600px">
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
        <el-button type="primary" @click="submitOtherModify">提交审批</el-button>
      </template>
    </el-dialog>

    <!--===============================================================-->
    <!-- MODAL: Renew Contract (审批驱动)                               -->
    <!--===============================================================-->
    <el-dialog :draggable="true" v-model="showRenew" title="合同续签" width="620px" :before-close="() => showRenew = false">
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
          <el-descriptions-item label="原到期日">{{ contract.endDate || '不限' }}</el-descriptions-item>

        </el-descriptions>
        <el-form :model="renewForm" label-width="120px">
          <el-form-item label="新月租金 (元)">
            <el-input-number v-model="renewForm.rentAmount" :min="0" :precision="2" style="width: 200px;" />
          </el-form-item>
          <el-form-item label="新到期日期" :required="true">
            <el-date-picker v-model="renewForm.endDate" type="date" value-format="YYYY-MM-DD" style="width: 200px;" placeholder="请选择到期日期" />
            <span style="margin-left:8px;color:#909399;font-size:12px;">起租日自动延续为 {{ contract.endDate }} 次日，到期日须晚于起租日</span>
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
    <el-dialog :draggable="true" v-model="showTerminate" title="合同终止" width="520px">
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

    <!-- Supplementary Fee Dialog -->
    <el-dialog :draggable="true" v-model="showSupplementaryFee" title="补充收费" width="550px">
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
    <el-dialog :draggable="true" v-model="showReceivablePreviewDialog" title="生成应收 — 预览" width="720px">
      <!-- Loading -->
      <div v-if="receivablePreviewLoading" style="text-align:center;padding:40px 0;">
        <el-icon class="is-loading" :size="32"><Loading /></el-icon>
        <p style="margin-top:12px;color:#909399;">正在计算应收...</p>
      </div>

      <template v-else>
        <!-- Info Card -->
        <div style="padding:12px 16px;background:#f5f7fa;border-radius:6px;margin-bottom:20px;">
          <el-row :gutter="24">
            <el-col :span="12"><span style="color:#909399;font-size:13px;">合同号</span><br><strong>{{ contract.contractNo }}</strong></el-col>
            <el-col :span="12"><span style="color:#909399;font-size:13px;">房屋</span><br><strong>{{ contract.roomName }}</strong></el-col>
          </el-row>
          <el-row :gutter="24" style="margin-top:8px;">
            <el-col :span="12"><span style="color:#909399;font-size:13px;">租客</span><br><strong>{{ contract.tenantName }}</strong></el-col>
            <el-col :span="12"><span style="color:#909399;font-size:13px;">付款周期</span><br><strong>{{ contract.paymentCycle === 'Monthly' ? '月付' : contract.paymentCycle === 'Quarterly' ? '季付' : contract.paymentCycle || '月付' }}</strong></el-col>
          </el-row>
        </div>

        <!-- 周期性收费 -->
        <div v-if="recurringPreviewItems.length > 0" style="margin-bottom:20px;">
          <div style="display:flex;align-items:baseline;gap:12px;margin-bottom:6px;">
            <span style="font-weight:600;font-size:15px;">周期性收费</span>
            <span style="font-size:13px;color:#909399;">账期 {{ contract.startDate }} ~ {{ recurringPeriodEnd }}</span>
          </div>
          <div style="height:1px;background:#e4e7ed;margin-bottom:12px;"></div>
          <el-table :data="recurringPreviewItems" stripe size="small">
            <el-table-column prop="period" label="账期" width="80" />
            <el-table-column prop="feeName" label="费用项目" min-width="110" />
            <el-table-column label="金额" width="130" align="right">
              <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
            </el-table-column>
            <el-table-column label="生效期" min-width="160">
              <template #default="{ row }">{{ row.effStart }} ~ {{ row.effEnd }}</template>
            </el-table-column>
          </el-table>
        </div>

        <!-- 一次性收费 -->
        <div v-if="oneTimePreviewItems.length > 0" style="margin-bottom:20px;">
          <div style="font-weight:600;font-size:15px;margin-bottom:6px;">一次性收费</div>
          <div style="height:1px;background:#e4e7ed;margin-bottom:12px;"></div>
          <el-table :data="oneTimePreviewItems" stripe size="small">
            <el-table-column prop="feeName" label="费用项目" min-width="110" />
            <el-table-column label="金额" width="130" align="right">
              <template #default="{ row }">¥{{ (row.amount || 0).toLocaleString() }}</template>
            </el-table-column>
          </el-table>
        </div>

        <!-- Empty -->
        <div v-if="receivablePreviewItems.length === 0" style="text-align:center;padding:40px 0;color:#909399;">
          <el-icon :size="32"><CircleCheckFilled /></el-icon>
          <p style="margin-top:8px;">无可生成的应收项目</p>
        </div>

        <!-- Summary Panel -->
        <div v-if="receivablePreviewItems.length > 0" style="padding:12px 20px;background:#f5f7fa;border-radius:6px;margin-top:4px;">
          <div style="display:flex;justify-content:space-between;align-items:center;font-size:14px;padding:4px 0;">
            <span style="color:#606266;">周期性收费</span>
            <span>¥{{ recurringSubtotal.toLocaleString() }}</span>
          </div>
          <div style="display:flex;justify-content:space-between;align-items:center;font-size:14px;padding:4px 0;">
            <span style="color:#606266;">一次性收费</span>
            <span>¥{{ oneTimeSubtotal.toLocaleString() }}</span>
          </div>
          <el-divider style="margin:8px 0;" />
          <div style="display:flex;justify-content:space-between;align-items:center;font-size:16px;font-weight:700;">
            <span>应收合计</span>
            <span style="color:#e6a23c;">¥{{ receivablePreviewTotal.toLocaleString() }}</span>
          </div>
        </div>
      </template>

      <template #footer>
        <el-button @click="showReceivablePreviewDialog = false">取消</el-button>
        <el-button type="primary" @click="submitReceivableGenerate" :loading="receivableSubmitting">提交审批</el-button>
      </template>
    </el-dialog>
    <!-- Add Tenant Dialog -->
    <el-dialog :draggable="true" v-model="showAddTenant" title="添加租户" width="600px">
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
    <el-dialog :draggable="true" v-model="showRemoveTenant" title="解绑租客" width="450px">
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
/**
 * =========================================================================
 * 合同详情 — 多 Tab 聚合页
 *
 * 职责：
 *   - 展示合同全维度信息（基本信息、费用配置、押金、变更历史）
 *   - 提供所有合同操作入口（调价、续签、终止、暂停、修改）
 *   - 所有修改类操作均走审批流，提交后跳转审批
 *
 * 操作模式：
 *   - 直接执行（0 级审批）：无审批配置时立即执行
 *   - 审批驱动：有审批配置时提交 → 审批通过 → 回调执行
 *
 * 依赖：
 *   - useUserStore：获取 currentUserId、effectiveCompanyId
 *   - 大量 API 函数（约 30 个接口）
 * =========================================================================
 */
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { toGuidId } from '@/utils'
import { formatDate, formatTime } from '@/utils/chinaTime'
import { submitApproval, getApprovalTypes, getRoles, createApprovalType, createApprovalLevel,
  getContract, updateContract, terminateContract, renewContract, feeAdjust, getJournals, generateJournals as apiGenerateReceivables, getDeposits,
  getContractFeeConfigs, createContractFeeConfig, updateContractFeeConfig, adjustContractFeeConfig,
  getContractFeeConfigHistory, getFeeCodes, previewRenewal, submitRenewal,
  getRenewalHistory, getRenewalChain, getAllowedOperations, getContractChanges,
  handleApiError, previewJournals, generateJournalRequest,
  submitContractModify, getReceipts } from '@/api/index.js'

// ---------------------------------------------------------------------------
// 路由 & 路由
// ---------------------------------------------------------------------------
const route = useRoute()
const router = useRouter()

/** 当前激活的 Tab */
const activeTab = ref('recurring')
watch(activeTab, (tab) => {
  if (tab === 'tenants') fetchContractTenants()
  if (tab === 'onetime') fetchFeeConfigs()
})

// =========================================================================
// 主数据 — 从 API 加载
// =========================================================================
/** 合同基本信息 */
const contract = ref({
  id: route.params.id,
  contractNo: '',
  roomName: '',
  tenantName: '',
  tenantPhone: '',
  startDate: '',
  endDate: '',
  status: '',
  outstandingBalance: 0,
  prepaidBalance: 0,
  remark: ''
})
/** 主数据加载中 */
const loading = ref(true)
/** 费用配置加载中 */
const feeConfigLoading = ref(false)
/** 应收加载中 */
const receivableLoading = ref(false)
/** 应收生成中 */
const generatingReceivables = ref(false)

// =========================================================================
// 租客管理 Tab
// =========================================================================
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

// =========================================================================
// 费用 & 应收
// =========================================================================
/** 费用配置列表 */
const feeConfigs = ref([])
/** 押金日志 */
const depositLogs = ref([])
/** 应收时间线 */
const receivableTimeline = ref([])
/** 收款记录 */
const receipts = ref([])

// =========================================================================
// 补充收费弹窗
// =========================================================================
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
  const totalPosted = receivableTimeline.value.filter(r => r.glPosted).reduce((s, r) => s + (r.amount || 0), 0)
  return { totalAmount, totalPosted }
})

/** 按账期分组的应收时间线 */
const groupedTimeline = computed(() => {
  const map = new Map()
  for (const r of receivableTimeline.value) {
    const key = r.period || '未知'
    if (!map.has(key)) {
      map.set(key, { period: key, dueDate: r.dueDate, billMonth: r.billMonth, items: [], totalAmount: 0, receiptItems: [] })
    }
    const g = map.get(key)
    g.items.push(r)
    g.totalAmount += r.amount || 0
    if (r.dueDate && r.dueDate < g.dueDate) g.dueDate = r.dueDate
    if (!g.billMonth && r.billMonth) g.billMonth = r.billMonth
  }
  // 将收款记录匹配到对应账期（按 receiptMonth 匹配 billMonth）
  for (const rc of receipts.value) {
    let matched = [...map.values()].find(g => g.billMonth === rc.receiptMonth || g.period === rc.receiptMonth)
    if (!matched) {
      const key = rc.receiptMonth
      if (!map.has(key)) {
        map.set(key, { period: key, dueDate: '', billMonth: key, items: [], totalAmount: 0, receiptItems: [], receiptTotal: 0 })
      }
      matched = map.get(key)
    }
    matched.receiptItems.push(rc)
    matched.receiptTotal = (matched.receiptTotal || 0) + (rc.amount || 0)
  }
  // 按账期降序排列（最近的在最上面）
  const groups = [...map.values()].sort((a, b) => b.period.localeCompare(a.period))
  return groups.map(g => {
    const receiptTotal = g.receiptTotal || 0
    const unpaid = g.totalAmount - receiptTotal
    const overdue = g.dueDate && new Date(g.dueDate) < new Date()
    let statusText, statusTagType, statusClass
    if (g.items.length > 0 && unpaid <= 0) {
      statusText = '已付清'
      statusTagType = 'success'
      statusClass = 'status-paid'
    } else if (overdue) {
      statusText = '已逾期'
      statusTagType = 'danger'
      statusClass = 'status-overdue'
    } else {
      statusText = '待收款'
      statusTagType = 'warning'
      statusClass = 'status-pending'
    }
    const postedItems = g.items.filter(i => i.glPosted)
    const unpostedItems = g.items.filter(i => !i.glPosted)
    return {
      ...g,
      postedItems,
      unpostedItems,
      receiptTotal,
      unpaid: Math.max(0, unpaid),
      statusText,
      statusTagType,
      statusClass,
      allPosted: g.items.length > 0 && g.items.every(i => i.glPosted),
      somePosted: g.items.some(i => i.glPosted)
    }
  })
})
/** 按收费类型拆分预览项 */
const recurringPreviewItems = computed(() =>
  receivablePreviewItems.value.filter(i => i.chargeType === 'Recurring')
)
const oneTimePreviewItems = computed(() =>
  receivablePreviewItems.value.filter(i => i.chargeType === 'OneTime')
)
/** 周期收费的账期截止日：有合同到期日则用，无则取最后账期所在月月末 */
const recurringPeriodEnd = computed(() => {
  if (contract.value.endDate) return contract.value.endDate
  const periods = [...new Set(recurringPreviewItems.value.map(i => i.period))].sort()
  if (periods.length > 0) {
    const last = periods[periods.length - 1]
    const [y, m] = last.split('-').map(Number)
    const lastDay = new Date(y, m, 0).getDate()
    return `${last}-${String(lastDay).padStart(2, '0')}`
  }
  return '不限'
})
/** 预览小计 */
const recurringSubtotal = computed(() =>
  recurringPreviewItems.value.reduce((s, i) => s + (i.amount || 0), 0)
)
const oneTimeSubtotal = computed(() =>
  oneTimePreviewItems.value.reduce((s, i) => s + (i.amount || 0), 0)
)
const recurringConfigs = computed(() => {
  // 只显示当前生效中的周期收费（无到期日、已启用），每条费用项目最多一条
  // 历史版本在展开行中通过 onFeeConfigExpand 加载
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
  // 一次性收费支持同项目多次添加，不过滤不去重
  return feeConfigs.value.filter(f => f.chargeType === 'OneTime' && f.feeCodeId)
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
        outstandingBalance: c.outstandingBalance || 0,
        prepaidBalance: c.prepaidBalance || 0,
        companyId: c.companyId || '',
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

    // 应收时间线（基于 Journal — 按账期分组展示）
    try {
      const recRes = await getJournals({ contractId: route.params.id, pageSize: 120 })
      const recItems = recRes.items || recRes.data || recRes || []
      receivableTimeline.value = recItems.map(r => {
        return {
          id: r.id,
          period: r.period || '',
          dueDate: r.dueDate || '',
          amount: r.amount || 0,
          feeName: r.feeName || r.entryType || '',
          chargeType: r.chargeType || '',
          entryType: r.entryType || '',
          glPosted: r.glPosted || false,
          billedAt: r.billedAt || '',
          billMonth: r.billMonth || '',
          contractNo: r.contractNo || ''
        }
      })
    } catch { /* 应收接口暂不可用，保留空列表 */ }

    // 收款记录（按 ReceivedDate 排序，FIFO 匹配到账期）
    try {
      const contractId = route.params.id
      const recRes = await getReceipts({ companyId: contract.value.companyId, contractId })
      const recList = Array.isArray(recRes) ? recRes : recRes.items || recRes.data || []
      receipts.value = recList
        .filter(r => r.status === 'Confirmed')
        .sort((a, b) => new Date(a.receivedDate) - new Date(b.receivedDate))
        .map(r => ({
          id: r.id,
          receiptNo: r.receiptNo || '',
          amount: r.amount || 0,
          receivedDate: r.receivedDate || '',
          status: r.status || 'Confirmed',
          referenceNo: r.referenceNo || '',
          contractId: r.contractId || '',
          receiptMonth: r.receivedDate ? r.receivedDate.slice(0, 7) : ''
        }))
    } catch { receipts.value = [] }

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
const addFeeDialogMode = ref('Recurring') // 'Recurring' | 'OneTime'
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

async function updateAvailableFeeCodes() {
  if (feeCodeList.value.length === 0) {
    try {
      const res = await getFeeCodes({ pageSize: 100 })
      feeCodeList.value = res.items || res.data || res || []
    } catch { /* 静默 */ }
  }
  // 周期性收费过滤掉当前活跃的配置；一次性收费不过滤，允许重复添加
  const usedIds = addFeeDialogMode.value === 'OneTime'
    ? new Set()
    : new Set(feeConfigs.value.filter(f => f.isActive).map(f => f.feeCodeId))
  const chargeTypeFilter = addFeeDialogMode.value === 'OneTime' ? 'OneTime' : 'Recurring'
  availableFeeCodes.value = feeCodeList.value.filter(f => !usedIds.has(f.id) && f.chargeType === chargeTypeFilter)
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
  addFeeDialogMode.value = 'Recurring'
  feeConfigForm.feeCodeId = null
  feeConfigForm.amount = 0
  feeConfigForm.effectiveDate = new Date().toISOString().split('T')[0]
  updateAvailableFeeCodes()
  showFeeConfigDialog.value = true
}

function openAddOneTimeFee() {
  addFeeDialogMode.value = 'OneTime'
  feeConfigForm.feeCodeId = null
  feeConfigForm.amount = 0
  feeConfigForm.effectiveDate = new Date().toISOString().split('T')[0]
  updateAvailableFeeCodes()
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
      effectiveDate: feeConfigForm.effectiveDate,
      chargeType: addFeeDialogMode.value
    })
    ElMessage.success(addFeeDialogMode.value === 'OneTime' ? '一次性收费已添加' : '费用配置已添加')
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

 * ================================================================ */

// toGuidId 从 @/utils 导入，见文件顶部

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
  tenantPhone: '',
  paymentDueDay: 5,
  allowDepositAsLastRent: false,
  remark: ''
})

/** 打开修改信息弹窗时，用当前合同数据初始化表单 */
watch(showOtherModify, (val) => {
  if (val && contract.value?.id) {
    otherForm.startDate = contract.value.startDate || ''
    otherForm.endDate = contract.value.endDate || ''
    otherForm.paymentCycle = contract.value.paymentCycle || 'Monthly'
    otherForm.tenantPhone = contract.value.tenantPhone || ''
    otherForm.paymentDueDay = contract.value.paymentDueDay || 5
    otherForm.allowDepositAsLastRent = contract.value.allowDepositAsLastRent || false
    otherForm.remark = contract.value.remark || ''
  }
})

async function submitOtherModify() {
  try {
    const res = await submitContractModify(route.params.id, {
      startDate: otherForm.startDate || null,
      endDate: otherForm.endDate || null,
      paymentCycle: otherForm.paymentCycle,
      tenantPhone: otherForm.tenantPhone,
      paymentDueDay: otherForm.paymentDueDay,
      allowDepositAsLastRent: otherForm.allowDepositAsLastRent,
      remark: otherForm.remark
    })
    if (res.status === 'PendingApproval') {
      ElMessage.success('修改申请已提交审批')
    } else {
      ElMessage.success('合同信息已更新')
    }
    showOtherModify.value = false
    await fetchContract()
  } catch (e) {
    handleApiError(e, '提交失败')
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
    renewForm.endDate = ''
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
  // 校验到期日必须晚于起租日
  const startDate = contract.value.endDate ? new Date(contract.value.endDate).getTime() + 86400000 : 0
  if (startDate && new Date(renewForm.endDate).getTime() <= startDate) {
    ElMessage.warning('到期日期必须晚于起租日期（' + (contract.value.endDate ? new Date(contract.value.endDate).toLocaleDateString('zh-CN') + ' 次日' : '') + '）')
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
  showReceivablePreviewDialog.value = true
  receivablePreviewLoading.value = true
  try {
    const res = await previewJournals({ contractId: contract.value.id })
    receivablePreviewItems.value = res.items || []
    receivablePreviewTotal.value = res.totalAmount || 0
  } catch (e) {
    receivablePreviewItems.value = []
    receivablePreviewTotal.value = 0
    ElMessage.error('加载应收预览失败: ' + (e?.response?.data?.message || e.message || '未知错误'))
  }
  receivablePreviewLoading.value = false
}

async function submitReceivableGenerate() {
  receivableSubmitting.value = true
  try {
    const res = await generateJournalRequest({ contractId: contract.value.id })
    if (res.status === 'PendingApproval') {
      ElMessage.success('应收生成请求已提交审批，等待审核')
    } else if (res.count !== undefined) {
      ElMessage.success(`已成功生成 ${res.count} 条应收记录`)
    } else {
      ElMessage.success(res.message || '操作成功')
    }
    showReceivablePreviewDialog.value = false
    await fetchContract()
  } catch (e) {
    ElMessage.closeAll()
    ElMessage.error(e?.response?.data?.message || e?.response?.data?.error || '提交失败')
  }
  receivableSubmitting.value = false
}

// Change Requests removed

/** 格式化日期（仅日期）为 yyyy-MM-dd，兼容 Date 对象/时间戳/各种字符串 */

</script>
<style scoped>
.el-timeline h4 {
  margin: 0 0 4px;
}

/* ================================================================
   应收时间线样式
   ================================================================ */
.timeline-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 8px;
}
.timeline-actions {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}
.timeline-stats {
  font-size: 13px;
  color: #909399;
}
.stat-amount { color: #e6a23c; }
.stat-posted { color: #67c23a; }
.stat-sep { margin: 0 6px; color: #dcdfe6; }

.timeline-container {
  padding: 8px 0;
}

/* 账期分组：时间线连接器 + 卡片 */
.period-group {
  display: flex;
  gap: 0;
  position: relative;
}

/* 左侧时间线 */
.period-connector {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 28px;
  flex-shrink: 0;
  padding-top: 18px;
}
.period-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid #c0c4cc;
  background: #fff;
  z-index: 1;
  flex-shrink: 0;
}
.period-dot.status-paid {
  border-color: #67c23a;
  background: #67c23a;
}
.period-dot.status-overdue {
  border-color: #f56c6c;
  background: #f56c6c;
}
.period-dot.status-pending {
  border-color: #e6a23c;
  background: #e6a23c;
}
.period-line {
  width: 2px;
  flex: 1;
  background: #e4e7ed;
  min-height: 24px;
}

/* 账期卡片 */
.period-card {
  flex: 1;
  margin: 8px 0 8px 8px;
  border: 1px solid #ebeef5;
  border-radius: 6px;
  overflow: hidden;
  transition: box-shadow 0.2s;
}
.period-card:hover {
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
}
.period-card.card-status-paid { border-left: 3px solid #67c23a; }
.period-card.card-status-overdue { border-left: 3px solid #f56c6c; }
.period-card.card-status-pending { border-left: 3px solid #e6a23c; }

.period-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: #fafafa;
  border-bottom: 1px solid #ebeef5;
  flex-wrap: wrap;
  gap: 8px;
}
.period-card-title {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.period-label {
  font-weight: 600;
  font-size: 15px;
  color: #303133;
}
.period-status-badge { font-weight: 600; }
.period-due-label {
  font-size: 12px;
  color: #909399;
}
.period-billmonth-label {
  font-size: 12px;
  color: #409eff;
  background: #ecf5ff;
  padding: 0 6px;
  border-radius: 3px;
  line-height: 20px;
}
.period-card-amounts {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 13px;
}
.period-total {
  font-weight: 700;
  font-size: 16px;
  color: #303133;
}
.period-received {
  font-size: 13px;
  color: #67c23a;
  font-weight: 600;
}
.period-unpaid {
  font-size: 13px;
  color: #f56c6c;
  font-weight: 600;
}
.period-settled {
  font-size: 13px;
  color: #67c23a;
  font-weight: 600;
}

.period-gl-section {
  margin: 0 16px;
}
.period-gl-section + .period-gl-section {
  border-top: 1px dashed #e4e7ed;
  padding-top: 6px;
  margin-top: 6px;
}
.period-gl-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 600;
  padding: 6px 0 4px;
}
.period-gl-header.posted-header { color: #67c23a; }
.period-gl-header.unposted-header { color: #e6a23c; }
.period-gl-amt {
  margin-left: auto;
  font-weight: 700;
  font-size: 14px;
}

.period-detail-table {
  margin: 0;
}
.period-detail-table :deep(.el-table__body-wrapper) {
  overflow-x: auto;
}

.period-receipt-section {
  border-top: 1px dashed #e4e7ed;
  margin: 8px 16px 4px;
  padding-top: 8px;
}
.period-receipt-header {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #606266;
  margin-bottom: 6px;
}
.period-receipt-icon {
  font-size: 14px;
}
.period-receipt-total {
  margin-left: auto;
  color: #67c23a;
  font-weight: 600;
  font-size: 13px;
}

</style>
