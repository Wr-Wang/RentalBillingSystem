import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import { useUserStore } from './user'

export const useMenuStore = defineStore('menu', () => {
  const sidebarMenus = ref([])
  /** 缓存路由配置，供后续 rebuildMenus 使用 */
  let cachedRoutes = []

  function initFromRoutes(routes) {
    cachedRoutes = routes || []
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

  function buildMenusFromRoutes(routes, userStore) {
    const isSuperAdmin = userStore.isSuperAdmin
    // 支持多角色：取所有角色编码的数组，任一角色匹配即可见
    const userRoles = (userStore.user?.roles || []).map(r => r.code).filter(Boolean)
    // 兼容旧字段 + 默认兜底
    if (userRoles.length === 0) {
      const fallback = userStore.user?.role || 'Admin'
      userRoles.push(fallback)
    }
    const result = []

    for (const route of routes) {
      if (route.hidden) continue
      const meta = route.meta || {}
      if (meta.scope === 'System' && !isSuperAdmin) continue
      // 超管无视角色过滤，所有菜单可见
      // 多角色用户：只要有一个角色匹配 routes 即可见
      if (!isSuperAdmin && meta.roles && meta.roles.length > 0 && !meta.roles.some(r => userRoles.includes(r))) continue
      if (meta.hidden) continue
      if (!meta.title && (!route.children || route.children.length === 0)) continue

      const item = {
        id: route.path || route.name,
        parentId: null,
        name: meta.title || route.name || '',
        path: route.path || '',
        fullPath: '/' + (route.path || ''),
        icon: meta.icon || '',
        sortOrder: meta.sortOrder || 0,
        children: []
      }

      if (route.children && route.children.length > 0) {
        const childRoutes = route.children.filter(c => !(c.meta && c.meta.hidden))
        for (const child of childRoutes) {
          const childMeta = child.meta || {}
          if (childMeta.scope === 'System' && !isSuperAdmin) continue
          if (!isSuperAdmin && childMeta.roles && childMeta.roles.length > 0 && !childMeta.roles.some(r => userRoles.includes(r))) continue
          if (!childMeta.title) continue
          const childFullPath = '/' + (route.path || '') + '/' + (child.path || '')
          item.children.push({
            id: child.path || child.name,
            parentId: item.id,
            name: childMeta.title || '',
            path: child.path || '',
            fullPath: childFullPath,
            icon: childMeta.icon || '',
            sortOrder: childMeta.sortOrder || 0
          })
        }
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
