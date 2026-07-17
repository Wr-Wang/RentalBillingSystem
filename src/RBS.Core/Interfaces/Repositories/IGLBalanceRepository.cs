namespace RBS.Core.Interfaces.Repositories;

/// <summary>
/// 总账余额查询仓储 — DDD 读模型仓储
/// 按期间查询 GeneralLedgerEntries 的科目级汇总数据
/// </summary>
public interface IGLBalanceRepository
{
    /// <summary>
    /// 查询期初余额（期间之前的所有分录汇总）
    /// </summary>
    Task<Dictionary<string, (decimal OpeningDebit, decimal OpeningCredit)>> GetOpeningBalancesAsync(
        Guid companyId, string period, string? contractNo, string? sourceType, CancellationToken ct);

    /// <summary>
    /// 查询本期发生额
    /// </summary>
    Task<Dictionary<string, (decimal PeriodDebit, decimal PeriodCredit)>> GetPeriodActivityAsync(
        Guid companyId, string period, string? contractNo, string? sourceType, CancellationToken ct);

    /// <summary>
    /// 查询年初至今累计发生额
    /// </summary>
    Task<Dictionary<string, (decimal YtdDebit, decimal YtdCredit)>> GetYtdActivityAsync(
        Guid companyId, string period, string yearStart, string? contractNo, string? sourceType, CancellationToken ct);

    /// <summary>
    /// 查询科目明细分录（按合同号排序）
    /// </summary>
    Task<List<GLEntryRow>> GetDetailAsync(
        Guid companyId, string period, string subjectCode, string? contractNo, CancellationToken ct);

    /// <summary>
    /// 查询公司所有活跃科目
    /// </summary>
    Task<List<GLSubjectRow>> GetSubjectsAsync(Guid companyId, CancellationToken ct);

    /// <summary>
    /// 查询科目名称
    /// </summary>
    Task<string> GetSubjectNameAsync(Guid companyId, string code, CancellationToken ct);
}

/// <summary>
/// 科目行（跨层共享）
/// </summary>
public class GLSubjectRow
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ParentCode { get; set; }
    public string Direction { get; set; } = "Debit";
    public int Level { get; set; }
    public bool IsLeaf { get; set; }
}

/// <summary>
/// 总表明细分录行（跨层共享）
/// </summary>
public class GLEntryRow
{
    public DateTime? Date { get; set; }
    public string? ContractNo { get; set; }
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string? Description { get; set; }
    public string? Direction { get; set; }
    public decimal Amount { get; set; }
}
