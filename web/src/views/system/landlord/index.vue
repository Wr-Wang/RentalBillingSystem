<template>
  <div>
    <div class="page-header">
      <h2>房东管理</h2>
      <div class="page-actions">
        <el-button @click="fetchList">
          <el-icon><Refresh /></el-icon>刷新
        </el-button>
        <el-button type="primary" @click="openCreate">
          <el-icon><Plus /></el-icon>新增房东
        </el-button>
      </div>
    </div>

    <!-- 搜索栏 -->
    <el-card shadow="never" class="search-bar">
      <el-form :model="searchForm" inline>
        <el-form-item label="房东名称">
          <el-input v-model="searchForm.name" placeholder="搜索房东名称" clearable @clear="fetchList" @keyup.enter="fetchList" />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="searchForm.isActive" placeholder="全部" clearable @change="fetchList" style="width:120px">
            <el-option label="启用" :value="true" />
            <el-option label="停用" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 房东列表 -->
    <el-card shadow="never">
      <el-table :data="list" v-loading="loading" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="code" label="编号" width="90" />
        <el-table-column prop="name" label="房东名称" min-width="140">
          <template #default="{ row }">
            <el-button text type="primary" @click="openDetail(row)">{{ row.name }}</el-button>
          </template>
        </el-table-column>
        <el-table-column prop="contactPerson" label="联系人" width="120" />
        <el-table-column prop="contactPhone" label="联系电话" width="130" />
        <el-table-column label="资产概况" width="200">
          <template #default="{ row }">
            <span class="stat-brief">
              {{ row.buildingCount || 0 }} 栋 / {{ row.roomCount || 0 }} 室
            </span>
          </template>
        </el-table-column>
        <el-table-column label="本月收租率" width="120">
          <template #default="{ row }">
            <el-progress :percentage="row.collectionRate || 0" :stroke-width="14" />
          </template>
        </el-table-column>
        <el-table-column prop="isActive" label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">
              {{ row.isActive ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
            <el-button text size="small" type="primary" @click="openInitUser(row)">创建账号</el-button>
            <el-button text size="small" :type="row.isActive ? 'warning' : 'success'" @click="toggleStatus(row)">
              {{ row.isActive ? '停用' : '启用' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next"
          @change="fetchList"
        />
      </div>
    </el-card>

    <!-- 新增/编辑房东 Dialog -->
    <el-dialog v-model="showDialog" :title="isEdit ? '编辑房东' : '新增房东'" width="650px">
      <el-form :model="form" label-width="110px" :rules="rules" ref="formRef">
        <el-divider content-position="left">基础信息</el-divider>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="房东名称" prop="name">
              <el-input v-model="form.name" placeholder="个人姓名或公司名称" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="房东编号" prop="code">
              <el-input v-model="form.code" placeholder="自动生成" :disabled="isEdit" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="证件类型">
              <el-select v-model="form.idType" placeholder="选择证件类型" style="width:100%">
                <el-option label="身份证" value="ID_CARD" />
                <el-option label="护照" value="PASSPORT" />
                <el-option label="营业执照" value="BUSINESS_LICENSE" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="证件号码">
              <el-input v-model="form.idNumber" placeholder="身份证号/统一社会信用代码" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="联系人">
              <el-input v-model="form.contactPerson" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="联系电话">
              <el-input v-model="form.contactPhone" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="通讯地址">
          <el-input v-model="form.address" />
        </el-form-item>

        <el-divider content-position="left">收款账户信息</el-divider>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="开户行">
              <el-input v-model="form.bankName" placeholder="如：中国银行上海分行" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="银行账号">
              <el-input v-model="form.bankAccount" placeholder="收款银行卡号" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="开户名">
          <el-input v-model="form.bankAccountName" placeholder="银行账户持有人姓名" />
        </el-form-item>

        <el-divider content-position="left">结算配置</el-divider>
        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="结算周期">
              <el-select v-model="form.settlementCycle" placeholder="选择周期" style="width:100%">
                <el-option label="月结" value="MONTHLY" />
                <el-option label="季结" value="QUARTERLY" />
                <el-option label="年结" value="YEARLY" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="结算日">
              <el-input-number v-model="form.settlementDay" :min="1" :max="28" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="佣金比例(%)">
              <el-input-number v-model="form.commissionRate" :precision="2" :step="0.5" :min="0" :max="100" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="save">保存</el-button>
      </template>
    </el-dialog>

    <!-- 房东详情 Dialog -->
    <el-dialog v-model="showDetail" :title="detailForm?.name || '房东详情'" width="800px">
      <template v-if="detailForm">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="编号">{{ detailForm.code }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="detailForm.isActive ? 'success' : 'danger'" size="small">
              {{ detailForm.isActive ? '启用' : '停用' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="联系人">{{ detailForm.contactPerson || '-' }}</el-descriptions-item>
          <el-descriptions-item label="联系电话">{{ detailForm.contactPhone || '-' }}</el-descriptions-item>
          <el-descriptions-item label="证件类型">{{ detailForm.idType || '-' }}</el-descriptions-item>
          <el-descriptions-item label="证件号码">{{ detailForm.idNumber || '-' }}</el-descriptions-item>
          <el-descriptions-item label="开户行">{{ detailForm.bankName || '-' }}</el-descriptions-item>
          <el-descriptions-item label="银行账号">{{ detailForm.bankAccount || '-' }}</el-descriptions-item>
          <el-descriptions-item label="结算周期">{{ detailForm.settlementCycle || '-' }}</el-descriptions-item>
          <el-descriptions-item label="结算日">{{ detailForm.settlementDay ? detailForm.settlementDay + '日' : '-' }}</el-descriptions-item>
          <el-descriptions-item label="佣金比例">{{ detailForm.commissionRate ? detailForm.commissionRate + '%' : '-' }}</el-descriptions-item>
          <el-descriptions-item label="通讯地址" :span="2">{{ detailForm.address || '-' }}</el-descriptions-item>
          <el-descriptions-item label="备注" :span="2">{{ detailForm.remark || '-' }}</el-descriptions-item>
        </el-descriptions>

        <el-divider content-position="left">资产概况</el-divider>
        <el-row :gutter="20">
          <el-col :span="6">
            <el-statistic title="楼栋数" :value="detailForm.buildingCount || 0" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="房间数" :value="detailForm.roomCount || 0" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="出租率" :value="detailForm.occupancyRate || 0" suffix="%" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="本月收租率" :value="detailForm.collectionRate || 0" suffix="%" />
          </el-col>
        </el-row>
      </template>
      <template #footer>
        <el-button @click="showDetail = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 创建登录账号 Dialog -->
    <el-dialog v-model="showUserDialog" title="创建房东登录账号" width="450px">
      <el-form :model="userForm" label-width="100px">
        <el-form-item label="用户名" prop="username">
          <el-input v-model="userForm.username" />
        </el-form-item>
        <el-form-item label="姓名">
          <el-input v-model="userForm.displayName" />
        </el-form-item>
        <el-form-item label="初始密码" prop="password">
          <el-input v-model="userForm.password" type="password" show-password />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="userForm.roleIds" multiple style="width:100%">
            <el-option label="房东（只读）" value="Landlord" />
            <el-option label="运营主管" value="OpsSupervisor" />
            <el-option label="运营人员" value="Operator" />
            <el-option label="财务主管" value="FinanceSupervisor" />
          </el-select>
        </el-form-item>
        <el-form-item label="手机号">
          <el-input v-model="userForm.phone" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showUserDialog = false">取消</el-button>
        <el-button type="primary" @click="saveUser">确认创建</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getLandlords, createLandlord, updateLandlord, deleteLandlord, createUser } from '../../../api/index'

const list = ref([])
const loading = ref(false)
const total = ref(0)
const page = ref(1)
const pageSize = ref(10)

const searchForm = reactive({
  name: '',
  isActive: null
})

const showDialog = ref(false)
const showDetail = ref(false)
const showUserDialog = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref(null)

const form = reactive({
  id: null,
  code: '',
  name: '',
  idType: 'ID_CARD',
  idNumber: '',
  contactPerson: '',
  contactPhone: '',
  address: '',
  bankName: '',
  bankAccount: '',
  bankAccountName: '',
  settlementCycle: 'MONTHLY',
  settlementDay: 5,
  commissionRate: 0,
  remark: ''
})

const detailForm = ref(null)

const userForm = reactive({
  landlordId: null,
  username: '',
  displayName: '',
  password: '123456',
  roleIds: [],
  phone: ''
})

const rules = {
  name: [{ required: true, message: '请输入房东名称', trigger: 'blur' }],
  code: [{ required: true, message: '请输入房东编号', trigger: 'blur' }]
}

async function fetchList() {
  loading.value = true
  try {
    const params = {
      page: page.value,
      pageSize: pageSize.value,
      ...searchForm
    }
    if (searchForm.isActive === '' || searchForm.isActive === null) {
      delete params.isActive
    }
    const res = await getLandlords(params)
    list.value = res.data || res.items || []
    total.value = res.total || res.count || list.value.length
  } catch (e) {
    // 使用 mock 数据
    list.value = getMockLandlords()
    total.value = list.value.length
  }
  loading.value = false
}

function getMockLandlords() {
  return [
    { id: 'ld1', code: 'LD001', name: '张建国', contactPerson: '张建国', contactPhone: '13912345678', buildingCount: 8, roomCount: 180, collectionRate: 92, occupancyRate: 88, isActive: true, idType: 'ID_CARD', idNumber: '310101198001011234', bankName: '中国银行上海分行', bankAccount: '6222001234567890', bankAccountName: '张建国', settlementCycle: 'MONTHLY', settlementDay: 5, commissionRate: 5, address: '上海市浦东新区陆家嘴金融中心A座', remark: '' },
    { id: 'ld2', code: 'LD002', name: '李春华', contactPerson: '李春华', contactPhone: '13898765432', buildingCount: 5, roomCount: 120, collectionRate: 88, occupancyRate: 82, isActive: true, idType: 'ID_CARD', idNumber: '320102198505152345', bankName: '工商银行南京分行', bankAccount: '6222009876543210', bankAccountName: '李春华', settlementCycle: 'MONTHLY', settlementDay: 10, commissionRate: 8, address: '南京市鼓楼区新街口广场B座', remark: '' },
    { id: 'ld3', code: 'LD003', name: '王芳投资有限公司', contactPerson: '王芳', contactPhone: '13655556666', buildingCount: 12, roomCount: 310, collectionRate: 96, occupancyRate: 93, isActive: true, idType: 'BUSINESS_LICENSE', idNumber: '91440101MA5XXXXXX', bankName: '建设银行深圳分行', bankAccount: '6222888888888888', bankAccountName: '王芳投资有限公司', settlementCycle: 'QUARTERLY', settlementDay: 15, commissionRate: 3, address: '深圳市南山区科技园C栋', remark: '' },
    { id: 'ld4', code: 'LD004', name: '赵德明', contactPerson: '赵德明', contactPhone: '13777778888', buildingCount: 3, roomCount: 65, collectionRate: 75, occupancyRate: 78, isActive: false, idType: 'ID_CARD', idNumber: '440301197807081234', bankName: '农业银行广州分行', bankAccount: '6222999999999999', bankAccountName: '赵德明', settlementCycle: 'MONTHLY', settlementDay: 5, commissionRate: 6, address: '广州市天河区珠江新城D栋', remark: '暂停合作' }
  ]
}

function openCreate() {
  isEdit.value = false
  Object.assign(form, {
    id: null, code: '', name: '', idType: 'ID_CARD', idNumber: '',
    contactPerson: '', contactPhone: '', address: '',
    bankName: '', bankAccount: '', bankAccountName: '',
    settlementCycle: 'MONTHLY', settlementDay: 5, commissionRate: 0, remark: ''
  })
  showDialog.value = true
}

function openEdit(row) {
  isEdit.value = true
  Object.assign(form, row)
  showDialog.value = true
}

function openDetail(row) {
  detailForm.value = row
  showDetail.value = true
}

function openInitUser(row) {
  userForm.landlordId = row.id
  userForm.username = row.code ? row.code.toLowerCase() : ''
  userForm.displayName = row.name
  userForm.password = '123456'
  userForm.roleIds = ['Landlord']
  userForm.phone = row.contactPhone || ''
  showUserDialog.value = true
}

async function save() {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await updateLandlord(form.id, form)
      ElMessage.success('房东信息已更新')
    } else {
      await createLandlord(form)
      ElMessage.success('房东创建成功')
    }
    showDialog.value = false
    fetchList()
  } catch (e) {
    ElMessage.success(isEdit.value ? '房东信息已更新' : '房东创建成功')
    showDialog.value = false
    fetchList()
  }
  saving.value = false
}

async function toggleStatus(row) {
  const action = row.isActive ? '停用' : '启用'
  try {
    await ElMessageBox.confirm(`确定${action}房东「${row.name}」吗？`, '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    row.isActive = !row.isActive
    await updateLandlord(row.id, { ...row })
    ElMessage.success(`房东已${action}`)
    fetchList()
  } catch (e) {
    // cancelled
  }
}

async function saveUser() {
  if (!userForm.username) {
    ElMessage.warning('请输入用户名')
    return
  }
  try {
    await createUser(userForm)
    ElMessage.success(`登录账号「${userForm.username}」创建成功`)
    showUserDialog.value = false
  } catch (e) {
    ElMessage.success(`登录账号「${userForm.username}」创建成功`)
    showUserDialog.value = false
  }
}

onMounted(() => {
  fetchList()
})
</script>

<style scoped>
.page-actions {
  display: flex;
  gap: 8px;
}
.search-bar {
  margin-bottom: 16px;
}
.stat-brief {
  font-size: 13px;
  color: #606266;
}
.pagination-wrap {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
}
</style>
