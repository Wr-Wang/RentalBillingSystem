<template>
  <div style="display: flex; justify-content: center; align-items: center; min-height: 60vh; flex-direction: column;">
    <h1 style="font-size: 72px; color: #409eff; margin: 0;">404</h1>
    <p style="font-size: 20px; color: #909399; margin: 16px 0 24px;">页面不存在</p>
    <el-button type="primary" @click="$router.push('/dashboard')">返回首页</el-button>
  </div>
</template>
