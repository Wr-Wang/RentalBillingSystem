using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Approval;

/// <summary>
/// 审批调价明细 — 费用调价审批中每个费用项的逐条记录
/// 1:N 关联 ApprovalRequest（一个审批请求可包含多个费用项的调整）
/// 记录每个费用项的旧金额、新金额及计费模式，支持按明细生效日期独立生效
/// </summary>
public class ApprovalFeeItem : AuditableEntity
{
    /// <summary>
    /// 所属审批请求标识
    /// 外键关联到 ApprovalRequest，指明所属的调价审批请求
    /// </summary>
    public Guid ApprovalRequestId { get; private set; }

    /// <summary>
    /// 关联合同标识
    /// 指明该费用项所属的合同
    /// </summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// 费用代码标识
    /// 关联 FeeCode 字典表，确定费用项目（如租金、物业费、水电费等）
    /// </summary>
    public Guid FeeCodeId { get; private set; }

    /// <summary>
    /// 费用名称（冗余字段）
    /// 便于查询展示，避免频繁关联 FeeCode 字典表
    /// </summary>
    public string FeeName { get; private set; }

    /// <summary>
    /// 旧金额（调整前）
    /// 展示该项费用在调整前的原金额
    /// </summary>
    public decimal OldAmount { get; private set; }

    /// <summary>
    /// 新金额（调整后）
    /// 审批通过后该项费用将更新为此金额
    /// </summary>
    public decimal NewAmount { get; private set; }

    /// <summary>
    /// 计费模式
    /// FixedAmount（固定金额）— 每月按固定金额计费；
    /// MeterBased（按表计费）— 按实际用量乘以单价计费
    /// </summary>
    public string BillingMode { get; private set; }

    /// <summary>
    /// 计量单位（MeterBased 模式使用）
    /// 如 "平方米"、"吨"、"度" 等，仅当 BillingMode 为 MeterBased 时有意义
    /// </summary>
    public string? Unit { get; private set; }

    /// <summary>
    /// 生效日期（每条费用独立，格式 yyyy-MM-dd）
    /// 每条费用的调价可独立生效，无需与其他费用项同步
    /// </summary>
    public string? EffectiveDate { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ApprovalFeeItem() : base()
    {
        FeeName = string.Empty;
        BillingMode = "FixedAmount";
    }

    /// <summary>
    /// 创建审批调价明细实例
    /// </summary>
    /// <param name="approvalRequestId">所属审批请求标识</param>
    /// <param name="contractId">关联合同标识</param>
    /// <param name="feeCodeId">费用代码标识</param>
    /// <param name="feeName">费用名称</param>
    /// <param name="oldAmount">旧金额（调整前）</param>
    /// <param name="newAmount">新金额（调整后）</param>
    /// <param name="billingMode">计费模式（FixedAmount / MeterBased）</param>
    /// <param name="unit">计量单位（MeterBased 模式使用）</param>
    /// <param name="effectiveDate">生效日期（可选，格式 yyyy-MM-dd）</param>
    public ApprovalFeeItem(Guid approvalRequestId, Guid contractId, Guid feeCodeId, string feeName,
        decimal oldAmount, decimal newAmount, string billingMode, string? unit, string? effectiveDate = null) : base()
    {
        ApprovalRequestId = approvalRequestId;
        ContractId = contractId;
        FeeCodeId = feeCodeId;
        FeeName = feeName;
        OldAmount = oldAmount;
        NewAmount = newAmount;
        BillingMode = billingMode;
        Unit = unit;
        EffectiveDate = effectiveDate;
    }
}
