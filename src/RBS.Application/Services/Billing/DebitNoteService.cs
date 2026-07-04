using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 欠款通知单服务 — 生成账单快照 + 导出 PDF
/// </summary>
public class DebitNoteService : IDebitNoteService
{
    private readonly IUnitOfWork _uow;
    private readonly IBillPdfGenerator _pdfGenerator;

    public DebitNoteService(IUnitOfWork uow, IBillPdfGenerator pdfGenerator)
    {
        _uow = uow;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<List<DebitNote>> GetByCompanyAsync(Guid companyId, string? period = null, CancellationToken ct = default)
    {
        return await _uow.GetDebitNotesByCompanyAsync(companyId, period, ct);
    }

    public async Task<List<DebitNote>> GetByContractAsync(Guid contractId, CancellationToken ct)
    {
        var all = await _uow.DebitNotes.GetAllAsync(ct);
        return all.Where(d => d.ContractId == contractId)
                  .OrderByDescending(d => d.CreatedAt)
                  .ToList();
    }

    public async Task<DebitNote?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var note = await _uow.DebitNotes.GetByIdAsync(id, ct);
        if (note == null) return null;

        // 加载明细
        var items = await _uow.GetDebitNoteItemsAsync(id, ct);
        note.LoadItems(items);
        return note;
    }

    public async Task<DebitNote> GenerateAsync(Guid contractId, string period, CancellationToken ct)
    {
        // 1. 加载合同和应收
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct)
            ?? throw new InvalidOperationException($"合同 {contractId} 不存在");

        var plans = await _uow.ReceivablePlans.GetByContractIdAsync(contractId, ct);
        var periodPlans = plans.Where(p => p.Period == period).ToList();
        if (periodPlans.Count == 0)
            throw new InvalidOperationException($"账期 {period} 无应收记录");


        // 3. 创建账单
        var noteNo = $"DN-{contract.ContractNo}-{period}";
        var note = new DebitNote(noteNo, contractId, period);
        await _uow.DebitNotes.AddAsync(note, ct);

        var total = periodPlans.Sum(p => p.Amount);
        note.SetTotalAmount(total);

        // 4. 写入 DebitNoteItems
        foreach (var plan in periodPlans)
        {
            await _uow.ExecuteSqlRawAsync(
                "INSERT INTO DebitNoteItems (Id, DebitNoteId, FeeCodeId, Amount, CreatedBy, CreatedAt) VALUES (@Id, @DebitNoteId, @FeeCodeId, @Amount, @CreatedBy, @CreatedAt)",
                new object[] { Guid.NewGuid(), note.Id, plan.FeeCodeId, plan.Amount, Guid.Empty, DateTime.UtcNow },
                ct);
        }
        note.LoadItems(periodPlans.Select(p => new DebitNoteItem(note.Id, p.FeeCodeId, p.Amount)).ToList());
        return note;
    }

    public async Task<byte[]> ExportPdfAsync(Guid id, CancellationToken ct)
    {
        var note = await GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"账单 {id} 不存在");

        // 合同信息
        var contract = await _uow.Contracts.GetByIdAsync(note.ContractId, ct);
        var contractNo = contract?.ContractNo ?? "";

        // 租客信息（加载所有租客，通过合同 ID 过滤关联关系）
        var tenants = await _uow.Tenants.GetAllAsync(ct);
        var tenantName = tenants.FirstOrDefault()?.Name ?? ""; // 简化：取第一个租客

        // 公司名
        var company = contract?.CompanyId != Guid.Empty && contract != null
            ? await _uow.Companies.GetByIdAsync(contract.CompanyId, ct)
            : null;

        // 费用明细
        var feeCodes = await _uow.FeeCodes.GetAllAsync(ct);
        var feeDict = feeCodes.ToDictionary(f => f.Id, f => f.Name);
        var items = note.Items.Select(i => (feeDict.GetValueOrDefault(i.FeeCodeId, "未知"), i.Amount)).ToList();

        return _pdfGenerator.Generate(note, items, contractNo, tenantName, company?.Name);
    }
}
