namespace RBS.Core.DomainServices;

using RBS.Core.Entities.Billing;

/// <summary>
/// 收款领域服务 — 收款编号生成策略、跨应收计划分配一致性校验
/// </summary>
public interface IReceiptDomainService
{
    /// <summary>生成收款单号（策略：RCP + yyyyMMdd + 4位随机数）</summary>
    string GenerateReceiptNo();

    /// <summary>
    /// 校验收款分配方案的一致性（总分配金额不超过收款金额、不能重复分配）
    /// </summary>
    void ValidateAllocation(Receipt receipt, decimal amount);
}
