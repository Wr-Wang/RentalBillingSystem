import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import { useUserStore } from './user'
import { getMenus } from '../api/index'

export const useMenuStore = defineStore('menu', () => {
  const sidebarMenus = ref([])
  /** 缓存路由配置，供后续 rebuildMenus 使用 */
  let cachedRoutes = []
  /**
   * path → permissionCode 映射表
   * 从后端菜单树构建，用于优先按权限码校验菜单可见性
   * 仅在 initFromRoutes 时加载一次，后续复用
   */
  let permCodeMap = {}

  /** 从后端加载所有菜单，构建 path → permissionCode 映射 */
  async function fetchPermCodeMap() {
    try {
      const menuTree = await getMenus()
      const map = {}
      flattenMenuPermCodes(Array.isArray(menuTree) ? menuTree : [], map)
      permCodeMap = map
    } catch {
      // API 失败时降级为空映射，后续走角色回退逻辑
      permCodeMap = {}
    }
  }

  /** 递归遍历菜单树，构建 { path: permissionCode } 映射（子覆盖父）*/
  function flattenMenuPermCodes(menus, map) {
    for (const m of menus) {
      if (m.path && m.permissionCode) {
        const normalized = normalizePath(m.path)
        map[normalized] = m.permissionCode
      }
      if (m.children && m.children.length > 0) {
        flattenMenuPermCodes(m.children, map)
      }
    }
  }

  /** 规范化路径：确保以 / 开头，去掉尾部 / */
  function normalizePath(p) {
    let s = p.startsWith('/') ? p : '/' + p
    if (s.length > 1 && s.endsWith('/')) s = s.slice(0, -1)
    return s
  }

  async function initFromRoutes(routes) {
    cachedRoutes = routes || []
    // 加载后端权限码映射（优先用权限码校验菜单可见性）
    await fetchPermCodeMap()
    rebuildMenus()

    // 监听用户资料加载完成后重新构建（确保角色数据就绪）
    const userStore2 = useUserStore()
    let unwatch2
    unwatch2 = watch(() => userStore2.profileLoaded, (loaded) => {
      if (loaded && unwatch2) {
        rebuildMenus()
        unwatch2()
      }
    }, { immediate: true })
  }

  function rebuildMenus() {
    const userStore = useUserStore()
    if (!cachedRoutes || cachedRoutes.length === 0) return
    const builtMenus = buildMenusFromRoutes(cachedRoutes, userStore)
    sidebarMenus.value = builtMenus
  }

  /**
   * 判断菜单对当前用户是否可见
   *
   * 双校验策略（按优先级）：
   *  1. 超管 → 全部可见
   *  2. 后端菜单存在 permissionCode → 以权限码为准（hasPermission）
   *  3. 无 permissionCode → 回退到角色名校验（meta.roles）
   */
  function isMenuVisible(meta, fullPath, userStore, userRoles) {
    if (userStore.isSuperAdmin) return true
    if (meta.scope === 'System') return false
    if (meta.hidden) return false

    const permCode = permCodeMap[fullPath]
    if (permCode !== undefined) {
      // 后端菜单有权限码 → 以此为准
      return userStore.hasPermission(permCode)
    }

    // 无权限码 → 回退到角色校验
    if (meta.roles && meta.roles.length > 0) {
      return meta.roles.some(r => userRoles.includes(r))
    }
    // 无 roles 限制 → 公共菜单
    return true
  }

  function buildMenusFromRoutes(routes, userStore) {
    const isSuperAdmin = userStore.isSuperAdmin
    // 支持多角色：取所有角色编码的数组，任一角色匹配即可见
    const userRoles = (userStore.user?.roles || []).map(r => r.code).filter(Boolean)
    const result = []

    for (const route of routes) {
      if (route.hidden) continue
      const meta = route.meta || {}
      if (meta.scope === 'System' && !isSuperAdmin) continue
      if (meta.hidden) continue

      const fullPath = normalizePath(route.path || '')
      const hasChildren = route.children && route.children.length > 0

      // ----- 先处理子路由（父路由需要根据子路由可见性判断是否显示）-----
      let visibleChildren = []
      if (hasChildren) {
        const childRoutes = route.children.filter(c => !(c.meta && c.meta.hidden))
        for (const child of childRoutes) {
          const childMeta = child.meta || {}
          if (childMeta.scope === 'System' && !isSuperAdmin) continue

          const childFullPath = normalizePath((route.path || '') + '/' + (child.path || ''))
          if (!isMenuVisible(childMeta, childFullPath, userStore, userRoles)) continue
          if (!childMeta.title) continue

          visibleChildren.push({
            id: child.path || child.name,
            parentId: fullPath,
            name: childMeta.title || '',
            path: child.path || '',
            fullPath: childFullPath,
            icon: childMeta.icon || '',
            sortOrder: childMeta.sortOrder || 0
          })
        }
      }

      // ----- 判断父路由是否可见 -----
      // 可见条件：自身有权限（权限码/角色） OR （有子路由且至少一个子路由可见）
      const parentVisible = isMenuVisible(meta, fullPath, userStore, userRoles)
      if (!parentVisible && visibleChildren.length === 0) continue
      if (!meta.title && visibleChildren.length === 0) continue

      const item = {
        id: route.path || route.name,
        parentId: null,
        name: meta.title || route.name || '',
        path: route.path || '',
        fullPath: fullPath,
        icon: meta.icon || '',
        sortOrder: meta.sortOrder || 0,
        children: visibleChildren
      }

      if (item.name || item.children.length > 0) {
        result.push(item)
      }
    }
    return result
  }

  function refreshByRole(role) {
    rebuildMenus()
  }

  return { sidebarMenus, initFromRoutes, refreshByRole, rebuildMenus }
})
