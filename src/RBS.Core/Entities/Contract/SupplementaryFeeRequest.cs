namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 补充收费请求暂存实体 — 审批通过前暂存补充收费数据
/// 用于合同运行过程中追加一次性或临时收费项目
/// </summary>
public class SupplementaryFeeRequest : AuditableEntity, IHasCompany
{
    /// <summary>所属合同标识</summary>
    public Guid ContractId { get; private set; }
    /// <summary>费用项目标识，指向 FeeCode 字典表</summary>
    public Guid FeeCodeId { get; private set; }
    /// <summary>补充收费金额</summary>
    public decimal Amount { get; private set; }
    /// <summary>计费模式：FixedAmount（固定金额）/ MeterBased（抄表计量）</summary>
    public string BillingMode { get; private set; } = "FixedAmount";
    /// <summary>生效日期（yyyy-MM-dd）</summary>
    public string EffectiveDate { get; private set; } = string.Empty;
    /// <summary>收费期间起始（yyyy-MM），用于按天折算</summary>
    public string PeriodFrom { get; private set; } = string.Empty;
    /// <summary>收费期间截止（yyyy-MM），用于按天折算</summary>
    public string PeriodTo { get; private set; } = string.Empty;
    /// <summary>请求状态：Draft（草稿）/ PendingApproval（待审批）/ Completed（已完成）/ Rejected（已驳回）</summary>
    public string Status { get; private set; } = "Draft";
    /// <summary>关联的审批请求标识</summary>
    public Guid? ApprovalRequestId { get; private set; }
    /// <summary>审批通过后创建的费用配置标识（ContractFeeConfig.Id）</summary>
    public Guid? FeeConfigId { get; private set; }
    /// <summary>所属公司标识</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private SupplementaryFeeRequest() { }

    /// <summary>
    /// 创建补充收费请求
    /// </summary>
    /// <param name="contractId">所属合同标识</param>
    /// <param name="feeCodeId">费用项目标识</param>
    /// <param name="amount">补充收费金额</param>
    /// <param name="effectiveDate">生效日期（yyyy-MM-dd）</param>
    /// <param name="periodFrom">收费期间起始（yyyy-MM）</param>
    /// <param name="periodTo">收费期间截止（yyyy-MM）</param>
    /// <param name="companyId">所属公司标识</param>
    public SupplementaryFeeRequest(Guid contractId, Guid feeCodeId, decimal amount, string effectiveDate,
        string periodFrom, string periodTo, Guid companyId)
    {
        ContractId = contractId; FeeCodeId = feeCodeId; Amount = amount;
        EffectiveDate = effectiveDate; PeriodFrom = periodFrom; PeriodTo = periodTo;
        CompanyId = companyId; Status = "Draft";
    }

    /// <summary>提交审批，状态变更为 PendingApproval</summary>
    public void Submit() => Status = "PendingApproval";
    /// <summary>审批通过完成创建，记录费用配置标识并置状态为 Completed</summary>
    /// <param name="feeConfigId">审批通过后创建的 ContractFeeConfig 标识</param>
    public void Complete(Guid feeConfigId) { FeeConfigId = feeConfigId; Status = "Completed"; }
    /// <summary>驳回请求，状态变更为 Rejected</summary>
    public void Reject() => Status = "Rejected";
}
