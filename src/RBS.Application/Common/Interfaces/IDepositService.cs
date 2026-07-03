using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 押金管理服务
/// </summary>
public interface IDepositService
{
    /// <summary>查询指定合同的押金流水</summary>
    Task<List<DepositLog>> GetByContractAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>创建押金（合同创建时调用）</summary>
    Task CreateAsync(Guid contractId, decimal amount, CancellationToken ct = default);

    /// <summary>退还押金</summary>
    Task<DepositLog> ReturnAsync(Guid contractId, decimal amount, string? remark, CancellationToken ct = default);

    /// <summary>扣除押金</summary>
    Task<DepositLog> DeductAsync(Guid contractId, decimal amount, string? remark, CancellationToken ct = default);

    /// <summary>获取当前押金余额</summary>
    Task<decimal> GetBalanceAsync(Guid contractId, CancellationToken ct = default);
}
