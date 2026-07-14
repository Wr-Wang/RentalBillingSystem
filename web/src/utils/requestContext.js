/**
 * =========================================================================
 *  请求上下文 — 请求拦截器的内存态上下文
 *
 *  为什么需要这个文件：
 *    请求拦截器 (request.js) 需要读取当前生效的 companyId 以自动注入请求参数，
 *    但不能直接引用 Pinia store（会导致循环依赖：store → request.js → store）。
 *
 *    之前方案是读 localStorage，但 localStorage 中存了 user / permissions 等
 *    敏感信息，存在被 XSS 篡改的风险。
 *
 *  本模块职责：
 *    - 在内存中维护「当前生效的 companyId」
 *    - store 层在 login / loadUserProfile / switchCompany 后主动调用 setter
 *    - 请求拦截器只读 getter，不碰 localStorage
 *
 *  安全意义：
 *    将权限/角色等敏感数据从 localStorage 移除到 Pinia 内存态，
 *    即使发生 XSS，攻击者也拿不到持久化的权限信息。
 * =========================================================================
 */

/** 当前生效的 companyId（用于请求拦截器注入到 params）*/
let effectiveCompanyId = null

/**
 * 设置当前生效的公司 ID
 * @param {string|null} id  公司 ID 或 null（全部数据）
 */
export function setEffectiveCompanyId(id) {
  effectiveCompanyId = id
}

/**
 * 获取当前生效的公司 ID
 * @returns {string|null}
 */
export function getEffectiveCompanyId() {
  return effectiveCompanyId
}

/**
 * 清除上下文（logout 时调用）
 */
export function clearEffectiveCompanyId() {
  effectiveCompanyId = null
}
