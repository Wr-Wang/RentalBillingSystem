<template>
  <div>
    <h1>总账余额表</h1>
    <el-card shadow="never" style="margin-top: 16px;">
      <el-form :inline="true">
        <el-form-item label="期间">
          <el-date-picker v-model="period" type="month" placeholder="选择期间" value-format="yyyy-MM" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchData">查询</el-button>
        </el-form-item>
      </el-form>
      <el-table :data="list" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="period" label="期间" width="100" />
        <el-table-column prop="openingBalance" label="期初应收" width="140" align="right" />
        <el-table-column prop="totalBilled" label="本期出账" width="140" align="right" />
        <el-table-column prop="totalReceived" label="本期收款" width="140" align="right" />
        <el-table-column prop="closingBalance" label="期末应收" width="140" align="right" />
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getGLBalance } from '@/api'

const list = ref([])
const loading = ref(false)
const period = ref('')

const fetchData = async () => {
  loading.value = true
  try {
    const res = await getGLBalance({ period: period.value })
    list.value = res || []
  } finally {
    loading.value = false
  }
}

onMounted(fetchData)
</script>
