<!--
  =========================================================================
  MainLayout — 主框架布局

  结构（从上到下，从左到右）：
    ┌──────────────────────────────────────────────────┐
    │  头部: logo / 公司切换 / 通知铃铛 / 用户菜单     │
    ├──────────┬───────────────────────────────────────┤
    │ 侧边栏   │  主内容区                             │
    │ 功能菜单 │  <router-view /> (页面组件)            │
    │ 折叠按钮 │                                       │
    └──────────┴───────────────────────────────────────┘

  公司视角切换：
    - 超管显示下拉选择器：全部数据 / 具体公司
    - 普通用户显示固定公司标签（不可切换）

  通知轮询：
    onMounted → notificationStore.startPolling(60000)
    onUnmounted → notificationStore.stopPolling()
    每 60 秒刷新一次未读计数

  侧边栏菜单：
    由 menuStore.initFromRoutes(router.options.routes) 在 onMounted 中初始化
    根据用户角色动态过滤
  =========================================================================
-->
<template>
  <div class="app-layout">
    <header class="app-header">
      <!-- 左上角：Logo + 系统名称 -->
      <div class="logo">
        <el-icon :size="28"><HomeFilled /></el-icon>
        <span>房屋租赁收租结算系统</span>
      </div>

      <!-- 右上角功能区 -->
      <div class="header-right">
        <!-- ★ 多公司视角切换（仅超管可见） -->
        <el-dropdown v-if="userStore.isSuperAdmin" @command="handleCompanySwitch">
          <span class="company-switcher">
            <el-icon><OfficeBuilding /></el-icon>
            <span>{{ userStore.currentCompanyName }}</span>
            <el-icon><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="all">
                <el-icon><DataAnalysis /></el-icon>全部数据
              </el-dropdown-item>
              <el-dropdown-item v-for="l in companyOptions" :key="l.id" :command="l.id" divided>
                <el-icon><HomeFilled /></el-icon>{{ l.name }}
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>

        <!-- 普通用户：只显示公司名称，不可切换 -->
        <span v-else-if="userStore.companyId" class="company-tag">
          <el-tag size="small" type="info">{{ userStore.currentCompanyName }}</el-tag>
        </span>

        <!-- 通知铃铛（显示未读总数） -->
        <el-badge :value="notifStore.unreadCounts.Total" :max="99" :hidden="notifStore.unreadCounts.Total === 0">
          <el-button circle size="small" @click="goToNotifications" style="border: none; color: #fff; background: transparent;">
            <el-icon :size="20"><Bell /></el-icon>
          </el-button>
        </el-badge>

        <el-dropdown trigger="click">
          <span class="user-info">
            <el-avatar :size="32" icon="UserFilled" />
            <span class="username">{{ userStore.user.displayName || '管理员' }}</span>
            <el-icon><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="showProfile = true">
                <el-icon><User /></el-icon>我的资料
              </el-dropdown-item>
              <el-dropdown-item @click="showChangePassword = true">
                <el-icon><Lock /></el-icon>修改密码
              </el-dropdown-item>
              <el-dropdown-item divided @click="handleLogout">
                <el-icon><SwitchButton /></el-icon>退出登录
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </header>
    <div class="app-main">
      <aside class="app-sidebar" :class="{ collapsed: appStore.sidebarCollapsed }">
        <div class="sidebar-header">
          <span v-show="!appStore.sidebarCollapsed" class="sidebar-title">功能菜单</span>
          <el-button
            class="collapse-btn"
            :icon="appStore.sidebarCollapsed ? Expand : Fold"
            text
            size="small"
            @click="appStore.toggleSidebar()"
          />
        </div>
        <el-menu
          :default-active="activeMenu"
          :collapse="appStore.sidebarCollapsed"
          background-color="#001529"
          text-color="rgba(255,255,255,0.65)"
          active-text-color="#fff"
          router
        >
          <template v-for="menu in menuStore.sidebarMenus" :key="menu.id">
            <el-sub-menu v-if="menu.children && menu.children.length > 0" :index="menu.id">
              <template #title>
                <el-icon v-if="menu.icon"><component :is="menu.icon" /></el-icon>
                <span>{{ menu.name }}</span>
              </template>
              <el-menu-item v-for="child in menu.children" :key="child.id" :index="child.fullPath">
                <el-icon v-if="child.icon"><component :is="child.icon" /></el-icon>
                <span>{{ child.name }}</span>
              </el-menu-item>
            </el-sub-menu>
            <el-menu-item v-else :index="menu.fullPath">
              <el-icon v-if="menu.icon"><component :is="menu.icon" /></el-icon>
              <span>{{ menu.name }}</span>
            </el-menu-item>
          </template>
        </el-menu>
      </aside>
      <main class="app-content">
        <div class="page-container">
          <router-view />
        </div>
      </main>
    </div>

    <!-- My Profile Dialog -->
    <el-dialog v-model="showProfile" title="我的资料" width="500px">
      <el-form :model="profileForm" label-width="100px">
        <el-form-item label="用户名">
          <el-input v-model="profileForm.username" disabled />
        </el-form-item>
        <el-form-item label="姓名">
          <el-input v-model="profileForm.displayName" />
        </el-form-item>
        <el-form-item label="手机号">
          <el-input v-model="profileForm.phone" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="profileForm.email" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showProfile = false">取消</el-button>
        <el-button type="primary" @click="saveProfile">保存</el-button>
      </template>
    </el-dialog>

    <!-- Change Password Dialog -->
    <el-dialog v-model="showChangePassword" title="修改密码" width="400px">
      <el-form :model="passwordForm" label-width="100px">
        <el-form-item label="旧密码">
          <el-input v-model="passwordForm.oldPassword" type="password" show-password />
        </el-form-item>
        <el-form-item label="新密码">
          <el-input v-model="passwordForm.newPassword" type="password" show-password />
        </el-form-item>
        <el-form-item label="确认密码">
          <el-input v-model="passwordForm.confirmPassword" type="password" show-password />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showChangePassword = false">取消</el-button>
        <el-button type="primary" @click="changePassword">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from '../store/user'
import { useAppStore } from '../store/app'
import { useMenuStore } from '../store/menu'
import { useNotificationStore } from '../store/notification'
import router from '../router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Bell, Fold, Expand } from '@element-plus/icons-vue'
import { changePassword as apiChangePassword } from '@/api'
import { broadcast, onMessage } from '../utils/broadcast'

// ---------------------------------------------------------------------------
// Stores
// ---------------------------------------------------------------------------
const userStore = useUserStore()
const appStore = useAppStore()
const menuStore = useMenuStore()
const notifStore = useNotificationStore()
const route = useRoute()

/** 跨标签页广播监听取消函数 */
let unlistenBroadcast = null

// ---------------------------------------------------------------------------
// Dialog 状态
// ---------------------------------------------------------------------------
/** 我的资料弹窗 */
const showProfile = ref(false)
/** 修改密码弹窗 */
const showChangePassword = ref(false)

/** 个人资料表单 */
const profileForm = ref({
  username: userStore.user.username || '',
  displayName: userStore.user.displayName || '',
  phone: userStore.user.phone || '',
  email: userStore.user.email || ''
})

/** 修改密码表单 */
const passwordForm = ref({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

// ---------------------------------------------------------------------------
// 计算属性
// ---------------------------------------------------------------------------
/** 当前激活的菜单项（由当前路由路径决定）*/
const activeMenu = computed(() => route.path)

/** 公司切换下拉框选项列表 */
const companyOptions = computed(() => userStore.companyList || [])

// ---------------------------------------------------------------------------
// 公司视角切换
// ---------------------------------------------------------------------------
/**
 * 处理公司切换
 * "all" → 查看全部数据
 * 其他 → 切换到指定公司
 * 切换后通过重定向触发页面刷新
 */
async function handleCompanySwitch(command) {
  if (command === 'all') {
    await userStore.switchToAll()
  } else {
    await userStore.switchToCompany(command)
  }
  const currentPath = route.path
  router.push('/redirect' + currentPath).then(() => {
    router.replace(currentPath)
  })
}

function goToNotifications() {
  router.push('/notifications')
}

onMounted(async () => {
  // -----------------------------------------------------------------------
  // 1. 加载用户资料（页面刷新后从后端重新获取）
  //    仅当 localStorage 有 token 时才会真正请求
  //    加载失败（token 过期）会触发 logout 并重定向到 /login
  // -----------------------------------------------------------------------
  try {
    await userStore.loadUserProfile()
  } catch {
    // token 过期或无效 → 已跳转登录页
    return
  }

  // -----------------------------------------------------------------------
  // 2. 检查当前路由的 scope 权限（非超管访问 System 路由 → 重定向）
  //    路由守卫中不再做此检查（因无法从 localStorage 拿 isSuperAdmin），
  //    改为等用户资料加载完成后在这里统一处理
  // -----------------------------------------------------------------------
  const currentRoute = route.meta
  if (currentRoute?.scope === 'System' && !userStore.isSuperAdmin) {
    router.push('/dashboard')
    return
  }

  // -----------------------------------------------------------------------
  // 3. 初始化侧边栏菜单
  //     initFromRoutes 内部会监听 userStore.profileLoaded，
  //     在用户资料加载完成后自动重新构建菜单
  // -----------------------------------------------------------------------
  menuStore.initFromRoutes(router.options.routes.find(r => r.path === '/')?.children || [])

  // 恢复上次的视角状态（超管切换公司）
  userStore.restoreView()

  // 启动通知轮询（5 分钟兜底，BroadcastChannel 负责实时更新）
  // startPolling 内部会立即执行一次 fetchUnreadCounts，无需单独调用
  notifStore.startPolling(300000)

  // -----------------------------------------------------------------------
  // 4. 启动跨标签页广播监听
  //    其他标签页提交审批后即时刷新本页面的通知计数
  //    其他标签页登出后即时跳转登录页
  // -----------------------------------------------------------------------
  unlistenBroadcast = notifStore.startBroadcastListener()

  // 监听其他标签页的登出信号
  const unlistenLogout = onMessage('LOGOUT', () => {
    notifStore.stopPolling()
    userStore.logout()
    router.push('/login')
  })
  // 合并清理函数
  const origUnlisten = unlistenBroadcast
  unlistenBroadcast = () => {
    origUnlisten()
    unlistenLogout()
  }
})

onUnmounted(() => {
  notifStore.stopPolling()
  if (unlistenBroadcast) unlistenBroadcast()
})

function handleLogout() {
  ElMessageBox.confirm('确定要退出登录吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(() => {
    broadcast('LOGOUT')   // 先通知其他标签页，再清本页状态
    userStore.logout()
    router.push('/login')
  }).catch(() => {})
}

function saveProfile() {
  ElMessage.success('资料修改成功')
  showProfile.value = false
}

async function changePassword() {
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    ElMessage.error('两次密码不一致')
    return
  }
  try {
    await apiChangePassword({
      oldPassword: passwordForm.value.oldPassword,
      newPassword: passwordForm.value.newPassword
    })
    ElMessage.success('密码修改成功')
    showChangePassword.value = false
    passwordForm.value = { oldPassword: '', newPassword: '', confirmPassword: '' }
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '密码修改失败')
  }
}
</script>

<style scoped>
.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  height: 48px;
  padding: 0 8px;
  border-bottom: 1px solid rgba(255,255,255,0.08);
  transition: height 0.25s ease, padding 0.25s ease;
}
.sidebar-title {
  flex: 1;
  font-size: 12px;
  color: rgba(255,255,255,0.45);
  letter-spacing: 1px;
  padding-left: 8px;
  overflow: hidden;
  white-space: nowrap;
}
.collapse-btn {
  flex-shrink: 0;
  color: rgba(255,255,255,0.65) !important;
  font-size: 18px;
}
.collapse-btn:hover {
  color: #fff !important;
  background: rgba(255,255,255,0.08) !important;
}
.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}
.company-switcher {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  color: #fff;
  padding: 4px 12px;
  border: 1px solid rgba(255,255,255,0.2);
  border-radius: 4px;
  font-size: 13px;
  transition: border-color 0.2s;
}
.company-switcher:hover {
  border-color: rgba(255,255,255,0.5);
}
.company-tag {
  display: flex;
  align-items: center;
}
.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  color: #fff;
}
.username {
  font-size: 14px;
}
</style>
