namespace RBS.Application.Services.Billing;

using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;
using RBS.Core.Interfaces.UnitOfWork;

/// <summary>
/// 计费应用服务 — 编排收款、应收相关用例
/// </summary>
public class BillingAppService : IBillingService
{
    private readonly IUnitOfWork _uow;

    public BillingAppService(IUnitOfWork uow) => _uow = uow;

    public Task<List<ReceivablePlanDto>> GetPlansAsync(Guid contractId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<ReceiptDto>> GetReceiptsAsync(Guid landlordId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ReceiptDto> RegisterReceiptAsync(CreateReceiptRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task ConfirmReceiptAsync(Guid receiptId, Guid userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task RejectReceiptAsync(Guid receiptId, string reason, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
