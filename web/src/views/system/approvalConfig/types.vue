<template>
  <div>
    <div class="page-header">
      <h2>审批类型配置</h2>
      <el-button type="primary" @click="showDialog = true">
        <el-icon><Plus /></el-icon>新增审批类型
      </el-button>
    </div>

    <el-table :data="list" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="code" label="类型编码" width="180" />
      <el-table-column prop="name" label="名称" width="150" />
      <el-table-column prop="routingStrategy" label="路由策略" width="120">
        <template #default="{ row }">
          <el-tag size="small">{{ row.routingStrategy === 'Fixed' ? '固定级数' : '金额阈值' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="levelCount" label="级别数" width="80" />
      <el-table-column prop="isActive" label="启用" width="60">
        <template #default="{ row }"><el-switch v-model="row.isActive" /></template>
      </el-table-column>
      <el-table-column label="操作" width="160" fixed="right">
        <template #default="{ row }">
          <el-button text size="small" type="primary" @click="editType(row)">编辑</el-button>
          <el-button text size="small" type="primary" @click="configLevels(row)">级别配置</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="审批类型" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="编码"><el-input v-model="form.code" /></el-form-item>
        <el-form-item label="名称"><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="路由策略">
          <el-select v-model="form.routingStrategy" style="width: 100%">
            <el-option label="固定级数 (Fixed)" value="Fixed" />
            <el-option label="金额阈值 (AmountBased)" value="AmountBased" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const showDialog = ref(false)
const form = ref({ code: '', name: '', routingStrategy: 'Fixed' })

const list = ref([
  { code: 'BATCH_IMPORT_ROOMS', name: '批量导入房屋', routingStrategy: 'Fixed', levelCount: 2, isActive: true },
  { code: 'CONTRACT_CREATE', name: '新建合同', routingStrategy: 'Fixed', levelCount: 1, isActive: true },
  { code: 'CONTRACT_TERMINATE', name: '提前解约', routingStrategy: 'AmountBased', levelCount: 3, isActive: true },
  { code: 'RECEIPT_REVERSE', name: '收款冲销', routingStrategy: 'Fixed', levelCount: 2, isActive: true },
  { code: 'DISCOUNT', name: '应收减免', routingStrategy: 'AmountBased', levelCount: 3, isActive: true }
])

function editType(row) { Object.assign(form.value, row); showDialog.value = true }
function configLevels(row) { ElMessage.info('请前往"审批级别配置"页面') }
function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
