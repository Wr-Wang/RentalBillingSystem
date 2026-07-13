namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 合同终止结算服务 — 审批通过后生成押金结算凭证
/// </summary>
public interface ITerminateJob
{
    Task ExecuteAsync(Guid contractId, string? actualEndDate, string depositReturn, string reason, CancellationToken ct);
}
