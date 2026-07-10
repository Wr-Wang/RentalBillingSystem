<template>
  <div class="app-layout">
    <header class="app-header">
      <div class="logo">
        <el-icon :size="28"><HomeFilled /></el-icon>
        <span>房屋租赁收租结算系统</span>
      </div>
      <div class="header-right">
        <!-- 超级管理员：公司视角切换器 -->
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

        <!-- 普通用户：显示当前所属公司 -->
        <span v-else-if="userStore.homeCompanyId" class="company-tag">
          <el-tag size="small" type="info">{{ userStore.currentCompanyName }}</el-tag>
        </span>

        <!-- 通知铃铛 -->
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

const userStore = useUserStore()
const appStore = useAppStore()
const menuStore = useMenuStore()
const notifStore = useNotificationStore()
const route = useRoute()

const showProfile = ref(false)
const showChangePassword = ref(false)

const profileForm = ref({
  username: userStore.user.username || '',
  displayName: userStore.user.displayName || '',
  phone: userStore.user.phone || '',
  email: userStore.user.email || ''
})

const passwordForm = ref({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const activeMenu = computed(() => {
  return route.path
})

// 公司切换器数据
const companyOptions = computed(() => {
  return userStore.companyList || []
})

async function handleCompanySwitch(command) {
  if (command === 'all') {
    await userStore.switchToAll()
  } else {
    await userStore.switchToCompany(command)
  }
  // 强制刷新当前页面数据（通过重新加载路由）
  const currentPath = route.path
  router.push('/redirect' + currentPath).then(() => {
    router.replace(currentPath)
  })
}

function goToNotifications() {
  router.push('/notifications')
}

onMounted(() => {
  menuStore.initFromRoutes(router.options.routes.find(r => r.path === '/')?.children || [])
  // 恢复上次的视角状态（超管切换公司）
  userStore.restoreView()
  // 启动通知轮询
  notifStore.fetchUnreadCounts()
  notifStore.startPolling()
})

onUnmounted(() => {
  notifStore.stopPolling()
})

function handleLogout() {
  ElMessageBox.confirm('确定要退出登录吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(() => {
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
