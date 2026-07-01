using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Import;
using RBS.Core.Entities.Import;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using System.Data;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportsController : ControllerBase
{
    private readonly IImportService _importService;
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;

    public ImportsController(IImportService importService, IUnitOfWork uow, IDbConnectionFactory db)
    {
        _importService = importService;
        _uow = uow;
        _db = db;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] ImportRequest request, CancellationToken ct)
    {
        var result = await _importService.SubmitAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var batch = await _uow.GetImportBatchWithItemsAsync(id, ct);
        if (batch == null) return NotFound();

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

        // Load items with proper type mapping via Dapper
        using var conn = _db.CreateConnection(); conn.Open();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM ImportBatchItems WHERE ImportBatchId = @Id ORDER BY RowIndex",
            new { Id = id });

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

        return Ok(new
        {
            batch.Id, batch.ImportType, batch.FileName, batch.TotalRows,
            batch.ValidRows, batch.FailedRows,
            StatusLabel = batch.Status switch
            {
                "PendingApproval" => "待审批", "Approved" => "已通过",
                "Rejected" => "已驳回", "Cancelled" => "已撤回", _ => batch.Status
            },
            batch.Status, batch.ApprovalRequestId, batch.CreatedAt,
            createdByName, createdByAccount,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? companyId, [FromQuery] string? importType, [FromQuery] string? status,
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
            b.Id, b.ImportType, b.FileName, b.TotalRows,
            b.ValidRows, b.FailedRows, b.Status,
            b.ApprovalRequestId, b.CreatedAt, b.CreatedBy
        }).ToList());
    }
}
