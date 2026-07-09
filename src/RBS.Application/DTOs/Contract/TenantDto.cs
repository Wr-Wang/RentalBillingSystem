namespace RBS.Application.DTOs.Contract;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IdCard { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    // 扩充字段
    public string? Wechat { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Address { get; set; }
    public string? Remark { get; set; }
    // 关联信息
    public int ContractCount { get; set; }
    public string? CurrentContractNo { get; set; }
}

public class ContractTenantInfoDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantPhone { get; set; }
    public string? IdCard { get; set; }
    public string? Email { get; set; }
    public string? Wechat { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdCard { get; set; }
    public string? Email { get; set; }
    public string? Wechat { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Address { get; set; }
    public string? Remark { get; set; }
    public Guid CompanyId { get; set; }
}

public class UpdateTenantRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? IdCard { get; set; }
    public string? Email { get; set; }
    public string? Wechat { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Address { get; set; }
    public string? Remark { get; set; }
}

public class AddContractTenantRequest
{
    /// <summary>已有租客ID（与 Name 二选一）</summary>
    public Guid? TenantId { get; set; }
    /// <summary>新建租客姓名（与 TenantId 二选一）</summary>
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? IdCard { get; set; }
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
}

public class RemoveContractTenantRequest
{
    public string Reason { get; set; } = string.Empty;
}
