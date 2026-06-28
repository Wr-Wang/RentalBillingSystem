<template>
  <div>
    <div class="page-header">
      <h2>节假日管理</h2>
      <div class="table-actions">
        <el-button type="primary" @click="showDialog = true"><el-icon><Plus /></el-icon>新增</el-button>
        <el-button>导入国务院节假日安排</el-button>
      </div>
    </div>

    <div class="search-bar">
      <el-select v-model="searchYear" placeholder="选择年份" style="width: 120px;">
        <el-option v-for="y in [2026, 2027]" :key="y" :label="y + '年'" :value="y" />
      </el-select>
      <el-button type="primary">查询</el-button>
    </div>

    <el-table :data="list" stripe>
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="holidayDate" label="日期" width="120" />
      <el-table-column prop="holidayName" label="名称" width="150" />
      <el-table-column prop="holidayType" label="类型" width="120">
        <template #default="{ row }">
          <el-tag :type="row.holidayType === 'Statutory' ? 'danger' : 'success'" size="small">
            {{ row.holidayType === 'Statutory' ? '法定节假日' : '调休上班日' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="120">
        <template #default="{ row }">
          <el-button text size="small" type="primary">编辑</el-button>
          <el-button text size="small" type="danger">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="节假日" width="500px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="日期"><el-date-picker v-model="form.holidayDate" type="date" style="width: 100%" /></el-form-item>
        <el-form-item label="名称"><el-input v-model="form.holidayName" /></el-form-item>
        <el-form-item label="类型"><el-select v-model="form.holidayType" style="width: 100%"><el-option label="法定节假日" value="Statutory" /><el-option label="调休上班日" value="ExtraWorkday" /></el-select></el-form-item>
      </el-form>
      <template #footer><el-button @click="showDialog = false">取消</el-button><el-button type="primary" @click="save">保存</el-button></template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const showDialog = ref(false)
const searchYear = ref(2026)
const form = ref({ holidayDate: '', holidayName: '', holidayType: 'Statutory' })

const list = ref([
  { holidayDate: '2026-10-01', holidayName: '国庆节', holidayType: 'Statutory' },
  { holidayDate: '2026-10-02', holidayName: '国庆节', holidayType: 'Statutory' },
  { holidayDate: '2026-10-07', holidayName: '国庆节', holidayType: 'Statutory' },
  { holidayDate: '2026-10-10', holidayName: '国庆调休', holidayType: 'ExtraWorkday' },
  { holidayDate: '2026-10-11', holidayName: '国庆调休', holidayType: 'ExtraWorkday' }
])

function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
