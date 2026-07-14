namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同创建请求暂存实体 — 新建合同审批通过前暂存数据
/// 审批通过前不触碰 Contracts 主表，审批通过后由应用服务将数据同步至正式合同表
/// </summary>
public class ContractCreateRequest : AuditableEntity, IHasCompany
{
    /// <summary>合同编号，业务唯一标识</summary>
    public string ContractNo { get; private set; } = string.Empty;
    /// <summary>房源标识，指向 HousingUnit 聚合根</summary>
    public Guid RoomId { get; private set; }
    /// <summary>合同起租日期</summary>
    public DateOnly StartDate { get; private set; }
    /// <summary>合同到期日期，null 表示不限制</summary>
    public DateOnly? EndDate { get; private set; }
    /// <summary>付款周期：Monthly（月付）/ Quarterly（季付）/ Yearly（年付）/ OneTime（一次性）</summary>
    public string PaymentCycle { get; private set; } = "Monthly";
    /// <summary>所属公司标识</summary>
    public Guid CompanyId { get; private set; }
    /// <summary>请求状态：Draft（草稿）/ PendingApproval（待审批）/ Executing（执行中）/ Completed（已完成）/ Rejected（已驳回）</summary>
    public string Status { get; private set; } = "Draft";
    /// <summary>备注信息</summary>
    public string? Remark { get; private set; }
    /// <summary>关联的审批请求标识</summary>
    public Guid? ApprovalRequestId { get; private set; }
    /// <summary>审批通过后创建的新合同标识（Contract.Id），null 表示尚未完成</summary>
    public Guid? NewContractId { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private ContractCreateRequest() { }

    /// <summary>
    /// 创建合同新建请求
    /// </summary>
    /// <param name="contractNo">合同编号</param>
    /// <param name="roomId">房源标识</param>
    /// <param name="startDate">起租日期</param>
    /// <param name="endDate">到期日期，null 表示不限制</param>
    /// <param name="paymentCycle">付款周期</param>
    /// <param name="companyId">所属公司标识</param>
    public ContractCreateRequest(string contractNo, Guid roomId, DateOnly startDate, DateOnly? endDate, string paymentCycle, Guid companyId)
    {
        ContractNo = contractNo;
        RoomId = roomId;
        StartDate = startDate;
        EndDate = endDate;
        PaymentCycle = paymentCycle;
        CompanyId = companyId;
        Status = "Draft";
    }

    /// <summary>提交审批，状态变更为 PendingApproval</summary>
    public void Submit() => Status = "PendingApproval";
    /// <summary>审批通过完成创建，记录新合同标识并置状态为 Completed</summary>
    /// <param name="newContractId">审批通过后创建的正式 Contract 标识</param>
    public void Complete(Guid newContractId) { NewContractId = newContractId; Status = "Completed"; }
    /// <summary>驳回请求，状态变更为 Rejected</summary>
    public void Reject() => Status = "Rejected";
}
