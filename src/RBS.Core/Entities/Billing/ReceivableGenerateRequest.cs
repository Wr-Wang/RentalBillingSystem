namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 应收生成请求暂存实体 — 手动生成应收记录前暂存请求数据，待审批通过后才正式生成应收。
/// 用于处理需要人工审核的应收生成场景，如特殊费用调整、补录等。
/// 继承自 <see cref="AuditableEntity"/> 并实现 <see cref="IHasCompany"/>。
/// 生命周期状态流转：Draft(草稿) → PendingApproval(待审批) → Completed(已完成) / Rejected(已驳回)。
/// </summary>
public class ReceivableGenerateRequest : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 合同ID — 关联的租赁合同标识，标识为哪个合同生成应收。
    /// </summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// 所属公司ID — 实现 <see cref="IHasCompany"/>，标识此请求归属的租户/公司。
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 期间起始 — 应收生成的费用期间起始值，格式为 "yyyyMM"（如 "202601" 表示 2026年1月）。
    /// </summary>
    public string PeriodFrom { get; private set; } = string.Empty;

    /// <summary>
    /// 期间结束 — 应收生成的费用期间结束值，格式为 "yyyyMM"（如 "202603" 表示 2026年3月）。
    /// </summary>
    public string PeriodTo { get; private set; } = string.Empty;

    /// <summary>
    /// 状态 — 应收生成请求的当前处理状态。支持以下值：
    /// "Draft"(草稿) — 新建未提交，初始状态；
    /// "PendingApproval"(待审批) — 已提交等待审批；
    /// "Completed"(已完成) — 审批通过，已生成应收记录；
    /// "Rejected"(已驳回) — 审批驳回，未生成应收。
    /// 默认值为 "Draft"。
    /// </summary>
    public string Status { get; private set; } = "Draft";

    /// <summary>
    /// 审批请求ID — 关联的审批单标识。当请求提交后生成对应的审批记录，存储其ID。
    /// 为 null 时表示尚未提交审批或未生成审批单。
    /// </summary>
    public Guid? ApprovalRequestId { get; private set; }

    /// <summary>
    /// 私有无参构造函数 — 供 EF Core 等 ORM 框架使用，禁止外部直接调用。
    /// </summary>
    private ReceivableGenerateRequest() { }

    /// <summary>
    /// 初始化应收生成请求暂存实体。
    /// 新建时状态自动设为 "Draft"(草稿)，尚未提交审批。
    /// </summary>
    /// <param name="contractId">合同ID，关联的租赁合同标识。</param>
    /// <param name="companyId">所属公司ID。</param>
    /// <param name="periodFrom">期间起始，格式 "yyyyMM"。</param>
    /// <param name="periodTo">期间结束，格式 "yyyyMM"。</param>
    public ReceivableGenerateRequest(Guid contractId, Guid companyId, string periodFrom, string periodTo)
    {
        ContractId = contractId; CompanyId = companyId;
        PeriodFrom = periodFrom; PeriodTo = periodTo; Status = "Draft";
    }

    /// <summary>
    /// 提交审批 — 将状态从 "Draft"(草稿) 变更为 "PendingApproval"(待审批)。
    /// 表示请求已提交至审批流程，等待审批人处理。
    /// </summary>
    public void Submit() => Status = "PendingApproval";

    /// <summary>
    /// 审批通过 — 将状态从 "PendingApproval"(待审批) 变更为 "Completed"(已完成)。
    /// 表示审批通过，系统随后将据此正式生成应收记录。
    /// </summary>
    public void Complete() => Status = "Completed";

    /// <summary>
    /// 审批驳回 — 将状态从 "PendingApproval"(待审批) 变更为 "Rejected"(已驳回)。
    /// 表示审批人驳回了此应收生成请求，不会生成应收记录。
    /// </summary>
    public void Reject() => Status = "Rejected";
}
