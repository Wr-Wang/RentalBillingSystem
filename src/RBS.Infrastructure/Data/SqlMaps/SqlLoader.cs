using System.Xml.Linq;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Infrastructure.Data.SqlMaps;

/// <summary>
/// SQL 映射加载器 — 启动时从 SqlMaps.xml 加载全部 SQL 到内存
/// 线程安全，单例生命周期
/// </summary>
public class SqlLoader : ISqlLoader
{
    private readonly Dictionary<string, string> _sqlMap;
    private static readonly HashSet<string> ValidCrud = new(StringComparer.OrdinalIgnoreCase)
        { "Select", "Insert", "Update", "Delete" };

    public SqlLoader(string xmlFilePath)
    {
        var doc = XDocument.Load(xmlFilePath);
        _sqlMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var el in doc.Root!.Elements())
        {
            var id = el.Attribute("id")?.Value
                ?? throw new InvalidOperationException($"SqlMaps.xml 中存在缺少 id 属性的元素：{el.Name.LocalName}");

            // 校验命名格式：{模块}.{CRUD}.{实体}.{描述}
            var parts = id.Split('.');
            if (parts.Length < 4 || !ValidCrud.Contains(parts[1]))
                throw new InvalidOperationException(
                    $"SqlMaps.xml 中 id \"{id}\" 不符合命名规范：id 格式应为 "
                    + $"{{模块}}.{{CRUD}}.{{实体}}.{{描述}}，CRUD 必须为 Select/Insert/Update/Delete");

            if (!_sqlMap.TryAdd(id, el.Value.Trim()))
                throw new InvalidOperationException($"SqlMaps.xml 中存在重复的 id：\"{id}\"");
        }

        if (_sqlMap.Count == 0)
            throw new InvalidOperationException("SqlMaps.xml 中未找到任何 SQL 映射");
    }

    /// <summary>获取指定 id 的 SQL 语句</summary>
    public string Get(string id)
    {
        if (_sqlMap.TryGetValue(id, out var sql))
            return sql;
        throw new KeyNotFoundException($"SqlMaps.xml 中不存在 id 为 \"{id}\" 的 SQL 映射");
    }

    /// <summary>检查指定 id 是否存在</summary>
    public bool Contains(string id) => _sqlMap.ContainsKey(id);

    /// <summary>当前加载的 SQL 总数</summary>
    public int Count => _sqlMap.Count;
}
