using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/change-requests")]
[Authorize]
public class ChangeRequestsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    public ChangeRequestsController(IUnitOfWork uow) => _uow = uow;

    /// <summary>获取指定合同的变更请求列表</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        var all = await _uow.ChangeRequests.GetAllAsync(ct);
        if (contractId.HasValue)
            all = all.Where(c => c.ContractId == contractId.Value).ToList();
        return Ok(all.OrderByDescending(c => c.CreatedAt));
    }

    /// <summary>创建合同变更请求</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChangeRequest request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty || string.IsNullOrEmpty(request.ChangeType))
            return BadRequest(new { message = "参数错误" });

        var entity = new ChangeRequest(request.ContractId, request.CompanyId, request.ChangeType, request.Reason);
        if (request.EffectiveDate.HasValue)
            entity.SetEffectiveDate(request.EffectiveDate.Value);

        if (request.Items?.Count > 0)
        {
            foreach (var item in request.Items)
            {
                entity.AddItem(item.TargetType, item.TargetId, item.FieldName,
                    item.OldValue, item.NewValue, item.OldValueDecimal, item.NewValueDecimal);
            }
        }

        await _uow.ChangeRequests.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return Ok(entity);
    }

    /// <summary>提交变更审批</summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var entity = await _uow.ChangeRequests.GetByIdAsync(id, ct);
        if (entity == null) return NotFound();

        try
        {
            entity.SubmitForApproval();
            await _uow.CommitAsync(ct);
            return Ok(new { message = "已提交审批", id = entity.Id, status = entity.Status });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreateChangeRequest
{
    public Guid ContractId { get; set; }
    public Guid CompanyId { get; set; }
    public string ChangeType { get; set; } = "";
    public string? Reason { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public List<ChangeRequestItemDto>? Items { get; set; }
}

public class ChangeRequestItemDto
{
    public string TargetType { get; set; } = "";
    public Guid? TargetId { get; set; }
    public string FieldName { get; set; } = "";
    public string? OldValue { get; set; }
    public string NewValue { get; set; } = "";
    public decimal? OldValueDecimal { get; set; }
    public decimal? NewValueDecimal { get; set; }
}
