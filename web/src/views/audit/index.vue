<template>
  <div>
    <div class="page-header">
      <h2>变更审计</h2>
    </div>

    <!-- 搜索栏 -->
    <div class="search-bar">
      <el-select v-model="search.tableName" placeholder="选择实体类型" clearable filterable style="width: 200px;" @change="onTableChange">
        <el-option
          v-for="t in entityTypes"
          :key="t.tableName"
          :label="t.displayName"
          :value="t.tableName"
        />
      </el-select>
      <el-input v-model="search.recordId" placeholder="记录ID" clearable style="width: 200px;" @clear="fetchHistory" @keyup.enter="fetchHistory" />
      <el-date-picker
        v-model="search.dateRange"
        type="daterange"
        range-separator="至"
        start-placeholder="开始日期"
        end-placeholder="结束日期"
        style="width: 220px;"
        @change="fetchHistory"
      />
      <el-button type="primary" @click="fetchHistory">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
      <el-tag v-if="!entityTypes.length" type="warning" style="margin-left: 8px;">实体类型加载中...</el-tag>
    </div>

    <!-- 审计统计 -->
    <el-card style="margin-bottom: 16px;">
      <template #header>审计统计</template>
      <el-row :gutter="16">
        <el-col :span="6">今日变更: {{ stats.todayCount }} 次</el-col>
        <el-col :span="6">本周变更: {{ stats.weekCount }} 次</el-col>
        <el-col :span="6">本月变更: {{ stats.monthCount }} 次</el-col>
        <el-col :span="6">涉及表: {{ stats.totalTables }} 张</el-col>
      </el-row>
    </el-card>

    <!-- 变更时间线 -->
    <el-card>
      <template #header>变更时间线</template>
      <el-timeline v-if="auditLogs.length > 0">
        <el-timeline-item
          v-for="(log, index) in auditLogs"
          :key="index"
          :timestamp="formatDateTime(log.auditChangedAt)"
          :type="auditTimelineType(log.auditAction)"
        >
          <!-- 标题区 -->
          <div class="audit-entry">
            <div class="audit-header">
              <el-tag :type="auditTagType(log.auditAction)" size="small">
                {{ auditActionLabel(log.auditAction) }}
              </el-tag>
              <span class="entity-name">{{ log.entityDisplayName }}</span>
              <el-tag size="small" type="info">v{{ log.auditVersionNo }}</el-tag>
              <span style="margin-left: 8px; color: #909399; font-size: 12px;">操作人: {{ log.changedByName || log.auditChangedBy }}</span>
            </div>

            <!-- 关键信息区（始终展示） -->
            <div v-if="hasKeyValues(log)" class="audit-section">
              <span class="section-label">📋 关键信息:</span>
              <span v-for="(val, key) in log.keyValues" :key="key" class="key-field-item">
                {{ fieldLabel(key) }}: <strong>{{ formatValue(val) }}</strong>
              </span>
            </div>

            <!-- 变更数据区（Update 有变更字段时展示） -->
            <div v-if="log.changedFieldNames && log.changedFieldNames.length > 0" class="audit-section">
              <span class="section-label">✏️ 变更内容:</span>
              <div class="change-list">
                <div v-for="field in visibleFields(log)" :key="field" class="change-item">
                  <span class="change-field">{{ fieldLabel(field) }}:</span>
                  <span class="change-value">{{ formatValue(log.changedValues[field]) }}</span>
                </div>
                <el-button v-if="log.changedFieldNames.length > 5" text size="small" type="primary" @click="toggleExpand(log)">
                  {{ log._expanded ? '收起' : `查看全部 ${log.changedFieldNames.length} 项变更` }}
                </el-button>
              </div>
            </div>

            <!-- Delete 标记 -->
            <div v-if="log.auditAction === 'Delete'" class="audit-section">
              <span class="section-label" style="color: #f56c6c;">🗑️ 该记录已被删除</span>
            </div>

            <!-- 版本对比入口 -->
            <div v-if="log.auditVersionNo > 1" style="margin-top: 6px;">
              <el-button text size="small" type="primary" @click="openCompare(log.entityId, log.auditVersionNo - 1, log.auditVersionNo)">
                v{{ log.auditVersionNo - 1 }} vs v{{ log.auditVersionNo }} 对比
              </el-button>
            </div>
          </div>
        </el-timeline-item>
      </el-timeline>
      <el-empty v-else description="暂无审计记录" />
    </el-card>

    <!-- 分页 -->
    <div style="margin-top: 16px; display: flex; justify-content: flex-end;">
      <el-pagination
        v-model:current-page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @change="fetchHistory"
      />
    </div>

    <!-- 版本对比 Dialog -->
    <el-dialog :draggable="true" v-model="showCompare" :title="'版本对比 ' + compareTitle" width="80%">
      <el-table :data="compareData" stripe>
        <el-table-column prop="field" label="字段" width="150" />
        <el-table-column prop="oldValue" label="旧值" show-overflow-tooltip />
        <el-table-column prop="newValue" label="新值" show-overflow-tooltip />
        <el-table-column label="变化" width="80">
          <template #default="{ row }">
            <el-tag v-if="row.changed" type="warning" size="small">已变更</el-tag>
            <el-tag v-else type="info" size="small">不变</el-tag>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="showCompare = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getAuditHistory, getAuditTables, getAuditStats, compareAuditVersions } from '../../api/index'
import { formatDateTime } from '@/utils/chinaTime'

// ===== 字段名 → 中文标签映射 =====
const fieldLabelMap = {
  // 通用
  Id: 'ID', Name: '名称', Code: '编码', Status: '状态',
  IsActive: '启用', Remark: '备注', SortOrder: '排序',
  CreatedBy: '创建人', CreatedAt: '创建时间', UpdatedBy: '更新人', UpdatedAt: '更新时间',
  CompanyId: '公司',
  // 合同
  ContractNo: '合同号', TenantName: '租客', StartDate: '开始日期', EndDate: '结束日期',
  ActualEndDate: '实际终止日', EffectiveDate: '生效日期', ExpiryDate: '失效日期',
  RentAmount: '租金', Deposit: '押金', OutstandingBalance: '欠款余额',
  PrepaidBalance: '预存余额', SettlementCycle: '结算周期', SettlementDay: '结算日',
  // 公司
  ContactPerson: '联系人', Phone: '电话', Address: '地址',
  IdType: '证件类型', IdNumber: '证件号码',
  BankName: '开户行', BankAccount: '银行账号', BankAccountName: '开户名',
  CommissionRate: '佣金比例',
  // 用户
  DisplayName: '显示名', Username: '用户名', Email: '邮箱', IsSuperAdmin: '超级管理员',
  // 房源
  FullCode: '房间编号', BuildingName: '楼栋', BuildingAddress: '楼栋地址',
  RoomName: '房间名', Floor: '楼层', Area: '面积',
  // 租客
  // 费用
  FeeName: '费用名称', Amount: '金额', Unit: '单位', UnitPrice: '单价',
  BillingMode: '计费方式', ChargeType: '收费类型',
  // 账单
  BillNo: '账单号', PeriodYear: '年份', PeriodMonth: '月份', DueDate: '到期日',
  TotalAmount: '总金额', TotalReceived: '已收金额',
  // 收款
  ReceiptNo: '收款单号', ReceivedDate: '收款日期', PaymentChannelId: '支付通道',
  ReferenceNo: '参考号',
  // 审批
  RequestNo: '申请单号', Title: '标题', ApprovalTypeId: '审批类型',
  CurrentLevel: '当前级别', MaxLevel: '最大级别',
  Action: '操作', Comment: '备注', ApproverId: '审批人',
  // 日记账
  Period: '账期', EntryType: '分录类型', GLPosted: '已过账',
  // 会计科目
  SubjectCode: '科目编码', Direction: '方向', Level: '级别', IsLeaf: '叶子科目',
  // 导入
  BatchNo: '批次号', RowIndex: '行号',
  // 调度
  JobName: '任务名称', TargetDate: '目标日期',
  // 节假日
  HolidayName: '节假日', HolidayDate: '日期',
  // 利息
  Rate: '利率',
  // 押金
  // 催缴
  ContactResult: '催缴结果', DayOffset: '偏移天数',
  // 续签
  NewRent: '新租金', NewEndDate: '新结束日', DepositHandling: '押金处理',
  // 合同变更
  ChangeType: '变更类型', OldAmount: '原金额', NewAmount: '新金额',
  Reason: '原因', TerminateType: '终止类型', DepositReturn: '押金退还',
  // 定价标准
  RoomType: '房型', StandardRent: '标准租金',
  // 银行
  TransactionNo: '交易号',
}

function fieldLabel(name) {
  return fieldLabelMap[name] || name
}

// ===== 实体类型列表（动态加载） =====
const entityTypes = ref([])
const loadingEntityTypes = ref(false)

async function loadEntityTypes() {
  loadingEntityTypes.value = true
  try {
    const res = await getAuditTables()
    entityTypes.value = Array.isArray(res) ? res : []
  } catch (e) {
    entityTypes.value = []
    ElMessage.warning('加载实体类型列表失败')
  }
  loadingEntityTypes.value = false
}

// ===== 搜索条件 =====
const search = reactive({
  tableName: '',
  recordId: '',
  dateRange: null
})

const pagination = reactive({
  page: 1,
  pageSize: 10,
  total: 0
})

const stats = reactive({
  todayCount: 0,
  weekCount: 0,
  monthCount: 0,
  totalTables: 0
})

const auditLogs = ref([])
const showCompare = ref(false)
const compareTitle = ref('')
const compareData = ref([])

function onTableChange() {
  pagination.page = 1
  fetchHistory()
}

function auditTimelineType(action) {
  if (action === 'Update') return 'primary'
  if (action === 'Insert' || action === 'Create') return 'success'
  return 'danger'
}

function auditTagType(action) {
  if (action === 'Update') return ''
  if (action === 'Insert' || action === 'Create') return 'success'
  return 'danger'
}

function auditActionLabel(action) {
  if (action === 'Insert' || action === 'Create') return '创建'
  if (action === 'Update') return '更新'
  if (action === 'Delete') return '删除'
  return action
}

function hasKeyValues(log) {
  return log.keyValues && Object.keys(log.keyValues).length > 0
}

function formatValue(val) {
  if (val === null || val === undefined) return '-'
  if (typeof val === 'boolean') return val ? '是' : '否'
  if (val instanceof Date || (typeof val === 'string' && val.includes('T') && !isNaN(Date.parse(val)))) {
    return formatDateTime(val)
  }
  return String(val)
}

function visibleFields(log) {
  if (log._expanded) return log.changedFieldNames
  return log.changedFieldNames.slice(0, 5)
}

function toggleExpand(log) {
  log._expanded = !log._expanded
}

async function fetchHistory() {
  if (!search.tableName) {
    auditLogs.value = []
    pagination.total = 0
    return
  }
  try {
    const params = {
      page: pagination.page,
      pageSize: pagination.pageSize,
      recordId: search.recordId || undefined
    }
    if (search.dateRange && search.dateRange.length === 2) {
      params.startDate = search.dateRange[0].toISOString()
      params.endDate = search.dateRange[1].toISOString()
    }
    const res = await getAuditHistory(search.tableName, params)
    const items = res.items || []
    // 为每条记录添加 _expanded 控制属性
    items.forEach(item => { item._expanded = false })
    auditLogs.value = items
    pagination.total = res.total || 0
  } catch (e) {
    auditLogs.value = []
    pagination.total = 0
  }
}

async function fetchStats() {
  try {
    const res = await getAuditStats()
    if (res) {
      stats.todayCount = res.todayCount ?? 0
      stats.weekCount = res.weekCount ?? 0
      stats.monthCount = res.monthCount ?? 0
      stats.totalTables = res.totalTables ?? 0
    }
  } catch (e) {
    // 静默失败
  }
}

function resetSearch() {
  search.tableName = entityTypes.value.length > 0 ? entityTypes.value[0].tableName : ''
  search.recordId = ''
  search.dateRange = null
  pagination.page = 1
  fetchHistory()
}

async function openCompare(entityId, v1, v2) {
  if (v1 < 1) {
    ElMessage.info('这是第一个版本，无更早版本可对比')
    return
  }
  try {
    compareTitle.value = `v${v1} vs v${v2}`
    const res = await compareAuditVersions(search.tableName, entityId, v1, v2)
    if (res && res.length > 0) {
      compareData.value = res
    } else {
      compareData.value = [{ field: '审计数据', oldValue: '-', newValue: '-', changed: false }]
    }
    showCompare.value = true
  } catch (e) {
    ElMessage.warning('版本对比暂时不可用: ' + e.message)
  }
}

onMounted(async () => {
  await loadEntityTypes()
  // 默认选择第一个实体类型
  if (entityTypes.value.length > 0) {
    search.tableName = entityTypes.value[0].tableName
    fetchHistory()
  }
  fetchStats()
})
</script>

<style scoped>
.audit-entry {
  line-height: 1.6;
}
.audit-header {
  margin-bottom: 6px;
  display: flex;
  align-items: center;
  gap: 6px;
}
.entity-name {
  font-weight: 600;
  font-size: 14px;
}
.audit-section {
  margin: 4px 0;
  padding: 4px 0;
  border-bottom: 1px dashed #eee;
}
.audit-section:last-of-type {
  border-bottom: none;
}
.section-label {
  font-size: 12px;
  color: #909399;
  margin-right: 6px;
  white-space: nowrap;
}
.key-field-item {
  font-size: 13px;
  margin-right: 12px;
}
.change-list {
  margin-top: 2px;
}
.change-item {
  font-size: 13px;
  padding: 1px 0;
  display: flex;
  gap: 8px;
}
.change-field {
  color: #606266;
  min-width: 80px;
}
.change-value {
  color: #303133;
  font-weight: 500;
}
</style>
