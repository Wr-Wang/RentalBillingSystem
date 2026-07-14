namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 费用科目实体（领域实体，继承 AuditableEntity 并实现 IHasCompany）
/// —— 定义一项具体的收费科目，如租金、物业费、水费、电费等。
/// 每个费用科目归属于一个公司（CompanyId），支持多种计费方式、分类与收费类型。
/// 生命周期：创建 -> 激活（默认）/停用，停用后不再用于新的计费配置。
/// </summary>
public class FeeCode : AuditableEntity, IHasCompany
{
    /// <summary>费用科目编码（唯一标识），例如 "RENT01"、"WATER01"</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>费用科目名称，例如 "写字楼租金"、"物业管理费"、"水费"</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 计费方式。
    /// "FixedAmount"（固定金额）—— 按月收取固定费用；
    /// "Metered"（按表计量）—— 根据水电气表读数计算费用；
    /// "RateByArea"（按面积分摊）—— 按租赁面积比例分摊费用。
    /// </summary>
    public string BillingMode { get; private set; } = "FixedAmount";

    /// <summary>计量单位，仅在 Metered（按表计量）模式下使用，例如 "吨"、"度"、"立方米"</summary>
    public string? Unit { get; private set; }

    /// <summary>排序序号，控制费用科目在界面或账单中的展示顺序（升序排列）</summary>
    public int SortOrder { get; private set; }

    /// <summary>是否启用。true=启用（默认），false=停用。停用的科目不会出现在新增计费配置的选择列表中。</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 费用科目分类。
    /// 例如 "Rent"（租金）、"ManagementFee"（物业费）、"Water"（水费）、
    /// "Electricity"（电费）、"Parking"（停车费）、"Other"（其他，默认值）。
    /// 用于按类别汇总统计数据。
    /// </summary>
    public string Category { get; private set; } = "Other";

    /// <summary>是否必选。true=该科目在创建合同时必须配置，不可跳过；false=可选配置。</summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// 收费类型。
    /// "OneTime"（一次性）—— 仅在合同期内收取一次，如入场费、押金；
    /// "Recurring"（周期性，默认）—— 按周期（月/季/年）重复收取，如租金、物业费。
    /// </summary>
    public string ChargeType { get; private set; } = "Recurring";

    /// <summary>所属公司 ID，实现多租户隔离（IHasCompany）</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>是否为一次性收费（ChargeType == "OneTime"）</summary>
    public bool IsOneTime => ChargeType == "OneTime";

    /// <summary>是否为周期性收费（ChargeType == "Recurring"）</summary>
    public bool IsRecurring => ChargeType == "Recurring";

    /// <summary>
    /// 私有无参构造函数，供 EF Core 延迟加载使用。
    /// 不允许外部直接调用。
    /// </summary>
    private FeeCode() { }

    /// <summary>
    /// 创建新的费用科目实例。
    /// 新创建的科目默认为启用状态（IsActive = true）、计费方式为 FixedAmount、
    /// 分类为 Other、收费类型为 Recurring。
    /// </summary>
    /// <param name="code">费用科目编码，不可为空，建议使用业务前缀+序号，如 "RENT01"</param>
    /// <param name="name">费用科目名称，如 "写字楼租金"</param>
    /// <param name="companyId">所属公司 ID</param>
    public FeeCode(string code, string name, Guid companyId)
    {
        Code = code;
        Name = name;
        CompanyId = companyId;
    }

    /// <summary>重命名费用科目名称</summary>
    /// <param name="name">新的名称</param>
    public void Rename(string name) => Name = name;

    /// <summary>设置费用科目编码</summary>
    /// <param name="code">新的编码</param>
    public void SetCode(string code) => Code = code;

    /// <summary>
    /// 设置计费方式
    /// </summary>
    /// <param name="mode">计费方式："FixedAmount"（固定金额）、"Metered"（按表计量）、"RateByArea"（按面积分摊）</param>
    /// <exception cref="ArgumentException">当 mode 不是 "FixedAmount"、"Metered" 或 "RateByArea" 时抛出</exception>
    public void SetBillingMode(string mode)
    {
        if (mode != "FixedAmount" && mode != "Metered" && mode != "RateByArea")
            throw new ArgumentException($"无效的计费方式：{mode}。允许的值：FixedAmount, Metered, RateByArea", nameof(mode));
        BillingMode = mode;
    }

    /// <summary>设置计量单位（仅在 Metered 模式下有意义）</summary>
    /// <param name="unit">计量单位，例如 "吨"、"度"、"立方米"；传 null 表示清除</param>
    public void SetUnit(string? unit) => Unit = unit;

    /// <summary>设置排序序号</summary>
    /// <param name="order">排序序号，数值越小越靠前</param>
    public void SetSortOrder(int order) => SortOrder = order;

    /// <summary>设置费用科目分类</summary>
    /// <param name="category">分类标识，例如 "Rent"、"ManagementFee"、"Water"、"Electricity"、"Other"</param>
    public void SetCategory(string category) => Category = category;

    /// <summary>设置是否为必选科目</summary>
    /// <param name="required">true=必选，false=可选</param>
    public void SetRequired(bool required) => IsRequired = required;

    /// <summary>
    /// 设置收费类型
    /// </summary>
    /// <param name="chargeType">收费类型："OneTime"（一次性）或 "Recurring"（周期性）</param>
    /// <exception cref="ArgumentException">当 chargeType 不是 "OneTime" 或 "Recurring" 时抛出</exception>
    public void SetChargeType(string chargeType)
    {
        if (chargeType != "Recurring" && chargeType != "OneTime")
            throw new ArgumentException($"无效的收费类型：{chargeType}。允许的值：Recurring, OneTime", nameof(chargeType));
        ChargeType = chargeType;
    }

    /// <summary>启用该费用科目，使其可被用于新的计费配置</summary>
    public void Activate() => IsActive = true;

    /// <summary>停用该费用科目，已存在的计费配置不受影响，但新建合同时不可选择</summary>
    public void Deactivate() => IsActive = false;
}
