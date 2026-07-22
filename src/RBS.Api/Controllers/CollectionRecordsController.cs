using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using Dapper;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollectionRecordsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    public CollectionRecordsController(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? contractId, CancellationToken ct)
    {
        var all = await _uow.CollectionRecords.GetAllAsync(ct);
        if (contractId.HasValue && contractId.Value != Guid.Empty)
            all = all.Where(r => r.ContractId == contractId.Value).ToList();

        // 批量查询合同号
        var contractIds = all.Select(r => r.ContractId).Distinct().ToList();
        Dictionary<Guid, string> contractNoMap;
        using (var conn = _db.CreateConnection())
        {
            conn.Open();
            var contracts = await conn.QueryAsync<(Guid Id, string No)>(
                _sql.Get("Lease.Select.Contract.IdNoPairs"),
                new { Ids = contractIds });
            contractNoMap = contracts.ToDictionary(c => c.Id, c => c.No);
        }

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
        return Ok(new { message = "催缴记录已创建", id = record.Id });
    }
}

public class ManualCollectionRequest
{
    public Guid ContractId { get; set; }
    public Guid? StageId { get; set; }
}
