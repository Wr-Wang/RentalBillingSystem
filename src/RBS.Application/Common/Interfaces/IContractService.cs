using RBS.Application.DTOs.Contract;
using RBS.Core.Interfaces.Repositories;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 合同管理应用服务接口 — 提供合同的全生命周期管理能力
/// 包括合同的增删改查、状态变更（激活/终止/暂停/恢复）
/// </summary>
public interface IContractService
{
    /// <summary>
    /// 获取指定公司的合同列表（含主租客信息）
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>合同 DTO 列表</returns>
    Task<List<ContractDto>> GetListAsync(Guid companyId, CancellationToken ct = default);

    /// <summary>
    /// 根据租客 ID 获取其关联的所有合同
    /// </summary>
    /// <param name="tenantId">租客 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>合同 DTO 列表</returns>
    Task<List<ContractDto>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// 分页查询合同列表（支持关键词、状态、房间筛选）
    /// </summary>
    /// <param name="companyId">公司 ID</param>
    /// <param name="page">页码，从 1 开始</param>
    /// <param name="pageSize">每页条数，默认 10</param>
    /// <param name="keyword">搜索关键词（合同号/房源编码）</param>
    /// <param name="status">合同状态筛选</param>
    /// <param name="roomId">房间 ID 筛选</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>分页结果，含租客信息和续签状态</returns>
    Task<PagedResult<ContractDto>> GetPagedListAsync(Guid companyId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null, Guid? roomId = null, CancellationToken ct = default);

    /// <summary>
    /// 根据 ID 获取合同详情（含租客、费用配置）
    /// </summary>
    /// <param name="id">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>合同详细 DTO，不存在则返回 null</returns>
    Task<ContractDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 获取合同租客列表（含详细信息）
    /// </summary>
    Task<List<ContractTenantInfoDto>> GetTenantsAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>
    /// 创建新合同
    /// </summary>
    /// <param name="request">创建合同请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>创建的合同 DTO</returns>
    Task<ContractDto> CreateAsync(CreateContractRequest request, CancellationToken ct = default);

    /// <summary>
    /// 激活合同（将状态设为 Active）
    /// </summary>
    /// <param name="id">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    Task ActivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 终止合同（将状态设为 Terminated）
    /// </summary>
    /// <param name="id">合同 ID</param>
    /// <param name="reason">终止原因</param>
    /// <param name="ct">取消令牌</param>
    Task TerminateAsync(Guid id, string reason, CancellationToken ct = default);

    /// <summary>
    /// 暂停合同（将状态设为 Suspended）
    /// </summary>
    /// <param name="id">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    Task SuspendAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 恢复合同（将状态从 Suspended 恢复为 Active）
    /// </summary>
    /// <param name="id">合同 ID</param>
    /// <param name="ct">取消令牌</param>
    Task ResumeAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// 检查合同是否有待审批的调价申请，有则抛出 InvalidOperationException
    /// </summary>
    Task EnsureNoPendingForContractAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>
    /// 执行合同创建（绕过审批，直接执行）
    /// </summary>
    Task<Guid> ExecuteContractCreationAsync(Guid requestId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// 提交合同创建请求状态更新
    /// </summary>
    Task SubmitContractCreateRequestStatusAsync(Guid requestId, CancellationToken ct = default);

    /// <summary>
    /// 设置审批请求关联的合同 ID
    /// </summary>
    Task SetApprovalRequestContractIdAsync(Guid approvalRequestId, Guid contractId, CancellationToken ct = default);

    /// <summary>
    /// 执行费用调价
    /// </summary>
    Task<object> FeeAdjustAsync(Guid contractId, FeeAdjustRequest request, Guid userId, CancellationToken ct = default);
}
