namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

/// <summary>
/// 公司 — 房产出租方聚合根
/// 代表系统中的出租公司（房东/物业管理公司），包含公司基本信息、银行账户信息、结算规则及启用状态
/// </summary>
public class Company : AggregateRoot
{
    /// <summary>
    /// 公司名称（必填），作为公司的业务标识和显示名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 公司编码（可选），用于内部管理和系统间对接的短码标识
    /// </summary>
    public string? Code { get; private set; }

    /// <summary>
    /// 联系人姓名（可选），公司的业务对接人
    /// </summary>
    public string? ContactPerson { get; private set; }

    /// <summary>
    /// 联系电话（可选），公司的业务联系方式
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// 公司地址（可选），注册或经营地址
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// 证件类型（可选），如"营业执照"、"身份证"等
    /// </summary>
    public string? IdType { get; private set; }

    /// <summary>
    /// 证件号码（可选），与证件类型对应的具体证件编号
    /// </summary>
    public string? IdNumber { get; private set; }

    /// <summary>
    /// 开户银行名称（可选），公司对公账户的银行名称
    /// </summary>
    public string? BankName { get; private set; }

    /// <summary>
    /// 银行账号（可选），公司对公账户的账号
    /// </summary>
    public string? BankAccount { get; private set; }

    /// <summary>
    /// 银行账户名称（可选），对公账户的开户名称
    /// </summary>
    public string? BankAccountName { get; private set; }

    /// <summary>
    /// 结算周期（可选），如"Monthly"按月、"Quarterly"按季等
    /// </summary>
    public string? SettlementCycle { get; private set; }

    /// <summary>
    /// 结算日（可选），配合结算周期使用的具体结算日期
    /// </summary>
    public int? SettlementDay { get; private set; }

    /// <summary>
    /// 佣金比例（可选），以小数表示的佣金比例，如 0.05 表示 5%
    /// </summary>
    public decimal? CommissionRate { get; private set; }

    /// <summary>
    /// 备注（可选），用于记录公司的补充说明信息
    /// </summary>
    public string? Remark { get; private set; }

    /// <summary>
    /// 是否启用，true=启用（正常使用），false=停用（不可操作）
    /// 默认值为 true
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private Company() { }

    /// <summary>
    /// 创建公司实例。公司名称是必填项，创建后默认处于启用状态
    /// </summary>
    /// <param name="name">公司名称，不能为空或空白字符</param>
    /// <exception cref="ArgumentException">当公司名称为空或仅含空白字符时抛出</exception>
    public Company(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("公司名称不能为空", nameof(name));
        Name = name;
        IsActive = true;
    }

    // ===== 属性设置方法 =====

    /// <summary>设置公司编码</summary>
    /// <param name="code">公司编码，可设为 null 表示清空</param>
    public void SetCode(string? code) => Code = code;

    /// <summary>设置联系人姓名</summary>
    /// <param name="contactPerson">联系人姓名，可设为 null 表示清空</param>
    public void SetContactPerson(string? contactPerson) => ContactPerson = contactPerson;

    /// <summary>设置联系电话</summary>
    /// <param name="phone">联系电话，可设为 null 表示清空</param>
    public void SetPhone(string? phone) => Phone = phone;

    /// <summary>设置公司地址</summary>
    /// <param name="address">公司地址，可设为 null 表示清空</param>
    public void SetAddress(string? address) => Address = address;

    /// <summary>设置证件信息（证件类型和证件号码）</summary>
    /// <param name="idType">证件类型，如"营业执照"、"身份证"等</param>
    /// <param name="idNumber">证件号码，与证件类型对应的编号</param>
    public void SetIdInfo(string? idType, string? idNumber) { IdType = idType; IdNumber = idNumber; }

    /// <summary>设置银行账户信息</summary>
    /// <param name="bankName">开户银行名称</param>
    /// <param name="bankAccount">银行账号</param>
    /// <param name="bankAccountName">银行账户名称</param>
    public void SetBankInfo(string? bankName, string? bankAccount, string? bankAccountName)
    {
        BankName = bankName;
        BankAccount = bankAccount;
        BankAccountName = bankAccountName;
    }

    /// <summary>设置结算规则</summary>
    /// <param name="cycle">结算周期，如 "Monthly" 按月、"Quarterly" 按季</param>
    /// <param name="day">结算日，配合结算周期使用的具体日期</param>
    /// <param name="rate">佣金比例，小数形式（如 0.05 表示 5%）</param>
    public void SetSettlement(string? cycle, int? day, decimal? rate)
    {
        SettlementCycle = cycle;
        SettlementDay = day;
        CommissionRate = rate;
    }

    /// <summary>设置备注信息</summary>
    /// <param name="remark">备注文本，可设为 null 表示清空</param>
    public void SetRemark(string? remark) => Remark = remark;

    /// <summary>重命名公司</summary>
    /// <param name="newName">新的公司名称，不能为空或空白字符</param>
    /// <exception cref="ArgumentException">当新名称为空或仅含空白字符时抛出</exception>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("公司名称不能为空", nameof(newName));
        Name = newName;
    }

    /// <summary>启用公司。将 IsActive 设为 true，恢复公司的正常使用状态</summary>
    public void Activate() => IsActive = true;

    /// <summary>停用公司。将 IsActive 设为 false，禁止对公司进行业务操作</summary>
    public void Deactivate() => IsActive = false;
}
