namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 补充收费请求暂存明细 — 各月分摊数据
/// 将补充收费金额按覆盖天数在各月间分摊，记录每月的分摊明细和后续生成的应收/凭证标识
/// </summary>
public class SupplementaryFeeRequestItem : AuditableEntity
{
    /// <summary>所属补充收费请求标识</summary>
    public Guid RequestId { get; private set; }
    /// <summary>分摊所属账期（yyyy-MM），如 "2026-07"</summary>
    public string Period { get; private set; } = string.Empty;
    /// <summary>当前账期分摊金额（按天数比例折算）</summary>
    public decimal ProratedAmount { get; private set; }
    /// <summary>当月总天数</summary>
    public int DaysInMonth { get; private set; }
    /// <summary>当期覆盖天数（收费期间落在此月的实际天数）</summary>
    public int CoveredDays { get; private set; }
    /// <summary>分摊后生成的应收计划标识，null 表示尚未生成</summary>
    public Guid? ReceivablePlanId { get; private set; }
    /// <summary>分摊后生成的凭证标识，null 表示尚未生成</summary>
    public Guid? VoucherId { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private SupplementaryFeeRequestItem() { }

    /// <summary>
    /// 创建补充收费分摊明细条目
    /// </summary>
    /// <param name="requestId">所属补充收费请求标识</param>
    /// <param name="period">分摊所属账期（yyyy-MM）</param>
    /// <param name="proratedAmount">当前账期分摊金额</param>
    /// <param name="daysInMonth">当月总天数</param>
    /// <param name="coveredDays">当期覆盖天数</param>
    public SupplementaryFeeRequestItem(Guid requestId, string period, decimal proratedAmount,
        int daysInMonth, int coveredDays)
    {
        RequestId = requestId; Period = period; ProratedAmount = proratedAmount;
        DaysInMonth = daysInMonth; CoveredDays = coveredDays;
    }

    /// <summary>
    /// 设置生成的应收计划和凭证标识（审批通过生成单据后调用）
    /// </summary>
    /// <param name="receivablePlanId">应收计划标识</param>
    /// <param name="voucherId">凭证标识</param>
    public void SetPlanIds(Guid receivablePlanId, Guid voucherId)
    {
        ReceivablePlanId = receivablePlanId; VoucherId = voucherId;
    }
}
