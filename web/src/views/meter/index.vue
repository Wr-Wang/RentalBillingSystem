<template>
  <div>
    <div class="page-header">
      <h2>抄表管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="showBatchImport = true">
          <el-icon><Upload /></el-icon>Excel批量导入
        </el-button>
        <el-button @click="estimateAll">逾期估读</el-button>
      </div>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="search.month" type="month" placeholder="选择月份" />
      <el-select v-model="search.feeCode" placeholder="费用类型" clearable style="width: 140px;">
        <el-option label="水费" value="WATER" />
        <el-option label="电费" value="ELECTRIC" />
        <el-option label="燃气费" value="GAS" />
      </el-select>
      <el-select v-model="search.status" placeholder="状态" clearable style="width: 140px;">
        <el-option label="待录入" value="Draft" />
        <el-option label="已录入" value="Confirmed" />
        <el-option label="已出账" value="Billed" />
        <el-option label="已估读" value="Estimated" />
      </el-select>
      <el-input v-model="search.keyword" placeholder="房屋/合同号" clearable style="width: 180px;" />
      <el-button type="primary">查询</el-button>
    </div>

    <el-row :gutter="16" style="margin-bottom: 16px;">
      <el-col :span="6">
        <el-statistic title="总户数" :value="25" />
      </el-col>
      <el-col :span="6">
        <el-statistic title="待录入" :value="12" />
      </el-col>
      <el-col :span="6">
        <el-statistic title="已录入" :value="10" />
      </el-col>
      <el-col :span="6">
        <el-statistic title="已估读" :value="3" />
      </el-col>
    </el-row>

    <el-card>
      <el-table :data="meterReadings" stripe>
        <el-table-column prop="roomName" label="房屋" width="100" />
        <el-table-column prop="contractNo" label="合同号" width="130" />
        <el-table-column prop="feeName" label="项目" width="80" />
        <el-table-column prop="previousReading" label="上期读数" width="100" />
        <el-table-column label="本期读数">
          <template #default="{ row }">
            <el-input-number v-if="row.status === 'Draft'" v-model="row.currentReading" :min="0" :precision="2" size="small" style="width: 120px;" />
            <span v-else>{{ row.currentReading }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="consumption" label="用量" width="80">
          <template #default="{ row }">
            {{ row.currentReading && row.previousReading ? (row.currentReading - row.previousReading).toFixed(2) : '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Billed' ? 'success' : row.status === 'Confirmed' ? 'primary' : row.status === 'Estimated' ? 'warning' : 'info'" size="small">
              {{ row.status === 'Draft' ? '待录入' : row.status === 'Confirmed' ? '已录入' : row.status === 'Billed' ? '已出账' : '已估读' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button v-if="row.status === 'Draft'" text size="small" type="primary" @click="saveReading(row)">保存</el-button>
            <el-button v-if="row.status === 'Confirmed' || row.status === 'Estimated'" text size="small" type="primary" @click="confirmReading(row)">确认</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

const search = reactive({ month: new Date(), feeCode: '', status: '', keyword: '' })

const meterReadings = ref([
  { roomName: 'A-301', contractNo: 'HT-2026-001', feeName: '水费', previousReading: 1234, currentReading: null, consumption: null, status: 'Draft' },
  { roomName: 'A-301', contractNo: 'HT-2026-001', feeName: '电费', previousReading: 5678, currentReading: null, consumption: null, status: 'Draft' },
  { roomName: 'A-301', contractNo: 'HT-2026-001', feeName: '燃气费', previousReading: 890, currentReading: null, consumption: null, status: 'Draft' },
  { roomName: 'A-302', contractNo: 'HT-2026-002', feeName: '水费', previousReading: 2345, currentReading: 2456, consumption: 111, status: 'Confirmed' },
  { roomName: 'A-302', contractNo: 'HT-2026-002', feeName: '电费', previousReading: 6789, currentReading: 7123, consumption: 334, status: 'Confirmed' },
  { roomName: 'B-101', contractNo: 'HT-2026-003', feeName: '水费', previousReading: 3456, currentReading: 3512, consumption: 56, status: 'Estimated' }
])

function saveReading(row) {
  if (!row.currentReading) {
    ElMessage.warning('请输入读数')
    return
  }
  row.status = 'Confirmed'
  ElMessage.success('读数已保存')
}

function confirmReading(row) {
  ElMessage.success('读数已确认')
}

function estimateAll() {
  ElMessage.success('逾期读数已自动估读')
}
</script>
