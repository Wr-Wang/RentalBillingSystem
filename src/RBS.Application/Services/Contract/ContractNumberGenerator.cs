using System.Text.RegularExpressions;
using RBS.Application.Common.Interfaces;
using RBS.Core.Common;

namespace RBS.Application.Services.Contract;

/// <summary>
/// 合同编号生成器 — 统一规则：
/// 新签：CT{yyyyMMddHHmmssfff}{XX}
/// 续签：{原始号}R{n}
/// </summary>
public partial class ContractNumberGenerator : IContractNumberGenerator
{
    // 匹配尾部 R{n} 续签标记，如 "R1"、"R12"
    [GeneratedRegex(@"R\d+$", RegexOptions.Compiled)]
    private static partial Regex RenewalSuffixPattern();

    /// <inheritdoc />
    public string GenerateNewContractNo()
    {
        var now = ChinaTime.Now;
        var timestamp = now.ToString("yyyyMMddHHmmssfff");
        var random = Random.Shared.Next(0, 256).ToString("X2");
        return $"CT{timestamp}{random}";
    }

    /// <inheritdoc />
    public string GenerateRenewalContractNo(string currentContractNo, int renewalCount)
    {
        // 剥离尾部已有的 R{n} 续签后缀，得到原始基础号
        var baseNo = RenewalSuffixPattern().Replace(currentContractNo, "");
        return $"{baseNo}R{renewalCount}";
    }
}
