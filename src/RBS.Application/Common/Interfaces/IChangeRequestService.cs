using RBS.Application.DTOs.Contract;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 合同变更请求服务接口
/// </summary>
public interface IChangeRequestService
{
    /// <summary>获取指定合同的变更请求列表</summary>
    Task<List<ChangeRequestDto>> GetByContractAsync(Guid contractId, CancellationToken ct = default);

    /// <summary>获取变更请求详情</summary>
    Task<ChangeRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>创建变更请求</summary>
    Task<ChangeRequestDto> CreateAsync(CreateChangeRequestDto request, Guid userId, CancellationToken ct = default);

    /// <summary>提交变更请求到审批</summary>
    Task<ChangeRequestDto> SubmitAsync(Guid id, Guid userId, CancellationToken ct = default);
}
