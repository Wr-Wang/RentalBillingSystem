import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('src', import.meta.url))
    }
  },
  build: {
    // 大体积第三方库（element-plus / echarts 等）单独拆包，避免打进同一个
    // chunk 导致体积过大；同时提高告警阈值（这些库本身就无法压到 500 kB 以下）
    chunkSizeWarningLimit: 1500,
    rolldownOptions: {
      checks: {
        // element-plus 的传递依赖 @vueuse/core 里有位置不合规的
        // /* #__PURE__ */ 注释，Rolldown 会输出 INVALID_ANNOTATION 告警。
        // 这是上游库的写法问题，不影响产物，关闭该检查避免构建日志被红色刷屏。
        invalidAnnotation: false
      },
      output: {
        codeSplitting: {
          groups: [
            { name: 'element-plus', test: /node_modules[\\/](element-plus|@element-plus)[\\/]/, priority: 30 },
            { name: 'echarts', test: /node_modules[\\/](echarts|zrender|vue-echarts)[\\/]/, priority: 30 },
            { name: 'xlsx', test: /node_modules[\\/]xlsx[\\/]/, priority: 30 },
            { name: 'exceljs', test: /node_modules[\\/]exceljs[\\/]/, priority: 30 },
            { name: 'vendor', test: /node_modules[\\/]/, priority: 10 }
          ]
        }
      }
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://www.rbsapi.com',//http://localhost:5178
        changeOrigin: true
      }
    },
    historyApiFallback: true
  }
})
