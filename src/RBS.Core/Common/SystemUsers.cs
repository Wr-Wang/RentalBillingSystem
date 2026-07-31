namespace RBS.Core.Common;

/// <summary>
/// 系统内置用户标识
///
/// 职责：为系统自动生成（非人工操作）的记录提供稳定的 CreatedBy 标识，
/// 使其与 Guid.Empty（未赋值）区分，并可通过该 ID 识别"系统产生"的数据。
///
/// 使用场景：
/// - 调度任务（BillJob/SettleJob/CollectionJob/AutoRenewJob/TerminateJob 等）
///   创建 Journals/DebitNotes/GL Entries/TaskLogs 时作为 CreatedBy
/// - 统计局区域数据同步（SqlBulkCopy 写入 Regions）时作为 CreatedBy
/// </summary>
public static class SystemUsers
{
    /// <summary>
    /// 系统调度任务标识 — 由后台作业自动创建记录的 CreatedBy
    /// </summary>
    public static readonly Guid Scheduler = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// 系统数据同步标识 — 由数据同步服务创建记录的 CreatedBy
    /// </summary>
    public static readonly Guid DataSync = Guid.Parse("00000000-0000-0000-0000-000000000002");
}
