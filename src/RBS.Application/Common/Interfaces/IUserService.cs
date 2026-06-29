using RBS.Application.DTOs.Organization;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 用户管理应用服务
/// </summary>
public interface IUserService
{
    Task<List<UserDto>> GetListAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SetDefaultCompanyAsync(Guid userId, Guid? companyId, CancellationToken ct = default);
}
