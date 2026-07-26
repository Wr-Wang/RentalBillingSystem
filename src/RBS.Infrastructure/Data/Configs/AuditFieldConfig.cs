using RBS.Application.Common.Interfaces;

namespace RBS.Infrastructure.Data.Configs;

/// <summary>
/// 审计字段配置 — 定义每个实体在审计展示时的中文名和关键标识字段
/// </summary>
public class AuditFieldConfig
{
    /// <summary>实体中文展示名（如"公司"、"合同"）</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>关键标识字段列表（用于识别记录，如合同号、租客名）</summary>
    public string[] KeyFields { get; set; } = Array.Empty<string>();
}

/// <summary>
/// 审计字段配置加载器 — 从 JSON 文件加载
/// </summary>
public class AuditFieldConfigLoader
{
    private readonly Dictionary<string, AuditFieldConfig> _configs;

    public AuditFieldConfigLoader(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _configs = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, AuditFieldConfig>>(json, options)
            ?? new Dictionary<string, AuditFieldConfig>();
    }

    /// <summary>
    /// 获取指定表的审计配置（大小写不敏感）
    /// </summary>
    public AuditFieldConfig? GetConfig(string tableName)
    {
        var key = tableName.ToLowerInvariant();
        return _configs.TryGetValue(key, out var cfg) ? cfg : null;
    }

    /// <summary>
    /// 获取所有可审计的表清单（用于前端下拉动态加载）
    /// </summary>
    public List<AuditTableInfo> GetAllTables()
    {
        return _configs.Select(kv => new AuditTableInfo
        {
            TableName = kv.Key,
            DisplayName = kv.Value.DisplayName
        }).OrderBy(t => t.DisplayName).ToList();
    }
}
