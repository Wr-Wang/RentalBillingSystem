namespace RBS.Core.Entities.Approval;
using RBS.Core.Common;

using RBS.Core.Entities.Base;

/// <summary>
/// 审批请求聚合根 — 支持 0~N 级审批流转
/// </summary>
public class ApprovalRequest : AggregateRoot, IHasCompany
{
    public Guid ApprovalTypeId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Guid TargetEntityId { get; private set; }
    public string TargetEntityType { get; private set; }
    public int CurrentLevel { get; private set; }
    public int MaxLevel { get; private set; }
    public string Status { get; private set; }
    public Guid CompanyId { get; private set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // ===== 审批跟踪 =====
    private readonly List<ApprovalRecord> _records = new();
    public IReadOnlyCollection<ApprovalRecord> Records => _records.AsReadOnly();

    /// <summary>是否无需审批（0级审批）</summary>
    public bool IsAutoApproved => MaxLevel <= 0;

    private ApprovalRequest() : base()
    {
        Title = string.Empty;
        TargetEntityType = string.Empty;
        Status = "Pending";
    }

    /// <summary>领域构造函数</summary>
    public ApprovalRequest(
        Guid approvalTypeId, string title, Guid targetEntityId,
        string targetEntityType, Guid companyId, int maxLevel = 1) : base()
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("审批标题不能为空");
        if (string.IsNullOrWhiteSpace(targetEntityType)) throw new ArgumentException("目标实体类型不能为空");

        ApprovalTypeId = approvalTypeId;
        Title = title;
        TargetEntityId = targetEntityId;
        TargetEntityType = targetEntityType;
        CompanyId = companyId;
        CurrentLevel = 1;
        MaxLevel = Math.Max(0, maxLevel);
        Status = "Draft";
    }

    // ===== 领域行为 =====

    /// <summary>提交审批</summary>
    public void Submit()
    {
        if (Status != "Draft")
            throw new InvalidOperationException("只有草稿状态的审批可以提交");

        if (IsAutoApproved)
        {
            // 0级审批：自动通过
            Status = "Approved";
            AddDomainEvent(new ApprovalCompletedEvent(Id, TargetEntityId, TargetEntityType, "Approved"));
        }
        else
        {
            Status = "Pending";
        }
    }

    /// <summary>记录审批操作</summary>
    public void AddRecord(Guid approverId, string action, string? comment)
    {
        var record = new ApprovalRecord(Id, CurrentLevel, approverId, action, comment);
        record.SetCreated(approverId, ChinaTime.Now);
        _records.Add(record);
    }

    /// <summary>推进到下一级</summary>
    public void AdvanceLevel()
    {
        if (Status != "Pending")
            throw new InvalidOperationException("审批状态不允许推进");
        if (CurrentLevel >= MaxLevel)
            throw new InvalidOperationException("已是最后一级审批");
        CurrentLevel++;
    }

    /// <summary>完成审批（终审）</summary>
    public void CompleteApproval(string result)
    {
        if (Status != "Pending")
            throw new InvalidOperationException("审批状态不允许完结");

        Status = result;
        AddDomainEvent(new ApprovalCompletedEvent(Id, TargetEntityId, TargetEntityType, result));
    }

    /// <summary>撤销审批请求</summary>
    public void Cancel(string? reason = null)
    {
        if (Status is "Approved" or "Rejected")
            throw new InvalidOperationException("已完结的审批不能撤销");
        Status = "Cancelled";
    }

    /// <summary>是否为终审</summary>
    public bool IsFinalLevel => CurrentLevel >= MaxLevel;

    /// <summary>获取最新一条审批记录</summary>
    public ApprovalRecord? LatestRecord => _records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
}
