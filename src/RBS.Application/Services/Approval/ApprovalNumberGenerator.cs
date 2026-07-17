using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.Services.Approval;

/// <summary>
/// 审批申请编号生成器 — 规则：AP{yyyyMMdd}{6位当日序号}
/// 每日从 000001 开始自增，跨日重置。
/// </summary>
public class ApprovalNumberGenerator : IApprovalNumberGenerator
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public ApprovalNumberGenerator(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db;
        _sql = sql;
    }

    /// <inheritdoc />
    public async Task<string> GenerateRequestNo()
    {
        var now = ChinaTime.Now;
        var datePrefix = now.ToString("yyyyMMdd");
        var prefix = $"AP{datePrefix}";

        using var conn = _db.CreateConnection();
        conn.Open();
        var maxSeq = await conn.QuerySingleAsync<int>(
            _sql.Get("Approval.Select.Request.MaxSeqNoByDate"),
            new { Prefix = prefix });

        return $"{prefix}{(maxSeq + 1):D6}";
    }
}
