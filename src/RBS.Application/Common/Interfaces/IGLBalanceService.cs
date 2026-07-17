using RBS.Application.DTOs.Accounting;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 总账余额查询应用服务 — 按期间查询科目级期初/本期/YTD/期末余额
/// </summary>
public interface IGLBalanceService
{
    /// <summary>
    /// 查询总账余额表
    /// </summary>
    Task<GLBalanceResultDto> GetBalancesAsync(
        Guid companyId, string period, string? subjectCode, int? subjectLevel,
        string? contractNo, string? sourceType, bool hideZero, CancellationToken ct);

    /// <summary>
    /// 查询科目明细（按合同号分组）
    /// </summary>
    Task<GLDetailResultDto> GetDetailAsync(
        Guid companyId, string period, string subjectCode, string? contractNo, CancellationToken ct);
}
