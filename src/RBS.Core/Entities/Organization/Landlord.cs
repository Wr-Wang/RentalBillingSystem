namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

/// <summary>
/// 房东/租户 — 房产出租方聚合根
/// </summary>
public class Landlord : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? IdType { get; private set; }
    public string? IdNumber { get; private set; }
    public string? BankName { get; private set; }
    public string? BankAccount { get; private set; }
    public string? BankAccountName { get; private set; }
    public string? SettlementCycle { get; private set; }
    public int? SettlementDay { get; private set; }
    public decimal? CommissionRate { get; private set; }
    public string? Remark { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Landlord() { }

    /// <summary>领域构造函数</summary>
    public Landlord(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("房东名称不能为空", nameof(name));
        Name = name;
        IsActive = true;
    }

    // ===== 属性设置 =====
    public void SetCode(string? code) => Code = code;
    public void SetContactPerson(string? contactPerson) => ContactPerson = contactPerson;
    public void SetPhone(string? phone) => Phone = phone;
    public void SetAddress(string? address) => Address = address;
    public void SetIdInfo(string? idType, string? idNumber) { IdType = idType; IdNumber = idNumber; }
    public void SetBankInfo(string? bankName, string? bankAccount, string? bankAccountName)
    {
        BankName = bankName;
        BankAccount = bankAccount;
        BankAccountName = bankAccountName;
    }
    public void SetSettlement(string? cycle, int? day, decimal? rate)
    {
        SettlementCycle = cycle;
        SettlementDay = day;
        CommissionRate = rate;
    }
    public void SetRemark(string? remark) => Remark = remark;
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("房东名称不能为空", nameof(newName));
        Name = newName;
    }
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
