using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Contract;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/changerequests")]
[Authorize]
public class ChangeRequestsController : ControllerBase
{
    private readonly IChangeRequestService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public ChangeRequestsController(
        IChangeRequestService service,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _service = service;
        _currentUser = currentUser;
        _uow = uow;
    }

    /// <summary>获取指定合同的变更请求列表</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        if (!contractId.HasValue)
            return Ok(Array.Empty<ChangeRequestDto>());

        var result = await _service.GetByContractAsync(contractId.Value, ct);
        return Ok(result);
    }

    /// <summary>获取变更请求详情</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>创建变更请求</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChangeRequestDto request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty || string.IsNullOrEmpty(request.ChangeType))
            return BadRequest(new { message = "参数错误" });

        var userId = _currentUser.UserId;
        var result = await _service.CreateAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>提交变更审批</summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        try
        {
            var result = await _service.SubmitAsync(id, userId, ct);
            return Ok(new { message = "已提交审批", id = result.Id, status = result.Status, approvalRequestId = result.ApprovalRequestId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
