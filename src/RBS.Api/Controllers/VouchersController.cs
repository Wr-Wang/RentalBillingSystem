using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Accounting;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;

namespace RBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VouchersController : ControllerBase
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantService _tenant;
    private readonly IAccountingPeriodService _periodService;

    public VouchersController(
        IDbConnectionFactory db, ISqlLoader sql,
        ICurrentUserService currentUser, ITenantService tenant,
        IAccountingPeriodService periodService)
    {
        _db = db;
        _sql = sql;
        _currentUser = currentUser;
        _tenant = tenant;
        _periodService = periodService;
    }

    /// <summary>生成凭证号格式：PZ-YYYYMMDD-NNNN</summary>
    private async Task<string> GenerateVoucherNoAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var seq = await conn.QuerySingleAsync<int>(
            "SELECT ISNULL(MAX(CAST(RIGHT(VoucherNo, 4) AS INT)), 0) + 1 FROM Vouchers " +
            "WHERE VoucherNo LIKE @Pattern AND CreatedAt >= @Today",
            new { Pattern = $"PZ-{today}-%", Today = DateOnly.FromDateTime(DateTime.UtcNow) }, tx);
        return $"PZ-{today}-{seq:D4}";
    }

    /// <summary>创建草稿凭证（手动录入）</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVoucherRequest request, CancellationToken ct)
    {
        if (request.Entries == null || request.Entries.Count < 2)
            return BadRequest(new { message = "凭证至少需要两条分录" });

        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var companyId = _tenant.CompanyId ?? Guid.Empty;
            if (companyId == Guid.Empty)
                return BadRequest(new { message = "未找到公司信息" });

            var period = request.VoucherDate.ToString("yyyy-MM");
            var voucherId = Guid.NewGuid();
            var voucherNo = await GenerateVoucherNoAsync(conn, tx, ct);

            // 创建领域实体验证借贷平衡
            var voucher = new Voucher(voucherNo, request.VoucherDate, request.Description);
            foreach (var entry in request.Entries)
            {
                voucher.AddEntry(entry.AccountingSubjectId, entry.Direction, entry.Amount, entry.Summary);
            }

            // 插入 Voucher
            await conn.ExecuteAsync(
                _sql.Get("Accounting.Insert.Voucher.WithCompanyId"),
                new
                {
                    Id = voucherId, No = voucherNo,
                    Date = request.VoucherDate,
                    Type = "Manual", SrcId = (object)DBNull.Value,
                    CId = (object)DBNull.Value, CoId = companyId,
                    Period = period, CBy = Guid.Empty
                }, tx);

            // 插入分录
            foreach (var entry in voucher.Entries)
            {
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new
                    {
                        Id = Guid.NewGuid(), VId = voucherId,
                        SId = entry.AccountingSubjectId,
                        Dir = entry.Direction, Amt = entry.Amount,
                        Sum = entry.Summary ?? "", CBy = Guid.Empty
                    }, tx);
            }

            tx.Commit();
            return await Get(voucherId, ct);
        }
        catch (InvalidOperationException ex)
        {
            tx.Rollback();
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            tx.Rollback();
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>更新草稿凭证的分录（替换全部分录）</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntries(Guid id, [FromBody] UpdateVoucherRequest request, CancellationToken ct)
    {
        if (request.Entries == null || request.Entries.Count < 2)
            return BadRequest(new { message = "凭证至少需要两条分录" });

        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var entity = await conn.QuerySingleOrDefaultAsync<Voucher>(
                _sql.Get("Accounting.Select.Voucher.ById"), new { Id = id }, tx);
            if (entity == null) return NotFound();
            if (entity.Status.Code != "Draft")
                return BadRequest(new { message = "仅草稿状态凭证可修改分录" });

            // 用领域实体校验新分录
            var voucher = new Voucher(entity.VoucherNo,
                request.VoucherDate ?? DateOnly.Parse(entity.VoucherDate.ToString()),
                request.Description ?? entity.Description);
            foreach (var entry in request.Entries)
            {
                voucher.AddEntry(entry.AccountingSubjectId, entry.Direction, entry.Amount, entry.Summary);
            }

            // 删除旧分录
            await conn.ExecuteAsync(
                "DELETE FROM JournalEntries WHERE VoucherId = @Id", new { Id = id }, tx);

            // 插入新分录
            foreach (var entry in voucher.Entries)
            {
                await conn.ExecuteAsync(_sql.Get("Accounting.Insert.JournalEntry.Simple"),
                    new
                    {
                        Id = Guid.NewGuid(), VId = id,
                        SId = entry.AccountingSubjectId,
                        Dir = entry.Direction, Amt = entry.Amount,
                        Sum = entry.Summary ?? "", CBy = Guid.Empty
                    }, tx);
            }

            // 更新摘要
            if (!string.IsNullOrEmpty(request.Description))
            {
                await conn.ExecuteAsync(
                    "UPDATE Vouchers SET Description = @Desc WHERE Id = @Id",
                    new { Desc = request.Description, Id = id }, tx);
            }

            tx.Commit();
            return await Get(id, ct);
        }
        catch (InvalidOperationException ex)
        {
            tx.Rollback();
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            tx.Rollback();
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var companyId = _tenant.EffectiveCompanyId;
        var offset = (page - 1) * pageSize;

        using var conn = _db.CreateConnection();
        conn.Open();
        var items = await conn.QueryAsync<Voucher>(_sql.Get("Accounting.Select.Voucher.Paged"),
            new { CompanyId = companyId, StartDate = startDate, EndDate = endDate, Offset = offset, PageSize = pageSize });
        var total = await conn.QuerySingleAsync<int>(_sql.Get("Accounting.Select.Voucher.PagedCount"),
            new { CompanyId = companyId, StartDate = startDate, EndDate = endDate });
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();

        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("Accounting.Select.Voucher.ByIdWithEntries"), new { Id = id });

        var voucher = await multi.ReadSingleOrDefaultAsync<Voucher>();
        if (voucher == null) return NotFound();

        var entries = (await multi.ReadAsync<JournalEntry>()).ToList();
        voucher.LoadEntries(entries);

        return Ok(voucher);
    }

    /// <summary>校验会计期间可操作（期间为空则跳过校验）</summary>
    private async Task EnsurePeriodOperableAsync(string? period, string action, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(period)) return; // 未分配期间，不做校验

        var isOpen = await _periodService.IsPeriodOpenAsync(period, ct);
        if (!isOpen)
            throw new InvalidOperationException(
                $"会计期间「{period}」未开启或已结账，无法执行「{action}」操作");
    }

    /// <summary>校验期间未锁定（反过账/冲销时使用）</summary>
    private async Task EnsurePeriodNotLockedAsync(string? period, string action, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(period)) return;

        var all = await _periodService.GetAllAsync(ct);
        var match = all.FirstOrDefault(p => p.Period == period);
        if (match?.Status == "Locked")
            throw new InvalidOperationException(
                $"会计期间「{period}」已锁定，无法执行「{action}」操作");
    }

    [HttpPut("{id}/post")]
    public async Task<IActionResult> Post(Guid id, CancellationToken ct)
    {
        try
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                var entity = await conn.QuerySingleOrDefaultAsync<Voucher>(
                    _sql.Get("Accounting.Select.Voucher.ById"), new { Id = id });
                if (entity == null) return NotFound();

                // 校验：过账只能在开启的会计期间进行
                await EnsurePeriodOperableAsync(entity.Period, "过账", ct);

                var entries = (await conn.QueryAsync<JournalEntry>(
                    _sql.Get("Accounting.Select.Entry.ByVoucherId"), new { Id = id })).ToList();
                entity.LoadEntries(entries);

                entity.Post();

                var now = DateTime.UtcNow;
                var userId = _currentUser.UserId;
                await conn.ExecuteAsync(_sql.Get("Accounting.Update.Voucher.Post"),
                    new { Status = entity.Status.Code, UpdatedBy = userId, UpdatedAt = now, Id = id });
            }
            return await Get(id, ct);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/audit")]
    public async Task<IActionResult> Audit(Guid id, CancellationToken ct)
    {
        try
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                var entity = await conn.QuerySingleOrDefaultAsync<Voucher>(
                    _sql.Get("Accounting.Select.Voucher.ById"), new { Id = id });
                if (entity == null) return NotFound();

                // 校验：审核不能在已锁定的期间进行
                await EnsurePeriodNotLockedAsync(entity.Period, "审核", ct);

                var entries = (await conn.QueryAsync<JournalEntry>(
                    _sql.Get("Accounting.Select.Entry.ByVoucherId"), new { Id = id })).ToList();
                entity.LoadEntries(entries);

                entity.Audit();

                var now = DateTime.UtcNow;
                var userId = _currentUser.UserId;
                await conn.ExecuteAsync(_sql.Get("Accounting.Update.Voucher.Post"),
                    new { Status = entity.Status.Code, UpdatedBy = userId, UpdatedAt = now, Id = id });
            }
            return await Get(id, ct);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] object dto, CancellationToken ct)
    {
        try
        {
            using var conn = _db.CreateConnection();
            conn.Open();
            var entity = await conn.QuerySingleOrDefaultAsync<Voucher>(
                _sql.Get("Accounting.Select.Voucher.ById"), new { Id = id });
            if (entity == null) return NotFound();

            // 校验：冲销不能在已锁定的期间进行
            await EnsurePeriodNotLockedAsync(entity.Period, "冲销", ct);

            var entries = (await conn.QueryAsync<JournalEntry>(
                _sql.Get("Accounting.Select.Entry.ByVoucherId"), new { Id = id })).ToList();
            entity.LoadEntries(entries);

            if (entity.Status.Code != "Posted")
                return BadRequest(new { message = "只能冲销已过账凭证" });

            entity.Unpost();
            var now = DateTime.UtcNow;
            var userId = _currentUser.UserId;
            await conn.ExecuteAsync(_sql.Get("Accounting.Update.Voucher.Post"),
                new { Status = entity.Status.Code, UpdatedBy = userId, UpdatedAt = now, Id = id });

            return Ok(new { message = "已冲销（反过账）", id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// ===== DTOs =====

public class CreateVoucherRequest
{
    public DateOnly VoucherDate { get; set; }
    public string? Description { get; set; }
    public List<JournalEntryDto> Entries { get; set; } = new();
}

public class UpdateVoucherRequest
{
    public DateOnly? VoucherDate { get; set; }
    public string? Description { get; set; }
    public List<JournalEntryDto> Entries { get; set; } = new();
}

public class JournalEntryDto
{
    public Guid AccountingSubjectId { get; set; }
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Summary { get; set; }
}
