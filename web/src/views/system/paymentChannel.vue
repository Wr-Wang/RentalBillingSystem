<template>
  <div>
    <div class="page-header">
      <h2>支付通道管理</h2>
      <el-button type="primary" @click="showDialog = true"><el-icon><Plus /></el-icon>新增通道</el-button>
    </div>

    <el-table :data="list" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="code" label="编码" width="120" />
      <el-table-column prop="name" label="名称" width="150" />
      <el-table-column prop="channelType" label="通道类型" width="120" />
      <el-table-column prop="accountNo" label="账号" width="200" />
      <el-table-column prop="isActive" label="启用" width="60">
        <template #default="{ row }"><el-switch v-model="row.isActive" /></template>
      </el-table-column>
      <el-table-column label="操作" width="120">
        <template #default="{ row }"><el-button text size="small" type="primary">编辑</el-button></template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="支付通道" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="编码"><el-input v-model="form.code" /></el-form-item>
        <el-form-item label="名称"><el-input v-model="form.name" /></el-form-item>
        <el-form-item label="通道类型"><el-select v-model="form.channelType" style="width: 100%"><el-option label="银行转账" value="Bank" /><el-option label="支付宝" value="Alipay" /><el-option label="微信支付" value="WeChat" /><el-option label="现金" value="Cash" /></el-select></el-form-item>
        <el-form-item label="账号"><el-input v-model="form.accountNo" /></el-form-item>
      </el-form>
      <template #footer><el-button @click="showDialog = false">取消</el-button><el-button type="primary" @click="save">保存</el-button></template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const showDialog = ref(false)
const form = ref({ code: '', name: '', channelType: 'Bank', accountNo: '' })

const list = ref([
  { code: 'ICBC', name: '工商银行', channelType: 'Bank', accountNo: '6222 0200 1234 5678', isActive: true },
  { code: 'ALIPAY', name: '支付宝', channelType: 'Alipay', accountNo: 'admin@rental.com', isActive: true },
  { code: 'WECHAT', name: '微信支付', channelType: 'WeChat', accountNo: 'wx_rental_001', isActive: true },
  { code: 'CASH', name: '现金', channelType: 'Cash', accountNo: '-', isActive: true }
])

function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
