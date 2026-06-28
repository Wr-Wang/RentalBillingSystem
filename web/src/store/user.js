import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginApi } from '../api/index'

export const useUserStore = defineStore('user', () => {
  const token = ref(localStorage.getItem('token') || '')
  const user = ref(JSON.parse(localStorage.getItem('user') || '{}'))
  const permissions = ref(JSON.parse(localStorage.getItem('permissions') || '[]'))

  // ========== 多房东扩展字段 ==========
  const homeLandlordId = ref(user.value.homeLandlordId || null)   // 所属房东
  const isSuperAdmin = ref(user.value.isSuperAdmin || false)       // 是否超级管理员
  const landlordScope = ref(user.value.landlordScope || [])       // 可查看的房东ID列表
  const currentLandlordId = ref(null)                              // 当前切换的视角（超管专用）
  const landlordList = ref(user.value.landlordList || [])          // 用户可选的房东列表

  // 计算属性：是否在查看全部数据（超管专用）
  const isViewingAll = computed(() => isSuperAdmin.value && currentLandlordId.value === null)

  // 计算属性：当前生效的 landlordId（用于 API 请求）
  const effectiveLandlordId = computed(() => {
    if (isSuperAdmin.value && currentLandlordId.value) {
      return currentLandlordId.value
    }
    return homeLandlordId.value
  })

  // 当前视角的房东名称
  const currentLandlordName = computed(() => {
    if (isViewingAll.value) return '全部数据'
    if (currentLandlordId.value) {
      const found = landlordList.value.find(l => l.id === currentLandlordId.value)
      return found ? found.name : '未知房东'
    }
    if (homeLandlordId.value) {
      const found = landlordList.value.find(l => l.id === homeLandlordId.value)
      return found ? found.name : '未知房东'
    }
    return '系统'
  })

  async function login(credentials) {
    const res = await loginApi(credentials)
    token.value = res.token || res.accessToken
    user.value = res.user
    permissions.value = res.permissions || []

    // 解析多房东字段
    homeLandlordId.value = res.user?.homeLandlordId || null
    isSuperAdmin.value = res.user?.isSuperAdmin || false
    landlordScope.value = res.user?.landlordScope || []
    landlordList.value = res.user?.landlordList || []
    currentLandlordId.value = null  // 登录后默认查看全部数据（超管）或自己的数据

    localStorage.setItem('token', token.value)
    localStorage.setItem('user', JSON.stringify(user.value))
    localStorage.setItem('permissions', JSON.stringify(permissions.value))

    // 如果是超管，恢复上次的视角状态
    if (isSuperAdmin.value) {
      restoreView()
    }

    return res
  }

  function logout() {
    token.value = ''
    user.value = {}
    permissions.value = []
    homeLandlordId.value = null
    isSuperAdmin.value = false
    landlordScope.value = []
    currentLandlordId.value = null
    landlordList.value = []

    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('permissions')
    localStorage.removeItem('currentLandlordId')
  }

  function hasPermission(code) {
    return permissions.value.includes(code)
  }

  // ========== 超级管理员视角切换 ==========
  function switchToLandlord(landlordId) {
    currentLandlordId.value = landlordId
    localStorage.setItem('currentLandlordId', landlordId || '')
  }

  function switchToAll() {
    currentLandlordId.value = null
    localStorage.removeItem('currentLandlordId')
  }

  // 初始化时从 localStorage 恢复视角状态
  function restoreView() {
    const saved = localStorage.getItem('currentLandlordId')
    if (saved && saved.length > 0 && isSuperAdmin.value) {
      currentLandlordId.value = saved
    }
  }

  // 判断是否需要显示房东筛选器（跨房东用户可见）
  const showLandlordFilter = computed(() => {
    if (isSuperAdmin.value) return true
    return landlordScope.value.length > 1
  })

  return {
    token, user, permissions,
    homeLandlordId, isSuperAdmin, landlordScope,
    currentLandlordId, landlordList,
    isViewingAll, effectiveLandlordId, currentLandlordName,
    showLandlordFilter,
    login, logout, hasPermission,
    switchToLandlord, switchToAll,
    restoreView
  }
})
