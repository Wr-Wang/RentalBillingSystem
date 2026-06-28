namespace RBS.Application.DTOs.Accounting;

public class AccountingSubjectDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public int Level { get; set; }
    public string Direction { get; set; } = "Debit";
    public bool IsLeaf { get; set; }
    public bool IsActive { get; set; }
    public List<AccountingSubjectDto> Children { get; set; } = new();
}

public class CreateAccountingSubjectRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public string Direction { get; set; } = "Debit";
}

public class UpdateAccountingSubjectRequest
{
    public string? Name { get; set; }
    public string? Direction { get; set; }
    public bool? IsActive { get; set; }
}
