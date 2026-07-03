using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 押金管理服务 — 创建/退还/扣除/余额查询
/// </summary>
public class DepositService : IDepositService
{
    private readonly IUnitOfWork _uow;

    public DepositService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<DepositLog>> GetByContractAsync(Guid contractId, CancellationToken ct)
    {
        var all = await _uow.DepositLogs.GetAllAsync(ct);
        return all.Where(d => d.ContractId == contractId)
                  .OrderByDescending(d => d.CreatedAt)
                  .ToList();
    }

    public async Task CreateAsync(Guid contractId, decimal amount, CancellationToken ct)
    {
        var log = new DepositLog(contractId, amount);
        await _uow.DepositLogs.AddAsync(log, ct);
        await _uow.CommitAsync(ct);
    }

    public async Task<DepositLog> ReturnAsync(Guid contractId, decimal amount, string? remark, CancellationToken ct)
    {
        var balance = await GetBalanceAsync(contractId, ct);
        var log = DepositLog.ForReturn(contractId, amount, balance, remark);
        await _uow.DepositLogs.AddAsync(log, ct);
        await _uow.CommitAsync(ct);
        return log;
    }

    public async Task<DepositLog> DeductAsync(Guid contractId, decimal amount, string? remark, CancellationToken ct)
    {
        var balance = await GetBalanceAsync(contractId, ct);
        var log = DepositLog.ForDeduct(contractId, amount, balance, remark);
        await _uow.DepositLogs.AddAsync(log, ct);
        await _uow.CommitAsync(ct);
        return log;
    }

    public async Task<decimal> GetBalanceAsync(Guid contractId, CancellationToken ct)
    {
        var all = await _uow.DepositLogs.GetAllAsync(ct);
        var logs = all.Where(d => d.ContractId == contractId).ToList();
        if (logs.Count == 0) return 0;
        return logs.Last().Balance;
    }
}
