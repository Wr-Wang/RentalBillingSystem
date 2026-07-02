namespace RBS.Core.Entities.SystemConfig;

/// <summary>
/// 站内通知实体
/// </summary>
public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public string Category { get; private set; } = string.Empty;  // Approval / Renewal / Collection / System
    public string Title { get; private set; } = string.Empty;
    public string? Content { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Notification() { }

    public Notification(Guid userId, string category, string title, string? content,
        string? referenceType = null, Guid? referenceId = null, Guid? companyId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CompanyId = companyId;
        Category = category;
        Title = title;
        Content = content;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead() => IsRead = true;
}
