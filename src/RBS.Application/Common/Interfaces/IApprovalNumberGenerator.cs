namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 审批申请编号生成器接口。
/// 格式：AP{yyyyMMdd}{6位当日序号}（无连字符）
/// - AP：固定前缀（审批）
/// - yyyyMMdd：提交日期
/// - 000001：当日自增序号
/// </summary>
public interface IApprovalNumberGenerator
{
    /// <summary>生成审批申请编号：AP{yyyyMMdd}{6位当日序号}</summary>
    Task<string> GenerateRequestNo();
}
