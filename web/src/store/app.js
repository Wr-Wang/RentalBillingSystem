/**
 * =========================================================================
 *  应用状态管理 (useAppStore)
 *
 *  功能：
 *    管理全局 UI 状态（侧边栏折叠、面包屑）
 * =========================================================================
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAppStore = defineStore('app', () => {
  /** 侧边栏是否折叠（小屏自动折叠，可手动切换）*/
  const sidebarCollapsed = ref(false)
  /** 面包屑导航路径 */
  const breadcrumb = ref([])

  /** 切换侧边栏折叠状态 */
  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  /** 设置面包屑路径 */
  function setBreadcrumb(path) {
    breadcrumb.value = path
  }

  return { sidebarCollapsed, breadcrumb, toggleSidebar, setBreadcrumb }
})
