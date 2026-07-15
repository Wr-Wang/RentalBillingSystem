namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 收款分配关联实体（继承 AssociationEntity）
/// 属于 Receipt 聚合根内部的值对象/子实体，
/// 记录一笔收款（Receipt）分配到具体日记账（Journal）的明细。
/// 用于实现多对多的收款-日记账分配关系，支持部分分配、多次分配场景。
/// 核心不变约束：同一分配记录的分配金额不能超过对应 Journal 的未收余额；
/// 同一收款的所有分配金额之和不能超过收款总金额。
/// 生命周期：由 Receipt.AllocateTo 创建，由 Receipt.RemoveAllocation / Reverse 移除。
/// </summary>
public class ReceiptAllocation : AssociationEntity
{
    /// <summary>关联的收款 ID，指向 <see cref="Receipt"/> 的主键</summary>
    public Guid ReceiptId { get; private set; }

    /// <summary>关联的日记账 ID，指向 <see cref="Journal"/> 的主键</summary>
    public Guid JournalId { get; private set; }

    /// <summary>本次分配的金额，单位：元。必须大于 0 且不超过应收未收余额。</summary>
    public decimal Amount { get; private set; }

    /// <summary>私有无参构造函数，仅供 Dapper 反序列化使用，禁止业务代码调用</summary>
    private ReceiptAllocation() { }

    /// <summary>
    /// 创建收款分配记录
    /// </summary>
    /// <param name="receiptId">所属收款 ID，指向 <see cref="Receipt"/> 的主键</param>
    /// <param name="journalId">目标日记账 ID，指向 <see cref="Journal"/> 的主键</param>
    /// <param name="amount">分配金额，单位：元；必须大于 0；调用方已确保不超出未分配余额</param>
    public ReceiptAllocation(Guid receiptId, Guid journalId, decimal amount)
    {
        ReceiptId = receiptId;
        JournalId = journalId;
        Amount = amount;
    }
}
