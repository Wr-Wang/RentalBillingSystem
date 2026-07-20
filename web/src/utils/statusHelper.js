/**
 * 审批状态 → 中文文本
 */
export function approvalStatusText(status) {
  const map = {
    Pending: '审批中',
    Approved: '已通过',
    Rejected: '已驳回',
    Cancelled: '已撤回'
  }
  return map[status] || status
}

/**
 * 审批状态 → el-tag type
 */
export function approvalStatusTagType(status) {
  const map = {
    Pending: 'warning',
    Approved: 'success',
    Rejected: 'danger',
    Cancelled: 'info'
  }
  return map[status] || 'info'
}

/**
 * 审批记录动作 → 中文文本
 */
export function approvalActionText(action) {
  const map = {
    Submitted: '提交',
    Approved: '通过',
    Rejected: '驳回',
    Cancelled: '撤回'
  }
  return map[action] || action
}

/**
 * 审批记录动作 → el-tag type
 */
export function approvalActionTagType(action) {
  const map = {
    Submitted: 'primary',
    Approved: 'success',
    Rejected: 'danger',
    Cancelled: 'warning'
  }
  return map[action] || 'info'
}

/**
 * 审批步骤状态 → 中文文本
 */
export function stepStatusText(status) {
  const map = {
    submitted: '已提交',
    completed: '已通过',
    rejected: '已驳回',
    cancelled: '已撤回',
    current: '待审批',
    skipped: '已跳过',
    pending: '等待中'
  }
  return map[status] || status
}
