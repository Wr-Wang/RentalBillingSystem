using RBS.Application.DTOs.Billing;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 计费应用服务接口 — 提供应收（Journal）查询、收款登记与确认、收款驳回等核心计费能力
/// </summary>
public interface IBillingService
{
    /// <summary>
    /// 获取指定合同的所有应收（Journal）
    /// </summary>
    /// <param name="contractId">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>Journal DTO 列表</returns>
    Task<List<JournalDto>> GetPlansAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>
    /// 获取指定公司的收款记录列表
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>收款记录 DTO 列表</returns>
    Task<List<ReceiptDto>> GetReceiptsAsync(Guid companyId, CancellationToken ct = default);

    /// <summary>
    /// 登记收款记录
    /// </summary>
    /// <param name="request">创建收款请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>创建的收款记录 DTO</returns>
    Task<ReceiptDto> RegisterReceiptAsync(CreateReceiptRequest request, CancellationToken ct = default);

    /// <summary>
    /// 确认收款（确认后自动触发凭证生成）
    /// </summary>
    /// <param name="receiptId">收款记录 ID</param>
    /// <param name="userId">确认操作的用户 ID</param>
    /// <param name="ct">取消令牌</param>
    Task ConfirmReceiptAsync(Guid receiptId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// 驳回收款记录
    /// </summary>
    /// <param name="receiptId">收款记录 ID</param>
    /// <param name="reason">驳回原因</param>
    /// <param name="ct">取消令牌</param>
    Task RejectReceiptAsync(Guid receiptId, string reason, CancellationToken ct = default);
}
