namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 催缴阶段配置实体 — 按逾期天数范围定义催缴动作。
/// 用于设置阶梯式催缴策略，根据逾期天数自动或手动执行相应催缴操作。
/// 继承自 <see cref="AuditableEntity"/> 并实现 <see cref="IHasCompany"/>。
/// </summary>
public class CollectionStage : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 阶段序号 — 催缴阶段的顺序编号，数字越小优先级越高（如 1 表示第一轮催缴）。
    /// </summary>
    public int StageNo { get; private set; }

    /// <summary>
    /// 阶段名称 — 催缴阶段的可读名称（如"初次提醒"、"严重逾期"等）。
    /// </summary>
    public string StageName { get; private set; } = string.Empty;

    /// <summary>
    /// 逾期天数起始值（含）— 此阶段适用的最小逾期天数。
    /// 例如 OverdueDaysFrom = 1 表示逾期第 1 天起触发此阶段。
    /// </summary>
    public int OverdueDaysFrom { get; private set; }

    /// <summary>
    /// 逾期天数结束值（含）— 此阶段适用的最大逾期天数。
    /// 例如 OverdueDaysTo = 7 表示逾期第 7 天截止此阶段。
    /// </summary>
    public int OverdueDaysTo { get; private set; }

    /// <summary>
    /// 动作类型 — 催缴执行的动作方式。支持以下值：
    /// "SMS"（短信通知）、"Email"（邮件通知）、"PhoneCall"（电话催缴）、"Visit"（上门催缴）。
    /// </summary>
    public string ActionType { get; private set; } = string.Empty;

    /// <summary>
    /// 是否自动执行 — true 表示到达逾期天数范围时系统自动触发催缴；false 表示需人工手动执行。
    /// 默认值为 true。
    /// </summary>
    public bool IsAuto { get; private set; } = true;

    /// <summary>
    /// 所属公司ID — 实现 <see cref="IHasCompany"/>，标识此催缴阶段配置归属的租户/公司。
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 私有无参构造函数 — 供 EF Core 等 ORM 框架使用，禁止外部直接调用。
    /// </summary>
    private CollectionStage() { }

    /// <summary>
    /// 初始化催缴阶段配置实体。
    /// </summary>
    /// <param name="stageNo">阶段序号，数字越小优先级越高。</param>
    /// <param name="stageName">阶段名称，催缴阶段的可读描述。</param>
    /// <param name="overdueDaysFrom">逾期天数起始值（含），此阶段适用的最小逾期天数。</param>
    /// <param name="overdueDaysTo">逾期天数结束值（含），此阶段适用的最大逾期天数。</param>
    /// <param name="actionType">动作类型，"SMS"(短信)、"Email"(邮件)、"PhoneCall"(电话)、"Visit"(上门)。</param>
    /// <param name="companyId">所属公司ID。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="stageName"/> 或 <paramref name="actionType"/> 为 null 时抛出。</exception>
    public CollectionStage(int stageNo, string stageName, int overdueDaysFrom, int overdueDaysTo, string actionType, Guid companyId)
    {
        StageNo = stageNo;
        StageName = stageName ?? throw new ArgumentNullException(nameof(stageName));
        OverdueDaysFrom = overdueDaysFrom;
        OverdueDaysTo = overdueDaysTo;
        ActionType = actionType ?? throw new ArgumentNullException(nameof(actionType));
        CompanyId = companyId;
    }
}
