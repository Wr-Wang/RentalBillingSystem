<template>
  <div v-loading="pageLoading" :element-loading-text="loadingText" element-loading-background="rgba(0,0,0,0.45)">
    <div class="page-header">
      <h2>批量导入房屋</h2>
      <el-button @click="$router.push('/buildings')">返回列表</el-button>
    </div>

<!-- 页面切换动画 -->
<Transition name="page-fade" mode="out-in">
  <!-- ====== 步骤1：上传 ====== -->
  <div key="upload" v-if="pageStatus === 'upload'">
      <el-card style="margin-bottom: 16px;">
        <template #header>导入说明</template>
        <el-alert
          title="请先下载模板，按模板格式填写数据后上传。有效数据将进入三级审批流程（运营主管 → 部门经理 → 总经理），审批通过后自动创建房源。"
          type="info"
          show-icon
          :closable="false"
        />
        <div style="margin-top: 16px;">
          <el-button type="primary" @click="downloadTemplate" :loading="downloading">
            <el-icon v-if="!downloading"><Download /></el-icon>
            <span>{{ downloading ? '正在生成模板...' : '下载导入模板' }}</span>
          </el-button>
          <span style="margin-left: 12px; font-size: 13px; color: #909399;">
            模板包含房型下拉列表，金额自动计算无需填写
          </span>
        </div>
      </el-card>

      <el-card>
        <template #header>上传数据</template>
        <el-upload
          ref="uploadRef"
          :auto-upload="false"
          accept=".xlsx,.xls"
          :limit="1"
          :on-change="handleFileChange"
          :on-exceed="() => ElMessage.warning('只能上传一个文件')"
          drag
        >
          <el-icon class="el-icon--upload" :size="48"><UploadFilled /></el-icon>
          <div class="el-upload__text">
            将 Excel 文件拖到此处，或 <em>点击选择</em>
          </div>
          <template #tip>
            <div class="el-upload__tip">
              支持 .xlsx 格式，单次不超过 10000 条记录。金额由系统自动计算，无需填写。
            </div>
          </template>
        </el-upload>
      </el-card>
    </div>

    <!-- ====== 步骤2：预览确认 ====== -->
    <div key="preview" v-else-if="pageStatus === 'preview'">
      <el-card style="margin-bottom: 16px;">
        <template #header>数据预览</template>
        <div style="margin-bottom: 12px;">
          <el-tag>总行数 {{ previewData.length }}</el-tag>
          <el-tag type="warning" style="margin-left: 8px;">
            提交后将进入三级审批流程，审批通过后自动创建
          </el-tag>
        </div>
        <el-table :data="previewData.slice(0, 10)" stripe max-height="400" style="width: 100%">
          <el-table-column type="index" label="行号" width="60" />
          <el-table-column prop="buildingName" label="座楼" width="100" />
          <el-table-column prop="floorName" label="楼层" width="80" />
          <el-table-column prop="unitNo" label="房号" width="80" />
          <el-table-column prop="roomTypeName" label="房型" width="120" />
          <el-table-column prop="area" label="面积" width="80">
            <template #default="{ row }">{{ row.area || '-' }}</template>
          </el-table-column>
          <el-table-column prop="orientation" label="朝向" width="80" />
        </el-table>
        <div v-if="previewData.length > 10" style="margin-top: 8px; text-align: center; color: #909399;">
          仅展示前 10 行，共 {{ previewData.length }} 行
        </div>
      </el-card>

      <div style="text-align: center; margin-bottom: 24px;">
        <el-button @click="pageStatus = 'upload'">重新选择文件</el-button>
        <el-button type="primary" size="large" @click="submitImport" :loading="submitting">
          <el-icon v-if="!submitting"><Upload /></el-icon>
          <span>{{ submitting ? '正在提交审批...' : '提交审批导入' }}</span>
        </el-button>
      </div>
    </div>

    <!-- ====== 步骤3：导入结果 ====== -->
    <div key="result" v-else>
      <!-- 统计卡片 -->
      <el-row :gutter="16" style="margin-bottom: 16px;">
        <el-col :span="8">
          <el-card shadow="hover">
            <div class="stat-card">
              <div class="stat-value">{{ result.totalRows }}</div>
              <div class="stat-label">总行数</div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="8">
          <el-card shadow="hover">
            <div class="stat-card">
              <div class="stat-value" style="color: #67c23a;">{{ result.validRows }}</div>
              <div class="stat-label">✅ 成功提交审批</div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="8">
          <el-card shadow="hover">
            <div class="stat-card">
              <div class="stat-value" style="color: #f56c6c;">{{ result.failedRows }}</div>
              <div class="stat-label">❌ 失败行数</div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <!-- 有有效行 → 提示已进入审批 -->
      <el-alert
        v-if="result.validRows > 0"
        title="已提交审批"
        type="success"
        show-icon
        style="margin-bottom: 16px;"
      >
        <template #description>
          <span>{{ result.validRows }} 条有效数据已进入审批流程，请前往</span>
          <el-button text type="primary" size="small" @click="goToMyApprovals" style="padding: 0 4px;">
            审批中心 &gt; 我的提交
          </el-button>
          <span>查看处理进度。</span>
        </template>
      </el-alert>

      <!-- 无有效行 -->
      <el-alert
        v-if="result.validRows === 0"
        title="全部失败"
        description="所有行格式校验未通过，未提交审批。请下载失败行列表修正后重新导入。"
        type="error"
        show-icon
        style="margin-bottom: 16px;"
      />

      <!-- Tab 切换 -->
      <el-card v-if="result.failures && result.failures.length > 0">
        <el-tabs v-model="activeTab">
          <el-tab-pane label="成功列表" :name="'success'">
            <el-empty v-if="result.validRows === 0" description="无成功数据" />
          </el-tab-pane>

          <el-tab-pane :label="`失败列表 (${result.failedRows})`" name="fail">
            <el-table :data="result.failures || []" stripe style="width: 100%">
              <el-table-column prop="rowIndex" label="Excel行号" width="100" />
              <el-table-column prop="buildingName" label="座楼" width="100" />
              <el-table-column prop="floorName" label="楼层" width="80" />
              <el-table-column prop="unitNo" label="房号" width="80" />
              <el-table-column prop="errorCode" label="错误码" width="140">
                <template #default="{ row }">
                  <el-tag :type="errorTagType(row.errorCode)" size="small">{{ row.errorCode }}</el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="errorMessage" label="失败原因" min-width="200">
                <template #default="{ row }">
                  <span :style="{ color: errorColor(row.errorCode) }">{{ row.errorMessage }}</span>
                </template>
              </el-table-column>
              <el-table-column label="💡 修正建议" min-width="220">
                <template #default="{ row }">
                  <span style="color: #909399; font-size: 13px;">{{ row.fixSuggestion || '-' }}</span>
                </template>
              </el-table-column>
            </el-table>
          </el-tab-pane>
        </el-tabs>
      </el-card>

      <div style="text-align: center; margin-top: 24px;">
        <el-button @click="$router.push('/buildings')">返回列表</el-button>
        <el-button type="primary" @click="resetPage">继续导入</el-button>
      </div>
    </div>

    </Transition>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import * as XLSX from 'xlsx'
import JSZip from 'jszip'
import { getRoomTypes, submitImport as submitImportApi } from '../../api/index'
import { useUserStore } from '../../store/user'

const router = useRouter()
const userStore = useUserStore()
const uploadRef = ref(null)
const pageStatus = ref('upload')
const submitting = ref(false)
const downloading = ref(false)
const pageLoading = ref(false)
const loadingText = ref('')
const previewData = ref([])
const activeTab = ref('fail')
const rawFile = ref(null)
const result = reactive({
  totalRows: 0, validRows: 0, failedRows: 0,
  batchId: '', approvalRequestId: null,
  failures: []
})

// ==================== 颜色工具 ====================
function errorTagType(code) {
  if (!code) return 'info'
  if (code.includes('REQUIRED') || code.includes('INVALID')) return 'danger'
  if (code.includes('AREA') || code.includes('ORIENTATION')) return 'warning'
  if (code.includes('DUPLICATE')) return 'warning'
  return 'info'
}
function errorColor(code) {
  if (!code) return '#909399'
  if (code.includes('REQUIRED') || code.includes('INVALID')) return '#f56c6c'
  if (code.includes('AREA') || code.includes('ORIENTATION')) return '#e6a23c'
  if (code.includes('DUPLICATE')) return '#f0c040'
  return '#909399'
}


// ==================== 下载模板 ====================
async function downloadTemplate() {
  downloading.value = true
  pageLoading.value = true
  loadingText.value = '正在生成模板...'
  try {
    // 获取系统房型列表
    let roomTypes = []
    try {
      const res = await getRoomTypes()
      roomTypes = Array.isArray(res) ? res : []
    } catch { /* ignore */ }

    const defaultRoomTypes = ['开间/单间', '一室一厅', '两室一厅', '两室两厅', '三室一厅', '三室两厅', '四室及以上', '主卧', '次卧', '公寓']
    const roomTypeNames = roomTypes.length > 0 ? roomTypes.map(rt => rt.name) : defaultRoomTypes
    const orientations = ['东', '南', '西', '北', '南北通透', '东南', '西南', '东北', '西北']

    // ===== 用 xlsx 构建工作表内容 =====
    const wsData = [
      ['⚠ 红色*列为必填，房型请从下拉列表选择，金额自动计算无需填写', '', '', '', '', ''],
      ['座楼名称(BuildingName)*', '楼层(FloorName)*', '房号(UnitNo)*', '房型(RoomTypeName)*', '面积(Area)', '朝向(Orientation)'],
      ['A栋', '1层', '103', '一室一厅', 55.5, '南'],
      ['A栋', '2层', '203', '两室一厅', 85, '南北通透'],
      ['B栋', '1层', '102', '开间/单间', 35, '东'],
    ]

    const ws = XLSX.utils.aoa_to_sheet(wsData)
    ws['!merges'] = [{ s: { r: 0, c: 0 }, e: { r: 0, c: 5 } }] // 第1行 A1:F1 合并
    ws['!cols'] = [
      { wch: 24 }, { wch: 14 }, { wch: 12 },
      { wch: 22 }, { wch: 12 }, { wch: 16 }
    ]

    // Sheet2: 房型参考
    const refData = [[roomTypeNames.join(', '), '']]
    const refWs = XLSX.utils.aoa_to_sheet(refData)
    refWs['!cols'] = [{ wch: 60 }]

    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, '导入模板')
    XLSX.utils.book_append_sheet(wb, refWs, '房型参考')

    // ===== 写入为 buffer，然后用 JSZip 注入样式 + 数据验证 =====
    const xlsxBinary = XLSX.write(wb, { type: 'base64', bookType: 'xlsx' })
    const zip = new JSZip()
    await zip.loadAsync(xlsxBinary, { base64: true })

    // ---- 1. 注入 styles.xml：添加蓝底白字表头样式 ----
    let stylesXml = await zip.file('xl/styles.xml')?.async('string')
    if (stylesXml) {
      // 更新 count 属性 + 添加新元素（用标签名限定，避免误匹配）
      stylesXml = stylesXml
        .replace('<fonts count="1">', '<fonts count="2">')
        .replace('<fills count="2">', '<fills count="3">')
        .replace('<borders count="1">', '<borders count="2">')
      // 追加新字体(fontId=1)：白色微软雅黑
      stylesXml = stylesXml.replace('</fonts>',
        '<font><sz val="11"/><color rgb="FFFFFF"/><name val="Microsoft YaHei"/></font></fonts>')
      // 追加新填充(fillId=2)：蓝底
      stylesXml = stylesXml.replace('</fills>',
        '<fill><patternFill patternType="solid"><fgColor rgb="4472C4"/></patternFill></fill></fills>')
      // 追加新边框(borderId=1)：细边框
      stylesXml = stylesXml.replace('</borders>',
        '<border><left style="thin"><color auto="1"/></left><right style="thin"><color auto="1"/></right><top style="thin"><color auto="1"/></top><bottom style="thin"><color auto="1"/></bottom></border></borders>')
      // 追加单元格格式(xfId=1)：引用fontId=1, fillId=2, borderId=1
      stylesXml = stylesXml.replace('</cellXfs>',
        '<xf numFmtId="0" fontId="1" fillId="2" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1"/></cellXfs>')
      zip.file('xl/styles.xml', stylesXml)
    }

    // ---- 2. 注入 sheet1.xml：表头样式 + 冻结窗格 + 数据验证 ----
    let sheetXml = await zip.file('xl/worksheets/sheet1.xml')?.async('string')
    if (!sheetXml) throw new Error('未找到 sheet1.xml')

    // 2a. 表头行第2行所有单元格加蓝底白字样式 s="1"
    sheetXml = sheetXml.replace(/(<row r="2">)/, '$1')
    ;['A2','B2','C2','D2','E2','F2'].forEach(ref => {
      sheetXml = sheetXml.replace(`<c r="${ref}" t="str">`, `<c r="${ref}" t="str" s="1">`)
    })

    // 2b. 冻结窗格（替换 sheetViews 节点）
    sheetXml = sheetXml.replace(
      /<sheetViews>.*?<\/sheetViews>/s,
      '<sheetViews><sheetView tabSelected="1" workbookViewId="0"><pane ySplit="2" topLeftCell="A3" activePane="bottomLeft" state="frozen"/></sheetView></sheetViews>')

    // 2c. 注入数据验证（下拉列表）
    const roomTypeFormula = roomTypeNames.join(',')
    const orientationFormula = orientations.join(',')
    sheetXml = sheetXml.replace('</worksheet>',
      `<dataValidations count="2">
      <dataValidation type="list" allowBlank="1" showErrorMessage="1" errorTitle="无效的房型" error="请从下拉列表中选择系统现有房型名称，不可手工输入" sqref="D3:D10001">
        <formula1>"${roomTypeFormula}"</formula1>
      </dataValidation>
      <dataValidation type="list" allowBlank="1" showErrorMessage="1" errorTitle="无效的朝向" error="请从下拉列表中选择：东、南、西、北、南北通透、东南、西南、东北、西北" sqref="F3:F10001">
        <formula1>"${orientationFormula}"</formula1>
      </dataValidation>
    </dataValidations>\n</worksheet>`)

    zip.file('xl/worksheets/sheet1.xml', sheetXml)

    // ===== 生成并下载 =====
    const blob = await zip.generateAsync({ type: 'blob', mimeType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    const now = new Date()
    const ts = `${now.getFullYear()}${String(now.getMonth()+1).padStart(2,'0')}${String(now.getDate()).padStart(2,'0')}_${String(now.getHours()).padStart(2,'0')}${String(now.getMinutes()).padStart(2,'0')}${String(now.getSeconds()).padStart(2,'0')}`
    a.download = `房源导入模板_${ts}.xlsx`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)

    downloading.value = false
    pageLoading.value = false
    loadingText.value = ''
    ElMessage.success('模板已下载，房型列已设置下拉列表，可直接从「房型参考」Sheet 选择')
  } catch (e) {
    downloading.value = false
    pageLoading.value = false
    loadingText.value = ''
    console.error(e)
    ElMessage.error('模板下载失败：' + (e.message || '未知错误'))
  }
}

// ==================== 文件上传解析 ====================
function handleFileChange(uploadFile) {
  const file = uploadFile.raw
  if (!file) return

  const ext = file.name.split('.').pop().toLowerCase()
  if (!['xlsx', 'xls'].includes(ext)) {
    ElMessage.error('仅支持 .xlsx / .xls 格式')
    return
  }

  rawFile.value = file
  const reader = new FileReader()
  reader.onload = (e) => {
    try {
      const data = new Uint8Array(e.target.result)
      const workbook = XLSX.read(data, { type: 'array' })
      const firstSheet = workbook.Sheets[workbook.SheetNames[0]]

      // 使用 header:1 解析为二维数组 [row][col]
      const rows = XLSX.utils.sheet_to_json(firstSheet, { header: 1, defval: '' })

      // 动态查找表头行（包含"座楼"关键字的行）
      const headerIdx = rows.findIndex(r => String(r[0] || '').includes('座楼'))
      if (headerIdx === -1) {
        ElMessage.error('无法识别模板格式：未找到"座楼名称*"表头行')
        return
      }

      // 从表头下一行开始取数据，过滤掉空行
      const dataRows = rows.slice(headerIdx + 1).filter(r => {
        return r.some(v => v !== null && v !== undefined && v !== '')
      })

      if (dataRows.length === 0) {
        ElMessage.warning('未找到有效数据行（请从第3行开始填写）')
        return
      }

      if (dataRows.length > 10000) {
        ElMessage.error('单次导入不超过 10000 条')
        return
      }

      // 映射：A=座楼, B=楼层, C=房号, D=房型, E=面积, F=朝向
      previewData.value = dataRows.map((r, i) => ({
        rowIndex: i + 1,
        buildingName: String(r[0] || '').trim(),
        floorName: String(r[1] || '').trim(),
        unitNo: String(r[2] || '').trim(),
        roomTypeName: String(r[3] || '').trim(),
        area: String(r[4] || '').trim(),
        orientation: String(r[5] || '').trim()
      }))

      // 检查是否至少有一行有数据
      const hasData = previewData.value.some(row =>
        row.buildingName || row.floorName || row.unitNo
      )
      if (!hasData) {
        ElMessage.warning('未找到有效数据，请检查模板格式')
        return
      }

      pageStatus.value = 'preview'
      ElMessage.success(`解析完成，共 ${previewData.value.length} 行数据`)
    } catch (err) {
      ElMessage.error('文件解析失败：' + err.message)
    }
  }
  reader.readAsArrayBuffer(file)
}

// ==================== 提交导入 ====================
async function submitImport() {
  submitting.value = true
  pageLoading.value = true
  loadingText.value = '正在提交审批，请勿关闭页面...'
  try {
    const items = previewData.value.map(row => ({
      rowIndex: row.rowIndex,
      data: {
        buildingName: row.buildingName,
        buildingCode: '',
        buildingAddress: '',
        floorName: row.floorName,
        unitNo: row.unitNo,
        roomTypeName: row.roomTypeName,
        area: parseFloat(row.area) || null,
        orientation: row.orientation || null
      }
    }))

    const res = await submitImportApi({
      importType: 'HousingUnit',
      companyId: userStore.currentCompanyId || undefined,
      fileName: rawFile.value?.name || '导入文件.xlsx',
      items
    })

    result.totalRows = res.totalRows || 0
    result.validRows = res.validRows || 0
    result.failedRows = res.failedRows || 0
    result.batchId = res.batchId || ''
    result.approvalRequestId = res.approvalRequestId || null
    result.failures = res.failures || []

    pageStatus.value = 'result'

    if (res.failedRows > 0 && res.validRows > 0) {
      ElMessage.warning(`提交完成：${res.validRows} 条进入审批，${res.failedRows} 条格式失败`)
    } else if (res.failedRows === 0) {
      ElMessage.success(`全部成功，${res.validRows} 条数据已进入审批流程`)
    } else {
      ElMessage.error(`全部失败，${res.failedRows} 条格式校验未通过`)
    }
  } catch (e) {
    ElMessage.error(e?.response?.data?.title || '提交失败')
  }
  submitting.value = false
  pageLoading.value = false
  loadingText.value = ''
}

// ==================== 重置 ====================
function resetPage() {
  pageStatus.value = 'upload'
  previewData.value = []
  rawFile.value = null
  result.totalRows = 0
  result.validRows = 0
  result.failedRows = 0
  result.batchId = ''
  result.approvalRequestId = null
  result.failures = []
  uploadRef.value?.clearFiles()
}

function goToMyApprovals() {
  // 检查用户是否有权限访问审批中心（非超级管理员都能访问）
  const userStr = localStorage.getItem('user')
  if (userStr) {
    router.push('/approvals/myrequests')
  } else {
    ElMessage.info('请先登录')
  }
}
</script>

<style scoped>
.stat-card { text-align: center; padding: 8px 0; }
.stat-value { font-size: 32px; font-weight: bold; line-height: 1.2; }
.stat-label { font-size: 14px; color: #909399; margin-top: 4px; }

/* 页面切换动画 */
.page-fade-enter-active,
.page-fade-leave-active {
  transition: opacity 0.25s ease, transform 0.25s ease;
}
.page-fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}
.page-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
