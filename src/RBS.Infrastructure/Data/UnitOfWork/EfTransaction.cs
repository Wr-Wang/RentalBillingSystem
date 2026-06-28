using Microsoft.EntityFrameworkCore.Storage;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Infrastructure.Data.UnitOfWork;

/// <summary>
/// EF Core 事务包装器
/// </summary>
internal class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken ct = default)
        => _transaction.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default)
        => _transaction.RollbackAsync(ct);

    public void Dispose() => _transaction.Dispose();

    public ValueTask DisposeAsync() => _transaction.DisposeAsync();
}
