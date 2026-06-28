namespace RBS.Application.Services.Contract;

using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Contract;
using RBS.Core.Interfaces.UnitOfWork;

/// <summary>
/// 合同应用服务 — 编排合同相关的用例
/// </summary>
public class ContractAppService : IContractService
{
    private readonly IUnitOfWork _uow;

    public ContractAppService(IUnitOfWork uow) => _uow = uow;

    public Task<List<ContractDto>> GetListAsync(Guid companyId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ContractDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ContractDto> CreateAsync(CreateContractRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task TerminateAsync(Guid id, string reason, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SuspendAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task ResumeAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
