namespace RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Billing;

/// <summary>
/// 收款应用服务 — 提供批量确认和冲销等收款编排操作
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// 获取收款列表（按公司、状态、合同筛选）
    /// </summary>
    Task<List<ReceiptDto>> GetAllAsync(Guid? companyId, string? status, Guid? contractId, CancellationToken ct);
    /// <summary>
    /// 批量确认收款记录
    /// </summary>
    /// <param name="ids">要确认的收款记录 ID 列表</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>批量确认结果</returns>
    Task<object> BatchConfirmAsync(List<Guid> ids, CancellationToken ct);

    /// <summary>
    /// 确认单笔收款记录（含更新合同欠款/预存余额）
    /// </summary>
    /// <param name="id">收款记录 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>确认结果</returns>
    Task<object> ConfirmReceiptAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// 冲销指定收款记录
    /// </summary>
    /// <param name="id">要冲销的收款记录 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>冲销操作结果</returns>
    Task<object> ReverseAsync(Guid id, CancellationToken ct);
}
