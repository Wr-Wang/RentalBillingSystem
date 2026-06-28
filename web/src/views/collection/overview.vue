<template>
  <div>
    <div class="page-header">
      <h2>催缴管理</h2>
      <div class="table-actions">
        <el-button @click="$router.push('/collection/config')">
          <el-icon><Setting /></el-icon>催缴配置
        </el-button>
        <el-button @click="$router.push('/collection/records')">
          <el-icon><Tickets /></el-icon>催缴记录
        </el-button>
      </div>
    </div>

    <!-- Overview Stats -->
    <div class="stat-cards">
      <div class="stat-card" style="border-left: 4px solid #909399;">
        <div class="label">逾期1-7天（短信提醒）</div>
        <div class="value" style="color: #909399;">{{ stats.stage1 }}</div>
        <div class="sub">即将进入下一阶段</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #e6a23c;">
        <div class="label">逾期8-15天（电话催缴）</div>
        <div class="value" style="color: #e6a23c;">{{ stats.stage2 }}</div>
        <div class="sub">需重点跟进</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #f56c6c;">
        <div class="label">逾期16-30天（催缴函）</div>
        <div class="value" style="color: #f56c6c;">{{ stats.stage3 }}</div>
        <div class="sub">发送正式催缴函</div>
      </div>
      <div class="stat-card" style="border-left: 4px solid #c03636;">
        <div class="label">逾期30天+（法律催缴）</div>
        <div class="value" style="color: #c03636;">{{ stats.stage4 }}</div>
        <div class="sub">法务介入</div>
      </div>
    </div>

    <el-row :gutter="16">
      <!-- Manual Collection -->
      <el-col :span="12">
        <el-card style="margin-bottom: 16px;">
          <template #header>手动催缴</template>
          <el-form :model="manualForm" label-width="100px">
            <el-form-item label="合同号">
              <el-select v-model="manualForm.contractId" filterable style="width: 100%">
                <el-option v-for="c in overdueContracts" :key="c.id" :label="c.contractNo + ' - ' + c.tenantName" :value="c.id" />
              </el-select>
            </el-form-item>
            <el-form-item label="催缴方式">
              <el-select v-model="manualForm.channel" style="width: 100%">
                <el-option label="短信" value="SMS" />
                <el-option label="电话" value="Phone" />
                <el-option label="系统通知" value="WeChat" />
              </el-select>
            </el-form-item>
            <el-form-item label="催缴内容">
              <el-input v-model="manualForm.content" type="textarea" :rows="3" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="sendManual">发送</el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <!-- Overdue List -->
      <el-col :span="12">
        <el-card style="margin-bottom: 16px;">
          <template #header>逾期合同列表</template>
          <el-table :data="overdueContracts" stripe size="small">
            <el-table-column prop="contractNo" label="合同号" width="120" />
            <el-table-column prop="tenantName" label="租客" width="80" />
            <el-table-column prop="overdueAmount" label="欠费金额" width="100">
              <template #default="{ row }">¥{{ row.overdueAmount?.toLocaleString() }}</template>
            </el-table-column>
            <el-table-column prop="overdueDays" label="逾期天数" width="80" />
            <el-table-column label="阶段" width="80">
              <template #default="{ row }">
                <el-tag :type="row.stage <= 1 ? 'info' : row.stage <= 2 ? 'warning' : 'danger'" size="small">
                  S{{ row.stage }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

const stats = reactive({ stage1: 12, stage2: 7, stage3: 3, stage4: 1 })

const manualForm = reactive({
  contractId: '', channel: 'SMS',
  content: '您好，您的房租已逾期，请尽快缴纳以免产生滞纳金。'
})

const overdueContracts = ref([
  { id: 'c1', contractNo: 'HT-2026-005', tenantName: '孙七', overdueAmount: 4500, overdueDays: 22, stage: 3 },
  { id: 'c2', contractNo: 'HT-2026-008', tenantName: '周八', overdueAmount: 8200, overdueDays: 15, stage: 2 },
  { id: 'c3', contractNo: 'HT-2026-012', tenantName: '吴九', overdueAmount: 3800, overdueDays: 8, stage: 2 },
  { id: 'c4', contractNo: 'HT-2026-015', tenantName: '郑十', overdueAmount: 6000, overdueDays: 3, stage: 1 }
])

function sendManual() {
  ElMessage.success('催缴已发送')
}
</script>
