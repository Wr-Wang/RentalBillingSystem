using RBS.Application.DTOs.Contract;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 合同管理应用服务
/// </summary>
public interface IContractService
{
    Task<List<ContractDto>> GetListAsync(Guid companyId, CancellationToken ct = default);
    Task<ContractDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ContractDto> CreateAsync(CreateContractRequest request, CancellationToken ct = default);
    Task ActivateAsync(Guid id, CancellationToken ct = default);
    Task TerminateAsync(Guid id, string reason, CancellationToken ct = default);
    Task SuspendAsync(Guid id, CancellationToken ct = default);
    Task ResumeAsync(Guid id, CancellationToken ct = default);
}
