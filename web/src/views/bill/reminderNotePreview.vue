<template>
  <div class="remindernote-preview">
    <!-- Action Bar -->
    <div class="preview-actions">
      <el-button @click="$router.push('/bills')">
        <el-icon><ArrowLeft /></el-icon>返回列表
      </el-button>
      <el-button type="primary" @click="downloadPdf">
        <el-icon><Download /></el-icon>导出 PDF
      </el-button>
      <el-button @click="printPdf">
        <el-icon><Printer /></el-icon>打印
      </el-button>
      <span class="version-badge">
        ReminderNote 演示 · 生成于 {{ generatedAt }}
      </span>
    </div>

    <!-- PDF Preview -->
    <div class="pdf-viewer-container" v-if="pdfUrl">
      <iframe
        ref="pdfFrameRef"
        :src="pdfUrl"
        class="pdf-frame"
        title="ReminderNote PDF预览"
      ></iframe>
    </div>

    <!-- Loading State -->
    <div class="loading-state" v-else-if="pdfLoading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <span>正在生成 ReminderNote PDF 预览...</span>
    </div>

    <!-- Error / Fallback -->
    <div class="loading-state" v-else>
      <el-icon :size="32" color="#e6a23c"><WarningFilled /></el-icon>
      <span>PDF 预览生成失败</span>
      <el-button type="primary" @click="retryLoad" style="margin-top: 12px;">重试</el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import { reminderNotePreview } from '../../api/index'

const pdfFrameRef = ref(null)
const pdfUrl = ref(null)
const pdfBlob = ref(null)
const pdfLoading = ref(true)
const generatedAt = ref(new Date().toISOString().slice(0, 10))

function triggerDownload(blob, filename) {
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  setTimeout(() => window.URL.revokeObjectURL(url), 10000)
}

async function loadPreview() {
  pdfLoading.value = true
  try {
    const blob = await reminderNotePreview()
    pdfBlob.value = blob
    pdfUrl.value = window.URL.createObjectURL(blob)
    pdfLoading.value = false
  } catch {
    ElMessage.error('加载 ReminderNote PDF 失败')
    pdfLoading.value = false
  }
}

function retryLoad() { loadPreview() }

function downloadPdf() {
  if (!pdfBlob.value) {
    ElMessage.warning('PDF 尚未加载完成，请稍候')
    return
  }
  const filename = `ReminderNote_${generatedAt.value}.pdf`
  triggerDownload(pdfBlob.value, filename)
  ElMessage.success('ReminderNote PDF 已下载')
}

function printPdf() {
  if (pdfUrl.value) {
    window.open(pdfUrl.value, '_blank')
  } else {
    ElMessage.warning('PDF 尚未加载完成')
  }
}

onMounted(() => { loadPreview() })
onUnmounted(() => {
  if (pdfUrl.value) window.URL.revokeObjectURL(pdfUrl.value)
})
</script>

<style scoped>
.remindernote-preview {
  width: 100%;
  max-width: 100%;
  padding: 0 24px;
  box-sizing: border-box;
  overflow-x: hidden;
  display: flex;
  flex-direction: column;
  align-items: center;
}
.preview-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 20px;
  padding: 12px 16px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.06);
  width: 210mm;
  box-sizing: border-box;
}
.version-badge {
  margin-left: auto;
  font-size: 12px;
  color: #909399;
  display: flex;
  align-items: center;
  gap: 6px;
}
.pdf-viewer-container {
  width: 210mm;
  height: 80vh;
  min-height: 600px;
  background: #fff;
  box-shadow: 0 2px 12px rgba(0,0,0,0.12);
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 24px;
}
.pdf-frame { width: 100%; height: 100%; border: none; }
.loading-state {
  width: 210mm;
  min-height: 400px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  background: #fff;
  box-shadow: 0 2px 12px rgba(0,0,0,0.12);
  border-radius: 4px;
  color: #909399;
  font-size: 14px;
  margin-bottom: 24px;
}
</style>
