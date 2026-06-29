<template>
  <div>
    <div class="page-header">
      <h2>API 调用日志</h2>
      <div style="display:flex;gap:8px;">
        <el-button size="small" @click="handleExport">导出</el-button>
        <el-button type="danger" size="small" @click="handleClearRange">按日期删除</el-button>
        <el-button type="danger" size="small" @click="handleClearAll">清空全部</el-button>
        <el-button @click="fetchList" :loading="loading">刷新</el-button>
      </div>
    </div>

    <el-card shadow="never" class="search-bar">
      <el-form :inline="true">
        <el-form-item label="方法">
          <el-select v-model="filter.method" placeholder="全部" clearable style="width:100px" @change="fetchList">
            <el-option label="GET" value="GET" />
            <el-option label="POST" value="POST" />
            <el-option label="PUT" value="PUT" />
            <el-option label="DELETE" value="DELETE" />
          </el-select>
        </el-form-item>
        <el-form-item label="路径">
          <el-input v-model="filter.path" placeholder="路径关键字" clearable style="width:180px" @keyup.enter="fetchList" />
        </el-form-item>
        <el-form-item label="状态码">
          <el-input v-model="filter.statusCode" placeholder="如 200/500" clearable style="width:120px" @keyup.enter="fetchList" />
        </el-form-item>
        <el-form-item label="开始时间">
          <el-date-picker v-model="filter.startDate" type="datetime" placeholder="开始" value-format="YYYY-MM-DDTHH:mm" style="width:170px" @change="fetchList" />
        </el-form-item>
        <el-form-item label="结束时间">
          <el-date-picker v-model="filter.endDate" type="datetime" placeholder="结束" value-format="YYYY-MM-DDTHH:mm" style="width:170px" @change="fetchList" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card>
      <el-table :data="list" v-loading="loading" stripe @row-click="showDetail">
        <el-table-column label="方法" width="80">
          <template #default="{ row }">
            <el-tag :type="row.httpMethod === 'GET' ? '' : row.httpMethod === 'POST' ? 'success' : row.httpMethod === 'PUT' ? 'warning' : 'danger'" size="small">
              {{ row.httpMethod }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="path" label="路径" min-width="250" show-overflow-tooltip />
        <el-table-column label="状态" width="70">
          <template #default="{ row }">
            <el-tag :type="row.statusCode < 300 ? 'success' : row.statusCode < 400 ? 'warning' : 'danger'" size="small">
              {{ row.statusCode }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="durationMs" label="耗时(ms)" width="90" sortable="custom" />
        <el-table-column prop="userDisplayName" label="用户" width="100" show-overflow-tooltip />
        <el-table-column prop="ipAddress" label="IP" width="130" />
        <el-table-column prop="createdAt" label="时间" width="160">
          <template #default="{ row }">{{ formatTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="80" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="danger" @click.stop="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100, 200]"
          layout="total, sizes, prev, pager, next"
          @change="fetchList"
        />
      </div>
    </el-card>

    <!-- 详情 Dialog -->
    <el-dialog v-model="showDetailDialog" title="API 调用详情" width="900px">
      <template v-if="detail">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="方法">
            <el-tag :type="detail.httpMethod === 'GET' ? '' : 'success'" size="small">{{ detail.httpMethod }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="时间">{{ formatTime(detail.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="路径" :span="2">{{ detail.path }}</el-descriptions-item>
          <el-descriptions-item label="状态码">
            <el-tag :type="detail.statusCode < 300 ? 'success' : 'danger'" size="small">{{ detail.statusCode }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="耗时">{{ detail.durationMs }}ms</el-descriptions-item>
          <el-descriptions-item label="用户">{{ detail.userDisplayName || '-' }} ({{ detail.userId || '未登录' }})</el-descriptions-item>
          <el-descriptions-item label="IP">{{ detail.ipAddress || '-' }}</el-descriptions-item>
          <el-descriptions-item label="User-Agent" :span="2">{{ detail.userAgent || '-' }}</el-descriptions-item>
        </el-descriptions>
        <el-divider />
        <h4>查询参数</h4>
        <pre v-if="detail.queryString" style="background:#f5f7fa;padding:12px;border-radius:4px;white-space:pre-wrap;font-size:13px;">{{ detail.queryString }}</pre>
        <span v-else style="color:#909399;font-size:13px;">无</span>
        <h4>请求体</h4>
        <pre v-if="detail.requestBody" style="background:#f5f7fa;padding:12px;border-radius:4px;white-space:pre-wrap;font-size:13px;max-height:300px;overflow:auto;">{{ formatJson(detail.requestBody) }}</pre>
        <span v-else style="color:#909399;font-size:13px;">无</span>
        <h4>响应体</h4>
        <pre v-if="detail.responseBody" style="background:#f0f9eb;padding:12px;border-radius:4px;white-space:pre-wrap;font-size:13px;max-height:300px;overflow:auto;">{{ formatJson(detail.responseBody) }}</pre>
        <span v-else style="color:#909399;font-size:13px;">无</span>
      </template>
      <template #footer>
        <el-button @click="showDetailDialog = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 按日期删除 Dialog -->
    <el-dialog v-model="clearDialogVisible" title="按日期范围删除" width="450px">
      <el-form label-width="100px">
        <el-form-item label="开始时间">
          <el-date-picker v-model="clearRange.startDate" type="datetime" placeholder="留空则不限" value-format="YYYY-MM-DDTHH:mm" style="width:100%" />
        </el-form-item>
        <el-form-item label="结束时间">
          <el-date-picker v-model="clearRange.endDate" type="datetime" placeholder="留空则不限" value-format="YYYY-MM-DDTHH:mm" style="width:100%" />
        </el-form-item>
      </el-form>
      <div style="font-size:12px;color:#909399;margin-bottom:12px;">留空开始/结束时间将删除该范围之外的全部日志</div>
      <template #footer>
        <el-button @click="clearDialogVisible = false">取消</el-button>
        <el-button type="danger" @click="confirmClearRange">确认删除</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getApiLogs, getApiLog, deleteApiLog, clearApiLogs } from '../../../api/index'

const loading = ref(false)
const list = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const filter = reactive({ method: '', path: '', statusCode: '', startDate: '', endDate: '' })

const showDetailDialog = ref(false)
const detail = ref(null)

const clearDialogVisible = ref(false)
const clearRange = reactive({ startDate: '', endDate: '' })

async function fetchList() {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: pageSize.value }
    if (filter.method) params.method = filter.method
    if (filter.path) params.path = filter.path
    if (filter.statusCode) params.statusCode = parseInt(filter.statusCode)
    if (filter.startDate) params.startDate = filter.startDate
    if (filter.endDate) params.endDate = filter.endDate
    const res = await getApiLogs(params)
    list.value = res.items || []
    total.value = res.total || 0
  } catch (e) {
    list.value = []
  }
  loading.value = false
}

async function showDetail(row) {
  try {
    const res = await getApiLog(row.id)
    detail.value = res
    showDetailDialog.value = true
  } catch (e) {
    ElMessage.error('获取详情失败')
  }
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm('确定删除此日志？', '提示', { type: 'warning' })
    await deleteApiLog(row.id)
    ElMessage.success('已删除')
    await fetchList()
  } catch (e) { /* cancelled */ }
}

async function handleClearAll() {
  try {
    await ElMessageBox.confirm(`确定清空全部 ${total.value} 条 API 日志？此操作不可恢复！`, '警告', { type: 'warning', confirmButtonText: '确认清空', confirmButtonClass: 'el-button--danger' })
    await clearApiLogs()
    ElMessage.success('已清空')
    await fetchList()
  } catch (e) { /* cancelled */ }
}

function handleClearRange() {
  clearRange.startDate = ''
  clearRange.endDate = ''
  clearDialogVisible.value = true
}

async function confirmClearRange() {
  try {
    const params = {}
    if (clearRange.startDate) params.startDate = clearRange.startDate
    if (clearRange.endDate) params.endDate = clearRange.endDate
    await clearApiLogs(params)
    clearDialogVisible.value = false
    ElMessage.success('已删除')
    await fetchList()
  } catch (e) { /* cancelled */ }
}

function handleExport() {
  ElMessage.info('导出功能开发中')
}

function formatJson(str) {
  if (!str) return ''
  try {
    return JSON.stringify(JSON.parse(str), null, 2)
  } catch {
    return str
  }
}

function formatTime(d) {
  if (!d) return ''
  const dt = new Date(d)
  return `${dt.getFullYear()}-${String(dt.getMonth()+1).padStart(2,'0')}-${String(dt.getDate()).padStart(2,'0')} ${String(dt.getHours()).padStart(2,'0')}:${String(dt.getMinutes()).padStart(2,'0')}`
}

onMounted(() => fetchList())
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.search-bar { margin-bottom: 16px; }
.pagination-wrap { margin-top: 16px; display: flex; justify-content: flex-end; }
pre { margin: 8px 0; }
h4 { margin: 16px 0 4px; }
</style>
