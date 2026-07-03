using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;

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
            "SELECT CreatedAt FROM Contracts WHERE Id=@Id", new { Id = contractId });
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

        // 收款事件
        var plans = await conn.QueryAsync(_sql.Get("Lease.Select.Contract.ReceivablePlans"),
            new { Id = contractId });
        foreach (var p in plans)
        {
            if ((string)p.Status == "Paid")
                events.Add(new TimelineEvent { Time = DateTime.Parse($"{p.Period}-01"), Type = "Payment",
                    Title = $"收款完成：{p.Period}", Description = $"¥{(decimal)p.Amount}" });
            if (p.DueDate != null && (string)p.Status != "Paid" && (string)p.Status != "Cancelled")
            {
                var dueDate = (DateOnly)p.DueDate;
                if (dueDate < DateOnly.FromDateTime(DateTime.UtcNow))
                    events.Add(new TimelineEvent { Time = dueDate.ToDateTime(TimeOnly.MinValue), Type = "Overdue",
                        Title = $"逾期：{p.Period}", Description = $"到期日 {dueDate}" });
            }
        }

        return events.OrderBy(e => e.Time).ToList();
    }
}

