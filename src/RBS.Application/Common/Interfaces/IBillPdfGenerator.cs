using RBS.Core.Entities.Billing;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 账单 PDF 生成器接口 — 实现层在 Infrastructure
/// </summary>
public interface IBillPdfGenerator
{
    byte[] Generate(DebitNote note, IReadOnlyList<(string FeeName, decimal Amount)> items,
        string contractNo, string tenantName, string? companyName);
}
