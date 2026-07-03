using System.Data;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Infrastructure.Data.UnitOfWork;

/// <summary>
/// Dapper 事务包装器 — 将 IDbTransaction 适配为 ITransaction 接口
/// </summary>
public class DapperTransaction : ITransaction
{
    private readonly IDbTransaction _transaction;
    private bool _disposed;

    public DapperTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        _transaction.Commit();
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        _transaction.Rollback();
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
