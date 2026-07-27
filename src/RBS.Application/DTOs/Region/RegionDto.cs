namespace RBS.Application.DTOs.Region;

/// <summary>行政区划 DTO</summary>
public class RegionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public int Level { get; set; }
    public string? FullPath { get; set; }
    public int SortOrder { get; set; }
    /// <summary>是否有子级（用于前端级联判断是否可展开）</summary>
    public bool HasChildren { get; set; }
}
