<template>
  <div>
    <div class="page-header">
      <h2>批量导入房屋</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-card style="margin-bottom: 16px;">
      <template #header>导入说明</template>
      <el-alert
        title="请先下载模板，按模板格式填写数据后上传。批量导入需要经过2级审批（运营主管 → 部门经理）后方可生效。"
        type="info"
        show-icon
        :closable="false"
      />
      <div style="margin-top: 16px;">
        <el-button type="primary"><el-icon><Download /></el-icon>下载导入模板</el-button>
      </div>
    </el-card>

    <el-card>
      <template #header>上传数据</template>
      <el-upload
        drag
        action="#"
        :auto-upload="false"
        style="margin-bottom: 16px;"
      >
        <el-icon class="el-icon--upload" :size="48"><UploadFilled /></el-icon>
        <div class="el-upload__text">
          将 Excel 文件拖到此处，或 <em>点击选择</em>
        </div>
        <template #tip>
          <div class="el-upload__tip">
            支持 .xlsx 格式，单次不超过10000条记录
          </div>
        </template>
      </el-upload>

      <el-table :data="previewData" stripe style="margin-bottom: 16px;">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="buildingName" label="座楼" />
        <el-table-column prop="floorNo" label="楼层" />
        <el-table-column prop="roomNo" label="房间号" />
        <el-table-column prop="roomType" label="房型" />
        <el-table-column prop="area" label="面积(m²)" />
        <el-table-column prop="status" label="状态" />
      </el-table>

      <div style="text-align: center;">
        <el-button type="primary" size="large" @click="submitImport">
          <el-icon><Upload /></el-icon>提交审批导入
        </el-button>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const previewData = ref([
  { buildingName: 'A栋', floorNo: 3, roomNo: '301', roomType: '两室一厅', area: 90, status: '空置' },
  { buildingName: 'A栋', floorNo: 3, roomNo: '302', roomType: '一室一厅', area: 55, status: '空置' },
  { buildingName: 'B栋', floorNo: 5, roomNo: '501', roomType: '三室一厅', area: 120, status: '空置' }
])

function submitImport() {
  ElMessage.success('已提交审批，请等待运营主管和部门经理审批')
}
</script>
