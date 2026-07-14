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
 *  通知刷新策略：
 *    1. BroadcastChannel 实时广播 — 其他标签页有操作时即时推送（主要路径）
 *    2. 兜底轮询 5 分钟一次 — 防止广播丢失（备用路径）
 *    3. startPolling() 在 MainLayout onMounted 中启动，stopPolling() 销毁时清除
 *    4. 广播触发后会重置轮询计时器，避免刚刷完又来一次轮询
 * =========================================================================
 */
import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import { getNotifications, getUnreadCounts, markNotificationRead, markAllNotificationsRead } from '../api'
import { onMessage } from '../utils/broadcast'

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
  /** 轮询是否活跃（用于 finally 中判断是否调度下一轮）*/
  let isPolling = false
  /** 当前轮询间隔（默认 5 分钟，作为广播的兜底）*/
  let pollInterval = 300000

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
    } finally {
      // 请求完成后再调度下一次，确保间隔从本次完成算起
      if (isPolling) {
        scheduleNextPoll()
      }
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
  //
  // 有了 BroadcastChannel 之后，轮询降级为兜底机制：
  //   - 实时通知 → 由其他标签页的广播触发
  //   - 兜底轮询 → 每 5 分钟一次，防止广播丢失
  //   - 广播触发后 → 重置轮询计时器（避免刚刷完又轮询）
  // =========================================================================
  /** 调度下一次轮询（完成一次请求后调用）*/
  function scheduleNextPoll(delay) {
    if (pollTimer) {
      clearTimeout(pollTimer)
      pollTimer = null
    }
    pollTimer = setTimeout(fetchUnreadCounts, delay || pollInterval)
  }

  /** 启动轮询：立即执行一次，完成后重新计时 */
  function startPolling(interval) {
    pollInterval = interval || pollInterval
    stopPolling()
    isPolling = true
    fetchUnreadCounts()
  }

  /** 停止轮询 */
  function stopPolling() {
    isPolling = false
    if (pollTimer) {
      clearTimeout(pollTimer)
      pollTimer = null
    }
  }

  // =========================================================================
  // startBroadcastListener — 监听跨标签页通知刷新信号（带防抖）
  //
  // 当其他标签页提交审批/催缴等操作后，会通过 BroadcastChannel 发送
  // NOTIFICATION_REFRESH 消息。本标签页收到后立即刷新未读计数，
  // 无需等待下一次轮询。
  //
  // 防抖：短时间内多次触发（如批量操作），只调用一次 API。
  // 重置轮询：广播触发后重新计时 5 分钟，避免刚刷完又来一次轮询。
  //
  // @returns {Function} 取消监听的函数，在组件销毁时调用
  // =========================================================================
  function startBroadcastListener() {
    let debounceTimer = null

    return onMessage('NOTIFICATION_REFRESH', () => {
      if (debounceTimer) clearTimeout(debounceTimer)
      debounceTimer = setTimeout(() => {
        fetchUnreadCounts()         // 内部 finally 会自动调度下一轮
        debounceTimer = null
      }, 800)
    })
  }

  return {
    notifications, total, loading, unreadCounts,
    fetchNotifications, fetchUnreadCounts, markRead, markAllRead,
    startPolling, stopPolling,
    startBroadcastListener
  }
})
