/**
 * =========================================================================
 *  Axios HTTP 客户端配置
 *  功能：统一管理 API 请求的发送、认证、多公司视角、错误处理
 *  核心机制：
 *    1. 请求拦截器 — 自动注入 JWT Token 和公司视角参数
 *    2. 响应拦截器 — 自动剥壳 response.data，全局处理 HTTP 错误
 *    3. handleApiError — 组件可调用的统一错误提示函数
 * =========================================================================
 */

import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '../router'

// ---------------------------------------------------------------------------
// Axios 实例：所有 API 请求的基础配置
// - baseURL: '/api' → 通过 Vite proxy 转发到后端 (localhost:5000)
// - timeout: 30s 超时自动断开
// ---------------------------------------------------------------------------
const request = axios.create({
  baseURL: '/api',
  timeout: 30000
})

// ===========================================================================
// handleApiError — 统一的业务错误处理函数
// 组件在 catch 块中手动调用，而非自动执行
//
// 设计意图：
//   - 后端返回 { code, message } 结构的业务错误 → 显示 message
//   - HTTP 500 → 隐藏详情，只提示"系统错误"
//   - 其他 → 取 message 或 error 字段
//   - 所有调用前先 ElMessage.closeAll() 清除响应拦截器已弹出的消息
//
// 调用示例：
//   try { await submitContractCreateRequest(data) }
//   catch (e) { handleApiError(e, '提交失败') }
//
// @param {Error} e             从 catch 捕获的 AxiosError
// @param {string} defaultMsg   兜底提示文案，当后端未返回具体消息时使用
// ===========================================================================
export function handleApiError(e, defaultMsg = '操作失败') {
  ElMessage.closeAll()                          // 先清除拦截器的消息，防叠加
  const status = e?.response?.status
  const data = e?.response?.data
  if (data?.code) {
    ElMessage.error(data.message || defaultMsg)  // 业务错误（如 "合同编号已存在"）
  } else if (status === 500) {
    ElMessage.error('系统错误，请稍后重试')       // 服务端异常，不暴露内部错误
  } else {
    ElMessage.error(data?.message || data?.error || defaultMsg)
  }
}

// ===========================================================================
// 请求拦截器 — 发送前注入认证信息和公司视角
// 执行顺序：组件调用 request(config) → 请求拦截器 → 实际 HTTP 请求
// ===========================================================================
request.interceptors.request.use(
  config => {
    // -----------------------------------------------------------------------
    // 1. 认证令牌：从 localStorage 读取 JWT Token，注入 Authorization 头
    //    token 在 login() 成功后写入，logout() 时清除
    // -----------------------------------------------------------------------
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    // -----------------------------------------------------------------------
    // 2. 多公司参数自动附加
    //
    // 公司视角优先级（由高到低）：
    //   a) currentCompanyId（超管手动切换的视角）→ 传该值
    //   b) homeCompanyId（普通用户的所属公司）   → 传该值
    //   c) 超管选择"全部数据"（currentCompanyId=null）→ 不传，后端不限制
    //
    // 为什么从 localStorage 读而非 Pinia store？
    //   避免循环依赖 — store 中 import request, request 中 import store 会死循环
    // -----------------------------------------------------------------------
    try {
      const userStr = localStorage.getItem('user')
      if (userStr) {
        const user = JSON.parse(userStr)
        const currentCompanyId = localStorage.getItem('currentCompanyId')

        if (currentCompanyId) {
          // 超管切换了公司视角 → 附加该 ID
          config.params = config.params || {}
          config.params.companyId = currentCompanyId
        } else if (user.homeCompanyId && !user.isSuperAdmin) {
          // 普通用户 → 自动带上所属公司
          config.params = config.params || {}
          config.params.companyId = user.homeCompanyId
        }
        // 超管选"全部数据": currentCompanyId 为 null → 不传 companyId
      }
    } catch (e) {
      // localStorage.getItem 解析失败时静默跳过
      // 通常在首次登录前或 localStorage 被清空时发生
    }

    return config
  },
  error => {
    // 请求构造阶段的错误（极少发生），继续往下抛给组件的 catch
    return Promise.reject(error)
  }
)

// ===========================================================================
// 响应拦截器 — 统一处理后端响应和 HTTP 错误
// ===========================================================================
request.interceptors.response.use(
  // -----------------------------------------------------------------------
  // 成功响应：自动剥壳 response.data
  // 组件调用的返回值 = response.data（即后端返回的 JSON 本体）
  // 例如：后端返回 { id, name }，组件直接拿到 { id, name }
  // -----------------------------------------------------------------------
  response => {
    return response.data
  },

  // -----------------------------------------------------------------------
  // 失败响应：按 HTTP Status 分类处理
  // 处理完仍继续 throw，让组件的 catch 块也能捕获并显示业务错误
  //
  // 设计注意：
  //   - 拦截器显示通用提示（如"服务器错误"）
  //   - 组件的 catch 中 handleApiError 会 ElMessage.closeAll() 再显示具体错误
  //   - 所以用户最终只看到 handleApiError 的消息，拦截器的消息被清掉
  // -----------------------------------------------------------------------
  error => {
    if (error.response) {
      switch (error.response.status) {

        // ----- 401 Unauthorized：令牌过期或无效 -----
        case 401:
          localStorage.removeItem('token')
          localStorage.removeItem('user')
          router.push('/login')
          ElMessage.closeAll()
          ElMessage.error('登录已过期，请重新登录')
          break

        // ----- 403 Forbidden：无权限操作 -----
        case 403:
          ElMessage.error(error.response.data?.message || '没有权限执行此操作，请联系管理员')
          // 触发全局权限拒绝事件，供权限状态刷新或权限缓存清除使用
          window.dispatchEvent(new CustomEvent('permission-denied', {
            detail: { path: error.config?.url, method: error.config?.method }
          }))
          break

        // ----- 404 Not Found：请求的资源不存在 -----
        case 404:
          ElMessage.error('请求的资源不存在')
          break

        // ----- 500 Internal Server Error：服务端异常 -----
        case 500:
          ElMessage.error('服务器错误')
          break

        // ----- 其他 HTTP 错误 -----
        default:
          ElMessage.error(error.response.data?.message || '请求失败')
      }
    } else {
      // ----- 网络层错误（无响应）-----
      // 可能原因：跨域、网络断开、后端未启动、DNS 解析失败
      ElMessage.error('网络连接失败')
    }

    // 继续抛出错误，让组件层也能捕获到
    return Promise.reject(error)
  }
)

export default request
