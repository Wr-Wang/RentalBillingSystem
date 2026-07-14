namespace RBS.Core.Interfaces.Repositories;

using RBS.Core.Entities.Base;

/// <summary>
/// 泛型仓储接口 — 定义领域实体持久化的基本契约。
/// 所有领域实体仓储均继承此接口，提供数据访问的抽象层。
/// 泛型约束 T 必须为 AuditableEntity，确保所有实体具备审计属性。
/// </summary>
/// <remarks>
/// 设计原则：
/// - 仓储仅提供聚合根的基础查询（按 ID/全部/存在性检查）和添加操作
/// - 分页查询应由特化仓储方法提供（如 IApprovalRequestRepository.GetHistoryAsync），
///   使用 SQL 原生参数而非 ORM 表达式，避免基础设施泄漏到领域层
/// - 实体修改由 Unit of Work 自动追踪（CommitAsync 时检测脏数据），无需显式 Update
/// - 物理删除应避免，通过实体状态变更（IsArchived 标记）实现软删除
/// </remarks>
/// <typeparam name="T">实体类型，必须继承自 AuditableEntity</typeparam>
public interface IRepository<T> where T : AuditableEntity
{
    /// <summary>
    /// 根据主键 ID 获取实体。
    /// </summary>
    /// <param name="id">实体主键 GUID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>实体对象，未找到时返回 null</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 获取所有实体列表。
    /// 注意：大量数据时应使用特化仓储的分页方法，避免全表加载。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>全部实体列表</returns>
    Task<List<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// 添加新实体。
    /// </summary>
    /// <param name="entity">待添加的实体</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>添加后的实体（包含自动生成的 ID 等属性）</returns>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// 更新现有实体。
    /// 建议优先使用 Unit of Work 变更追踪方式：加载实体后修改属性，
    /// 由 CommitAsync 自动检测并持久化变更。
    /// </summary>
    /// <param name="entity">待更新的实体，需包含主键 ID</param>
    /// <param name="ct">取消令牌</param>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// 删除实体。
    /// 建议优先使用软删除模式：设置实体 IsActive/IsArchived 等状态标记字段，
    /// 通过 Update 或 UoW 持久化。
    /// </summary>
    /// <param name="entity">待删除的实体</param>
    /// <param name="ct">取消令牌</param>
    Task DeleteAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// 检查指定 ID 的实体是否存在。
    /// </summary>
    /// <param name="id">实体主键 GUID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>存在时返回 true，否则 false</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// 分页结果。
/// 封装分页查询的返回数据，包含当前页数据集合、总记录数、
/// 当前页码、每页大小和总页数。
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public class PagedResult<T>
{
    /// <summary>当前页的数据集合</summary>
    public List<T> Items { get; set; } = new();
    /// <summary>总记录数</summary>
    public int Total { get; set; }
    /// <summary>当前页码（从 1 开始）</summary>
    public int Page { get; set; }
    /// <summary>每页记录数</summary>
    public int PageSize { get; set; }
    /// <summary>总页数</summary>
    public int TotalPages { get; set; }
}
