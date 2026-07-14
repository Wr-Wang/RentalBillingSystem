/**
 * =========================================================================
 *  BroadcastChannel 跨标签页通信工具
 *
 *  为什么需要：
 *    多标签页场景下，一个标签页的操作（提交审批、登出等）应即时通知
 *    其他标签页。仅靠轮询有延迟（60s），WebSocket 又太重。
 *
 *  使用场景：
 *    - 审批提交 → 其他标签页即时刷新待审批数
 *    - 登出 → 其他标签页同步跳登录页
 *    - 公司切换 → 其他标签页同步视角
 * =========================================================================
 */

const CHANNEL_NAME = 'rbs-broadcast'

/** @type {BroadcastChannel | null} */
let channel = null

/**
 * 获取 BroadcastChannel 实例（懒初始化）
 * BroadcastChannel 在同源页面间共享，每个标签页通过频道名连接
 */
function getChannel() {
  if (!channel) {
    try {
      channel = new BroadcastChannel(CHANNEL_NAME)
    } catch {
      // 极旧浏览器不支持 BroadcastChannel，静默降级
      return null
    }
  }
  return channel
}

/**
 * 发送消息到所有同频道标签页
 * @param {string} type    消息类型（如 'NOTIFICATION'、'LOGOUT'）
 * @param {any}    payload 附加数据（可选）
 */
export function broadcast(type, payload) {
  const ch = getChannel()
  if (!ch) return
  ch.postMessage({ type, payload, from: window.name || 'unknown' })
}

/**
 * 监听指定类型的跨标签页消息
 * @param {string}   type     消息类型
 * @param {Function} handler  回调函数 (payload) => void
 * @returns {Function}        取消监听的函数
 *
 * 用法：
 *   const unlisten = onMessage('NOTIFICATION', () => fetchUnreadCounts())
 *   // 组件销毁前调用 unlisten()
 */
export function onMessage(type, handler) {
  const ch = getChannel()
  if (!ch) return () => {}

  function listener(event) {
    if (event.data?.type === type) {
      handler(event.data.payload, event.data.from)
    }
  }

  ch.addEventListener('message', listener)

  return () => {
    try { ch.removeEventListener('message', listener) } catch { /* 静默 */ }
  }
}

/**
 * 关闭频道连接（应用销毁时调用）
 */
export function closeChannel() {
  if (channel) {
    channel.close()
    channel = null
  }
}
