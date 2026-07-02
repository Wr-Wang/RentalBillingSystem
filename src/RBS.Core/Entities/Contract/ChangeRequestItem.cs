namespace RBS.Core.Entities.Contract;
using RBS.Core.Entities.Base;

/// <summary>
/// 合同变更明细实体 — 每个字段变更一行
/// </summary>
public class ChangeRequestItem : AssociationEntity
{
    public Guid ChangeRequestId { get; private set; }
    public string TargetType { get; private set; } = string.Empty;
    public Guid? TargetId { get; private set; }
    public string FieldName { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string NewValue { get; private set; } = string.Empty;
    public decimal? OldValueDecimal { get; private set; }
    public decimal? NewValueDecimal { get; private set; }

    private ChangeRequestItem() { }

    public ChangeRequestItem(Guid changeRequestId, string targetType, Guid? targetId, string fieldName,
        string? oldValue, string newValue, decimal? oldValueDec, decimal? newValueDec)
    {
        ChangeRequestId = changeRequestId;
        TargetType = targetType;
        TargetId = targetId;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
        OldValueDecimal = oldValueDec;
        NewValueDecimal = newValueDec;
    }
}
