using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Accounting;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Services.Accounting;

/// <summary>
/// 总账余额查询应用服务 — DDD Application Layer
/// 编排领域仓储查询 + 执行余额计算 + 构建树形结构
/// </summary>
public class GLBalanceService : IGLBalanceService
{
    private readonly IGLBalanceRepository _glRepo;

    public GLBalanceService(IGLBalanceRepository glRepo)
    {
        _glRepo = glRepo;
    }

    public async Task<GLBalanceResultDto> GetBalancesAsync(
        Guid companyId, string period, string? subjectCode, int? subjectLevel,
        string? contractNo, string? sourceType, bool hideZero, CancellationToken ct)
    {
        var parts = period.Split('-');
        var yearStart = parts.Length == 2 ? $"{parts[0]}-01" : null;

        // === 1. 并行查询数据源 ===
        var openingTask = _glRepo.GetOpeningBalancesAsync(companyId, period, contractNo, sourceType, ct);
        var periodTask = _glRepo.GetPeriodActivityAsync(companyId, period, contractNo, sourceType, ct);
        var ytdTask = yearStart != null
            ? _glRepo.GetYtdActivityAsync(companyId, period, yearStart, contractNo, sourceType, ct)
            : Task.FromResult(new Dictionary<string, (decimal YtdDebit, decimal YtdCredit)>());
        var subjectsTask = _glRepo.GetSubjectsAsync(companyId, ct);

        await Task.WhenAll(openingTask, periodTask, ytdTask, subjectsTask);

        var openingMap = openingTask.Result;
        var periodMap = periodTask.Result;
        var ytdMap = ytdTask.Result;
        var subjects = subjectsTask.Result;

        // === 3. 合并计算 ===
        var items = new List<SubjectBalanceDto>();
        foreach (var subj in subjects)
        {
            openingMap.TryGetValue(subj.Code, out var op);
            periodMap.TryGetValue(subj.Code, out var pd);
            ytdMap.TryGetValue(subj.Code, out var yt);

            var closingBalance = (op.OpeningDebit - op.OpeningCredit) + (pd.PeriodDebit - pd.PeriodCredit);
            decimal closingDebit, closingCredit;
            if (subj.Direction == "Debit")
            {
                closingDebit = closingBalance >= 0 ? closingBalance : 0;
                closingCredit = closingBalance < 0 ? -closingBalance : 0;
            }
            else
            {
                closingCredit = closingBalance >= 0 ? closingBalance : 0;
                closingDebit = closingBalance < 0 ? -closingBalance : 0;
            }

            if (hideZero && op.OpeningDebit == 0 && op.OpeningCredit == 0
                && pd.PeriodDebit == 0 && pd.PeriodCredit == 0
                && closingDebit == 0 && closingCredit == 0)
                continue;

            if (subjectLevel.HasValue && subj.Level != subjectLevel.Value)
                continue;

            if (!string.IsNullOrEmpty(subjectCode) && subj.Code != subjectCode
                && !subj.Code.StartsWith(subjectCode))
                continue;

            items.Add(new SubjectBalanceDto
            {
                Code = subj.Code,
                Name = subj.Name,
                ParentCode = subj.ParentCode,
                Direction = subj.Direction,
                Level = subj.Level,
                IsLeaf = subj.IsLeaf,
                OpeningDebit = op.OpeningDebit,
                OpeningCredit = op.OpeningCredit,
                PeriodDebit = pd.PeriodDebit,
                PeriodCredit = pd.PeriodCredit,
                YtdDebit = yt.YtdDebit,
                YtdCredit = yt.YtdCredit,
                ClosingDebit = closingDebit,
                ClosingCredit = closingCredit
            });
        }

        // === 4. 构建树形结构 ===
        var tree = BuildTree(items);

        // === 5. 汇总 ===
        var totals = new BalanceTotalsDto();
        foreach (var item in items)
        {
            totals.OpeningDebit += item.OpeningDebit;
            totals.OpeningCredit += item.OpeningCredit;
            totals.PeriodDebit += item.PeriodDebit;
            totals.PeriodCredit += item.PeriodCredit;
            totals.YtdDebit += item.YtdDebit;
            totals.YtdCredit += item.YtdCredit;
            totals.ClosingDebit += item.ClosingDebit;
            totals.ClosingCredit += item.ClosingCredit;
        }

        return new GLBalanceResultDto { Period = period, Items = tree, Totals = totals };
    }

    public async Task<GLDetailResultDto> GetDetailAsync(
        Guid companyId, string period, string subjectCode, string? contractNo, CancellationToken ct)
    {
        // 并行查询：分录 + 科目名称 + 期初/本期
        var entriesTask = _glRepo.GetDetailAsync(companyId, period, subjectCode, contractNo, ct);
        var subjectNameTask = GetSubjectNameAsync(companyId, subjectCode, ct);
        var openingTask = _glRepo.GetOpeningBalancesAsync(companyId, period, contractNo, null, ct);
        var periodTask = _glRepo.GetPeriodActivityAsync(companyId, period, contractNo, null, ct);

        await Task.WhenAll(entriesTask, subjectNameTask, openingTask, periodTask);

        var entries = entriesTask.Result;
        var subjectName = subjectNameTask.Result;
        openingTask.Result.TryGetValue(subjectCode, out var op);
        periodTask.Result.TryGetValue(subjectCode, out var pd);

        var closingBalance = (op.OpeningDebit - op.OpeningCredit) + (pd.PeriodDebit - pd.PeriodCredit);
        var closingDebit = closingBalance >= 0 ? closingBalance : 0;
        var closingCredit = closingBalance < 0 ? -closingBalance : 0;

        // 按合同号分组
        var grouped = entries
            .GroupBy(e => e.ContractNo ?? "(无合同)")
            .Select(g => new ContractGroupDto
            {
                ContractNo = g.Key,
                Entries = g.Select(e => new GLEntryItemDto
                {
                    Date = e.Date?.ToString("yyyy-MM-dd") ?? "",
                    ContractNo = e.ContractNo ?? "",
                    SourceType = e.SourceType ?? "",
                    SourceId = e.SourceId?.ToString() ?? "",
                    Description = e.Description ?? "",
                    Direction = e.Direction ?? "",
                    Amount = e.Amount
                }).ToList(),
                SubtotalDebit = g.Where(e => e.Direction == "Debit").Sum(e => e.Amount),
                SubtotalCredit = g.Where(e => e.Direction == "Credit").Sum(e => e.Amount)
            }).ToList();

        return new GLDetailResultDto
        {
            SubjectCode = subjectCode,
            SubjectName = subjectName,
            Period = period,
            OpeningDebit = op.OpeningDebit,
            OpeningCredit = op.OpeningCredit,
            PeriodDebit = pd.PeriodDebit,
            PeriodCredit = pd.PeriodCredit,
            ClosingDebit = closingDebit,
            ClosingCredit = closingCredit,
            GroupedByContract = grouped
        };
    }

    // ========== 私有方法 ==========

    private async Task<string> GetSubjectNameAsync(Guid companyId, string code, CancellationToken ct)
        => await _glRepo.GetSubjectNameAsync(companyId, code, ct);

    private static List<SubjectBalanceDto> BuildTree(List<SubjectBalanceDto> flatList)
    {
        var lookup = flatList.ToDictionary(x => x.Code);
        var roots = new List<SubjectBalanceDto>();

        foreach (var item in flatList)
        {
            if (!string.IsNullOrEmpty(item.ParentCode) && lookup.TryGetValue(item.ParentCode, out var parent))
                parent.Children.Add(item);
            else
                roots.Add(item);
        }

        void Aggregate(SubjectBalanceDto node)
        {
            foreach (var child in node.Children) Aggregate(child);
            if (node.Children.Count <= 0) return;
            node.OpeningDebit = node.Children.Sum(c => c.OpeningDebit);
            node.OpeningCredit = node.Children.Sum(c => c.OpeningCredit);
            node.PeriodDebit = node.Children.Sum(c => c.PeriodDebit);
            node.PeriodCredit = node.Children.Sum(c => c.PeriodCredit);
            node.YtdDebit = node.Children.Sum(c => c.YtdDebit);
            node.YtdCredit = node.Children.Sum(c => c.YtdCredit);
            node.ClosingDebit = node.Children.Sum(c => c.ClosingDebit);
            node.ClosingCredit = node.Children.Sum(c => c.ClosingCredit);
        }

        foreach (var root in roots) Aggregate(root);
        return roots;
    }

}
