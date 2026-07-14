namespace RBS.Application.DTOs.Accounting;

/// <summary>
/// 会计凭证数据传输对象
/// </summary>
public class VoucherDto
{
    /// <summary>凭证 ID</summary>
    public Guid Id { get; set; }
    /// <summary>凭证编号</summary>
    public string VoucherNo { get; set; } = string.Empty;
    /// <summary>凭证日期</summary>
    public DateOnly VoucherDate { get; set; }
    /// <summary>凭证描述</summary>
    public string? Description { get; set; }
    /// <summary>凭证状态：Draft / Posted</summary>
    public string Status { get; set; } = "Draft";
    /// <summary>借方总金额</summary>
    public decimal TotalDebit { get; set; }
    /// <summary>贷方总金额</summary>
    public decimal TotalCredit { get; set; }
    /// <summary>分录列表</summary>
    public List<JournalEntryDto> Entries { get; set; } = new();
}

/// <summary>
/// 日记账分录数据传输对象
/// </summary>
public class JournalEntryDto
{
    /// <summary>会计科目编码</summary>
    public string SubjectCode { get; set; } = string.Empty;
    /// <summary>会计科目名称</summary>
    public string SubjectName { get; set; } = string.Empty;
    /// <summary>方向：Debit / Credit</summary>
    public string Direction { get; set; } = string.Empty;
    /// <summary>金额</summary>
    public decimal Amount { get; set; }
    /// <summary>摘要</summary>
    public string? Summary { get; set; }
}
