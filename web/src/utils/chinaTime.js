/**
 * 东八区时间工具 — 所有时间相关操作统一使用此模块
 */
export const chinaTime = {
  /** 获取当前东八区时间 */
  now() {
    const d = new Date()
    return new Date(d.getTime() + 8 * 60 * 60 * 1000)
  },
  /** 格式化日期 yyyy-MM-dd */
  today() {
    return this.now().toISOString().slice(0, 10)
  },
  /** 格式化月份 yyyy-MM */
  currentMonth() {
    return this.now().toISOString().slice(0, 7)
  },
  /** 判断是否逾期 */
  isOverdue(dateStr) {
    if (!dateStr) return false
    const due = new Date(dateStr)
    const cnNow = this.now()
    return due < cnNow
  }
}
