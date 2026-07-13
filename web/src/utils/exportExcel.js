import * as XLSX from 'xlsx'

/**
 * 导出一组数据到 Excel 文件
 * @param {Array<{name:string, columns:string[], rows:any[][]}>} sheets - 工作表定义
 * @param {string} filename - 导出文件名（不含扩展名）
 */
export function exportToExcel(sheets, filename) {
  const wb = XLSX.utils.book_new()
  for (const { name, columns, rows } of sheets) {
    const data = [columns, ...rows]
    const ws = XLSX.utils.aoa_to_sheet(data)
    // 自动列宽
    ws['!cols'] = columns.map((_, i) => {
      const maxLen = Math.max(
        columns[i].length * 2,
        ...rows.map(r => String(r[i] || '').length * 1.2)
      )
      return { wch: Math.min(Math.max(maxLen, 10), 40) }
    })
    XLSX.utils.book_append_sheet(wb, ws, name)
  }
  const binary = XLSX.write(wb, { type: 'array', bookType: 'xlsx' })
  const blob = new Blob([binary], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `${filename}_${new Date().toISOString().slice(0, 10)}.xlsx`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
