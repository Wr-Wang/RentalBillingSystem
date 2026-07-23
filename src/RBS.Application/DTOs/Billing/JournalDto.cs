namespace RBS.Application.DTOs.Billing;
public class JournalDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Guid FeeCodeId { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public string EntryType { get; set; } = "Normal";
    public bool GLPosted { get; set; }
    public string? BillMonth => Period;
}
