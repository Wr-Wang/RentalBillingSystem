<template>
  <div>
    <div class="page-header">
      <h2>房源管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="showAddUnit = true">
          <el-icon><Plus /></el-icon>新增房源
        </el-button>
        <el-button @click="$router.push('/buildings/import')">
          <el-icon><Upload /></el-icon>批量导入
        </el-button>
      </div>
    </div>

    <!-- Stats Cards -->
    <el-row :gutter="16" style="margin-bottom: 16px;">
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-value">{{ stats.total }}</div>
            <div class="stat-label">房源总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-value" style="color: #909399;">{{ stats.vacant }}</div>
            <div class="stat-label">空置</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-value" style="color: #67c23a;">{{ stats.rented }}</div>
            <div class="stat-label">已租</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-value" style="color: #e6a23c;">{{ stats.maintenance }}</div>
            <div class="stat-label">维修</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Search -->
    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="搜索座楼/房号/地址" clearable style="width: 220px;" @clear="handleSearch" />
      <el-select v-model="search.status" placeholder="房屋状态" clearable style="width: 140px;" @change="handleSearch">
        <el-option label="空置" value="Vacant" />
        <el-option label="已租" value="Rented" />
        <el-option label="维修" value="Maintenance" />
      </el-select>
      <el-select v-model="search.buildingName" placeholder="选择座楼" clearable style="width: 150px;" @change="handleSearch">
        <el-option v-for="b in buildings" :key="b.buildingName" :label="b.buildingName" :value="b.buildingName" />
      </el-select>
      <el-button type="primary" @click="handleSearch">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-row :gutter="16">
      <!-- Building Tree -->
      <el-col :span="6">
        <el-card style="min-height: 480px;">
          <template #header>
            <span>房屋结构</span>
          </template>
          <el-tree
            :data="buildingTree"
            node-key="id"
            :props="{ label: 'name', children: 'children' }"
            @node-click="handleNodeClick"
            :highlight-current="true"
            v-loading="treeLoading"
          />
        </el-card>
      </el-col>

      <!-- Room List -->
      <el-col :span="18">
        <el-card>
          <template #header>
            <span>房源列表</span>
          </template>
          <el-table :data="roomList" stripe v-loading="roomLoading" style="width: 100%">
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="fullCode" label="房源编号" width="130" />
            <el-table-column prop="buildingName" label="座楼" width="80" />
            <el-table-column prop="floorName" label="楼层" width="70" />
            <el-table-column prop="unitNo" label="房号" width="65" />
            <el-table-column prop="roomTypeName" label="房型" width="90" />
            <el-table-column prop="area" label="面积(m²)" width="85">
              <template #default="{ row }">{{ row.area ? row.area + ' m²' : '-' }}</template>
            </el-table-column>
            <el-table-column prop="orientation" label="朝向" width="65" />
            <el-table-column prop="status" label="状态" width="80">
              <template #default="{ row }">
                <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
                  {{ statusMap[row.status]?.label || row.status }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="baseRentAmount" label="标准租金" width="100">
              <template #default="{ row }">¥{{ (row.baseRentAmount || 0).toLocaleString() }}</template>
            </el-table-column>
            <el-table-column label="操作" fixed="right" width="200">
              <template #default="{ row }">
                <el-button text size="small" type="primary" @click="viewDetail(row)">详情</el-button>
                <el-button text size="small" type="primary" @click="editRoom(row)">编辑</el-button>
                <el-dropdown trigger="click">
                  <el-button text size="small" type="primary">
                    更多<i class="el-icon--right"><ArrowDown /></i>
                  </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item @click="changeStatus(row, 'Vacant')" :disabled="row.status === 'Vacant'">设为空置</el-dropdown-item>
                      <el-dropdown-item @click="changeStatus(row, 'Rented')" :disabled="row.status === 'Rented'">设为已租</el-dropdown-item>
                      <el-dropdown-item @click="changeStatus(row, 'Maintenance')" :disabled="row.status === 'Maintenance'">设为维修</el-dropdown-item>
                      <el-dropdown-item divided @click="handleDelete(row)" style="color: #f56c6c;">删除</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </template>
            </el-table-column>
          </el-table>
          <div style="margin-top: 16px; text-align: right;">
            <el-pagination
              v-model:current-page="pagination.page"
              v-model:page-size="pagination.pageSize"
              :page-sizes="[10, 20, 50]"
              :total="pagination.total"
              layout="total, sizes, prev, pager, next"
              @change="fetchRooms"
            />
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Add Unit Dialog -->
    <el-dialog v-model="showAddUnit" title="新增房源" width="600px">
      <el-form ref="addFormRef" :model="addForm" :rules="addRules" label-width="100px">
        <el-divider content-position="left">座楼信息</el-divider>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="座楼名称" prop="buildingName">
              <el-input v-model="addForm.buildingName" placeholder="如：A栋" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="座楼编码" prop="buildingCode">
              <el-input v-model="addForm.buildingCode" placeholder="如：A" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="座楼地址" prop="buildingAddress">
          <el-input v-model="addForm.buildingAddress" type="textarea" :rows="2" placeholder="座楼地址" />
        </el-form-item>
        <el-divider content-position="left">楼层房号</el-divider>
        <el-row :gutter="16">
          <el-col :span="8">
            <el-form-item label="楼层" prop="floorName">
              <el-input v-model="addForm.floorName" placeholder="如：1层" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="排序值" prop="floorSortOrder">
              <el-input-number v-model="addForm.floorSortOrder" :min="1" :max="999" style="width: 100%;" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="房号" prop="unitNo">
              <el-input v-model="addForm.unitNo" placeholder="如：101" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="完整编码">
          <el-input v-model="addForm.fullCode" placeholder="自动生成，可手动修改" />
        </el-form-item>
        <el-divider content-position="left">房屋属性</el-divider>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="房型">
              <el-select v-model="addForm.roomTypeId" style="width: 100%;" clearable>
                <el-option v-for="rt in roomTypes" :key="rt.id" :label="rt.name" :value="rt.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="朝向">
              <el-select v-model="addForm.orientation" style="width: 100%;" clearable>
                <el-option label="东" value="东" />
                <el-option label="南" value="南" />
                <el-option label="西" value="西" />
                <el-option label="北" value="北" />
                <el-option label="南北通透" value="南北通透" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="面积(m²)">
              <el-input-number v-model="addForm.area" :min="0" :precision="2" style="width: 100%;" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="标准租金(元)">
              <el-input-number v-model="addForm.baseRentAmount" :min="0" :precision="2" style="width: 100%;" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="showAddUnit = false">取消</el-button>
        <el-button type="primary" @click="saveUnit" :loading="savingUnit">保存</el-button>
      </template>
    </el-dialog>

    <!-- Edit Unit Dialog -->
    <el-dialog v-model="showEditUnit" :title="'编辑房源 - ' + editForm.fullCode" width="600px">
      <el-form :model="editForm" label-width="100px">
        <el-divider content-position="left">座楼信息</el-divider>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="座楼名称">
              <el-input v-model="editForm.buildingName" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="座楼编码">
              <el-input v-model="editForm.buildingCode" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="座楼地址">
          <el-input v-model="editForm.buildingAddress" type="textarea" :rows="2" />
        </el-form-item>
        <el-divider content-position="left">楼层房号</el-divider>
        <el-row :gutter="16">
          <el-col :span="8">
            <el-form-item label="楼层">
              <el-input v-model="editForm.floorName" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="排序值">
              <el-input-number v-model="editForm.floorSortOrder" :min="1" :max="999" style="width: 100%;" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="房号">
              <el-input v-model="editForm.unitNo" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">房屋属性</el-divider>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="房型">
              <el-select v-model="editForm.roomTypeId" style="width: 100%;" clearable>
                <el-option v-for="rt in roomTypes" :key="rt.id" :label="rt.name" :value="rt.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="朝向">
              <el-select v-model="editForm.orientation" style="width: 100%;" clearable>
                <el-option label="东" value="东" />
                <el-option label="南" value="南" />
                <el-option label="西" value="西" />
                <el-option label="北" value="北" />
                <el-option label="南北通透" value="南北通透" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="面积(m²)">
              <el-input-number v-model="editForm.area" :min="0" :precision="2" style="width: 100%;" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="标准租金(元)">
              <el-input-number v-model="editForm.baseRentAmount" :min="0" :precision="2" style="width: 100%;" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="showEditUnit = false">取消</el-button>
        <el-button type="primary" @click="saveEdit" :loading="savingEdit">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getHousingUnits, getHousingUnitTree, getHousingUnitStats,
  getRoomTypes, getBuildingList,
  createHousingUnit, updateHousingUnit, deleteHousingUnit
} from '../../api/index'
import { useUserStore } from '../../store/user'

const router = useRouter()
const userStore = useUserStore()

// ==================== State ====================
const buildings = ref([])
const buildingTree = ref([])
const roomList = ref([])
const roomTypes = ref([])
const stats = reactive({ total: 0, vacant: 0, rented: 0, maintenance: 0 })
const treeLoading = ref(false)
const roomLoading = ref(false)
const savingUnit = ref(false)
const savingEdit = ref(false)

const statusMap = {
  Vacant: { label: '空置', type: 'info' },
  Rented: { label: '已租', type: 'success' },
  Maintenance: { label: '维修', type: 'warning' }
}

const search = reactive({ keyword: '', status: '', buildingName: '' })
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })
const showAddUnit = ref(false)
const showEditUnit = ref(false)
const addFormRef = ref(null)

// ==================== Add Form ====================
const defaultAddForm = () => ({
  buildingName: '', buildingCode: '', buildingAddress: '',
  floorName: '', floorSortOrder: 1, unitNo: '', fullCode: '',
  roomTypeId: null, area: 0, orientation: '', baseRentAmount: 0,
  companyId: userStore.currentCompanyId || ''
})
const addForm = reactive(defaultAddForm())
const addRules = {
  buildingName: [{ required: true, message: '请输入座楼名称' }],
  floorName: [{ required: true, message: '请输入楼层' }],
  unitNo: [{ required: true, message: '请输入房号' }]
}

// ==================== Edit Form ====================
const editForm = reactive({
  id: '', fullCode: '',
  buildingName: '', buildingCode: '', buildingAddress: '',
  floorName: '', floorSortOrder: 1, unitNo: '',
  roomTypeId: null, area: 0, orientation: '', baseRentAmount: 0
})

// ==================== Data Fetching ====================
async function fetchStats() {
  try {
    const res = await getHousingUnitStats()
    if (res) { stats.total = res.total || 0; stats.vacant = res.vacant || 0; stats.rented = res.rented || 0; stats.maintenance = res.maintenance || 0 }
  } catch (e) { /* ignore */ }
}

async function fetchBuildingList() {
  try {
    const res = await getBuildingList()
    buildings.value = Array.isArray(res) ? res : []
  } catch (e) { buildings.value = [] }
}

async function fetchUnitTree() {
  treeLoading.value = true
  try {
    const res = await getHousingUnitTree()
    buildingTree.value = Array.isArray(res) ? res : []
  } catch (e) { buildingTree.value = [] }
  treeLoading.value = false
}

async function fetchUnits() {
  roomLoading.value = true
  try {
    const res = await getHousingUnits({
      buildingName: search.buildingName || undefined,
      keyword: search.keyword || undefined,
      status: search.status || undefined
    })
    roomList.value = Array.isArray(res) ? res : []
    pagination.total = roomList.value.length
  } catch (e) {
    roomList.value = []
    pagination.total = 0
  }
  roomLoading.value = false
}

async function fetchRoomTypes() {
  try {
    const res = await getRoomTypes()
    roomTypes.value = Array.isArray(res) ? res : []
  } catch (e) { roomTypes.value = [] }
}

// ==================== Actions ====================
function handleSearch() { pagination.page = 1; fetchUnits() }
function resetSearch() {
  search.keyword = ''; search.status = ''; search.buildingName = ''
  pagination.page = 1; fetchUnits()
}

function handleNodeClick(data) {
  if (data.children && data.children.length > 0) {
    search.buildingName = data.id.startsWith('B:') ? data.id.substring(2) : data.id
    pagination.page = 1; fetchUnits()
  } else {
    viewDetail({ id: data.id })
  }
}

function viewDetail(row) { router.push('/buildings/unit/' + row.id) }

function editRoom(row) {
  Object.assign(editForm, {
    id: row.id, fullCode: row.fullCode || row.unitNo,
    buildingName: row.buildingName || '', buildingCode: row.buildingCode || '',
    buildingAddress: row.buildingAddress || '',
    floorName: row.floorName || '', floorSortOrder: row.floorSortOrder || 1,
    unitNo: row.unitNo || '',
    roomTypeId: row.roomTypeId || null, area: row.area || 0,
    orientation: row.orientation || '', baseRentAmount: row.baseRentAmount || 0
  })
  showEditUnit.value = true
}

// ==================== Add Unit ====================
async function saveUnit() {
  const valid = await addFormRef.value?.validate().catch(() => false)
  if (!valid) return
  savingUnit.value = true
  try {
    const payload = {
      ...addForm,
      fullCode: addForm.fullCode || `${addForm.buildingName}-${addForm.floorName}-${addForm.unitNo}`,
      area: addForm.area || null,
      baseRentAmount: addForm.baseRentAmount || null,
      roomTypeId: addForm.roomTypeId || null,
      orientation: addForm.orientation || null,
      buildingCode: addForm.buildingCode || null,
      buildingAddress: addForm.buildingAddress || null,
      companyId: userStore.currentCompanyId || undefined
    }
    await createHousingUnit(payload)
    ElMessage.success('房源创建成功')
    showAddUnit.value = false
    Object.assign(addForm, defaultAddForm())
    await Promise.all([fetchUnits(), fetchUnitTree(), fetchStats()])
  } catch (e) {
    ElMessage.error(e?.response?.data?.title || '创建失败')
  }
  savingUnit.value = false
}

// ==================== Edit Unit ====================
async function saveEdit() {
  savingEdit.value = true
  try {
    await updateHousingUnit(editForm.id, {
      buildingName: editForm.buildingName || undefined,
      buildingCode: editForm.buildingCode || undefined,
      buildingAddress: editForm.buildingAddress || undefined,
      floorName: editForm.floorName || undefined,
      floorSortOrder: editForm.floorSortOrder || undefined,
      unitNo: editForm.unitNo || undefined,
      roomTypeId: editForm.roomTypeId || null,
      area: editForm.area || null,
      orientation: editForm.orientation || null,
      baseRentAmount: editForm.baseRentAmount || null
    })
    ElMessage.success('房源信息已更新')
    showEditUnit.value = false
    await Promise.all([fetchUnits(), fetchUnitTree()])
  } catch (e) { ElMessage.error('更新失败') }
  savingEdit.value = false
}

// ==================== Status Change ====================
async function changeStatus(row, status) {
  const label = statusMap[status]?.label || status
  try {
    await ElMessageBox.confirm(`确定将 ${row.fullCode || row.unitNo} 的状态改为"${label}"吗？`, '提示')
    await updateHousingUnit(row.id, { status })
    ElMessage.success(`状态已更新为"${label}"`)
    await Promise.all([fetchUnits(), fetchStats()])
  } catch (e) { if (e !== 'cancel') ElMessage.error('状态更新失败') }
}

// ==================== Delete ====================
async function handleDelete(row) {
  try {
    await ElMessageBox.confirm(
      `确定要删除房源 ${row.fullCode || row.unitNo} 吗？此操作不可恢复。`,
      '删除确认',
      { confirmButtonText: '删除', cancelButtonText: '取消', type: 'warning' }
    )
    await deleteHousingUnit(row.id)
    ElMessage.success('已删除')
    await Promise.all([fetchUnits(), fetchUnitTree(), fetchStats()])
  } catch (e) { if (e !== 'cancel') ElMessage.error('删除失败') }
}

// ==================== Lifecycle ====================
onMounted(async () => {
  await Promise.all([fetchBuildingList(), fetchUnitTree(), fetchRoomTypes(), fetchStats()])
  await fetchUnits()
})
</script>

<style scoped>
.header-right { display: flex; align-items: center; gap: 16px; }
.stat-card { text-align: center; padding: 8px 0; }
.stat-value { font-size: 32px; font-weight: bold; line-height: 1.2; }
.stat-label { font-size: 14px; color: #909399; margin-top: 4px; }
</style>
