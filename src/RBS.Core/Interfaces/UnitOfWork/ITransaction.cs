namespace RBS.Core.Interfaces.UnitOfWork;

/// <summary>
/// 事务抽象 — Core 层不依赖 EF Core 的具体事务类型
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    /// <summary>提交事务</summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>回滚事务</summary>
    Task RollbackAsync(CancellationToken ct = default);
}
