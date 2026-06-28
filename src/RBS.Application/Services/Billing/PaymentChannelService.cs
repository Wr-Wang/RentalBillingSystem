using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Services;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Billing;

public class PaymentChannelService : IPaymentChannelService
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantService _tenant;
    public PaymentChannelService(IUnitOfWork uow, ITenantService tenant) { _uow = uow; _tenant = tenant; }

    private Guid CompanyId => _tenant.EffectiveCompanyId ?? _tenant.HomeCompanyId ?? Guid.Empty;

    public async Task<List<PaymentChannelDto>> GetListAsync(CancellationToken ct = default)
        => (await _uow.PaymentChannels.GetAllAsync(ct)).Select(MapToDto).ToList();

    public async Task<PaymentChannelDto> CreateAsync(CreatePaymentChannelRequest request, CancellationToken ct = default)
    {
        var entity = new PaymentChannel(request.Name, request.Code, CompanyId);
        await _uow.PaymentChannels.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return MapToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdatePaymentChannelRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.PaymentChannels.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("支付通道不存在");
        if (request.Name != null) entity.Rename(request.Name);
        if (request.Code != null) entity.SetCode(request.Code);
        if (request.IsActive.HasValue) { if (request.IsActive.Value) entity.Activate(); else entity.Deactivate(); }
        await _uow.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.PaymentChannels.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("支付通道不存在");
        await _uow.PaymentChannels.DeleteAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }

    private static PaymentChannelDto MapToDto(PaymentChannel p) => new()
    { Id = p.Id, Name = p.Name, Code = p.Code, IsActive = p.IsActive };
}
