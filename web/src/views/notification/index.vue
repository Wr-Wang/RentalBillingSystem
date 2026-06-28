<template>
  <div>
    <div class="page-header">
      <h2>通知中心</h2>
      <el-badge :value="unreadCount" :max="99">
        <el-button @click="markAllRead" :disabled="unreadCount === 0">
          <el-icon><Check /></el-icon>全部标记已读
        </el-button>
      </el-badge>
    </div>

    <!-- Tabs with unread badges -->
    <el-tabs v-model="activeTab" @tab-change="handleTabChange">
      <el-tab-pane label="全部" name="all" />
      <el-tab-pane name="approval">
        <template #label>
          <el-badge :value="unreadByCategory.approval" :max="99" :hidden="unreadByCategory.approval === 0">
            <span>审批通知</span>
          </el-badge>
        </template>
      </el-tab-pane>
      <el-tab-pane name="collection">
        <template #label>
          <el-badge :value="unreadByCategory.collection" :max="99" :hidden="unreadByCategory.collection === 0">
            <span>催缴结果</span>
          </el-badge>
        </template>
      </el-tab-pane>
      <el-tab-pane name="system">
        <template #label>
          <el-badge :value="unreadByCategory.system" :max="99" :hidden="unreadByCategory.system === 0">
            <span>系统通知</span>
          </el-badge>
        </template>
      </el-tab-pane>
    </el-tabs>

    <!-- Search/Filter -->
    <div class="search-bar">
      <el-select v-model="search.readStatus" placeholder="已读状态" clearable style="width: 130px;">
        <el-option label="全部" value="" />
        <el-option label="未读" value="unread" />
        <el-option label="已读" value="read" />
      </el-select>
      <el-date-picker v-model="search.dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width: 220px;" />
      <el-input v-model="search.keyword" placeholder="搜索标题/内容" clearable style="width: 200px;" />
      <el-button type="primary">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <!-- Notification List -->
    <el-card>
      <template #header>
        <span>通知列表（{{ filteredList.length }} 条）</span>
        <span style="float: right; color: #909399; font-size: 12px;">● 未读  ○ 已读</span>
      </template>

      <div v-if="filteredList.length === 0" style="text-align: center; padding: 40px; color: #c0c4cc;">
        <el-icon :size="48"><Bell /></el-icon>
        <p style="margin-top: 12px;">暂无通知</p>
      </div>

      <div v-for="item in filteredList" :key="item.id" class="notification-item" :class="{ unread: !item.isRead }" @click="viewDetail(item)">
        <div class="notif-row">
          <span class="notif-dot" :class="{ read: item.isRead }">●</span>
          <span class="notif-category">
            <el-tag size="small" :type="categoryTag(item.category)" effect="plain">{{ categoryLabel(item.category) }}</el-tag>
          </span>
          <span class="notif-title">{{ item.title }}</span>
          <span class="notif-time">{{ item.time }}</span>
        </div>
        <div class="notif-content">{{ item.content }}</div>
        <div class="notif-actions" v-if="!item.isRead">
          <el-button text size="small" type="primary" @click.stop="markRead(item)">标记已读</el-button>
          <el-button text size="small" type="primary" @click.stop="goToBiz(item)">查看详情</el-button>
        </div>
      </div>

      <div style="margin-top: 16px; text-align: center;">
        <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" layout="total, prev, pager, next" small />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Bell, Check } from '@element-plus/icons-vue'

const router = useRouter()
const activeTab = ref('all')

const search = reactive({ readStatus: '', dateRange: null, keyword: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 25 })

// Mock notifications
const notifications = ref([
  { id: 'n1', category: 'approval', title: '合同终止申请已通过', content: '合同HT-2026-001终止申请已由运营主管王五审批通过，请确认押金退还事项。', time: '2026-07-03 14:30', isRead: false, bizPath: '/approvals' },
  { id: 'n2', category: 'collection', title: '租客张三逾期催缴已发送', content: '合同HT-2026-005 逾期7天，已自动发送短信催缴提醒。', time: '2026-07-02 09:00', isRead: false, bizPath: '/collection/records' },
  { id: 'n3', category: 'system', title: '月度应收已生成', content: '2026年7月应收计划已全部生成，共156条应收记录。', time: '2026-06-25 08:00', isRead: false, bizPath: '/receipts' },
  { id: 'n4', category: 'approval', title: '租金调整申请待审批', content: '合同HT-2026-002 租金调整 ¥3,800 → ¥4,200，等待您的审批。', time: '2026-07-01 10:00', isRead: false, bizPath: '/approvals' },
  { id: 'n5', category: 'approval', title: '费用调价已通过', content: '合同HT-2026-003 电费调价 0.85→0.80 申请已由运营主管审批通过。', time: '2026-06-30 16:00', isRead: true, bizPath: '/approvals' },
  { id: 'n6', category: 'system', title: '滞纳金已计算', content: '2026-06-27 滞纳金计算完成，共涉及15笔逾期应收，合计滞纳金¥1,245.00。', time: '2026-06-27 02:00', isRead: true, bizPath: '/system/late-fee' },
  { id: 'n7', category: 'collection', title: '法律催缴建议', content: '合同HT-2026-008 逾期已超30天，欠费合计¥8,200，建议启动法律催缴程序。', time: '2026-06-26 09:00', isRead: true, bizPath: '/collection/records' }
])

// Computed
const unreadCount = computed(() => notifications.value.filter(n => !n.isRead).length)

const unreadByCategory = computed(() => {
  const result = { approval: 0, collection: 0, system: 0 }
  notifications.value.forEach(n => {
    if (!n.isRead) result[n.category]++
  })
  return result
})

const filteredList = computed(() => {
  let list = notifications.value
  // Tab filter
  if (activeTab.value !== 'all') {
    list = list.filter(n => n.category === activeTab.value)
  }
  // Read status filter
  if (search.readStatus === 'unread') list = list.filter(n => !n.isRead)
  else if (search.readStatus === 'read') list = list.filter(n => n.isRead)
  // Keyword filter
  if (search.keyword) {
    const kw = search.keyword.toLowerCase()
    list = list.filter(n => n.title.toLowerCase().includes(kw) || n.content.toLowerCase().includes(kw))
  }
  return list
})

function categoryLabel(cat) {
  return { approval: '审批', collection: '催缴', system: '系统' }[cat] || cat
}
function categoryTag(cat) {
  return { approval: 'primary', collection: 'warning', system: 'info' }[cat] || 'info'
}

function handleTabChange() { /* filter will recompute */ }
function resetSearch() { search.readStatus = ''; search.dateRange = null; search.keyword = '' }

function markRead(item) {
  item.isRead = true
  ElMessage.success('已标记为已读')
}

function markAllRead() {
  notifications.value.forEach(n => { n.isRead = true })
  ElMessage.success('全部标记为已读')
}

function viewDetail(item) {
  if (!item.isRead) item.isRead = true
}

function goToBiz(item) {
  if (item.bizPath) router.push(item.bizPath)
}
</script>

<style scoped>
.notification-item {
  padding: 16px;
  border-bottom: 1px solid #ebeef5;
  cursor: pointer;
  transition: background 0.2s;
}
.notification-item:hover { background: #f5f7fa; }
.notification-item.unread { background: #f0f9ff; }
.notification-item.unread:hover { background: #e6f7ff; }

.notif-row {
  display: flex;
  align-items: center;
  gap: 10px;
}
.notif-dot {
  color: #409eff;
  font-size: 14px;
  width: 14px;
}
.notif-dot.read { color: #c0c4cc; }
.notif-title { flex: 1; font-weight: 500; color: #303133; }
.notif-time { color: #909399; font-size: 12px; white-space: nowrap; }

.notif-content {
  margin: 6px 0 4px 24px;
  color: #606266;
  font-size: 13px;
  line-height: 1.5;
}
.notif-actions {
  margin-left: 24px;
  margin-top: 4px;
}
</style>
