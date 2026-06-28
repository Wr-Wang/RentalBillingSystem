namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

public class PaymentChannel : AuditableEntity, IHasLandlord
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public Guid LandlordId { get; private set; }
    private PaymentChannel() { }
    public PaymentChannel(string name, string code, Guid landlordId) { Name = name; Code = code; LandlordId = landlordId; }
}
