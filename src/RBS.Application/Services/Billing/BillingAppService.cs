using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;
using RBS.Core.Interfaces.UnitOfWork;
using ReceiptEntity = RBS.Core.Entities.Billing.Receipt;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 计费应用服务实现 — 编排收款登记、确认、驳回等核心计费流程
/// 依赖 IAutoVoucherService 实现收款确认后的自动凭证生成
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

    /// <summary>
    /// 获取指定合同的应收计划列表，计算每项余额
    /// </summary>
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

    /// <summary>
    /// 获取指定公司的收款记录列表（按创建时间倒序）
    /// </summary>
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

    /// <summary>
    /// 登记收款记录 — 生成收款单号并持久化
    /// </summary>
    public async Task<ReceiptDto> RegisterReceiptAsync(CreateReceiptRequest request, CancellationToken ct = default)
    {
        var entity = ReceiptEntity.CreateNew(
            request.Amount, request.ReceivedDate, request.CompanyId, request.PaymentChannelId);

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

    /// <summary>
    /// 确认收款 — 调用实体 Confirm 方法后自动生成会计凭证
    /// </summary>
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

    /// <summary>
    /// 驳回收款记录 — 调用实体 Reject 方法
    /// </summary>
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
