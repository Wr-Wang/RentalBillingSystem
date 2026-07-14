namespace RBS.Core.Entities.SystemConfig;

/// <summary>
/// 站内通知实体 — 用于向用户发送系统内消息通知
/// 支持审批通知、续签提醒、催收通知、系统公告等多种分类。
/// 可通过 ReferenceType/ReferenceId 关联到具体的业务对象
/// </summary>
public class Notification
{
    /// <summary>
    /// 通知唯一标识
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 接收用户标识，通知的目标用户
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// 所属公司标识（可选），用于多租户场景下的通知隔离
    /// </summary>
    public Guid? CompanyId { get; private set; }

    /// <summary>
    /// 通知分类。
    /// Approval=审批通知, Renewal=续签通知, Collection=催收通知, System=系统公告
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// 通知标题，如 "合同续签提醒"、"审批通过通知"
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// 通知正文内容（可选），详细的通知描述信息
    /// </summary>
    public string? Content { get; private set; }

    /// <summary>
    /// 关联业务类型（可选），如 "Contract"、"ApprovalRequest"
    /// </summary>
    public string? ReferenceType { get; private set; }

    /// <summary>
    /// 关联业务标识（可选），与 ReferenceType 配合定位具体业务对象
    /// </summary>
    public Guid? ReferenceId { get; private set; }

    /// <summary>
    /// 是否已读。false=未读（前端标记提醒），true=已读
    /// 默认值为 false
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// 通知创建时间（北京时间）
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private Notification() { }

    /// <summary>
    /// 创建站内通知实例。创建后默认未读，自动记录创建时间（北京时间）
    /// </summary>
    /// <param name="userId">接收用户标识</param>
    /// <param name="category">通知分类（Approval/Renewal/Collection/System）</param>
    /// <param name="title">通知标题</param>
    /// <param name="content">通知正文（可选）</param>
    /// <param name="referenceType">关联业务类型（可选）</param>
    /// <param name="referenceId">关联业务标识（可选）</param>
    /// <param name="companyId">所属公司标识（可选）</param>
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
        CreatedAt = RBS.Core.Common.ChinaTime.Now;
    }

    /// <summary>将通知标记为已读</summary>
    public void MarkAsRead() => IsRead = true;
}
