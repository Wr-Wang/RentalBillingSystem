<template>
  <div>
    <div class="page-header">
      <h2>抄表管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="showBatchImport = true">
          <el-icon><Upload /></el-icon>Excel批量导入
        </el-button>
        <el-button @click="estimateAll" :loading="estimating">逾期估读</el-button>
        <el-button @click="fetchList"><el-icon><Refresh /></el-icon>刷新</el-button>
      </div>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="search.month" type="month" placeholder="选择月份" @change="fetchList" />
      <el-select v-model="search.feeCode" placeholder="费用类型" clearable style="width: 140px;" @change="fetchList">
        <el-option label="水费" value="WATER" />
        <el-option label="电费" value="ELECTRIC" />
        <el-option label="燃气费" value="GAS" />
      </el-select>
      <el-input v-model="search.keyword" placeholder="房屋/合同号" clearable style="width: 180px;" @clear="fetchList" @keyup.enter="fetchList" />
      <el-button type="primary" @click="fetchList">查询</el-button>
    </div>

    <el-row :gutter="16" style="margin-bottom: 16px;">
      <el-col :span="6"><el-statistic title="总户数" :value="stats.total" /></el-col>
      <el-col :span="6"><el-statistic title="待录入" :value="stats.draft" /></el-col>
      <el-col :span="6"><el-statistic title="已录入" :value="stats.confirmed" /></el-col>
      <el-col :span="6"><el-statistic title="已估读" :value="stats.estimated" /></el-col>
    </el-row>

    <el-card>
      <el-table :data="filteredList" v-loading="loading" stripe>
        <el-table-column prop="roomName" label="房屋" width="100" />
        <el-table-column prop="contractNo" label="合同号" width="200" />
        <el-table-column prop="feeName" label="项目" width="80" />
        <el-table-column prop="previousReading" label="上期读数" width="100" />
        <el-table-column label="本期读数">
          <template #default="{ row }">
            <el-input-number v-if="row.status === 'Draft'" v-model="row.currentReading" :min="0" :precision="2" size="small" style="width: 120px;" />
            <span v-else>{{ row.currentReading }}</span>
          </template>
        </el-table-column>
        <el-table-column label="用量" width="80">
          <template #default="{ row }">
            {{ row.currentReading != null && row.previousReading != null ? (row.currentReading - row.previousReading).toFixed(2) : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button v-if="row.status === 'Draft'" text size="small" type="primary" :loading="row._saving" @click="saveReading(row)">保存</el-button>
            <el-button v-if="row.status === 'Confirmed' || row.status === 'Estimated'" text size="small" type="primary" @click="confirmReading(row)">确认</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Excel Import Dialog -->
    <el-dialog :draggable="true" v-model="showBatchImport" title="Excel批量导入" width="550px">
      <el-alert title="请上传标准抄表模板的 Excel 文件（.xlsx）" type="info" show-icon :closable="false" style="margin-bottom:16px;" />
      <p style="color:#909399;font-size:13px;margin-bottom:12px;">
        模板格式：第一行为表头，包含列：房屋编码、费用类型、上期读数、本期读数
      </p>
      <input ref="fileInputRef" type="file" accept=".xlsx,.xls" @change="onFileChange" style="margin-bottom:12px;" />
      <div v-if="parsedData.length > 0" style="margin-bottom:12px;">
        <p style="color:#67c23a;font-weight:600;">已解析 {{ parsedData.length }} 条记录</p>
        <el-table :data="parsedData.slice(0, 5)" size="small" stripe max-height="200">
          <el-table-column prop="idx" label="#" width="40" />
          <el-table-column label="房屋" prop="roomCode" width="100" />
          <el-table-column label="费用" prop="feeType" width="80" />
          <el-table-column label="上期" prop="prev" width="80" />
          <el-table-column label="本期" prop="curr" width="80" />
        </el-table>
        <p v-if="parsedData.length > 5" style="color:#909399;font-size:12px;">...还有 {{ parsedData.length - 5 }} 条</p>
      </div>
      <template #footer>
        <el-button @click="showBatchImport = false; parsedData = []">取消</el-button>
        <el-button type="primary" :loading="importing" :disabled="parsedData.length === 0" @click="submitImport">
          导入 {{ parsedData.length > 0 ? '(' + parsedData.length + '条)' : '' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/user'
import { chinaTime } from '@/utils/chinaTime'
import { getMeterReadings, createMeterReading, updateMeterReading, confirmMeterReading, importMeterReadings } from '@/api'
import * as XLSX from 'xlsx'

const userStore = useUserStore()
const search = reactive({ month: chinaTime.now(), feeCode: '', keyword: '' })
const meterReadings = ref([])
const loading = ref(false)
const showBatchImport = ref(false)
const importing = ref(false)
const estimating = ref(false)
const fileInputRef = ref(null)
const parsedData = ref([])

function getEffectiveCompanyId() {
  return userStore.effectiveCompanyId
}
function statusType(s) {
  return { Draft: 'info', Confirmed: 'primary', Billed: 'success', Estimated: 'warning' }[s] || 'info'
}
function statusLabel(s) {
  return { Draft: '待录入', Confirmed: '已录入', Billed: '已出账', Estimated: '已估读' }[s] || s
}

const stats = computed(() => {
  const list = filteredList.value
  return { total: list.length, draft: list.filter(r => r.status === 'Draft').length, confirmed: list.filter(r => r.status === 'Confirmed').length, estimated: list.filter(r => r.status === 'Estimated').length }
})

const filteredList = computed(() => {
  let list = meterReadings.value
  if (search.feeCode) list = list.filter(r => r.feeCode === search.feeCode)
  if (search.keyword) { const kw = search.keyword.toLowerCase(); list = list.filter(r => (r.roomName || '').toLowerCase().includes(kw) || (r.contractNo || '').toLowerCase().includes(kw)) }
  return list
})

async function fetchList() {
  const companyId = getEffectiveCompanyId()
  if (!companyId) { meterReadings.value = []; return }
  loading.value = true
  try {
    const dt = search.month || chinaTime.now()
    const year = dt.getFullYear(); const month = dt.getMonth() + 1
    const res = await getMeterReadings({ companyId, year, month })
    let items = Array.isArray(res) ? res : (res.items || res.data || res || [])
    meterReadings.value = items.map(r => ({
      id: r.id, contractFeeConfigId: r.contractFeeConfigId,
      contractNo: r.contractNo || '', roomName: r.roomName || '',
      feeName: r.feeName || '', feeCode: r.feeCodeId || '',
      previousReading: r.previousReading || 0,
      currentReading: r.currentReading != null ? r.currentReading : null,
      status: r.status || 'Draft', year: r.year, month: r.month, _saving: false
    }))
  } catch { ElMessage.error('加载抄表数据失败') }
  loading.value = false
}

async function saveReading(row) {
  if (row.currentReading == null || row.currentReading < 0) { ElMessage.warning('请输入有效读数'); return }
  row._saving = true
  try {
    await createMeterReading({
      contractFeeConfigId: row.contractFeeConfigId,
      year: row.year, month: row.month,
      previousReading: row.previousReading,
      currentReading: row.currentReading
    })
    row.status = 'Confirmed'
    ElMessage.success('读数已保存')
  } catch { ElMessage.error('保存失败') }
  row._saving = false
}

async function confirmReading(row) {
  try { await confirmMeterReading(row.id); row.status = 'Confirmed'; ElMessage.success('读数已确认') }
  catch { ElMessage.error('确认失败') }
}

// ===== Excel 批量导入 =====
function onFileChange(e) {
  const file = e.target.files[0]
  if (!file) { parsedData.value = []; return }
  const reader = new FileReader()
  reader.onload = (ev) => {
    try {
      const data = new Uint8Array(ev.target.result)
      const workbook = XLSX.read(data, { type: 'array' })
      const sheet = workbook.Sheets[workbook.SheetNames[0]]
      const json = XLSX.utils.sheet_to_json(sheet, { header: 1 })
      if (json.length < 2) { ElMessage.warning('文件为空或格式不正确'); return }

      // 跳过表头行，解析数据
      const rows = []
      for (let i = 1; i < json.length; i++) {
        const row = json[i]
        if (!row || !row[0]) continue
        rows.push({ idx: i, roomCode: String(row[0] || ''), feeType: String(row[1] || ''), prev: Number(row[2]) || 0, curr: Number(row[3]) || 0 })
      }
      parsedData.value = rows
      if (rows.length > 0) ElMessage.success(`已解析 ${rows.length} 条记录，请确认后点击导入`)
    } catch { ElMessage.error('文件解析失败，请检查格式'); parsedData.value = [] }
  }
  reader.readAsArrayBuffer(file)
}

async function submitImport() {
  if (parsedData.value.length === 0) { ElMessage.warning('请先选择文件'); return }
  importing.value = true
  try {
    // 构造 MeterReading 数据发送到后端
    const readings = parsedData.value.map(p => ({
      contractFeeConfigId: p.roomCode, // 导入时用房屋编码对应，由后端解析
      year: (search.month || chinaTime.now()).getFullYear(),
      month: (search.month || chinaTime.now()).getMonth() + 1,
      previousReading: p.prev,
      currentReading: p.curr
    }))
    const res = await importMeterReadings(readings)
    ElMessage.success(`导入完成：${res.imported || 0}/${readings.length} 条`)
    showBatchImport.value = false
    parsedData.value = []
    if (fileInputRef.value) fileInputRef.value.value = ''
    await fetchList()
  } catch { ElMessage.error('导入失败') }
  importing.value = false
}

// ===== 逾期估读 =====
async function estimateAll() {
  estimating.value = true
  try {
    // 将所有 Draft 状态的记录标记为 Estimated
    const drafts = meterReadings.value.filter(r => r.status === 'Draft')
    if (drafts.length === 0) { ElMessage.info('没有待录入的读数'); return }
    let count = 0
    for (const row of drafts) {
      try {
        // 使用上期读数作为本期读数（估读）
        await createMeterReading({
          contractFeeConfigId: row.contractFeeConfigId,
          year: row.year, month: row.month,
          previousReading: row.previousReading,
          currentReading: row.previousReading // 估读 = 上期读数
        })
        row.currentReading = row.previousReading
        row.status = 'Estimated'
        count++
      } catch { /* skip */ }
    }
    ElMessage.success(`逾期估读完成，共处理 ${count} 条`)
    await fetchList()
  } catch { ElMessage.error('估读失败') }
  estimating.value = false
}

onMounted(() => { if (getEffectiveCompanyId()) fetchList() })
</script>
