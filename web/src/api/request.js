import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '../router'

const request = axios.create({
  baseURL: '/api',
  timeout: 30000
})

// Request interceptor
request.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    // ========== 多房东：自动附加 landlordId ==========
    // 从 localStorage 读取用户信息（避免 import store 循环引用）
    try {
      const userStr = localStorage.getItem('user')
      if (userStr) {
        const user = JSON.parse(userStr)
        const currentLandlordId = localStorage.getItem('currentLandlordId')

        // 如果用户选择了特定视角，用那个 landlordId
        if (currentLandlordId) {
          config.params = config.params || {}
          config.params.landlordId = currentLandlordId
        } else if (user.homeLandlordId && !user.isSuperAdmin) {
          // 普通用户自动带上自己的 homeLandlordId
          config.params = config.params || {}
          config.params.landlordId = user.homeLandlordId
        }
        // 超级管理员选择"全部数据"时，不传 landlordId（后端不限制）
      }
    } catch (e) {
      // ignore parse errors
    }

    return config
  },
  error => {
    return Promise.reject(error)
  }
)

// Response interceptor
request.interceptors.response.use(
  response => {
    const res = response.data
    return res
  },
  error => {
    if (error.response) {
      switch (error.response.status) {
        case 401:
          localStorage.removeItem('token')
          localStorage.removeItem('user')
          router.push('/login')
          ElMessage.error('登录已过期，请重新登录')
          break
        case 403:
          ElMessage.error(error.response.data?.message || '没有权限执行此操作，请联系管理员')
          // 触发全局权限拒绝事件，供权限状态刷新使用
          window.dispatchEvent(new CustomEvent('permission-denied', {
            detail: { path: error.config?.url, method: error.config?.method }
          }))
          break
        case 404:
          ElMessage.error('请求的资源不存在')
          break
        case 500:
          ElMessage.error('服务器错误')
          break
        default:
          ElMessage.error(error.response.data?.message || '请求失败')
      }
    } else {
      ElMessage.error('网络连接失败')
    }
    return Promise.reject(error)
  }
)

export default request
