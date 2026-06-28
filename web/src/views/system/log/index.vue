<template>
  <div>
    <div class="page-header">
      <h2>系统日志</h2>
      <div>
        <el-button type="danger" size="small" @click="handleClearAll">清空全部</el-button>
        <el-button @click="fetchList" :loading="loading">刷新</el-button>
      </div>
    </div>

    <el-card shadow="never" class="search-bar">
      <el-form :inline="true">
        <el-form-item label="级别">
          <el-select v-model="filter.level" placeholder="全部" clearable style="width:120px" @change="fetchList">
            <el-option label="错误" value="Error" />
            <el-option label="警告" value="Warning" />
            <el-option label="信息" value="Info" />
          </el-select>
        </el-form-item>
        <el-form-item label="来源">
          <el-input v-model="filter.source" placeholder="搜索来源" clearable style="width:180px" @keyup.enter="fetchList" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card>
      <el-table :data="list" v-loading="loading" stripe @row-click="showDetail">
        <el-table-column label="级别" width="80">
          <template #default="{ row }">
            <el-tag :type="row.level === 'Error' ? 'danger' : row.level === 'Warning' ? 'warning' : 'info'" size="small">
              {{ row.level }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="message" label="消息" min-width="300" show-overflow-tooltip />
        <el-table-column prop="source" label="来源" width="150" />
        <el-table-column prop="path" label="路径" width="200" show-overflow-tooltip />
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
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next"
          @change="fetchList"
        />
      </div>
    </el-card>

    <!-- 详情 Dialog -->
    <el-dialog v-model="showDetailDialog" title="日志详情" width="800px">
      <template v-if="detail">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="级别">
            <el-tag :type="detail.level === 'Error' ? 'danger' : 'warning'" size="small">{{ detail.level }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="时间">{{ formatTime(detail.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="来源" :span="2">{{ detail.source }}</el-descriptions-item>
          <el-descriptions-item label="路径">{{ detail.path }}</el-descriptions-item>
          <el-descriptions-item label="方法">{{ detail.method }}</el-descriptions-item>
          <el-descriptions-item label="IP">{{ detail.ipAddress }}</el-descriptions-item>
          <el-descriptions-item label="用户ID">{{ detail.userId || '-' }}</el-descriptions-item>
          <el-descriptions-item label="用户代理" :span="2">{{ detail.userAgent || '-' }}</el-descriptions-item>
        </el-descriptions>
        <el-divider />
        <h4>消息</h4>
        <pre style="background:#f5f7fa;padding:12px;border-radius:4px;white-space:pre-wrap;font-size:13px;">{{ detail.message }}</pre>
        <h4 v-if="detail.exception">
          异常堆栈
          <el-button size="small" text type="primary" @click="copyText(detail.exception)" style="margin-left:8px">复制</el-button>
        </h4>
        <pre v-if="detail.exception" style="background:#fef0f0;padding:12px;border-radius:4px;white-space:pre-wrap;font-size:12px;max-height:400px;overflow:auto;position:relative;">{{ detail.exception }}</pre>
      </template>
      <template #footer>
        <el-button @click="showDetailDialog = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getSystemLogs, getSystemLog, deleteSystemLog, clearSystemLogs } from '../../../api/index'

const loading = ref(false)
const list = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const filter = reactive({ level: '', source: '' })

const showDetailDialog = ref(false)
const detail = ref(null)

async function fetchList() {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: pageSize.value }
    if (filter.level) params.level = filter.level
    if (filter.source) params.source = filter.source
    const res = await getSystemLogs(params)
    list.value = res.items || []
    total.value = res.total || 0
  } catch (e) {
    list.value = []
  }
  loading.value = false
}

async function showDetail(row) {
  try {
    const res = await getSystemLog(row.id)
    detail.value = res
    showDetailDialog.value = true
  } catch (e) {
    ElMessage.error('获取日志详情失败')
  }
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm('确定删除此日志？', '提示', { type: 'warning' })
    await deleteSystemLog(row.id)
    ElMessage.success('已删除')
    await fetchList()
  } catch (e) { /* cancelled */ }
}

async function handleClearAll() {
  try {
    await ElMessageBox.confirm(`确定清空全部 ${total.value} 条日志？`, '警告', { type: 'warning' })
    await clearSystemLogs()
    ElMessage.success('已清空')
    await fetchList()
  } catch (e) { /* cancelled */ }
}

async function copyText(text) {
  try {
    await navigator.clipboard.writeText(text)
    ElMessage.success('已复制')
  } catch {
    ElMessage.warning('复制失败')
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
.page-actions { display: flex; gap: 8px; }
.search-bar { margin-bottom: 16px; }
.pagination-wrap { margin-top: 16px; display: flex; justify-content: flex-end; }
pre { margin: 8px 0; }
h4 { margin: 16px 0 4px; }
</style>
