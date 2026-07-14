/**
 * =========================================================================
 *  用户状态管理 (useUserStore)
 *
 *  功能：
 *    1. 登录/登出 — token + 用户信息持久化到 localStorage
 *    2. 多公司视角 — 超管可在公司间切换，普通用户固定所属公司
 *    3. 权限校验 — 基于 permission codes 的按钮级权限
 *
 *  关键设计 — 多公司视角优先级：
 *    超管手动切换公司 → currentCompanyId
 *    超管查看全部     → currentCompanyId = null
 *    普通用户         → effectiveCompanyId = homeCompanyId（不可切换）
 *
 *  数据流向：
 *    login() → 后端返回 user （含 homeCompanyId, companyList）
 *            → 写入 Pinia state + localStorage
 *            → 请求拦截器从 localStorage 读取 currentCompanyId 附加到 params
 *
 *  为什么从 localStorage 读而非 Pinia？
 *    避免 request.js 与 store 的循环依赖（store 引用 request, request 引用 store）
 * =========================================================================
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginApi, setMyDefaultCompany as setDefaultCompanyApi, getCompanies as getCompaniesApi } from '../api/index'

export const useUserStore = defineStore('user', () => {
  // ---------------------------------------------------------------------------
  // 核心状态 — 从 localStorage 恢复（刷新页面不丢失）
  // ---------------------------------------------------------------------------
  const token = ref(localStorage.getItem('token') || '')
  const user = ref(JSON.parse(localStorage.getItem('user') || '{}'))
  const permissions = ref(JSON.parse(localStorage.getItem('permissions') || '[]'))

  // ========== 多公司扩展字段 ==========
  const homeCompanyId = ref(user.value.homeCompanyId || null)   // 用户所属公司（普通用户固定）
  const isSuperAdmin = ref(user.value.isSuperAdmin || false)       // 是否超级管理员
  const currentCompanyId = ref(null)                              // 当前切换的视角（超管专用）
  const companyList = ref(user.value.companyList || [])          // 用户可选的公司列表

  // 计算属性：是否在查看全部数据（超管专用，无 currentCompanyId）
  const isViewingAll = computed(() => isSuperAdmin.value && currentCompanyId.value === null)

  // 计算属性：当前生效的 companyId（用于 API 请求拦截器注入）
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

  // =========================================================================
  // login — 登录
  // 调用后端 /api/auth/login，保存 token + user + permissions
  // 写入 localStorage 持久化，刷新页面后自动恢复
  // =========================================================================
  async function login(credentials) {
    const res = await loginApi(credentials)
    token.value = res.token || res.accessToken
    user.value = res.user
    permissions.value = res.permissions || []

    // 解析多公司字段
    homeCompanyId.value = res.user?.homeCompanyId || null
    isSuperAdmin.value = res.user?.isSuperAdmin || false
    companyList.value = res.user?.companyList || []

    // 优先使用上次持久化的默认公司，否则超管默认查看全部
    currentCompanyId.value = res.user?.defaultCompanyId || null
    if (currentCompanyId.value) {
      localStorage.setItem('currentCompanyId', currentCompanyId.value)
    } else {
      localStorage.removeItem('currentCompanyId')
    }

    // 持久化到 localStorage
    localStorage.setItem('token', token.value)
    localStorage.setItem('user', JSON.stringify(user.value))
    localStorage.setItem('permissions', JSON.stringify(permissions.value))

    // 如果登录响应未返回公司列表，主动加载（用于公司切换下拉框）
    // 必须放在 localStorage 写入之后，避免被覆盖
    if (companyList.value.length === 0) await fetchCompanyList()

    return res
  }

  // =========================================================================
  // logout — 登出
  // 清除所有状态和 localStorage，回到登录页
  // =========================================================================
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

  // =========================================================================
  // hasPermission — 按钮级权限校验
  // 用于 v-permission 指令，或在组件中手动调用
  // @param {string} code  权限编码（如 "Contract.Create"）
  // @returns {boolean}
  // =========================================================================
  function hasPermission(code) {
    return permissions.value.includes(code)
  }

  // =========================================================================
  // 超级管理员视角切换
  // 写入 localStorage + 持久化到数据库（下次登录自动恢复）
  // =========================================================================
  /** 切换到指定公司 */
  async function switchToCompany(companyId) {
    currentCompanyId.value = companyId
    localStorage.setItem('currentCompanyId', companyId || '')
    try { await setDefaultCompanyApi(companyId) } catch (e) { /* 静默 */ }
  }

  /** 切换到"全部数据"（超管专用）*/
  async function switchToAll() {
    currentCompanyId.value = null
    localStorage.removeItem('currentCompanyId')
    try { await setDefaultCompanyApi(null) } catch (e) { /* 静默 */ }
  }

  // =========================================================================
  // fetchCompanyList — 主动加载公司列表
  // 兜底：登录响应未包含公司列表时调用（例如旧版本后端）
  // 加载后同步到 localStorage.user.companyList，避免刷新后丢失
  // =========================================================================
  async function fetchCompanyList() {
    try {
      const res = await getCompaniesApi({ pageSize: 100, isActive: true })
      const list = res.data || res.items || []
      if (list.length > 0) {
        companyList.value = list.map(c => ({ id: c.id, name: c.name }))
        // 同步到 localStorage 中的 user 对象
        const stored = JSON.parse(localStorage.getItem('user') || '{}')
        stored.companyList = companyList.value
        localStorage.setItem('user', JSON.stringify(stored))
      }
    } catch (e) { /* 静默 — 非关键路径，失败不影响主体流程 */ }
  }

  // =========================================================================
  // restoreView — 初始化时恢复视角
  // 应用启动时调用，从 localStorage 恢复上次的视角状态
  // 如果公司列表为空则先加载
  // =========================================================================
  async function restoreView() {
    if (companyList.value.length === 0) await fetchCompanyList()
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
    restoreView, fetchCompanyList
  }
})
