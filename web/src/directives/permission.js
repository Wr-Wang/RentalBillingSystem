/**
 * v-permission 自定义指令
 *
 * 功能：按钮级权限控制，根据用户权限码动态隐藏/显示 DOM 元素
 *
 * 用法：
 *   <!-- 单个权限码 -->
 *   <el-button v-permission="'building:create'">新增座楼</el-button>
 *
 *   <!-- 多个权限码（任一满足即可见） -->
 *   <el-button v-permission="['receipt:confirm', 'receipt:batch-confirm']">批量确认</el-button>
 *
 *   <!-- 包裹多个元素 -->
 *   <template v-permission="'contract:terminate'">
 *     <el-button>终止</el-button>
 *     <el-button>解约</el-button>
 *   </template>
 *
 *   <!-- 结合 v-if 条件 -->
 *   <el-button
 *     v-permission="'contract:terminate'"
 *     v-if="row.status === 'Active'"
 *   >终止合同</el-button>
 */
import { useUserStore } from '@/store/user'

export default {
  mounted(el, binding) {
    const { value } = binding
    if (!value) return

    // 支持字符串或数组
    const codes = Array.isArray(value) ? value : [value]
    if (!codes.length) return

    const userStore = useUserStore()
    const hasPermission = codes.some(code => userStore.hasPermission(code))

    if (!hasPermission) {
      // 从 DOM 中移除元素
      el.parentNode && el.parentNode.removeChild(el)
    }
  }
}
