/**
 * 将任意字符串 ID 转为 GUID 格式，已有 GUID 则直接返回
 * @param {string} id
 * @returns {string}
 */
export function toGuidId(id) {
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id)) return id
  const hex = Array.from(String(id))
    .reduce((h, c) => { const n = c.charCodeAt(0).toString(16); return h + (n.length < 2 ? '0' + n : n) }, '')
    .padEnd(32, '0').slice(0, 32)
  return `${hex.slice(0,8)}-${hex.slice(8,12)}-${hex.slice(12,16)}-${hex.slice(16,20)}-${hex.slice(20,32)}`
}
