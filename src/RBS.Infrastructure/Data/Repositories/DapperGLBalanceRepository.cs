using Dapper;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Infrastructure.Data.Repositories;

/// <summary>
/// Dapper 总账余额查询仓储 — DDD Infrastructure Layer
/// 执行 SqlMaps.xml 中定义的 GL 查询 SQL
/// </summary>
public class DapperGLBalanceRepository : IGLBalanceRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public DapperGLBalanceRepository(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    public async Task<Dictionary<string, (decimal OpeningDebit, decimal OpeningCredit)>> GetOpeningBalancesAsync(
        Guid companyId, string period, string? contractNo, string? sourceType, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync<dynamic>(_sql.Get("Accounting.Select.GL.OpeningBalances"), new
        {
            CompanyId = companyId,
            Period = period,
            ContractNo = string.IsNullOrEmpty(contractNo) ? null : $"%{contractNo}%",
            SourceType = sourceType
        });
        var dict = new Dictionary<string, (decimal, decimal)>();
        foreach (var r in rows)
            dict[(string)r.SubjectCode] = ((decimal)(r.OpeningDebit ?? 0m), (decimal)(r.OpeningCredit ?? 0m));
        return dict;
    }

    public async Task<Dictionary<string, (decimal PeriodDebit, decimal PeriodCredit)>> GetPeriodActivityAsync(
        Guid companyId, string period, string? contractNo, string? sourceType, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync<dynamic>(_sql.Get("Accounting.Select.GL.PeriodActivity"), new
        {
            CompanyId = companyId,
            Period = period,
            ContractNo = string.IsNullOrEmpty(contractNo) ? null : $"%{contractNo}%",
            SourceType = sourceType
        });
        var dict = new Dictionary<string, (decimal, decimal)>();
        foreach (var r in rows)
            dict[(string)r.SubjectCode] = ((decimal)(r.PeriodDebit ?? 0m), (decimal)(r.PeriodCredit ?? 0m));
        return dict;
    }

    public async Task<Dictionary<string, (decimal YtdDebit, decimal YtdCredit)>> GetYtdActivityAsync(
        Guid companyId, string period, string yearStart, string? contractNo, string? sourceType, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync<dynamic>(_sql.Get("Accounting.Select.GL.YtdActivity"), new
        {
            CompanyId = companyId,
            Period = period,
            YearStart = yearStart,
            ContractNo = string.IsNullOrEmpty(contractNo) ? null : $"%{contractNo}%",
            SourceType = sourceType
        });
        var dict = new Dictionary<string, (decimal, decimal)>();
        foreach (var r in rows)
            dict[(string)r.SubjectCode] = ((decimal)(r.YtdDebit ?? 0m), (decimal)(r.YtdCredit ?? 0m));
        return dict;
    }

    public async Task<List<GLEntryRow>> GetDetailAsync(
        Guid companyId, string period, string subjectCode, string? contractNo, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync<GLEntryRow>(_sql.Get("Accounting.Select.GL.DetailBySubject"), new
        {
            CompanyId = companyId,
            Period = period,
            SubjectCode = subjectCode,
            ContractNo = string.IsNullOrEmpty(contractNo) ? null : $"%{contractNo}%"
        });
        return rows.ToList();
    }

    public async Task<List<GLSubjectRow>> GetSubjectsAsync(Guid companyId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        return (await conn.QueryAsync<GLSubjectRow>(
            _sql.Get("Accounting.Select.GL.Subjects"), new { CompanyId = companyId })).ToList();
    }

    public async Task<string> GetSubjectNameAsync(Guid companyId, string code, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        return await conn.QueryFirstOrDefaultAsync<string>(
            _sql.Get("Accounting.Select.GL.SubjectName"), new { Code = code, CompanyId = companyId }) ?? code;
    }
}
