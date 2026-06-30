using System.Text.Json;
using System.Text.RegularExpressions;
using RBS.Application.DTOs.Import;
using RBS.Core.Entities.Import;
using RBS.Core.Entities.Property;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Import;

public partial class HousingUnitImportHandler : IImportTypeHandler
{
    private readonly IUnitOfWork _uow;

    public HousingUnitImportHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public string ImportType => "HousingUnit";

    // ==================== 常量 ====================

    private static readonly HashSet<string> ValidOrientations = new(StringComparer.OrdinalIgnoreCase)
    {
        "东", "南", "西", "北", "南北通透", "东南", "西南", "东北", "西北"
    };

    /// <summary>批量导入房屋的审批类型 Code</summary>
    private const string ApprovalTypeCode = "BATCH_IMPORT_ROOMS";

    // ==================== 审批类型 ====================

    public async Task<Guid> GetApprovalTypeIdAsync(Guid companyId)
    {
        var type = await _uow.FindApprovalTypeByCodeAsync(ApprovalTypeCode);
        return type?.Id ?? throw new InvalidOperationException(
            $"未找到审批类型「{ApprovalTypeCode}」，请执行 SeedApprovalTypes_3Level.sql 脚本初始化审批数据");
    }

    // ==================== 数据转换 + 校验（9 项） ====================

    public ImportBatchItem ParseAndValidate(Guid importBatchId, int rowIndex, JsonElement data, ImportValidationContext context)
    {
        var item = new ImportBatchItemHousingUnit(importBatchId, rowIndex)
        {
            BuildingName = data.GetStringOrNull("buildingName") ?? "",
            BuildingCode = data.GetStringOrNull("buildingCode"),
            BuildingAddress = data.GetStringOrNull("buildingAddress"),
            FloorName = data.GetStringOrNull("floorName") ?? "",
            FloorSortOrder = ExtractFloorSortOrder(data.GetStringOrNull("floorName") ?? ""),
            UnitNo = data.GetStringOrNull("unitNo") ?? "",
            FullCode = data.GetStringOrNull("fullCode"),
            RoomTypeName = data.GetStringOrNull("roomTypeName"),
            Area = data.GetDecimalOrNull("area"),
            Orientation = data.GetStringOrNull("orientation"),
        };

        // 自动生成 FullCode
        if (string.IsNullOrEmpty(item.FullCode) && !string.IsNullOrEmpty(item.BuildingName))
        {
            item.FullCode = $"{item.BuildingName}-{item.FloorName}-{item.UnitNo}";
        }

        // ===== 开始 9 项校验 =====

        // ① 座楼名称 — 必填
        if (string.IsNullOrWhiteSpace(item.BuildingName))
        { item.SetFailed("ERR_REQUIRED_BUILDING", "座楼名称为空", "请在「座楼名称」列填写座楼名称，如：A栋"); return item; }

        // ② 楼层 — 必填
        if (string.IsNullOrWhiteSpace(item.FloorName))
        { item.SetFailed("ERR_REQUIRED_FLOOR", "楼层为空", "请在「楼层」列填写楼层，格式如：1层、2层"); return item; }

        // ③ 房号 — 必填
        if (string.IsNullOrWhiteSpace(item.UnitNo))
        { item.SetFailed("ERR_REQUIRED_UNIT", "房号为空", "请在「房号」列填写房号，如：101"); return item; }

        // ④ 房型 — 必填
        if (string.IsNullOrWhiteSpace(item.RoomTypeName))
        { item.SetFailed("ERR_REQUIRED_ROOMTYPE", "房型为空", "请在「房型」列从下拉列表中选择系统现有房型"); return item; }

        // ⑤ 房型名称 → 匹配系统房型
        var roomTypeMap = context.CustomData.GetValueOrDefault("RoomTypeMap") as Dictionary<string, Guid>;
        if (roomTypeMap != null)
        {
            if (roomTypeMap.TryGetValue(item.RoomTypeName, out var matchedId))
            {
                item.RoomTypeId = matchedId;
            }
            else
            {
                var available = string.Join("、", roomTypeMap.Keys.Take(10));
                item.SetFailed("ERR_INVALID_ROOMTYPE",
                    $"房型「{item.RoomTypeName}」不存在，系统可用房型：{available}",
                    "请从 Sheet2 参考数据中选择系统现有房型名称填入");
                return item;
            }
        }

        // ⑥ 面积 — 大于0
        if (item.Area.HasValue && item.Area.Value <= 0)
        { item.SetFailed("ERR_INVALID_AREA", $"面积必须是大于0的数字，当前值：{item.Area}", "请检查「面积」列，填写正数，如：85.5"); return item; }

        // ⑦ 朝向 — 仅允许预设值
        if (!string.IsNullOrEmpty(item.Orientation) && !ValidOrientations.Contains(item.Orientation))
        {
            item.SetFailed("ERR_INVALID_ORIENTATION",
                $"朝向「{item.Orientation}」不合法，仅允许：{string.Join("、", ValidOrientations)}",
                "请在「朝向」列从下拉列表中选择，可选值：东、南、西、北、南北通透、东南、西南、东北、西北");
            return item;
        }

        // ⑧ 房源唯一性 — (BuildingName + FloorName + UnitNo)
        var key = $"{item.BuildingName}|{item.FloorName}|{item.UnitNo}";
        if (context.BatchKeys.Contains(key))
        { item.SetFailed("ERR_DUPLICATE_UNIT", $"该房源（{item.BuildingName}-{item.FloorName}-{item.UnitNo}）在本次导入中重复", "请删除重复行，每条座楼+楼层+房号组合只能出现一次"); return item; }
        if (context.ExistingKeys.Contains(key))
        { item.SetFailed("ERR_DUPLICATE_UNIT", $"该房源（{item.BuildingName}-{item.FloorName}-{item.UnitNo}）已存在", "该房源已存在系统中，如需修改请删除对应行后重导，或修改座楼/楼层/房号避免重复"); return item; }
        context.BatchKeys.Add(key);

        // ⑨ 完整编码唯一性
        var fullCode = item.FullCode ?? $"{item.BuildingName}-{item.FloorName}-{item.UnitNo}";
        if (context.BatchKeys.Contains($"FC:{fullCode}"))
        { item.SetFailed("ERR_DUPLICATE_FULLCODE", $"完整编码「{fullCode}」在本次导入中重复", "请修改座楼/楼层/房号组合避免完整编码重复"); return item; }
        if (context.CustomData.TryGetValue("ExistingFullCodes", out var fcObj) && fcObj is HashSet<string> existingFcs && existingFcs.Contains(fullCode))
        { item.SetFailed("ERR_DUPLICATE_FULLCODE", $"完整编码「{fullCode}」已存在", "该完整编码已存在，请修改座楼/楼层/房号组合避免重复"); return item; }
        context.BatchKeys.Add($"FC:{fullCode}");

        // ===== 金额自动计算（不阻塞，仅标记警告） =====
        CalculatePrice(item, context);

        return item;
    }

    // ==================== 金额自动计算 ====================

    private static void CalculatePrice(ImportBatchItemHousingUnit item, ImportValidationContext context)
    {
        if (item.FloorSortOrder == null || item.FloorSortOrder <= 0)
        {
            item.PriceWarning = $"楼层「{item.FloorName}」格式无法识别（未能提取楼层数字），租金未设置";
            return;
        }

        var allBands = context.CustomData.GetValueOrDefault("FloorLevelBands") as List<FloorLevelBand>;
        var band = allBands?.OrderBy(b => b.MinLevel)
            .FirstOrDefault(b => b.MinLevel <= item.FloorSortOrder && b.MaxLevel >= item.FloorSortOrder);

        if (band == null)
        {
            item.PriceWarning = $"楼层「{item.FloorName}」超出所有楼层分级范围，租金未设置";
            return;
        }

        if (item.RoomTypeId == null) return;

        var pricing = (context.CustomData.GetValueOrDefault("CompanyPricing") as List<RoomPricingStandard>)
            ?.FirstOrDefault(p => p.RoomTypeId == item.RoomTypeId.Value && p.FloorLevelBandId == band.Id);

        if (pricing != null)
        {
            item.BaseRentAmount = pricing.RentAmount;
        }
        else
        {
            item.PriceWarning = $"房型「{item.RoomTypeName}」在{band.Name}无匹配定价标准，租金未设置";
        }
    }

    // ==================== 审批通过后执行 ====================

    public async Task<int> ExecuteAsync(ImportBatch batch, CancellationToken ct)
    {
        var existingUnits = await _uow.HousingUnits.GetAllAsync(ct);
        var existingKeys = new HashSet<string>(
            existingUnits.Select(u => $"{u.BuildingName}|{u.FloorName}|{u.UnitNo}"),
            StringComparer.OrdinalIgnoreCase);

        int created = 0;
        foreach (var item in batch.Items.OfType<ImportBatchItemHousingUnit>().Where(i => i.IsValid))
        {
            var key = $"{item.BuildingName}|{item.FloorName}|{item.UnitNo}";
            if (existingKeys.Contains(key)) continue; // 二次检查唯一性

            var unit = new HousingUnit(
                item.BuildingName ?? "",
                item.FloorName ?? "",
                item.FloorSortOrder ?? 0,
                item.UnitNo ?? "",
                batch.CompanyId);

            unit.SetBuildingInfo(item.BuildingCode, item.BuildingAddress);
            unit.SetFullCode(item.FullCode ?? $"{item.BuildingName}-{item.FloorName}-{item.UnitNo}");
            unit.UpdateDetails(item.RoomTypeId, item.Area, item.Orientation, item.BaseRentAmount);

            await _uow.HousingUnits.AddAsync(unit, ct);
            existingKeys.Add(key);
            created++;
        }

        return created;
    }

    // ==================== 工具方法 ====================

    [GeneratedRegex(@"(\d+)")]
    private static partial Regex FloorNumberRegex();

    private static int ExtractFloorSortOrder(string floorName)
    {
        if (string.IsNullOrWhiteSpace(floorName)) return 0;
        var m = FloorNumberRegex().Match(floorName);
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    private static decimal? ParseDecimal(object? value)
    {
        if (value == null) return null;
        if (value is decimal d) return d;
        if (decimal.TryParse(value.ToString(), out var result)) return result;
        return null;
    }
}
