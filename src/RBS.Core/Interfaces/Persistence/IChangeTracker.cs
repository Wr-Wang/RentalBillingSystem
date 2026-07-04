using RBS.Core.Entities.Base;

namespace RBS.Core.Interfaces.Persistence;

/// <summary>
/// 变更追踪器 — 记录已加载实体的快照，Commit 时自动发现并持久化变更
/// </summary>
public interface IChangeTracker
{
    /// <summary>注册实体到追踪列表，保存当前快照</summary>
    void Track<T>(T entity, string tableName) where T : AuditableEntity;

    /// <summary>获取所有有变更的实体条目（tableName → { entityId → (entity, snapshot) }）</summary>
    IReadOnlyDictionary<string, Dictionary<Guid, TrackedEntry>> DirtyEntries { get; }

    /// <summary>清空追踪列表</summary>
    void Clear();
}

public record TrackedEntry(
    object Entity,
    Dictionary<string, object?> Snapshot
);
