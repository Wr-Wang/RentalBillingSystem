namespace RBS.Application.DTOs.Approval;

public class ApprovalTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Guid CompanyId { get; set; }
}

public class CreateApprovalTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateApprovalTypeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class ApprovalLevelConfigDto
{
    public Guid Id { get; set; }
    public Guid ApprovalTypeId { get; set; }
    public int Level { get; set; }
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}

public class CreateApprovalLevelRequest
{
    public Guid ApprovalTypeId { get; set; }
    public int Level { get; set; }
    public Guid RoleId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}

public class UpdateApprovalLevelRequest
{
    public int? Level { get; set; }
    public Guid? RoleId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}
