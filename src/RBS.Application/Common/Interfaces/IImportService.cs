using RBS.Application.DTOs.Import;

namespace RBS.Application.Common.Interfaces;

/// <summary>导入服务 — 通用导入审批流程编排</summary>
public interface IImportService
{
    /// <summary>提交导入（校验 → 暂存 → 提交审批）</summary>
    Task<ImportResponse> SubmitAsync(ImportRequest request, CancellationToken ct);

    /// <summary>审批通过后执行创建（由 ApprovalCompletedEventHandler 调用）</summary>
    Task ExecuteApprovedImportAsync(Guid batchId, CancellationToken ct);
}
