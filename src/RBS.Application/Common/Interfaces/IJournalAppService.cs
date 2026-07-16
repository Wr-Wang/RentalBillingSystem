namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 日记账应用服务接口 — 提供日记账的查询、预览、生成编排能力
/// </summary>
public interface IJournalAppService
{
    /// <summary>
    /// 分页查询日记账
    /// </summary>
    Task<object> GetPagedAsync(Guid? companyId, string? period, string? contractNo, Guid? feeCodeId, bool? glPosted, Guid? contractId, int page, int pageSize);

    /// <summary>
    /// 根据 ID 获取日记账详情，不存在时返回 null
    /// </summary>
    Task<object?> GetByIdAsync(Guid id);

    /// <summary>
    /// 根据合同 ID 获取日记账列表（含付款信息）
    /// </summary>
    Task<List<object>> GetByContractAsync(Guid contractId);

    /// <summary>
    /// 预览生成应收 — 计算哪些账期缺少 Journal
    /// </summary>
    Task<object> PreviewAsync(Guid contractId);

    /// <summary>
    /// 提交生成应收 — 直接创建或走审批
    /// </summary>
    Task<object> GenerateRequestAsync(Guid contractId, Guid userId);
}
