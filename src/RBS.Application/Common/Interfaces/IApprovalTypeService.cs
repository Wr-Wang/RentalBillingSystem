using RBS.Application.DTOs.Approval;

namespace RBS.Application.Common.Interfaces;

public interface IApprovalTypeService
{
    Task<List<ApprovalTypeDto>> GetListAsync(CancellationToken ct = default);
    Task<ApprovalTypeDto> CreateAsync(CreateApprovalTypeRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateApprovalTypeRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // 审批级别
    Task<List<ApprovalLevelConfigDto>> GetLevelsAsync(Guid typeId, CancellationToken ct = default);
    Task<ApprovalLevelConfigDto> CreateLevelAsync(CreateApprovalLevelRequest request, CancellationToken ct = default);
    Task UpdateLevelAsync(Guid id, UpdateApprovalLevelRequest request, CancellationToken ct = default);
    Task DeleteLevelAsync(Guid id, CancellationToken ct = default);
}
