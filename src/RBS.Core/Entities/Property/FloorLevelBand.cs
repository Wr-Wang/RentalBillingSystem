namespace RBS.Core.Entities.Property;
using RBS.Core.Entities.Base;

/// <summary>
/// 楼层段字典实体 — 将楼层按自然层划分为若干区间段
/// 如 "低区(1-5层)"、"中区(6-15层)"、"高区(16-30层)"
/// 用于结合房型进行定价标准管理和批量租金设定
/// </summary>
public class FloorLevelBand : AuditableEntity, IHasCompany
{
    /// <summary>楼层段名称，如 "低区"、"中区"、"高区"</summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>起始楼层号（含），楼层段的下界</summary>
    public int MinLevel { get; private set; }
    /// <summary>截止楼层号（含），楼层段的上界</summary>
    public int MaxLevel { get; private set; }
    /// <summary>楼层段描述，可选</summary>
    public string? Description { get; private set; }
    /// <summary>所属公司标识</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>仅供 EF Core 反序列化使用</summary>
    private FloorLevelBand() { }

    /// <summary>
    /// 创建楼层段
    /// </summary>
    /// <param name="name">楼层段名称</param>
    /// <param name="minLevel">起始楼层号（含）</param>
    /// <param name="maxLevel">截止楼层号（含）</param>
    /// <param name="companyId">所属公司标识</param>
    public FloorLevelBand(string name, int minLevel, int maxLevel, Guid companyId) { Name = name; MinLevel = minLevel; MaxLevel = maxLevel; CompanyId = companyId; }

    /// <summary>重命名楼层段</summary>
    public void Rename(string name) => Name = name;
    /// <summary>设置起始楼层号</summary>
    public void SetMinLevel(int level)
    {
        if (level > MaxLevel)
            throw new ArgumentException($"起始楼层({level})不能大于截止楼层({MaxLevel})", nameof(level));
        MinLevel = level;
    }

    /// <summary>设置截止楼层号</summary>
    public void SetMaxLevel(int level)
    {
        if (level < MinLevel)
            throw new ArgumentException($"截止楼层({level})不能小于起始楼层({MinLevel})", nameof(level));
        MaxLevel = level;
    }
    /// <summary>设置描述信息，null 表示清空</summary>
    public void SetDescription(string? desc) => Description = desc;
}
