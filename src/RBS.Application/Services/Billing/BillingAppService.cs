using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;
using RBS.Core.Interfaces.UnitOfWork;
using ReceiptEntity = RBS.Core.Entities.Billing.Receipt;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 计费应用服务 — 编排收款、应收相关用例
/// </summary>
public class BillingAppService : IBillingService
{
    private readonly IUnitOfWork _uow;
    private readonly IAutoVoucherService _autoVoucher;

    public BillingAppService(IUnitOfWork uow, IAutoVoucherService autoVoucher)
    {
        _uow = uow;
        _autoVoucher = autoVoucher;
    }

    public async Task<List<ReceivablePlanDto>> GetPlansAsync(Guid contractId, CancellationToken ct = default)
    {
        var plans = await _uow.ReceivablePlans.GetByContractIdAsync(contractId, ct);
        return plans.Select(p => new ReceivablePlanDto
        {
            Id = p.Id,
            ContractId = p.ContractId,
            FeeCodeId = p.FeeCodeId,
            Period = p.Period,
            Amount = p.Amount,
            Received = p.Received,
            Balance = p.Amount - p.Received,
            DueDate = p.DueDate,
            Status = p.Status
        }).ToList();
    }

    public async Task<List<ReceiptDto>> GetReceiptsAsync(Guid companyId, CancellationToken ct = default)
    {
        var all = await _uow.Receipts.GetAllAsync(ct);
        var list = all.Where(r => r.CompanyId == companyId)
                      .OrderByDescending(r => r.CreatedAt)
                      .ToList();
        return list.Select(r => new ReceiptDto
        {
            Id = r.Id,
            ReceiptNo = r.ReceiptNo,
            ContractId = r.ContractId,
            Amount = r.Amount,
            Status = r.Status,
            ReceivedDate = r.ReceivedDate
        }).ToList();
    }

    public async Task<ReceiptDto> RegisterReceiptAsync(CreateReceiptRequest request, CancellationToken ct = default)
    {
        // 生成收款单号：RCP + yyyyMMdd + 4位随机
        var receiptNo = $"RCP{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";

        var entity = new ReceiptEntity(receiptNo, request.Amount, request.ReceivedDate, request.CompanyId);

        if (request.PaymentChannelId.HasValue)
        {
            // 通过反射设置 PaymentChannelId（无公开 setter 但有字段）
            typeof(ReceiptEntity).GetProperty(nameof(ReceiptEntity.PaymentChannelId))
                ?.SetValue(entity, request.PaymentChannelId.Value);
        }

        await _uow.Receipts.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);

        return new ReceiptDto
        {
            Id = entity.Id,
            ReceiptNo = entity.ReceiptNo,
            ContractId = entity.ContractId,
            Amount = entity.Amount,
            Status = entity.Status,
            ReceivedDate = entity.ReceivedDate
        };
    }

    public async Task ConfirmReceiptAsync(Guid receiptId, Guid userId, CancellationToken ct = default)
    {
        var entity = await _uow.Receipts.GetByIdAsync(receiptId, ct);
        if (entity == null)
            throw new InvalidOperationException("收款记录不存在");

        entity.Confirm(userId);
        await _uow.Receipts.UpdateAsync(entity, ct);
        await _uow.CommitAsync(ct);

        // 自动生成凭证
        await _autoVoucher.GenerateFromReceiptAsync(receiptId, ct);
    }

    public async Task RejectReceiptAsync(Guid receiptId, string reason, CancellationToken ct = default)
    {
        var entity = await _uow.Receipts.GetByIdAsync(receiptId, ct);
        if (entity == null)
            throw new InvalidOperationException("收款记录不存在");

        entity.Reject(reason);
        await _uow.Receipts.UpdateAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }
}
