using System.Text.Json;

namespace RBS.Application.DTOs.Import;

/// <summary>提交导入请求</summary>
public class ImportRequest
{
    public string ImportType { get; set; } = "";
    public Guid CompanyId { get; set; }
    public string FileName { get; set; } = "";
    public List<ImportRowDto> Items { get; set; } = new();
}

/// <summary>单行导入数据</summary>
public class ImportRowDto
{
    public int RowIndex { get; set; }
    /// <summary>行数据（JSON对象，由Handler解析）</summary>
    public System.Text.Json.JsonElement Data { get; set; }
}

/// <summary>导入结果</summary>
public class ImportResponse
{
    public Guid BatchId { get; set; }
    public string ImportType { get; set; } = "";
    public Guid? ApprovalRequestId { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int FailedRows { get; set; }
    public List<ImportFailureDto> Failures { get; set; } = new();
}

/// <summary>失败行信息</summary>
public class ImportFailureDto
{
    public int RowIndex { get; set; }
    public string? BuildingName { get; set; }
    public string? FloorName { get; set; }
    public string? UnitNo { get; set; }
    public string ErrorCode { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public string? FixSuggestion { get; set; }
}

/// <summary>导入批次行明细响应</summary>
public class ImportBatchItemResponse
{
    public int RowIndex { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FixSuggestion { get; set; }
    // 房源字段
    public string? BuildingName { get; set; }
    public string? FloorName { get; set; }
    public string? UnitNo { get; set; }
    public string? FullCode { get; set; }
    public string? RoomTypeName { get; set; }
    public decimal? Area { get; set; }
    public string? Orientation { get; set; }
    public decimal? BaseRentAmount { get; set; }
    public string? PriceWarning { get; set; }
}

/// <summary>JsonElement 辅助扩展</summary>
public static class JsonElementExtensions
{
    public static string? GetStringOrNull(this JsonElement el, string property)
        => el.TryGetProperty(property, out var p) && p.ValueKind != JsonValueKind.Null
            ? p.GetString() : null;

    public static decimal? GetDecimalOrNull(this JsonElement el, string property)
    {
        if (!el.TryGetProperty(property, out var p) || p.ValueKind == JsonValueKind.Null)
            return null;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d))
            return d;
        if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var d2))
            return d2;
        return null;
    }
}
