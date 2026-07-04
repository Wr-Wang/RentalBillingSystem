<template>
  <div>
    <div class="page-header">
      <h2>通知中心</h2>
      <el-badge :value="unreadCount" :max="99">
        <el-button @click="handleMarkAllRead" :disabled="unreadCount === 0">
          <el-icon><Check /></el-icon>全部标记已读
        </el-button>
      </el-badge>
    </div>

    <!-- Tabs with unread badges -->
    <el-tabs v-model="activeTab" @tab-change="handleTabChange">
      <el-tab-pane label="全部" name="all" />
      <el-tab-pane name="approval">
        <template #label>
          <el-badge :value="store.unreadCounts.Approval" :max="99" :hidden="store.unreadCounts.Approval === 0">
            <span>审批通知</span>
          </el-badge>
        </template>
      </el-tab-pane>
      <el-tab-pane name="renewal">
        <template #label>
          <el-badge :value="store.unreadCounts.Renewal" :max="99" :hidden="store.unreadCounts.Renewal === 0">
            <span>续签通知</span>
          </el-badge>
        </template>
      </el-tab-pane>
      <el-tab-pane name="collection">
        <template #label>
          <el-badge :value="store.unreadCounts.Collection" :max="99" :hidden="store.unreadCounts.Collection === 0">
            <span>催缴结果</span>
          </el-badge>
        </template>
      </el-tab-pane>
      <el-tab-pane name="system">
        <template #label>
          <el-badge :value="store.unreadCounts.System" :max="99" :hidden="store.unreadCounts.System === 0">
            <span>系统通知</span>
          </el-badge>
        </template>
      </el-tab-pane>
    </el-tabs>

    <!-- Search/Filter -->
    <div class="search-bar">
      <el-select v-model="search.readStatus" placeholder="已读状态" clearable style="width: 130px;" @change="handleSearch">
        <el-option label="全部" value="" />
        <el-option label="未读" value="unread" />
        <el-option label="已读" value="read" />
      </el-select>
      <el-date-picker v-model="search.dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width: 220px;" @change="handleSearch" />
      <el-input v-model="search.keyword" placeholder="搜索标题/内容" clearable style="width: 200px;" @keyup.enter="handleSearch" @clear="handleSearch" />
      <el-button type="primary" @click="handleSearch">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <!-- Notification List -->
    <el-card>
      <template #header>
        <span>通知列表（{{ store.total }} 条）</span>
        <span style="float: right; color: #909399; font-size: 12px;">● 未读  ○ 已读</span>
      </template>

      <div v-if="store.loading" style="text-align: center; padding: 40px; color: #c0c4cc;">
        <el-icon :size="48" class="is-loading"><Loading /></el-icon>
        <p style="margin-top: 12px;">加载中...</p>
      </div>

      <div v-else-if="store.notifications.length === 0" style="text-align: center; padding: 40px; color: #c0c4cc;">
        <el-icon :size="48"><Bell /></el-icon>
        <p style="margin-top: 12px;">暂无通知</p>
      </div>

      <div v-for="item in store.notifications" :key="item.id" class="notification-item" :class="{ unread: !item.isRead }" @click="viewDetail(item)">
        <div class="notif-row">
          <span class="notif-dot" :class="{ read: item.isRead }">●</span>
          <span class="notif-category">
            <el-tag size="small" :type="categoryTag(item.category)" effect="plain">{{ categoryLabel(item.category) }}</el-tag>
          </span>
          <span class="notif-title">{{ item.title }}</span>
          <span class="notif-time">{{ formatTime(item.createdAt) }}</span>
        </div>
        <div class="notif-content">{{ item.content }}</div>
        <div class="notif-actions" v-if="!item.isRead">
          <el-button text size="small" type="primary" @click.stop="handleMarkRead(item)">标记已读</el-button>
          <el-button text size="small" type="primary" @click.stop="goToBiz(item)">查看详情</el-button>
        </div>
      </div>

      <div style="margin-top: 16px; text-align: center;">
        <el-pagination
          v-model:current-page="pagination.page"
          v-model:page-size="pagination.pageSize"
          :total="store.total"
          layout="total, prev, pager, next"
          small
          @current-change="handlePageChange"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Bell, Check, Loading } from '@element-plus/icons-vue'
import { useNotificationStore } from '../../store/notification'

const router = useRouter()
const store = useNotificationStore()
const activeTab = ref('all')

const search = reactive({ readStatus: '', dateRange: null, keyword: '' })
const pagination = reactive({ page: 1, pageSize: 10 })

const unreadCount = computed(() => store.unreadCounts.Total)

function buildParams() {
  const params = { page: pagination.page, pageSize: pagination.pageSize }
  if (activeTab.value !== 'all') params.category = activeTab.value
  if (search.readStatus === 'unread') params.isRead = false
  else if (search.readStatus === 'read') params.isRead = true
  if (search.keyword) params.keyword = search.keyword
  if (search.dateRange) {
    params.dateFrom = search.dateRange[0]
    params.dateTo = search.dateRange[1]
  }
  return params
}

async function loadNotifications() {
  await store.fetchNotifications(buildParams())
}

function handleTabChange() {
  pagination.page = 1
  loadNotifications()
}

function handleSearch() {
  pagination.page = 1
  loadNotifications()
}

function resetSearch() {
  search.readStatus = ''
  search.dateRange = null
  search.keyword = ''
  handleSearch()
}

function handlePageChange(page) {
  pagination.page = page
  loadNotifications()
}

async function handleMarkRead(item) {
  await store.markRead(item.id)
  ElMessage.success('已标记为已读')
}

async function handleMarkAllRead() {
  await store.markAllRead()
  ElMessage.success('全部标记为已读')
}

async function viewDetail(item) {
  if (!item.isRead) {
    try { await store.markRead(item.id) } catch (e) { /* 静默 */ }
  }
}

function goToBiz(item) {
  const path = bizPath(item)
  if (path) router.push(path)
}

function bizPath(item) {
  const cat = item.category?.toLowerCase()
  if (cat === 'approval') return '/approvals'
  if (cat === 'renewal') return '/contracts'
  if (cat === 'collection') return '/collection/records'
  if (cat === 'system') return '/receipts'
  return null
}

function categoryLabel(cat) {
  const map = { approval: '审批', renewal: '续签', collection: '催缴', system: '系统' }
  return map[cat?.toLowerCase()] || cat
}

function categoryTag(cat) {
  const map = { approval: 'primary', renewal: 'success', collection: 'warning', system: 'info' }
  return map[cat?.toLowerCase()] || 'info'
}

function formatTime(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

onMounted(() => {
  loadNotifications()
  store.fetchUnreadCounts()
})

onUnmounted(() => {
  // 不在此处 stopPolling，因为 MainLayout 管理轮询生命周期
})
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

.search-bar {
  display: flex;
  gap: 12px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}
</style>
