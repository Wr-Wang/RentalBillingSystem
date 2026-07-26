using System.Reflection;

namespace RBS.Infrastructure.Data.Services;

/// <summary>
/// 审计辅助方法 — 将实体对象反射为全字段字典（用于 _Audit 表全量快照写入）
/// </summary>
internal static class AuditHelper
{
    /// <summary>
    /// 将实体转换为属性名→值的字典，排除导航属性和系统字段
    /// </summary>
    public static Dictionary<string, object?> EntityToDict(object entity)
    {
        var dict = new Dictionary<string, object?>();
        var props = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var p in props)
        {
            if (IsNavProp(p) || p.Name is "DomainEvents" or "RowVersion") continue;
            if (!p.CanWrite) continue; // 排除计算属性
            dict[p.Name] = p.GetValue(entity);
        }
        return dict;
    }

    /// <summary>
    /// 判断属性是否为导航属性（关联实体集合）
    /// </summary>
    private static bool IsNavProp(PropertyInfo p)
    {
        var t = p.PropertyType;
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            return false;
        return t == typeof(System.Collections.IList) || t.IsGenericType ||
               p.Name is "DomainEvents" or "RowVersion" or "Records" or "Roles";
    }
}
