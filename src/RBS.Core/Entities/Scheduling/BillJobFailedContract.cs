namespace RBS.Core.Entities.Scheduling;

/// <summary>
/// BillJob 失败合同记录 — 记录出账任务中哪些合同在哪一步因什么原因失败。
/// 支持重试标记，用于事后修复和重试。
/// 与 TaskLog 关联，不继承 AuditableEntity（日志型实体）。
/// </summary>
public class BillJobFailedContract
{
    /// <summary>主键，自增 BIGINT</summary>
    public long Id { get; private set; }

    /// <summary>关联的任务日志 ID</summary>
    public Guid TaskLogId { get; private set; }

    /// <summary>失败合同 ID</summary>
    public Guid ContractId { get; private set; }

    /// <summary>合同编号（方便查看，冗余）</summary>
    public string ContractNo { get; private set; } = string.Empty;

    /// <summary>失败步骤名称，如 "Step03_Journal"、"Step05_DebitNote"</summary>
    public string StepName { get; private set; } = string.Empty;

    /// <summary>错误消息</summary>
    public string ErrorMessage { get; private set; } = string.Empty;

    /// <summary>失败时间</summary>
    public DateTime FailedAt { get; private set; }

    /// <summary>是否已重试成功</summary>
    public bool IsRetried { get; private set; }

    /// <summary>重试时间</summary>
    public DateTime? RetriedAt { get; private set; }

    /// <summary>私有无参构造函数，供 Dapper 使用</summary>
    private BillJobFailedContract() { }

    /// <summary>
    /// 创建失败合同记录
    /// </summary>
    /// <param name="taskLogId">任务日志 ID</param>
    /// <param name="contractId">合同 ID</param>
    /// <param name="contractNo">合同编号</param>
    /// <param name="stepName">失败步骤</param>
    /// <param name="errorMessage">错误消息</param>
    public BillJobFailedContract(Guid taskLogId, Guid contractId, string contractNo,
        string stepName, string errorMessage)
    {
        TaskLogId = taskLogId;
        ContractId = contractId;
        ContractNo = contractNo ?? "";
        StepName = stepName;
        ErrorMessage = errorMessage;
        FailedAt = RBS.Core.Common.ChinaTime.Now;
        IsRetried = false;
    }

    /// <summary>
    /// 标记为重试成功
    /// </summary>
    public void MarkRetried()
    {
        IsRetried = true;
        RetriedAt = RBS.Core.Common.ChinaTime.Now;
    }
}
