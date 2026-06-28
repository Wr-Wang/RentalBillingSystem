namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Organization;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeId = null, CancellationToken ct = default);
}
