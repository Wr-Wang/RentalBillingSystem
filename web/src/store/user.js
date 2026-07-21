/**
 * =========================================================================
 *  用户状态管理 (useUserStore)
 *
 *  功能：
 *    1. 登录/登出 — token 持久化到 localStorage，用户信息存 Pinia 内存
 *    2. 多公司视角 — 超管可在公司间切换，普通用户固定所属公司
 *    3. 权限校验 — 基于 permission codes 的按钮级权限
 *
 *  安全改进（2026-07-14）：
 *    - 移除 `user` / `permissions` / `isSuperAdmin` 在 localStorage 的持久化
 *    - 改为页面刷新时通过 loadUserProfile() 从后端重新获取
 *    - localStorage 仅存 token + userId（用于恢复登录态）
 *
 *  多公司视角优先级：
 *    超管手动切换公司 → currentCompanyId
 *    超管查看全部     → currentCompanyId = null
 *    普通用户         → effectiveCompanyId = companyId（不可切换）
 *
 *  数据流向：
 *    login() → 后端返回 user（含 companyId, companyList）
 *            → 写入 Pinia state + 仅 token/userId 写入 localStorage
 *            → 请求拦截器从 requestContext 读取 companyId
 *
 *  为什么不用 localStorage 存 user/permissions？
 *    localStorage 同源所有 JS 均可读写，存在被 XSS 篡改的风险。
 *    改为 Pinia 内存态 + 后端接口每次刷新时重新拉取。
 * =========================================================================
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginApi, setMyDefaultCompany as setDefaultCompanyApi, getCompanies as getCompaniesApi, getMyProfile } from '../api/index'
import { setEffectiveCompanyId, clearEffectiveCompanyId } from '../utils/requestContext'

export const useUserStore = defineStore('user', () => {
  // ---------------------------------------------------------------------------
  // 核心状态 — 仅 token 和 userId 从 localStorage 恢复
  // user / permissions / isSuperAdmin 等敏感信息仅存内存
  // ---------------------------------------------------------------------------
  const token = ref(localStorage.getItem('token') || '')
  const userId = ref(localStorage.getItem('userId') || '')

  /** 用户详情（仅内存，页面刷新后通过 loadUserProfile 重新获取）*/
  const user = ref({})
  /** 权限码数组（仅内存）*/
  const permissions = ref([])

  // ========== 多公司扩展字段 ==========
  const companyId = ref(null)       // 用户所属公司（普通用户固定）
  const isSuperAdmin = ref(false)       // 是否超级管理员
  const currentCompanyId = ref(null)    // 当前切换的视角（超管专用）
  const companyList = ref([])           // 用户可选的公司列表

  /** 用户资料是否已从后端加载（防止重复加载）*/
  const profileLoaded = ref(false)

  // 计算属性：是否在查看全部数据（超管专用，无 currentCompanyId）
  const isViewingAll = computed(() => isSuperAdmin.value && currentCompanyId.value === null)

  // 计算属性：当前生效的 companyId（用于 API 请求拦截器注入）
  const effectiveCompanyId = computed(() => {
    if (isSuperAdmin.value && currentCompanyId.value) {
      return currentCompanyId.value
    }
    return companyId.value
  })

  // 当前视角的公司名称
  const currentCompanyName = computed(() => {
    if (isViewingAll.value) return '全部数据'
    if (currentCompanyId.value) {
      const found = companyList.value.find(l => l.id === currentCompanyId.value)
      return found ? found.name : '未知公司'
    }
    if (companyId.value) {
      const found = companyList.value.find(l => l.id === companyId.value)
      return found ? found.name : '未知公司'
    }
    return '系统'
  })

  // =========================================================================
  // setUserData — 统一设置用户数据（login / loadUserProfile 共用）
  // @param {Object}  payload    后端返回的用户数据
  // @param {string}  tokenVal   登录令牌
  // =========================================================================
  function setUserData(payload, tokenVal) {
    const userData = payload.user || payload
    token.value = tokenVal || token.value
    // 合并 roles（登录响应 roles 在顶层不在 user 内部）
    user.value = { ...userData, roles: userData.roles || payload.roles || [] }
    permissions.value = payload.permissions || []

    companyId.value = userData.companyId || null
    isSuperAdmin.value = userData.isSuperAdmin || false
    companyList.value = userData.companyList || []

    // 恢复或设置默认公司视角
    const savedCompanyId = localStorage.getItem('currentCompanyId')
    if (savedCompanyId && isSuperAdmin.value) {
      currentCompanyId.value = savedCompanyId
    } else {
      currentCompanyId.value = userData.defaultCompanyId || null
    }

    // 同步有效 companyId 到请求上下文（供请求拦截器使用）
    syncEffectiveCompanyId()

    profileLoaded.value = true
  }

  // =========================================================================
  // syncEffectiveCompanyId — 同步有效 companyId 到 requestContext
  // 计算 effectiveCompanyId 并写入 requestContext（请求拦截器读取）
  // =========================================================================
  function syncEffectiveCompanyId() {
    const eid = effectiveCompanyId.value
    setEffectiveCompanyId(eid)
  }

  // =========================================================================
  // login — 登录
  // 写入 localStorage 仅存 token + userId，敏感信息存 Pinia 内存
  // =========================================================================
  async function login(credentials) {
    const res = await loginApi(credentials)
    const tokenVal = res.token || res.accessToken

    // ---- 保存到 Pinia 内存 ----
    setUserData(res, tokenVal)

    // ---- 持久化到 localStorage（仅 token + userId）----
    localStorage.setItem('token', tokenVal)
    localStorage.setItem('userId', (res.user?.id || '').toString())

    // 如果登录响应未返回公司列表，主动加载
    if (companyList.value.length === 0) await fetchCompanyList()

    return res
  }

  // =========================================================================
  // loadUserProfile — 页面刷新后加载用户数据
  // 当 localStorage 中存在 token 时，应用初始化时调用
  // 从后端重新获取用户信息，只存 Pinia 内存，不写 localStorage
  // =========================================================================
  async function loadUserProfile() {
    if (!token.value || profileLoaded.value) return
    try {
      const res = await getMyProfile()
      setUserData(res, token.value)
      // 如果公司列表为空，加载它
      if (companyList.value.length === 0) await fetchCompanyList()
    } catch (e) {
      // 接口失败（如 token 过期），清除登录态
      logout()
      throw e
    }
  }

  // =========================================================================
  // logout — 登出
  // 清除所有状态：Pinia 内存 + localStorage + requestContext
  // =========================================================================
  function logout() {
    token.value = ''
    userId.value = ''
    user.value = {}
    permissions.value = []
    companyId.value = null
    isSuperAdmin.value = false
    currentCompanyId.value = null
    companyList.value = []
    profileLoaded.value = false

    localStorage.removeItem('token')
    localStorage.removeItem('userId')
    localStorage.removeItem('currentCompanyId')

    clearEffectiveCompanyId()
  }

  // =========================================================================
  // hasPermission — 按钮级权限校验
  // 用于 v-permission 指令，或在组件中手动调用
  // =========================================================================
  function hasPermission(code) {
    return permissions.value.includes(code)
  }

  // =========================================================================
  // 超级管理员视角切换
  // currentCompanyId 持久化到 localStorage（下次登录恢复视角）
  // 同时同步到 requestContext 供请求拦截器使用
  // =========================================================================
  /** 切换到指定公司 */
  async function switchToCompany(targetId) {
    currentCompanyId.value = targetId
    localStorage.setItem('currentCompanyId', targetId || '')
    syncEffectiveCompanyId()
    try { await setDefaultCompanyApi(targetId) } catch (e) { /* 静默 */ }
  }

  /** 切换到"全部数据"（超管专用）*/
  async function switchToAll() {
    currentCompanyId.value = null
    localStorage.removeItem('currentCompanyId')
    syncEffectiveCompanyId()
    try { await setDefaultCompanyApi(null) } catch (e) { /* 静默 */ }
  }

  // =========================================================================
  // fetchCompanyList — 主动加载公司列表
  // =========================================================================
  async function fetchCompanyList() {
    try {
      const res = await getCompaniesApi({ pageSize: 100, isActive: true })
      const list = res.data || res.items || []
      if (list.length > 0) {
        companyList.value = list.map(c => ({ id: c.id, name: c.name }))
      }
    } catch (e) { /* 静默 */ }
  }

  // =========================================================================
  // restoreView — 初始化时恢复视角
  // 仅从 localStorage 恢复 currentCompanyId（纯 UI 偏好，非权限数据）
  // =========================================================================
  async function restoreView() {
    if (!profileLoaded.value) return
    if (companyList.value.length === 0) await fetchCompanyList()
    const saved = localStorage.getItem('currentCompanyId')
    if (saved && saved.length > 0 && isSuperAdmin.value) {
      currentCompanyId.value = saved
      syncEffectiveCompanyId()
    }
  }

  return {
    token, userId, user, permissions,
    companyId, isSuperAdmin,
    currentCompanyId, companyList,
    isViewingAll, effectiveCompanyId, currentCompanyName,
    profileLoaded,
    login, logout, hasPermission, loadUserProfile,
    switchToCompany, switchToAll,
    restoreView, fetchCompanyList
  }
})
