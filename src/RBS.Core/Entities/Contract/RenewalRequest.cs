namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 续签请求实体 — 存储待审批的续签数据
/// 审批通过前不触碰 Contracts 主表
/// 审批通过后创建新合同并将原合同标记为已续签
/// </summary>
public class RenewalRequest : AuditableEntity, IHasCompany
{
    /// <summary>原合同标识，指向被续签的上一份合同</summary>
    public Guid OldContractId { get; private set; }
    /// <summary>续签后创建的新合同标识，审批通过后赋值</summary>
    public Guid? NewContractId { get; private set; }
    /// <summary>新合同编号</summary>
    public string ContractNo { get; private set; } = string.Empty;
    /// <summary>续签类型：Standard（标准续签）/ RentAdjustment（仅调价）等</summary>
    public string RenewalType { get; private set; } = "Standard";
    /// <summary>原合同租金（用于对比参考）</summary>
    public decimal PreviousRent { get; private set; }
    /// <summary>续签后新租金</summary>
    public decimal NewRent { get; private set; }
    /// <summary>续签后新的到期日期</summary>
    public DateTime NewEndDate { get; private set; }
    /// <summary>押金处理方式：TRANSFER（转移）/ RETURN（退还）/ NEW_DEPOSIT（新押金）等</summary>
    public string DepositHandling { get; private set; } = "TRANSFER";
    /// <summary>原合同押金金额</summary>
    public decimal OldDepositAmount { get; private set; }
    /// <summary>新合同押金金额，null 表示沿用旧押金</summary>
    public decimal? NewDepositAmount { get; private set; }
    /// <summary>续签时的市场参考价格，用于定价审计</summary>
    public decimal? MarketReferencePrice { get; private set; }
    /// <summary>付款状态检查是否通过，续签前需确认原合同无欠款</summary>
    public bool PaymentStatusCheck { get; private set; }
    /// <summary>请求状态：Draft（草稿）/ PendingApproval（待审批）/ Approved（已批准）/ Completed（已完成）/ Rejected（已驳回）</summary>
    public string Status { get; private set; } = "Draft";
    /// <summary>备注信息</summary>
    public string? Remark { get; private set; }
    /// <summary>所属公司标识</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private RenewalRequest() { }

    /// <summary>
    /// 创建续签请求
    /// </summary>
    /// <param name="oldContractId">原合同标识</param>
    /// <param name="contractNo">新合同编号</param>
    /// <param name="previousRent">原合同租金</param>
    /// <param name="newRent">续签后新租金</param>
    /// <param name="newEndDate">续签后新到期日期</param>
    /// <param name="companyId">所属公司标识</param>
    public RenewalRequest(Guid oldContractId, string contractNo, decimal previousRent, decimal newRent, DateTime newEndDate, Guid companyId)
    {
        OldContractId = oldContractId;
        ContractNo = contractNo;
        PreviousRent = previousRent;
        NewRent = newRent;
        NewEndDate = newEndDate;
        CompanyId = companyId;
        Status = "Draft";
    }

    /// <summary>
    /// 设置押金处理信息
    /// </summary>
    /// <param name="handling">押金处理方式：TRANSFER / RETURN / NEW_DEPOSIT</param>
    /// <param name="oldAmount">原合同押金金额</param>
    /// <param name="newAmount">新合同押金金额，null 表示沿用旧押金</param>
    public void SetDepositInfo(string handling, decimal oldAmount, decimal? newAmount)
    {
        DepositHandling = handling;
        OldDepositAmount = oldAmount;
        NewDepositAmount = newAmount;
    }

    /// <summary>设置续签时市场参考价格</summary>
    /// <param name="price">市场参考价格，null 表示未参考</param>
    public void SetMarketPrice(decimal? price) => MarketReferencePrice = price;
    /// <summary>设置付款状态检查结果，续签前需确认原合同无欠款</summary>
    /// <param name="passed">true 表示检查通过（无欠款）</param>
    public void SetPaymentStatusCheck(bool passed) => PaymentStatusCheck = passed;
    /// <summary>设置备注信息</summary>
    public void SetRemark(string? remark) => Remark = remark;

    /// <summary>
    /// 提交审批，状态变更为 PendingApproval
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不是草稿时抛出</exception>
    public void SubmitForApproval()
    {
        if (Status != "Draft")
            throw new InvalidOperationException($"状态为 {Status} 的续签请求不能提交");
        Status = "PendingApproval";
    }

    /// <summary>
    /// 审批通过，记录新合同标识并置状态为 Completed
    /// </summary>
    /// <param name="newContractId">审批通过后创建的新合同标识</param>
    /// <exception cref="InvalidOperationException">当状态不是待审批或已批准时抛出</exception>
    public void Complete(Guid newContractId)
    {
        if (Status != "PendingApproval" && Status != "Approved")
            throw new InvalidOperationException($"状态为 {Status} 的续签请求不能完成");
        NewContractId = newContractId;
        Status = "Completed";
    }

    /// <summary>
    /// 驳回续签请求
    /// </summary>
    /// <exception cref="InvalidOperationException">当状态不是待审批时抛出</exception>
    public void Reject()
    {
        if (Status != "PendingApproval")
            throw new InvalidOperationException($"状态为 {Status} 的续签请求不能驳回");
        Status = "Rejected";
    }
}
