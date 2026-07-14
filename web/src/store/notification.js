/**
 * =========================================================================
 *  通知状态管理 (useNotificationStore)
 *
 *  功能：
 *    管理通知列表、未读计数、定时轮询
 *
 *  未读计数分类：
 *    Approval    — 待审批数
 *    Renewal     — 待续签数
 *    Collection  — 待催缴数
 *    System      — 系统通知
 *    Total       — 总和
 *
 *  轮询机制：
 *    startPolling(60000) 在 MainLayout 的 onMounted 中启动
 *    每 60 秒刷新一次未读计数
 *    组件销毁时 stopPolling() 清除定时器
 * =========================================================================
 */
import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import { getNotifications, getUnreadCounts, markNotificationRead, markAllNotificationsRead } from '../api'

export const useNotificationStore = defineStore('notification', () => {
  // ---------------------------------------------------------------------------
  // 状态
  // ---------------------------------------------------------------------------
  /** 通知列表 */
  const notifications = ref([])
  /** 通知总数 */
  const total = ref(0)
  /** 加载中 */
  const loading = ref(false)
  /** 未读计数（按类型分组）*/
  const unreadCounts = reactive({
    Approval: 0,
    Renewal: 0,
    Collection: 0,
    System: 0,
    Total: 0
  })

  /** 轮询定时器句柄 */
  let pollTimer = null

  // =========================================================================
  // fetchNotifications — 获取通知列表
  // @param {Object} params  分页/筛选参数
  // =========================================================================
  async function fetchNotifications(params = {}) {
    loading.value = true
    try {
      const res = await getNotifications(params)
      notifications.value = res.items || []
      total.value = res.total || 0
    } catch (e) {
      notifications.value = []
      total.value = 0
    } finally {
      loading.value = false
    }
  }

  // =========================================================================
  // fetchUnreadCounts — 刷新未读计数
  // 被 startPolling 定时调用，也可手动调用
  // =========================================================================
  async function fetchUnreadCounts() {
    try {
      const res = await getUnreadCounts()
      unreadCounts.Approval = res.approval || 0
      unreadCounts.Renewal = res.renewal || 0
      unreadCounts.Collection = res.collection || 0
      unreadCounts.System = res.system || 0
      unreadCounts.Total = res.total || 0
    } catch (e) {
      // 静默失败，保持上次的值
    }
  }

  // =========================================================================
  // markRead — 标记单条通知为已读
  // @param {string} id  通知 ID
  // =========================================================================
  async function markRead(id) {
    await markNotificationRead(id)
    const item = notifications.value.find(n => n.id === id)
    if (item) item.isRead = true
    await fetchUnreadCounts()
  }

  // =========================================================================
  // markAllRead — 全部标记为已读
  // =========================================================================
  async function markAllRead() {
    await markAllNotificationsRead()
    notifications.value.forEach(n => { n.isRead = true })
    await fetchUnreadCounts()
  }

  // =========================================================================
  // 轮询控制
  // =========================================================================
  /** 启动轮询（默认 60 秒）*/
  function startPolling(interval = 60000) {
    stopPolling()
    pollTimer = setInterval(fetchUnreadCounts, interval)
  }

  /** 停止轮询 */
  function stopPolling() {
    if (pollTimer) {
      clearInterval(pollTimer)
      pollTimer = null
    }
  }

  return {
    notifications, total, loading, unreadCounts,
    fetchNotifications, fetchUnreadCounts, markRead, markAllRead,
    startPolling, stopPolling
  }
})
