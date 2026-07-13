using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Accounting;

/// <summary>
/// 会计期间 — 记录某公司某月份的账期状态
/// 状态流转: Open（开启） → Closed（已结账） → Locked（已锁定，不可逆）
/// </summary>
public class AccountingPeriod : AuditableEntity, IHasCompany
{
    public Guid CompanyId { get; private set; }
    public string Period { get; private set; } = string.Empty;   // yyyy-MM
    public string Status { get; private set; } = "Open";          // Open / Closed / Locked
    public DateTime OpenedAt { get; private set; }
    public Guid OpenedBy { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Guid? ClosedBy { get; private set; }

    private AccountingPeriod() { }

    public AccountingPeriod(Guid companyId, string period, Guid openedBy)
    {
        CompanyId = companyId;
        Period = period;
        Status = "Open";
        OpenedAt = DateTime.UtcNow;
        OpenedBy = openedBy;
    }

    /// <summary>结账：将开启状态转为已结账</summary>
    public void Close(Guid closedBy)
    {
        if (Status != "Open")
            throw new InvalidOperationException($"会计期间 {Period} 当前状态为「{Status}」，仅开启状态可结账");
        Status = "Closed";
        ClosedAt = DateTime.UtcNow;
        ClosedBy = closedBy;
    }

    /// <summary>反结账：将已结账状态重新开启</summary>
    public void Reopen()
    {
        if (Status != "Closed")
            throw new InvalidOperationException($"会计期间 {Period} 当前状态为「{Status}」，仅已结账状态可反结账");
        Status = "Open";
        ClosedAt = null;
        ClosedBy = null;
    }

    /// <summary>锁定：结账后进一步锁定，不可反结账</summary>
    public void Lock()
    {
        if (Status != "Closed")
            throw new InvalidOperationException($"会计期间 {Period} 当前状态为「{Status}」，仅已结账状态可锁定");
        Status = "Locked";
    }
}
