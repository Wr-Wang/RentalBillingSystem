namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 收款分配关联实体（继承 AssociationEntity）
/// 属于 Receipt 聚合根内部的值对象/子实体，
/// 记录一笔收款（Receipt）分配到具体应收计划（ReceivablePlan）的明细。
/// 用于实现多对多的收款-应收分配关系，支持部分分配、多次分配场景。
/// 核心不变约束：同一分配记录的分配金额不能超过对应应收的未收余额；
/// 同一收款的所有分配金额之和不能超过收款总金额。
/// 生命周期：由 Receipt.AllocateTo 创建，由 Receipt.RemoveAllocation / Reverse 移除。
/// </summary>
public class ReceiptAllocation : AssociationEntity
{
    /// <summary>关联的收款 ID，指向 <see cref="Receipt"/> 的主键</summary>
    public Guid ReceiptId { get; private set; }

    /// <summary>关联的应收计划 ID，指向 <see cref="ReceivablePlan"/> 的主键</summary>
    public Guid ReceivablePlanId { get; private set; }

    /// <summary>本次分配的金额，单位：元。必须大于 0 且不超过应收未收余额。</summary>
    public decimal Amount { get; private set; }

    /// <summary>私有无参构造函数，仅供 EF Core 延迟加载（代理）使用，禁止业务代码调用</summary>
    private ReceiptAllocation() { }

    /// <summary>
    /// 创建收款分配记录
    /// </summary>
    /// <param name="receiptId">所属收款 ID，指向 <see cref="Receipt"/> 的主键</param>
    /// <param name="receivablePlanId">目标应收计划 ID，指向 <see cref="ReceivablePlan"/> 的主键</param>
    /// <param name="amount">分配金额，单位：元；必须大于 0；调用方（<see cref="Receipt.AllocateTo"/>）已确保不超出未分配余额</param>
    public ReceiptAllocation(Guid receiptId, Guid receivablePlanId, decimal amount)
    {
        ReceiptId = receiptId;
        ReceivablePlanId = receivablePlanId;
        Amount = amount;
    }
}
