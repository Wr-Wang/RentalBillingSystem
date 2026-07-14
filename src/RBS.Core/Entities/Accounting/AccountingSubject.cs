namespace RBS.Core.Entities.Accounting;
using RBS.Core.Entities.Base;

/// <summary>
/// 会计科目 — AuditableEntity（以自增长 int 为主键）
/// 会计科目是会计核算的最小分类单元，定义了对经济业务进行分类记录的项目。
/// 采用树形层级结构（通过 ParentCode 实现父子关系），支持多级科目体系。
/// 每个科目归属于特定公司（CompanyId），实现公司间科目体系的隔离。
/// </summary>
public class AccountingSubject : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 科目编码
    /// 唯一标识一个会计科目（如 "1001" 表示库存现金，"4001" 表示实收资本等），
    /// 编码规则通常按行业标准（如企业会计准则）设计，层级关系隐含在编码前缀中
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 科目名称
    /// 会计科目的中文显示名称（如 "库存现金"、"银行存款"、"管理费用"）
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 父级科目编码
    /// 用于构建科目树形结构，为 null 表示一级科目（根节点）；
    /// 通过 ParentCode 关联同一公司下其他科目的 Code 字段
    /// </summary>
    public string? ParentCode { get; private set; }

    /// <summary>
    /// 科目层级深度
    /// 一级科目为 1，下级逐级递增；用于控制科目选择范围和报表汇总粒度
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// 科目余额方向
    /// "Debit"（借方余额）— 资产类、成本类科目使用；
    /// "Credit"（贷方余额）— 负债类、所有者权益类、收入类科目使用
    /// </summary>
    public string Direction { get; private set; } = "Debit";

    /// <summary>
    /// 是否末级科目
    /// true 表示该科目为最底层科目，可以用于记账；
    /// false 表示该科目为父级科目，仅用于汇总分类，不可直接使用
    /// </summary>
    public bool IsLeaf { get; private set; }

    /// <summary>
    /// 是否启用
    /// true 表示科目处于启用状态，可在凭证中使用；
    /// false 表示科目已停用，不可用于新增凭证（历史数据不受影响）
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 所属公司标识
    /// 科目按公司隔离，每个公司可维护自己的科目体系
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private AccountingSubject() { }

    /// <summary>
    /// 创建会计科目实例
    /// </summary>
    /// <param name="code">科目编码，需符合企业会计准则规范</param>
    /// <param name="name">科目名称</param>
    /// <param name="companyId">所属公司标识</param>
    public AccountingSubject(string code, string name, Guid companyId)
    {
        Code = code;
        Name = name;
        CompanyId = companyId;
    }

    /// <summary>
    /// 重命名科目名称
    /// </summary>
    /// <param name="name">新的科目名称</param>
    public void Rename(string name) => Name = name;

    /// <summary>
    /// 设置父级科目编码
    /// 设置后自动判断是否为末级科目（有父级则为末级），
    /// 并自动设定层级深度（一级为 1，子级继承父级+1）
    /// </summary>
    /// <param name="parentCode">父级科目编码，为 null 表示设置为一级科目</param>
    public void SetParentCode(string? parentCode)
    {
        ParentCode = parentCode;
        IsLeaf = parentCode != null;
        Level = parentCode == null ? 1 : (Level > 0 ? Level : 2);
    }

    /// <summary>
    /// 设置科目余额方向
    /// </summary>
    /// <param name="dir">方向值，"Debit"（借方余额）或 "Credit"（贷方余额）</param>
    public void SetDirection(string dir) => Direction = dir;

    /// <summary>
    /// 设置是否为末级科目
    /// </summary>
    /// <param name="isLeaf">是否为末级科目</param>
    public void SetIsLeaf(bool isLeaf) => IsLeaf = isLeaf;

    /// <summary>
    /// 启用科目
    /// 启用后可在凭证分录中选择该科目使用
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// 停用科目
    /// 停用后不可在新的凭证中使用该科目，已有历史凭证不受影响
    /// </summary>
    public void Deactivate() => IsActive = false;
}
