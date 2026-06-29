import { createRouter, createWebHistory } from 'vue-router'
import RouteView from '../layout/RouteView.vue'

const routes = [
  // Redirect helper（用于视角切换时强制刷新页面）
  {
    path: '/redirect/:path(.*)',
    component: () => import('../views/redirect/index.vue'),
    hidden: true
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/login/index.vue'),
    hidden: true
  },
  {
    path: '/',
    component: () => import('../layout/MainLayout.vue'),
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('../views/dashboard/index.vue'),
        meta: { title: '仪表盘', icon: 'DataAnalysis', roles: ['Admin', 'OpsSupervisor', 'Operator', 'FinanceSupervisor', 'FinanceDirector', 'Accountant', 'DeptManager', 'GeneralManager', 'Legal'] }
      },
      // 房屋管理
      {
        path: 'buildings',
        name: 'BuildingList',
        component: () => import('../views/building/index.vue'),
        meta: { title: '房屋管理', icon: 'HomeFilled', roles: ['Admin', 'OpsSupervisor', 'Operator', 'FinanceSupervisor'] }
      },
      {
        path: 'buildings/room/:id',
        name: 'RoomDetail',
        component: () => import('../views/building/roomDetail.vue'),
        meta: { title: '房间详情', icon: 'HomeFilled', hidden: true }
      },
      {
        path: 'buildings/import',
        name: 'BuildingImport',
        component: () => import('../views/building/import.vue'),
        meta: { title: '批量导入房屋', icon: 'Upload', hidden: true }
      },
      // 合同管理
      {
        path: 'contracts',
        name: 'ContractList',
        component: () => import('../views/contract/index.vue'),
        meta: { title: '合同管理', icon: 'Document', roles: ['Admin', 'OpsSupervisor', 'Operator', 'FinanceSupervisor', 'Accountant', 'DeptManager', 'GeneralManager'] }
      },
      {
        path: 'contracts/create',
        name: 'ContractCreate',
        component: () => import('../views/contract/create.vue'),
        meta: { title: '新建合同', icon: 'DocumentAdd', hidden: true }
      },
      {
        path: 'contracts/:id',
        name: 'ContractDetail',
        component: () => import('../views/contract/detail.vue'),
        meta: { title: '合同详情', icon: 'Document', hidden: true }
      },
      // 收款管理
      {
        path: 'receipts',
        name: 'ReceiptList',
        component: () => import('../views/receipt/index.vue'),
        meta: { title: '收款管理', icon: 'Money', roles: ['Admin', 'OpsSupervisor', 'Operator', 'FinanceSupervisor', 'FinanceDirector', 'Accountant'] }
      },
      {
        path: 'receipts/register',
        name: 'ReceiptRegister',
        component: () => import('../views/receipt/register.vue'),
        meta: { title: '收款登记', icon: 'Edit', hidden: true }
      },
      {
        path: 'receipts/confirm',
        name: 'ReceiptConfirm',
        component: () => import('../views/receipt/confirm.vue'),
        meta: { title: '收款确认', icon: 'Select', hidden: true }
      },
      // 账单管理
      {
        path: 'bills',
        name: 'BillList',
        component: () => import('../views/bill/index.vue'),
        meta: { title: '账单管理', icon: 'DocumentCopy', roles: ['Admin', 'OpsSupervisor', 'FinanceSupervisor', 'FinanceDirector', 'Accountant'] }
      },
      {
        path: 'bills/generate',
        name: 'BillGenerate',
        component: () => import('../views/bill/generate.vue'),
        meta: { title: '生成账单', hidden: true }
      },
      {
        path: 'bills/preview/:id',
        name: 'BillPreview',
        component: () => import('../views/bill/preview.vue'),
        meta: { title: '账单预览', hidden: true }
      },
      // 租客管理
      {
        path: 'tenants',
        name: 'TenantList',
        component: () => import('../views/tenant/index.vue'),
        meta: { title: '租客管理', icon: 'UserFilled', roles: ['Admin', 'OpsSupervisor', 'Operator'] }
      },
      {
        path: 'tenants/:id',
        name: 'TenantDetail',
        component: () => import('../views/tenant/detail.vue'),
        meta: { title: '租客详情', icon: 'UserFilled', hidden: true }
      },
      // 催缴管理
      {
        path: 'collection',
        name: 'CollectionOverview',
        component: () => import('../views/collection/overview.vue'),
        meta: { title: '催缴管理', icon: 'BellFilled', roles: ['Admin', 'OpsSupervisor', 'Operator', 'FinanceSupervisor', 'Legal'] }
      },
      {
        path: 'collection/config',
        name: 'CollectionConfig',
        component: () => import('../views/collection/config.vue'),
        meta: { title: '催缴配置', icon: 'Setting', hidden: true }
      },
      {
        path: 'collection/records',
        name: 'CollectionRecords',
        component: () => import('../views/collection/records.vue'),
        meta: { title: '催缴记录', icon: 'Tickets', hidden: true }
      },
      // 抄表管理
      {
        path: 'meter',
        name: 'MeterReading',
        component: () => import('../views/meter/index.vue'),
        meta: { title: '抄表管理', icon: 'Reading', roles: ['Admin', 'OpsSupervisor', 'Operator'] }
      },
      // 审批中心
      {
        path: 'approvals',
        name: 'ApprovalPending',
        component: () => import('../views/approval/pending.vue'),
        meta: { title: '审批中心', icon: 'CircleCheck', roles: ['Admin', 'OpsSupervisor', 'FinanceSupervisor', 'FinanceDirector', 'DeptManager', 'GeneralManager', 'Legal'] }
      },
      {
        path: 'approvals/myrequests',
        name: 'ApprovalMyRequests',
        component: () => import('../views/approval/myRequests.vue'),
        meta: { title: '我的提交', icon: 'EditPen', hidden: true }
      },
      {
        path: 'approvals/history',
        name: 'ApprovalHistory',
        component: () => import('../views/approval/history.vue'),
        meta: { title: '审批历史', icon: 'Timer', hidden: true }
      },
      // 会计管理
      {
        path: 'accounting',
        component: RouteView,
        redirect: '/accounting/subjects',
        meta: { title: '会计管理', icon: 'DataBoard', roles: ['Admin', 'FinanceSupervisor', 'FinanceDirector', 'Accountant'] },
        children: [
          {
            path: 'subjects',
            name: 'AccountingSubjects',
            component: () => import('../views/accounting/subjects.vue'),
            meta: { title: '科目表', icon: 'List' }
          },
          {
            path: 'journal',
            name: 'AccountingJournal',
            component: () => import('../views/accounting/journal.vue'),
            meta: { title: '日记账', icon: 'DocumentCopy' }
          },
          {
            path: 'vouchers',
            name: 'AccountingVouchers',
            component: () => import('../views/accounting/vouchers.vue'),
            meta: { title: '凭证管理', icon: 'Files' }
          },
          {
            path: 'trialbalance',
            name: 'TrialBalance',
            component: () => import('../views/accounting/trialBalance.vue'),
            meta: { title: '试算平衡表', icon: 'ScaleToOriginal' }
          }
        ]
      },
      // 银行对账
      {
        path: 'bank',
        component: RouteView,
        redirect: '/bank/import',
        meta: { title: '银行对账', icon: 'CreditCard', roles: ['Admin', 'FinanceSupervisor', 'FinanceDirector', 'Accountant'] },
        children: [
          {
            path: 'import',
            name: 'BankImport',
            component: () => import('../views/bank/import.vue'),
            meta: { title: '流水导入', icon: 'Upload' }
          },
          {
            path: 'match',
            name: 'BankMatch',
            component: () => import('../views/bank/match.vue'),
            meta: { title: '自动匹配', icon: 'Link' }
          },
          {
            path: 'reconciliation',
            name: 'BankReconciliation',
            component: () => import('../views/bank/reconciliation.vue'),
            meta: { title: '余额调节表', icon: 'DataAnalysis' }
          }
        ]
      },
      // 报表中心
      {
        path: 'reports',
        component: RouteView,
        redirect: '/reports/collectionrate',
        meta: { title: '报表中心', icon: 'DataLine', roles: ['Admin', 'OpsSupervisor', 'Operator', 'FinanceSupervisor', 'FinanceDirector', 'Accountant', 'DeptManager', 'GeneralManager', 'Legal'] },
        children: [
          {
            path: 'collectionrate',
            name: 'ReportCollectionRate',
            component: () => import('../views/report/collectionRate.vue'),
            meta: { title: '收租率统计', icon: 'TrendCharts' }
          },
          {
            path: 'overduedetail',
            name: 'ReportOverdueDetail',
            component: () => import('../views/report/overdueDetail.vue'),
            meta: { title: '欠费明细表', icon: 'WarningFilled' }
          },
          {
            path: 'dailyreceipt',
            name: 'ReportDailyReceipt',
            component: () => import('../views/report/dailyReceipt.vue'),
            meta: { title: '收款日报', icon: 'Sunny' }
          },
          {
            path: 'monthlyreceipt',
            name: 'ReportMonthlyReceipt',
            component: () => import('../views/report/monthlyReceipt.vue'),
            meta: { title: '收款月报', icon: 'Calendar' }
          },
          {
            path: 'feerevenue',
            name: 'ReportFeeRevenue',
            component: () => import('../views/report/feeRevenue.vue'),
            meta: { title: '费用收入统计', icon: 'Coin' }
          },
          {
            path: 'occupancyrate',
            name: 'ReportOccupancyRate',
            component: () => import('../views/report/occupancyRate.vue'),
            meta: { title: '出租率统计', icon: 'Grid' }
          }
        ]
      },
      // 通知中心
      {
        path: 'notifications',
        name: 'NotificationCenter',
        component: () => import('../views/notification/index.vue'),
        meta: { title: '通知中心', icon: 'Bell', roles: ['Admin', 'OpsSupervisor', 'Operator', 'FinanceSupervisor', 'FinanceDirector', 'Accountant', 'DeptManager', 'GeneralManager', 'Legal'] }
      },

      // ========== 多公司相关页面 ==========
      // 多公司总览（仅超级管理员可见）
      {
        path: 'reports/companyoverview',
        name: 'ReportCompanyOverview',
        component: () => import('../views/report/companyOverview.vue'),
        meta: { title: '多公司总览', icon: 'DataAnalysis', roles: ['Admin'] }
      },
      // Report shortcuts for companyoverview
      { path: 'companyoverview', redirect: '/reports/companyoverview', meta: { hidden: true } },
      // 变更审计
      {
        path: 'audit',
        name: 'AuditLog',
        component: () => import('../views/audit/index.vue'),
        meta: { title: '变更审计', icon: 'Search', roles: ['Admin', 'OpsSupervisor', 'FinanceSupervisor', 'FinanceDirector'] }
      },
      // ========== Convenience Redirect Aliases (no hyphens) ==========
      { path: 'organization/users', redirect: '/system/organization/users', meta: { hidden: true } },
      { path: 'organization/roles', redirect: '/system/organization/roles', meta: { hidden: true } },
      { path: 'menus', redirect: '/system/menus', meta: { hidden: true } },
      { path: 'approvaltypes', redirect: '/system/approvaltypes', meta: { hidden: true } },
      { path: 'approvallevels', redirect: '/system/approvallevels', meta: { hidden: true } },
      { path: 'feecodes', redirect: '/system/feecodes', meta: { hidden: true } },
      { path: 'roomtypes', redirect: '/system/roomtypes', meta: { hidden: true } },
      { path: 'pricing', redirect: '/system/pricing', meta: { hidden: true } },
      { path: 'paymentchannels', redirect: '/system/paymentchannels', meta: { hidden: true } },
      { path: 'taxrates', redirect: '/system/taxrates', meta: { hidden: true } },
      { path: 'accountingsubjects', redirect: '/system/accountingsubjects', meta: { hidden: true } },
      { path: 'scheduler', redirect: '/system/scheduler', meta: { hidden: true } },
      { path: 'holidays', redirect: '/system/holidays', meta: { hidden: true } },
      { path: 'latefee', redirect: '/system/latefee', meta: { hidden: true } },
      // Report shortcuts
      { path: 'collectionrate', redirect: '/reports/collectionrate', meta: { hidden: true } },
      { path: 'overduedetail', redirect: '/reports/overduedetail', meta: { hidden: true } },
      { path: 'dailyreceipt', redirect: '/reports/dailyreceipt', meta: { hidden: true } },
      { path: 'monthlyreceipt', redirect: '/reports/monthlyreceipt', meta: { hidden: true } },
      { path: 'feerevenue', redirect: '/reports/feerevenue', meta: { hidden: true } },
      { path: 'occupancyrate', redirect: '/reports/occupancyrate', meta: { hidden: true } },
      // Accounting shortcuts
      { path: 'trialbalance', redirect: '/accounting/trialbalance', meta: { hidden: true } },
      // Other convenience shortcuts
      { path: 'import', redirect: '/buildings/import', meta: { hidden: true } },
      { path: 'register', redirect: '/receipts/register', meta: { hidden: true } },
      { path: 'confirm', redirect: '/receipts/confirm', meta: { hidden: true } },
      { path: 'config', redirect: '/collection/config', meta: { hidden: true } },
      { path: 'records', redirect: '/collection/records', meta: { hidden: true } },
      { path: 'history', redirect: '/approvals/history', meta: { hidden: true } },
      { path: 'subjects', redirect: '/accounting/subjects', meta: { hidden: true } },
      { path: 'journal', redirect: '/accounting/journal', meta: { hidden: true } },
      { path: 'vouchers', redirect: '/accounting/vouchers', meta: { hidden: true } },
      { path: 'match', redirect: '/bank/match', meta: { hidden: true } },
      { path: 'reconciliation', redirect: '/bank/reconciliation', meta: { hidden: true } },
      { path: 'users', redirect: '/system/organization/users', meta: { hidden: true } },
      { path: 'roles', redirect: '/system/organization/roles', meta: { hidden: true } },
      // 系统设置
      {
        path: 'system',
        component: RouteView,
        redirect: '/system/organization/users',
        meta: { title: '系统设置', icon: 'Setting', roles: ['Admin'] },
        children: [
          {
            path: 'companies',
            name: 'SystemCompanies',
            component: () => import('../views/system/company/index.vue'),
            meta: { title: '公司管理', icon: 'OfficeBuilding' }
          },
          {
            path: 'organization/users',
            name: 'SystemUsers',
            component: () => import('../views/system/organization/user.vue'),
            meta: { title: '用户管理', icon: 'User' }
          },
          {
            path: 'organization/roles',
            name: 'SystemRoles',
            component: () => import('../views/system/organization/role.vue'),
            meta: { title: '角色管理', icon: 'Avatar' }
          },
          {
            path: 'organization/userscope',
            name: 'SystemUserScope',
            component: () => import('../views/system/organization/userScope.vue'),
            meta: { title: '用户数据权限', icon: 'Unlock' }
          },
          {
            path: 'menus',
            name: 'SystemMenus',
            component: () => import('../views/system/menu/index.vue'),
            meta: { title: '菜单权限配置', icon: 'Menu' }
          },
          {
            path: 'approvaltypes',
            name: 'SystemApprovalTypes',
            component: () => import('../views/system/approvalConfig/types.vue'),
            meta: { title: '审批类型配置', icon: 'CircleCheck' }
          },
          {
            path: 'approvallevels',
            name: 'SystemApprovalLevels',
            component: () => import('../views/system/approvalConfig/levels.vue'),
            meta: { title: '审批级别配置', icon: 'Sort', hidden: true }
          },
          {
            path: 'feecodes',
            name: 'SystemFeeCodes',
            component: () => import('../views/system/feeCodes.vue'),
            meta: { title: '收费项目管理', icon: 'Coin' }
          },
          {
            path: 'roomtypes',
            name: 'SystemRoomTypes',
            component: () => import('../views/system/roomType.vue'),
            meta: { title: '房型管理', icon: 'Grid' }
          },
          {
            path: 'pricing',
            name: 'SystemPricing',
            component: () => import('../views/system/pricing.vue'),
            meta: { title: '定价标准管理', icon: 'PriceTag' }
          },
          {
            path: 'paymentchannels',
            name: 'SystemPaymentChannels',
            component: () => import('../views/system/paymentChannel.vue'),
            meta: { title: '支付通道管理', icon: 'CreditCard' }
          },
          {
            path: 'taxrates',
            name: 'SystemTaxRates',
            component: () => import('../views/system/taxRate.vue'),
            meta: { title: '税率配置', icon: 'CollectionTag' }
          },
          {
            path: 'accountingsubjects',
            name: 'SystemAccountingSubjects',
            component: () => import('../views/system/accountingSubject.vue'),
            meta: { title: '会计科目管理', icon: 'DataBoard' }
          },
          {
            path: 'scheduler',
            name: 'SystemScheduler',
            component: () => import('../views/system/scheduler.vue'),
            meta: { title: '调度任务管理', icon: 'Timer' }
          },
          {
            path: 'holidays',
            name: 'SystemHolidays',
            component: () => import('../views/system/holiday.vue'),
            meta: { title: '节假日管理', icon: 'Calendar' }
          },
          {
            path: 'latefee',
            name: 'SystemLateFee',
            component: () => import('../views/system/lateFeeConfig.vue'),
            meta: { title: '滞纳金配置', icon: 'WarningFilled' }
          },
          {
            path: 'logs',
            name: 'SystemLogs',
            component: () => import('../views/system/log/index.vue'),
            meta: { title: '系统日志', icon: 'Document', roles: ['Admin'] }
          },
          {
            path: 'apilogs',
            name: 'SystemApiLogs',
            component: () => import('../views/system/apilog/index.vue'),
            meta: { title: 'API 日志', icon: 'Monitor', roles: ['Admin'] }
          }
        ]
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Route guard
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  if (to.path !== '/login' && !token) {
    next('/login')
  } else if (to.path === '/login' && token) {
    next('/dashboard')
  } else {
    next()
  }
})

export default router
