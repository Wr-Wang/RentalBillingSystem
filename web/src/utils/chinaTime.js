/**
 * 东八区时间工具 — 所有时间相关操作统一使用此模块
 */

/** 个位补零 */
const pad = n => String(n).padStart(2, '0')

/** 月份名称 → 数字映射 */
const monthMap = {
  Jan:'01', Feb:'02', Mar:'03', Apr:'04', May:'05', Jun:'06',
  Jul:'07', Aug:'08', Sep:'09', Oct:'10', Nov:'11', Dec:'12'
}

/**
 * 从字符串中提取英文月份日期格式（Mon DD YYYY / DD Mon YYYY）
 * @param {string} d
 * @returns {string|undefined} yyyy-MM-dd 或 undefined
 */
function parseEnglishDate(d) {
  const m = d.match(/\b(\d{4})\b.*\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b.*\b(\d{1,2})\b/i)
    || d.match(/\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b.*\b(\d{1,2})\b.*\b(\d{4})\b/i)
  if (!m) return undefined
  const year = m[1].length === 4 ? m[1] : m[3]
  const month = monthMap[(m[1].length === 4 ? m[2] : m[1]).toLowerCase()]
  const day = pad(m[1].length === 4 ? m[3] : m[2])
  if (year && month) return `${year}-${month}-${day}`
  return undefined
}

/**
 * 尝试将任意输入解析为 Date 对象
 */
function toDate(d) {
  if (!d) return null
  if (d instanceof Date && !isNaN(d.getTime())) return d
  if (typeof d === 'string') {
    // ISO 格式直接解析
    const dt = new Date(d.includes('T') ? d : d.replace(' ', 'T'))
    if (!isNaN(dt.getTime())) return dt
  }
  const dt = new Date(d)
  if (!isNaN(dt.getTime())) return dt
  return null
}

/**
 * 将任意格式的日期/时间输入转为 yyyy-MM-dd 格式
 * 兼容 Date 对象、ISO 字符串、英文月份格式（Mon DD YYYY）
 * @param {any} d
 * @returns {string}
 */
export function formatDate(d) {
  if (!d) return ''
  if (typeof d === 'string' && /^\d{4}-\d{2}-\d{2}/.test(d)) return d.slice(0, 10)
  // 尝试标准解析
  const dt = toDate(d)
  if (dt) return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}`
  // 兜底：英文月份格式
  if (typeof d === 'string') {
    const parsed = parseEnglishDate(d)
    if (parsed) return parsed
    return d.slice(0, 10)
  }
  return ''
}

/**
 * 将任意格式的日期/时间输入转为 yyyy-MM-dd HH:mm 格式
 * @param {any} d
 * @returns {string}
 */
export function formatTime(d) {
  if (!d) return ''
  if (typeof d === 'string' && /^\d{4}-\d{2}-\d{2} \d{2}:\d{2}/.test(d)) return d.slice(0, 16)
  const dt = toDate(d)
  if (dt) {
    return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())} ${pad(dt.getHours())}:${pad(dt.getMinutes())}`
  }
  if (typeof d === 'string') return String(d).slice(0, 16)
  return ''
}

/**
 * 将任意格式的日期/时间输入转为 yyyy-MM-dd HH:mm:ss 格式
 * @param {any} d
 * @returns {string}
 */
export function formatDateTime(d) {
  if (!d) return ''
  const dt = toDate(d)
  if (dt) {
    return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())} ${pad(dt.getHours())}:${pad(dt.getMinutes())}:${pad(dt.getSeconds())}`
  }
  if (typeof d === 'string') return String(d).slice(0, 19)
  return ''
}

/**
 * 根据出账时间计算账单月
 *
 * 账单月规则：每月 25日 20:00 为截点，
 *   25日20:00 ~ 次月24日20:00 期间出账 → 账单月 = 当前月 + 2
 *   次月25日20:00 之前出账 → 账单月 = 当前月 + 1
 *
 * 示例：
 *   6月25日20:00 ~ 7月24日20:00 出账 → 账单月 = 2026-08
 *   7月25日20:00 ~ 8月24日20:00 出账 → 账单月 = 2026-09
 *
 * @param {any} billedAt 出账时间（Date/ISO字符串等）
 * @returns {string} yyyy-MM 格式账单月，空输入返回空字符串
 */
export function computeBillMonth(billedAt) {
  if (!billedAt) return ''
  const dt = toDate(billedAt)
  if (!dt) return ''
  const day = dt.getDate()
  const hour = dt.getHours()
  const year = dt.getFullYear()
  const month = dt.getMonth() + 1 // 1-based
  // 在 25日20:00 之后（含）→ 账单月 = 当前月 + 2
  // 否则 → 账单月 = 当前月 + 1
  const offset = (day > 25 || (day === 25 && hour >= 20)) ? 2 : 1
  const targetMonth = month + offset
  if (targetMonth > 12) {
    return `${year + 1}-${pad(targetMonth - 12)}`
  }
  return `${year}-${pad(targetMonth)}`
}

/**
 * 将任意格式的日期转为 MM-dd 标签（用于趋势图 X 轴等紧凑场景）
 * 兼容 "2026-07-01"、"2026-07-01T00:00:00"、Date 对象等格式
 * 先统一格式化为 yyyy-MM-dd，再取 MM-dd
 * @param {any} d
 * @returns {string}
 */
export function formatMonthDay(d) {
  if (!d) return ''
  const normalized = formatDate(d)
  const m = normalized.match(/^\d{4}-(\d{2}-\d{2})$/)
  return m ? m[1] : normalized.slice(-5)
}

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
  },
  /** 格式化日期（委托） */
  formatDate,
  /** 格式化月份-日 MM-dd（委托） */
  formatMonthDay,
  /** 格式化时间（含时分，委托） */
  formatTime,
  /** 格式化时间（含时分秒，委托） */
  formatDateTime,
  /** 计算账单月（委托） */
  computeBillMonth,
}
