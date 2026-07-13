<template>
  <div>
    <div class="page-header">
      <h2>凭证管理</h2>
      <el-button type="primary" @click="openCreateDialog">新建凭证</el-button>
    </div>

    <el-table :data="vouchers" v-loading="loading" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="voucherNo" label="凭证号" width="160" sortable="custom" />
      <el-table-column prop="voucherDate" label="日期" width="100" />
      <el-table-column label="借方合计" width="120">
        <template #default="{ row }">¥{{ (row.totalDebit || 0).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column label="贷方合计" width="120">
        <template #default="{ row }">¥{{ (row.totalCredit || 0).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column prop="sourceType" label="来源" width="100">
        <template #default="{ row }">{{ sourceTypeLabel(row.sourceType) }}</template>
      </el-table-column>
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Audited' ? 'warning' : row.status === 'Posted' ? 'success' : row.status === 'Draft' ? 'info' : 'danger'" size="small">
            {{ row.status === 'Draft' ? '草稿' : row.status === 'Posted' ? '已过账' : row.status === 'Audited' ? '已审核' : '已冲销' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="viewVoucher(row)">查看</el-button>
          <el-button text size="small" type="primary" v-if="row.status === 'Draft'" @click="editVoucher(row)">编辑</el-button>
          <el-button text size="small" type="success" v-if="row.status === 'Draft'" :loading="row._posting" @click="postVoucher(row)">过账</el-button>
          <el-button text size="small" type="warning" v-if="row.status === 'Posted'" :loading="row._auditing" @click="auditVoucherHandler(row)">审核</el-button>
          <el-button text size="small" type="danger" v-if="row.status === 'Posted'" :loading="row._reversing" @click="reverseVoucher(row)">冲销</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination
        v-model:page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @current-change="fetchList"
        @size-change="fetchList"
      />
    </div>

    <!-- Voucher Detail Dialog -->
    <el-dialog v-model="showVoucherDetail" title="凭证详情" width="700px">
      <template #default>
        <el-descriptions :column="3" border style="margin-bottom: 16px;">
          <el-descriptions-item label="凭证号">{{ voucherDetail.voucherNo }}</el-descriptions-item>
          <el-descriptions-item label="日期">{{ voucherDetail.voucherDate }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="voucherDetail.status === 'Audited' ? 'warning' : voucherDetail.status === 'Posted' ? 'success' : 'info'" size="small">{{ voucherDetail.status === 'Audited' ? '已审核' : voucherDetail.status === 'Posted' ? '已过账' : '草稿' }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="摘要" :span="3">{{ voucherDetail.description || '-' }}</el-descriptions-item>
        </el-descriptions>

        <el-table :data="voucherDetail.entries" stripe>
          <el-table-column type="index" label="行号" width="60" />
          <el-table-column prop="subjectCode" label="科目编码" width="100" />
          <el-table-column prop="subjectName" label="科目名称" width="180" />
          <el-table-column prop="summary" label="摘要" min-width="150" />
          <el-table-column label="借方金额" width="120" align="right">
            <template #default="{ row }">{{ row.direction === 'Debit' ? '¥' + (row.amount || 0).toLocaleString() : '-' }}</template>
          </el-table-column>
          <el-table-column label="贷方金额" width="120" align="right">
            <template #default="{ row }">{{ row.direction === 'Credit' ? '¥' + (row.amount || 0).toLocaleString() : '-' }}</template>
          </el-table-column>
        </el-table>
        <div style="margin-top: 8px; text-align: right; font-weight: bold;">
          借方合计: ¥{{ entriesDebitTotal.toLocaleString() }} | 贷方合计: ¥{{ entriesCreditTotal.toLocaleString() }}
        </div>
      </template>
    </el-dialog>

    <!-- Create/Edit Voucher Dialog -->
    <el-dialog v-model="showVoucherForm" :title="editingVoucherId ? '编辑凭证' : '新建凭证'" width="750px">
      <el-form :model="voucherForm" label-width="80px">
        <el-form-item label="日期">
          <el-date-picker v-model="voucherForm.voucherDate" type="date" placeholder="选择日期" value-format="YYYY-MM-DD" style="width:200px;" />
        </el-form-item>
        <el-form-item label="摘要">
          <el-input v-model="voucherForm.description" placeholder="凭证摘要说明（可选）" maxlength="200" />
        </el-form-item>
      </el-form>

      <h4 style="margin:16px 0 8px;">分录</h4>
      <el-table :data="voucherForm.entries" stripe>
        <el-table-column label="科目" width="220">
          <template #default="{ row, $index }">
            <el-select v-model="row.subjectId" filterable placeholder="选择科目" style="width:200px;" size="small">
              <el-option v-for="s in subjects" :key="s.id" :label="s.code + ' ' + s.name" :value="s.id" />
            </el-select>
          </template>
        </el-table-column>
        <el-table-column label="方向" width="90">
          <template #default="{ row, $index }">
            <el-select v-model="row.direction" style="width:80px;" size="small">
              <el-option label="借" value="Debit" />
              <el-option label="贷" value="Credit" />
            </el-select>
          </template>
        </el-table-column>
        <el-table-column label="金额" width="140">
          <template #default="{ row, $index }">
            <el-input-number v-model="row.amount" :min="0.01" :precision="2" style="width:130px;" size="small" controls-position="right" />
          </template>
        </el-table-column>
        <el-table-column label="摘要" min-width="160">
          <template #default="{ row, $index }">
            <el-input v-model="row.summary" placeholder="可选" size="small" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="60">
          <template #default="{ $index }">
            <el-button text size="small" type="danger" @click="voucherForm.entries.splice($index, 1)" v-if="voucherForm.entries.length > 2">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div style="margin-top:8px;">
        <el-button size="small" @click="addEntryRow">+ 添加分录</el-button>
      </div>

      <div style="margin-top:12px;text-align:right;font-weight:bold;">
        借方合计: ¥{{ formDebitTotal.toFixed(2) }} | 贷方合计: ¥{{ formCreditTotal.toFixed(2) }}
        <el-tag v-if="formBalanced" type="success" style="margin-left:12px;">平衡</el-tag>
        <el-tag v-else type="danger" style="margin-left:12px;">不平衡</el-tag>
      </div>

      <template #footer>
        <el-button @click="showVoucherForm = false">取消</el-button>
        <el-button type="primary" :loading="saving" :disabled="!formBalanced || voucherForm.entries.length < 2" @click="saveVoucher">
          {{ editingVoucherId ? '保存修改' : '创建凭证' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getVouchers, getVoucher, createVoucher, updateVoucher, postVoucher as apiPostVoucher, reverseVoucher as apiReverseVoucher, auditVoucher as apiAuditVoucher, getAccountingSubjects } from '../../api/index'

const loading = ref(false)
const vouchers = ref([])
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })
const showVoucherDetail = ref(false)
const showVoucherForm = ref(false)
const editingVoucherId = ref(null)
const saving = ref(false)
const subjects = ref([])

const voucherDetail = ref({ voucherNo: '', voucherDate: '', status: '', description: '', entries: [] })
const entriesDebitTotal = computed(() => voucherDetail.value.entries.reduce((s, e) => s + (e.direction === 'Debit' ? (e.amount || 0) : 0), 0))
const entriesCreditTotal = computed(() => voucherDetail.value.entries.reduce((s, e) => s + (e.direction === 'Credit' ? (e.amount || 0) : 0), 0))

const voucherForm = reactive({
  voucherDate: new Date().toISOString().slice(0, 10),
  description: '',
  entries: [
    { subjectId: '', direction: 'Debit', amount: null, summary: '' },
    { subjectId: '', direction: 'Credit', amount: null, summary: '' }
  ]
})

const formDebitTotal = computed(() => voucherForm.entries.reduce((s, e) => s + (e.direction === 'Debit' ? (e.amount || 0) : 0), 0))
const formCreditTotal = computed(() => voucherForm.entries.reduce((s, e) => s + (e.direction === 'Credit' ? (e.amount || 0) : 0), 0))
const formBalanced = computed(() => Math.abs(formDebitTotal.value - formCreditTotal.value) < 0.01)

function sourceTypeLabel(type) {
  const map = { 'Manual': '手动录入', 'Receipt': '收款确认', 'ReceivablePlan': '账单生成', 'ContractActivation': '合同激活', 'ContractFee.Immediate': '一次性费用', 'ContractFee.Supplementary': '调价补差', 'SettleJob': '结算处理', 'ContractTermination': '合同终止' }
  return map[type] || type || '-'
}

function addEntryRow() {
  voucherForm.entries.push({ subjectId: '', direction: 'Debit', amount: null, summary: '' })
}

function resetVoucherForm() {
  editingVoucherId.value = null
  voucherForm.voucherDate = new Date().toISOString().slice(0, 10)
  voucherForm.description = ''
  voucherForm.entries = [
    { subjectId: '', direction: 'Debit', amount: null, summary: '' },
    { subjectId: '', direction: 'Credit', amount: null, summary: '' }
  ]
}

async function openCreateDialog() {
  resetVoucherForm()
  if (subjects.value.length === 0) {
    try {
      subjects.value = await getAccountingSubjects() || []
    } catch { subjects.value = [] }
  }
  showVoucherForm.value = true
}

async function editVoucher(row) {
  resetVoucherForm()
  editingVoucherId.value = row.id
  if (subjects.value.length === 0) {
    try {
      subjects.value = await getAccountingSubjects() || []
    } catch { subjects.value = [] }
  }
  try {
    const res = await getVoucher(row.id)
    voucherForm.voucherDate = res.voucherDate || ''
    voucherForm.description = res.description || ''
    voucherForm.entries = (res.entries || []).map(e => ({
      subjectId: e.accountingSubjectId || e.subjectId || '',
      direction: e.direction || 'Debit',
      amount: e.amount || 0,
      summary: e.summary || ''
    }))
    showVoucherForm.value = true
  } catch {
    ElMessage.error('加载凭证数据失败')
  }
}

async function saveVoucher() {
  // 校验每行
  for (const [i, e] of voucherForm.entries.entries()) {
    if (!e.subjectId) { ElMessage.warning(`第 ${i+1} 行：请选择科目`); return }
    if (!e.amount || e.amount <= 0) { ElMessage.warning(`第 ${i+1} 行：金额必须大于零`); return }
  }
  saving.value = true
  try {
    const data = {
      voucherDate: voucherForm.voucherDate,
      description: voucherForm.description || null,
      entries: voucherForm.entries.map(e => ({
        accountingSubjectId: e.subjectId,
        direction: e.direction,
        amount: e.amount,
        summary: e.summary || null
      }))
    }
    if (editingVoucherId.value) {
      await updateVoucher(editingVoucherId.value, data)
      ElMessage.success('凭证已更新')
    } else {
      await createVoucher(data)
      ElMessage.success('凭证已创建')
    }
    showVoucherForm.value = false
    await fetchList()
  } catch (e) {
    ElMessage.error(e?.response?.data?.message || '保存失败')
  }
  saving.value = false
}

async function fetchList() {
  loading.value = true
  try {
    const params = { page: pagination.page, pageSize: pagination.pageSize }
    const res = await getVouchers(params)
    const items = res.items || res.data || []
    vouchers.value = items.map(v => ({
      id: v.id,
      voucherNo: v.voucherNo || '',
      voucherDate: v.voucherDate || v.createdAt?.slice(0, 10) || '',
      totalDebit: v.totalDebit ?? 0,
      totalCredit: v.totalCredit ?? 0,
      sourceType: v.sourceEntityType || v.sourceType || '',
      status: (v.status && typeof v.status === 'object' ? v.status.code : v.status) || 'Draft',
      _posting: false,
      _auditing: false,
      _reversing: false
    }))
    pagination.total = res.total ?? items.length
  } catch { ElMessage.error('加载凭证列表失败') }
  finally { loading.value = false }
}

async function viewVoucher(row) {
  try {
    const res = await getVoucher(row.id)
    voucherDetail.value = {
      id: res.id,
      voucherNo: res.voucherNo || '',
      voucherDate: res.voucherDate || '',
      status: (res.status && typeof res.status === 'object' ? res.status.code : res.status) || '',
      description: res.description || '',
      entries: (res.entries || []).map(e => ({
        subjectCode: e.subjectCode || '',
        subjectName: e.subjectName || '',
        summary: e.summary || '',
        direction: e.direction,
        amount: e.amount || 0
      }))
    }
    showVoucherDetail.value = true
  } catch {
    voucherDetail.value = {
      voucherNo: row.voucherNo, voucherDate: row.voucherDate, status: row.status, description: '',
      entries: [{ subjectCode: '-', subjectName: '-', summary: '无法加载明细', direction: 'Debit', amount: 0 }]
    }
    showVoucherDetail.value = true
  }
}

async function postVoucher(row) {
  try {
    await ElMessageBox.confirm(`确定过账凭证 ${row.voucherNo} 吗？`, '确认')
    row._posting = true
    await apiPostVoucher(row.id)
    ElMessage.success('凭证已过账')
    await fetchList()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '过账失败')
  }
  row._posting = false
}

async function auditVoucherHandler(row) {
  try {
    await ElMessageBox.confirm(`确定审核凭证 ${row.voucherNo} 吗？审核后将不可撤销。`, '确认')
    row._auditing = true
    await apiAuditVoucher(row.id)
    ElMessage.success('凭证已审核')
    await fetchList()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '审核失败')
  }
  row._auditing = false
}

async function reverseVoucher(row) {
  try {
    const { value } = await ElMessageBox.prompt(`冲销原因（${row.voucherNo}）：`, '冲销', {
      confirmButtonText: '确定', cancelButtonText: '取消', inputPlaceholder: '请输入冲销原因'
    })
    if (!value) { ElMessage.warning('请输入冲销原因'); return }
    row._reversing = true
    await apiReverseVoucher(row.id, { reason: value })
    ElMessage.success('冲销成功')
    await fetchList()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '冲销失败')
  }
  row._reversing = false
}

onMounted(fetchList)
</script>
