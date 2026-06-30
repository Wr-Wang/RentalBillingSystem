namespace RBS.Core.Entities.Billing;
using RBS.Core.Common;

using RBS.Core.Entities.Base;

/// <summary>
/// 应收计划聚合根 — 每个合同+费用+账期产生一条应收记录
/// 核心不变约束：同一合同同一账期同一费用类型只能有一条应收
/// </summary>
public class ReceivablePlan : AggregateRoot
{
    public Guid ContractId { get; private set; }
    public Guid FeeCodeId { get; private set; }
    public string Period { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Received { get; private set; }
    public decimal Balance => Amount - Received;
    public DateOnly DueDate { get; private set; }
    public string Status { get; private set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    private ReceivablePlan() : base()
    {
        Period = string.Empty;
        Status = "Pending";
    }

    /// <summary>领域构造函数</summary>
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

    /// <summary>记录一笔收款（部分或全部）</summary>
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

    /// <summary>取消收款记录（冲销）</summary>
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

    /// <summary>标记为逾期</summary>
    public void MarkAsOverdue()
    {
        if (Status != "Pending" && Status != "Partial")
            throw new InvalidOperationException($"状态为 {Status} 的应收不能标记为逾期");

        var daysOverdue = DateOnly.FromDateTime(ChinaTime.Now).DayNumber - DueDate.DayNumber;

        Status = "Overdue";
        AddDomainEvent(new ReceivableOverdueEvent(Id, ContractId, Period, Balance, Math.Max(0, daysOverdue)));
    }

    /// <summary>取消应收计划</summary>
    public void Cancel(string reason)
    {
        if (Status == "Paid") throw new InvalidOperationException("已结清的应收不能取消");
        if (Status == "Cancelled") return;
        Status = "Cancelled";
    }

    /// <summary>判断是否逾期</summary>
    public bool IsOverdue => Status is "Pending" or "Partial"
        && DueDate < DateOnly.FromDateTime(ChinaTime.Now);

    /// <summary>获取逾期天数</summary>
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
