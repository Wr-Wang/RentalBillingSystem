import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useUserStore } from './user'

export const useMenuStore = defineStore('menu', () => {
  const sidebarMenus = ref([])
  const currentRoutes = ref([])

  function initFromRoutes(routes) {
    currentRoutes.value = routes
    const userStore = useUserStore()
    const builtMenus = buildMenusFromRoutes(routes, userStore)
    sidebarMenus.value = builtMenus
  }

  function buildMenusFromRoutes(routes, userStore) {
    const isSuperAdmin = userStore.isSuperAdmin
    // 获取用户有效角色：优先取 roles 数组第一个，无则取 role 字段，都无则默认 Admin
    const userRole = userStore.user?.roles?.[0]?.code || userStore.user?.role || 'Admin'
    const result = []
    for (const route of routes) {
      if (route.hidden) continue
      const meta = route.meta || {}
      if (meta.scope === 'System' && !isSuperAdmin) continue
      if (meta.roles && meta.roles.length > 0 && !meta.roles.includes(userRole)) continue
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
          if (childMeta.roles && childMeta.roles.length > 0 && !childMeta.roles.includes(userRole)) continue
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
    const userStore = useUserStore()
    sidebarMenus.value = buildMenusFromRoutes(currentRoutes.value, userStore)
  }

  return { sidebarMenus, currentRoutes, initFromRoutes, refreshByRole }
})
