namespace RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

/// <summary>
/// 审批类型 — AuditableEntity（以自增长 int 为主键）
/// 定义审批的分类模板，如"合同终止审批"、"调租审批"等。
/// 每种审批类型关联一组 ApprovalLevelConfig 配置来确定审批级数和审批人角色。
/// 按公司隔离（CompanyId），不同公司可独立维护审批类型体系。
/// </summary>
public class ApprovalType : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 审批类型名称
    /// 如"合同终止审批"、"调价审批"、"暂停计费审批"等
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 审批类型编码
    /// 业务唯一标识代码，用于系统内部识别审批类别（如 "TERMINATE"、"FEE_ADJUST"）
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 审批类型描述
    /// 详细说明该审批类型的适用场景和使用条件
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 是否启用
    /// true 表示该审批类型可用，false 表示已停用
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 所属公司标识
    /// 审批类型按公司隔离，每个公司可自定义审批类型
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ApprovalType() { }

    /// <summary>
    /// 创建审批类型实例，默认启用
    /// </summary>
    /// <param name="name">审批类型名称</param>
    /// <param name="code">审批类型编码</param>
    /// <param name="companyId">所属公司标识</param>
    public ApprovalType(string name, string code, Guid companyId)
    {
        Name = name;
        Code = code;
        CompanyId = companyId;
    }

    /// <summary>
    /// 重命名审批类型
    /// </summary>
    /// <param name="name">新的名称</param>
    public void Rename(string name) => Name = name;

    /// <summary>
    /// 设置审批类型描述
    /// </summary>
    /// <param name="description">描述文本</param>
    public void SetDescription(string? description) => Description = description;

    /// <summary>
    /// 启用审批类型
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// 停用审批类型
    /// 停用后不可新建该类型的审批请求，已有审批流程不受影响
    /// </summary>
    public void Deactivate() => IsActive = false;
}
