namespace RBS.Core.Interfaces.Persistence;

/// <summary>
/// SQL 映射加载器接口 — 从 SqlMaps.xml 获取 SQL 语句。
/// 替代 ORM 自动生成的 SQL，提供手写 SQL 的统一管理能力。
/// 遵循 {领域}.{CRUD}.{实体}.{描述} 的命名规范，
/// 由实现层（如 Infrastructure）在启动时从 XML 文件加载所有 SQL 映射。
/// </summary>
public interface ISqlLoader
{
    /// <summary>
    /// 根据唯一标识 ID 获取对应的 SQL 语句。
    /// </summary>
    /// <param name="id">SQL 语句的唯一标识，格式如 "Contract.Select.Contract.ByRoomId"</param>
    /// <returns>SQL 语句字符串</returns>
    /// <exception cref="KeyNotFoundException">指定的 ID 不存在时抛出</exception>
    string Get(string id);

    /// <summary>
    /// 检查指定 ID 的 SQL 语句是否存在。
    /// </summary>
    /// <param name="id">SQL 语句的唯一标识</param>
    /// <returns>存在时返回 true，否则 false</returns>
    bool Contains(string id);

    /// <summary>
    /// 获取已加载的 SQL 映射总数。
    /// </summary>
    int Count { get; }
}
