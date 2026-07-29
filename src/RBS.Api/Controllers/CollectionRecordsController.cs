using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollectionRecordsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IContractService _contractService;
    private readonly INotificationService _notificationService;
    public CollectionRecordsController(IUnitOfWork uow, IContractService contractService,
        INotificationService notificationService)
    {
        _uow = uow;
        _contractService = contractService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        var all = await _uow.CollectionRecords.GetAllAsync(ct);
        if (contractId.HasValue && contractId.Value != Guid.Empty)
            all = all.Where(r => r.ContractId == contractId.Value).ToList();

        // 批量查询合同号
        var contractIds = all.Select(r => r.ContractId).Distinct().ToList();
        var contractNoMap = await _contractService.GetIdNoPairsAsync(contractIds, ct);

        var result = all.Select(r => new
        {
            r.Id, r.ContractId,
            ContractNo = contractNoMap.GetValueOrDefault(r.ContractId, ""),
            r.StageNo, r.Channel, r.Content, r.Status, r.SentAt,
            r.OperatedBy, r.CompanyId, r.CreatedAt
        }).OrderByDescending(r => r.CreatedAt);

        return Ok(result);
    }

    [HttpPost("manual")]
    public async Task<IActionResult> Manual([FromBody] ManualCollectionRequest request, CancellationToken ct)
    {
        if (request.ContractId == Guid.Empty)
            return BadRequest(new { message = "contractId 不能为空" });

        CollectionStage? stage = null;
        if (request.StageId.HasValue && request.StageId.Value != Guid.Empty)
        {
            stage = await _uow.CollectionStages.GetByIdAsync(request.StageId.Value, ct);
            if (stage == null)
                return BadRequest(new { message = "指定的催缴阶段不存在" });
        }
        else
        {
            // 使用默认催缴阶段（最低阶段号）
            var stages = await _uow.CollectionStages.GetAllAsync(ct);
            stage = stages.Where(s => s.IsAuto).OrderBy(s => s.StageNo).FirstOrDefault();
            if (stage == null)
                return BadRequest(new { message = "未配置催缴阶段" });
        }

        var channel = stage.ActionType switch
        {
            "SMS" => "SMS",
            "CALL" => "PHONE",
            "VISIT" => "VISIT",
            "LEGAL" => "LEGAL",
            _ => "SMS"
        };
        var content = $"{stage.StageName} - 手动催缴";
        var companyId = stage.CompanyId;

        var record = new CollectionRecord(request.ContractId, stage.StageNo, channel, content, companyId);
        await _uow.CollectionRecords.AddAsync(record, ct);
        await _uow.CommitAsync(ct);

        // 发送催缴系统通知
        try
        {
            var contractNoMap = await _contractService.GetIdNoPairsAsync(new List<Guid> { request.ContractId }, ct);
            var contractNo = contractNoMap.GetValueOrDefault(request.ContractId, "");
            await _notificationService.NotifyRoleAsync("OpsSupervisor", "System",
                $"催缴通知 - {contractNo}",
                $"合同 {contractNo} 已手动发起「{stage.StageName}」催缴（{channel}）",
                "CollectionRecord", record.Id, companyId, ct);
        }
        catch
        {
            // 通知失败不影响催缴记录创建
        }

        return Ok(new { message = "催缴记录已创建", id = record.Id });
    }
}

public class ManualCollectionRequest
{
    public Guid ContractId { get; set; }
    public Guid? StageId { get; set; }
}
