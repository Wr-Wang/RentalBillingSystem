<template>
  <div>
    <div class="page-header">
      <h2>凭证管理</h2>
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
      <el-table-column prop="sourceType" label="来源" width="100" />
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Posted' ? 'success' : row.status === 'Reversed' ? 'danger' : 'info'" size="small">
            {{ row.status === 'Draft' ? '草稿' : row.status === 'Posted' ? '已过账' : '已冲销' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="160" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="viewVoucher(row)">查看</el-button>
          <el-button text size="small" type="success" v-if="row.status === 'Draft'" :loading="row._posting" @click="postVoucher(row)">过账</el-button>
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
            <el-tag :type="voucherDetail.status === 'Posted' ? 'success' : 'info'" size="small">{{ voucherDetail.status === 'Posted' ? '已过账' : '草稿' }}</el-tag>
          </el-descriptions-item>
        </el-descriptions>

        <el-table :data="voucherDetail.entries" stripe>
          <el-table-column type="index" label="行号" width="60" />
          <el-table-column prop="subjectCode" label="科目编码" width="100" />
          <el-table-column prop="subjectName" label="科目名称" width="180" />
          <el-table-column prop="summary" label="摘要" min-width="150" />
          <el-table-column label="借方金额" width="120">
            <template #default="{ row }">{{ row.debitAmount ? '¥' + row.debitAmount?.toLocaleString() : '-' }}</template>
          </el-table-column>
          <el-table-column label="贷方金额" width="120">
            <template #default="{ row }">{{ row.creditAmount ? '¥' + row.creditAmount?.toLocaleString() : '-' }}</template>
          </el-table-column>
        </el-table>
        <div style="margin-top: 8px; text-align: right; font-weight: bold;">
          借方合计: ¥{{ entriesDebitTotal.toLocaleString() }} | 贷方合计: ¥{{ entriesCreditTotal.toLocaleString() }}
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getVouchers, getVoucher, postVoucher as apiPostVoucher, reverseVoucher as apiReverseVoucher } from '../../api/index'

const loading = ref(false)
const vouchers = ref([])
const pagination = reactive({ page: 1, pageSize: 10, total: 0 })
const showVoucherDetail = ref(false)

const voucherDetail = ref({ voucherNo: '', voucherDate: '', status: '', entries: [] })
const entriesDebitTotal = computed(() => voucherDetail.value.entries.reduce((s, e) => s + (e.debitAmount || 0), 0))
const entriesCreditTotal = computed(() => voucherDetail.value.entries.reduce((s, e) => s + (e.creditAmount || 0), 0))

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
      totalDebit: v.totalDebit || 0,
      totalCredit: v.totalCredit || 0,
      sourceType: v.sourceType || '',
      status: v.status || 'Draft',
      _posting: false,
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
      status: res.status || '',
      entries: (res.entries || []).map(e => ({
        subjectCode: e.subjectCode || '',
        subjectName: e.subjectName || '',
        summary: e.summary || '',
        debitAmount: e.direction === 'Debit' ? e.amount || 0 : 0,
        creditAmount: e.direction === 'Credit' ? e.amount || 0 : 0
      }))
    }
    showVoucherDetail.value = true
  } catch {
    // 降级显示
    voucherDetail.value = {
      voucherNo: row.voucherNo, voucherDate: row.voucherDate, status: row.status,
      entries: [
        { subjectCode: '-', subjectName: '-', summary: '无法加载明细', debitAmount: 0, creditAmount: 0 }
      ]
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
    if (e !== 'cancel') ElMessage.error('过账失败')
  }
  row._posting = false
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
    if (e !== 'cancel') ElMessage.error('冲销失败')
  }
  row._reversing = false
}

onMounted(fetchList)
</script>
