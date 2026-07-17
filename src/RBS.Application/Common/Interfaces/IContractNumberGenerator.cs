namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 合同编号生成器接口 — 统一合同编号生成规则。
/// 格式：CT{yyyyMMddHHmmssfff}{XX}（无连字符）
/// - CT：固定前缀
/// - yyyyMMddHHmmssfff：北京时间毫秒级时间戳
/// - XX：2 位随机十六进制（00-FF）防冲突
/// 续签：在原始编号末尾追加 R{n}
/// </summary>
public interface IContractNumberGenerator
{
    /// <summary>生成新合同编号：CT{yyyyMMddHHmmssfff}{XX}</summary>
    string GenerateNewContractNo();

    /// <summary>生成续签合同编号：{原始合同号}R{续签次数}</summary>
    string GenerateRenewalContractNo(string currentContractNo, int renewalCount);
}
