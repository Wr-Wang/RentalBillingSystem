using RBS.Application.Common.Interfaces;
using RBS.Application.DTOs.Approval;
using RBS.Application.DTOs.Contract;
using RBS.Core.Entities.Contract;
using RBS.Core.Interfaces.UnitOfWork;

namespace RBS.Application.Services.Contract;

/// <summary>
/// 合同变更请求应用服务
/// </summary>
public class ChangeRequestAppService : IChangeRequestService
{
    private readonly IUnitOfWork _uow;
    private readonly IApprovalService _approvalService;

    public ChangeRequestAppService(IUnitOfWork uow, IApprovalService approvalService)
    {
        _uow = uow;
        _approvalService = approvalService;
    }

    public async Task<List<ChangeRequestDto>> GetByContractAsync(Guid contractId, CancellationToken ct = default)
    {
        var all = await _uow.ChangeRequests.GetAllAsync(ct);
        var list = all.Where(c => c.ContractId == contractId)
                      .OrderByDescending(c => c.CreatedAt)
                      .ToList();

        return list.Select(MapToDto).ToList();
    }

    public async Task<ChangeRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.ChangeRequests.GetByIdAsync(id, ct);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<ChangeRequestDto> CreateAsync(CreateChangeRequestDto request, Guid userId, CancellationToken ct = default)
    {
        var entity = new ChangeRequest(request.ContractId, request.CompanyId, request.ChangeType, request.Reason);

        if (request.EffectiveDate.HasValue)
            entity.SetEffectiveDate(request.EffectiveDate.Value);

        if (request.Items?.Count > 0)
        {
            foreach (var item in request.Items)
            {
                entity.AddItem(item.TargetType, item.TargetId, item.FieldName,
                    item.OldValue, item.NewValue, item.OldValueDecimal, item.NewValueDecimal);
            }
        }

        await _uow.ChangeRequests.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);

        return MapToDto(entity);
    }

    public async Task<ChangeRequestDto> SubmitAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entity = await _uow.ChangeRequests.GetByIdAsync(id, ct);
        if (entity == null)
            throw new InvalidOperationException("变更请求不存在");

        entity.SubmitForApproval();

        // 查找 CHANGE_REQUEST 审批类型
        var changeType = await _uow.FindApprovalTypeByCodeAsync("CHANGE_REQUEST", ct);
        if (changeType == null)
            throw new InvalidOperationException("未找到有效的合同变更审批类型，请联系管理员配置");

        // 构建变更描述
        var itemDescriptions = string.Join("; ",
            entity.Items.Select(i => $"{i.TargetType}.{i.FieldName}: {i.OldValue ?? "-"} → {i.NewValue}"));

        var submitRequest = new SubmitApprovalRequest
        {
            ApprovalTypeId = changeType.Id,
            Title = $"合同变更 - {entity.ChangeType}",
            Description = itemDescriptions,
            TargetEntityId = entity.Id,
            TargetEntityType = "ChangeRequest"
        };

        var approvalResult = await _approvalService.SubmitAsync(submitRequest, ct);

        entity.SetApprovalRequestId(approvalResult.Id);
        await _uow.CommitAsync(ct);

        return MapToDto(entity);
    }

    private static ChangeRequestDto MapToDto(ChangeRequest entity)
    {
        var statusLabels = new Dictionary<string, string>
        {
            ["Draft"] = "草稿",
            ["PendingApproval"] = "审批中",
            ["Approved"] = "已通过",
            ["Rejected"] = "已驳回"
        };

        return new ChangeRequestDto
        {
            Id = entity.Id,
            ContractId = entity.ContractId,
            CompanyId = entity.CompanyId,
            ChangeType = entity.ChangeType,
            Status = entity.Status,
            StatusLabel = statusLabels.GetValueOrDefault(entity.Status, entity.Status),
            EffectiveDate = entity.EffectiveDate,
            Reason = entity.Reason,
            ApprovalRequestId = entity.ApprovalRequestId,
            CreatedAt = entity.CreatedAt,
            Items = entity.Items.Select(i => new ChangeRequestItemDto
            {
                TargetType = i.TargetType,
                TargetId = i.TargetId,
                FieldName = i.FieldName,
                OldValue = i.OldValue,
                NewValue = i.NewValue,
                OldValueDecimal = i.OldValueDecimal,
                NewValueDecimal = i.NewValueDecimal
            }).ToList()
        };
    }
}
