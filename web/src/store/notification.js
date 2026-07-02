import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import { getNotifications, getUnreadCounts, markNotificationRead, markAllNotificationsRead } from '../api'

export const useNotificationStore = defineStore('notification', () => {
  const notifications = ref([])
  const total = ref(0)
  const loading = ref(false)
  const unreadCounts = reactive({
    Approval: 0,
    Renewal: 0,
    Collection: 0,
    System: 0,
    Total: 0
  })

  let pollTimer = null

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

  async function markRead(id) {
    await markNotificationRead(id)
    const item = notifications.value.find(n => n.id === id)
    if (item) item.isRead = true
    // 刷新未读计数
    await fetchUnreadCounts()
  }

  async function markAllRead() {
    await markAllNotificationsRead()
    notifications.value.forEach(n => { n.isRead = true })
    await fetchUnreadCounts()
  }

  function startPolling(interval = 60000) {
    stopPolling()
    pollTimer = setInterval(fetchUnreadCounts, interval)
  }

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
