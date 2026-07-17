namespace RBS.Application.DTOs.Accounting;

/// <summary>
/// 总账科目余额 DTO
/// </summary>
public class SubjectBalanceDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public string Direction { get; set; } = "Debit";
    public int Level { get; set; }
    public bool IsLeaf { get; set; }
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal YtdDebit { get; set; }
    public decimal YtdCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public List<SubjectBalanceDto> Children { get; set; } = new();
}

/// <summary>
/// 总账汇总行 DTO
/// </summary>
public class BalanceTotalsDto
{
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal YtdDebit { get; set; }
    public decimal YtdCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

/// <summary>
/// 总账余额查询结果 DTO
/// </summary>
public class GLBalanceResultDto
{
    public string Period { get; set; } = string.Empty;
    public List<SubjectBalanceDto> Items { get; set; } = new();
    public BalanceTotalsDto Totals { get; set; } = new();
}

/// <summary>
/// 总账明细分录 DTO
/// </summary>
public class GLEntryItemDto
{
    public string Date { get; set; } = string.Empty;
    public string ContractNo { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// 按合同号分组 DTO
/// </summary>
public class ContractGroupDto
{
    public string ContractNo { get; set; } = string.Empty;
    public List<GLEntryItemDto> Entries { get; set; } = new();
    public decimal SubtotalDebit { get; set; }
    public decimal SubtotalCredit { get; set; }
}

/// <summary>
/// 总账明细查询结果 DTO
/// </summary>
public class GLDetailResultDto
{
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public List<ContractGroupDto> GroupedByContract { get; set; } = new();
}
