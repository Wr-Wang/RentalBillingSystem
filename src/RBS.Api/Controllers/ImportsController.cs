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

        // Load items via Application Service
        var items = await _importService.GetBatchItemsAsync(id, ct);

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
