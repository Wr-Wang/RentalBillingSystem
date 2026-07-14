/**
 * =========================================================================
 *  菜单状态管理 (useMenuStore)
 *
 *  功能：
 *    从路由配置动态生成侧边栏菜单，基于角色过滤
 *
 *  执行时机：
 *    MainLayout.vue 的 onMounted 中调用 initFromRoutes(router.options.routes)
 *    用户角色变更时调用 refreshByRole(role)
 *
 *  过滤规则（按优先级）：
 *    1. route.hidden === true → 跳过（如详情页、创建页）
 *    2. meta.scope === 'System' 且非超管 → 跳过（系统设置仅超管可见）
 *    3. meta.roles 存在且不包含当前用户角色 → 跳过
 *    4. meta.title 不存在且无子路由 → 跳过（占位路由）
 * =========================================================================
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useUserStore } from './user'

export const useMenuStore = defineStore('menu', () => {
  // ---------------------------------------------------------------------------
  // 状态
  // ---------------------------------------------------------------------------
  /** 侧边栏菜单树（渲染用） */
  const sidebarMenus = ref([])
  /** 路由配置快照（初始化时保存，用于角色切换后重新计算） */
  const currentRoutes = ref([])

  // =========================================================================
  // initFromRoutes — 应用启动时初始化菜单
  // 传入 router.options.routes（全部路由配置）
  // 根据当前用户的角色过滤出可见菜单
  // @param {Array} routes  Vue Router 配置数组
  // =========================================================================
  function initFromRoutes(routes) {
    currentRoutes.value = routes
    const userStore = useUserStore()
    const builtMenus = buildMenusFromRoutes(routes, userStore)
    sidebarMenus.value = builtMenus
  }

  // =========================================================================
  // buildMenusFromRoutes — 核心过滤逻辑
  // 递归遍历路由树，按角色/权限/scope 过滤
  //
  // 角色判断优先级：
  //   user.roles[0].code  → 数组第一个角色的编码
  //   user.role           → 旧版单角色字段
  //   'Admin'             → 兜底默认值
  //
  // @param {Array}  routes    路由配置
  // @param {Object} userStore 用户 store（读 isSuperAdmin + user.roles）
  // @returns {Array} 过滤后的菜单树
  // =========================================================================
  function buildMenusFromRoutes(routes, userStore) {
    const isSuperAdmin = userStore.isSuperAdmin
    const userRole = userStore.user?.roles?.[0]?.code || userStore.user?.role || 'Admin'
    const result = []

    for (const route of routes) {
      // ----- 顶层路由过滤 -----
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

      // ----- 子路由过滤（用于嵌套布局如会计管理 / 系统设置）-----
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

  // =========================================================================
  // refreshByRole — 角色切换后刷新菜单
  // 不重新传入路由配置，复用初始化时的快照 currentRoutes
  // 在审批通过/角色分配等场景触发
  // =========================================================================
  function refreshByRole(role) {
    const userStore = useUserStore()
    sidebarMenus.value = buildMenusFromRoutes(currentRoutes.value, userStore)
  }

  return { sidebarMenus, currentRoutes, initFromRoutes, refreshByRole }
})
