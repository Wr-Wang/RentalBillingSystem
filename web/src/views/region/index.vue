<template>
  <div>
    <div class="page-header">
      <h2>行政区划管理</h2>
      <div class="page-actions">
        <el-button @click="fetchList">
          <el-icon><Refresh /></el-icon>刷新
        </el-button>
        <el-button @click="collapseAll">
          <el-icon><Fold /></el-icon>全部折叠
        </el-button>
        <el-button type="primary" @click="openSync">
          <el-icon><Download /></el-icon>从 API 同步
        </el-button>
        <el-button type="info" @click="showStatsGov = true">
          <el-icon><Connection /></el-icon>同步统计局四级/五级
        </el-button>
        <el-button type="success" @click="openCreate">
          <el-icon><Plus /></el-icon>新增
        </el-button>
      </div>
    </div>

    <!-- 搜索栏 -->
    <el-card shadow="never" class="search-bar">
      <el-form :model="searchForm" inline>
        <el-form-item label="搜索">
          <el-input v-model="searchForm.keyword" placeholder="名称 / 代码" clearable @clear="fetchList" @keyup.enter="fetchList" style="width: 300px" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 区域列表：懒加载 -->
    <el-card shadow="never">
      <el-table ref="tableRef" :key="tableKey" :data="list" v-loading="loading" stripe row-key="code" lazy :load="loadChildren" :tree-props="{ children: 'children', hasChildren: 'hasChildren' }">
        <el-table-column prop="name" label="名称" min-width="360">
          <template #default="{ row }">
            <el-tooltip :content="row.fullPath || row.name" placement="top" :show-after="300">
              <span style="display:inline-block;max-width:320px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;vertical-align:middle">
                <template v-if="row.level === 1">🏛 </template>
                <template v-else-if="row.level === 2">🏙 </template>
                <template v-else-if="row.level === 3">📍 </template>
                <template v-else-if="row.level === 4">🚩 </template>
                <template v-else>🏘 </template>
                {{ row.name }}
              </span>
            </el-tooltip>
            <el-button link size="small" @click="copyText(row.name)" style="margin-left:4px">
              <el-icon><CopyDocument /></el-icon>
            </el-button>
          </template>
        </el-table-column>
        <el-table-column prop="level" label="层级" width="90">
          <template #default="{ row }">
            <el-tag :color="levelColor(row.level)" effect="dark" size="small" style="border:none;">{{ levelLabel(row.level) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="全路径" min-width="260">
          <template #default="{ row }">
            <div style="display:flex;align-items:center">
              <el-tooltip :content="row.fullPath || '-'" placement="top" :show-after="300">
                <span class="cell-ellipsis">{{ row.fullPath || '-' }}</span>
              </el-tooltip>
              <el-button v-if="row.fullPath" link size="small" @click="copyText(row.fullPath)" style="flex-shrink:0;margin-left:2px">
                <el-icon><CopyDocument /></el-icon>
              </el-button>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="code" label="代码" width="140" />
        <el-table-column prop="sortOrder" label="排序" width="70" />
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="openEdit(row)">编辑</el-button>
            <el-button text size="small" type="danger" class="dev-only" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增/编辑弹窗 -->
    <el-dialog :draggable="true" v-model="showForm" :title="isEdit ? '编辑区域' : '新增区域'" width="520px">
      <el-form ref="formRef" :model="form" :rules="formRules" label-width="100px">
        <el-form-item label="父级区域">
          <el-cascader
            v-model="form.parentChain"
            :props="parentCascaderProps"
            :placeholder="isEdit ? '不修改父级' : '选择父级（留空为根节点）'"
            clearable
            style="width:100%"
          />
        </el-form-item>
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" placeholder="如：天河区" />
        </el-form-item>
        <el-form-item label="代码" prop="code">
          <el-input v-model="form.code" placeholder="6 位行政区划代码" :disabled="isEdit" />
        </el-form-item>
        <el-form-item label="层级" prop="level">
          <el-select v-model="form.level" placeholder="选择层级" style="width:100%">
            <el-option :value="1" label="1 - 省/直辖市" />
            <el-option :value="2" label="2 - 地级市" />
            <el-option :value="3" label="3 - 区/县" />
            <el-option :value="4" label="4 - 街道/镇" />
            <el-option :value="5" label="5 - 社区/村" />
          </el-select>
        </el-form-item>
        <el-form-item label="排序号">
          <el-input-number v-model="form.sortOrder" :min="0" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showForm = false">取消</el-button>
        <el-button type="primary" @click="saveForm">保存</el-button>
      </template>
    </el-dialog>

    <!-- 同步确认弹窗：一二三级（高德 API） -->
    <el-dialog :draggable="true" v-model="showSync" title="同步省/市/区数据" width="440px">
      <el-alert type="warning" :closable="false" style="margin-bottom:12px">
        <p>同步过程中请勿刷新页面或关闭系统。</p>
      </el-alert>
      <p>从高德地图 API 拉取省/市/区数据，与本地已有数据合并（已存在的跳过，不删除）。</p>
      <p style="color:#909399;font-size:13px;margin-top:8px">数据来源：高德地图 WebService API（GB/T 2260 行政区划代码）</p>
      <template #footer>
        <el-button @click="showSync = false">取消</el-button>
        <el-button type="primary" :loading="syncing" @click="handleSync">
          开始同步
        </el-button>
      </template>
    </el-dialog>

    <!-- 同步确认弹窗：四五级（国家统计局） -->
    <el-dialog :draggable="true" v-model="showStatsGov" title="同步街道/社区数据" width="440px">
      <el-alert type="warning" :closable="false" style="margin-bottom:12px">
        <p><strong>此操作会先删除现有四级/五级数据，再重新写入，耗时 2-5 分钟。</strong></p>
        <p>同步期间请勿刷新页面或关闭系统，否则可能导致区域数据不完整。</p>
      </el-alert>
      <p>从国家统计局数据集拉取街道级（9 位编码）和社区级（12 位编码）数据，通过 SqlBulkCopy 批量写入。</p>
      <p style="color:#909399;font-size:13px;margin-top:8px">数据来源：国家统计局《统计用区划代码和城乡划分代码》</p>
      <template #footer>
        <el-button @click="showStatsGov = false">取消</el-button>
        <el-button type="warning" :loading="syncingCommunity" @click="handleSyncStatsGov">
          确认同步
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CopyDocument } from '@element-plus/icons-vue'
import {
  getRegionProvinces, getRegionChildren, searchRegions,
  upsertRegion, deleteRegion, syncRegions, syncStatsGov
} from '../../api'

const tableRef = ref(null)
const loading = ref(false)
const list = ref([])
const tableKey = ref(0)
const showForm = ref(false)
const isEdit = ref(false)
const showSync = ref(false)
const showStatsGov = ref(false)
const syncing = ref(false)
const syncingCommunity = ref(false)
const syncIncludeStreet = ref(true)
const searchForm = reactive({ keyword: '' })
const formRef = ref(null)

const defaultForm = () => ({
  code: '', name: '', parentCode: null, level: 3, sortOrder: 0, parentChain: []
})

const form = reactive(defaultForm())
const formRules = {
  name: [{ required: true, message: '请输入名称' }],
  code: [{ required: true, message: '请输入代码' }],
  level: [{ required: true, message: '请选择层级' }]
}

// 父级级联选择器
const parentCascaderProps = {
  lazy: true,
  lazyLoad: async (node, resolve) => {
    if (node.level === 0) {
      const provinces = await getRegionProvinces()
      resolve(provinces.map(p => ({ value: p.code, label: p.name, leaf: p.level >= 3 })))
    } else {
      const children = await getRegionChildren(node.value)
      resolve(children.map(c => ({ value: c.code, label: c.name, leaf: c.level >= 3 })))
    }
  }
}

function levelLabel(level) {
  const map = { 1: '省/直辖市', 2: '地级市', 3: '区/县', 4: '街道', 5: '社区' }
  return map[level] || '未知'
}

function levelColor(level) {
  const map = { 1: '#409EFF', 2: '#67C23A', 3: '#E6A23C', 4: '#9B59B6', 5: '#909399' }
  return map[level] || '#909399'
}

async function fetchList() {
  loading.value = true
  try {
    if (searchForm.keyword) {
      // 搜索模式：全量查询后构建树（搜索结果量小）
      const rows = await searchRegions(searchForm.keyword)
      list.value = buildTree(rows)
    } else {
      // 普通模式：只加载省一级，展开时懒加载子级
      const provinces = await getRegionProvinces()
      list.value = provinces.map(p => ({
        ...p,
        hasChildren: true
      }))
    }
  } catch (e) {
    ElMessage.error('加载区域数据失败')
  } finally {
    loading.value = false
  }
}

function buildTree(flatList) {
  const map = {}
  flatList.forEach(item => { map[item.code] = { ...item, children: [] } })
  const roots = []
  flatList.forEach(item => {
    if (item.parentCode && map[item.parentCode]) {
      map[item.parentCode].children.push(map[item.code])
    } else {
      roots.push(map[item.code])
    }
  })
  return roots
}

/** 懒加载子级（展开时触发） */
async function loadChildren(row, treeNode, resolve) {
  try {
    const children = await getRegionChildren(row.code)
    resolve(children.map(c => ({ ...c, hasChildren: c.level < 5 })))
  } catch {
    resolve([])
  }
}

/** 全部折叠：收起所有行并恢复懒加载模式 */
function copyText(text) {
  navigator.clipboard.writeText(text).then(() => {
    ElMessage.success('已复制')
  }).catch(() => {
    ElMessage.warning('复制失败')
  })
}

async function collapseAll() {
  try {
    const provinces = await getRegionProvinces()
    // 用全新对象重置数据，el-table 检测到 row-key 不同自动收起
    tableKey.value++
    list.value = provinces.map(p => ({
      code: p.code, name: p.name, level: p.level,
      parentCode: p.parentCode, fullPath: p.fullPath,
      sortOrder: p.sortOrder, hasChildren: true
    }))
  } catch (e) {
    ElMessage.error('折叠失败')
  }
}

function openCreate() {
  isEdit.value = false
  Object.assign(form, defaultForm())
  showForm.value = true
}

function openEdit(row) {
  isEdit.value = true
  form.code = row.code
  form.name = row.name
  form.parentCode = row.parentCode
  form.level = row.level
  form.sortOrder = row.sortOrder
  form.parentChain = row.parentCode ? [row.parentCode] : []
  showForm.value = true
}

async function saveForm() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  try {
    const pChain = form.parentChain || []
    await upsertRegion({
      code: form.code,
      name: form.name,
      parentCode: pChain.length > 0 ? pChain[pChain.length - 1] : null,
      level: form.level,
      sortOrder: form.sortOrder
    })
    ElMessage.success(isEdit.value ? '已更新' : '已新增')
    showForm.value = false
    await fetchList()
  } catch (e) {
    ElMessage.error('保存失败')
  }
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm(`确定删除「${row.name}」及其所有子级？`, '确认删除', { type: 'warning' })
    await deleteRegion(row.code)
    ElMessage.success('已删除')
    await fetchList()
  } catch { /* cancelled */ }
}

function openSync() {
  syncIncludeStreet.value = false
  showSync.value = true
}

async function handleSync() {
  syncing.value = true
  ElMessage.info('开始同步省份数据...')
  try {
    const res = await syncRegions(syncIncludeStreet.value)
    ElMessageBox.alert(
      res.progress.join('<br>'),
      '同步结果',
      { dangerouslyUseHTMLString: true, confirmButtonText: '知道了' }
    )
    if (res.skipped && res.skipped.length > 0) {
      ElMessage.info(`${res.skipped.length} 个省已有数据，已跳过`)
    }
    showSync.value = false
    await fetchList()
    ElMessage.success(`同步完成，共 ${res.totalSynced || res.provinceCount} 条`)
  } catch (e) {
    const data = e?.response?.data
    if (data?.progress) {
      ElMessageBox.alert(data.progress.join('<br>'), '同步进度',
        { dangerouslyUseHTMLString: true, confirmButtonText: '知道了' })
    } else {
      ElMessage.warning('同步被中断，已同步的数据已保存，可稍后再次同步补充')
    }
  } finally {
    syncing.value = false
  }
}

async function handleSyncStatsGov() {
  syncingCommunity.value = true
  showStatsGov.value = false
  try {
    const res = await syncStatsGov()
    const lines = (res?.progress || []).join('<br>')
    ElMessageBox.alert(lines || `同步完成，共 ${res?.synced || 0} 条`, '同步结果',
      { dangerouslyUseHTMLString: true, confirmButtonText: '知道了' })
    await fetchList()
  } catch (e) {
    ElMessage.warning('四级/五级数据同步未完成，可稍后重试')
  } finally {
    syncingCommunity.value = false
  }
}

onMounted(fetchList)
</script>

<style scoped>
/* 删除按钮默认隐藏（F12 可改为 display:block 恢复） */
.dev-only {
  display: none;
}

/* 单元格文本溢出省略 */
.cell-ellipsis {
  display: inline-block;
  max-width: 260px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  vertical-align: middle;
}
</style>
