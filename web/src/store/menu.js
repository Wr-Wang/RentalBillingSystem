import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useMenuStore = defineStore('menu', () => {
  const sidebarMenus = ref([])
  const currentRoutes = ref([])

  function initFromRoutes(routes) {
    currentRoutes.value = routes
    const userRole = JSON.parse(localStorage.getItem('user') || '{}').role || 'Admin'
    const builtMenus = buildMenusFromRoutes(routes, userRole)
    sidebarMenus.value = builtMenus
  }

  function buildMenusFromRoutes(routes, role) {
    const result = []
    for (const route of routes) {
      if (route.hidden) continue
      const meta = route.meta || {}
      if (meta.roles && meta.roles.length > 0 && !meta.roles.includes(role)) continue
      if (meta.hidden) continue
      // Use meta.title as name; skip pure redirects without a name
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
          if (childMeta.roles && childMeta.roles.length > 0 && !childMeta.roles.includes(role)) continue
          if (!childMeta.title) continue
          // Build the full child path: /parentPath/childPath
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
    sidebarMenus.value = buildMenusFromRoutes(currentRoutes.value, role)
  }

  return { sidebarMenus, currentRoutes, initFromRoutes, refreshByRole }
})
