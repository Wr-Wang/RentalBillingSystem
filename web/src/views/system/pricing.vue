<template>
  <div>
    <div class="page-header"><h2>定价标准管理</h2></div>

    <el-tabs v-model="activeTab">
      <!-- Tab 1: 定价标准 -->
      <el-tab-pane label="定价标准" name="pricing">
        <div class="table-actions">
          <el-button type="primary" @click="openCreatePricing"><el-icon><Plus /></el-icon>新增定价</el-button>
        </div>
        <el-card shadow="never">
          <el-table :data="pricingList" stripe v-loading="pricingLoading" style="width:100%">
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="roomTypeName" label="房型" width="120" />
            <el-table-column prop="floorLevelBandName" label="楼层级别" width="100" />
            <el-table-column label="标准租金" width="120">
              <template #default="{ row }">¥{{ row.rentAmount?.toLocaleString() }}</template>
            </el-table-column>
            <el-table-column label="操作" width="160" fixed="right">
              <template #default="{ row }">
                <el-button text size="small" type="primary" @click="openEditPricing(row)">编辑</el-button>
                <el-button text size="small" type="danger" @click="handleDeletePricing(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-tab-pane>

      <!-- Tab 2: 楼层级别 -->
      <el-tab-pane label="楼层级别" name="bands">
        <div class="table-actions">
          <el-button type="primary" @click="openCreateBand"><el-icon><Plus /></el-icon>新增级别</el-button>
        </div>
        <el-card shadow="never">
          <el-table :data="bandList" stripe v-loading="bandLoading" style="width:100%">
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="name" label="名称" width="120" />
            <el-table-column label="楼层范围" width="150">
              <template #default="{ row }">{{ row.minLevel }}F - {{ row.maxLevel }}F</template>
            </el-table-column>
            <el-table-column prop="description" label="说明" min-width="200" />
            <el-table-column label="操作" width="160" fixed="right">
              <template #default="{ row }">
                <el-button text size="small" type="primary" @click="openEditBand(row)">编辑</el-button>
                <el-button text size="small" type="danger" @click="handleDeleteBand(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-tab-pane>
    </el-tabs>

    <!-- 定价 Dialog -->
    <el-dialog v-model="showPricing" :title="isEditPricing ? '编辑定价' : '新增定价'" width="500px">
      <el-form :model="pricingForm" label-width="100px" :rules="pricingRules" ref="pricingFormRef">
        <el-form-item label="房型" prop="roomTypeId">
          <el-select v-model="pricingForm.roomTypeId" style="width:100%">
            <el-option v-for="r in roomTypeList" :key="r.id" :label="r.name" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="楼层级别" prop="floorLevelBandId">
          <el-select v-model="pricingForm.floorLevelBandId" style="width:100%">
            <el-option v-for="f in bandList" :key="f.id" :label="f.name" :value="f.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="标准租金" prop="rentAmount">
          <el-input-number v-model="pricingForm.rentAmount" :min="0" :precision="2" style="width:100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showPricing = false">取消</el-button>
        <el-button type="primary" :loading="savingPricing" @click="savePricing">保存</el-button>
      </template>
    </el-dialog>

    <!-- 楼层级别 Dialog -->
    <el-dialog v-model="showBand" :title="isEditBand ? '编辑级别' : '新增级别'" width="500px">
      <el-form :model="bandForm" label-width="100px" :rules="bandRules" ref="bandFormRef">
        <el-form-item label="名称" prop="name">
          <el-input v-model="bandForm.name" />
        </el-form-item>
        <el-form-item label="最小楼层" prop="minLevel">
          <el-input-number v-model="bandForm.minLevel" :min="1" :max="99" style="width:100%" />
        </el-form-item>
        <el-form-item label="最大楼层" prop="maxLevel">
          <el-input-number v-model="bandForm.maxLevel" :min="1" :max="99" style="width:100%" />
        </el-form-item>
        <el-form-item label="说明">
          <el-input v-model="bandForm.description" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showBand = false">取消</el-button>
        <el-button type="primary" :loading="savingBand" @click="saveBand">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getPricingStandards, createPricingStandard, updatePricingStandard, deletePricingStandard, getRoomTypes, getFloorLevelBands, createFloorLevelBand, updateFloorLevelBand, deleteFloorLevelBand } from '../../api/index'

const activeTab = ref('pricing')

// Pricing
const pricingLoading = ref(false); const pricingList = ref([]); const roomTypeList = ref([])
const showPricing = ref(false); const isEditPricing = ref(false); const savingPricing = ref(false); const pricingFormRef = ref(null)
const pricingForm = ref({ id: null, roomTypeId: '', floorLevelBandId: '', rentAmount: 0 })
const pricingRules = { roomTypeId: [{ required: true, message: '请选择房型', trigger: 'change' }], floorLevelBandId: [{ required: true, message: '请选择楼层级别', trigger: 'change' }], rentAmount: [{ required: true, message: '请输入租金', trigger: 'blur' }] }

async function fetchPricing() { pricingLoading.value = true; try { const r = await getPricingStandards(); pricingList.value = Array.isArray(r) ? r : [] } catch { pricingList.value = [] }; pricingLoading.value = false }
async function fetchRoomTypes() { try { const r = await getRoomTypes(); roomTypeList.value = Array.isArray(r) ? r : [] } catch { roomTypeList.value = [] } }

function openCreatePricing() { isEditPricing.value = false; pricingForm.value = { id: null, roomTypeId: '', floorLevelBandId: '', rentAmount: 0 }; showPricing.value = true }
function openEditPricing(row) { isEditPricing.value = true; pricingForm.value = { id: row.id, roomTypeId: row.roomTypeId, floorLevelBandId: row.floorLevelBandId, rentAmount: row.rentAmount }; showPricing.value = true }
async function savePricing() {
  if (!pricingFormRef.value) return; if (!(await pricingFormRef.value.validate().catch(() => false))) return; savingPricing.value = true
  try {
    if (isEditPricing.value) { await updatePricingStandard(pricingForm.value.id, { rentAmount: pricingForm.value.rentAmount }); ElMessage.success('已更新') }
    else { await createPricingStandard({ roomTypeId: pricingForm.value.roomTypeId, floorLevelBandId: pricingForm.value.floorLevelBandId, rentAmount: pricingForm.value.rentAmount }); ElMessage.success('已创建') }
    showPricing.value = false; await fetchPricing()
  } catch { ElMessage.error(isEditPricing.value ? '更新失败' : '创建失败') }
  savingPricing.value = false
}
async function handleDeletePricing(row) { try { await ElMessageBox.confirm('确定删除？', '提示', { type: 'warning' }); await deletePricingStandard(row.id); ElMessage.success('已删除'); await fetchPricing() } catch (e) { } }

// Bands
const bandLoading = ref(false); const bandList = ref([])
const showBand = ref(false); const isEditBand = ref(false); const savingBand = ref(false); const bandFormRef = ref(null)
const bandForm = ref({ id: null, name: '', minLevel: 1, maxLevel: 99, description: '' })
const bandRules = { name: [{ required: true, message: '请输入名称', trigger: 'blur' }], minLevel: [{ required: true, message: '请输入最小楼层', trigger: 'blur' }], maxLevel: [{ required: true, message: '请输入最大楼层', trigger: 'blur' }] }

async function fetchBands() { bandLoading.value = true; try { const r = await getFloorLevelBands(); bandList.value = Array.isArray(r) ? r : [] } catch { bandList.value = [] }; bandLoading.value = false }

function openCreateBand() { isEditBand.value = false; bandForm.value = { id: null, name: '', minLevel: 1, maxLevel: 99, description: '' }; showBand.value = true }
function openEditBand(row) { isEditBand.value = true; bandForm.value = { id: row.id, name: row.name, minLevel: row.minLevel, maxLevel: row.maxLevel, description: row.description || '' }; showBand.value = true }
async function saveBand() {
  if (!bandFormRef.value) return; if (!(await bandFormRef.value.validate().catch(() => false))) return; savingBand.value = true
  try {
    if (isEditBand.value) { await updateFloorLevelBand(bandForm.value.id, { name: bandForm.value.name, minLevel: bandForm.value.minLevel, maxLevel: bandForm.value.maxLevel, description: bandForm.value.description || undefined }); ElMessage.success('已更新') }
    else { await createFloorLevelBand({ name: bandForm.value.name, minLevel: bandForm.value.minLevel, maxLevel: bandForm.value.maxLevel, description: bandForm.value.description || undefined }); ElMessage.success('已创建') }
    showBand.value = false; await fetchBands()
  } catch { ElMessage.error(isEditBand.value ? '更新失败' : '创建失败') }
  savingBand.value = false
}
async function handleDeleteBand(row) { try { await ElMessageBox.confirm(`确定删除「${row.name}」？`, '提示', { type: 'warning' }); await deleteFloorLevelBand(row.id); ElMessage.success('已删除'); await fetchBands() } catch (e) { } }

const fetchData = () => { fetchPricing(); fetchBands(); fetchRoomTypes() }
onMounted(() => fetchData())
</script>
<style scoped>
.table-actions { margin-bottom: 12px; }
</style>
