namespace RBS.Core.Entities.Base;

/// <summary>
/// 可审计实体基类（Auditable Entity Base）
///
/// DDD 角色：所有聚合根（Aggregate Root）及需要审计追踪的实体继承此类。
/// 提供统一的创建/修改审计字段（谁、什么时间、什么 IP、什么主机名），
/// 满足合规审计需求，记录每一条数据在系统中的完整生命周期操作痕迹。
///
/// 设计要点：
/// - Id 在构造时自动生成 GUID，确保分布式环境下唯一
/// - CreatedAt 默认使用中国标准时间（UTC+8）
/// - SetCreated / SetUpdated 由领域工厂或应用服务在适当的时机显式调用
/// - Updated* 字段可为 null，表示实体未被修改过
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// 实体的全局唯一标识（主键）
    /// 使用 GUID 作为主键类型，适合分布式系统和多数据库场景。
    /// 在构造时自动通过 Guid.NewGuid() 生成，子类可在构造中覆盖。
    /// </summary>
    public Guid Id { get; protected set; }

    // ===== 创建审计字段 =====
    // 首次创建时由 SetCreated 方法填充，记录创建者的身份和环境信息

    /// <summary>
    /// 创建人用户 ID
    /// 关联到用户表的唯一标识，记录是谁创建了本条数据。
    /// </summary>
    public Guid CreatedBy { get; protected set; }

    /// <summary>
    /// 创建时间（中国标准时间 UTC+8）
    /// 记录数据创建的时刻，默认在构造时赋值为 ChinaTime.Now。
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// 创建时的客户端 IP 地址
    /// 用于审计追踪，记录创建操作发起的网络来源。
    /// 可能为 null（如系统内部初始化创建时）。
    /// </summary>
    public string? CreatedIp { get; protected set; }

    /// <summary>
    /// 创建时的客户端主机名
    /// 用于审计追踪，记录创建操作发起的机器名称。
    /// 可能为 null。
    /// </summary>
    public string? CreatedHostname { get; protected set; }

    // ===== 修改审计字段 =====
    // 每次更新时由 SetUpdated 方法覆写，仅在发生修改时填充

    /// <summary>
    /// 最后修改人用户 ID
    /// 关联到用户表的唯一标识，记录最后一次修改本条数据的操作者。
    /// 未修改过时为 null。
    /// </summary>
    public Guid? UpdatedBy { get; protected set; }

    /// <summary>
    /// 最后修改时间（中国标准时间 UTC+8）
    /// 记录数据被最后修改的时刻。
    /// 未修改过时为 null。
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// 最后修改时的客户端 IP 地址
    /// 用于审计追踪，记录修改操作发起的网络来源。
    /// 未修改过时为 null。
    /// </summary>
    public string? UpdatedIp { get; protected set; }

    /// <summary>
    /// 最后修改时的客户端主机名
    /// 用于审计追踪，记录修改操作发起的机器名称。
    /// 未修改过时为 null。
    /// </summary>
    public string? UpdatedHostname { get; protected set; }

    /// <summary>
    /// 保护构造方法
    /// 自动生成 GUID 主键，并记录创建时间为当前中国标准时间。
    /// 子类构造时会继承此默认行为，也可在构造体内重新赋值 Id 以支持持久化还原。
    /// </summary>
    protected AuditableEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = RBS.Core.Common.ChinaTime.Now;
    }

    /// <summary>
    /// 设置创建审计信息
    /// 由领域工厂方法（Domain Factory）或应用服务在首次持久化前调用。
    /// 基础设施层（如 DapperRepository.InsertAsync）不应再自动覆写此信息，
    /// 以确保领域层对审计信息的控制权。
    /// </summary>
    /// <param name="userId">创建人用户 ID</param>
    /// <param name="utcNow">创建时间（中国标准时间）</param>
    /// <param name="ip">创建时的客户端 IP 地址，可为 null</param>
    /// <param name="hostname">创建时的客户端主机名，可为 null</param>
    /// <remarks>建议通过实体构造函数或领域工厂方法自动设置审计信息，减少显式调用</remarks>
    public void SetCreated(Guid userId, DateTime utcNow, string? ip, string? hostname)
    {
        CreatedBy = userId;
        CreatedAt = utcNow;
        CreatedIp = ip;
        CreatedHostname = hostname;
    }

    /// <summary>
    /// 设置更新审计信息
    /// 由基础设施层（DapperRepository.UpdateAsync）或上层应用服务在实体更新时调用。
    /// 每次更新都会覆写 Updated* 字段的值，不会保留历史修改记录（历史记录由变更日志表承载）。
    /// </summary>
    /// <param name="userId">修改人用户 ID</param>
    /// <param name="utcNow">修改时间（中国标准时间）</param>
    /// <param name="ip">修改时的客户端 IP 地址，可为 null</param>
    /// <param name="hostname">修改时的客户端主机名，可为 null</param>
    /// <remarks>建议使用领域服务或 UoW 变更追踪自动处理审计更新</remarks>
    public void SetUpdated(Guid userId, DateTime utcNow, string? ip, string? hostname)
    {
        UpdatedBy = userId;
        UpdatedAt = utcNow;
        UpdatedIp = ip;
        UpdatedHostname = hostname;
    }
}
