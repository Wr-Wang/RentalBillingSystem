import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginApi, setMyDefaultCompany as setDefaultCompanyApi } from '../api/index'

export const useUserStore = defineStore('user', () => {
  const token = ref(localStorage.getItem('token') || '')
  const user = ref(JSON.parse(localStorage.getItem('user') || '{}'))
  const permissions = ref(JSON.parse(localStorage.getItem('permissions') || '[]'))

  // ========== 多公司扩展字段 ==========
  const homeCompanyId = ref(user.value.homeCompanyId || null)   // 所属公司
  const isSuperAdmin = ref(user.value.isSuperAdmin || false)       // 是否超级管理员
  const currentCompanyId = ref(null)                              // 当前切换的视角（超管专用）
  const companyList = ref(user.value.companyList || [])          // 用户可选的公司列表

  // 计算属性：是否在查看全部数据（超管专用）
  const isViewingAll = computed(() => isSuperAdmin.value && currentCompanyId.value === null)

  // 计算属性：当前生效的 companyId（用于 API 请求）
  const effectiveCompanyId = computed(() => {
    if (isSuperAdmin.value && currentCompanyId.value) {
      return currentCompanyId.value
    }
    return homeCompanyId.value
  })

  // 当前视角的公司名称
  const currentCompanyName = computed(() => {
    if (isViewingAll.value) return '全部数据'
    if (currentCompanyId.value) {
      const found = companyList.value.find(l => l.id === currentCompanyId.value)
      return found ? found.name : '未知公司'
    }
    if (homeCompanyId.value) {
      const found = companyList.value.find(l => l.id === homeCompanyId.value)
      return found ? found.name : '未知公司'
    }
    return '系统'
  })

  async function login(credentials) {
    const res = await loginApi(credentials)
    token.value = res.token || res.accessToken
    user.value = res.user
    permissions.value = res.permissions || []

    // 解析多公司字段
    homeCompanyId.value = res.user?.homeCompanyId || null
    isSuperAdmin.value = res.user?.isSuperAdmin || false
    companyList.value = res.user?.companyList || []
    // 优先使用数据库持久化的默认公司，否则超管默认查看全部
    currentCompanyId.value = res.user?.defaultCompanyId || null

    localStorage.setItem('token', token.value)
    localStorage.setItem('user', JSON.stringify(user.value))
    localStorage.setItem('permissions', JSON.stringify(permissions.value))

    return res
  }

  function logout() {
    token.value = ''
    user.value = {}
    permissions.value = []
    homeCompanyId.value = null
    isSuperAdmin.value = false
    currentCompanyId.value = null
    companyList.value = []

    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('permissions')
    localStorage.removeItem('currentCompanyId')
  }

  function hasPermission(code) {
    return permissions.value.includes(code)
  }

  // ========== 超级管理员视角切换（持久化到数据库） ==========
  async function switchToCompany(companyId) {
    currentCompanyId.value = companyId
    localStorage.setItem('currentCompanyId', companyId || '')
    try { await setDefaultCompanyApi(companyId) } catch (e) { /* 静默 */ }
  }

  async function switchToAll() {
    currentCompanyId.value = null
    localStorage.removeItem('currentCompanyId')
    try { await setDefaultCompanyApi(null) } catch (e) { /* 静默 */ }
  }

  // 初始化时从 localStorage 恢复视角状态
  function restoreView() {
    const saved = localStorage.getItem('currentCompanyId')
    if (saved && saved.length > 0 && isSuperAdmin.value) {
      currentCompanyId.value = saved
    }
  }

  return {
    token, user, permissions,
    homeCompanyId, isSuperAdmin,
    currentCompanyId, companyList,
    isViewingAll, effectiveCompanyId, currentCompanyName,
    login, logout, hasPermission,
    switchToCompany, switchToAll,
    restoreView
  }
})
