<template>
  <div>
    <div class="page-header">
      <h2>审批级别配置</h2>
    </div>

    <div class="search-bar">
      <el-select v-model="selectedType" placeholder="选择审批类型" style="width: 250px;">
        <el-option v-for="t in types" :key="t.code" :label="t.name" :value="t.code" />
      </el-select>
    </div>

    <el-card>
      <template #header>
        <span>{{ currentType?.name || '请选择审批类型' }} - 级别列表</span>
        <el-button text type="primary" style="float: right;" @click="showDialog = true">新增级别</el-button>
      </template>

      <el-table :data="levels" stripe v-if="currentType">
        <el-table-column type="index" label="级别" width="60" />
        <el-table-column prop="levelNo" label="级序号" width="80" />
        <el-table-column prop="approverRole" label="审批角色" width="150" />
        <el-table-column prop="approvalMode" label="审批模式" width="100">
          <template #default="{ row }">
            <el-tag size="small">{{ row.approvalMode === 'AnyOne' ? '一人通过' : '全部会签' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="minAmount" label="最小金额" width="100">
          <template #default="{ row }">{{ row.minAmount !== null ? '¥' + row.minAmount : '-' }}</template>
        </el-table-column>
        <el-table-column prop="maxAmount" label="最大金额" width="100">
          <template #default="{ row }">{{ row.maxAmount !== null ? '¥' + row.maxAmount : '-' }}</template>
        </el-table-column>
        <el-table-column prop="cumulativeCheck" label="累计检查" width="80">
          <template #default="{ row }"><el-tag :type="row.cumulativeCheck ? 'warning' : 'info'" size="small">{{ row.cumulativeCheck ? '启用' : '关闭' }}</el-tag></template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button text size="small" type="primary">编辑</el-button>
            <el-button text size="small" type="danger">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-empty v-else description="请先选择审批类型" />
    </el-card>

    <el-dialog v-model="showDialog" title="新增审批级别" width="500px">
      <el-form :model="form" label-width="120px">
        <el-form-item label="审批角色">
          <el-select v-model="form.approverRole" style="width: 100%">
            <el-option label="运营主管" value="运营主管" />
            <el-option label="部门经理" value="部门经理" />
            <el-option label="财务主管" value="财务主管" />
            <el-option label="财务总监" value="财务总监" />
            <el-option label="总经理" value="总经理" />
          </el-select>
        </el-form-item>
        <el-form-item label="审批模式">
          <el-select v-model="form.approvalMode" style="width: 100%">
            <el-option label="一人通过(AnyOne)" value="AnyOne" />
            <el-option label="全部会签(AllOf)" value="AllOf" />
          </el-select>
        </el-form-item>
        <el-form-item label="最小金额">
          <el-input-number v-model="form.minAmount" :min="0" :precision="2" style="width: 100%" />
        </el-form-item>
        <el-form-item label="最大金额">
          <el-input-number v-model="form.maxAmount" :min="0" :precision="2" style="width: 100%" />
        </el-form-item>
        <el-form-item label="累计检查">
          <el-switch v-model="form.cumulativeCheck" />
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
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'

const showDialog = ref(false)
const selectedType = ref('')
const form = ref({ approverRole: '', approvalMode: 'AnyOne', minAmount: 0, maxAmount: 99999999, cumulativeCheck: false })

const types = ref([
  { code: 'CONTRACT_TERMINATE', name: '提前解约', strategy: 'AmountBased' },
  { code: 'DISCOUNT', name: '应收减免', strategy: 'AmountBased' },
  { code: 'BATCH_IMPORT_ROOMS', name: '批量导入房屋', strategy: 'Fixed' }
])

const levelsData = {
  'CONTRACT_TERMINATE': [
    { levelNo: 1, approverRole: '运营主管', approvalMode: 'AnyOne', minAmount: 0, maxAmount: 5000, cumulativeCheck: false },
    { levelNo: 2, approverRole: '部门经理', approvalMode: 'AnyOne', minAmount: 5000, maxAmount: 50000, cumulativeCheck: false },
    { levelNo: 3, approverRole: '总经理', approvalMode: 'AnyOne', minAmount: 50000, maxAmount: 99999999, cumulativeCheck: false }
  ],
  'DISCOUNT': [
    { levelNo: 1, approverRole: '运营主管', approvalMode: 'AnyOne', minAmount: 0, maxAmount: 5000, cumulativeCheck: true },
    { levelNo: 2, approverRole: '部门经理', approvalMode: 'AnyOne', minAmount: 5000, maxAmount: 50000, cumulativeCheck: false },
    { levelNo: 3, approverRole: '总经理', approvalMode: 'AnyOne', minAmount: 50000, maxAmount: 99999999, cumulativeCheck: false }
  ],
  'BATCH_IMPORT_ROOMS': [
    { levelNo: 1, approverRole: '运营主管', approvalMode: 'AnyOne', minAmount: null, maxAmount: null, cumulativeCheck: false },
    { levelNo: 2, approverRole: '部门经理', approvalMode: 'AnyOne', minAmount: null, maxAmount: null, cumulativeCheck: false }
  ]
}

const currentType = computed(() => types.value.find(t => t.code === selectedType.value))
const levels = computed(() => levelsData[selectedType.value] || [])

function save() { ElMessage.success('保存成功'); showDialog.value = false }
</script>
