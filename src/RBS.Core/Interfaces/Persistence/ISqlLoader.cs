namespace RBS.Core.Interfaces.Persistence;

/// <summary>
/// SQL 映射加载器接口 — 从 SqlMaps.xml 获取 SQL 语句
/// </summary>
public interface ISqlLoader
{
    string Get(string id);
    bool Contains(string id);
    int Count { get; }
}
