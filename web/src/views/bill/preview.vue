<template>
  <div class="bill-preview">
    <!-- Action Bar -->
    <div class="preview-actions">
      <el-button @click="$router.push('/bills')">
        <el-icon><ArrowLeft /></el-icon>返回列表
      </el-button>
      <el-button type="primary" @click="exportPdf">
        <el-icon><Download /></el-icon>导出PDF{{ bill?.isHistorical ? '（历史）' : '' }}
      </el-button>
      <el-button @click="printBill">
        <el-icon><Printer /></el-icon>打印
      </el-button>
      <span class="version-badge" v-if="bill">
        生成于 {{ bill.generatedAt }}
        <el-tag v-if="bill.isHistorical" type="warning" size="small" style="margin-left: 6px;">历史账单</el-tag>
        <el-tag v-else type="success" size="small" style="margin-left: 6px;">当前账单</el-tag>
      </span>
    </div>

    <!-- Bill Content -->
    <div class="bill-paper" ref="billPaperRef" :class="{ 'is-historical': bill?.isHistorical }">
      <!-- Historical Watermark Banner -->
      <div v-if="bill?.isHistorical" class="historical-banner">
        <el-icon><WarningFilled /></el-icon>
        <span>历 史 账 单 — 仅 供 参 考</span>
        <el-icon><WarningFilled /></el-icon>
      </div>

      <!-- Header -->
      <div class="bill-header">
        <div class="bill-title">房 屋 租 赁 费 用 账 单</div>
        <div class="bill-subtitle">{{ bill?.billNo }}</div>
      </div>

      <!-- Basic Info -->
      <div class="bill-section">
        <h3 class="section-title">基本信息</h3>
        <table class="info-table">
          <tr>
            <td class="label">租客姓名</td>
            <td class="value">{{ bill?.tenantName }}</td>
            <td class="label">合同编号</td>
            <td class="value">{{ bill?.contractNo }}</td>
          </tr>
          <tr>
            <td class="label">房屋地址</td>
            <td class="value">{{ bill?.roomName }}</td>
            <td class="label">账期</td>
            <td class="value">{{ bill?.period }}</td>
          </tr>
          <tr>
            <td class="label">到期日</td>
            <td class="value">{{ bill?.dueDate }}</td>
            <td class="label">账单状态</td>
            <td class="value">
              <el-tag
                :type="bill?.status === 'Paid' ? 'success' : bill?.status === 'Partial' ? 'warning' : 'danger'"
                size="small"
              >
                {{ bill?.status === 'Paid' ? '已付清' : bill?.status === 'Partial' ? '部分已收' : '待收款' }}
              </el-tag>
              <el-tag v-if="bill?.isHistorical" type="warning" size="small" style="margin-left: 4px;">历史</el-tag>
            </td>
          </tr>
        </table>
      </div>

      <!-- Fee Details -->
      <div class="bill-section">
        <h3 class="section-title">费用明细</h3>
        <table class="fee-table">
          <thead>
            <tr>
              <th style="width: 50px;">#</th>
              <th style="width: 140px;">费用项目</th>
              <th style="width: 100px;">金额</th>
              <th style="width: 90px;">已收</th>
              <th>说明</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, idx) in bill?.items" :key="item.id">
              <td style="text-align: center;">{{ idx + 1 }}</td>
              <td>{{ item.feeName }}</td>
              <td style="text-align: right;">¥{{ item.amount?.toLocaleString() }}</td>
              <td style="text-align: right;">¥{{ item.received?.toLocaleString() }}</td>
              <td style="color: #606266;">{{ item.description || '-' }}</td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="total-row">
              <td colspan="2" style="text-align: right; font-weight: 600;">合计：</td>
              <td style="text-align: right; font-weight: 600; color: #409eff;">
                ¥{{ bill?.totalAmount?.toLocaleString() }}
              </td>
              <td style="text-align: right; font-weight: 600;">
                ¥{{ bill?.totalReceived?.toLocaleString() }}
              </td>
              <td></td>
            </tr>
          </tfoot>
        </table>
      </div>

      <!-- Amount in Chinese -->
      <div class="bill-section amount-section">
        <div class="amount-chinese">
          <span class="label">金额大写：</span>
          <span class="value">{{ amountChinese }}</span>
        </div>
      </div>

      <!-- Historical Notice -->
      <div v-if="bill?.isHistorical" class="historical-notice">
        <el-icon style="margin-right: 4px;"><InfoFilled /></el-icon>
        本账单为历史版本数据，仅供参考，不作为当期缴费依据。
      </div>

      <!-- Footer -->
      <div class="bill-footer">
        <div class="footer-left">
          <div>生成日期：{{ bill?.generatedAt }}</div>
          <div>系统自动生成，无需盖章</div>
        </div>
        <div class="footer-right">
          <div class="qrcode-placeholder">
            <el-icon :size="48"><Link /></el-icon>
            <span style="font-size: 10px; display: block; margin-top: 4px;">扫码查账单</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Export Progress -->
    <el-dialog v-model="showExportProgress" title="导出PDF" width="400px">
      <div style="text-align: center; padding: 20px 0;">
        <el-alert
          v-if="bill?.isHistorical"
          title="历史账单标记"
          description="此账单为历史数据，PDF 将自动标注「历史账单」水印。"
          type="warning"
          show-icon
          :closable="false"
          style="margin-bottom: 16px;"
        />
        <el-progress v-if="exportProgress < 100" :percentage="exportProgress" :stroke-width="12" />
        <el-result v-else icon="success" title="导出成功" sub-title="PDF 文件已生成，请下载">
          <template #extra>
            <el-button type="primary" @click="downloadPdf">下载文件</el-button>
            <el-button @click="showExportProgress = false">关闭</el-button>
          </template>
        </el-result>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const route = useRoute()
const router = useRouter()

const billPaperRef = ref(null)
const showExportProgress = ref(false)
const exportProgress = ref(0)

const bill = ref(null)

const defaultItems = [
  { id: 'i1', feeName: '房租费', amount: 5200, received: 0, description: '月租金 ¥5,200' },
  { id: 'i2', feeName: '水费', amount: 66, received: 0, description: '11吨 × ¥6/吨' },
  { id: 'i3', feeName: '电费', amount: 80, received: 0, description: '100度 × ¥0.8/度' },
  { id: 'i4', feeName: '卫生费', amount: 30, received: 0, description: '月固定 ¥30' },
  { id: 'i5', feeName: '管理费', amount: 150, received: 0, description: '月固定 ¥150' },
  { id: 'i6', feeName: '网费', amount: 80, received: 0, description: '月固定 ¥80' }
]

// Mock bill detail data — b4 is historical, others are current
const mockBillData = {
  'b1': {
    id: 'b1', billNo: 'ZD-202606-00001', contractNo: 'HT-2026-001',
    tenantName: '张三', roomName: 'A栋-101', period: '2026-06', dueDate: '2026-06-05',
    totalAmount: 5460, totalReceived: 5460, status: 'Paid',
    generatedAt: '2026-06-25 08:00:30', isHistorical: false,
    items: [
      { id: 'i1', feeName: '房租费', amount: 5000, received: 5000, description: '月租金 ¥5,000' },
      { id: 'i2', feeName: '水费', amount: 100, received: 100, description: '15吨 × ¥6/吨（读数:1245-1234=11吨，估读补4吨）' },
      { id: 'i3', feeName: '电费', amount: 200, received: 200, description: '250度 × ¥0.8/度（读数:5678-5500=178度，估读补72度）' },
      { id: 'i4', feeName: '卫生费', amount: 30, received: 30, description: '月固定 ¥30' },
      { id: 'i5', feeName: '管理费', amount: 150, received: 150, description: '月固定 ¥150' },
      { id: 'i6', feeName: '网费', amount: 80, received: 80, description: '月固定 ¥80' }
    ]
  },
  'b2': {
    id: 'b2', billNo: 'ZD-202606-00002', contractNo: 'HT-2026-002',
    tenantName: '李四', roomName: 'A栋-102', period: '2026-06', dueDate: '2026-06-05',
    totalAmount: 4030, totalReceived: 4000, status: 'Partial',
    generatedAt: '2026-06-25 08:00:30', isHistorical: false,
    items: [
      { id: 'i1', feeName: '房租费', amount: 3800, received: 3800, description: '月租金 ¥3,800' },
      { id: 'i2', feeName: '水费', amount: 80, received: 80, description: '12吨 × ¥6/吨（读数:5234-5222=12吨）' },
      { id: 'i3', feeName: '电费', amount: 120, received: 120, description: '150度 × ¥0.8/度（读数:3456-3306=150度）' },
      { id: 'i4', feeName: '卫生费', amount: 30, received: 0, description: '月固定 ¥30' },
      { id: 'i5', feeName: '管理费', amount: 150, received: 0, description: '月固定 ¥150' },
      { id: 'i6', feeName: '网费', amount: 80, received: 0, description: '月固定 ¥80' }
    ]
  },
  'b4': {
    id: 'b4', billNo: 'ZD-202606-00004', contractNo: 'HT-2026-004',
    tenantName: '赵六', roomName: 'B栋-202', period: '2026-06', dueDate: '2026-06-05',
    totalAmount: 6200, totalReceived: 0, status: 'Pending',
    generatedAt: '2026-06-27 09:15:00', isHistorical: true,
    items: [
      { id: 'i1', feeName: '房租费', amount: 5800, received: 0, description: '月租金 ¥5,800（自2026-06-01调价生效）' },
      { id: 'i2', feeName: '水费', amount: 90, received: 0, description: '15吨 × ¥6/吨（读数:2345-2330=15吨）' },
      { id: 'i3', feeName: '电费', amount: 160, received: 0, description: '200度 × ¥0.8/度（读数:7890-7690=200度）' },
      { id: 'i4', feeName: '卫生费', amount: 30, received: 0, description: '月固定 ¥30' },
      { id: 'i5', feeName: '管理费', amount: 150, received: 0, description: '月固定 ¥150' },
      { id: 'i6', feeName: '网费', amount: 80, received: 0, description: '月固定 ¥80' }
    ]
  }
}

const amountChinese = computed(() => {
  if (!bill.value) return ''
  return numToChinese(bill.value.totalAmount)
})

function numToChinese(num) {
  const digits = ['零', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖']
  const units = ['', '拾', '佰', '仟', '万', '拾', '佰', '仟', '亿']
  if (num === 0) return '零元整'
  const intPart = Math.floor(num)
  const decPart = Math.round((num - intPart) * 100)
  let result = ''
  let intStr = String(intPart)
  let len = intStr.length
  for (let i = 0; i < len; i++) {
    const d = parseInt(intStr[i])
    const pos = len - 1 - i
    if (d !== 0) {
      result += digits[d] + units[pos]
    } else {
      if (pos % 4 !== 0 && i + 1 < len && parseInt(intStr[i + 1]) !== 0) result += '零'
      if (pos === 4 || pos === 8) result += units[pos]
    }
  }
  result += '元'
  if (decPart === 0) result += '整'
  else {
    const jiao = Math.floor(decPart / 10)
    const fen = decPart % 10
    if (jiao > 0) result += digits[jiao] + '角'
    if (fen > 0) result += digits[fen] + '分'
  }
  return result
}

onMounted(() => {
  const id = route.params.id
  bill.value = mockBillData[id]
  if (!bill.value) {
    // Fallback for unregistered IDs
    bill.value = {
      id, billNo: `ZD-${new Date().getFullYear()}${String(new Date().getMonth() + 1).padStart(2, '0')}-XXXXX`,
      contractNo: 'HT-XXXX-XXX', tenantName: '未知', roomName: '未知',
      period: '2026-06', dueDate: '2026-06-05',
      totalAmount: defaultItems.reduce((s, i) => s + i.amount, 0),
      totalReceived: 0, status: 'Pending',
      generatedAt: new Date().toISOString().replace('T', ' ').slice(0, 19),
      isHistorical: false,
      items: defaultItems.map((item, idx) => ({ ...item, id: `i${idx + 1}`, received: 0 }))
    }
  }
})

function exportPdf() {
  showExportProgress.value = true
  exportProgress.value = 0
  const interval = setInterval(() => {
    exportProgress.value += Math.floor(Math.random() * 25) + 5
    if (exportProgress.value >= 100) {
      exportProgress.value = 100
      clearInterval(interval)
    }
  }, 300)
}

function downloadPdf() {
  const tag = bill.value?.isHistorical ? '历史账单' : '当前账单'
  ElMessage.success(`账单 ${bill.value?.billNo}（${tag}）已下载` +
    (bill.value?.isHistorical ? '，PDF已标注「历史账单」标记' : ''))
  showExportProgress.value = false
}

function printBill() {
  window.print()
}
</script>

<style scoped>
.bill-preview {
  max-width: 900px;
  margin: 0 auto;
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
}

.version-badge {
  margin-left: auto;
  font-size: 12px;
  color: #909399;
  display: flex;
  align-items: center;
}

.bill-paper {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.08);
  padding: 40px 48px;
  margin-bottom: 24px;
  position: relative;
}

/* Historical watermark background */
.bill-paper.is-historical::before {
  content: '历 史 账 单';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%) rotate(-30deg);
  font-size: 80px;
  font-weight: 700;
  color: rgba(230, 162, 60, 0.10);
  pointer-events: none;
  letter-spacing: 12px;
  white-space: nowrap;
  z-index: 0;
}

.historical-banner {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 8px 0;
  margin-bottom: 20px;
  background: linear-gradient(90deg, #fdf6ec, #faecd8, #fdf6ec);
  border: 1px solid #e6a23c;
  border-radius: 4px;
  color: #e6a23c;
  font-size: 16px;
  font-weight: 700;
  letter-spacing: 4px;
}

.historical-notice {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 10px;
  margin: 16px 0;
  background: #fdf6ec;
  border-radius: 4px;
  font-size: 13px;
  color: #e6a23c;
}

.bill-header {
  text-align: center;
  border-bottom: 2px solid #001529;
  padding-bottom: 20px;
  margin-bottom: 24px;
}

.bill-title {
  font-size: 24px;
  font-weight: 700;
  letter-spacing: 6px;
  color: #001529;
}

.bill-subtitle {
  font-size: 14px;
  color: #909399;
  margin-top: 6px;
  letter-spacing: 2px;
}

.bill-section {
  margin-bottom: 24px;
  position: relative;
  z-index: 1;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #303133;
  padding-bottom: 8px;
  border-bottom: 1px solid #ebeef5;
  margin-bottom: 12px;
}

.info-table {
  width: 100%;
  border-collapse: collapse;
}

.info-table td {
  padding: 8px 12px;
  border: 1px solid #ebeef5;
  font-size: 14px;
}

.info-table .label {
  width: 100px;
  background: #f5f7fa;
  font-weight: 500;
  color: #606266;
}

.info-table .value {
  width: 200px;
  color: #303133;
}

.fee-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.fee-table th,
.fee-table td {
  padding: 10px 12px;
  border: 1px solid #ebeef5;
  text-align: left;
}

.fee-table th {
  background: #f5f7fa;
  font-weight: 600;
  color: #606266;
}

.fee-table .total-row td {
  background: #fafafa;
  border-top: 2px solid #dcdfe6;
}

.amount-section {
  padding: 16px;
  background: #fafafa;
  border-radius: 6px;
  border: 1px solid #ebeef5;
}

.amount-chinese .label {
  font-weight: 500;
  color: #606266;
}

.amount-chinese .value {
  font-size: 18px;
  font-weight: 600;
  color: #c03636;
  letter-spacing: 2px;
}

.bill-footer {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-top: 32px;
  padding-top: 16px;
  border-top: 1px solid #ebeef5;
  font-size: 12px;
  color: #909399;
  position: relative;
  z-index: 1;
}

.qrcode-placeholder {
  width: 80px;
  height: 80px;
  border: 1px dashed #dcdfe6;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #909399;
}

@media print {
  .preview-actions { display: none; }
  .bill-paper {
    box-shadow: none;
    border: none;
    padding: 20px;
  }
  .bill-paper.is-historical::before {
    color: rgba(230, 162, 60, 0.15);
  }
}
</style>
