namespace RBS.Core.Entities.Billing;

using RBS.Core.Entities.Base;

/// <summary>
/// 预收账款明细（PrepaidDetail）—— 追踪预收款的来源和冲抵去向
///
/// DDD 角色：领域实体（Entity），继承 AuditableEntity，实现 IHasCompany。
/// 记录每笔预收款的来源（Direction=In）和冲抵去向（Direction=Out），
/// 支持按合同维度查询预收余额。
///
/// 状态流转：
///   Pending（未冲抵）→ Partial（部分冲抵）→ Applied（全部冲抵）
/// </summary>
public class PrepaidDetail : AuditableEntity, IHasCompany
{
    /// <summary>所属公司ID</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>合同ID（预收归属于特定合同）</summary>
    public Guid ContractId { get; private set; }

    /// <summary>来源收款单ID</summary>
    public Guid ReceiptId { get; private set; }

    /// <summary>被冲抵的日记账ID（Direction=Out时指向被冲应收）</summary>
    public Guid? JournalId { get; private set; }

    /// <summary>发生期间 yyyy-MM</summary>
    public string Period { get; private set; } = string.Empty;

    /// <summary>预收金额（正数）</summary>
    public decimal Amount { get; private set; }

    /// <summary>已冲抵金额</summary>
    public decimal AppliedAmount { get; private set; }

    /// <summary>余额 = Amount - AppliedAmount</summary>
    public decimal Balance => Amount - AppliedAmount;

    /// <summary>方向（In=收入预收，Out=冲抵应收）</summary>
    public string Direction { get; private set; } = "In";

    /// <summary>状态（Pending/Partial/Applied）</summary>
    public string Status { get; private set; } = "Pending";

    /// <summary>私有无参构造，仅供 Dapper 反序列化使用</summary>
    private PrepaidDetail() { }

    /// <summary>
    /// 创建预收记录（Direction=In）
    /// </summary>
    public PrepaidDetail(Guid companyId, Guid contractId, Guid receiptId, string period, decimal amount)
    {
        CompanyId = companyId;
        ContractId = contractId;
        ReceiptId = receiptId;
        Period = period;
        Amount = amount;
        AppliedAmount = 0;
        Direction = "In";
        Status = "Pending";
    }

    /// <summary>
    /// 创建冲抵记录（Direction=Out）
    /// </summary>
    public PrepaidDetail(Guid companyId, Guid contractId, Guid receiptId, Guid journalId, string period, decimal amount)
    {
        CompanyId = companyId;
        ContractId = contractId;
        ReceiptId = receiptId;
        JournalId = journalId;
        Period = period;
        Amount = amount;
        AppliedAmount = 0;
        Direction = "Out";
        Status = "Pending";
    }

    /// <summary>
    /// 记录一笔冲抵
    /// </summary>
    public void Apply(decimal amount)
    {
        if (Status == "Applied")
            throw new InvalidOperationException("该预收已全部冲抵，无法继续冲抵");

        AppliedAmount += amount;
        Status = Balance <= 0 ? "Applied" : "Partial";
    }
}
