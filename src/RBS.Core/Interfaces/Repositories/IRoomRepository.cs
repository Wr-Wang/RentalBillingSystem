namespace RBS.Core.Interfaces.Repositories;
using RBS.Core.Entities.Property;

public interface IRoomRepository : IRepository<Room>
{
    Task<List<Room>> GetByBuildingIdAsync(Guid buildingId, CancellationToken ct = default);
    Task<Room?> GetByFullCodeAsync(string fullCode, CancellationToken ct = default);
    Task<bool> HasActiveContractAsync(Guid roomId, CancellationToken ct = default);
}
