using System.Data;
using System.Text.Json;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Application.DTOs.Import;
using RBS.Core.Entities.Import;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Import;

/// <summary>通用导入服务 — 校验 → 暂存 → 提交审批 → 审批通过后执行</summary>
public class ImportService : IImportService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IApprovalService _approvalService;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly Dictionary<string, IImportTypeHandler> _handlers;

    public ImportService(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ITenantService tenantService,
        IApprovalService approvalService,
        IDbConnectionFactory db,
        ISqlLoader sql,
        IEnumerable<IImportTypeHandler> handlers)
    {
        _uow = uow;
        _currentUser = currentUser;
        _tenantService = tenantService;
        _approvalService = approvalService;
        _db = db;
        _sql = sql;
        _handlers = handlers.ToDictionary(h => h.ImportType, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<ImportResponse> SubmitAsync(ImportRequest request, CancellationToken ct)
    {
        var response = new ImportResponse
        {
            ImportType = request.ImportType,
            TotalRows = request.Items.Count
        };

        if (request.Items.Count == 0) return response;

        // 1. 获取 handler
        if (!_handlers.TryGetValue(request.ImportType, out var handler))
            throw new InvalidOperationException($"不支持的导入类型：{request.ImportType}");

        var companyId = request.CompanyId != Guid.Empty ? request.CompanyId
            : _tenantService.EffectiveCompanyId ?? throw new InvalidOperationException("无法确定公司");

        // 2. 构建校验上下文
        var context = await BuildValidationContextAsync(handler, companyId, ct);

        // 3. 创建批次
        var batch = new ImportBatch(companyId, request.ImportType, request.FileName);
        await _uow.ImportBatches.AddAsync(batch, ct);

        // 4. 逐行处理
        var failures = new List<ImportFailureDto>();
        int validCount = 0;

        foreach (var row in request.Items)
        {
            var item = handler.ParseAndValidate(batch.Id, row.RowIndex, row.Data, context);

            if (item.IsValid)
            {
                validCount++;
            }
            else
            {
                // 记录批次内 key 防重
                if (item is ImportBatchItemHousingUnit hu)
                {
                    var key = $"{hu.BuildingName}|{hu.FloorName}|{hu.UnitNo}";
                    context.BatchKeys.Add(key);
                    context.BatchKeys.Add($"FC:{hu.FullCode}");
                }

                failures.Add(new ImportFailureDto
                {
                    RowIndex = item.RowIndex,
                    ErrorCode = item.ErrorCode ?? "",
                    ErrorMessage = item.ErrorMessage ?? "",
                    FixSuggestion = item.FixSuggestion
                });
            }

            // 提取房源字段用于失败列表展示
            if (item is ImportBatchItemHousingUnit hu2)
            {
                if (!item.IsValid && failures.Count > 0)
                {
                    var f = failures[^1];
                    f.BuildingName = hu2.BuildingName;
                    f.FloorName = hu2.FloorName;
                    f.UnitNo = hu2.UnitNo;
                }
            }

            await _uow.ImportBatchItems.AddAsync(item, ct);
        }

        batch.SetRowCounts(request.Items.Count, 0); // TotalRows
        batch.SetRowCounts(validCount, failures.Count);

        // 5. 如果有有效行 → 提交审批
        Guid? approvalRequestId = null;
        if (validCount > 0)
        {
            var approvalTypeId = await handler.GetApprovalTypeIdAsync(companyId);
            var submitResult = await _approvalService.SubmitAsync(new SubmitApprovalRequest
            {
                ApprovalTypeId = approvalTypeId,
                Title = $"[批量导入] {request.FileName}",
                Description = $"共 {validCount} 条有效数据待审批",
                TargetEntityId = batch.Id,
                TargetEntityType = "Import"
            }, ct);

            approvalRequestId = submitResult?.Id;
            batch.SetApprovalRequest(approvalRequestId);
        }

        await _uow.CommitAsync(ct);

        response.BatchId = batch.Id;
        response.ApprovalRequestId = approvalRequestId;
        response.ValidRows = validCount;
        response.FailedRows = failures.Count;
        response.Failures = failures;

        return response;
    }

    public async Task ExecuteApprovedImportAsync(Guid batchId, CancellationToken ct)
    {
        var batch = await _uow.GetImportBatchWithItemsAsync(batchId, ct);
        if (batch == null) return;
        if (batch.Status != "PendingApproval") return; // 幂等

        if (!_handlers.TryGetValue(batch.ImportType, out var handler))
            throw new InvalidOperationException($"不支持的导入类型：{batch.ImportType}");

        var created = await handler.ExecuteAsync(batch, ct);

        batch.Approve();
        batch.SetRowCounts(created, 0);

        await _uow.CommitAsync(ct);
    }

    public async Task<List<ImportBatchItemResponse>> GetBatchItemsAsync(Guid batchId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync<dynamic>(
            _sql.Get("Import.Select.BatchItem.ByBatchId"),
            new { Id = batchId });

        var items = rows.Select(r =>
        {
            var dict = (IDictionary<string, object>)r;
            var resp = new ImportBatchItemResponse
            {
                RowIndex = (int)dict["RowIndex"],
                IsValid = (bool)dict["IsValid"],
                ErrorCode = dict["ErrorCode"]?.ToString(),
                ErrorMessage = dict["ErrorMessage"]?.ToString(),
                FixSuggestion = dict["FixSuggestion"]?.ToString()
            };

            // HousingUnit-specific fields
            string? importType = dict["ImportType"]?.ToString();
            if (importType == "HousingUnit" || dict.ContainsKey("BuildingName"))
            {
                resp.BuildingName = dict["BuildingName"]?.ToString();
                resp.FloorName = dict["FloorName"]?.ToString();
                resp.UnitNo = dict["UnitNo"]?.ToString();
                resp.FullCode = dict["FullCode"]?.ToString();
                resp.RoomTypeName = dict["RoomTypeName"]?.ToString();
                resp.Area = dict["Area"] != null ? Convert.ToDecimal(dict["Area"]) : null;
                resp.Orientation = dict["Orientation"]?.ToString();
                resp.BaseRentAmount = dict["BaseRentAmount"] != null ? Convert.ToDecimal(dict["BaseRentAmount"]) : null;
            }
            return resp;
        }).ToList();

        return items;
    }

    private async Task<ImportValidationContext> BuildValidationContextAsync(
        IImportTypeHandler handler, Guid companyId, CancellationToken ct)
    {
        var context = new ImportValidationContext
        {
            CompanyId = companyId
        };

        // 加载现有房源唯一键
        var existingUnits = await _uow.HousingUnits.GetAllAsync(ct);
        context.ExistingKeys = new HashSet<string>(
            existingUnits.Select(u => $"{u.BuildingName}|{u.FloorName}|{u.UnitNo}"),
            StringComparer.OrdinalIgnoreCase);

        // FullCodes
        context.CustomData["ExistingFullCodes"] = new HashSet<string>(
            existingUnits.Where(u => u.FullCode != null).Select(u => u.FullCode!),
            StringComparer.OrdinalIgnoreCase);

        // 房型映射
        var allRoomTypes = await _uow.RoomTypes.GetAllAsync(ct);
        context.CustomData["RoomTypeMap"] = allRoomTypes
            .ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);

        // 楼层分级 + 定价标准
        var allBands = await _uow.FloorLevelBands.GetAllAsync(ct);
        context.CustomData["FloorLevelBands"] = allBands;

        var companyPricing = (await _uow.RoomPricingStandards.GetAllAsync(ct))
            .Where(p => p.CompanyId == companyId).ToList();
        context.CustomData["CompanyPricing"] = companyPricing;

        return context;
    }
}
