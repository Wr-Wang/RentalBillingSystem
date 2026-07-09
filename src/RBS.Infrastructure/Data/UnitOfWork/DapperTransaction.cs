using System.Data;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Infrastructure.Data.UnitOfWork;

/// <summary>
/// Dapper 事务包装器 — 将 IDbTransaction 适配为 ITransaction 接口
/// 提交/回滚后自动清除 UoW 中的共享事务引用，防止后续误用已提交的事务
/// </summary>
public class DapperTransaction : ITransaction
{
    private readonly IDbTransaction _transaction;
    private bool _disposed;
    private readonly Action? _onComplete;

    public DapperTransaction(IDbTransaction transaction, Action? onComplete = null)
    {
        _transaction = transaction;
        _onComplete = onComplete;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        _transaction.Commit();
        _onComplete?.Invoke();
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        _transaction.Rollback();
        _onComplete?.Invoke();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction.Dispose();
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _transaction.Dispose();
            _disposed = true;
        }
        await Task.CompletedTask;
    }
}
