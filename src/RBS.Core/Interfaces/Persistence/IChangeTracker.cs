using RBS.Core.Entities.Base;

namespace RBS.Core.Interfaces.Persistence;

/// <summary>
/// 变更追踪器 — 记录已加载实体的快照，Commit 时自动发现并持久化变更。
/// 实现类似 EF Core ChangeTracker 的功能，通过快照对比机制检测实体变更。
/// 在实体从数据库加载时拍摄快照，提交时比对当前值与快照，
/// 自动生成 UPDATE 语句仅包含发生变更的字段。
/// 用于 Dapper + 原始 SQL 架构下的变更追踪场景。
/// </summary>
public interface IChangeTracker
{
    /// <summary>
    /// 注册实体到追踪列表，保存当前快照。
    /// 在每次查询加载实体后调用，记录实体各字段的初始值。
    /// </summary>
    /// <typeparam name="T">实体类型，必须继承自 AuditableEntity</typeparam>
    /// <param name="entity">待追踪的实体实例</param>
    /// <param name="tableName">实体对应的数据库表名</param>
    void Track<T>(T entity, string tableName) where T : AuditableEntity;

    /// <summary>
    /// 获取所有有变更的实体条目。
    /// 返回按表名分组的字典：tableName → { entityId → (entity, snapshot) }。
    /// 仅在实体当前属性值与快照不一致时才被标记为 Dirty。
    /// </summary>
    IReadOnlyDictionary<string, Dictionary<Guid, TrackedEntry>> DirtyEntries { get; }

    /// <summary>
    /// 清空追踪列表。
    /// 通常在 Commit 成功后调用，重置追踪状态。
    /// </summary>
    void Clear();
}

/// <summary>
/// 追踪条目记录。
/// 保存实体的当前引用和从数据库加载时的原始字段快照，
/// 供变更检测时进行字段级别的比对。
/// </summary>
/// <param name="Entity">实体对象的当前引用</param>
/// <param name="Snapshot">从数据库加载时的原始字段快照（字段名 → 值）</param>
public record TrackedEntry(
    object Entity,
    Dictionary<string, object?> Snapshot
);
