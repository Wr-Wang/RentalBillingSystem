namespace RBS.Application.Services.Scheduling;

/// <summary>
/// 调度任务并行度配置 — 从 appsettings.json "Scheduling" 节读取
/// </summary>
/// <remarks>
/// 注入方式：builder.Services.Configure&lt;SchedulingOptions&gt;(builder.Configuration.GetSection("Scheduling"))
/// 所有属性都有默认值，未配置节或部分属性缺失时使用默认值
/// </remarks>
public class SchedulingOptions
{
    public const string SectionName = "Scheduling";

    /// <summary>
    /// 合同级并行度
    /// BillJob/SettleJob 内部使用 Parallel.ForEachAsync 并行处理合同的数量上限。
    /// 设为 1 相当于串行。值过大会撑爆数据库连接池（需配合 Max Pool Size 使用）。
    /// 默认值 1（串行，避免并发写入同一套表时的锁冲突）
    /// </summary>
    public int ContractParallelism { get; set; } = 1;

    /// <summary>
    /// PDF 导出并行度
    /// BillJob 批量生成账单 PDF 时，同时写入磁盘的文件数。
    /// 磁盘 IO 密集操作，建议不超过 CPU 核心数 x2。
    /// 默认值 16
    /// </summary>
    public int PdfParallelism { get; set; } = 16;

    /// <summary>
    /// 调度引擎公司间并行度
    /// SchedulingHostedService 使用 Parallel.ForEachAsync 同时调度的公司数。
    /// 0 表示自动（Environment.ProcessorCount / 2），正数表示固定值。
    /// 默认值 0（自动）
    /// </summary>
    public int SchedulerParallelism { get; set; } = 0;

    /// <summary>
    /// 单个 Job 最长执行时间（分钟）
    /// 超过此时间 Job 自动取消并标记失败，让下游任务继续执行。
    /// 10 万合同 + ContractParallelism=20 约需 2.8 小时，建议留余量。
    /// 默认值 180（3 小时）
    /// </summary>
    public int JobTimeoutMinutes { get; set; } = 180;

    /// <summary>
    /// 调度引擎轮询间隔（秒）
    /// 每隔多少秒检查一次 JobScheduleExecutions 表是否有到期排期。
    /// 过小增加数据库压力，过大导致任务触发延迟。
    /// 默认值 60
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 60;
}
