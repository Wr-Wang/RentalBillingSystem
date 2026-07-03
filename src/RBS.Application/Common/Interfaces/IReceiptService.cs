namespace RBS.Application.Common.Interfaces;

/// <summary>收款应用服务 — 收款编排操作</summary>
public interface IReceiptService
{
    Task<object> BatchConfirmAsync(List<Guid> ids, CancellationToken ct);
    Task<object> ReverseAsync(Guid id, CancellationToken ct);
}
