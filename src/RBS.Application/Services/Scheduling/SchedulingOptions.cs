namespace RBS.Application.Services.Scheduling;

/// <summary>
/// 调度任务并行度配置
/// </summary>
public class SchedulingOptions
{
    public const string SectionName = "Scheduling";

    /// <summary>合同级并行度，默认 20</summary>
    public int ContractParallelism { get; set; } = 20;

    /// <summary>PDF 导出并行度，默认 16</summary>
    public int PdfParallelism { get; set; } = 16;

    /// <summary>调度引擎并行度（公司间），默认 CPU/2</summary>
    public int SchedulerParallelism { get; set; } = 0; // 0 = auto

    /// <summary>单个 Job 执行超时（分钟），默认 180 分钟（3 小时）</summary>
    public int JobTimeoutMinutes { get; set; } = 180;

    /// <summary>调度引擎轮询间隔（秒），默认 60 秒</summary>
    public int PollIntervalSeconds { get; set; } = 60;
}
