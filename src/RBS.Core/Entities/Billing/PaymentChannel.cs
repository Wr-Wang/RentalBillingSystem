namespace RBS.Core.Entities.Billing;
using RBS.Core.Entities.Base;

/// <summary>
/// 支付渠道实体（领域实体，继承 AuditableEntity 并实现 IHasCompany）
/// —— 定义租户可选的付款方式，如银行转账、微信支付、支付宝、现金、POS 刷卡等。
/// 每个支付渠道归属于一个公司（CompanyId），支持启用/停用控制。
/// 生命周期：创建 -> 激活（默认）/停用。
/// </summary>
public class PaymentChannel : AuditableEntity, IHasCompany
{
    /// <summary>支付渠道显示名称，例如 "银行转账"、"微信支付"、"支付宝"、"现金"、"POS刷卡"</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>支付渠道编码，用于系统内部标识，例如 "BANK_TRANSFER"、"WECHAT"、"ALIPAY"、"CASH"、"POS"</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>是否启用。true=启用（默认），false=停用。停用后该渠道不可用于收款登记。</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>所属公司 ID，实现多租户隔离（IHasCompany）</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>私有无参构造函数，供 EF Core 延迟加载使用</summary>
    private PaymentChannel() { }

    /// <summary>
    /// 创建支付渠道实例
    /// </summary>
    /// <param name="name">支付渠道名称，例如 "银行转账"</param>
    /// <param name="code">支付渠道编码，例如 "BANK_TRANSFER"</param>
    /// <param name="companyId">所属公司 ID</param>
    public PaymentChannel(string name, string code, Guid companyId)
    {
        Name = name;
        Code = code;
        CompanyId = companyId;
    }

    /// <summary>重命名支付渠道名称</summary>
    /// <param name="name">新的名称</param>
    public void Rename(string name) => Name = name;

    /// <summary>设置支付渠道编码</summary>
    /// <param name="code">新的编码</param>
    public void SetCode(string code) => Code = code;

    /// <summary>启用该支付渠道，使其可被用于收款登记</summary>
    public void Activate() => IsActive = true;

    /// <summary>停用该支付渠道，已登记的收款记录不受影响，但新收款不可选择</summary>
    public void Deactivate() => IsActive = false;
}
