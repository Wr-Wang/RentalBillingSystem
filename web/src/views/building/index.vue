<template>
  <div>
    <div class="page-header">
      <h2>房屋管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="showAddBuilding = true">
          <el-icon><Plus /></el-icon>新增座楼
        </el-button>
        <el-button @click="$router.push('/buildings/import')">
          <el-icon><Upload /></el-icon>批量导入
        </el-button>
      </div>
    </div>

    <!-- Search -->
    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="搜索房间号/地址" clearable style="width: 200px;" />
      <el-select v-model="search.status" placeholder="房间状态" clearable style="width: 140px;">
        <el-option label="空置" value="Vacant" />
        <el-option label="已租" value="Rented" />
        <el-option label="维修" value="Maintenance" />
      </el-select>
      <el-select v-model="search.buildingId" placeholder="选择座楼" clearable style="width: 140px;">
        <el-option v-for="b in buildings" :key="b.id" :label="b.name" :value="b.id" />
      </el-select>
      <el-button type="primary" @click="handleSearch">查询</el-button>
      <el-button @click="resetSearch">重置</el-button>
    </div>

    <el-row :gutter="16">
      <!-- Building Tree -->
      <el-col :span="6">
        <el-card style="min-height: 500px;">
          <template #header>
            <span>房屋结构</span>
          </template>
          <el-tree
            :data="buildingTree"
            node-key="id"
            :props="{ label: 'name', children: 'children' }"
            @node-click="handleNodeClick"
            :highlight-current="true"
          />
        </el-card>
      </el-col>

      <!-- Room List -->
      <el-col :span="18">
        <el-card>
          <template #header>
            <span>房间列表</span>
          </template>
          <el-table :data="roomList" stripe style="width: 100%">
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="fullCode" label="房间编号" width="140" />
            <el-table-column prop="buildingName" label="座楼" width="100" />
            <el-table-column prop="floorNo" label="楼层" width="70" />
            <el-table-column prop="roomNo" label="房间号" width="80" />
            <el-table-column prop="roomTypeName" label="房型" width="100" />
            <el-table-column prop="area" label="面积(m²)" width="90" />
            <el-table-column prop="status" label="状态" width="90">
              <template #default="{ row }">
                <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
                  {{ statusMap[row.status]?.label || row.status }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="standardRent" label="标准租金" width="100">
              <template #default="{ row }">¥{{ row.standardRent?.toLocaleString() || '-' }}</template>
            </el-table-column>
            <el-table-column label="操作" fixed="right" width="180">
              <template #default="{ row }">
                <el-button text size="small" type="primary" @click="viewRoom(row)">详情</el-button>
                <el-button text size="small" type="primary" @click="editRoom(row)">编辑</el-button>
                <el-dropdown trigger="click">
                  <el-button text size="small" type="primary">状态<i class="el-icon--right"><ArrowDown /></i></el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item @click="changeStatus(row, 'Vacant')">空置</el-dropdown-item>
                      <el-dropdown-item @click="changeStatus(row, 'Rented')">已租</el-dropdown-item>
                      <el-dropdown-item @click="changeStatus(row, 'Maintenance')">维修</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </template>
            </el-table-column>
          </el-table>
          <div style="margin-top: 16px; text-align: right;">
            <el-pagination
              v-model:page-size="pagination.pageSize"
              :page-sizes="[10, 20, 50]"
              :total="pagination.total"
              layout="total, sizes, prev, pager, next"
            />
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Add Building Dialog -->
    <el-dialog v-model="showAddBuilding" title="新增座楼" width="500px">
      <el-form ref="buildingFormRef" :model="buildingForm" :rules="buildingRules" label-width="100px">
        <el-form-item label="座楼名称" prop="name">
          <el-input v-model="buildingForm.name" placeholder="如：A栋" />
        </el-form-item>
        <el-form-item label="编码" prop="code">
          <el-input v-model="buildingForm.code" placeholder="如：A" />
        </el-form-item>
        <el-form-item label="地址" prop="address">
          <el-input v-model="buildingForm.address" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="物业类型" prop="propertyType">
          <el-select v-model="buildingForm.propertyType" style="width: 100%">
            <el-option label="住宅" value="Residential" />
            <el-option label="商业" value="Commercial" />
            <el-option label="混合" value="Mixed" />
          </el-select>
        </el-form-item>
        <el-form-item label="楼层数">
          <el-input-number v-model="buildingForm.floorCount" :min="1" :max="100" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddBuilding = false">取消</el-button>
        <el-button type="primary" @click="saveBuilding">保存</el-button>
      </template>
    </el-dialog>

    <!-- Edit Room Dialog -->
    <el-dialog v-model="showEditRoom" :title="'编辑房间 - ' + editRoomForm.fullCode" width="500px">
      <el-form :model="editRoomForm" label-width="100px">
        <el-form-item label="房型">
          <el-select v-model="editRoomForm.roomTypeId" style="width: 100%">
            <el-option v-for="rt in roomTypes" :key="rt.id" :label="rt.name" :value="rt.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="面积(m²)">
          <el-input-number v-model="editRoomForm.area" :min="0" :precision="2" style="width: 100%" />
        </el-form-item>
        <el-form-item label="朝向">
          <el-select v-model="editRoomForm.orientation" style="width: 100%">
            <el-option label="东" value="东" />
            <el-option label="南" value="南" />
            <el-option label="西" value="西" />
            <el-option label="北" value="北" />
            <el-option label="南北通透" value="南北通透" />
          </el-select>
        </el-form-item>
        <el-form-item label="标准租金">
          <el-input-number v-model="editRoomForm.standardRent" :min="0" :precision="2" style="width: 100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showEditRoom = false">取消</el-button>
        <el-button type="primary" @click="saveRoom">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()

// Mock data
const buildings = ref([
  { id: 'b1', name: 'A栋', code: 'A', address: '北京市朝阳区xx路1号', floorCount: 18 },
  { id: 'b2', name: 'B栋', code: 'B', address: '北京市朝阳区xx路2号', floorCount: 22 },
  { id: 'b3', name: 'C栋', code: 'C', address: '北京市朝阳区xx路3号', floorCount: 15 }
])

const buildingTree = ref([
  {
    id: 'b1', name: 'A栋', children: [
      { id: 'b1-f1', name: '1层', children: [{ id: 'b1-f1-r1', name: '101室' }, { id: 'b1-f1-r2', name: '102室' }] },
      { id: 'b1-f2', name: '2层', children: [{ id: 'b1-f2-r1', name: '201室' }, { id: 'b1-f2-r2', name: '202室' }] },
      { id: 'b1-f3', name: '3层', children: [{ id: 'b1-f3-r1', name: '301室' }, { id: 'b1-f3-r2', name: '302室' }] }
    ]
  },
  {
    id: 'b2', name: 'B栋', children: [
      { id: 'b2-f1', name: '1层', children: [{ id: 'b2-f1-r1', name: '101室' }, { id: 'b2-f1-r2', name: '102室' }] },
      { id: 'b2-f2', name: '2层', children: [{ id: 'b2-f2-r1', name: '201室' }] }
    ]
  }
])

const roomList = ref([
  { id: 'r1', fullCode: 'A-1-101', buildingName: 'A栋', floorNo: 1, roomNo: '101', roomTypeName: '两室一厅', area: 85, status: 'Rented', standardRent: 5200 },
  { id: 'r2', fullCode: 'A-1-102', buildingName: 'A栋', floorNo: 1, roomNo: '102', roomTypeName: '一室一厅', area: 55, status: 'Vacant', standardRent: 3800 },
  { id: 'r3', fullCode: 'A-2-201', buildingName: 'A栋', floorNo: 2, roomNo: '201', roomTypeName: '三室一厅', area: 120, status: 'Rented', standardRent: 6800 },
  { id: 'r4', fullCode: 'A-2-202', buildingName: 'A栋', floorNo: 2, roomNo: '202', roomTypeName: '两室一厅', area: 88, status: 'Maintenance', standardRent: 5000 },
  { id: 'r5', fullCode: 'B-1-101', buildingName: 'B栋', floorNo: 1, roomNo: '101', roomTypeName: '开间', area: 35, status: 'Vacant', standardRent: 2500 }
])

const roomTypes = ref([
  { id: 'rt1', name: '开间/单间', code: 'STUDIO' },
  { id: 'rt2', name: '一室一厅', code: 'ONE_BR_ONE_LR' },
  { id: 'rt3', name: '两室一厅', code: 'TWO_BR_ONE_LR' },
  { id: 'rt4', name: '三室一厅', code: 'THREE_BR_ONE_LR' }
])

const statusMap = {
  'Vacant': { label: '空置', type: 'info' },
  'Rented': { label: '已租', type: 'success' },
  'Maintenance': { label: '维修', type: 'warning' }
}

const search = reactive({ keyword: '', status: '', buildingId: '' })
const pagination = reactive({ pageSize: 10, total: 50 })

const showAddBuilding = ref(false)
const showEditRoom = ref(false)
const buildingForm = reactive({ name: '', code: '', address: '', propertyType: 'Residential', floorCount: 6 })
const buildingRules = { name: [{ required: true, message: '请输入座楼名称' }], code: [{ required: true, message: '请输入编码' }] }
const editRoomForm = reactive({ id: '', fullCode: '', roomTypeId: '', area: 0, orientation: '南', standardRent: 0 })

function handleSearch() { ElMessage.info('搜索功能待API接入') }
function resetSearch() { search.keyword = ''; search.status = ''; search.buildingId = '' }

function handleNodeClick(data) {
  if (data.children) return
  ElMessage.info('选中: ' + data.name)
}

function viewRoom(row) { router.push('/buildings/room/' + row.id) }
function editRoom(row) {
  Object.assign(editRoomForm, {
    id: row.id, fullCode: row.fullCode, roomTypeId: row.roomTypeId || '', area: row.area,
    orientation: row.orientation || '南', standardRent: row.standardRent || 0
  })
  showEditRoom.value = true
}
function saveBuilding() { ElMessage.success('座楼创建成功'); showAddBuilding.value = false }
function saveRoom() { ElMessage.success('房间信息已更新'); showEditRoom.value = false }

function changeStatus(row, status) {
  const label = statusMap[status]?.label || status
  ElMessageBox.confirm(`确定将房间 ${row.fullCode} 的状态改为"${label}"吗？`, '提示').then(() => {
    row.status = status
    ElMessage.success(`状态已更新为"${label}"`)
  }).catch(() => {})
}
</script>
