namespace RBS.Core.Entities.Billing;
using RBS.Core.Common;
using RBS.Core.Entities.Base;

/// <summary>
/// 催缴记录实体 — 每次催缴动作的详细记录，用于追踪催缴执行历史。
/// 继承自 <see cref="AuditableEntity"/> 并实现 <see cref="IHasCompany"/>。
/// </summary>
public class CollectionRecord : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 合同ID — 被催缴的合同标识，关联到具体的租赁合同。
    /// </summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// 阶段序号 — 催缴时所属的阶段编号，与 <see cref="CollectionStage.StageNo"/> 对应。
    /// </summary>
    public int StageNo { get; private set; }

    /// <summary>
    /// 催缴渠道 — 执行催缴的通信方式。支持以下值：
    /// "SMS"(短信)、"Email"(邮件)、"PhoneCall"(电话)、"Visit"(上门)、"System"(系统自动通知)。
    /// </summary>
    public string Channel { get; private set; } = string.Empty;

    /// <summary>
    /// 催缴内容 — 催缴消息的具体文本内容，包含逾期金额、截止日期等信息。
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// 催缴状态 — 当前催缴的执行状态。支持以下值：
    /// "Pending"(待发送)、"Sent"(已发送)、"Failed"(发送失败)。
    /// 新建记录时默认值为 "Pending"。
    /// </summary>
    public string Status { get; private set; } = string.Empty;

    /// <summary>
    /// 发送时间 — 催缴消息发送或执行的具体时间。
    /// 新建记录时默认设置为当前中国标准时间（<see cref="ChinaTime.Now"/>）。
    /// </summary>
    public DateTime SentAt { get; private set; }

    /// <summary>
    /// 操作人ID — 执行催缴操作的用户标识。为 null 表示系统自动触发。
    /// 当 Channel 为 "System" 时通常为 null。
    /// </summary>
    public Guid? OperatedBy { get; private set; }

    /// <summary>
    /// 所属公司ID — 实现 <see cref="IHasCompany"/>，标识此催缴记录归属的租户/公司。
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 私有无参构造函数 — 供 EF Core 等 ORM 框架使用，禁止外部直接调用。
    /// </summary>
    private CollectionRecord() { }

    /// <summary>
    /// 初始化催缴记录实体。
    /// 新建时状态自动设为 "Pending"(待发送)，发送时间设为当前中国标准时间。
    /// </summary>
    /// <param name="contractId">合同ID，标识被催缴的合同。不允许为空。</param>
    /// <param name="stageNo">阶段序号，标识催缴所处的阶段。</param>
    /// <param name="channel">催缴渠道，"SMS"(短信)、"Email"(邮件)、"PhoneCall"(电话)、"Visit"(上门)、"System"(系统)。</param>
    /// <param name="content">催缴内容，催缴消息的文本正文。</param>
    /// <param name="companyId">所属公司ID。</param>
    /// <exception cref="ArgumentException">当 <paramref name="contractId"/> 为 Guid.Empty 时抛出。</exception>
    /// <exception cref="ArgumentNullException">当 <paramref name="channel"/> 或 <paramref name="content"/> 为 null 时抛出。</exception>
    public CollectionRecord(Guid contractId, int stageNo, string channel, string content, Guid companyId)
    {
        if (contractId == Guid.Empty) throw new ArgumentException("合同ID不能为空", nameof(contractId));
        ContractId = contractId;
        StageNo = stageNo;
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Status = "Pending";
        SentAt = ChinaTime.Now;
        CompanyId = companyId;
    }
}
