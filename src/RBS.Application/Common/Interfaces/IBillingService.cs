using RBS.Application.DTOs.Billing;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 计费应用服务
/// </summary>
public interface IBillingService
{
    Task<List<ReceivablePlanDto>> GetPlansAsync(Guid contractId, CancellationToken ct = default);
    Task<List<ReceiptDto>> GetReceiptsAsync(Guid landlordId, CancellationToken ct = default);
    Task<ReceiptDto> RegisterReceiptAsync(CreateReceiptRequest request, CancellationToken ct = default);
    Task ConfirmReceiptAsync(Guid receiptId, Guid userId, CancellationToken ct = default);
    Task RejectReceiptAsync(Guid receiptId, string reason, CancellationToken ct = default);
}
