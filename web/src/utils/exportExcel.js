/**
 * =========================================================================
 *  Excel 导出工具
 *
 *  基于 xlsx 库（SheetJS），支持多工作表导出
 *
 *  用法：
 *    import { exportToExcel } from '@/utils/exportExcel'
 *
 *    exportToExcel([
 *      {
 *        name: '租金明细',
 *        columns: ['合同号', '租客', '金额'],
 *        rows: [['CT-001', '张三', 4500], ['CT-002', '李四', 3200]]
 *      },
 *      {
 *        name: '押金明细',
 *        columns: ['合同号', '押金'],
 *        rows: [['CT-001', 9000]]
 *      }
 *    ], '月度报表')
 *    // → 下载 月度报表_2026-07-13.xlsx（两个工作表）
 *
 *  特性：
 *    - 自动列宽（基于内容 + 表头长度计算，最小 10 字符，最大 40 字符）
 *    - 多 Sheet 支持（传入多个工作表定义）
 *    - 文件名自动附加日期
 * =========================================================================
 */
import * as XLSX from 'xlsx'

/**
 * 导出一组数据到 Excel 文件
 *
 * @param {Array<Object>} sheets  工作表定义数组
 * @param {string} sheets[].name       工作表名称（显示在 Excel 底部标签）
 * @param {string[]} sheets[].columns  列标题数组
 * @param {any[][]} sheets[].rows      数据行数组（每行与 columns 索引对齐）
 * @param {string} filename  导出文件名（不含扩展名，自动附加日期）
 */
export function exportToExcel(sheets, filename) {
  // 1. 创建新的工作簿
  const wb = XLSX.utils.book_new()

  // 2. 遍历每个工作表定义
  for (const { name, columns, rows } of sheets) {
    // 2a. 将 [columns, ...rows] 转为二维数组 → SheetJS 的 aoa_to_sheet
    //     第一行是表头，后续行是数据
    const data = [columns, ...rows]
    const ws = XLSX.utils.aoa_to_sheet(data)

    // 2b. 自动列宽计算
    //     取"表头宽度 * 2"和"该列所有数据宽度 * 1.2"的最大值
    //     最终限制在 [10, 40] 字符范围内
    ws['!cols'] = columns.map((_, i) => {
      const maxLen = Math.max(
        columns[i].length * 2,                 // 表头宽度（中文字符按 2 倍算）
        ...rows.map(r => String(r[i] || '').length * 1.2)  // 数据宽度
      )
      return { wch: Math.min(Math.max(maxLen, 10), 40) }
    })

    // 2c. 将工作表添加到工作簿
    XLSX.utils.book_append_sheet(wb, ws, name)
  }

  // 3. 写入二进制 XLSX 文件（ArrayBuffer）
  const binary = XLSX.write(wb, { type: 'array', bookType: 'xlsx' })

  // 4. 创建 Blob 并触发浏览器下载
  const blob = new Blob([binary], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `${filename}_${new Date().toISOString().slice(0, 10)}.xlsx`  // 文件名_2026-07-13.xlsx
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)

  // 5. 释放内存
  URL.revokeObjectURL(url)
}
