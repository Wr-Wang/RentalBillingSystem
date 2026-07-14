namespace RBS.Core.Entities.Billing;
using RBS.Core.Common;

using RBS.Core.Entities.Base;

/// <summary>
/// 应收计划聚合根 — 每个合同+费用+账期产生一条应收记录
/// 核心不变约束：同一合同同一账期同一费用类型只能有一条应收
/// </summary>
public class ReceivablePlan : AggregateRoot
{
    /// <summary>合同标识，关联到对应的租赁合同</summary>
    public Guid ContractId { get; private set; }
    /// <summary>费用代码标识，指明该应收的费用类型（如租金、物业费、押金等）</summary>
    public Guid FeeCodeId { get; private set; }
    /// <summary>账期，格式如 "2026-07"，表示该笔应收所属的会计期间</summary>
    public string Period { get; private set; }
    /// <summary>应收金额（含税），创建时确定，不可变更</summary>
    public decimal Amount { get; private set; }
    /// <summary>已收金额，通过 RecordPayment / ReversePayment 调整，始终 ≤ Amount</summary>
    public decimal Received { get; private set; }
    /// <summary>违约金金额，通过 SetLateFee 设置</summary>
    public decimal LateFee { get; private set; }
    /// <summary>未收余额 = Amount - Received，计算属性</summary>
    public decimal Balance => Amount - Received;
    /// <summary>到期日，用于判断是否逾期及违约金计算</summary>
    public DateOnly DueDate { get; private set; }
    /// <summary>
    /// 应收状态：Pending（待收）| Partial（部分收款）| Paid（已结清）| Overdue（逾期）| Frozen（冻结）| Cancelled（已取消）
    /// </summary>
    public string Status { get; private set; }
    /// <summary>乐观并发控制版本戳，用于 EF Core 并发冲突检测</summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    /// <summary>是否已纳入账单</summary>
    public bool IsBilled { get; private set; }
    /// <summary>关联的账单ID（出账后回写）</summary>
    public Guid? DebitNoteId { get; private set; }
    /// <summary>出账时间</summary>
    public DateTime? BilledAt { get; private set; }
    /// <summary>条目类型：Normal / Deposit / Supplementary</summary>
    public string EntryType { get; private set; } = "Normal";

    private ReceivablePlan() : base()
    {
        Period = string.Empty;
        Status = "Pending";
    }

    /// <summary>领域构造函数</summary>
    /// <param name="contractId">合同标识</param>
    /// <param name="feeCodeId">费用代码标识</param>
    /// <param name="period">账期，格式如 "2026-07"</param>
    /// <param name="amount">应收金额，必须大于 0</param>
    /// <param name="dueDate">到期日</param>
    /// <exception cref="ArgumentException">金额小于等于 0、账期为空或到期日为默认值时抛出</exception>
    public ReceivablePlan(Guid contractId, Guid feeCodeId, string period, decimal amount, DateOnly dueDate) : base()
    {
        if (amount <= 0) throw new ArgumentException("应收金额必须大于0", nameof(amount));
        if (string.IsNullOrWhiteSpace(period)) throw new ArgumentException("账期不能为空", nameof(period));
        if (dueDate == default) throw new ArgumentException("到期日不能为空", nameof(dueDate));

        ContractId = contractId;
        FeeCodeId = feeCodeId;
        Period = period;
        Amount = amount;
        Received = 0;
        DueDate = dueDate;
        Status = "Pending";
    }

    // ===== 领域行为 =====

    /// <summary>记录一笔收款（部分或全部），更新已收金额及状态</summary>
    /// <param name="amount">本次收款金额，必须大于 0</param>
    /// <exception cref="ArgumentException">amount 小于等于 0 时抛出</exception>
    /// <exception cref="InvalidOperationException">已结清或已取消的应收无法收款；累计收款超过应收金额时抛出</exception>
    /// <remarks>满额收款时状态变更为 Paid 并触发 <see cref="ReceivableSettledEvent"/> 领域事件</remarks>
    public void RecordPayment(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("收款金额必须大于0");
        if (Status == "Paid" || Status == "Cancelled")
            throw new InvalidOperationException($"已{GetStatusDisplayName()}的应收无法收款");

        var newReceived = Received + amount;
        if (newReceived > Amount)
            throw new InvalidOperationException($"收款金额 {newReceived} 超过应收金额 {Amount}");

        Received = newReceived;

        // 更新状态
        if (Received >= Amount)
        {
            Status = "Paid";
            AddDomainEvent(new ReceivableSettledEvent(Id, ContractId));
        }
        else
        {
            Status = "Partial";
        }
    }

    /// <summary>取消收款记录（冲销），减少已收金额并回退状态</summary>
    /// <param name="amount">冲销金额，必须大于 0</param>
    /// <exception cref="ArgumentException">amount 小于等于 0 时抛出</exception>
    /// <exception cref="InvalidOperationException">已取消的应收无法操作；冲销金额超过已收金额时抛出</exception>
    /// <remarks>冲销后若已收金额归零则状态回退为 Pending，否则为 Partial</remarks>
    public void ReversePayment(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("冲销金额必须大于0");
        if (Status == "Cancelled")
            throw new InvalidOperationException("已取消的应收无法操作");

        var newReceived = Received - amount;
        if (newReceived < 0)
            throw new InvalidOperationException("冲销金额超过已收金额");

        Received = newReceived;

        Status = Received <= 0 ? "Pending" : "Partial";
    }

    /// <summary>设置违约金金额</summary>
    /// <param name="fee">违约金金额</param>
    public void SetLateFee(decimal fee) { LateFee = fee; }

    /// <summary>将应收标记为逾期，触发 <see cref="ReceivableOverdueEvent"/> 领域事件</summary>
    /// <exception cref="InvalidOperationException">状态不是 Pending 或 Partial 时无法标记逾期</exception>
    public void MarkAsOverdue()
    {
        if (Status != "Pending" && Status != "Partial")
            throw new InvalidOperationException($"状态为 {Status} 的应收不能标记为逾期");

        var daysOverdue = DateOnly.FromDateTime(ChinaTime.Now).DayNumber - DueDate.DayNumber;

        Status = "Overdue";
        AddDomainEvent(new ReceivableOverdueEvent(Id, ContractId, Period, Balance, Math.Max(0, daysOverdue)));
    }

    /// <summary>取消应收计划</summary>
    /// <param name="reason">取消原因</param>
    /// <exception cref="InvalidOperationException">已结清的应收不能取消</exception>
    public void Cancel(string reason)
    {
        if (Status == "Paid") throw new InvalidOperationException("已结清的应收不能取消");
        if (Status == "Cancelled") return;
        Status = "Cancelled";
    }

    /// <summary>标记为已出账，回填账单标识和出账时间</summary>
    /// <param name="debitNoteId">关联的账单 ID</param>
    public void MarkAsBilled(Guid debitNoteId)
    {
        IsBilled = true;
        DebitNoteId = debitNoteId;
        BilledAt = ChinaTime.Now;
    }

    /// <summary>冻结应收（暂停时调用），将状态置为 Frozen</summary>
    /// <exception cref="InvalidOperationException">状态不是 Pending 时无法冻结</exception>
    public void Freeze()
    {
        if (Status != "Pending")
            throw new InvalidOperationException($"状态为 {Status} 的应收不能冻结");
        Status = "Frozen";
    }

    /// <summary>解冻应收（恢复时调用），将状态从 Frozen 恢复为 Pending</summary>
    /// <exception cref="InvalidOperationException">状态不是 Frozen 时无法解冻</exception>
    public void Unfreeze()
    {
        if (Status != "Frozen")
            throw new InvalidOperationException($"状态为 {Status} 的应收不能解冻");
        Status = "Pending";
    }

    /// <summary>判断是否逾期：状态为 Pending 或 Partial 且到期日早于当前日期时返回 true</summary>
    public bool IsOverdue => Status is "Pending" or "Partial"
        && DueDate < DateOnly.FromDateTime(ChinaTime.Now);

    /// <summary>获取逾期天数，未逾期返回 0</summary>
    public int DaysOverdue
    {
        get
        {
            if (!IsOverdue) return 0;
            return DateOnly.FromDateTime(ChinaTime.Now).DayNumber - DueDate.DayNumber;
        }
    }

    private string GetStatusDisplayName() => Status switch
    {
        "Paid" => "结清",
        "Cancelled" => "取消",
        _ => Status
    };
}
