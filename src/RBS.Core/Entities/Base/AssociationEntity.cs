namespace RBS.Core.Entities.Base;

/// <summary>
/// 关联表实体基类（Association Entity Base）
///
/// DDD 角色：关联实体（Association Entity）用于多对多（Many-to-Many）关联表的实体化表示。
/// 与 <see cref="AuditableEntity"/> 不同，关联实体仅记录创建信息，不追踪后续修改，
/// 因为关联关系一旦建立通常不会变更（如需变更应删除旧关联并创建新关联）。
///
/// 使用场景：
/// - 合同与费用的关联表（ContractFeeConfig）
/// - 用户与角色的关联表（UserRole）
/// - 任何仅记录"何时由谁建立关联"的多对多中间表
///
/// 设计要点：
/// - 不包含 Updated* 审计字段，简化存储
/// - 不自动填充 CreatedAt（由 SetCreated 显式调用），给予领域层控制权
/// </summary>
public abstract class AssociationEntity
{
    /// <summary>
    /// 关联记录的全局唯一标识（主键）
    /// 使用 GUID 作为主键类型，在构造时自动生成。
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// 关联记录的创建人用户 ID
    /// 记录是谁建立了本条关联关系。
    /// </summary>
    public Guid CreatedBy { get; protected set; }

    /// <summary>
    /// 关联记录的创建时间（中国标准时间 UTC+8）
    /// 记录关联关系建立的时刻。
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// 保护构造方法
    /// 自动生成 GUID 主键。CreatedAt 不在构造时填充，由 SetCreated 方法显式赋值。
    /// </summary>
    protected AssociationEntity()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// 设置关联记录的创建审计信息
    /// 由领域工厂方法在建立关联关系时调用。
    /// 与 AuditableEntity 不同，本方法仅需 userId 和 utcNow 两个参数，无需 IP 和主机名信息。
    /// </summary>
    /// <param name="userId">创建人用户 ID</param>
    /// <param name="utcNow">创建时间（中国标准时间）</param>
    public void SetCreated(Guid userId, DateTime utcNow)
    {
        CreatedBy = userId;
        CreatedAt = utcNow;
    }
}
