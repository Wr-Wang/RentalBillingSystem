<!--
  =========================================================================
  ApprovalDetailDialog — 审批详情抽屉

  布局：
    ┌─ Drawer (720px, 右侧滑出) ──────────────────────────────┐
    │  头部: 标题 + 状态标签 + ID                              │
    ├────────────────────────────────────────────────────────┤
    │  Tab 1: 审批流程                                        │
    │    - 审批概要（标题/申请人/时间/进度）                     │
    │    - 流转步骤（审批级别进度条）                           │
    │    - 最新审批意见                                        │
    ├────────────────────────────────────────────────────────┤
    │  Tab 2: 审批记录（时间线）                                │
    ├────────────────────────────────────────────────────────┤
    │  Tab 3: 业务数据                                        │
    │    - 变更摘要（字段对比列表）                             │
    │    - [调价专用] 费用项目卡片                              │
    │    - [新建合同专用] 费用明细表格                          │
    ├────────────────────────────────────────────────────────┤
    │  Tab 4: 导入数据（Import 专用）                           │
    ├────────────────────────────────────────────────────────┤
    │  底部: 审批意见输入 + 通过/驳回/撤回按钮                  │
    └────────────────────────────────────────────────────────┘

  状态流转：
    父组件控制 modelValue → visible
    loadDetail() → 加载审批主数据 + 业务数据 + 导入批次
    用户操作 → approve / reject / cancel → emit 事件通知父组件
  =========================================================================
-->
<template>
  <el-drawer
    v-model="visible"
    direction="rtl"
    size="720px"
    :close-on-click-modal="true"
    @close="close"
  >
    <!-- ====== Header ====== -->
    <template #header>
      <div class="drawer-header">
        <div class="drawer-title">
          <span>审批详情</span>
          <el-tag v-if="data" :type="statusTagType" size="small" effect="dark" class="status-tag">
            {{ statusText }}
          </el-tag>
        </div>
        <div v-if="data" class="drawer-id">#{{ data.id?.slice(0, 8) }}</div>
      </div>
    </template>

    <!-- ====== Body ====== -->
    <div v-loading="loading" class="drawer-body" element-loading-text="加载中...">
      <template v-if="data">
        <el-tabs v-model="activeTab" class="detail-tabs" @tab-change="onTabChange">
          <!-- ═══════ Tab 1: 审批流程 ═══════ -->
          <el-tab-pane name="flow">
            <template #label>
              <span class="tab-label"><el-icon><CircleCheck /></el-icon> 审批流程</span>
            </template>

            <!-- 审批概要 -->
            <el-card shadow="never" class="section-card">
              <div class="summary-grid">
                <div class="summary-main">
                  <div class="summary-title">{{ data.title || '无标题' }}</div>
                  <div class="summary-meta">
                    <el-tag size="small" type="primary" effect="plain">{{ data.approvalTypeName || '通用审批' }}</el-tag>
                  </div>
                </div>
                <div class="summary-info">
                  <div class="info-row">
                    <span class="info-label">申请人</span>
                    <span class="info-value">
                      <el-avatar :size="20" style="background: #409eff; vertical-align: middle; margin-right: 4px;">
                        {{ (data.submitterName || '?')[0] }}
                      </el-avatar>
                      {{ data.submitterName || '-' }}
                    </span>
                  </div>
                  <div class="info-row">
                    <span class="info-label">提交时间</span>
                    <span class="info-value">{{ formatTime(data.createdAt) }}</span>
                  </div>
                  <div class="info-row" v-if="data.completedAt">
                    <span class="info-label">完成时间</span>
                    <span class="info-value">{{ formatTime(data.completedAt) }}</span>
                  </div>
                  <div class="info-row">
                    <span class="info-label">当前进度</span>
                    <span class="info-value">
                      <el-tag size="small" :type="data.status === 'Approved' ? 'success' : 'info'" effect="plain">
                        第 {{ data.currentLevel }}/{{ data.maxLevel }} 级
                      </el-tag>
                    </span>
                  </div>
                </div>
              </div>
              <div v-if="data.description" class="summary-desc">
                <el-icon><InfoFilled /></el-icon> {{ data.description }}
              </div>
            </el-card>

            <!-- 审批流程步骤 -->
            <el-card shadow="never" class="section-card">
              <template #header>
                <span style="font-weight: 600;">流转步骤</span>
                <el-tag size="small" style="float: right;" :type="data.status === 'Approved' ? 'success' : data.status === 'Rejected' ? 'danger' : 'warning'" effect="plain">
                  {{ data.status === 'Pending' ? '审批中' : data.status === 'Approved' ? '已完成' : data.status === 'Rejected' ? '已驳回' : data.status === 'Cancelled' ? '已撤回' : data.status }}
                </el-tag>
              </template>

              <div v-if="levelDetails.length > 0" class="approval-steps">
                <div
                  v-for="(level, idx) in levelDetails"
                  :key="idx"
                  class="step-row"
                  :class="'step-status-' + level.status"
                >
                  <div class="step-indicator">
                    <div class="step-dot" :class="'dot-' + level.status">
                      <el-icon v-if="level.status === 'completed' || level.status === 'submitted'"><Check /></el-icon>
                      <el-icon v-else-if="level.status === 'current'"><MoreFilled /></el-icon>
                      <span v-else class="dot-ring"></span>
                    </div>
                    <div v-if="idx < levelDetails.length - 1" class="step-line" :class="'line-' + level.status" />
                  </div>
                  <div class="step-body">
                    <div class="step-header">
                      <span class="step-role">{{ level.roleName }}</span>
                      <el-tag
                        v-if="level.status === 'submitted'"
                        size="small"
                        type="primary"
                        effect="plain"
                      >已提交</el-tag>
                      <el-tag
                        v-else-if="level.status === 'completed'"
                        size="small"
                        type="success"
                        effect="plain"
                      >已通过</el-tag>
                      <el-tag
                        v-else-if="level.status === 'rejected'"
                        size="small"
                        type="danger"
                        effect="plain"
                      >已驳回</el-tag>
                      <el-tag
                        v-else-if="level.status === 'current'"
                        size="small"
                        type="warning"
                        effect="dark"
                      >待审批</el-tag>
                      <el-tag
                        v-else-if="level.status === 'skipped'"
                        size="small"
                        type="info"
                        effect="plain"
                      >已跳过</el-tag>
                      <el-tag
                        v-else
                        size="small"
                        type="info"
                        effect="plain"
                      >等待中</el-tag>
                      <span v-if="level.level > 0" class="step-level">第{{ level.level }}级</span>
                    </div>
                    <div v-if="level.status === 'submitted'" class="step-detail">
                      <el-avatar :size="22" class="step-avatar-submitted">{{ (level.approverName || '?')[0] }}</el-avatar>
                      <span class="step-name">{{ level.approverName }}</span>
                      <span class="step-account">{{ level.approverAccount }}</span>
                      <span class="step-time">{{ formatTime(level.timestamp) }}</span>
                    </div>
                    <div v-else-if="level.status === 'completed'" class="step-detail">
                      <el-avatar :size="22" class="step-avatar-success">{{ (level.approverName || '?')[0] }}</el-avatar>
                      <span class="step-name">{{ level.approverName }}</span>
                      <span class="step-account">{{ level.approverAccount }}</span>
                      <span class="step-time">{{ formatTime(level.timestamp) }}</span>
                      <div v-if="level.comment" class="step-comment">{{ level.comment }}</div>
                    </div>
                    <div v-else-if="level.status === 'rejected'" class="step-detail">
                      <el-avatar :size="22" class="step-avatar-danger">{{ (level.approverName || '?')[0] }}</el-avatar>
                      <span class="step-name" style="color:#f56c6c;">{{ level.approverName }}</span>
                      <span class="step-account" style="color:#f56c6c;">{{ level.approverAccount }}</span>
                      <span class="step-time">{{ formatTime(level.timestamp) }}</span>
                      <div v-if="level.comment" class="step-comment">{{ level.comment }}</div>
                    </div>
                    <div v-else-if="level.status === 'current'" class="step-detail">
                      <span class="step-waiting-text">等待</span>
                      <span class="step-name" style="color: #e6a23c;">{{ level.approverName || level.roleName }}</span>
                      <span v-if="level.approverAccount" class="step-account" style="color: #e6a23c;">{{ level.approverAccount }}</span>
                      <span class="step-waiting-text">审批</span>
                    </div>
                    <div v-else-if="level.status === 'pending' && level.approverName" class="step-detail">
                      <el-avatar :size="22" class="step-avatar-pending">{{ (level.approverName || '?')[0] }}</el-avatar>
                      <span class="step-name" style="color: #909399;">{{ level.approverName }}</span>
                      <span v-if="level.approverAccount" class="step-account" style="color: #909399;">{{ level.approverAccount }}</span>
                    </div>
                    <div v-else class="step-detail step-empty-detail">&nbsp;</div>
                  </div>
                </div>
              </div>
              <el-empty v-else description="暂无审批流程配置" :image-size="60" />
            </el-card>

            <!-- 最新审批意见 -->
            <el-card v-if="latestComment" shadow="never" class="section-card">
              <template #header>
                <span style="font-weight: 600;">审批意见</span>
              </template>
              <div class="comment-box">
                <div class="comment-header">
                  <el-avatar :size="20" style="background: #909399; vertical-align: middle;">
                    {{ (latestComment.approverName || '?')[0] }}
                  </el-avatar>
                  <span class="comment-author">{{ latestComment.approverName }}</span>
                  <span class="comment-account">{{ latestComment.approverAccount }}</span>
                  <el-tag size="small" :type="latestComment.action === 'Approved' ? 'success' : 'danger'" effect="plain">
                    {{ latestComment.action === 'Approved' ? '通过' : '驳回' }}
                  </el-tag>
                  <span class="comment-time">{{ formatTime(latestComment.createdAt) }}</span>
                </div>
                <div class="comment-text">{{ latestComment.comment }}</div>
              </div>
            </el-card>
          </el-tab-pane>

          <!-- ═══════ Tab 2: 审批记录 ═══════ -->
          <el-tab-pane name="records">
            <template #label>
              <span class="tab-label"><el-icon><List /></el-icon> 审批记录</span>
            </template>

            <el-card shadow="never" class="section-card">
              <template #header>
                <span style="font-weight: 600;">操作时间线</span>
                <span style="float: right; font-size: 12px; color: #909399;">共 {{ data.records?.length || 0 }} 条记录</span>
              </template>

              <el-timeline v-if="data.records && data.records.length > 0">
                <el-timeline-item
                  v-for="(record, idx) in sortedRecords"
                  :key="idx"
                  :timestamp="formatTime(record.createdAt)"
                  placement="top"
                  :type="record.action === 'Approved' ? 'success' : record.action === 'Rejected' ? 'danger' : record.action === 'Submitted' ? 'primary' : 'warning'"
                  :hollow="record.action !== 'Approved' && record.action !== 'Rejected'"
                >
                  <div class="timeline-content">
                    <div class="timeline-header">
                      <el-avatar :size="26" :style="avatarStyle(record.action)">
                        {{ (record.approverName || '?')[0] }}
                      </el-avatar>
                      <div class="timeline-info">
                        <strong>{{ record.approverName || '未知' }}</strong>
                        <span class="record-account">{{ record.approverAccount }}</span>
                        <el-tag
                          size="small"
                          :type="record.action === 'Approved' ? 'success' : record.action === 'Rejected' ? 'danger' : record.action === 'Submitted' ? 'primary' : 'warning'"
                          style="margin-left: 6px;"
                        >
                          {{ record.action === 'Approved' ? '通过' : record.action === 'Rejected' ? '驳回' : record.action === 'Submitted' ? '提交' : record.action === 'Cancelled' ? '撤回' : record.action }}
                        </el-tag>
                        <span v-if="record.level && record.action !== 'Submitted'" class="record-level">第{{ record.level }}级</span>
                      </div>
                    </div>
                    <p v-if="record.comment" class="timeline-comment">
                      <el-icon><ChatDotSquare /></el-icon> {{ record.comment }}
                    </p>
                  </div>
                </el-timeline-item>
              </el-timeline>

              <el-empty v-else description="暂无审批记录" :image-size="60" />
            </el-card>
          </el-tab-pane>

          <!-- ═══════ Tab 3: 业务数据（v3 增强版 — 三种布局） ═══════ -->
          <el-tab-pane v-if="showBizTab" name="biz" lazy>
            <template #label>
              <span class="tab-label"><el-icon><DataBoard /></el-icon> 业务数据</span>
            </template>
            <div v-loading="bizLoading">
              <template v-if="bizDetail">
                <!-- ===== 1. 通用字段对比（所有 BizType） ===== -->
                <el-card v-if="bizDetail.fields?.length || bizDetail.effectiveDate" shadow="never" class="section-card">
                  <template #header>
                    <span style="font-weight: 600;">变更摘要</span>
                    <el-tag v-if="bizDetail.effectiveDate" size="small" type="warning" effect="plain" style="float: right;">
                      生效日: {{ formatBizDate(bizDetail.effectiveDate) }}
                    </el-tag>
                  </template>

                  <!-- [新建合同] 双栏描述列表 -->
                  <el-descriptions v-if="bizDetail.bizType === 'ContractActivation'" :column="2" border size="small">
                    <el-descriptions-item
                      v-for="field in bizDetail.fields"
                      :key="field.label"
                      :label="field.label"
                      :span="field.label.startsWith('费用合计') ? 2 : 1"
                    >
                      <span :style="{ fontWeight: field.label.startsWith('费用合计') ? 'bold' : 500, color: field.label.startsWith('费用合计') ? '#e6a23c' : '#303133' }">{{ field.newValue || '-' }}</span>
                    </el-descriptions-item>
                  </el-descriptions>

                  <!-- [其他审批类型] 原始变更行布局 -->
                  <div v-else-if="bizDetail.fields?.length" class="change-summary">
                    <div
                      v-for="field in bizDetail.fields"
                      :key="field.label"
                      class="summary-row"
                      :class="{ 'row-changed': field.isChanged }"
                    >
                      <span class="summary-label">{{ field.label }}</span>
                      <span v-if="field.oldValue && field.newValue" class="summary-values">
                        <span class="summary-oldval">{{ field.oldValue }}</span>
                        <el-icon class="summary-arrow"><ArrowRight /></el-icon>
                        <span class="summary-newval">{{ field.newValue }}</span>
                      </span>
                      <span v-else-if="field.oldValue && !field.newValue" class="summary-values">
                        <span class="summary-oldval">{{ field.oldValue }}</span>
                      </span>
                      <span v-else-if="field.newValue" class="summary-values">
                        <span class="summary-newval summary-newval-only">{{ field.newValue }}</span>
                      </span>
                      <span v-else class="summary-values summary-empty">—</span>
                      <el-tag v-if="field.isChanged" size="small" type="warning" effect="plain" class="summary-tag">变更</el-tag>
                    </div>
                  </div>
                </el-card>

                <!-- ===== 2. [调价专用] 费用项目卡片列表 ===== -->
                <el-card v-if="bizDetail.bizType === 'FEE_ADJUST' && bizDetail.feeItems?.length"
                  shadow="never" class="section-card" style="margin-top: 14px;">
                  <template #header>
                    <span style="font-weight: 600;">费用项目明细</span>
                    <el-tag size="small" type="warning" effect="plain" style="float: right;">
                      共 {{ bizDetail.feeItems.length }} 项
                    </el-tag>
                  </template>
                  <div class="fee-cards-container">
                    <FeeItemCard
                      v-for="(item, idx) in bizDetail.feeItems"
                      :key="idx"
                      :item="item"
                    />
                  </div>
                </el-card>

                <!-- ===== 3. [新建合同专用] 费用明细表格 ===== -->
                <el-card v-if="bizDetail.bizType === 'ContractActivation' && bizDetail.feeItems?.length"
                  shadow="never" class="section-card" style="margin-top: 14px;">
                  <template #header>
                    <span style="font-weight: 600;">费用明细</span>
                    <el-tag size="small" type="warning" effect="plain" style="float: right;">
                      共 {{ bizDetail.feeItems.length }} 项
                    </el-tag>
                  </template>
                  <el-table :data="bizDetail.feeItems" stripe size="small">
                    <el-table-column prop="feeName" label="收费项目" min-width="120" />
                    <el-table-column label="金额" width="140" align="right">
                      <template #default="{ row }">
                        <span style="font-weight: bold;">¥{{ Number(row.newAmount).toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</span>
                      </template>
                    </el-table-column>
                    <el-table-column label="类型" width="80" align="center">
                      <template #default="{ row }">{{ row.chargeType === 'OneTime' ? '一次性' : '周期性' }}</template>
                    </el-table-column>
                    <el-table-column label="计费方式" width="100" align="center">
                      <template #default="{ row }">{{ row.billingMode === 'MeterBased' ? '按表计量' : '固定金额' }}</template>
                    </el-table-column>
                    <el-table-column prop="effectiveDate" label="生效日期" width="110" align="center">
                      <template #default="{ row }">{{ row.effectiveDate || '-' }}</template>
                    </el-table-column>
                  </el-table>
                </el-card>

                <!-- ===== 4. [终止专用] 终止详情已合并至变更摘要，无需重复展示 ===== -->

                <!-- ===== 4. 无业务字段时的兜底提示 ===== -->
                <el-empty v-if="!bizDetail.fields?.length && !bizDetail.feeItems?.length"
                  description="暂无结构化业务数据" :image-size="60" />
              </template>
              <el-empty v-else-if="!bizLoading" description="暂无结构化业务数据" :image-size="60" />
            </div>
          </el-tab-pane>

          <!-- ═══════ Tab 4: 导入数据（lazy 确保 DOM 在 Tab 激活后才渲染） ═══════ -->
          <el-tab-pane v-if="showImportTab" name="import" lazy>
            <template #label>
              <span class="tab-label"><el-icon><Upload /></el-icon> 导入数据</span>
            </template>

            <div v-loading="loadingBatch">
              <template v-if="importBatch">
                <!-- 批次概要 -->
                <el-card shadow="never" class="section-card">
                  <template #header>
                    <span style="font-weight: 600;">批次概要</span>
                    <el-tag
                      size="small"
                      :type="batchStatusType"
                      effect="plain"
                      style="float: right;"
                    >{{ batchStatusText }}</el-tag>
                  </template>
                  <el-descriptions :column="2" border size="small">
                    <el-descriptions-item label="文件名称" :span="2">{{ importBatch.fileName || '-' }}</el-descriptions-item>
                    <el-descriptions-item label="导入类型">{{ importBatch.importType || '-' }}</el-descriptions-item>
                    <el-descriptions-item label="总行数">{{ importBatch.totalRows ?? 0 }}</el-descriptions-item>
                    <el-descriptions-item label="有效行">
                      <span style="color: #67c23a;">{{ importBatch.validRows ?? 0 }}</span>
                    </el-descriptions-item>
                    <el-descriptions-item label="失败行">
                      <span style="color: #f56c6c;">{{ importBatch.failedRows ?? 0 }}</span>
                    </el-descriptions-item>
                    <el-descriptions-item label="创建时间">{{ formatTime(importBatch.createdAt) }}</el-descriptions-item>
                    <el-descriptions-item label="创建人">{{ importBatch.createdByName ? importBatch.createdByName + ' (' + importBatch.createdByAccount + ')' : importBatch.createdBy?.slice(0, 8) || '-' }}</el-descriptions-item>
                  </el-descriptions>
                </el-card>

                <!-- 行明细 -->
                <el-card shadow="never" class="section-card">
                  <template #header>
                    <span style="font-weight: 600;">导入行明细（共 {{ importBatch.items?.length || 0 }} 行）</span>
                    <div style="float: right;">
                      <el-radio-group v-model="filterStatus" size="small">
                        <el-radio-button label="all">全部</el-radio-button>
                        <el-radio-button label="valid">有效</el-radio-button>
                        <el-radio-button label="fail">失败</el-radio-button>
                      </el-radio-group>
                    </div>
                  </template>
                  <el-table ref="itemsTableRef" :data="filteredItems" :key="'items-' + tableKey" stripe max-height="400" size="small">
                    <el-table-column prop="rowIndex" label="行号" width="55" />
                    <el-table-column label="结果" width="60">
                      <template #default="{ row }">
                        <el-tag :type="row.isValid ? 'success' : 'danger'" size="small" effect="plain">
                          {{ row.isValid ? '通过' : '失败' }}
                        </el-tag>
                      </template>
                    </el-table-column>
                    <el-table-column prop="buildingName" label="座楼" width="90" show-overflow-tooltip />
                    <el-table-column prop="floorName" label="楼层" width="70" />
                    <el-table-column prop="unitNo" label="房号" width="70" />
                    <el-table-column prop="fullCode" label="完整编码" width="160" show-overflow-tooltip />
                    <el-table-column prop="roomTypeName" label="房型" width="90" show-overflow-tooltip />
                    <el-table-column prop="area" label="面积" width="65">
                      <template #default="{ row }">{{ row.area ?? '-' }}</template>
                    </el-table-column>
                    <el-table-column prop="orientation" label="朝向" width="65" />
                    <el-table-column prop="baseRentAmount" label="标准租金" width="95">
                      <template #default="{ row }">{{ row.baseRentAmount ? '¥' + row.baseRentAmount.toLocaleString() : '-' }}</template>
                    </el-table-column>
                    <el-table-column label="错误信息" min-width="160">
                      <template #default="{ row }">
                        <span v-if="!row.isValid" class="cell-error">{{ row.errorMessage }}</span>
                        <span v-else-if="row.priceWarning" class="cell-warning">⚠ {{ row.priceWarning }}</span>
                        <span v-else class="cell-empty">—</span>
                      </template>
                    </el-table-column>
                  </el-table>
                </el-card>
              </template>
              <el-empty v-else-if="!loadingBatch" description="暂无关联的导入数据" :image-size="60" />
            </div>
          </el-tab-pane>
        </el-tabs>
      </template>

      <el-empty v-else-if="!loading" description="未找到审批数据" :image-size="80" />
    </div>

    <!-- ====== Footer ====== -->
    <template #footer>
      <div class="drawer-footer">
        <el-input
          v-if="showActions && data?.status === 'Pending'"
          v-model="comment"
          type="textarea"
          :rows="2"
          placeholder="审批意见（可选，驳回时建议填写原因）"
          class="footer-comment"
        />
        <div class="footer-buttons">
          <div class="footer-left">
            <el-button v-if="canCancel" type="warning" @click="handleCancel" :loading="cancelling" :disabled="cancelling || approving || rejecting" :icon="RefreshLeft">
              撤回
            </el-button>
          </div>
          <div class="footer-right">
            <el-button @click="close">关闭</el-button>
            <el-button v-if="showActions && data?.status === 'Pending'" type="danger" @click="handleReject" :loading="rejecting" :disabled="rejecting || approving || cancelling" :icon="Close">
              驳回
            </el-button>
            <el-button v-if="showActions && data?.status === 'Pending'" type="success" @click="handleApprove" :loading="approving" :disabled="approving || rejecting || cancelling" :icon="Check">
              通过
            </el-button>
          </div>
        </div>
      </div>
    </template>
  </el-drawer>
</template>

<script setup>
/**
 * =========================================================================
 * 审批详情抽屉组件
 *
 * 三种状态模式：
 *   modelValue = true → 打开抽屉
 *   approvalId → 加载指定审批
 *   showActions = true → 底部显示通过/驳回/撤回按钮
 *
 * 数据加载顺序：
 *   loadDetail()
 *     → getApprovalDetail(id)          审批主数据
 *     → getApprovalBizDetail(id)       业务数据（变更摘要/费用明细）
 *     → (if Import) getImportBatch(id)  导入批次详情
 *
 * 事件通知：
 *   'approved'  → 父组件刷新列表
 *   'rejected'  → 父组件刷新列表
 *   'cancelled' → 父组件刷新列表
 * =========================================================================
 */
import { ref, computed, watch, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Check, Close, CircleCheck, List, Upload, InfoFilled, MoreFilled, ChatDotSquare, RefreshLeft, DataBoard, ArrowRight } from '@element-plus/icons-vue'
import FeeItemCard from './FeeItemCard.vue'
import { getApprovalDetail, getImportBatch, approveApproval, rejectApproval, cancelApproval, getApprovalBizDetail } from '../../api/index'

// ---------------------------------------------------------------------------
// Props / Emits
// ---------------------------------------------------------------------------
const props = defineProps({
  modelValue: Boolean,       // 控制 drawer 开/关
  approvalId: String,        // 审批请求 ID
  showActions: { type: Boolean, default: false }  // 是否显示操作按钮
})
const emit = defineEmits(['update:modelValue', 'approved', 'rejected', 'cancelled'])

// ---------------------------------------------------------------------------
// 状态
// ---------------------------------------------------------------------------
const visible = ref(false)       // drawer 可见性
const data = ref(null)           // 审批主数据
const loading = ref(false)       // 主数据加载中
const activeTab = ref('flow')    // 当前 Tab
const comment = ref('')          // 审批意见输入
const approving = ref(false)     // 通过中
const rejecting = ref(false)     // 驳回中
const cancelling = ref(false)    // 撤回中
const importBatch = ref(null)    // 导入批次数据
const loadingBatch = ref(false)  // 导入数据加载中
const bizDetail = ref(null)      // 业务数据
const bizLoading = ref(false)    // 业务数据加载中
const filterStatus = ref('all')  // 导入行筛选
const itemsTableRef = ref(null)  // 导入表格 ref
const tableKey = ref(0)          // 强制表格重挂载

// —— Computed ——

/** 当前用户 ID（从 localStorage 读取） */
const currentUserId = ref(localStorage.getItem('userId') || '')

/** 状态标签类型 */
const statusTagType = computed(() => {
  if (!data.value) return 'info'
  const map = { Pending: 'warning', Approved: 'success', Rejected: 'danger', Cancelled: 'info' }
  return map[data.value.status] || 'info'
})

/** 状态中文 */
const statusText = computed(() => {
  if (!data.value) return ''
  const map = { Pending: '审批中', Approved: '已通过', Rejected: '已驳回', Cancelled: '已撤回' }
  return map[data.value.status] || data.value.status
})

/** 审批步骤增强数据（合并 record 中的审批人、时间、备注） */
const levelDetails = computed(() => {
  if (!data.value?.levelChain) return []
  return data.value.levelChain.map(level => {
    const record = data.value.records?.find(r => r.level === level.level)
    return {
      ...level,
      timestamp: record?.createdAt || null,
      comment: record?.comment || null,
      action: record?.action || null,
    }
  })
})

/** 是否可以撤回（当前用户是提交人且状态为 Pending） */
const canCancel = computed(() => {
  if (!data.value || data.value.status !== 'Pending') return false
  const submitRecord = data.value.records?.find(r => r.action === 'Submitted')
  return submitRecord && submitRecord.approverId === currentUserId.value
})

/** 是否显示导入数据 Tab */
const showImportTab = computed(() => data.value?.targetEntityType === 'Import')
/** 是否显示业务数据 Tab（Import 已有独立 Tab，跳过） */
const showBizTab = computed(() => data.value && data.value?.targetEntityType !== 'Import')

/** 排序后的审批记录（时间正序） */
const sortedRecords = computed(() => {
  if (!data.value?.records) return []
  return [...data.value.records].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
})

/** 最新一条有备注的审批记录 */
const latestComment = computed(() => {
  if (!data.value?.records) return null
  const withComment = [...data.value.records]
    .filter(r => r.comment && (r.action === 'Approved' || r.action === 'Rejected'))
    .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
  return withComment[0] || null
})

/** 批次状态标签类型 */
const batchStatusType = computed(() => {
  if (!importBatch.value) return 'info'
  const map = { PendingApproval: 'warning', Approved: 'success', Rejected: 'danger', Cancelled: 'info' }
  return map[importBatch.value.status] || 'info'
})

/** 批次状态中文 */
const batchStatusText = computed(() => {
  if (!importBatch.value) return ''
  const map = { PendingApproval: '待审批', Approved: '已通过', Rejected: '已驳回', Cancelled: '已撤回' }
  return map[importBatch.value.status] || importBatch.value.status
})

/** 过滤后的导入行 */
const filteredItems = computed(() => {
  if (!importBatch.value?.items) return []
  const items = importBatch.value.items
  if (filterStatus.value === 'valid') return items.filter(i => i.isValid)
  if (filterStatus.value === 'fail') return items.filter(i => !i.isValid)
  return items
})

// —— Watch ——
watch(() => props.modelValue, async (val) => {
  visible.value = val
  if (val && props.approvalId) {
    activeTab.value = 'flow'
    filterStatus.value = 'all'
    await loadDetail()
  }
})

watch(visible, (val) => {
  emit('update:modelValue', val)
})

// —— Tab Events ——
function onTabChange(tabName) {
  if (tabName === 'import' && importBatch.value?.items?.length) {
    nextTick(() => {
      itemsTableRef.value?.doLayout?.()
    })
  }
}

// —— Data Loading ——
async function loadDetail() {
  loading.value = true
  try {
    const res = await getApprovalDetail(props.approvalId)
    data.value = res || null
    if (res?.targetEntityType === 'Import' && res?.targetEntityId) {
      await loadImportBatch(res.targetEntityId)
    }
    if (res && res.targetEntityType !== 'Import') {
      await loadBizDetail()
    }
  } catch {
    data.value = null
  }
  loading.value = false
}

async function loadBizDetail() {
  bizLoading.value = true
  try {
    const res = await getApprovalBizDetail(props.approvalId)
    bizDetail.value = res || null
  } catch {
    bizDetail.value = null
  }
  bizLoading.value = false
}

async function loadImportBatch(batchId) {
  loadingBatch.value = true
  try {
    const res = await getImportBatch(batchId)
    console.log('[ImportBatch] raw response:', res)
    console.log('[ImportBatch] items:', res?.items)
    console.log('[ImportBatch] items length:', res?.items?.length)
    if (res?.items?.length) {
      console.log('[ImportBatch] first item:', JSON.stringify(res.items[0]))
    }
    importBatch.value = res || null
    tableKey.value++ // 强制表格重新挂载
    // 如果在导入数据 Tab 页，额外触发布局刷新
    if (activeTab.value === 'import' && res?.items?.length) {
      await nextTick()
      itemsTableRef.value?.doLayout?.()
    }
  } catch {
    importBatch.value = null
  }
  loadingBatch.value = false
}

function formatTime(t) {
  if (!t) return ''
  const d = new Date(t)
  if (isNaN(d.getTime())) return String(t).slice(0, 16)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function formatBizDate(d) {
  if (!d) return ''
  // 已是 yyyy-MM-dd 格式的字符串，直接返回
  if (typeof d === 'string' && /^\d{4}-\d{2}-\d{2}/.test(d)) {
    return d.slice(0, 10)
  }
  // 尝试标准 Date 解析
  const dt = new Date(d)
  if (!isNaN(dt.getTime())) {
    const pad = n => String(n).padStart(2, '0')
    return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}`
  }
  // 兜底：从形如 "Mon Jul 06 2026 ..." 的字符串中提取 yyyy-MM-dd
  if (typeof d === 'string') {
    // 尝试匹配 "Mon DD YYYY" 或 "DD Mon YYYY" 模式
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
    // 最后的保底：从字符串中提取任意四位数年份 + 月份 + 日期
    const fallback = d.match(/(\d{4})[^\d](\d{1,2})[^\d](\d{1,2})/)
    if (fallback) {
      const [, y, m2, d2] = fallback
      return `${y}-${String(m2).padStart(2, '0')}-${String(d2).padStart(2, '0')}`
    }
    return d.slice(0, 10) // 完全无法解析时截断
  }
  return ''
}

/** 费用明细行样式：有变价的行加高亮标记 */
function avatarStyle(action) {
  const colors = {
    Approved: '#67c23a',
    Rejected: '#f56c6c',
    Submitted: '#409eff',
    Cancelled: '#e6a23c',
  }
  return { background: colors[action] || '#909399', verticalAlign: 'middle' }
}

// —— Actions ——
async function handleApprove() {
  approving.value = true
  try {
    await approveApproval(props.approvalId, { comment: comment.value || null })
    ElMessage.success('审批通过')
    emit('approved')
    close()
  } catch (e) {
    ElMessage.error(e?.response?.data?.title || '操作失败')
  }
  approving.value = false
}

async function handleReject() {
  if (!comment.value) {
    ElMessage.warning('驳回时请填写原因')
    rejecting.value = false
    return
  }
  rejecting.value = true
  try {
    await rejectApproval(props.approvalId, { comment: comment.value })
    ElMessage.success('已驳回')
    emit('rejected')
    close()
  } catch (e) {
    ElMessage.error(e?.response?.data?.title || '操作失败')
  }
  rejecting.value = false
}

async function handleCancel() {
  try {
    await ElMessageBox.confirm('确定要撤回该审批申请吗？撤回后可重新编辑提交。', '撤回确认', {
      confirmButtonText: '撤回', cancelButtonText: '取消', type: 'warning'
    })
  } catch { return }
  cancelling.value = true
  try {
    await cancelApproval(props.approvalId, { reason: comment.value || undefined })
    ElMessage.success('已撤回')
    emit('cancelled')
    close()
  } catch (e) {
    ElMessage.error(e?.response?.data?.title || '操作失败')
  }
  cancelling.value = false
}

function close() {
  visible.value = false
  comment.value = ''
  activeTab.value = 'flow'
}
</script>

<style scoped>
/* ====== Drawer Header ====== */
.drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding-right: 40px;
}
.drawer-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 16px;
  font-weight: 600;
}
.drawer-id {
  font-size: 12px;
  color: #909399;
  font-family: monospace;
}
.status-tag {
  font-size: 12px;
}

/* ====== Drawer Body ====== */
.drawer-body {
  min-height: 200px;
}

/* ====== Tabs ====== */
.detail-tabs {
  --el-tabs-header-height: 44px;
}
.detail-tabs :deep(.el-tabs__header) {
  margin-bottom: 16px;
}
.detail-tabs :deep(.el-tabs__item) {
  font-size: 14px;
  padding: 0 16px;
}
.tab-label {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

/* ====== Section Cards ====== */
.section-card {
  margin-bottom: 14px;
  border: 1px solid #ebeef5;
  border-radius: 8px;
}
.section-card :deep(.el-card__header) {
  padding: 10px 16px;
  border-bottom: 1px solid #f2f3f5;
  font-size: 13px;
}
.section-card :deep(.el-card__body) {
  padding: 12px 16px;
}
.section-card + .section-card {
  margin-top: 0;
}

/* ====== Tab 1: Summary ====== */
.summary-grid {
  display: flex;
  justify-content: space-between;
  gap: 24px;
}
.summary-main {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.summary-title {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}
.summary-meta {
  display: flex;
  gap: 6px;
  align-items: center;
}
.summary-info {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 260px;
}
.info-row {
  display: flex;
  align-items: center;
  gap: 8px;
}
.info-label {
  font-size: 12px;
  color: #909399;
  min-width: 56px;
  flex-shrink: 0;
}
.info-value {
  font-size: 13px;
  color: #303133;
}
.summary-desc {
  margin-top: 12px;
  padding: 10px 12px;
  background: #f5f7fa;
  border-radius: 6px;
  font-size: 13px;
  color: #606266;
  display: flex;
  align-items: flex-start;
  gap: 6px;
}

/* ====== Tab 1: Approval Steps (Custom Vertical) ====== */
.approval-steps {
  padding: 4px 0;
}
.step-row {
  display: flex;
  gap: 14px;
  min-height: 72px;
}
.step-indicator {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 28px;
  flex-shrink: 0;
}
.step-dot {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid;
  background: #fff;
  font-size: 14px;
  z-index: 1;
  box-sizing: border-box;
}
.dot-submitted {
  border-color: #409eff;
  background: #409eff;
  color: #fff;
}
.dot-completed {
  border-color: #67c23a;
  background: #67c23a;
  color: #fff;
}
.dot-current {
  border-color: #e6a23c;
  color: #e6a23c;
  animation: dot-pulse 2s ease-in-out infinite;
}
.dot-pending {
  border-color: #dcdfe6;
}
.dot-skipped {
  border-color: #dcdfe6;
}
.dot-rejected {
  border-color: #f56c6c;
  background: #f56c6c;
  color: #fff;
}
.dot-ring {
  display: block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #dcdfe6;
}
@keyframes dot-pulse {
  0%, 100% { box-shadow: 0 0 0 0 rgba(230, 162, 60, 0.4); }
  50% { box-shadow: 0 0 0 6px rgba(230, 162, 60, 0.1); }
}
.step-line {
  width: 2px;
  flex: 1;
}
.line-submitted {
  background: linear-gradient(to bottom, #409eff 0%, #67c23a 100%);
}
.line-completed {
  background: linear-gradient(to bottom, #67c23a 0%, #67c23a 100%);
}
.line-current {
  background: linear-gradient(to bottom, #e6a23c 0%, #dcdfe6 100%);
}
.line-pending {
  background: #dcdfe6;
}
.line-skipped {
  background: #dcdfe6;
}
.step-body {
  flex: 1;
  padding-bottom: 16px;
}
.step-header {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.step-role {
  font-weight: 600;
  font-size: 14px;
  color: #303133;
}
.step-status-current .step-role {
  color: #e6a23c;
}
.step-status-completed .step-role {
  color: #303133;
}
.step-status-pending .step-role {
  color: #909399;
}
.step-status-skipped .step-role {
  color: #c0c4cc;
}
.step-status-rejected .step-role {
  color: #f56c6c;
}
.step-level {
  font-size: 11px;
  color: #c0c4cc;
}
.step-detail {
  margin-top: 6px;
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.step-avatar-success {
  background: #67c23a !important;
  font-size: 12px;
}
.step-avatar-submitted {
  background: #409eff !important;
  font-size: 12px;
}
.step-avatar-pending {
  background: #909399 !important;
  font-size: 12px;
}
.step-avatar-danger {
  background: #f56c6c !important;
  font-size: 12px;
}
.step-name {
  font-size: 13px;
  color: #333;
  font-weight: 500;
}
.step-account {
  font-size: 12px;
  color: #909399;
  margin-left: -2px;
}
.step-account::before {
  content: '(';
}
.step-account::after {
  content: ')';
}
.step-time {
  font-size: 12px;
  color: #909399;
}
.step-waiting-text {
  font-size: 13px;
  color: #e6a23c;
}
.step-comment {
  width: 100%;
  margin-top: 4px;
  padding: 6px 10px;
  background: #f5f7fa;
  border-radius: 4px;
  font-size: 12px;
  color: #606266;
}
.step-empty-detail {
  height: 0;
}

/* ====== Tab 1: Comment Box ====== */
.comment-box {
  padding: 4px 0;
}
.comment-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}
.comment-author {
  font-size: 13px;
  font-weight: 500;
  color: #303133;
}
.comment-account {
  font-size: 12px;
  color: #909399;
}
.comment-account::before {
  content: '(';
}
.comment-account::after {
  content: ')';
}
.comment-time {
  font-size: 12px;
  color: #909399;
  margin-left: auto;
}
.comment-text {
  padding: 10px 12px;
  background: #f0f9eb;
  border-radius: 6px;
  font-size: 13px;
  color: #529b2e;
  line-height: 1.6;
}

/* ====== Tab 2: Timeline ====== */
.timeline-content {
  margin-top: -4px;
}
.timeline-header {
  display: flex;
  align-items: center;
  gap: 10px;
}
.timeline-info {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}
.record-level {
  font-size: 12px;
  color: #909399;
  margin-left: 2px;
}
.record-account {
  font-size: 12px;
  color: #909399;
}
.record-account::before {
  content: '(';
}
.record-account::after {
  content: ')';
}
.timeline-comment {
  margin: 6px 0 0 36px;
  padding: 6px 10px;
  background: #f5f7fa;
  border-radius: 4px;
  font-size: 13px;
  color: #606266;
  display: flex;
  align-items: flex-start;
  gap: 4px;
}

/* ====== Tab 3: Import Data ====== */
.cell-error {
  color: #f56c6c;
  font-size: 12px;
}
.cell-warning {
  color: #e6a23c;
  font-size: 12px;
}
.cell-empty {
  color: #c0c4cc;
}

/* ====== Tab 3: 变更摘要卡片布局 ====== */
.change-summary {
  display: flex;
  flex-direction: column;
}
.summary-row {
  display: flex;
  align-items: center;
  padding: 3px 0;
  gap: 8px;
}
.row-changed {
  background: #fffcf0;
  margin: 0 -12px;
  padding: 3px 12px;
  border-radius: 4px;
}
.summary-label {
  min-width: 72px;
  font-size: 13px;
  color: #606266;
  flex-shrink: 0;
}
.summary-values {
  display: flex;
  align-items: center;
  gap: 6px;
  flex: 1;
  flex-wrap: wrap;
}
.summary-oldval {
  font-size: 13px;
  color: #909399;
  text-decoration: line-through;
}
.summary-arrow {
  font-size: 14px;
  color: #c0c4cc;
  flex-shrink: 0;
}
.summary-newval {
  font-size: 13px;
  color: #e6a23c;
  font-weight: bold;
}
.summary-newval-only {
  color: #303133;
  font-weight: 500;
  white-space: pre-line;
  line-height: 1.7;
}
.summary-empty {
  font-size: 13px;
  color: #c0c4cc;
}
.summary-tag {
  flex-shrink: 0;
  margin-left: auto;
}

/* ====== Tab 3: 费用项卡片列表容器 ====== */
.fee-cards-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

/* ====== Drawer Footer ====== */
.drawer-footer {
  padding: 0;
}
.footer-comment {
  margin-bottom: 12px;
}
.footer-buttons {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.footer-left,
.footer-right {
  display: flex;
  gap: 8px;
}
</style>
