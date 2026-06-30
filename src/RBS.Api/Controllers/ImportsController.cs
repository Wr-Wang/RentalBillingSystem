using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Import;
using RBS.Core.Entities.Import;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportsController : ControllerBase
{
    private readonly IImportService _importService;
    private readonly IUnitOfWork _uow;

    public ImportsController(IImportService importService, IUnitOfWork uow)
    {
        _importService = importService;
        _uow = uow;
    }

    /// <summary>提交导入（校验 → 暂存 → 提交审批）</summary>
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] ImportRequest request, CancellationToken ct)
    {
        var result = await _importService.SubmitAsync(request, ct);
        return Ok(result);
    }

    /// <summary>获取导入批次详情</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var batch = await _uow.GetImportBatchWithItemsAsync(id, ct);
        if (batch == null) return NotFound();

        // 查询创建人信息
        string? createdByName = null;
        string? createdByAccount = null;
        if (batch.CreatedBy != Guid.Empty)
        {
            var creator = await _uow.Users.GetByIdAsync(batch.CreatedBy, ct);
            if (creator != null)
            {
                createdByName = creator.DisplayName;
                createdByAccount = creator.Username;
            }
        }

        return Ok(new
        {
            batch.Id,
            batch.ImportType,
            batch.FileName,
            batch.TotalRows,
            batch.ValidRows,
            batch.FailedRows,
            StatusLabel = batch.Status switch
            {
                "PendingApproval" => "待审批",
                "Approved" => "已通过",
                "Rejected" => "已驳回",
                "Cancelled" => "已撤回",
                _ => batch.Status
            },
            batch.Status,
            batch.ApprovalRequestId,
            batch.CreatedAt,
            createdByName,
            createdByAccount,
            Items = batch.Items.OrderBy(i => i.RowIndex).Select<ImportBatchItem, object>(i =>
            {
                if (i is ImportBatchItemHousingUnit hu)
                    return new ImportBatchItemResponse
                    {
                        RowIndex = i.RowIndex,
                        IsValid = i.IsValid,
                        ErrorCode = i.ErrorCode,
                        ErrorMessage = i.ErrorMessage,
                        FixSuggestion = i.FixSuggestion,
                        BuildingName = hu.BuildingName,
                        FloorName = hu.FloorName,
                        UnitNo = hu.UnitNo,
                        FullCode = hu.FullCode,
                        RoomTypeName = hu.RoomTypeName,
                        Area = hu.Area,
                        Orientation = hu.Orientation,
                        BaseRentAmount = hu.BaseRentAmount,
                        PriceWarning = hu.PriceWarning
                    };
                return new ImportBatchItemResponse
                {
                    RowIndex = i.RowIndex,
                    IsValid = i.IsValid,
                    ErrorCode = i.ErrorCode,
                    ErrorMessage = i.ErrorMessage,
                    FixSuggestion = i.FixSuggestion
                };
            })
        });
    }

    /// <summary>查询导入记录列表</summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? companyId,
        [FromQuery] string? importType,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var all = await _uow.ImportBatches.GetAllAsync(ct);
        var query = all.AsEnumerable();
        if (companyId.HasValue && companyId.Value != Guid.Empty)
            query = query.Where(b => b.CompanyId == companyId.Value);
        if (!string.IsNullOrEmpty(importType))
            query = query.Where(b => b.ImportType == importType);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(b => b.Status == status);

        return Ok(query.OrderByDescending(b => b.CreatedAt).Select(b => new
        {
            b.Id,
            b.ImportType,
            b.FileName,
            b.TotalRows,
            b.ValidRows,
            b.FailedRows,
            b.Status,
            b.ApprovalRequestId,
            b.CreatedAt,
            b.CreatedBy
        }).ToList());
    }
}
