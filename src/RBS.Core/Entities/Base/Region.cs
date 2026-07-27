namespace RBS.Core.Entities.Base;

using RBS.Core.Common;

/// <summary>
/// 行政区划实体 — 省/市/区县/街道/社区
/// 数据来源：第三方 API（高德/百度）同步，作为前端级联选择器的数据源
/// 继承 AuditableEntity 以对齐项目规范，审计字段在同步时由系统自动填充
/// </summary>
public class Region : AuditableEntity
{
    /// <summary>行政区划代码，如 "440103"（GB/T 2260）</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>名称，如 "天河区"</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>父级代码，null 为根节点（省份）</summary>
    public string? ParentCode { get; private set; }

    /// <summary>层级：1=省 2=市 3=区县 4=街道 5=社区</summary>
    public int Level { get; private set; }

    /// <summary>全路径，如 "广东省/广州市/天河区"</summary>
    public string? FullPath { get; private set; }

    /// <summary>排序序号</summary>
    public int SortOrder { get; private set; }

    /// <summary>仅供 Dapper 反序列化使用</summary>
    private Region() { }

    /// <summary>创建区域实例</summary>
    public Region(string code, string name, string? parentCode, int level, string? fullPath, int sortOrder)
        : base()
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("代码不能为空", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("名称不能为空", nameof(name));
        Code = code.Trim();
        Name = name.Trim();
        ParentCode = parentCode?.Trim();
        Level = level;
        FullPath = fullPath?.Trim();
        SortOrder = sortOrder;
    }

    /// <summary>重命名</summary>
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("名称不能为空", nameof(name));
        Name = name.Trim();
    }

    /// <summary>设置排序</summary>
    public void SetSortOrder(int order) => SortOrder = order;

    /// <summary>设置全路径</summary>
    public void SetFullPath(string? fullPath) => FullPath = fullPath?.Trim();
}
