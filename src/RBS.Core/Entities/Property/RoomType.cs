namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

/// <summary>
/// 房型字典实体 — 标识房源的类型分类
/// 如 "单间"、"一室一厅"、"两室一厅"、"商铺"、"办公室" 等
/// 用于业务分类统计和定价基准
/// </summary>
public class RoomType : AuditableEntity
{
    /// <summary>房型名称，如 "单间"、"一室一厅"</summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>房型描述，可选</summary>
    public string? Description { get; private set; }
    /// <summary>是否启用，false 表示已废弃不再使用</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private RoomType() { }

    /// <summary>
    /// 创建房型
    /// </summary>
    /// <param name="name">房型名称，不能为空</param>
    /// <exception cref="ArgumentException">当名称为空时抛出</exception>
    public RoomType(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("房型名称不能为空", nameof(name));
        Name = name;
        IsActive = true;
    }

    /// <summary>
    /// 重命名房型
    /// </summary>
    /// <param name="name">新名称，不能为空</param>
    /// <exception cref="ArgumentException">当名称为空时抛出</exception>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("房型名称不能为空", nameof(name));
        Name = name;
    }
    /// <summary>设置描述信息，null 表示清空</summary>
    public void SetDescription(string? description) => Description = description;
    /// <summary>启用房型</summary>
    public void Activate() => IsActive = true;
    /// <summary>停用房型</summary>
    public void Deactivate() => IsActive = false;
}
