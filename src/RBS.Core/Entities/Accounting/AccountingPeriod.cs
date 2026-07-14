using RBS.Core.Entities.Base;

namespace RBS.Core.Entities.Accounting;

/// <summary>
/// 会计期间 — 记录某公司某月份的账期状态
/// 状态流转: Open（开启） → Closed（已结账） → Locked（已锁定，不可逆）
/// 会计期间是财务月结的基础控制单元，确保各月账务数据隔离和结账顺序正确
/// </summary>
public class AccountingPeriod : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 所属公司标识
    /// 每个公司独立管理自己的会计期间，互不干扰
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 会计期间字符串，格式为 yyyy-MM（如 "2026-07"）
    /// 表示该期间覆盖的年份和月份，全局唯一约束（按公司+期间）
    /// </summary>
    public string Period { get; private set; } = string.Empty;

    /// <summary>
    /// 期间状态
    /// Open（开启）— 可录入凭证；Closed（已结账）— 月结完成，凭证锁定；
    /// Locked（已锁定）— 最终锁定，不可逆
    /// </summary>
    public string Status { get; private set; } = "Open";

    /// <summary>
    /// 开启时间（UTC）
    /// 记录该会计期间首次被打开的时刻，通常由系统自动触发
    /// </summary>
    public DateTime OpenedAt { get; private set; }

    /// <summary>
    /// 开启操作人标识
    /// 记录谁开启了该会计期间（通常是系统自动操作或财务管理员）
    /// </summary>
    public Guid OpenedBy { get; private set; }

    /// <summary>
    /// 结账时间（UTC）
    /// 记录执行结账操作的时刻，仅在 Status 为 Closed 时有值
    /// </summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>
    /// 结账操作人标识
    /// 记录谁执行了结账操作，仅在 Status 为 Closed 时有值
    /// </summary>
    public Guid? ClosedBy { get; private set; }

    /// <summary>
    /// 仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private AccountingPeriod() { }

    /// <summary>
    /// 创建会计期间实例，初始状态为 Open（开启）
    /// </summary>
    /// <param name="companyId">所属公司标识</param>
    /// <param name="period">会计期间字符串（yyyy-MM 格式）</param>
    /// <param name="openedBy">开启操作人标识</param>
    public AccountingPeriod(Guid companyId, string period, Guid openedBy)
    {
        CompanyId = companyId;
        Period = period;
        Status = "Open";
        OpenedAt = DateTime.UtcNow;
        OpenedBy = openedBy;
    }

    /// <summary>
    /// 结账：将开启状态转为已结账
    /// 结账后该期间不可再新增或修改凭证，确保月结数据的稳定性
    /// </summary>
    /// <param name="closedBy">结账操作人标识</param>
    /// <exception cref="InvalidOperationException">当期间状态不是 Open 时抛出</exception>
    public void Close(Guid closedBy)
    {
        if (Status != "Open")
            throw new InvalidOperationException($"会计期间 {Period} 当前状态为「{Status}」，仅开启状态可结账");
        Status = "Closed";
        ClosedAt = DateTime.UtcNow;
        ClosedBy = closedBy;
    }

    /// <summary>
    /// 反结账：将已结账状态重新开启
    /// 用于发现月结错误后需要回退修改的场景，反结账后可以继续录入凭证
    /// </summary>
    /// <exception cref="InvalidOperationException">当期间状态不是 Closed 时抛出</exception>
    public void Reopen()
    {
        if (Status != "Closed")
            throw new InvalidOperationException($"会计期间 {Period} 当前状态为「{Status}」，仅已结账状态可反结账");
        Status = "Open";
        ClosedAt = null;
        ClosedBy = null;
    }

    /// <summary>
    /// 锁定：结账后进一步锁定，不可反结账
    /// 最终锁定状态为不可逆操作，通常在审计结束后执行，确保历史账务数据不可篡改
    /// </summary>
    /// <exception cref="InvalidOperationException">当期间状态不是 Closed 时抛出</exception>
    public void Lock()
    {
        if (Status != "Closed")
            throw new InvalidOperationException($"会计期间 {Period} 当前状态为「{Status}」，仅已结账状态可锁定");
        Status = "Locked";
    }
}
