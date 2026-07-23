using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.UnitOfWork;
using RBS.Core.Entities.Billing;
using ReceiptEntity = RBS.Core.Entities.Billing.Receipt;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 计费应用服务实现 — 编排收款登记、确认、驳回等核心计费流程
/// 收款确认后自动更新总账（GL）
/// </summary>
public class BillingAppService : IBillingService
{
    private readonly IUnitOfWork _uow;
    private readonly IDbConnectionFactory _db;
    private readonly ISqlLoader _sql;

    public BillingAppService(IUnitOfWork uow, IDbConnectionFactory db, ISqlLoader sql)
    {
        _uow = uow;
        _db = db;
        _sql = sql;
    }

    /// <summary>
    /// 获取指定合同的 Journal 列表
    /// </summary>
    public async Task<List<JournalDto>> GetPlansAsync(Guid contractId, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var rows = await conn.QueryAsync(_sql.Get("Billing.Select.Journal.ByContract"),
            new { CId = contractId });
        return rows.Select(p => new JournalDto
        {
            Id = (Guid)p.Id,
            ContractId = (Guid)p.ContractId,
            FeeCodeId = (Guid)p.FeeCodeId,
            Period = (string)p.Period,
            Amount = (decimal)p.Amount,
            DueDate = DateOnly.FromDateTime((DateTime)p.DueDate),
            EntryType = (string)p.EntryType,
            GLPosted = (bool)p.GLPosted,
            BillMonth = (string?)(p.BillMonth ?? null)
        }).ToList();
    }

    public async Task<List<ReceiptDto>> GetReceiptsAsync(Guid companyId, CancellationToken ct = default)
    {
        var all = await _uow.Receipts.GetAllAsync(ct);
        var list = all.Where(r => r.CompanyId == companyId)
                      .OrderByDescending(r => r.CreatedAt).ToList();
        return list.Select(r => new ReceiptDto
        {
            Id = r.Id, ReceiptNo = r.ReceiptNo, ContractId = r.ContractId,
            Amount = r.Amount, Status = r.Status, ReceivedDate = r.ReceivedDate
        }).ToList();
    }

    public async Task<ReceiptDto> RegisterReceiptAsync(CreateReceiptRequest request, CancellationToken ct = default)
    {
        var entity = ReceiptEntity.CreateNew(
            request.Amount, request.ReceivedDate, request.CompanyId, request.PaymentChannelId);
        await _uow.Receipts.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);
        return new ReceiptDto
        {
            Id = entity.Id, ReceiptNo = entity.ReceiptNo, ContractId = entity.ContractId,
            Amount = entity.Amount, Status = entity.Status, ReceivedDate = entity.ReceivedDate
        };
    }

    /// <summary>确认收款 — 更新 GL（不再创建 Voucher）</summary>
    public async Task ConfirmReceiptAsync(Guid receiptId, Guid userId, CancellationToken ct = default)
    {
        var entity = await _uow.Receipts.GetByIdAsync(receiptId, ct);
        if (entity == null) throw new InvalidOperationException("收款记录不存在");

        entity.Confirm(userId);
        await _uow.Receipts.UpdateAsync(entity, ct);
        await _uow.CommitAsync(ct);

        // GL 在期间结账时统一生成，此处不再实时更新
    }

    public async Task RejectReceiptAsync(Guid receiptId, string reason, CancellationToken ct = default)
    {
        var entity = await _uow.Receipts.GetByIdAsync(receiptId, ct);
        if (entity == null) throw new InvalidOperationException("收款记录不存在");
        entity.Reject(reason);
        await _uow.Receipts.UpdateAsync(entity, ct);
        await _uow.CommitAsync(ct);
    }
}
