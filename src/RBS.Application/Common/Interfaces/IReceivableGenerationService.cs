using RBS.Core.Entities.Billing;
using ContractEntity = RBS.Core.Entities.Contract.Contract;

namespace RBS.Application.Common.Interfaces;

public class ActivationInitResult
{
    public int JournalsCreated { get; set; }
    public List<string> PeriodsProcessed { get; set; } = new();
    public bool OneTimeFeeGenerated { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// 应收生成编排服务 — 按合同账期批量生成 Journal
/// </summary>
public interface IReceivableGenerationService
{
    /// <summary>
    /// 为指定合同生成指定账期范围内的应收（Journal）
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="periodFrom">起始账期 (yyyy-MM)，null 表示从合同起始月</param>
    /// <param name="periodTo">截止账期 (yyyy-MM)，null 表示到合同结束月</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>生成的 Journal 数量</returns>
    Task<int> GenerateAsync(Guid contractId, string? periodFrom, string? periodTo, CancellationToken ct = default);

    /// <summary>
    /// 合同激活时初始化生成应收（含 GL 更新）
    /// 补全从合同起租月到当前月的所有 Journal
    /// </summary>
    Task<ActivationInitResult> GenerateForActivationAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>
    /// 按付款周期拆分所有应收月份
    /// </summary>
    List<string> SplitPeriods(ContractEntity contract);

    /// <summary>
    /// 计算指定账期的到期日（付款周期末月 × 合同约定日）
    /// </summary>
    DateOnly CalculateDueDate(string periodStr, ContractEntity contract);
}
