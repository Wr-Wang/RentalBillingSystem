namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Billing;

/// <summary>
/// 收款领域服务实现 — 封装编号策略和分配校验规则
/// </summary>
public class ReceiptDomainService : IReceiptDomainService
{
    /// <summary>
    /// 生成收款单号：RCP + 当前日期(yyyyMMdd) + 4位随机数
    /// </summary>
    public string GenerateReceiptNo()
    {
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var randomPart = Random.Shared.Next(1000, 9999);
        return $"RCP{datePart}{randomPart}";
    }

    /// <summary>
    /// 校验收款分配金额是否有效（不超过未分配余额）
    /// </summary>
    public void ValidateAllocation(Receipt receipt, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("分配金额必须大于0", nameof(amount));
        if (receipt.Status != "Confirmed")
            throw new InvalidOperationException("只有已确认的收款才能分配");
        if (receipt.UnallocatedAmount < amount)
            throw new InvalidOperationException($"可分配余额不足（剩余 {receipt.UnallocatedAmount}）");
    }
}
