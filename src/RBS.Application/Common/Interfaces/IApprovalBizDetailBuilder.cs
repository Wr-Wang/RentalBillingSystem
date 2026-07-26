using RBS.Core.Entities.Approval;
using RBS.Application.DTOs.Approval;

namespace RBS.Application.Common.Interfaces;

/// <summary>
/// 审批业务详情构建器 — 从结构化数据或旧版 Description 解析构建审批对比视图
/// </summary>
public interface IApprovalBizDetailBuilder
{
    /// <summary>
    /// 获取审批业务详情
    /// </summary>
    Task<ApprovalBizDetailDto?> GetBizDetailAsync(ApprovalRequest approval);
}
