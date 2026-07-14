using System.Xml.Linq;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Infrastructure.Data.SqlMaps;

/// <summary>
/// SQL 映射加载器 — 启动时从 SqlMaps.xml 加载全部 SQL 到内存
/// 线程安全，单例生命周期
/// </summary>
/// <remarks>
/// 核心机制：
/// <list type="bullet">
///   <item><description>使用 XDocument 解析 SqlMaps.xml，将所有 &lt;sql&gt; 元素的 id+内容加载到 Dictionary</description></item>
///   <item><description>启动时校验 id 命名格式：{模块}.{CRUD}.{实体}.{描述}，CRUD 必须为 Select/Insert/Update/Delete</description></item>
///   <item><description>检测重复 id 和空映射文件，启动即失败（Fail-Fast）</description></item>
///   <item><description>Dictionary 使用 OrdinalIgnoreCase 比较器，id 大小写不敏感</description></item>
/// </list>
/// 设计模式：启动时加载 + 内存只读缓存。
/// </remarks>
public class SqlLoader : ISqlLoader
{
    private readonly Dictionary<string, string> _sqlMap;
    private static readonly HashSet<string> ValidCrud = new(StringComparer.OrdinalIgnoreCase)
        { "Select", "Insert", "Update", "Delete" };

    /// <summary>
    /// 加载并校验 SqlMaps.xml
    /// </summary>
    /// <param name="xmlFilePath">SqlMaps.xml 的完整文件路径</param>
    /// <exception cref="InvalidOperationException">XML 缺少 id 属性、命名格式不符、重复 id 或空映射时抛出</exception>
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
