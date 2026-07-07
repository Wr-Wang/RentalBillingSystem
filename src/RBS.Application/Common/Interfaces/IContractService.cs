using RBS.Application.DTOs.Contract;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 合同管理应用服务
/// </summary>
public interface IContractService
{
    Task<List<ContractDto>> GetListAsync(Guid companyId, CancellationToken ct = default);
    Task<List<ContractDto>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<PagedResult<ContractDto>> GetPagedListAsync(Guid companyId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null, CancellationToken ct = default);
    Task<ContractDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ContractDto> CreateAsync(CreateContractRequest request, CancellationToken ct = default);
    Task ActivateAsync(Guid id, CancellationToken ct = default);
    Task TerminateAsync(Guid id, string reason, CancellationToken ct = default);
    Task SuspendAsync(Guid id, CancellationToken ct = default);
    Task ResumeAsync(Guid id, CancellationToken ct = default);
}
