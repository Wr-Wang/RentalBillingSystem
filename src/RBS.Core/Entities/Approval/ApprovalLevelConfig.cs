namespace RBS.Core.Entities.Approval;
using RBS.Core.Entities.Base;

/// <summary>
/// 审批级别配置 — AuditableEntity（以自增长 int 为主键）
/// 定义某审批类型下每一级的审批规则，包括审批人角色和金额区间。
/// 通过 LevelNo 排序形成审批链，支持按金额路由到不同审批级。
/// 例如：1 级审批（主管，0-5000 元）→ 2 级审批（经理，5000-50000 元）→ 3 级审批（总监，50000 元以上）
/// </summary>
public class ApprovalLevelConfig : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 关联审批类型标识
    /// 指向 ApprovalType，确定该级别配置所属的审批类型
    /// </summary>
    public Guid ApprovalTypeId { get; private set; }

    /// <summary>
    /// 审批级别序号（从 1 开始）
    /// 数字越小优先级越高，1 为初审，依次递增
    /// </summary>
    public int LevelNo { get; private set; }

    /// <summary>
    /// 审批人角色标识
    /// 关联角色表，指定该级别由哪个角色的用户进行审批
    /// </summary>
    public Guid ApproverRoleId { get; private set; }

    /// <summary>
    /// 金额区间下限（含）
    /// 审批金额在此区间内的请求会路由到该级别，为 null 表示不设下限
    /// </summary>
    public decimal? MinAmount { get; private set; }

    /// <summary>
    /// 金额区间上限（含）
    /// 审批金额在此区间内的请求会路由到该级别，为 null 表示不设上限
    /// </summary>
    public decimal? MaxAmount { get; private set; }

    /// <summary>
    /// 所属公司标识
    /// 审批级别配置按公司隔离
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private ApprovalLevelConfig() { }

    /// <summary>
    /// 创建审批级别配置实例
    /// </summary>
    /// <param name="approvalTypeId">关联审批类型标识</param>
    /// <param name="levelNo">审批级别序号</param>
    /// <param name="approverRoleId">审批人角色标识</param>
    /// <param name="companyId">所属公司标识</param>
    public ApprovalLevelConfig(Guid approvalTypeId, int levelNo, Guid approverRoleId, Guid companyId)
    {
        ApprovalTypeId = approvalTypeId;
        LevelNo = levelNo;
        ApproverRoleId = approverRoleId;
        CompanyId = companyId;
    }

    /// <summary>
    /// 设置审批级别序号
    /// </summary>
    /// <param name="levelNo">新的级别序号</param>
    public void SetLevelNo(int levelNo) => LevelNo = levelNo;

    /// <summary>
    /// 设置审批人角色
    /// </summary>
    /// <param name="approverRoleId">审批人角色标识</param>
    public void SetApproverRole(Guid approverRoleId) => ApproverRoleId = approverRoleId;

    /// <summary>
    /// 设置金额路由区间
    /// 审批金额在此区间内的请求会自动路由到该级别
    /// </summary>
    /// <param name="min">金额下限（含），null 表示不设下限</param>
    /// <param name="max">金额上限（含），null 表示不设上限</param>
    public void SetAmountRange(decimal? min, decimal? max)
    {
        MinAmount = min;
        MaxAmount = max;
    }
}
