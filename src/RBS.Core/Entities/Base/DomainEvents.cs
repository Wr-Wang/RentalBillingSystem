namespace RBS.Core.Entities.Base;
using RBS.Core.Common;

// ============================================================
// 合同领域事件（Contract Domain Events）
// 放在最前面——合同是租赁系统的核心聚合根，关联着房源、费用、应收等。
// 这些事件由 Contract 聚合根中的业务方法触发，通知其他组件做出响应。
// ============================================================

/// <summary>
/// 合同已签订（生效）事件
///
/// 触发时机：合同审批通过后首次生效时发出。
/// 预期响应：
/// - 将合同状态从 PendingApproval 更新为 Active
/// - 将对应房源状态从 Vacant 更新为 Rented
/// - 自动生成该合同第一期应收计划
/// - 通知物业/财务系统合同已开始计费
/// </summary>
public sealed record ContractActivatedEvent : IDomainEvent
{
    /// <summary>被激活的合同 ID</summary>
    public Guid ContractId { get; }

    /// <summary>合同关联的房源 ID（用于更新房源状态为已租）</summary>
    public Guid RoomId { get; }

    /// <summary>合同所属公司 ID（用于数据隔离和跨公司通知）</summary>
    public Guid CompanyId { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建合同生效事件
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="roomId">房源 ID</param>
    /// <param name="companyId">公司 ID</param>
    public ContractActivatedEvent(Guid contractId, Guid roomId, Guid companyId)
    {
        ContractId = contractId;
        RoomId = roomId;
        CompanyId = companyId;
        OccurredAt = ChinaTime.Now;
    }
}

/// <summary>
/// 合同已终止事件
///
/// 触发时机：合同在到期前被提前解除时发出。
/// 预期响应：
/// - 合同状态更新为 Terminated
/// - 房源状态恢复为 Vacant
/// - 尚未结清的应收计划标记为 Cancelled 或 Overdue
/// - 如有押金，触发押金退还流程
/// </summary>
public sealed record ContractTerminatedEvent : IDomainEvent
{
    /// <summary>被终止的合同 ID</summary>
    public Guid ContractId { get; }

    /// <summary>合同关联的房源 ID（用于恢复房源状态）</summary>
    public Guid RoomId { get; }

    /// <summary>终止原因描述（如"提前退租"、"违约解除"等）</summary>
    public string Reason { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建合同终止事件
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="roomId">房源 ID</param>
    /// <param name="reason">终止原因</param>
    public ContractTerminatedEvent(Guid contractId, Guid roomId, string reason)
    {
        ContractId = contractId;
        RoomId = roomId;
        Reason = reason;
        OccurredAt = ChinaTime.Now;
    }
}

/// <summary>
/// 合同已暂停事件
///
/// 触发时机：合同因故暂停执行时发出（如房屋维修、租户暂停经营等）。
/// 预期响应：
/// - 合同状态更新为 Suspended
/// - 暂停期间暂停计费（不计入应收计划）
/// - 记录暂停起止时间，用于后续恢复计费的计算
/// </summary>
public sealed record ContractSuspendedEvent : IDomainEvent
{
    /// <summary>被暂停的合同 ID</summary>
    public Guid ContractId { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建合同暂停事件
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    public ContractSuspendedEvent(Guid contractId)
    {
        ContractId = contractId;
        OccurredAt = ChinaTime.Now;
    }
}

// ============================================================
// 收款领域事件（Payment / Receipt Domain Events）
// 对应收款单（Receipt）的审核操作。收款单是财务模块的核心聚合根之一。
// ============================================================

/// <summary>
/// 收款已确认事件
///
/// 触发时机：财务人员确认收到款项，将收款单状态改为 Confirmed 时发出。
/// 预期响应：
/// - 更新对应应收计划（ReceivablePlan）的已收金额
/// - 如果应收计划全部结清，将其状态更新为 Paid
/// - 更新合同的最新收款日期和累计收款金额
/// - 通知租户收款已确认
/// </summary>
public sealed record PaymentConfirmedEvent : IDomainEvent
{
    /// <summary>已确认的收款单 ID</summary>
    public Guid ReceiptId { get; }

    /// <summary>收款关联的合同 ID（用于更新合同维度的收款统计）</summary>
    public Guid ContractId { get; }

    /// <summary>确认的收款金额（原始数值，非 Money 值对象以简化事件传输）</summary>
    public decimal Amount { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建收款确认事件
    /// </summary>
    /// <param name="receiptId">收款单 ID</param>
    /// <param name="contractId">合同 ID</param>
    /// <param name="amount">确认金额</param>
    public PaymentConfirmedEvent(Guid receiptId, Guid contractId, decimal amount)
    {
        ReceiptId = receiptId;
        ContractId = contractId;
        Amount = amount;
        OccurredAt = ChinaTime.Now;
    }
}

/// <summary>
/// 收款已驳回事件
///
/// 触发时机：财务审核不通过，将收款单状态改为 Rejected 时发出。
/// 预期响应：
/// - 收款单退回待修改状态
/// - 不更新任何应收数据
/// - 通知提交人审核不通过及驳回原因
/// </summary>
public sealed record PaymentRejectedEvent : IDomainEvent
{
    /// <summary>被驳回的收款单 ID</summary>
    public Guid ReceiptId { get; }

    /// <summary>驳回原因描述</summary>
    public string Reason { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建收款驳回事件
    /// </summary>
    /// <param name="receiptId">收款单 ID</param>
    /// <param name="reason">驳回原因</param>
    public PaymentRejectedEvent(Guid receiptId, string reason)
    {
        ReceiptId = receiptId;
        Reason = reason;
        OccurredAt = ChinaTime.Now;
    }
}

// ============================================================
// 应收领域事件（Receivable Domain Events）
// 应收计划（ReceivablePlan）按月生成，代表每个月应收取的费用。
// 这些事件由定时任务（逾期检测）或收款确认时触发。
// ============================================================

/// <summary>
/// 应收计划已逾期事件
///
/// 触发时机：系统定时任务检测到应收计划超过付款截止日仍未结清时发出。
/// 预期响应：
/// - 将应收计划状态从 Pending 更新为 Overdue
/// - 开始计算滞纳金（如合同约定有滞纳金条款）
/// - 触发催收通知（短信、邮件、系统消息）
/// - 更新合同的逾期统计信息
/// </summary>
public sealed record ReceivableOverdueEvent : IDomainEvent
{
    /// <summary>已逾期的应收计划 ID</summary>
    public Guid PlanId { get; }

    /// <summary>应收计划所属合同 ID</summary>
    public Guid ContractId { get; }

    /// <summary>应收计划所属账期（如 "2026-06"）</summary>
    public string Period { get; }

    /// <summary>逾期金额（原始数值）</summary>
    public decimal Amount { get; }

    /// <summary>已逾期天数（从付款截止日到当前日期）</summary>
    public int DaysOverdue { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建应收逾期事件
    /// </summary>
    /// <param name="planId">应收计划 ID</param>
    /// <param name="contractId">合同 ID</param>
    /// <param name="period">账期字符串</param>
    /// <param name="amount">逾期金额</param>
    /// <param name="daysOverdue">已逾期天数</param>
    public ReceivableOverdueEvent(Guid planId, Guid contractId, string period, decimal amount, int daysOverdue)
    {
        PlanId = planId;
        ContractId = contractId;
        Period = period;
        Amount = amount;
        DaysOverdue = daysOverdue;
        OccurredAt = ChinaTime.Now;
    }
}

/// <summary>
/// 应收计划已结清事件
///
/// 触发时机：收款确认后，应收计划的已收金额达到或超过应收总额时发出。
/// 预期响应：
/// - 将应收计划状态更新为 Paid
/// - 更新合同的整体回收率统计
/// - 如有滞纳金，停止滞纳金计算
/// - 通知租户该月费用已结清
/// </summary>
public sealed record ReceivableSettledEvent : IDomainEvent
{
    /// <summary>已结清的应收计划 ID</summary>
    public Guid PlanId { get; }

    /// <summary>应收计划所属合同 ID</summary>
    public Guid ContractId { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建应收结清事件
    /// </summary>
    /// <param name="planId">应收计划 ID</param>
    /// <param name="contractId">合同 ID</param>
    public ReceivableSettledEvent(Guid planId, Guid contractId)
    {
        PlanId = planId;
        ContractId = contractId;
        OccurredAt = ChinaTime.Now;
    }
}

// ============================================================
// 审批领域事件（Approval Domain Events）
// 通用的审批流程引擎事件，适用于合同审批、费用调整审批等多种场景。
// 审批请求（ApprovalRequest）的流转路径：
//   Submitted（已提交）→ Level 1 审核 → Level N 审核 → Completed（完成）
//   Submitted（已提交）→ 任一审核不通过 → Rejected（驳回）
//   Submitted（已提交）→ 申请人撤销 → Cancelled（撤销）
// ============================================================

/// <summary>
/// 审批已提交事件
///
/// 触发时机：申请人提交审批请求后发出。
/// 预期响应：
/// - 通知第一级审批人有新的审批待处理
/// - 记录审批日志
/// - 在"我的申请"列表中展示提交状态
/// </summary>
public sealed record ApprovalSubmittedEvent : IDomainEvent
{
    /// <summary>审批请求 ID</summary>
    public Guid ApprovalRequestId { get; }

    /// <summary>审批类型 ID（区分合同审批、费用审批等）</summary>
    public Guid ApprovalTypeId { get; }

    /// <summary>被审批的目标实体 ID（如合同 ID、费用配置 ID）</summary>
    public Guid TargetEntityId { get; }

    /// <summary>被审批的目标实体类型名称（如 "Contract"、"FeeConfig"）</summary>
    public string TargetEntityType { get; }

    /// <summary>审批标题（前端审批列表展示用）</summary>
    public string Title { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建审批提交事件
    /// </summary>
    /// <param name="approvalRequestId">审批请求 ID</param>
    /// <param name="approvalTypeId">审批类型 ID</param>
    /// <param name="targetEntityId">目标实体 ID</param>
    /// <param name="targetEntityType">目标实体类型</param>
    /// <param name="title">审批标题</param>
    public ApprovalSubmittedEvent(Guid approvalRequestId, Guid approvalTypeId,
        Guid targetEntityId, string targetEntityType, string title)
    {
        ApprovalRequestId = approvalRequestId;
        ApprovalTypeId = approvalTypeId;
        TargetEntityId = targetEntityId;
        TargetEntityType = targetEntityType;
        Title = title;
        OccurredAt = ChinaTime.Now;
    }
}

/// <summary>
/// 审批已推进到下一级事件（非终审通过）
///
/// 触发时机：当前审批人审核通过，但审批流程还未到最终级别时发出。
/// 预期响应：
/// - 通知下一级审批人有新的审批待处理
/// - 更新审批请求的当前审批级别
/// - 记录本次审批的操作日志
/// </summary>
public sealed record ApprovalLevelAdvancedEvent : IDomainEvent
{
    /// <summary>审批请求 ID</summary>
    public Guid ApprovalRequestId { get; }

    /// <summary>下一级审批级别序号（从 1 开始递增）</summary>
    public int NextLevel { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建审批高级推进事件
    /// </summary>
    /// <param name="approvalRequestId">审批请求 ID</param>
    /// <param name="nextLevel">下一级级别序号</param>
    public ApprovalLevelAdvancedEvent(Guid approvalRequestId, int nextLevel)
    {
        ApprovalRequestId = approvalRequestId;
        NextLevel = nextLevel;
        OccurredAt = ChinaTime.Now;
    }
}

/// <summary>
/// 审批已通过（全部级别完成）事件
///
/// 触发时机：审批流程的所有级别都已审核通过时发出。
/// 预期响应：
/// - 执行审批通过后的业务动作（如合同生效、费用变更生效）
/// - 通知申请人审批已通过
/// - 记录审批完成日志
/// </summary>
public sealed record ApprovalCompletedEvent : IDomainEvent
{
    /// <summary>审批请求 ID</summary>
    public Guid ApprovalRequestId { get; }

    /// <summary>被审批的目标实体 ID（用于执行审批通过后的业务动作）</summary>
    public Guid TargetEntityId { get; }

    /// <summary>被审批的目标实体类型名称</summary>
    public string TargetEntityType { get; }

    /// <summary>
    /// 审批通过后要执行的动作名称
    /// 如 "ActivateContract"（激活合同）、"ApplyFeeChange"（应用费用变更）
    /// 用于确定审批完成后应触发的具体业务逻辑。
    /// </summary>
    public string Action { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建审批完成事件
    /// </summary>
    /// <param name="approvalRequestId">审批请求 ID</param>
    /// <param name="targetEntityId">目标实体 ID</param>
    /// <param name="targetEntityType">目标实体类型</param>
    /// <param name="action">执行动作名称</param>
    public ApprovalCompletedEvent(Guid approvalRequestId, Guid targetEntityId, string targetEntityType, string action)
    {
        ApprovalRequestId = approvalRequestId;
        TargetEntityId = targetEntityId;
        TargetEntityType = targetEntityType;
        Action = action;
        OccurredAt = ChinaTime.Now;
    }
}

// ============================================================
// 抄表领域事件（Meter Reading Domain Events）
// 用于抄表计量（MeterBased）计费模式的合同。
// 抄表读取了水表/电表读数后，需要确认后才能用于计费。
// ============================================================

/// <summary>
/// 抄表已确认事件
///
/// 触发时机：物业/财务人员确认抄表读数有效后发出。
/// 预期响应：
/// - 将抄表记录状态更新为 Confirmed
/// - 获取对应费用配置的单价，计算当月应计金额
/// - 生成或更新该月应收计划中的计量部分
/// - 通知租户本次抄表读数
/// </summary>
public sealed record MeterReadingConfirmedEvent : IDomainEvent
{
    /// <summary>已确认的抄表记录 ID</summary>
    public Guid ReadingId { get; }

    /// <summary>关联的费用配置 ID（用于获取单价和计费规则）</summary>
    public Guid ContractFeeConfigId { get; }

    /// <summary>抄表所属年份</summary>
    public int Year { get; }

    /// <summary>抄表所属月份</summary>
    public int Month { get; }

    /// <summary>本期用量（水表吨数/电表度数等，保留两位小数）</summary>
    public decimal Usage { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建抄表确认事件
    /// </summary>
    /// <param name="readingId">抄表记录 ID</param>
    /// <param name="contractFeeConfigId">费用配置 ID</param>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <param name="usage">用量</param>
    public MeterReadingConfirmedEvent(Guid readingId, Guid contractFeeConfigId, int year, int month, decimal usage)
    {
        ReadingId = readingId;
        ContractFeeConfigId = contractFeeConfigId;
        Year = year;
        Month = month;
        Usage = usage;
        OccurredAt = ChinaTime.Now;
    }
}

// ============================================================
// 合同恢复事件
// ============================================================

/// <summary>
/// 合同已恢复事件
///
/// 触发时机：被暂停的合同重新恢复执行时发出。
/// 预期响应：
/// - 合同状态从 Suspended 恢复为 Active
/// - 恢复计费，生成暂停期间的应收计划（如需补计）
/// - 更新合同的暂停/恢复日志
/// </summary>
public sealed record ContractResumedEvent : IDomainEvent
{
    /// <summary>被恢复的合同 ID</summary>
    public Guid ContractId { get; }

    /// <summary>恢复执行的日期（可能不等于事件触发时间，如指定未来某日恢复）</summary>
    public DateTime ResumedAt { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建合同恢复事件
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="resumedAt">恢复日期</param>
    public ContractResumedEvent(Guid contractId, DateTime resumedAt)
    {
        ContractId = contractId;
        ResumedAt = resumedAt;
        OccurredAt = ChinaTime.Now;
    }
}

// ============================================================
// 公司创建事件
// ============================================================

/// <summary>
/// 公司（租户）已创建事件
///
/// 触发时机：系统中新增一个公司（多租户场景下新增租户）后发出。
/// 预期响应：
/// - 为新公司初始化默认的系统配置（如审批类型、费用类型、角色权限）
/// - 记录公司创建的操作日志
/// - 通知超级管理员新公司已就绪
/// </summary>
public sealed record CompanyCreatedEvent : IDomainEvent
{
    /// <summary>新创建的公司 ID</summary>
    public Guid CompanyId { get; }

    /// <summary>事件发生时间（中国标准时间）</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// 创建公司创建事件
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    public CompanyCreatedEvent(Guid companyId)
    {
        CompanyId = companyId;
        OccurredAt = ChinaTime.Now;
    }
}
