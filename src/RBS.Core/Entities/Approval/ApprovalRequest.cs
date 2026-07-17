namespace RBS.Core.Entities.Approval;
using RBS.Core.Common;

using RBS.Core.Entities.Base;

/// <summary>
/// 审批请求聚合根 — 支持 0~N 级审批流转
/// 负责管理审批全生命周期，包括提交、逐级审批、驳回、撤销等操作。
/// 通过 ApprovalLevelConfig 配置确定审批级数，支持金额区间路由。
/// 状态流转: Draft（草稿）→ Pending（审批中）→ Approved（通过）/ Rejected（驳回）/ Cancelled（撤销）
/// </summary>
public class ApprovalRequest : AggregateRoot, IHasCompany
{
    /// <summary>
    /// 审批类型标识
    /// 关联 ApprovalType，确定审批类别（如合同终止审批、调租审批等）
    /// </summary>
    public Guid ApprovalTypeId { get; private set; }

    /// <summary>
    /// 审批标题
    /// 简要描述审批事项，如"合同 XXXX 提前终止审批"
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// 审批描述/备注
    /// 补充说明审批事项的详细信息
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 目标业务实体标识
    /// 审批所针对的具体业务对象 ID（如合同 ID、收款单 ID 等）
    /// </summary>
    public Guid TargetEntityId { get; private set; }

    /// <summary>
    /// 目标业务实体类型名称
    /// 标识业务对象的类型（如 "Contract"、"Receipt" 等），与 TargetEntityId 共同定位业务记录
    /// </summary>
    public string TargetEntityType { get; private set; }

    /// <summary>
    /// 当前审批级别序号（从 1 开始）
    /// 标识审批流转到第几级，初始为 1
    /// </summary>
    public int CurrentLevel { get; private set; }

    /// <summary>
    /// 最大审批级别数
    /// 配置的最高审批级数，0 表示无需审批（自动通过）
    /// </summary>
    public int MaxLevel { get; private set; }

    /// <summary>
    /// 审批状态
    /// Draft（草稿）/ Pending（审批中）/ Approved（通过）/ Rejected（驳回）/ Cancelled（撤销）
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// 所属公司标识
    /// 审批请求按公司隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 乐观并发控制行版本号
    /// </summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    /// <summary>
    /// 关联合同标识（统一并发控制用）
    /// 用于在审批操作期间锁定关联合同，防止并发操作导致数据不一致
    /// </summary>
    public Guid? ContractId { get; private set; }

    /// <summary>
    /// 审批完成时间（终审通过/驳回时写入，仅一次）
    /// 记录审批流程最终完成的时刻
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// 申请编号（自动生成，规则：APR{yyyyMMddHHmmssfff}{XX}）
    /// </summary>
    public string? RequestNo { get; private set; }

    // ===== 审批跟踪 =====
    private readonly List<ApprovalRecord> _records = new();

    /// <summary>
    /// 审批操作记录集合（只读）
    /// 记录每一级审批的具体操作内容，按时间排序可查看完整审批轨迹
    /// </summary>
    public IReadOnlyCollection<ApprovalRecord> Records => _records.AsReadOnly();

    /// <summary>
    /// 是否无需审批（0级审批）
    /// 当 MaxLevel <= 0 时自动通过，无需人工介入
    /// </summary>
    public bool IsAutoApproved => MaxLevel <= 0;

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ApprovalRequest() : base()
    {
        Title = string.Empty;
        TargetEntityType = string.Empty;
        Status = "Pending";
    }

    /// <summary>
    /// 创建审批请求实例，初始状态为 Draft（草稿）
    /// </summary>
    /// <param name="approvalTypeId">审批类型标识</param>
    /// <param name="title">审批标题</param>
    /// <param name="targetEntityId">目标业务实体标识</param>
    /// <param name="targetEntityType">目标业务实体类型名称</param>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="maxLevel">最大审批级数，默认为 1，0 表示自动通过</param>
    /// <exception cref="ArgumentException">当审批标题或目标实体类型为空时抛出</exception>
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

    /// <summary>
    /// 提交审批
    /// 将草稿状态转为审批中（Pending）或自动通过（Approved）。
    /// 若为 0 级审批（MaxLevel <= 0）则直接自动通过并触发审批完成事件。
    /// </summary>
    /// <exception cref="InvalidOperationException">当审批状态不是 Draft 时抛出</exception>
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

    /// <summary>
    /// 设置关联合同 ID（用于统一并发控制）
    /// 在审批操作期间锁定关联合同，防止并发操作导致数据不一致
    /// </summary>
    /// <param name="contractId">合同标识</param>
    public void SetContractId(Guid contractId) => ContractId = contractId;

    /// <summary>
    /// 设置申请编号（由生成器在 SubmitAsync 中创建）
    /// </summary>
    /// <param name="requestNo">申请编号</param>
    public void SetRequestNo(string requestNo) => RequestNo = requestNo;

    /// <summary>
    /// 记录审批操作
    /// 将审批人的操作（通过/驳回）追加到审批记录列表中
    /// </summary>
    /// <param name="approverId">审批人标识</param>
    /// <param name="action">审批动作（Approved = 通过，Rejected = 驳回）</param>
    /// <param name="comment">审批意见（可选）</param>
    public void AddRecord(Guid approverId, string action, string? comment)
    {
        var record = new ApprovalRecord(Id, CurrentLevel, approverId, action, comment);
        record.SetCreated(approverId, ChinaTime.Now);
        _records.Add(record);
    }

    /// <summary>
    /// 推进到下一级审批
    /// 当前级审批通过后调用，将 CurrentLevel 增加 1
    /// </summary>
    /// <exception cref="InvalidOperationException">当审批状态不是 Pending 或已是最后一级时抛出</exception>
    public void AdvanceLevel()
    {
        if (Status != "Pending")
            throw new InvalidOperationException("审批状态不允许推进");
        if (CurrentLevel >= MaxLevel)
            throw new InvalidOperationException("已是最后一级审批");
        CurrentLevel++;
    }

    /// <summary>
    /// 完成审批（终审）
    /// 终审通过或驳回后调用，设置最终状态并触发审批完成领域事件
    /// </summary>
    /// <param name="result">审批结果（Approved = 通过，Rejected = 驳回）</param>
    /// <exception cref="InvalidOperationException">当审批状态不是 Pending 时抛出</exception>
    public void CompleteApproval(string result)
    {
        if (Status != "Pending")
            throw new InvalidOperationException("审批状态不允许完结");

        Status = result;
        AddDomainEvent(new ApprovalCompletedEvent(Id, TargetEntityId, TargetEntityType, result));
    }

    /// <summary>
    /// 撤销审批请求
    /// 将审批请求置为 Cancelled 状态，已完结（通过/驳回）的审批不可撤销
    /// </summary>
    /// <param name="reason">撤销原因（可选）</param>
    /// <exception cref="InvalidOperationException">当审批已完结（Approved 或 Rejected）时抛出</exception>
    public void Cancel(string? reason = null)
    {
        if (Status is "Approved" or "Rejected")
            throw new InvalidOperationException("已完结的审批不能撤销");
        Status = "Cancelled";
    }

    /// <summary>
    /// 是否为终审级别
    /// 当前审批级别已达到或超过最大级别数，表示本次审批操作即为终审
    /// </summary>
    public bool IsFinalLevel => CurrentLevel >= MaxLevel;

    /// <summary>
    /// 获取最新一条审批记录
    /// 按创建时间降序排列，取最近的一条审批操作记录
    /// </summary>
    public ApprovalRecord? LatestRecord => _records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
}
