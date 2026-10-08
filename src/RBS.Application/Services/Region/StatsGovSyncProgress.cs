namespace RBS.Application.Services.Region;

/// <summary>
/// 四级/五级（国家统计局）同步任务的进程内进度跟踪器（单例）
///
/// 同步在后台任务中执行（POST 立即返回），前端通过 GET 轮询快照显示实时进度。
/// 注意：保存在进程内存中，单实例部署有效；若为多副本部署则每副本各自维护，
/// 轮询需落在启动任务的同一副本上（本系统本地/演示部署为单副本，符合预期）。
/// </summary>
public sealed class StatsGovSyncProgress
{
    private readonly object _lock = new();

    private bool _running;
    private string _stage = "";
    private long _processed;
    private long _total;
    private string _unit = "rows"; // rows=条数，bytes=字节（下载阶段）
    private readonly List<string> _lines = new();
    private bool _succeeded;
    private string? _error;
    private int _synced;

    /// <summary>尝试占用同步任务（已在进行则返回 false）</summary>
    public bool TryBegin()
    {
        lock (_lock)
        {
            if (_running) return false;
            _running = true;
            _stage = "正在启动同步任务…";
            _processed = 0;
            _total = 0;
            _lines.Clear();
            _succeeded = false;
            _error = null;
            _synced = 0;
            return true;
        }
    }

    /// <summary>切换为不定进度阶段（如下载、清理），单位默认条数</summary>
    public void SetStage(string stage) => SetStage(stage, 0, "rows");

    /// <summary>切换为定量进度阶段（如逐条构建、SqlBulkCopy 写入），单位默认条数</summary>
    public void SetStage(string stage, long total) => SetStage(stage, total, "rows");

    /// <summary>切换为定量进度阶段（下载阶段传 total=文件总字节、unit="bytes"）</summary>
    public void SetStage(string stage, long total, string unit)
    {
        lock (_lock)
        {
            _stage = stage;
            _processed = 0;
            _total = total;
            _unit = unit;
        }
    }

    /// <summary>上报当前已处理条数（SqlRowsCopied/逐条循环回调，可能跨线程）</summary>
    public void Report(long processed, long total)
    {
        lock (_lock)
        {
            _processed = processed;
            _total = total;
        }
    }

    /// <summary>追加一条里程碑信息（最终展示在结果里）</summary>
    public void AddLine(string line)
    {
        lock (_lock)
        {
            _lines.Add(line);
        }
    }

    /// <summary>标记同步成功完成</summary>
    public void Succeed(int synced)
    {
        lock (_lock)
        {
            _running = false;
            _succeeded = true;
            _synced = synced;
            _stage = "同步完成";
        }
    }

    /// <summary>标记同步失败</summary>
    public void Fail(string error)
    {
        lock (_lock)
        {
            _running = false;
            _succeeded = false;
            _error = error;
            _stage = "同步失败";
        }
    }

    public StatsGovSyncProgressSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            return new StatsGovSyncProgressSnapshot
            {
                Running = _running,
                Stage = _stage,
                Processed = _processed,
                Total = _total,
                Unit = _unit,
                Lines = _lines.ToList(),
                Succeeded = _succeeded,
                Error = _error,
                Synced = _synced
            };
        }
    }
}

/// <summary>同步进度的不可变快照（JSON 输出 camelCase）</summary>
public sealed class StatsGovSyncProgressSnapshot
{
    public bool Running { get; set; }
    public string Stage { get; set; } = "";
    public long Processed { get; set; }
    public long Total { get; set; }
    public string Unit { get; set; } = "rows";
    public List<string> Lines { get; set; } = new();
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int Synced { get; set; }
}
