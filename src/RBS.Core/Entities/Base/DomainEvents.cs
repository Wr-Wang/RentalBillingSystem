namespace RBS.Core.Entities.Base;

// ===== 合同领域事件 =====

/// <summary>合同已签订（生效）</summary>
public sealed record ContractActivatedEvent : IDomainEvent
{
    public Guid ContractId { get; }
    public Guid RoomId { get; }
    public Guid LandlordId { get; }
    public DateTime OccurredAt { get; }

    public ContractActivatedEvent(Guid contractId, Guid roomId, Guid landlordId)
    {
        ContractId = contractId;
        RoomId = roomId;
        LandlordId = landlordId;
        OccurredAt = DateTime.Now;
    }
}

/// <summary>合同已终止</summary>
public sealed record ContractTerminatedEvent : IDomainEvent
{
    public Guid ContractId { get; }
    public Guid RoomId { get; }
    public string Reason { get; }
    public DateTime OccurredAt { get; }

    public ContractTerminatedEvent(Guid contractId, Guid roomId, string reason)
    {
        ContractId = contractId;
        RoomId = roomId;
        Reason = reason;
        OccurredAt = DateTime.Now;
    }
}

/// <summary>合同已暂停</summary>
public sealed record ContractSuspendedEvent : IDomainEvent
{
    public Guid ContractId { get; }
    public DateTime OccurredAt { get; }

    public ContractSuspendedEvent(Guid contractId)
    {
        ContractId = contractId;
        OccurredAt = DateTime.Now;
    }
}

// ===== 收款领域事件 =====

/// <summary>收款已确认</summary>
public sealed record PaymentConfirmedEvent : IDomainEvent
{
    public Guid ReceiptId { get; }
    public Guid ContractId { get; }
    public decimal Amount { get; }
    public DateTime OccurredAt { get; }

    public PaymentConfirmedEvent(Guid receiptId, Guid contractId, decimal amount)
    {
        ReceiptId = receiptId;
        ContractId = contractId;
        Amount = amount;
        OccurredAt = DateTime.Now;
    }
}

/// <summary>收款已驳回</summary>
public sealed record PaymentRejectedEvent : IDomainEvent
{
    public Guid ReceiptId { get; }
    public string Reason { get; }
    public DateTime OccurredAt { get; }

    public PaymentRejectedEvent(Guid receiptId, string reason)
    {
        ReceiptId = receiptId;
        Reason = reason;
        OccurredAt = DateTime.Now;
    }
}

// ===== 应收领域事件 =====

/// <summary>应收计划已逾期</summary>
public sealed record ReceivableOverdueEvent : IDomainEvent
{
    public Guid PlanId { get; }
    public Guid ContractId { get; }
    public string Period { get; }
    public decimal Amount { get; }
    public int DaysOverdue { get; }
    public DateTime OccurredAt { get; }

    public ReceivableOverdueEvent(Guid planId, Guid contractId, string period, decimal amount, int daysOverdue)
    {
        PlanId = planId;
        ContractId = contractId;
        Period = period;
        Amount = amount;
        DaysOverdue = daysOverdue;
        OccurredAt = DateTime.Now;
    }
}

/// <summary>应收计划已结清</summary>
public sealed record ReceivableSettledEvent : IDomainEvent
{
    public Guid PlanId { get; }
    public Guid ContractId { get; }
    public DateTime OccurredAt { get; }

    public ReceivableSettledEvent(Guid planId, Guid contractId)
    {
        PlanId = planId;
        ContractId = contractId;
        OccurredAt = DateTime.Now;
    }
}

// ===== 审批领域事件 =====

/// <summary>审批已通过（全部级别完成）</summary>
public sealed record ApprovalCompletedEvent : IDomainEvent
{
    public Guid ApprovalRequestId { get; }
    public Guid TargetEntityId { get; }
    public string TargetEntityType { get; }
    public string Action { get; }
    public DateTime OccurredAt { get; }

    public ApprovalCompletedEvent(Guid approvalRequestId, Guid targetEntityId, string targetEntityType, string action)
    {
        ApprovalRequestId = approvalRequestId;
        TargetEntityId = targetEntityId;
        TargetEntityType = targetEntityType;
        Action = action;
        OccurredAt = DateTime.Now;
    }
}

// ===== 抄表领域事件 =====

/// <summary>抄表已确认</summary>
public sealed record MeterReadingConfirmedEvent : IDomainEvent
{
    public Guid ReadingId { get; }
    public Guid ContractFeeConfigId { get; }
    public int Year { get; }
    public int Month { get; }
    public decimal Usage { get; }
    public DateTime OccurredAt { get; }

    public MeterReadingConfirmedEvent(Guid readingId, Guid contractFeeConfigId, int year, int month, decimal usage)
    {
        ReadingId = readingId;
        ContractFeeConfigId = contractFeeConfigId;
        Year = year;
        Month = month;
        Usage = usage;
        OccurredAt = DateTime.Now;
    }
}
