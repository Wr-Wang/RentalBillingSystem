namespace RBS.Core.Interfaces.UnitOfWork;

/// <summary>
/// 事务抽象 — Core 层不依赖 EF Core 的具体事务类型。
/// 封装数据库事务的生命周期，提供提交和回滚操作。
/// 实现层（如 Infrastructure）使用具体数据库事务（如 DbTransaction）
/// 实现此接口，Core 层仅依赖接口定义。
/// 继承 IDisposable 和 IAsyncDisposable 以支持 using 模式自动释放。
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 提交事务。
    /// 将所有在事务范围内的数据库变更持久化到数据库。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// 回滚事务。
    /// 撤销所有在事务范围内已执行的数据库变更。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task RollbackAsync(CancellationToken ct = default);
}
