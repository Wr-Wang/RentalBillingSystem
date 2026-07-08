namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 作业执行上下文 — 宿主服务与 Job 之间的通信通道
/// 宿主设置 TaskLogId，Job 读取后用于步骤日志记录
/// </summary>
public class JobExecutionContext
{
    public Guid TaskLogId { get; set; }
}
