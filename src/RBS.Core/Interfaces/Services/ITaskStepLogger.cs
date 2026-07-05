namespace RBS.Core.Interfaces.Services;

using System.Data;

/// <summary>
/// 步骤级日志记录器
/// 步骤日志与业务数据在同一事务中写入
/// </summary>
public interface ITaskStepLogger
{
    /// <summary>开始一个步骤</summary>
    Task<Guid> StartStepAsync(Guid taskLogId, string stepName, string displayName,
        Guid? parentId = null, IDbTransaction? tx = null, CancellationToken ct = default);

    /// <summary>完成一个步骤（成功）</summary>
    Task CompleteStepAsync(Guid stepLogId, int affectedCount,
        IDbTransaction? tx = null, CancellationToken ct = default);

    /// <summary>步骤失败</summary>
    Task FailStepAsync(Guid stepLogId, string error,
        IDbTransaction? tx = null, CancellationToken ct = default);

    /// <summary>跳过步骤</summary>
    Task SkipStepAsync(Guid stepLogId, string reason,
        IDbTransaction? tx = null, CancellationToken ct = default);
}
