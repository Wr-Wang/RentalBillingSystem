using Dapper;
using RBS.Core.Common;
using RBS.Core.Interfaces.Persistence;

namespace RBS.Application.Services.Contract;

/// <summary>合同时间线条目</summary>
public class TimelineEvent
{
    public DateTime Time { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}

/// <summary>合同时间线服务 — 聚合审批/续签/收款等事件</summary>
public interface IContractTimelineService
{
    Task<List<TimelineEvent>> GetTimelineAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>获取合同变更历史</summary>
    Task<IEnumerable<object>> GetChangesAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>插入合同变更历史记录</summary>
    Task InsertChangeHistoryAsync(Guid contractId, string changeType, string title, string detail,
        decimal? oldValue, decimal? newValue, string? effectiveDate, Guid? operatorId, string? operatorName = null, CancellationToken ct = default);
}

public class ContractTimelineService : IContractTimelineService
{
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public ContractTimelineService(IDbConnectionFactory db, ISqlLoader sql)
    {
        _db = db; _sql = sql;
    }

    public async Task<List<TimelineEvent>> GetTimelineAsync(Guid contractId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var events = new List<TimelineEvent>();

        // 合同创建事件（从 Contract 实体获取 CreatedAt）
        var created = await conn.QuerySingleOrDefaultAsync<DateTime?>(
            _sql.Get("Lease.Select.Contract.CreatedAt"), new { Id = contractId });
        if (created.HasValue)
            events.Add(new TimelineEvent { Time = created.Value, Type = "Created", Title = "合同创建" });

        // 审批事件
        var approvals = await conn.QueryAsync(_sql.Get("Lease.Select.Contract.ApprovalRequests"),
            new { Id = contractId });
        foreach (var a in approvals)
            events.Add(new TimelineEvent { Time = (DateTime)a.CreatedAt, Type = "Approval",
                Title = (string)a.Title, Description = (string?)a.Description });

        // 续签事件
        var renewals = await conn.QueryAsync(_sql.Get("Lease.Select.Contract.RenewalRequests"),
            new { Id = contractId });
        foreach (var r in renewals)
            events.Add(new TimelineEvent { Time = (DateTime)r.CreatedAt, Type = "Renewal",
                Title = $"续签（{(string)r.Status}）", Description = $"新租金 ¥{(decimal)r.NewRent}" });

        // 收款/逾期事件（基于 Journal）
        var journals = await conn.QueryAsync(_sql.Get("Billing.Select.Journal.ByContract"),
            new { CId = contractId });
        foreach (var j in journals)
        {
            events.Add(new TimelineEvent { Time = DateTime.Parse($"{j.Period}-01"), Type = "Payment",
                Title = $"应收：{j.Period}", Description = $"¥{(decimal)j.Amount}" });
            var dueDate = ((DateTime)j.DueDate).Date;
            if (dueDate < ChinaTime.Now.Date)
                events.Add(new TimelineEvent { Time = dueDate, Type = "Overdue",
                    Title = $"逾期：{j.Period}", Description = $"到期日 {dueDate:yyyy-MM-dd}" });
        }

        return events.OrderBy(e => e.Time).ToList();
    }

    public async Task<IEnumerable<object>> GetChangesAsync(Guid contractId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var items = await conn.QueryAsync(
            _sql.Get("Contract.Select.ChangeHistory.ByContract"), new { ContractId = contractId });
        return items;
    }

    public async Task InsertChangeHistoryAsync(Guid contractId, string changeType, string title, string detail,
        decimal? oldValue, decimal? newValue, string? effectiveDate, Guid? operatorId, string? operatorName = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        if (string.IsNullOrEmpty(operatorName) && operatorId.HasValue)
            try { operatorName = await conn.QuerySingleOrDefaultAsync<string>(
                _sql.Get("Contract.Select.User.DisplayNameById"), new { Id = operatorId }); } catch { }
        await conn.ExecuteAsync(_sql.Get("Contract.Insert.ChangeHistory.Default"),
            new { Id = Guid.NewGuid(), ContractId = contractId, ChangeType = changeType, Title = title,
                Detail = detail, OldValue = oldValue, NewValue = newValue, EffectiveDate = effectiveDate,
                OperatorId = operatorId, OperatorName = operatorName ?? "" });
    }
}

