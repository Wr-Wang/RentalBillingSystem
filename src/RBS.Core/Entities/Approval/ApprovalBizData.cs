using RBS.Core.Common;
using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Approval;

/// <summary>
/// 审批业务数据 — 合同操作闭环的结构化业务参数
/// 1:1 关联 ApprovalRequest，存储调租/调价/终止等操作的业务数据。
/// 当审批完成后，系统根据这些数据执行后续业务操作（如更新合同金额、终止合同等）。
/// 通过 IsProcessed 幂等标记防止回调重复执行。
/// </summary>
public class ApprovalBizData : AuditableEntity
{
    /// <summary>
    /// 关联审批请求标识
    /// 1:1 外键关联到 ApprovalRequest，创建审批时先建 ApprovalRequest 再关联
    /// </summary>
    public Guid? ApprovalRequestId { get; private set; }

    /// <summary>
    /// 关联合同标识
    /// 指明该审批业务数据所属的合同
    /// </summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// 合同编号（冗余字段，方便查询展示）
    /// </summary>
    public string? ContractNo { get; private set; }

    /// <summary>
    /// 所属公司标识
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 变更类型
    /// 可选值：FEE_ADJUST（调租/调价）/ TERMINATE（终止）/ SUSPEND（暂停计费）
    /// 根据此类型决定审批完成后执行的具体业务逻辑
    /// </summary>
    public string ChangeType { get; private set; }

    /// <summary>
    /// 生效日期（调租/调价使用）
    /// 调价后新金额开始执行的日期
    /// </summary>
    public DateTime? EffectiveDate { get; private set; }

    /// <summary>
    /// 旧金额（调整前）
    /// 用于展示变更前后对比，也可用于计算差价
    /// </summary>
    public decimal? OldAmount { get; private set; }

    /// <summary>
    /// 新金额（调整后）
    /// 审批通过后合同将更新为此金额
    /// </summary>
    public decimal? NewAmount { get; private set; }

    /// <summary>
    /// 变更原因说明
    /// 描述为什么需要进行此次变更操作
    /// </summary>
    public string? Reason { get; private set; }

    // === 终止专用字段 ===

    /// <summary>
    /// 终止类型
    /// EARLY（提前终止）/ EXPIRED（到期终止）
    /// 仅当 ChangeType 为 TERMINATE 时有意义
    /// </summary>
    public string? TerminateType { get; private set; }

    /// <summary>
    /// 实际搬离日期
    /// 客户实际搬离房屋的日期，用于计算最终费用
    /// 仅当 ChangeType 为 TERMINATE 时有意义
    /// </summary>
    public DateTime? ActualEndDate { get; private set; }

    /// <summary>
    /// 押金处理方式
    /// FULL（全额退还）/ DEDUCT（扣除后退还）/ LAST_RENT（抵作最后一期租金）
    /// 仅当 ChangeType 为 TERMINATE 时有意义
    /// </summary>
    public string? DepositReturn { get; private set; }

    // === 幂等标记 ===

    /// <summary>审批回调是否已执行</summary>
    public bool IsProcessed { get; private set; }

    /// <summary>回调执行时间</summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ApprovalBizData() : base()
    {
        ChangeType = string.Empty;
    }

    /// <summary>
    /// 创建审批业务数据实例
    /// 初始 IsProcessed 为 false，表示回调尚未执行
    /// </summary>
    /// <param name="contractId">关联合同标识</param>
    /// <param name="contractNo">合同编号（可选，用于查询展示）</param>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="changeType">变更类型（FEE_ADJUST / TERMINATE / SUSPEND）</param>
    public ApprovalBizData(Guid contractId, string? contractNo, Guid companyId, string changeType) : base()
    {
        ContractId = contractId;
        ContractNo = contractNo;
        CompanyId = companyId;
        ChangeType = changeType;
        IsProcessed = false;
    }

    // ===== 领域行为 =====

    /// <summary>
    /// 设置关联审批请求标识
    /// 在创建 ApprovalRequest 后调用，建立 1:1 关联
    /// </summary>
    /// <param name="approvalRequestId">审批请求标识</param>
    public void SetApprovalRequestId(Guid approvalRequestId) => ApprovalRequestId = approvalRequestId;

    /// <summary>
    /// 设置调租/调价业务数据
    /// 当 ChangeType 为 FEE_ADJUST 时使用
    /// </summary>
    /// <param name="effectiveDate">新金额生效日期</param>
    /// <param name="reason">调价原因说明</param>
    public void SetFeeAdjustData(DateTime? effectiveDate, string? reason)
    {
        EffectiveDate = effectiveDate;
        Reason = reason;
    }

    /// <summary>
    /// 设置终止合同业务数据
    /// 当 ChangeType 为 TERMINATE 时使用
    /// </summary>
    /// <param name="terminateType">终止类型（EARLY=提前终止 / EXPIRED=到期终止）</param>
    /// <param name="actualEndDate">实际搬离日期</param>
    /// <param name="depositReturn">押金处理方式（FULL / DEDUCT / LAST_RENT）</param>
    /// <param name="reason">终止原因说明</param>
    public void SetTerminateData(string terminateType, DateTime? actualEndDate, string? depositReturn, string? reason)
    {
        TerminateType = terminateType;
        ActualEndDate = actualEndDate;
        DepositReturn = depositReturn;
        Reason = reason;
    }

    /// <summary>标记回调已执行（幂等保护）</summary>
    public void MarkAsProcessed()
    {
        if (IsProcessed)
            throw new InvalidOperationException("业务数据已被处理，不可重复标记");
        IsProcessed = true;
        ProcessedAt = ChinaTime.Now;
    }
}
