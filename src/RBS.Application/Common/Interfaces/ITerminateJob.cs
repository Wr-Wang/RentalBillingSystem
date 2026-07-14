namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 合同终止结算服务 — 审批通过后生成押金结算凭证
/// </summary>
public interface ITerminateJob
{
    /// <summary>
    /// 执行合同终止结算
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="actualEndDate">实际搬离日期 (yyyy-MM-dd)，null 表示当日</param>
    /// <param name="depositReturn">押金处理方式：FULL（全额退还）/ DEDUCT（扣款后退还）/ LAST_RENT（抵扣最后月租）</param>
    /// <param name="reason">终止原因</param>
    /// <param name="ct">取消令牌</param>
    Task ExecuteAsync(Guid contractId, string? actualEndDate, string depositReturn, string reason, CancellationToken ct);
}
