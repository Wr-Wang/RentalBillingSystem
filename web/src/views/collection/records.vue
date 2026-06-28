<template>
  <div>
    <div class="page-header">
      <h2>催缴记录</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <div class="search-bar">
      <el-input v-model="search.keyword" placeholder="合同号/租客" clearable style="width: 200px;" />
      <el-select v-model="search.channel" placeholder="催缴方式" clearable style="width: 140px;">
        <el-option label="短信" value="SMS" />
        <el-option label="电话" value="Phone" />
        <el-option label="系统通知" value="WeChat" />
      </el-select>
      <el-date-picker v-model="search.dateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" style="width: 220px;" />
      <el-button type="primary">查询</el-button>
    </div>

    <el-table :data="records" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="contractNo" label="合同号" width="120" />
      <el-table-column prop="tenantName" label="租客" width="100" />
      <el-table-column prop="stageNo" label="阶段" width="60" />
      <el-table-column prop="channel" label="方式" width="80">
        <template #default="{ row }">
          <el-tag size="small">{{ row.channel === 'SMS' ? '短信' : row.channel === 'Phone' ? '电话' : '系统通知' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="content" label="催缴内容" min-width="250" show-overflow-tooltip />
      <el-table-column prop="status" label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="row.status === 'Sent' ? 'success' : 'danger'" size="small">{{ row.status === 'Sent' ? '已发送' : '失败' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="sentAt" label="发送时间" width="160" />
    </el-table>

    <div style="margin-top: 16px; text-align: right;">
      <el-pagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="pagination.total" layout="total, sizes, prev, pager, next" />
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'

const search = reactive({ keyword: '', channel: '', dateRange: null })
const pagination = reactive({ page: 1, pageSize: 10, total: 50 })

const records = ref([
  { contractNo: 'HT-2026-005', tenantName: '孙七', stageNo: 1, channel: 'SMS', content: '【房租提醒】您好，您本月的房租已逾期，请尽快缴纳。', status: 'Sent', sentAt: '2026-06-20 09:00:00' },
  { contractNo: 'HT-2026-005', tenantName: '孙七', stageNo: 2, channel: 'SMS', content: '【催缴通知】您的房租已逾期7天，请及时缴纳避免产生滞纳金。', status: 'Sent', sentAt: '2026-06-27 09:00:00' },
  { contractNo: 'HT-2026-008', tenantName: '周八', stageNo: 1, channel: 'SMS', content: '【房租提醒】您好，您本月的房租已逾期，请尽快缴纳。', status: 'Sent', sentAt: '2026-06-18 09:00:00' }
])
</script>
