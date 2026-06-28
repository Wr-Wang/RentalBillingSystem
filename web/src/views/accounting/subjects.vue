<template>
  <div>
    <div class="page-header">
      <h2>会计科目表</h2>
      <el-button type="primary" @click="showAddSubject = true">
        <el-icon><Plus /></el-icon>新增科目
      </el-button>
    </div>

    <el-card>
      <el-table :data="subjectTree" stripe row-key="id" default-expand-all :tree-props="{ children: 'children' }">
        <el-table-column prop="code" label="科目编码" width="120" />
        <el-table-column prop="name" label="科目名称" min-width="200" />
        <el-table-column prop="direction" label="方向" width="80">
          <template #default="{ row }">{{ row.direction === 'Debit' ? '借方' : '贷方' }}</template>
        </el-table-column>
        <el-table-column prop="level" label="级别" width="60" />
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button text size="small" type="primary" @click="editSubject(row)">编辑</el-button>
            <el-button text size="small" type="danger" @click="deleteSubject(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="showAddSubject" title="新增会计科目" width="500px">
      <el-form :model="subjectForm" label-width="100px">
        <el-form-item label="科目编码">
          <el-input v-model="subjectForm.code" />
        </el-form-item>
        <el-form-item label="科目名称">
          <el-input v-model="subjectForm.name" />
        </el-form-item>
        <el-form-item label="方向">
          <el-select v-model="subjectForm.direction" style="width: 100%">
            <el-option label="借方" value="Debit" />
            <el-option label="贷方" value="Credit" />
          </el-select>
        </el-form-item>
        <el-form-item label="上级科目">
          <el-tree-select v-model="subjectForm.parentId" :data="subjectTree" :props="{ label: 'name', value: 'id' }" placeholder="不选则为顶级" clearable style="width: 100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddSubject = false">取消</el-button>
        <el-button type="primary" @click="saveSubject">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const showAddSubject = ref(false)
const subjectForm = ref({ code: '', name: '', direction: 'Debit', parentId: null })

const subjectTree = ref([
  {
    id: 's1', code: '1000', name: '资产类', direction: 'Debit', level: 1, children: [
      { id: 's1-1', code: '1001', name: '库存现金', direction: 'Debit', level: 2, parentId: 's1' },
      { id: 's1-2', code: '1002', name: '银行存款', direction: 'Debit', level: 2, parentId: 's1' },
      { id: 's1-3', code: '1122', name: '应收账款', direction: 'Debit', level: 2, children: [
        { id: 's1-3-1', code: '112201', name: '应收账款-房租', direction: 'Debit', level: 3 },
        { id: 's1-3-2', code: '112202', name: '应收账款-水费', direction: 'Debit', level: 3 },
        { id: 's1-3-3', code: '112210', name: '应收账款-滞纳金', direction: 'Debit', level: 3 }
      ]}
    ]
  },
  {
    id: 's2', code: '2000', name: '负债类', direction: 'Credit', level: 1, children: [
      { id: 's2-1', code: '2221', name: '应交税费', direction: 'Credit', level: 2, children: [
        { id: 's2-1-1', code: '222101', name: '应交税费-增值税(6%)', direction: 'Credit', level: 3 }
      ]}
    ]
  },
  {
    id: 's3', code: '6000', name: '收入类', direction: 'Credit', level: 1, children: [
      { id: 's3-1', code: '6001', name: '主营业务收入', direction: 'Credit', level: 2, children: [
        { id: 's3-1-1', code: '600101', name: '主营业务收入-房租', direction: 'Credit', level: 3 },
        { id: 's3-1-2', code: '600102', name: '主营业务收入-水费', direction: 'Credit', level: 3 },
        { id: 's3-1-3', code: '600110', name: '主营业务收入-滞纳金', direction: 'Credit', level: 3 }
      ]}
    ]
  }
])

function editSubject(row) { ElMessage.info('编辑功能待实现') }
function deleteSubject(row) {
  ElMessageBox.confirm(`确定删除科目 ${row.name} 吗？`, '提示').then(() => {
    ElMessage.success('科目已删除')
  }).catch(() => {})
}
function saveSubject() {
  ElMessage.success('科目已创建')
  showAddSubject.value = false
}
</script>
