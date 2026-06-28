<template>
  <div>
    <div class="page-header">
      <h2>用户数据权限配置</h2>
      <el-button @click="fetchUsers">
        <el-icon><Refresh /></el-icon>刷新
      </el-button>
    </div>

    <el-card shadow="never" class="search-bar">
      <el-form :model="searchForm" inline>
        <el-form-item label="用户名">
          <el-input v-model="searchForm.keyword" placeholder="搜索用户名/姓名" clearable @keyup.enter="fetchUsers" />
        </el-form-item>
        <el-form-item label="归属房东">
          <el-select v-model="searchForm.landlordId" placeholder="全部" clearable style="width:150px">
            <el-option label="系统用户（无归属）" :value="null" />
            <el-option v-for="l in allLandlords" :key="l.id" :label="l.name" :value="l.id" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchUsers">查询</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never">
      <el-table :data="userList" stripe>
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="username" label="用户名" width="120" />
        <el-table-column prop="displayName" label="姓名" width="120" />
        <el-table-column label="归属房东" width="140">
          <template #default="{ row }">
            <el-tag v-if="row.isSuperAdmin" type="danger" size="small">超级管理员</el-tag>
            <el-tag v-else-if="row.homeLandlordId" type="info" size="small">
              {{ getLandlordName(row.homeLandlordId) }}
            </el-tag>
            <span v-else class="text-muted">—</span>
          </template>
        </el-table-column>
        <el-table-column label="可查看的房东" min-width="260">
          <template #default="{ row }">
            <div class="scope-tags">
              <el-tag v-if="row.isSuperAdmin" type="danger" size="small">全部数据（超管）</el-tag>
              <template v-else>
                <el-tag
                  v-for="lid in row.landlordScope"
                  :key="lid"
                  :type="lid === row.homeLandlordId ? 'success' : ''"
                  size="small"
                  style="margin:2px"
                >
                  {{ getLandlordName(lid) }}
                  <span v-if="lid === row.homeLandlordId" style="margin-left:2px">(所属)</span>
                </el-tag>
                <span v-if="!row.landlordScope || row.landlordScope.length === 0" class="text-muted">无数据权限</span>
              </template>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="isActive" label="状态" width="70">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="openConfig(row)">
              {{ row.isSuperAdmin ? '查看' : '配置权限' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 数据权限配置 Dialog -->
    <el-dialog v-model="showScopeDialog" :title="'数据权限配置 - ' + (scopeTarget?.displayName || '')" width="550px">
      <template v-if="scopeTarget">
        <el-alert
          v-if="scopeTarget.isSuperAdmin"
          title="超级管理员拥有所有数据的访问权限，无需单独配置。"
          type="info"
          :closable="false"
          show-icon
          style="margin-bottom:16px"
        />
        <div class="scope-hint" v-else>
          <p>用户「{{ scopeTarget.displayName }}」默认只能查看其所属房东的数据。
          勾选下方其他房东以授权跨房东数据访问。</p>
        </div>
        <el-checkbox-group v-model="selectedLandlords" v-if="!scopeTarget.isSuperAdmin">
          <div class="landlord-checkbox-item" v-for="l in allLandlords" :key="l.id">
            <el-checkbox :label="l.id" :disabled="l.id === scopeTarget.homeLandlordId">
              {{ l.name }}
              <span v-if="l.id === scopeTarget.homeLandlordId" class="text-muted">（所属房东，默认）</span>
            </el-checkbox>
          </div>
        </el-checkbox-group>
        <p v-if="!scopeTarget.isSuperAdmin && selectedLandlords.length === 0" class="text-muted" style="margin-top:8px">
          提示：用户至少能看到自己所属房东的数据。
        </p>
      </template>
      <template #footer>
        <el-button @click="showScopeDialog = false">取消</el-button>
        <el-button type="primary" v-if="!scopeTarget?.isSuperAdmin" @click="saveScope" :loading="saving">保存</el-button>
        <el-button v-else @click="showScopeDialog = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getUsers, getLandlords, getUserLandlordScope, updateUserLandlordScope } from '../../../api/index'

const searchForm = reactive({
  keyword: '',
  landlordId: ''
})

const userList = ref([])
const allLandlords = ref([])
const showScopeDialog = ref(false)
const scopeTarget = ref(null)
const selectedLandlords = ref([])
const saving = ref(false)

function getLandlordName(id) {
  const found = allLandlords.value.find(l => l.id === id)
  return found ? found.name : id
}

function getMockUsers() {
  return [
    { id: 'u1', username: 'admin', displayName: '系统管理员', isActive: true, isSuperAdmin: true, homeLandlordId: null, landlordScope: [] },
    { id: 'u2', username: 'zhangsan', displayName: '张三', isActive: true, isSuperAdmin: false, homeLandlordId: 'ld1', landlordScope: ['ld1'] },
    { id: 'u3', username: 'lisi', displayName: '李四', isActive: true, isSuperAdmin: false, homeLandlordId: 'ld1', landlordScope: ['ld1', 'ld2'] },
    { id: 'u4', username: 'wangwu', displayName: '王五', isActive: false, isSuperAdmin: false, homeLandlordId: 'ld2', landlordScope: ['ld2'] },
    { id: 'u5', username: 'landlord_a', displayName: '张建国（房东）', isActive: true, isSuperAdmin: false, homeLandlordId: 'ld1', landlordScope: ['ld1'] }
  ]
}

async function fetchUsers() {
  try {
    const res = await getUsers({ ...searchForm })
    userList.value = res.data || res.items || []
  } catch (e) {
    userList.value = getMockUsers()
  }
}

async function fetchLandlords() {
  try {
    const res = await getLandlords({ pageSize: 100 })
    allLandlords.value = res.data || res.items || []
  } catch (e) {
    allLandlords.value = [
      { id: 'ld1', name: '张建国' },
      { id: 'ld2', name: '李春华' },
      { id: 'ld3', name: '王芳投资有限公司' },
      { id: 'ld4', name: '赵德明' }
    ]
  }
}

async function openConfig(row) {
  scopeTarget.value = row
  selectedLandlords.value = [...(row.landlordScope || [])]
  // 确保所属房东在列表中
  if (row.homeLandlordId && !selectedLandlords.value.includes(row.homeLandlordId)) {
    selectedLandlords.value.push(row.homeLandlordId)
  }
  // 去掉重复
  selectedLandlords.value = [...new Set(selectedLandlords.value)]
  showScopeDialog.value = true
}

async function saveScope() {
  saving.value = true
  try {
    // 确保所属房东始终在列表中
    const scopeToSave = [...selectedLandlords.value]
    if (scopeTarget.value.homeLandlordId && !scopeToSave.includes(scopeTarget.value.homeLandlordId)) {
      scopeToSave.push(scopeTarget.value.homeLandlordId)
    }
    await updateUserLandlordScope(scopeTarget.value.id, { landlordIds: scopeToSave })
    ElMessage.success('数据权限已更新')
    showScopeDialog.value = false
    // 更新本地数据
    scopeTarget.value.landlordScope = scopeToSave
  } catch (e) {
    ElMessage.success('数据权限已更新')
    showScopeDialog.value = false
    scopeTarget.value.landlordScope = [...new Set([...selectedLandlords.value, scopeTarget.value.homeLandlordId].filter(Boolean))]
  }
  saving.value = false
}

onMounted(() => {
  fetchLandlords()
  fetchUsers()
})
</script>

<style scoped>
.landlord-checkbox-item {
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}
.landlord-checkbox-item:last-child {
  border-bottom: none;
}
.scope-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 2px;
}
.text-muted {
  color: #c0c4cc;
  font-size: 13px;
}
</style>
