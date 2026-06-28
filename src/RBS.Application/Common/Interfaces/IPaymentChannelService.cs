using RBS.Application.DTOs.Billing;

namespace RBS.Application.Common.Interfaces;

public interface IPaymentChannelService
{
    Task<List<PaymentChannelDto>> GetListAsync(CancellationToken ct = default);
    Task<PaymentChannelDto> CreateAsync(CreatePaymentChannelRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdatePaymentChannelRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
