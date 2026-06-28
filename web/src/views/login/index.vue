<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <el-icon :size="48" color="#409eff"><HomeFilled /></el-icon>
        <h2>房屋租赁收租结算系统</h2>
        <p class="subtitle">专业住宅租赁管理平台</p>
      </div>
      <el-form
        ref="formRef"
        :model="loginForm"
        :rules="rules"
        class="login-form"
        @keyup.enter="handleLogin"
      >
        <el-form-item prop="username">
          <el-input
            v-model="loginForm.username"
            placeholder="用户名"
            :prefix-icon="User"
            size="large"
          />
        </el-form-item>
        <el-form-item prop="password">
          <el-input
            v-model="loginForm.password"
            type="password"
            placeholder="密码"
            :prefix-icon="Lock"
            show-password
            size="large"
          />
        </el-form-item>
        <el-form-item>
          <el-button
            type="primary"
            size="large"
            class="login-btn"
            :loading="loading"
            @click="handleLogin"
          >
            {{ loading ? '登录中...' : '登 录' }}
          </el-button>
        </el-form-item>
      </el-form>
      <div class="login-footer">
        <p>演示账号: admin / admin123</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../../store/user'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'

const router = useRouter()
const userStore = useUserStore()
const formRef = ref(null)
const loading = ref(false)

const loginForm = reactive({
  username: '',
  password: ''
})

const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

onMounted(() => {
  loginForm.username = 'admin'
  loginForm.password = 'admin123'
})

async function handleLogin() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    // 调用 store 的 login 方法（内部已包含 API 调用和状态同步）
    await userStore.login({
      username: loginForm.username,
      password: loginForm.password
    })

    ElMessage.success('登录成功')
    router.push('/dashboard')
  } catch (error) {
    // API 不可用时使用 mock 登录（开发调试）
    if (!navigator.onLine || error.message?.includes('Network Error')) {
      ElMessage.warning('后端服务未启动，使用离线模式')
      await mockLogin()
    } else {
      ElMessage.error(error.response?.data?.message || '登录失败，请检查用户名和密码')
    }
  } finally {
    loading.value = false
  }
}

// Mock 登录（后端未启动时的降级方案）
async function mockLogin() {
  const isAdmin = loginForm.username === 'admin'
  const mockUser = {
    id: '1',
    username: loginForm.username,
    displayName: isAdmin ? '系统管理员' : '张三',
    role: 'Admin',
    phone: '13800138000',
    email: 'admin@rental.com',
    homeCompanyId: isAdmin ? null : 'ld1',
    isSuperAdmin: isAdmin,
    companyScope: ['ld1', 'ld2', 'ld3', 'ld4'],
    companyList: [
      { id: 'ld1', name: '张建国' },
      { id: 'ld2', name: '李春华' },
      { id: 'ld3', name: '王芳投资有限公司' },
      { id: 'ld4', name: '赵德明' }
    ]
  }
  const token = 'mock-token-' + Date.now()
  const permissions = ['*']

  userStore.token = token
  userStore.user = mockUser
  userStore.permissions = permissions
  userStore.homeCompanyId = mockUser.homeCompanyId
  userStore.isSuperAdmin = mockUser.isSuperAdmin
  userStore.companyScope = mockUser.companyScope
  userStore.companyList = mockUser.companyList
  userStore.currentCompanyId = null

  localStorage.setItem('token', token)
  localStorage.setItem('user', JSON.stringify(mockUser))
  localStorage.setItem('permissions', JSON.stringify(permissions))
  localStorage.removeItem('currentCompanyId')

  if (userStore.isSuperAdmin) userStore.restoreView()
  ElMessage.success('离线模式登录成功')
  router.push('/dashboard')
}
</script>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  width: 420px;
  background: #fff;
  border-radius: 12px;
  padding: 40px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.login-header {
  text-align: center;
  margin-bottom: 32px;
}

.login-header h2 {
  font-size: 24px;
  color: #303133;
  margin: 12px 0 4px;
}

.subtitle {
  font-size: 14px;
  color: #909399;
}

.login-form {
  margin-bottom: 16px;
}

.login-btn {
  width: 100%;
}

.login-footer {
  text-align: center;
  color: #909399;
  font-size: 12px;
}
</style>
