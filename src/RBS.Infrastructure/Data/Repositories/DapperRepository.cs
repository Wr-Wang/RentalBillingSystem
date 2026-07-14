using Dapper;
using RBS.Core.Entities.Base;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;
using RBS.Core.Interfaces.Services;

namespace RBS.Infrastructure.Data.Repositories;

/// <summary>
/// Dapper 泛型仓储基类 — 实现 IRepository&lt;T&gt; 接口，提供基于反射的 CRUD 操作。
/// </summary>
/// <remarks>
/// 实现方式：
/// <list type="bullet">
///   <item><description>通过反射自动推断表名（复数规则：y→ies, s/ch/sh/x→+es, 其余+s）</description></item>
///   <item><description>INSERT/UPDATE 自动排除导航属性和审计字段（CreatedIp, UpdatedBy 等）</description></item>
///   <item><description>集成变更追踪（IChangeTracker）和审计日志（IAuditLogWriter）</description></item>
///   <item><description>集成多租户过滤（ITenantService），支持 CompanyId 自动注入</description></item>
///   <item><description>UPDATE 采用"读旧值→计算差异→写审计"模式</description></item>
/// </list>
/// 设计模式：Repository Pattern + Active Record 风格的快照追踪。
/// 限制：不支持 LINQ 表达式分页（GetPagedAsync 抛出 NotSupportedException）。
/// </remarks>
/// <typeparam name="T">实体类型，必须继承自 AuditableEntity</typeparam>
public class DapperRepository<T> : IRepository<T> where T : AuditableEntity
{
    /// <summary>数据库连接工厂，用于创建 SqlConnection</summary>
    protected readonly IDbConnectionFactory _db;
    /// <summary>数据库表名，由构造函数推断或显式传入</summary>
    protected readonly string _tableName;
    /// <summary>审计日志写入器，记录 Create/Update/Delete 操作</summary>
    protected readonly IAuditLogWriter _auditWriter;
    /// <summary>变更追踪器（可选），用于快照追踪和延迟提交</summary>
    protected readonly IChangeTracker? _tracker;
    /// <summary>多租户服务（可选），用于自动注入 CompanyId 过滤条件</summary>
    protected readonly ITenantService? _tenant;

    /// <summary>
    /// 初始化泛型仓储
    /// </summary>
    /// <param name="db">数据库连接工厂</param>
    /// <param name="auditWriter">审计日志写入器</param>
    /// <param name="tableName">可选的显式表名，不传则通过反射推断</param>
    /// <param name="tracker">变更追踪器（可选），传入后启用快照追踪</param>
    /// <param name="tenant">多租户服务（可选），传入后启用 CompanyId 自动过滤</param>
    public DapperRepository(IDbConnectionFactory db, IAuditLogWriter auditWriter, string? tableName = null, IChangeTracker? tracker = null, ITenantService? tenant = null)
    {
        _db = db;
        _auditWriter = auditWriter;
        _tracker = tracker;
        _tableName = tableName ?? InferTableName();
        _tenant = tenant;
    }

    /// <summary>
    /// 判断当前实体是否启用 CompanyId 过滤，并输出公司 ID
    /// </summary>
    /// <remarks>
    /// 条件：TenantService 不为 null，且存在有效的 CompanyId，且实体实现了 IHasCompany 接口。
    /// </remarks>
    /// <param name="companyId">输出的公司 ID</param>
    /// <returns>是否启用公司过滤</returns>
    private bool HasCompanyFilter([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Guid companyId)
    {
        companyId = Guid.Empty;
        if (_tenant == null) return false;
        var cid = _tenant.EffectiveCompanyId;
        if (cid == null) return false;
        if (!typeof(IHasCompany).IsAssignableFrom(typeof(T))) return false;
        companyId = cid.Value;
        return true;
    }

    /// <summary>
    /// 根据主键 ID 查询实体
    /// </summary>
    /// <param name="id">实体主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>实体对象，未找到时返回 null</returns>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        T? entity;
        if (HasCompanyFilter(out var cid))
            entity = await conn.QuerySingleOrDefaultAsync<T>($"SELECT * FROM [{_tableName}] WHERE Id=@Id AND CompanyId=@CompanyId", new { Id = id, CompanyId = cid });
        else
            entity = await conn.QuerySingleOrDefaultAsync<T>($"SELECT * FROM [{_tableName}] WHERE Id=@Id", new { Id = id });
        if (entity != null) _tracker?.Track(entity, _tableName);
        return entity;
    }

    /// <summary>
    /// 查询所有实体列表
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>实体列表</returns>
    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        List<T> list;
        if (HasCompanyFilter(out var cid))
            list = (await conn.QueryAsync<T>($"SELECT * FROM [{_tableName}] WHERE CompanyId=@CompanyId", new { CompanyId = cid })).ToList();
        else
            list = (await conn.QueryAsync<T>($"SELECT * FROM [{_tableName}]")).ToList();
        foreach (var entity in list) _tracker?.Track(entity, _tableName);
        return list;
    }

    /// <summary>
    /// 新增实体 — 通过反射拼接 INSERT 语句，自动排除导航属性和审计字段
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新增后的实体（含生成的 ID）</returns>
    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var exclude = new HashSet<string> { "CreatedIp", "CreatedHostname", "UpdatedBy", "UpdatedAt", "UpdatedIp", "UpdatedHostname" };
        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && !exclude.Contains(p.Name) && !IsNavProp(p))
            .Select(p => p.Name).ToList();
        var cols = string.Join(",", props);
        var vals = string.Join(",", props.Select(p => "@" + p));
        await conn.ExecuteAsync($"INSERT INTO [{_tableName}] ({cols}) VALUES ({vals})", entity);

        // 审计：记录所有字段
        var createdBy = typeof(T).GetProperty("CreatedBy")?.GetValue(entity) as Guid? ?? Guid.Empty;
        await _auditWriter.LogChangesAsync(_tableName, GetEntityId(entity), "Create", EntityToDict(entity), createdBy, ct);
        _tracker?.Track(entity, _tableName);
        return entity;
    }

    /// <summary>
    /// 更新实体 — 先读取旧值用于审计差异计算，再通过反射拼接 UPDATE SET 语句
    /// </summary>
    /// <remarks>
    /// 更新策略：
    /// 1. 先调用 GetByIdAsync 读取修改前的旧实体快照
    /// 2. 通过反射生成 UPDATE SET 子句（排除主键、审计字段、导航属性）
    /// 3. 计算新/旧实体的字段差异，写入审计日志
    /// </remarks>
    /// <param name="entity">待更新的实体对象</param>
    /// <param name="ct">取消令牌</param>
    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        // 1. 读取旧值
        var oldEntity = await GetByIdAsync(GetEntityGuidId(entity), ct);

        // 2. 更新主表
        using var conn = _db.CreateConnection(); conn.Open();
        var exclude = new HashSet<string> { "Id", "CreatedBy", "CreatedAt", "CreatedIp", "CreatedHostname", "UpdatedBy", "UpdatedAt", "UpdatedIp", "UpdatedHostname" };
        var sets = string.Join(",",
            typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && !exclude.Contains(p.Name) && !IsNavProp(p))
                .Select(p => $"[{p.Name}]=@{p.Name}"));
        await conn.ExecuteAsync($"UPDATE [{_tableName}] SET {sets} WHERE Id=@Id", entity);

        // 3. 审计：计算差异
        if (oldEntity != null)
        {
            var changes = DiffDict(EntityToDict(oldEntity), EntityToDict(entity));
            var updatedBy = typeof(T).GetProperty("UpdatedBy")?.GetValue(entity) as Guid? ?? Guid.Empty;
            await _auditWriter.LogChangesAsync(_tableName, GetEntityId(entity), "Update", changes, updatedBy, ct);
        }
    }

    /// <summary>
    /// 删除实体 — 按主键删除并记录审计日志
    /// </summary>
    /// <param name="entity">待删除的实体对象</param>
    /// <param name="ct">取消令牌</param>
    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        var id = GetEntityGuidId(entity);
        using var conn = _db.CreateConnection(); conn.Open();
        await conn.ExecuteAsync($"DELETE FROM [{_tableName}] WHERE Id=@Id", new { Id = id });

        // 审计
        var changes = new Dictionary<string, object?> { ["Id"] = id.ToString() };
        await _auditWriter.LogChangesAsync(_tableName, id.ToString(), "Delete", changes, Guid.Empty, ct);
    }

    /// <summary>
    /// 分页查询 — 当前实现不支持，Dapper 不支持 LINQ 表达式转换
    /// </summary>
    /// <exception cref="NotSupportedException">始终抛出，表示不支持</exception>
    public Task<PagedResult<T>> GetPagedAsync(int page, int pageSize,
        System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken ct = default) => throw new NotSupportedException("Dapper 不支持 LINQ 表达式分页");

    /// <summary>
    /// 判断指定主键的实体是否存在
    /// </summary>
    /// <param name="id">实体主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>存在返回 true，否则 false</returns>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        if (HasCompanyFilter(out var cid))
            return await conn.QuerySingleAsync<int>($"SELECT COUNT(1) FROM [{_tableName}] WHERE Id=@Id AND CompanyId=@CompanyId", new { Id = id, CompanyId = cid }) > 0;
        return await conn.QuerySingleAsync<int>($"SELECT COUNT(1) FROM [{_tableName}] WHERE Id=@Id", new { Id = id }) > 0;
    }

    // ===== 辅助方法 =====

    /// <summary>获取实体的 Id 属性字符串表示</summary>
    private static string GetEntityId(T entity)
    {
        return typeof(T).GetProperty("Id")?.GetValue(entity)?.ToString() ?? Guid.Empty.ToString();
    }

    /// <summary>获取实体的 Id 属性 Guid 值</summary>
    private static Guid GetEntityGuidId(T entity)
    {
        return typeof(T).GetProperty("Id")?.GetValue(entity) is Guid g ? g : Guid.Empty;
    }

    /// <summary>
    /// 将实体转换为字典（用于审计快照比较）
    /// </summary>
    /// <remarks>排除 DomainEvents、RowVersion、导航属性和只读计算属性（如 IsVacant/IsRented）</remarks>
    protected static Dictionary<string, object?> EntityToDict(T entity)
    {
        var dict = new Dictionary<string, object?>();
        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var p in props)
        {
            if (IsNavProp(p) || p.Name is "DomainEvents" or "RowVersion") continue;
            if (!p.CanWrite) continue; // 排除计算属性（如 IsVacant/IsRented）
            dict[p.Name] = p.GetValue(entity);
        }
        return dict;
    }

    /// <summary>
    /// 计算两个字典的差异（用于审计日志字段级别变更记录）
    /// </summary>
    /// <remarks>排除 RowVersion、UpdatedAt/By/Ip/Hostname 等审计字段</remarks>
    /// <param name="old">旧值字典（修改前快照）</param>
    /// <param name="now">新值字典（修改后当前值）</param>
    /// <returns>发生了变化的字段集合</returns>
    protected static Dictionary<string, object?> DiffDict(Dictionary<string, object?> old, Dictionary<string, object?> now)
    {
        var diff = new Dictionary<string, object?>();
        var exclude = new HashSet<string> { "RowVersion", "UpdatedAt", "UpdatedBy", "UpdatedIp", "UpdatedHostname" };
        foreach (var kv in now)
        {
            if (exclude.Contains(kv.Key)) continue;
            if (!old.ContainsKey(kv.Key)) { diff[kv.Key] = kv.Value; continue; }
            var oldVal = old[kv.Key];
            var newVal = kv.Value;
            if (!Equals(oldVal, newVal))
                diff[kv.Key] = newVal;
        }
        return diff;
    }

    /// <summary>
    /// 判断属性是否为导航属性（关联实体集合）
    /// </summary>
    /// <remarks>Nullable&lt;T&gt; 视为标量值类型而非导航属性</remarks>
    /// <param name="p">属性信息</param>
    /// <returns>是导航属性返回 true</returns>
    private static bool IsNavProp(System.Reflection.PropertyInfo p)
    {
        var t = p.PropertyType;
        // 可空值类型 Nullable<T> 不是导航属性，应参与 INSERT/UPDATE
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            return false;
        return t == typeof(System.Collections.IList) || t.IsGenericType ||
               p.Name is "DomainEvents" or "RowVersion" or "Records" or "Roles";
    }

    /// <summary>
    /// 根据实体类名推断数据库表名（英文复数规则）
    /// </summary>
    /// <remarks>
    /// 规则：y 结尾 → ies；s/ch/sh/x 结尾 → +es；其余 → +s
    /// 示例：Company→Companies, Business→Businesses, Box→Boxes, Contract→Contracts
    /// </remarks>
    /// <returns>推断的表名</returns>
    private static string InferTableName()
    {
        var name = typeof(T).Name;
        if (name.EndsWith("y")) return name.Substring(0, name.Length - 1) + "ies";
        if (name.EndsWith("s") || name.EndsWith("ch") || name.EndsWith("sh") || name.EndsWith("x")) return name + "es";
        return name + "s";
    }
}
