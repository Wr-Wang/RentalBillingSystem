using System.Data;
using Dapper;
using RBS.Application.Common.Interfaces;
using RBS.Core.Entities.Billing;
using RBS.Core.Interfaces.Persistence;
using RBS.Core.Interfaces.Services;
using RBS.Core.Common;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Billing;

/// <summary>
/// 缴费通知单服务 — 生成账单快照 + 导出 PDF
/// </summary>
public class DebitNoteService : IDebitNoteService
{
    /// <summary>
    /// 生成账单编号，格式：BN{yyyyMMdd}{8位序号}
    /// </summary>
    /// <param name="conn">已打开的数据库连接</param>
    /// <param name="tx">可选事务</param>
    /// <returns>账单编号</returns>
    public static async Task<string> GenerateBillNoAsync(IDbConnection conn, IDbTransaction? tx = null)
    {
        var today = ChinaTime.Now;
        var prefix = $"BN{today:yyyyMMdd}";
        var maxBillNo = await conn.QuerySingleOrDefaultAsync<string>(
            "SELECT MAX(BillNo) FROM DebitNotes WHERE BillNo LIKE @Prefix + '%'",
            new { Prefix = prefix }, tx);
        var seq = 1;
        if (!string.IsNullOrEmpty(maxBillNo) && maxBillNo.Length > prefix.Length)
            seq = int.Parse(maxBillNo[prefix.Length..]) + 1;
        return $"{prefix}{seq:D8}";
    }
    private readonly IUnitOfWork _uow;
    private readonly IBillPdfGenerator _pdfGenerator;
    private readonly ISqlLoader _sql;
    private readonly IDbConnectionFactory _db;
    private readonly ICurrentUserService _currentUser;

    public DebitNoteService(IUnitOfWork uow, IBillPdfGenerator pdfGenerator, ISqlLoader sql,
        IDbConnectionFactory db, ICurrentUserService currentUser)
    {
        _uow = uow;
        _pdfGenerator = pdfGenerator;
        _sql = sql;
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<object>> GetByCompanyAsync(Guid companyId, string? period = null,
        Guid? contractId = null, string? keyword = null, string? status = null, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var kw = !string.IsNullOrWhiteSpace(keyword) ? $"%{keyword}%" : null;
        int? periodYear = null, periodMonth = null;
        if (!string.IsNullOrEmpty(period))
        {
            var parts = period.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0], out var y) && int.TryParse(parts[1], out var m))
            {
                periodYear = y;
                periodMonth = m;
            }
        }
        return (await conn.QueryAsync(
            _sql.Get("Billing.Select.DebitNote.ByCompany"),
            new { CompanyId = companyId, PeriodYear = periodYear, PeriodMonth = periodMonth,
                ContractId = contractId, Keyword = kw, Status = status })).Cast<object>().ToList();
    }

    public async Task<List<object>> GetByContractAsync(Guid contractId, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        return (await conn.QueryAsync(
            _sql.Get("Billing.Select.DebitNote.ByCompany"),
            new { CompanyId = (Guid?)null, PeriodYear = (int?)null, PeriodMonth = (int?)null,
                ContractId = contractId, Keyword = (string?)null, Status = (string?)null })).Cast<object>().ToList();
    }

    public async Task<dynamic?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("DebitNote.Select.DebitNote.DetailById") + ";" +
            _sql.Get("Billing.Select.DebitNoteItem.ByDebitNoteId"),
            new { Id = id });
        var row = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (row == null) return null;
        var items = (await multi.ReadAsync<dynamic>()).ToList();

        // 上月结余和当期收款
        int year = (int)row.PeriodYear;
        int month = (int)row.PeriodMonth;
        var contractId = (Guid)row.ContractId;

        var prevBalance = await conn.QuerySingleOrDefaultAsync<decimal>(
            _sql.Get("Billing.Select.Journal.PreviousBalance"),
            new { ContractId = contractId, Year = year, Month = month });

        var receipts = (await conn.QueryAsync(
            _sql.Get("Billing.Select.Receipt.ByContractAndPeriod"),
            new { ContractId = contractId, Year = year, Month = month })).ToList();

        // 合并为 ExpandoObject
        var result = new System.Dynamic.ExpandoObject() as IDictionary<string, object?>;
        foreach (var prop in (IDictionary<string, object?>)row)
            result[prop.Key] = prop.Value;
        result["Items"] = items;
        result["PreviousBalance"] = prevBalance;
        result["Receipts"] = receipts;
        return result;
    }

    public async Task<DebitNote> GenerateAsync(Guid contractId, string period, CancellationToken ct)
    {
        // 1. 加载合同和应收
        var contract = await _uow.Contracts.GetByIdAsync(contractId, ct)
            ?? throw new InvalidOperationException($"合同 {contractId} 不存在");

        var allJournals = await _uow.Journals.GetAllAsync(ct);
        var periodJournals = allJournals.Where(j => j.ContractId == contractId && j.Period == period).ToList();
        if (periodJournals.Count == 0)
            throw new InvalidOperationException($"账期 {period} 无应收记录");

        // 2. 解析账期
        var periodParts = period.Split('-');
        var periodYear = int.Parse(periodParts[0]);
        var periodMonth = int.Parse(periodParts[1]);

        // 3. 创建账单（含快照字段）
        var noteId = Guid.NewGuid();
        using var conn = _db.CreateConnection(); conn.Open();
        var billNo = await GenerateBillNoAsync(conn);
        var total = periodJournals.Sum(p => Math.Abs(p.Amount));
        var lastDay = DateTime.DaysInMonth(periodYear, periodMonth);
        var dueDay = contract.EndDate != null ? Math.Min(contract.EndDate.Value.Day, lastDay) : lastDay;
        var dueDate = new DateTime(periodYear, periodMonth, dueDay);

        // 收集快照数据（出账时定格，后续变更不影响已生成的账单）
        var contractNo = contract.ContractNo ?? "";
        var tenantName = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.Tenant.PrimaryNameByContract"), new { Id = contractId });
        var buildingAddress = await conn.QuerySingleOrDefaultAsync<string>(
            _sql.Get("Lease.Select.HousingUnit.BuildingAddressByContract"), new { Id = contractId });
        var companyRow = await conn.QuerySingleOrDefaultAsync<dynamic>(
            _sql.Get("Organization.Select.Company.ById"), new { Id = contract.CompanyId });
        var companyName = companyRow?.Name as string ?? "";
        var previousBalance = await conn.QuerySingleOrDefaultAsync<decimal>(
            _sql.Get("Billing.Select.Journal.PreviousBalance"),
            new { ContractId = contractId, Year = periodYear, Month = periodMonth });
        var rawReceipts = (await conn.QueryAsync(
            _sql.Get("Billing.Select.Receipt.ByContractAndPeriod"),
            new { ContractId = contractId, Year = periodYear, Month = periodMonth })).ToList();

        await conn.ExecuteAsync(
            _sql.Get("DebitNote.Insert.DebitNote.ManualGenerate"),
            new
            {
                Id = noteId,
                ContractId = contractId,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                BillNo = billNo,
                DueDate = dueDate,
                TotalAmount = total,
                GeneratedBy = _currentUser.UserId,
                CompanyId = contract.CompanyId,
                CreatedBy = _currentUser.UserId,
                ContractNo = contractNo,
                TenantName = tenantName ?? "",
                BuildingAddress = buildingAddress ?? "",
                CompanyName = companyName ?? "",
                PreviousBalance = previousBalance
            });

        // 4. 写入 DebitNoteItems（FeeName 从 FeeCodes 表查询）
        var allFeeCodes = await _uow.FeeCodes.GetAllAsync(ct);
        var feeNameMap = allFeeCodes.ToDictionary(f => f.Id, f => f.Name);
        foreach (var journal in periodJournals)
        {
            if (journal.Amount <= 0) continue;
            var feeName = feeNameMap.GetValueOrDefault(journal.FeeCodeId, "");
            await conn.ExecuteAsync(
                _sql.Get("Billing.Insert.DebitNoteItem.Default"),
                new { Id = Guid.NewGuid(), DebitNoteId = noteId, FeeCodeId = journal.FeeCodeId,
                    FeeName = feeName, Amount = journal.Amount, CreatedBy = _currentUser.UserId, CreatedAt = ChinaTime.Now });
        }

        // 5. 写入收款快照
        int sortOrder = 0;
        foreach (var r in rawReceipts)
        {
            await conn.ExecuteAsync(
                _sql.Get("Billing.Insert.DebitNoteReceipt.Default"),
                new
                {
                    Id = Guid.NewGuid(),
                    DebitNoteId = noteId,
                    Amount = (decimal)(r.Amount ?? 0),
                    ReceivedDate = (DateTime?)r.ReceivedDate,
                    PaymentChannel = (string)(r.PaymentChannel ?? "") ?? "",
                    SortOrder = sortOrder++
                });
        }

        // 6. 返回新创建的账单（使用真实的 noteId）
        var created = await GetByIdAsync(noteId, ct);
        return new DebitNote(billNo, contractId, period, noteId);
    }

    public async Task<byte[]> ExportPdfAsync(Guid id, CancellationToken ct)
    {
        using var conn = _db.CreateConnection(); conn.Open();

        using var multi = await conn.QueryMultipleAsync(
            _sql.Get("DebitNote.Select.DebitNote.PdfExport") + ";" +
            _sql.Get("Billing.Select.DebitNoteItem.ByDebitNoteId") + ";" +
            _sql.Get("Billing.Select.DebitNoteReceipt.ByDebitNoteId"),
            new { Id = id });

        var row = await multi.ReadSingleOrDefaultAsync<dynamic>();
        if (row == null)
            throw new InvalidOperationException($"账单 {id} 不存在");

        var billNo = (string)row.BillNo;
        var contractId = (Guid)row.ContractId;
        var periodVal = $"{row.PeriodYear:D4}-{row.PeriodMonth:D2}";
        var totalAmount = (decimal)row.TotalAmount;

        // 构建 DebitNote 实体供 PDF 生成
        var note = new DebitNote(billNo, contractId, periodVal);
        note.SetTotalAmount(totalAmount);

        // 设置快照数据
        note.SetSnapshot(
            contractNo: (string)row.ContractNo ?? "",
            roomCode: null,
            tenantName: (string)row.TenantName ?? "",
            buildingAddress: (string)row.BuildingAddress ?? "",
            companyName: (string)row.CompanyName ?? "",
            previousBalance: (decimal)(row.PreviousBalance ?? 0));

        // 加载明细（FeeName 直接从快照列读取，无需查 FeeCodes）
        var dbItems = (await multi.ReadAsync<DebitNoteItem>()).ToList();
        note.LoadItems(dbItems);

        // 加载收款快照
        var dbReceipts = (await multi.ReadAsync<dynamic>()).ToList();
        var receiptList = dbReceipts.Select(r => new DebitNoteReceipt(
            id, (decimal)(r.Amount ?? 0),
            (DateTime?)r.ReceivedDate,
            (string)(r.PaymentChannel ?? "") ?? "",
            (int)(r.SortOrder ?? 0)
        )).ToList();
        note.LoadReceipts(receiptList);

        // 构建 PDF 参数（直接使用快照字段）
        var items2 = note.Items.Select(i => (i.FeeName ?? "未知", i.Amount)).ToList();
        var genDate = row.CreatedAt is DateTime dt ? dt.ToString("yyyy-MM-dd") : row.CreatedAt?.ToString()?[..10];
        var receiptsForPdf = note.Receipts.Select(r => (r.Amount, r.ReceivedDate?.ToString("yyyy-MM-dd") ?? "", r.PaymentChannel ?? "")).ToList();

        return _pdfGenerator.Generate(note, items2, note.ContractNo ?? "", note.TenantName ?? "",
            note.CompanyName, note.BuildingAddress, genDate, note.PreviousBalance, receiptsForPdf);
    }

    public async Task CancelAsync(Guid id, string reason, Guid cancelledBy, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var row = await conn.QuerySingleOrDefaultAsync(
            _sql.Get("DebitNote.Select.DebitNote.DetailById"), new { Id = id });
        if (row == null) throw new KeyNotFoundException("账单不存在");

        // 公司数据隔离
        var companyId = (Guid)row.CompanyId;
        if (!_currentUser.IsSuperAdmin && _currentUser.CompanyId.HasValue && companyId != _currentUser.CompanyId.Value)
            throw new InvalidOperationException("无权操作其他公司的账单");

        var status = (string)row.Status;
        if (status == "Cancelled")
            throw new InvalidOperationException("账单已作废，无需重复操作");
        if (status == "Paid")
            throw new InvalidOperationException("账单已付清，不允许作废");

        var affected = await conn.ExecuteAsync(
            _sql.Get("DebitNote.Update.DebitNote.Cancel"),
            new { Id = id, CancelledBy = cancelledBy });

        if (affected == 0)
            throw new InvalidOperationException("账单状态不允许作废，仅 Published/Draft 状态可作废");
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection(); conn.Open();
        var row = await conn.QuerySingleOrDefaultAsync(
            _sql.Get("DebitNote.Select.DebitNote.DetailById"), new { Id = id });
        if (row == null) throw new KeyNotFoundException("账单不存在");

        // 公司数据隔离
        var companyId = (Guid)row.CompanyId;
        if (!_currentUser.IsSuperAdmin && _currentUser.CompanyId.HasValue && companyId != _currentUser.CompanyId.Value)
            throw new InvalidOperationException("无权操作其他公司的账单");

        if ((string)row.Status == "Paid")
            throw new InvalidOperationException("账单已付清，不允许删除");

        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(_sql.Get("DebitNote.Delete.DebitNoteReceipts.ByDebitNoteId"), new { Id = id }, tx);
        await conn.ExecuteAsync(_sql.Get("DebitNote.Delete.DebitNoteItems.ByDebitNoteId"), new { Id = id }, tx);
        await conn.ExecuteAsync(_sql.Get("DebitNote.Update.Journal.ResetBilledByDebitNoteId"), new { Id = id }, tx);
        await conn.ExecuteAsync(_sql.Get("DebitNote.Delete.DebitNote.ById"), new { Id = id }, tx);
        tx.Commit();
    }
}
