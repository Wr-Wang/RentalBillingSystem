<template>
  <div>
    <div class="page-header">
      <h2>收款日报</h2>
    </div>

    <div class="search-bar">
      <el-date-picker v-model="searchDate" type="date" placeholder="选择日期" />
      <el-button type="primary">查询</el-button>
      <el-button><el-icon><Download /></el-icon>导出</el-button>
    </div>

    <div class="stat-cards">
      <div class="stat-card"><div class="label">收款笔数</div><div class="value">12</div></div>
      <div class="stat-card"><div class="label">收款总额</div><div class="value" style="color: #67c23a;">¥85,600</div></div>
      <div class="stat-card"><div class="label">银行转账</div><div class="value" style="color: #409eff;">¥65,200</div></div>
      <div class="stat-card"><div class="label">支付宝/微信</div><div class="value" style="color: #409eff;">¥20,400</div></div>
    </div>

    <el-card>
      <el-table :data="dailyList" stripe>
        <el-table-column prop="receiptNo" label="收据号" width="160" />
        <el-table-column prop="contractNo" label="合同号" width="120" />
        <el-table-column prop="tenantName" label="租客" width="100" />
        <el-table-column prop="amount" label="金额" width="110">
          <template #default="{ row }">¥{{ row.amount?.toLocaleString() }}</template>
        </el-table-column>
        <el-table-column prop="channelName" label="支付方式" width="100" />
        <el-table-column prop="status" label="状态" width="90">
          <template #default="{ row }"><el-tag :type="row.status === 'Confirmed' ? 'success' : 'warning'" size="small">{{ row.status === 'Confirmed' ? '已确认' : '待确认' }}</el-tag></template>
        </el-table-column>
        <el-table-column prop="receivedTime" label="收款时间" width="150" />
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const searchDate = ref(new Date())
const dailyList = ref([
  { receiptNo: 'SJ-20260627-001', contractNo: 'HT-2026-001', tenantName: '张三', amount: 5460, channelName: '银行转账', status: 'Confirmed', receivedTime: '2026-06-27 09:15' },
  { receiptNo: 'SJ-20260627-002', contractNo: 'HT-2026-005', tenantName: '孙七', amount: 4500, channelName: '支付宝', status: 'PendingConfirm', receivedTime: '2026-06-27 10:30' },
  { receiptNo: 'SJ-20260627-003', contractNo: 'HT-2026-008', tenantName: '周八', amount: 8200, channelName: '银行转账', status: 'Confirmed', receivedTime: '2026-06-27 11:00' }
])
</script>
