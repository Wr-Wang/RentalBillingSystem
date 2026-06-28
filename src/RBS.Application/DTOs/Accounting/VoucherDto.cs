namespace RBS.Application.DTOs.Accounting;

public class VoucherDto
{
    public Guid Id { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateOnly VoucherDate { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<JournalEntryDto> Entries { get; set; } = new();
}

public class JournalEntryDto
{
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Summary { get; set; }
}
